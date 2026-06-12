using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class ExportHelper
    {
        // Export DataTable to CSV
        public static bool ExportToCSV(DataTable data, string fileName = null)
        {
            try
            {
                if (fileName == null)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "CSV files (*.csv)|*.csv";
                    sfd.DefaultExt = "csv";
                    sfd.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return false;

                    fileName = sfd.FileName;
                }

                StringBuilder sb = new StringBuilder();

                // Headers
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    sb.Append(data.Columns[i].ColumnName);
                    if (i < data.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();

                // Data
                foreach (DataRow row in data.Rows)
                {
                    for (int i = 0; i < data.Columns.Count; i++)
                    {
                        string value = row[i].ToString();
                        // Handle commas in data
                        if (value.Contains(","))
                            value = "\"" + value + "\"";
                        sb.Append(value);
                        if (i < data.Columns.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show($"Report exported successfully!\n\nLocation: {fileName}",
                    "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Export DataTable to Excel (HTML format)
        public static bool ExportToExcel(DataTable data, string fileName = null)
        {
            try
            {
                if (fileName == null)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Excel files (*.xls)|*.xls";
                    sfd.DefaultExt = "xls";
                    sfd.FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (sfd.ShowDialog() != DialogResult.OK)
                        return false;

                    fileName = sfd.FileName;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
                sb.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");

                // Headers
                sb.AppendLine("<tr style='background-color:#78161b; color:white;'>");
                foreach (DataColumn col in data.Columns)
                {
                    sb.AppendLine($"<th>{col.ColumnName}</th>");
                }
                sb.AppendLine("</tr>");

                // Data
                foreach (DataRow row in data.Rows)
                {
                    sb.AppendLine("<tr>");
                    foreach (DataColumn col in data.Columns)
                    {
                        sb.AppendLine($"<td>{row[col]}</td>");
                    }
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</table>");
                sb.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy hh:mm:ss}</p>");
                sb.AppendLine("</body></html>");

                File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show($"Report exported successfully!\n\nLocation: {fileName}",
                    "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Print DataGridView
        public static void PrintDataGridView(DataGridView dgv, string title)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                PrintDocument printDocument = new PrintDocument();

                printDocument.DocumentName = title;
                printDocument.PrintPage += (sender, e) =>
                {
                    // Simple print implementation
                    e.Graphics.DrawString(title, new System.Drawing.Font("Arial", 16),
                        System.Drawing.Brushes.Black, 100, 50);

                    int y = 100;
                    int x = 50;
                    int rowHeight = 25;

                    // Headers
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        e.Graphics.DrawString(dgv.Columns[i].HeaderText,
                            new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                            System.Drawing.Brushes.Black, x, y);
                        x += 80;
                    }

                    y += rowHeight;

                    // Data
                    for (int i = 0; i < dgv.Rows.Count && i < 30; i++)
                    {
                        x = 50;
                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            string value = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                            e.Graphics.DrawString(value, new System.Drawing.Font("Arial", 9),
                                System.Drawing.Brushes.Black, x, y);
                            x += 80;
                        }
                        y += rowHeight;
                    }
                };

                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print failed: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}