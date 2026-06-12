namespace BloodBankManagementSystem.Classes.Models
{
    public class BloodBank
    {
        public int BloodBankID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsActive { get; set; }
    }
}