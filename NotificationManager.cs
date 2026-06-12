using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;
using System;
using System.Data;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class NotificationManager
    {
        // Send notification to single user
        public static bool SendNotification(int userID, string title, string message, string type = "General", string priority = "Normal")
        {
            Notification notif = new Notification
            {
                UserID = userID,
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                IsRead = false,
                CreatedAt = DateTime.Now,
                SentBy = SessionManager.CurrentUsername
            };
            return NotificationDAL.Insert(notif);
        }

        // Send notification with action URL (click to navigate)
        public static bool SendNotificationWithAction(int userID, string title, string message, string actionUrl, int relatedID, string relatedType)
        {
            Notification notif = new Notification
            {
                UserID = userID,
                Title = title,
                Message = message,
                Type = "Action",
                Priority = "Normal",
                IsRead = false,
                CreatedAt = DateTime.Now,
                ActionUrl = actionUrl,
                RelatedID = relatedID,
                RelatedType = relatedType,
                SentBy = SessionManager.CurrentUsername
            };
            return NotificationDAL.Insert(notif);
        }

        // Broadcast to all users with specific role
        public static int BroadcastToRole(string role, string title, string message, string priority = "Normal")
        {
            return NotificationDAL.BroadcastToRole(role, title, message, priority);
        }

        // Broadcast to all active users
        public static int BroadcastToAllUsers(string title, string message, string priority = "Normal")
        {
            DataTable users = AdminDAL.GetAllUsers();
            if (users == null || users.Rows.Count == 0)
                return 0;
            return NotificationDAL.InsertBulk(users, title, message, "Broadcast", priority, SessionManager.CurrentUsername);
        }

        // Send emergency blood request to donors
        public static int SendEmergencyBloodRequest(string bloodGroup, int unitsNeeded, string hospital, string message = "")
        {
            return NotificationDAL.SendBloodRequestAlert(bloodGroup, unitsNeeded, hospital, message);
        }

        // Get user notifications
        public static DataTable GetUserNotifications(int userID)
        {
            return NotificationDAL.GetByUser(userID);
        }

        // Get unread notifications
        public static DataTable GetUnreadNotifications(int userID)
        {
            return NotificationDAL.GetUnreadByUser(userID);
        }

        // Get unread count (for badge)
        public static int GetUnreadCount(int userID)
        {
            return NotificationDAL.GetUnreadCount(userID);
        }

        // Mark as read
        public static bool MarkAsRead(int notificationID)
        {
            return NotificationDAL.MarkAsRead(notificationID);
        }

        // Mark all as read
        public static bool MarkAllAsRead(int userID)
        {
            return NotificationDAL.MarkAllAsRead(userID);
        }

        // Delete notification
        public static bool DeleteNotification(int notificationID)
        {
            return NotificationDAL.Delete(notificationID);
        }

        // Clear all user notifications
        public static bool ClearAllNotifications(int userID)
        {
            return NotificationDAL.DeleteAllByUser(userID);
        }

        // Auto cleanup old notifications (call from scheduled task)
        public static int CleanupOldNotifications(int daysToKeep = 30)
        {
            return NotificationDAL.DeleteOldNotifications(daysToKeep);
        }
    }
}