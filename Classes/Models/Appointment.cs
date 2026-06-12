using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int DonorID { get; set; }
        public string DonorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; }
    }
}