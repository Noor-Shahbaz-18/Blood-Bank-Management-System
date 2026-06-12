using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Common
{
    public partial class DataGridViewPrinter : Form
    {
        private DataGridView _dgv;
        private string _title;

        public DataGridViewPrinter(DataGridView dgv, string title = "Report")
        {
            _dgv = dgv;
            _title = title;
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Print Preview";
            this.Size = new Size(800, 600);
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += PrintDoc_PrintPage;

            PrintPreviewDialog previewDialog = new PrintPreviewDialog
            {
                Document = printDoc,
                WindowState = FormWindowState.Maximized
            };
            previewDialog.ShowDialog();
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font headerFont = new Font("Arial", 10, FontStyle.Bold);
            Font dataFont = new Font("Arial", 9);
            int y = 100;
            int x = 50;
            int rowHeight = 25;

            // Title
            g.DrawString(_title, titleFont, Brushes.Black, x, y - 50);

            // Headers
            for (int i = 0; i < _dgv.Columns.Count; i++)
            {
                g.DrawString(_dgv.Columns[i].HeaderText, headerFont, Brushes.Black, x, y);
                x += 100;
            }

            y += rowHeight;
            x = 50;

            // Data (max 40 rows per page)
            int rowsPrinted = 0;
            for (int i = 0; i < _dgv.Rows.Count && rowsPrinted < 40; i++)
            {
                x = 50;
                for (int j = 0; j < _dgv.Columns.Count; j++)
                {
                    string value = _dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                    g.DrawString(value, dataFont, Brushes.Black, x, y);
                    x += 100;
                }
                y += rowHeight;
                rowsPrinted++;
            }

            e.HasMorePages = rowsPrinted < _dgv.Rows.Count;
        }
    }
}