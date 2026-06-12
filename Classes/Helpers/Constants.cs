using System.Drawing;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class Constants
    {
        // Blood Bank Information
        public const string SYSTEM_NAME = "Blood Bank Management System";
        public const string SYSTEM_VERSION = "1.0.0";

        // Blood Donation Rules
        public const int MIN_AGE_FOR_DONATION = 18;
        public const int MAX_AGE_FOR_DONATION = 65;
        public const int MIN_WEIGHT_FOR_DONATION = 50; // in KG
        public const int MIN_HEMOGLOBIN_FOR_DONATION = 12; // in g/dL
        public const int DONATION_GAP_DAYS = 90; // 3 months

        // Blood Bag Information
        public const int STANDARD_BLOOD_UNIT_VOLUME = 450; // in mL
        public const int BLOOD_BAG_EXPIRY_DAYS = 35; // Whole blood

        // Component Expiry Days
        public const int RED_CELLS_EXPIRY_DAYS = 35;
        public const int PLATELETS_EXPIRY_DAYS = 5;
        public const int PLASMA_EXPIRY_DAYS = 365;
        public const int CRYOPRECIPITATE_EXPIRY_DAYS = 365;

        // Storage Temperatures
        public const string WHOLE_BLOOD_TEMP = "4°C ± 2°C";
        public const string PLATELETS_TEMP = "22°C ± 2°C";
        public const string PLASMA_TEMP = "-30°C or below";

        // File Paths
        public const string REPORTS_FOLDER = "Reports";
        public const string CERTIFICATES_FOLDER = "Certificates";
        public const string QR_CODES_FOLDER = "QRCodes";
        public const string BARCODES_FOLDER = "Barcodes";
        public const string LOGS_FOLDER = "Logs";

        // Application Colors
        public static readonly Color PRIMARY_COLOR = Color.FromArgb(120, 22, 27);     // Brick Red
        public static readonly Color SECONDARY_COLOR = Color.FromArgb(5, 31, 64);      // Dark Blue
        public static readonly Color SUCCESS_COLOR = Color.FromArgb(34, 197, 94);      // Green
        public static readonly Color WARNING_COLOR = Color.FromArgb(245, 158, 11);     // Orange
        public static readonly Color ERROR_COLOR = Color.FromArgb(220, 38, 38);        // Red
        public static readonly Color INFO_COLOR = Color.FromArgb(59, 130, 246);        // Blue

        // Blood Groups List
        public static readonly string[] BLOOD_GROUPS = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        // User Roles
        public static readonly string[] USER_ROLES = { "Admin", "Manager", "Doctor", "Donor", "Patient", "Technician" };
    }
}