using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class ManageReports : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Label lblStatus;
        private DataGridView dgvReport;
        private ComboBox cmbReportType;
        private DateTimePicker dtpFromDate, dtpToDate;
        private Button btnGenerate, btnExport, btnNewRequisition, btnRefresh;
        private DataTable currentReportData;

        public ManageReports()
        {
            InitializeComponent();  // ✅ This calls Designer.cs version
            this.Text = "Manage Reports";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
        }

        private void BuildUI()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // top panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));   // filter panel
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // heading
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // grid
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // buttons

            // =========================================================
            // TOP PANEL
            // =========================================================
            topPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(5, 31, 64) };
            Label title = new Label
            {
                Text = "Manage Reports",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 18)
            };
            topPanel.Controls.Add(title);

            lblStatus = new Label
            {
                Text = "✅ Ready",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Location = new Point(topPanel.Width - 220, 22)
            };
            topPanel.Controls.Add(lblStatus);
            topPanel.Resize += (s, e) => lblStatus.Location = new Point(topPanel.Width - 220, 22);
            mainLayout.Controls.Add(topPanel, 0, 0);

            // =========================================================
            // FILTER PANEL
            // =========================================================
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 10),
                Margin = new Padding(20, 10, 20, 10)
            };

            // Report Type Label
            Label lblReportType = new Label
            {
                Text = "Report Type:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblReportType);

            // Report Type ComboBox
            cmbReportType = new ComboBox
            {
                Location = new Point(110, 15),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbReportType.Items.AddRange(new[] { "Donation Report", "Inventory Report", "Requisition Report", "Donor Report", "Blood Group Summary", "Expiry Alerts Report" });
            cmbReportType.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbReportType);

            // From Date Label
            Label lblFrom = new Label
            {
                Text = "From Date:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(320, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblFrom);

            // From Date Picker
            dtpFromDate = new DateTimePicker
            {
                Location = new Point(395, 15),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-30)
            };
            filterPanel.Controls.Add(dtpFromDate);

            // To Date Label
            Label lblTo = new Label
            {
                Text = "To Date:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(540, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblTo);

            // To Date Picker
            dtpToDate = new DateTimePicker
            {
                Location = new Point(600, 15),
                Size = new Size(130, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            filterPanel.Controls.Add(dtpToDate);

            // Generate Button
            btnGenerate = new Button
            {
                Text = "📊 Generate Report",
                Location = new Point(750, 13),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += BtnGenerate_Click;
            filterPanel.Controls.Add(btnGenerate);

            // New Requisition Button
            btnNewRequisition = new Button
            {
                Text = "📋 New Requisition",
                Location = new Point(895, 13),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNewRequisition.FlatAppearance.BorderSize = 0;
            btnNewRequisition.Click += BtnNewRequisition_Click;
            filterPanel.Controls.Add(btnNewRequisition);

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(1040, 13),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => BtnGenerate_Click(s, e);
            filterPanel.Controls.Add(btnRefresh);

            // Export Button
            btnExport = new Button
            {
                Text = "📎 Export",
                Location = new Point(1145, 13),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            filterPanel.Controls.Add(btnExport);

            mainLayout.Controls.Add(filterPanel, 0, 1);

            // =========================================================
            // HEADING
            // =========================================================
            Label lblHeading = new Label
            {
                Text = "📋 Report Results",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 50),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(lblHeading, 0, 2);

            // =========================================================
            // DATA GRID VIEW
            // =========================================================
            dgvReport = CreateModernDataGridView();
            mainLayout.Controls.Add(dgvReport, 0, 3);

            // =========================================================
            // BOTTOM BUTTON PANEL
            // =========================================================
            FlowLayoutPanel bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(20, 10, 20, 10)
            };

            Button btnPrint = CreateButton("🖨️ Print", Color.FromArgb(100, 110, 125));
            btnPrint.Click += BtnPrint_Click;

            Button btnClear = CreateButton("🗑️ Clear", Color.FromArgb(220, 38, 38));
            btnClear.Click += (s, e) => ClearReport();

            bottomPanel.Controls.Add(btnPrint);
            bottomPanel.Controls.Add(btnClear);
            mainLayout.Controls.Add(bottomPanel, 0, 4);

            Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 38),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private DataGridView CreateModernDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 40,
                Dock = DockStyle.Fill
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 242, 242);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(120, 22, 27);
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);

            dgv.CellFormatting += DgvReport_CellFormatting;
            return dgv;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("🔄 Generating report...", Color.FromArgb(59, 130, 246));
                btnExport.Enabled = false;
                Cursor = Cursors.WaitCursor;

                string reportType = cmbReportType.SelectedItem?.ToString();
                DataTable result = null;

                switch (reportType)
                {
                    case "Donation Report":
                        result = ReportDAL.GetDonationReport(dtpFromDate.Value, dtpToDate.Value);
                        break;
                    case "Inventory Report":
                        result = ReportDAL.GetInventoryReport();
                        break;
                    case "Requisition Report":
                        result = ReportDAL.GetRequisitionReport(dtpFromDate.Value, dtpToDate.Value);
                        break;
                    case "Donor Report":
                        result = ReportDAL.GetDonorReport();
                        break;
                    case "Blood Group Summary":
                        result = ReportDAL.GetBloodGroupSummary();
                        break;
                    case "Expiry Alerts Report":
                        result = BloodBagDAL.GetExpiringBags();
                        break;
                    default:
                        result = ReportDAL.GetDonationReport(dtpFromDate.Value, dtpToDate.Value);
                        break;
                }

                if (result != null && result.Rows.Count > 0)
                {
                    dgvReport.DataSource = result;
                    currentReportData = result;
                    btnExport.Enabled = true;
                    UpdateStatus($"✅ Report generated: {result.Rows.Count} records found", Color.FromArgb(16, 185, 129));
                }
                else
                {
                    dgvReport.DataSource = null;
                    dgvReport.Rows.Clear();
                    dgvReport.Columns.Clear();
                    dgvReport.Columns.Add("Message", "Information");
                    dgvReport.Rows.Add($"No data found for {reportType}");
                    currentReportData = null;
                    UpdateStatus("⚠️ No data found for selected criteria", Color.FromArgb(245, 158, 11));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BtnGenerate_Click Error: {ex.Message}");
                UpdateStatus($"❌ Error: {ex.Message}", Color.FromArgb(220, 38, 38));
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentReportData == null || currentReportData.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export. Please generate a report first.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string reportType = cmbReportType.SelectedItem?.ToString().Replace(" ", "_");
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".csv"))
                        ExportHelper.ExportToCSV(currentReportData, sfd.FileName);
                    else
                        ExportHelper.ExportToExcel(currentReportData, sfd.FileName);

                    MessageBox.Show($"Report exported successfully!\n\n{sfd.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    AuditHelper.Log("Export Report", "Report", $"Exported {reportType} to {sfd.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNewRequisition_Click(object sender, EventArgs e)
        {
            using (var form = new AddEditRequisition())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (cmbReportType.SelectedItem?.ToString() == "Requisition Report")
                    {
                        BtnGenerate_Click(sender, e);
                    }
                    UpdateStatus("✅ New requisition created successfully", Color.FromArgb(16, 185, 129));
                }
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("No data to print. Please generate a report first.", "Print",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                PrintDialog printDialog = new PrintDialog();
                System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.DocumentName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";

                printDoc.PrintPage += (s, pe) =>
                {
                    string title = cmbReportType.SelectedItem?.ToString() ?? "Blood Bank Report";
                    pe.Graphics.DrawString(title, new Font("Arial", 16, FontStyle.Bold),
                        Brushes.Black, 100, 50);

                    pe.Graphics.DrawString($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}",
                        new Font("Arial", 9), Brushes.Gray, 100, 80);

                    pe.Graphics.DrawString($"From: {dtpFromDate.Value:dd-MMM-yyyy} To: {dtpToDate.Value:dd-MMM-yyyy}",
                        new Font("Arial", 9), Brushes.Gray, 100, 100);

                    int y = 140;
                    int x = 50;
                    int rowHeight = 25;

                    for (int i = 0; i < dgvReport.Columns.Count && i < 6; i++)
                    {
                        pe.Graphics.DrawString(dgvReport.Columns[i].HeaderText,
                            new Font("Arial", 9, FontStyle.Bold), Brushes.Black, x, y);
                        x += 120;
                    }
                    y += rowHeight;

                    for (int r = 0; r < Math.Min(dgvReport.Rows.Count, 30); r++)
                    {
                        x = 50;
                        for (int c = 0; c < dgvReport.Columns.Count && c < 6; c++)
                        {
                            string value = dgvReport.Rows[r].Cells[c].Value?.ToString() ?? "";
                            pe.Graphics.DrawString(value, new Font("Arial", 8), Brushes.Black, x, y);
                            x += 120;
                        }
                        y += rowHeight;

                        if (y > pe.MarginBounds.Bottom - 50)
                        {
                            pe.HasMorePages = true;
                            return;
                        }
                    }
                };

                printDialog.Document = printDoc;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                    UpdateStatus("🖨️ Report sent to printer", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearReport()
        {
            dgvReport.DataSource = null;
            dgvReport.Rows.Clear();
            dgvReport.Columns.Clear();
            currentReportData = null;
            btnExport.Enabled = false;
            UpdateStatus("✅ Report cleared", Color.FromArgb(16, 185, 129));
        }

        private void DgvReport_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null) return;

            if (dgvReport.Columns[e.ColumnIndex].HeaderText == "Status" ||
                dgvReport.Columns[e.ColumnIndex].HeaderText == "Urgency")
            {
                string value = e.Value.ToString();
                if (value == "Approved" || value == "Completed" || value == "Available")
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                else if (value == "Pending" || value == "Urgent")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (value == "Rejected" || value == "Cancelled" || value == "Expired")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (value == "Emergency")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
            }

            if (dgvReport.Columns[e.ColumnIndex].HeaderText == "Days Left" && e.Value != null)
            {
                if (int.TryParse(e.Value.ToString(), out int days))
                {
                    if (days < 0)
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    else if (days <= 7)
                        e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                    else
                        e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
                }
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }
    }
}