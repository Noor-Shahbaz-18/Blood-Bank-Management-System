using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Complaint
    {
        public int ComplaintID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string ComplaintType { get; set; }
        public string IssueDescription { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}