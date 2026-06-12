using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }  // NEW - For display
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }  // Normal, Important, Urgent, Emergency, Broadcast
        public string Priority { get; set; }  // NEW - Low, Medium, High, Critical
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }  // NEW - When user read it
        public string ActionUrl { get; set; }  // NEW - Click to navigate (e.g., "ViewRequest/123")
        public int RelatedID { get; set; }  // NEW - Related record ID (request ID, donation ID, etc.)
        public string RelatedType { get; set; }  // NEW - "Requisition", "Donation", "BloodBag"
        public string SentBy { get; set; }  // NEW - Who sent this notification
        public DateTime? ExpiryDate { get; set; }  // NEW - Auto-delete after this date
    }
}