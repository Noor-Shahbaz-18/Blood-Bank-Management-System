namespace BloodBankManagementSystem.Classes.Enums
{
    public enum BloodGroup
    {
        APositive = 1,
        ANegative = 2,
        BPositive = 3,
        BNegative = 4,
        ABPositive = 5,
        ABNegative = 6,
        OPositive = 7,
        ONegative = 8
    }

    // Helper extension methods
    public static class BloodGroupExtensions
    {
        public static string ToDisplayString(this BloodGroup bloodGroup)
        {
            switch (bloodGroup)
            {
                case BloodGroup.APositive: return "A+";
                case BloodGroup.ANegative: return "A-";
                case BloodGroup.BPositive: return "B+";
                case BloodGroup.BNegative: return "B-";
                case BloodGroup.ABPositive: return "AB+";
                case BloodGroup.ABNegative: return "AB-";
                case BloodGroup.OPositive: return "O+";
                case BloodGroup.ONegative: return "O-";
                default: return "Unknown";
            }
        }

        public static BloodGroup FromString(string bloodGroup)
        {
            switch (bloodGroup)
            {
                case "A+": return BloodGroup.APositive;
                case "A-": return BloodGroup.ANegative;
                case "B+": return BloodGroup.BPositive;
                case "B-": return BloodGroup.BNegative;
                case "AB+": return BloodGroup.ABPositive;
                case "AB-": return BloodGroup.ABNegative;
                case "O+": return BloodGroup.OPositive;
                case "O-": return BloodGroup.ONegative;
                default: return BloodGroup.OPositive;
            }
        }
    }
}