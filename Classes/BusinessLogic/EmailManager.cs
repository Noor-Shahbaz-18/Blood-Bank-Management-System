using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class EmailManager
    {
        // Email Settings
        private static string smtpServer = "smtp.gmail.com";
        private static int smtpPort = 587;
        private static string senderEmail = "";
        private static string senderPassword = "";
        private static bool isConfigured = false;

        // Load email settings from database
        public static void LoadEmailSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LOADING EMAIL SETTINGS ===");

                string server = SettingsDAL.GetSetting("SmtpServer");
                System.Diagnostics.Debug.WriteLine($"SmtpServer from DB: '{server}'");
                if (!string.IsNullOrEmpty(server))
                    smtpServer = server;

                string portStr = SettingsDAL.GetSetting("SmtpPort");
                System.Diagnostics.Debug.WriteLine($"SmtpPort from DB: '{portStr}'");
                if (!string.IsNullOrEmpty(portStr))
                    int.TryParse(portStr, out smtpPort);

                string email = SettingsDAL.GetSetting("SenderEmail");
                System.Diagnostics.Debug.WriteLine($"SenderEmail from DB: '{email}'");
                if (!string.IsNullOrEmpty(email))
                    senderEmail = email;

                string password = SettingsDAL.GetSetting("SenderPassword");
                System.Diagnostics.Debug.WriteLine($"SenderPassword from DB: {(string.IsNullOrEmpty(password) ? "EMPTY" : "RECEIVED (length: " + password.Length + ")")}");
                if (!string.IsNullOrEmpty(password))
                    senderPassword = password;

                isConfigured = !string.IsNullOrEmpty(senderEmail) &&
                              !string.IsNullOrEmpty(senderPassword) &&
                              !string.IsNullOrEmpty(smtpServer);

                System.Diagnostics.Debug.WriteLine($"isConfigured: {isConfigured}");
                System.Diagnostics.Debug.WriteLine($"Final Settings - Server: {smtpServer}, Port: {smtpPort}, Email: {senderEmail}");
                System.Diagnostics.Debug.WriteLine("=== LOADING COMPLETE ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadEmailSettings Error: {ex.Message}");
                isConfigured = false;
            }
        }

        // Save email settings to database
        public static bool SaveEmailSettings(string server, int port, string email, string password, int userId)
        {
            try
            {
                SettingsDAL.UpdateSetting("SmtpServer", server, userId);
                SettingsDAL.UpdateSetting("SmtpPort", port.ToString(), userId);
                SettingsDAL.UpdateSetting("SenderEmail", email, userId);
                SettingsDAL.UpdateSetting("SenderPassword", password, userId);

                LoadEmailSettings(); // Reload settings
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveEmailSettings Error: {ex.Message}");
                return false;
            }
        }

        // Test email configuration
        public static async Task<bool> TestEmailConfigurationAsync(string server, int port, string email, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TESTING EMAIL CONFIGURATION ===");
                using (SmtpClient client = new SmtpClient(server, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(email, password);
                    client.Timeout = 10000;

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(email);
                    message.To.Add(email);
                    message.Subject = "Blood Bank Management System - Test Email";
                    message.Body = "✅ Congratulations! Your email configuration is working correctly.\n\n" +
                                   "This is a test email from Blood Bank Management System.\n\n" +
                                   $"Server: {server}:{port}\n" +
                                   $"Email: {email}\n\n" +
                                   "You can now send real notifications to users.\n\n" +
                                   "Regards,\nBlood Bank Team";
                    message.IsBodyHtml = false;

                    await client.SendMailAsync(message);
                    System.Diagnostics.Debug.WriteLine("Test email sent successfully!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestEmailConfigurationAsync Error: {ex.Message}");
                return false;
            }
        }

        // Send email (async)
        public static async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            System.Diagnostics.Debug.WriteLine($"=== SENDING EMAIL TO: {toEmail} ===");

            if (!isConfigured)
            {
                LoadEmailSettings(); // Try to reload settings
                if (!isConfigured)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Email not configured. Cannot send email.");
                    return false;
                }
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Using SMTP Server: {smtpServer}:{smtpPort}");
                System.Diagnostics.Debug.WriteLine($"From Email: {senderEmail}");

                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.Timeout = 15000;

                    MailMessage message = new MailMessage(senderEmail, toEmail, subject, body);
                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                    System.Diagnostics.Debug.WriteLine($"✅ Email sent successfully to: {toEmail}");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Email Error: {ex.Message}");
                return false;
            }
        }

        // Sync version (for compatibility)
        public static bool SendEmail(string toEmail, string subject, string body)
        {
            return SendEmailAsync(toEmail, subject, body).GetAwaiter().GetResult();
        }

        // Send donation confirmation email
        public static async Task<bool> SendDonationConfirmationAsync(string donorEmail, string donorName, string donationDate, string bloodGroup)
        {
            string subject = "Thank You for Your Blood Donation! 🩸";
            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }}
                        .header {{ background-color: #78161b; color: white; padding: 15px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f0f0f0; padding: 10px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px; }}
                        .button {{ background-color: #78161b; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                        .highlight {{ color: #78161b; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🩸 Blood Bank Management System</h2>
                        </div>
                        <div class='content'>
                            <h2 style='color: #78161b;'>Thank You, {donorName}! 👏</h2>
                            <p>Your blood donation on <strong>{donationDate}</strong> has been successfully recorded.</p>
                            <p><strong>Blood Group:</strong> <span class='highlight'>{bloodGroup}</span></p>
                            <p>You have saved lives! We appreciate your generosity and humanity.</p>
                            <br/>
                            <p>📅 Your next eligible donation date is: <strong>{DateTime.Now.AddDays(90):dd-MMM-yyyy}</strong></p>
                            <hr/>
                            <p style='font-size: 14px;'>Every drop counts. Thank you for being a hero! 🦸‍♂️🦸‍♀️</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Blood Bank Management System | Saving Lives Together</p>
                            <p style='font-size: 11px;'>This is an automated message, please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(donorEmail, subject, body);
        }

        // Send approval notification email
        public static async Task<bool> SendApprovalNotificationAsync(string patientEmail, string patientName, string requestID, string bloodGroup, string hospital)
        {
            string subject = "✅ Blood Request Approved - Blood Bank";
            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; }}
                        .header {{ background-color: #78161b; color: white; padding: 15px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f0f0f0; padding: 10px; text-align: center; font-size: 12px; border-radius: 0 0 8px 8px; }}
                        .approved {{ color: green; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🩸 Blood Bank Management System</h2>
                        </div>
                        <div class='content'>
                            <h2 style='color: #78161b;'>Request Approved! ✅</h2>
                            <p>Dear {patientName},</p>
                            <p>Your blood request <strong>#{requestID}</strong> for <strong>{bloodGroup}</strong> blood has been <span class='approved'>APPROVED</span>.</p>
                            <p><strong>Hospital:</strong> {hospital}</p>
                            <p>Your blood unit is ready for pickup or will be delivered soon.</p>
                            <p>Please contact the blood bank for further details.</p>
                            <br/>
                            <p>Thank you for using our service.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Blood Bank Management System</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendEmailAsync(patientEmail, subject, body);
        }

        // Send emergency alert email to donors
        public static async Task<int> SendEmergencyAlertToDonorsAsync(string bloodGroup, string hospital, string message, int unitsNeeded)
        {
            int sentCount = 0;
            try
            {
                DataTable donors = DonorDAL.GetDonorsByBloodGroup(bloodGroup);

                foreach (DataRow row in donors.Rows)
                {
                    string email = row["Email"]?.ToString();
                    string donorName = row["FullName"]?.ToString();

                    if (!string.IsNullOrEmpty(email))
                    {
                        string subject = $"🚨 URGENT: {bloodGroup} Blood Needed - Emergency";
                        string body = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <style>
                                    body {{ font-family: Arial, sans-serif; }}
                                    .emergency {{ background-color: #dc2626; color: white; padding: 10px; text-align: center; }}
                                </style>
                            </head>
                            <body>
                                <div class='emergency'>
                                    <h2>🚨 EMERGENCY BLOOD REQUIREMENT 🚨</h2>
                                </div>
                                <div style='padding: 20px;'>
                                    <h3>Dear {donorName},</h3>
                                    <p><strong>{unitsNeeded} unit(s) of {bloodGroup}</strong> blood needed immediately at:</p>
                                    <p><strong>🏥 Hospital:</strong> {hospital}</p>
                                    <p><strong>📝 Message:</strong> {message}</p>
                                    <p>Please visit the hospital if you are eligible to donate.</p>
                                    <p>Your donation can save a life!</p>
                                    <hr/>
                                    <p style='font-size: 12px; color: gray;'>This is an automated emergency alert.</p>
                                </div>
                            </body>
                            </html>";

                        if (await SendEmailAsync(email, subject, body))
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

        // Send broadcast email to multiple users
        public static async Task<int> SendBulkEmailAsync(DataTable recipients, string title, string message, string priority)
        {
            int sentCount = 0;
            string subject = $"[{priority}] {title} - Blood Bank";

            string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: auto; padding: 20px; }}
                        .header {{ background-color: #78161b; color: white; padding: 15px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .priority-{priority.ToLower()} {{ background-color: #fee2e2; padding: 5px 10px; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🩸 Blood Bank Management System</h2>
                        </div>
                        <div class='content'>
                            <h3>{title}</h3>
                            <div class='priority-{priority.ToLower()}'>
                                <strong>Priority:</strong> {priority}
                            </div>
                            <p>{message}</p>
                            <br/>
                            <p>Thank you,<br/>Blood Bank Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            foreach (DataRow row in recipients.Rows)
            {
                string email = row["Email"]?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    if (await SendEmailAsync(email, subject, body))
                        sentCount++;
                }
            }
            return sentCount;
        }

        // Send reminder for eligible donors
        public static async Task<int> SendDonationReminderAsync()
        {
            int sentCount = 0;
            try
            {
                DataTable eligibleDonors = DonorDAL.GetEligibleDonors();

                foreach (DataRow row in eligibleDonors.Rows)
                {
                    string email = row["Email"]?.ToString();
                    string donorName = row["FullName"]?.ToString();
                    string bloodGroup = row["BloodGroup"]?.ToString();

                    if (!string.IsNullOrEmpty(email))
                    {
                        string subject = "🩸 You're Eligible to Donate Blood Again!";
                        string body = $@"
                            <html>
                            <body>
                                <h2 style='color: #78161b;'>Dear {donorName},</h2>
                                <p>You are now eligible to donate blood again!</p>
                                <p><strong>Your Blood Group:</strong> {bloodGroup}</p>
                                <p>Please visit your nearest blood bank at your earliest convenience.</p>
                                <p>Your donation can save up to 3 lives!</p>
                                <br/>
                                <p>Thank you for being a regular donor.</p>
                            </body>
                            </html>";

                        if (await SendEmailAsync(email, subject, body))
                            sentCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendDonationReminder Error: {ex.Message}");
            }
            return sentCount;
        }

        // Get email configuration status
        public static bool IsEmailConfigured()
        {
            LoadEmailSettings();
            return isConfigured;
        }

        // Get email settings (for display)
        public static (string server, int port, string email, bool isConfigured) GetEmailSettings()
        {
            LoadEmailSettings();
            return (smtpServer, smtpPort, senderEmail, isConfigured);
        }
    }
}