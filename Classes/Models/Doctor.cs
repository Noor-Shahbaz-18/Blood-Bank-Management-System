using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string PMDCNumber { get; set; } // Pakistan Medical & Dental Council
        public string Hospital { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int Experience { get; set; } // in years
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; }
        public string ProfileImage { get; set; }
        public string Qualification { get; set; }
    }
}