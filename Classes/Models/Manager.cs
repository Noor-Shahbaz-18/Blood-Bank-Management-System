using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Manager
    {
        public int ManagerID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string EmployeeID { get; set; }
        public DateTime JoiningDate { get; set; }
        public int YearsOfExperience { get; set; }
        public string Qualification { get; set; }
        public string Specialization { get; set; }
        public bool IsActive { get; set; }
        public string ReportsTo { get; set; }
        public string OfficeLocation { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}