using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Technician
    {
        public int TechnicianID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string EmployeeID { get; set; }
        public string Designation { get; set; }
        public string Specialization { get; set; } // Lab Technician, Phlebotomist, etc.
        public string Qualification { get; set; }
        public int YearsOfExperience { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Shift { get; set; } // Morning, Evening, Night
        public DateTime JoiningDate { get; set; }
        public string CertificationNumber { get; set; }
        public string WorkingLocation { get; set; }
        public bool IsActive { get; set; }
        public int TestsPerformed { get; set; }
        public int BagsProcessed { get; set; }
        public DateTime LastActiveDate { get; set; }
        public string Notes { get; set; }
    }
}