using System;
using System.Data;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class DonorManager
    {
        public static bool RegisterDonor(Donor donor, User user, string password)
        {
            if (donor == null) return false;

            // Validation
            if (string.IsNullOrEmpty(donor.FullName)) return false;
            if (string.IsNullOrEmpty(donor.CNIC)) return false;
            if (string.IsNullOrEmpty(donor.BloodGroup)) return false;

            // Save user first (password required)
            bool userSaved = UserDAL.Insert(user, password);
            if (!userSaved) return false;

            // Save donor
            donor.UserID = user.UserID;
            return DonorDAL.InsertDonor(donor);
        }

        public static DataTable GetAllDonors()
        {
            return DonorDAL.GetAllDonors();
        }

        public static Donor GetDonorByID(int donorID)
        {
            return DonorDAL.GetDonorByID(donorID);
        }

        public static Donor GetDonorByUserID(int userID)
        {
            return DonorDAL.GetByUserID(userID);
        }

        public static bool UpdateDonor(Donor donor)
        {
            return DonorDAL.UpdateDonor(donor);
        }

        public static DataTable SearchDonors(string searchTerm)
        {
            return DonorDAL.SearchDonors(searchTerm);
        }

        public static bool CheckEligibility(Donor donor)
        {
            // Check if donor can donate
            if (donor == null) return false;
            if (donor.Age < 18 || donor.Age > 65) return false;
            if (donor.Weight < 50) return false;
            if (donor.LastDonationDate.HasValue)
            {
                if (donor.LastDonationDate.Value.AddMonths(3) > DateTime.Today)
                    return false;
            }
            return true;
        }
    }
}