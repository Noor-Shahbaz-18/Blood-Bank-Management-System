using System;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class DateTimeHelper
    {
        // Get formatted date string
        public static string ToFormattedDate(this DateTime date)
        {
            return date.ToString("dd-MMM-yyyy");
        }

        // Get formatted date and time
        public static string ToFormattedDateTime(this DateTime date)
        {
            return date.ToString("dd-MMM-yyyy hh:mm tt");
        }

        // Get time only
        public static string ToFormattedTime(this DateTime date)
        {
            return date.ToString("hh:mm tt");
        }

        // Get age from date of birth
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        // Check if eligible for donation
        public static bool IsEligibleForDonation(DateTime lastDonationDate)
        {
            return lastDonationDate.AddDays(Constants.DONATION_GAP_DAYS) <= DateTime.Today;
        }

        // Get next eligible date
        public static DateTime GetNextEligibleDate(DateTime lastDonationDate)
        {
            return lastDonationDate.AddDays(Constants.DONATION_GAP_DAYS);
        }

        // Calculate expiry date for blood bag
        public static DateTime CalculateExpiryDate(DateTime collectionDate, string componentType)
        {
            switch (componentType)
            {
                case "Whole Blood":
                case "Red Cells":
                    return collectionDate.AddDays(Constants.RED_CELLS_EXPIRY_DAYS);
                case "Platelets":
                    return collectionDate.AddDays(Constants.PLATELETS_EXPIRY_DAYS);
                case "Plasma":
                case "Cryoprecipitate":
                    return collectionDate.AddDays(Constants.PLASMA_EXPIRY_DAYS);
                default:
                    return collectionDate.AddDays(Constants.BLOOD_BAG_EXPIRY_DAYS);
            }
        }

        // Get days until expiry
        public static int GetDaysUntilExpiry(DateTime expiryDate)
        {
            return (expiryDate.Date - DateTime.Today).Days;
        }

        // Check if expired
        public static bool IsExpired(DateTime expiryDate)
        {
            return expiryDate.Date < DateTime.Today;
        }

        // Check if expiring soon (within 7 days)
        public static bool IsExpiringSoon(DateTime expiryDate)
        {
            int daysLeft = GetDaysUntilExpiry(expiryDate);
            return daysLeft >= 0 && daysLeft <= 7;
        }

        // Get current financial year
        public static string GetCurrentFinancialYear()
        {
            int currentYear = DateTime.Now.Year;
            int nextYear = currentYear + 1;
            return $"{currentYear}-{nextYear}";
        }

        // Get start of week (Monday)
        public static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        // Get end of week (Sunday)
        public static DateTime GetEndOfWeek(DateTime date)
        {
            return GetStartOfWeek(date).AddDays(6);
        }

        // Get date range for last 30 days
        public static (DateTime from, DateTime to) GetLast30Days()
        {
            var to = DateTime.Today;
            var from = to.AddDays(-30);
            return (from, to);
        }

        // Get date range for current month
        public static (DateTime from, DateTime to) GetCurrentMonth()
        {
            var from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            return (from, to);
        }
    }
}