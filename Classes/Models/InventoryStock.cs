using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class InventoryStock
    {
        public int StockID { get; set; }
        public string BloodGroup { get; set; }
        public string ComponentType { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinimumRequired { get; set; }
        public int MaximumCapacity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int ExpiringSoonCount { get; set; } // Expiring in 7 days
        public int ExpiredCount { get; set; }
        public string Status { get; set; } // Good, Low Stock, Critical
        public DateTime LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
        public string StorageLocation { get; set; }
        public string Notes { get; set; }
    }
}