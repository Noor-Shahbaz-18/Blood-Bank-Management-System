using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class DonationCamp
    {
        public int CampID { get; set; }
        public string CampName { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Organizer { get; set; }
        public string ContactNumber { get; set; }
        public string Status { get; set; } // Upcoming, Ongoing, Completed, Cancelled
        public int TargetDonors { get; set; }
        public int DonorsCollected { get; set; }
        public int UnitsCollected { get; set; }
        public string CoordinatorName { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}