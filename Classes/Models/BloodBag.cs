using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class BloodBag
    {
        public int BloodBagID { get; set; }
        public string BagID { get; set; }
        public string DonorID { get; set; }
        public string DonorName { get; set; }
        public string BloodGroup { get; set; }
        public string ComponentType { get; set; }
        public int Quantity { get; set; }
        public int Volume { get; set; }
        public DateTime CollectionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string StorageLocation { get; set; }
        public string Status { get; set; }
        public string ScreeningResult { get; set; }
        public string PreparedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string IssuedTo { get; set; }
        public string Remarks { get; set; }
    }
}