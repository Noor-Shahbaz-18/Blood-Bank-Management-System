using System;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class SessionManager
    {
        public static int CurrentUserID { get; private set; }
        public static string CurrentFullName { get; private set; }
        public static string CurrentUsername { get; private set; }
        public static UserRole CurrentRole { get; private set; }
        public static bool IsLoggedIn { get; private set; }

        // 🆕 Additional properties
        public static string CurrentEmail { get; private set; }
        public static string CurrentPhone { get; private set; }
        public static DateTime LoginTime { get; private set; }

        // Login method (Existing - Keep as is)
        public static void Login(User user)
        {
            CurrentUserID = user.UserID;
            CurrentFullName = user.FullName;
            CurrentUsername = user.Username;
            CurrentRole = user.Role;
            CurrentEmail = user.Email;
            CurrentPhone = user.Phone;
            IsLoggedIn = true;
            LoginTime = DateTime.Now;

            // Update last login in database
            AdminDAL.UpdateLastLogin(user.UserID);

            // Log the login
            Logger.LogLogin(user.Username, true, "");
        }

        // Logout method (Existing - Keep as is)
        public static void Logout()
        {
            if (IsLoggedIn)
            {
                Logger.LogAction(CurrentUsername, "Logout", "User logged out");
            }

            CurrentUserID = 0;
            CurrentFullName = null;
            CurrentUsername = null;
            CurrentRole = 0;
            CurrentEmail = null;
            CurrentPhone = null;
            IsLoggedIn = false;
        }

        // 🆕 Check if user has specific role
        public static bool HasRole(UserRole role)
        {
            return IsLoggedIn && CurrentRole == role;
        }

        // 🆕 Check if user is Admin
        public static bool IsAdmin => HasRole(UserRole.Admin);

        // 🆕 Check if user is Manager
        public static bool IsManager => HasRole(UserRole.Manager);

        // 🆕 Check if user is Doctor
        public static bool IsDoctor => HasRole(UserRole.Doctor);

        // 🆕 Check if user is Donor
        public static bool IsDonor => HasRole(UserRole.Donor);

        // 🆕 Check if user is Patient
        public static bool IsPatient => HasRole(UserRole.Patient);

        // 🆕 Check if user is Technician
        public static bool IsTechnician => HasRole(UserRole.Technician);

        // 🆕 Get session duration
        public static TimeSpan GetSessionDuration()
        {
            if (!IsLoggedIn) return TimeSpan.Zero;
            return DateTime.Now - LoginTime;
        }

        // 🆕 Get session info as string
        public static string GetSessionInfo()
        {
            if (!IsLoggedIn) return "No active session";

            return $"User: {CurrentFullName} ({CurrentUsername})\n" +
                   $"Role: {CurrentRole}\n" +
                   $"Logged in since: {LoginTime:dd-MMM-yyyy hh:mm:ss tt}\n" +
                   $"Duration: {GetSessionDuration():hh\\:mm\\:ss}";
        }
    }
}