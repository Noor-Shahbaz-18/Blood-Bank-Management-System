using System;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class AuthenticationManager
    {
        public static User Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return null;

                var user = AdminDAL.GetUserByUsername(username);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"User not found: {username}");
                    return null;
                }

                if (!user.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine($"User inactive: {username}");
                    return null;
                }

                // Debug hash comparison
                string computedHash = EncryptionHelper.HashPassword(password);
                System.Diagnostics.Debug.WriteLine($"Computed Hash: {computedHash}");
                System.Diagnostics.Debug.WriteLine($"Stored Hash: {user.PasswordHash}");
                System.Diagnostics.Debug.WriteLine($"Match: {computedHash == user.PasswordHash}");

                bool isValid = EncryptionHelper.Verify(password, user.PasswordHash);

                if (!isValid)
                {
                    System.Diagnostics.Debug.WriteLine($"Password invalid for: {username}");
                    return null;
                }

                SessionManager.Login(user);
                AdminDAL.UpdateLastLogin(user.UserID);

                return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
                return null;
            }
        }

        public static bool Logout()
        {
            SessionManager.Logout();
            return true;
        }

        public static bool IsAuthenticated()
        {
            return SessionManager.IsLoggedIn;
        }

        public static User GetCurrentUser()
        {
            if (!SessionManager.IsLoggedIn)
                return null;

            return new User
            {
                UserID = SessionManager.CurrentUserID,
                FullName = SessionManager.CurrentFullName,
                Username = SessionManager.CurrentUsername,
                Role = SessionManager.CurrentRole,
                IsActive = true
            };
        }
    }
}