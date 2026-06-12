using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;  // Install-Package QRCoder

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    // Renamed to avoid colliding with QRCoder.QRCodeGenerator type
    public class QRCodeHelper
    {
        public static Bitmap GenerateQRCode(string text, int pixelsPerModule = 20)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            // fully qualify QRCoder's QRCodeGenerator to avoid name collision with this class
            using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(pixelsPerModule);
                }
            }
        }

        public static string GenerateQRDataForBag(string bagID, string bloodGroup, string expiryDate)
        {
            return $"BAG:{bagID}|BG:{bloodGroup}|EXP:{expiryDate}|BANK:BloodBankSystem";
        }

        public static string GenerateQRDataForDonor(string donorID, string name, string bloodGroup)
        {
            return $"DON:{donorID}|NAME:{name}|BG:{bloodGroup}";
        }

        public static bool SaveQRCodeToFile(string text, string filePath)
        {
            try
            {
                using (var qrCode = GenerateQRCode(text))
                {
                    qrCode.Save(filePath, ImageFormat.Png);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
