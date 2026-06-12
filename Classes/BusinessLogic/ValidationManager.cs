using System;
using System.Text.RegularExpressions;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class ValidationManager
    {
        public static bool IsValidCNIC(string cnic)
        {
            if (string.IsNullOrEmpty(cnic)) return false;
            // Format: 12345-1234567-1
            Regex regex = new Regex(@"^\d{5}-\d{7}-\d{1}$");
            return regex.IsMatch(cnic);
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            // Pakistani format: 03XXXXXXXXX
            Regex regex = new Regex(@"^03\d{9}$");
            return regex.IsMatch(phone);
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
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

        public static bool IsValidBloodGroup(string bloodGroup)
        {
            string[] validGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return Array.Exists(validGroups, bg => bg == bloodGroup);
        }

        public static bool IsValidAge(int age)
        {
            return age >= 18 && age <= 65;
        }

        public static bool IsValidWeight(int weightKg)
        {
            return weightKg >= 50;
        }

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (username.Length < 3 || username.Length > 20) return false;
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            return regex.IsMatch(username);
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 6) return false;
            // At least one number and one letter
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasNumber = Regex.IsMatch(password, @"\d");
            return hasLetter && hasNumber;
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            // Remove potentially harmful characters
            return Regex.Replace(input, @"[<>'""&]", "");
        }
    }
}