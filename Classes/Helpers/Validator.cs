using System;
using System.Text.RegularExpressions;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class Validator
    {
        // CNIC Validation (Pakistan)
        public static bool IsValidCNIC(string cnic)
        {
            if (string.IsNullOrWhiteSpace(cnic))
                return false;

            // Format: 12345-1234567-1
            Regex regex = new Regex(@"^\d{5}-\d{7}-\d{1}$");
            return regex.IsMatch(cnic);
        }

        // Phone Number Validation (Pakistan)
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Format: 03XXXXXXXXX (11 digits starting with 03)
            Regex regex = new Regex(@"^03\d{9}$");
            return regex.IsMatch(phone);
        }

        // Email Validation
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Blood Group Validation
        public static bool IsValidBloodGroup(string bloodGroup)
        {
            string[] validGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return Array.Exists(validGroups, bg => bg == bloodGroup);
        }

        // Age Validation
        public static bool IsValidAge(int age)
        {
            return age >= Constants.MIN_AGE_FOR_DONATION && age <= Constants.MAX_AGE_FOR_DONATION;
        }

        // Weight Validation
        public static bool IsValidWeight(int weightKg)
        {
            return weightKg >= Constants.MIN_WEIGHT_FOR_DONATION;
        }

        // Hemoglobin Validation
        public static bool IsValidHemoglobin(decimal hemoglobin)
        {
            return hemoglobin >= Constants.MIN_HEMOGLOBIN_FOR_DONATION;
        }

        // Username Validation
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 3 || username.Length > 20)
                return false;

            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            return regex.IsMatch(username);
        }

        // Password Validation
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 6)
                return false;

            // At least one letter and one number
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasNumber = Regex.IsMatch(password, @"\d");

            return hasLetter && hasNumber;
        }

        // Name Validation (Only letters and spaces)
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            Regex regex = new Regex(@"^[a-zA-Z\s]+$");
            return regex.IsMatch(name);
        }

        // Quantity Validation
        public static bool IsValidQuantity(int quantity)
        {
            return quantity > 0 && quantity <= 10;
        }

        // Date Validation (Not in future)
        public static bool IsValidDate(DateTime date)
        {
            return date <= DateTime.Today;
        }

        // Future Date Validation
        public static bool IsValidFutureDate(DateTime date)
        {
            return date >= DateTime.Today;
        }

        // CNIC Formatting (Auto-add dashes)
        public static string FormatCNIC(string cnic)
        {
            // Remove all non-digits
            string digits = Regex.Replace(cnic, @"[^\d]", "");

            if (digits.Length >= 13)
            {
                return $"{digits.Substring(0, 5)}-{digits.Substring(5, 7)}-{digits.Substring(12, 1)}";
            }
            return cnic;
        }

        // Phone Formatting
        public static string FormatPhoneNumber(string phone)
        {
            // Remove all non-digits
            string digits = Regex.Replace(phone, @"[^\d]", "");

            if (digits.Length == 11)
            {
                return $"{digits.Substring(0, 4)}-{digits.Substring(4, 7)}";
            }
            return phone;
        }

        // Sanitize Input (Remove harmful characters)
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            // Remove HTML tags and special characters
            string sanitized = Regex.Replace(input, @"<[^>]*>", "");
            sanitized = Regex.Replace(sanitized, @"[<>'""&]", "");

            return sanitized.Trim();
        }

        // Validate all donor fields
        public static (bool isValid, string errorMessage) ValidateDonor(
            string fullName, string cnic, string phone, string email, int age, int weight)
        {
            if (!IsValidName(fullName))
                return (false, "Please enter a valid name (letters only)");

            if (!IsValidCNIC(cnic))
                return (false, "Please enter a valid CNIC (Format: 12345-1234567-1)");

            if (!IsValidPhoneNumber(phone))
                return (false, "Please enter a valid phone number (03XXXXXXXXX)");

            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
                return (false, "Please enter a valid email address");

            if (!IsValidAge(age))
                return (false, $"Age must be between {Constants.MIN_AGE_FOR_DONATION} and {Constants.MAX_AGE_FOR_DONATION} years");

            if (!IsValidWeight(weight))
                return (false, $"Weight must be at least {Constants.MIN_WEIGHT_FOR_DONATION} kg");

            return (true, null);
        }

        // Validate blood requisition
        public static (bool isValid, string errorMessage) ValidateRequisition(
            string patientName, string bloodGroup, int units, string hospital)
        {
            if (!IsValidName(patientName))
                return (false, "Please enter a valid patient name");

            if (!IsValidBloodGroup(bloodGroup))
                return (false, "Please select a valid blood group");

            if (!IsValidQuantity(units))
                return (false, "Units must be between 1 and 10");

            if (string.IsNullOrWhiteSpace(hospital))
                return (false, "Please enter hospital name");

            return (true, null);
        }
    }
}