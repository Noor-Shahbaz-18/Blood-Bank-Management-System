using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class BloodComponent
    {
        public int ComponentID { get; set; }
        public string ComponentName { get; set; }
        public string ComponentCode { get; set; }
        public string BloodGroup { get; set; }
        public int Quantity { get; set; }
        public int Volume { get; set; }
        public string StorageTemperature { get; set; }
        public int ShelfLife { get; set; } // in days
        public DateTime PreparationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ParentBagID { get; set; }
        public string Status { get; set; }
        public string PreparedBy { get; set; }
        public string StorageLocation { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}