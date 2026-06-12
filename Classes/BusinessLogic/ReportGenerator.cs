using System;
using System.Data;
using System.Text;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class ReportGenerator
    {
        public static DataTable GenerateDonationReport(DateTime fromDate, DateTime toDate)
        {
            return ReportDAL.GetDonationReport(fromDate, toDate);
        }

        public static DataTable GenerateInventoryReport()
        {
            return ReportDAL.GetInventoryReport();
        }

        public static DataTable GenerateRequisitionReport(DateTime fromDate, DateTime toDate)
        {
            return ReportDAL.GetRequisitionReport(fromDate, toDate);
        }

        public static string ExportToCSV(DataTable data, string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // Headers
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    sb.Append(data.Columns[i].ColumnName);
                    if (i < data.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();

                // Data
                foreach (DataRow row in data.Rows)
                {
                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        sb.Append(row[i].ToString());
                        if (i < data.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();
                }

                System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return filePath;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public static string ExportToExcel(DataTable data, string filePath)
        {
            // Use EPPlus or other library for Excel export
            // Install-Package EPPlus
            try
            {
                // Placeholder - implement with EPPlus
                return filePath;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}