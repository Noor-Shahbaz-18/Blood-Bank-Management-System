using System;
using System.Collections.Generic;
using System.Data;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class BloodInventoryManager
    {
        public static DataTable GetAllBloodBags()
        {
            // Will connect to BloodBagDAL
            return new DataTable();
        }

        public static bool AddBloodBag(BloodBag bag)
        {
            if (bag == null) return false;
            // Validation
            if (string.IsNullOrEmpty(bag.BagID)) return false;
            if (string.IsNullOrEmpty(bag.BloodGroup)) return false;

            // Save to database
            return BloodBagDAL.Insert(bag);
        }

        public static bool UpdateBloodBag(BloodBag bag)
        {
            if (bag == null) return false;
            return BloodBagDAL.Update(bag);
        }

        public static bool DeleteBloodBag(string bagID)
        {
            return BloodBagDAL.Delete(bagID);
        }

        public static BloodBag GetBloodBagByID(string bagID)
        {
            return BloodBagDAL.GetByID(bagID);
        }

        public static DataTable GetBloodBagsByGroup(string bloodGroup)
        {
            return BloodBagDAL.GetByBloodGroup(bloodGroup);
        }

        public static int GetAvailableUnits(string bloodGroup)
        {
            return BloodBagDAL.GetAvailableCount(bloodGroup);
        }

        public static bool UpdateStockStatus(string bagID, string status)
        {
            return BloodBagDAL.UpdateStatus(bagID, status);
        }
    }
}