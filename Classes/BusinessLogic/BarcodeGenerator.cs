using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;  // You need to install: Install-Package ZXing.Net

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class BarcodeGenerator
    {
        public static Bitmap GenerateBarcode(string text, int width = 300, int height = 100)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10
                }
            };

            var pixelData = writer.Write(text);

            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                return new Bitmap(bitmap);
            }
        }

        public static string GenerateBagID()
        {
            return $"BB-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public static string GenerateDonorID()
        {
            return $"DON-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        public static bool SaveBarcodeToFile(string text, string filePath)
        {
            try
            {
                using (var barcode = GenerateBarcode(text))
                {
                    barcode.Save(filePath, ImageFormat.Png);
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