using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class AuditLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string UserRole { get; set; }
        public string Action { get; set; } // Create, Update, Delete, Login, Logout
        public string EntityType { get; set; } // Donor, Patient, BloodBag, etc.
        public string EntityID { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string IPAddress { get; set; }
        public string DeviceInfo { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string Status { get; set; } // Success, Failed
        public string ErrorMessage { get; set; }
        public string AdditionalData { get; set; }
    }
}