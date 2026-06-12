using System;
using System.Data;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class SMSSender
    {
        // SMS Settings
        private static string accountSid = "";
        private static string authToken = "";
        private static string fromNumber = "";
        private static bool isConfigured = false;

        // Load SMS settings from database
        public static void LoadSMSSettings()
        {
            try
            {
                accountSid = SettingsDAL.GetSetting("TwilioAccountSID");
                authToken = SettingsDAL.GetSetting("TwilioAuthToken");
                fromNumber = SettingsDAL.GetSetting("TwilioPhoneNumber");

                isConfigured = !string.IsNullOrEmpty(accountSid) &&
                              !string.IsNullOrEmpty(authToken) &&
                              !string.IsNullOrEmpty(fromNumber);

                if (isConfigured)
                {
                    TwilioClient.Init(accountSid, authToken);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSMSSettings Error: {ex.Message}");
                isConfigured = false;
            }
        }

        // Save SMS settings to database
        public static bool SaveSMSSettings(string sid, string token, string number, int userId)
        {
            try
            {
                SettingsDAL.UpdateSetting("TwilioAccountSID", sid, userId);
                SettingsDAL.UpdateSetting("TwilioAuthToken", token, userId);
                SettingsDAL.UpdateSetting("TwilioPhoneNumber", number, userId);

                LoadSMSSettings();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveSMSSettings Error: {ex.Message}");
                return false;
            }
        }

        // Test SMS configuration (Fixed - no Api error)
        public static async Task<bool> TestSMSConfigurationAsync(string sid, string token, string number)
        {
            try
            {
                TwilioClient.Init(sid, token);

                // Simple way to test - try to send a test SMS to yourself
                // This verifies credentials are working
                var message = await MessageResource.CreateAsync(
                    body: "✅ Test message from Blood Bank Management System. Your SMS configuration is working!",
                    from: new Twilio.Types.PhoneNumber(number),
                    to: new Twilio.Types.PhoneNumber(number)
                );

                return message != null && (message.Status == MessageResource.StatusEnum.Sent ||
                                           message.Status == MessageResource.StatusEnum.Accepted);
            }
            catch (ApiException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestSMSConfigurationAsync API Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestSMSConfigurationAsync Error: {ex.Message}");
                return false;
            }
        }

        // Send SMS
        public static async Task<bool> SendSMSAsync(string phoneNumber, string message)
        {
            if (!isConfigured)
            {
                LoadSMSSettings();
                if (!isConfigured)
                {
                    System.Diagnostics.Debug.WriteLine("SMS not configured. Skipping...");
                    return false;
                }
            }

            try
            {
                // Format phone number (ensure it has country code)
                string formattedNumber = phoneNumber;
                if (!formattedNumber.StartsWith("+"))
                {
                    formattedNumber = "+92" + formattedNumber.TrimStart('0');
                }

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(fromNumber),
                    to: new Twilio.Types.PhoneNumber(formattedNumber)
                );

                return messageResource.Status == MessageResource.StatusEnum.Sent ||
                       messageResource.Status == MessageResource.StatusEnum.Accepted ||
                       messageResource.Status == MessageResource.StatusEnum.Queued;
            }
            catch (ApiException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMS API Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMS Error: {ex.Message}");
                return false;
            }
        }

        // Sync version
        public static bool SendSMS(string phoneNumber, string message)
        {
            return SendSMSAsync(phoneNumber, message).GetAwaiter().GetResult();
        }

        // Send emergency alert SMS to donors
        public static async Task<int> SendEmergencyAlertToDonorsAsync(string bloodGroup, string hospital, string message, int unitsNeeded)
        {
            int sentCount = 0;
            try
            {
                DataTable donors = DonorDAL.GetDonorsByBloodGroup(bloodGroup);

                foreach (DataRow row in donors.Rows)
                {
                    string phone = row["Phone"]?.ToString();
                    string donorName = row["FullName"]?.ToString();

                    if (!string.IsNullOrEmpty(phone))
                    {
                        string smsMessage = $"🚨 URGENT: {unitsNeeded} unit(s) of {bloodGroup} blood needed at {hospital}. {message} Please contact blood bank. - Blood Bank";
                        if (await SendSMSAsync(phone, smsMessage))
                            sentCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendEmergencyAlertToDonors Error: {ex.Message}");
            }
            return sentCount;
        }

        // Send donation reminder SMS
        public static async Task<bool> SendDonationReminderAsync(string phoneNumber, string donorName, DateTime nextDate)
        {
            string message = $"Dear {donorName}, you are eligible to donate blood on {nextDate:dd-MMM-yyyy}. Please visit the blood bank. Thank you for saving lives!";
            return await SendSMSAsync(phoneNumber, message);
        }

        // Send blood request approval SMS
        public static async Task<bool> SendApprovalSMSAsync(string phoneNumber, string patientName, string requestID)
        {
            string message = $"Dear {patientName}, your blood request #{requestID} has been approved. Please contact blood bank for pickup. Thank you.";
            return await SendSMSAsync(phoneNumber, message);
        }

        // Send bulk SMS to multiple recipients
        public static async Task<int> SendBulkSMSAsync(DataTable recipients, string message)
        {
            int sentCount = 0;
            foreach (DataRow row in recipients.Rows)
            {
                string phone = row["Phone"]?.ToString();
                if (!string.IsNullOrEmpty(phone))
                {
                    if (await SendSMSAsync(phone, message))
                        sentCount++;
                }
            }
            return sentCount;
        }

        // Send test SMS to a specific number
        public static async Task<bool> SendTestSMSAsync(string phoneNumber)
        {
            string message = "✅ This is a test message from Blood Bank Management System. Your SMS configuration is working correctly!";
            return await SendSMSAsync(phoneNumber, message);
        }

        // Get SMS configuration status
        public static bool IsSMSConfigured()
        {
            LoadSMSSettings();
            return isConfigured;
        }

        // Get SMS settings (for display)
        public static (string accountSid, string fromNumber, bool isConfigured) GetSMSSettings()
        {
            LoadSMSSettings();
            return (accountSid, fromNumber, isConfigured);
        }
    }
}