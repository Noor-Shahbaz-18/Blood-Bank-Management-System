namespace BloodBankManagementSystem.Classes.Models
{
    public class Hospital
    {
        public int HospitalID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public bool IsActive { get; set; }
        public int RegistrationNumber { get; set; }
    }
}