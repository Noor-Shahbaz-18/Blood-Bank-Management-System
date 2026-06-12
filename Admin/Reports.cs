using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class Reports : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private DateTimePicker fromDate, toDate;
        private Button generateBtn;
        private FlowLayoutPanel reportGrid;

        public Reports()
        {
            this.Text = "Reports - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildMainContent();
            BuildSidebar();
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 230;
            sidebarPanel.BackColor = Color.FromArgb(5, 31, 64);

            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 72;
            logoPanel.BackColor = Color.Transparent;

            Label bloodIcon = new Label();
            bloodIcon.Text = "🩸";
            bloodIcon.Font = new Font("Segoe UI Emoji", 22);
            bloodIcon.ForeColor = Color.FromArgb(220, 50, 50);
            bloodIcon.BackColor = Color.Transparent;
            bloodIcon.AutoSize = true;
            bloodIcon.Location = new Point(16, 18);

            Label logoText = new Label();
            logoText.Text = "Blood Bank";
            logoText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            logoText.ForeColor = Color.White;
            logoText.BackColor = Color.Transparent;
            logoText.AutoSize = true;
            logoText.Location = new Point(66, 22);

            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(8, 8, 8, 8);
            menuPanel.BackColor = Color.Transparent;

            AddSidebarMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                AdminDashboard dashboard = new AdminDashboard();
                dashboard.Show();
                this.Close();
            });
            AddSidebarMenuItem(menuPanel, "👥", "User Management", false, () =>
            {
                using (UserManagement f = new UserManagement()) { this.Hide(); f.ShowDialog(); this.Show(); }
            });
            AddSidebarMenuItem(menuPanel, "🔐", "Roles & Permissions", false, () =>
            {
                using (RolesPermissions f = new RolesPermissions()) { this.Hide(); f.ShowDialog(); this.Show(); }
            });
            AddSidebarMenuItem(menuPanel, "🏥", "Hospitals", false, () =>
            {
                using (HospitalManagement f = new HospitalManagement()) { this.Hide(); f.ShowDialog(); this.Show(); }
            });
            AddSidebarMenuItem(menuPanel, "🩸", "Blood Components", false, () =>
            {
                using (BloodComponents f = new BloodComponents()) { this.Hide(); f.ShowDialog(); this.Show(); }
            });
            AddSidebarMenuItem(menuPanel, "📊", "Reports", true, null);
            AddSidebarMenuItem(menuPanel, "⚙", "Settings", false, () =>
            {
                using (Settings f = new Settings()) { this.Hide(); f.ShowDialog(); this.Show(); }
            });
            AddSidebarMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                Logout logoutForm = new Logout();
                if (logoutForm.ShowDialog() == DialogResult.Yes)
                { SessionManager.Logout(); Application.Exit(); }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void AddSidebarMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 46);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent;

            Label iconLbl = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 14), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = false, Size = new Size(35, 30), Location = new Point(10, 8), TextAlign = ContentAlignment.MiddleCenter };
            Label textLbl = new Label { Text = text, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = false, Size = new Size(155, 30), Location = new Point(48, 8), TextAlign = ContentAlignment.MiddleLeft };

            EventHandler enter = (s, e) => { if (!active) item.BackColor = Color.FromArgb(200, 57, 70); };
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click; iconLbl.Click += click; textLbl.Click += click;
            }
            item.MouseEnter += enter; item.MouseLeave += leave;
            iconLbl.MouseEnter += enter; iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter; textLbl.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(245, 247, 251);
            mainPanel.Padding = new Padding(25, 20, 25, 20);
            mainPanel.AutoScroll = true;

            // REPORT CARDS GRID
            reportGrid = new FlowLayoutPanel();
            reportGrid.Dock = DockStyle.Fill;
            reportGrid.FlowDirection = FlowDirection.LeftToRight;
            reportGrid.WrapContents = true;
            reportGrid.AutoScroll = true;
            reportGrid.Padding = new Padding(0, 10, 0, 0);
            reportGrid.BackColor = Color.Transparent;

            string[] titles = { "Donation Report", "Inventory Report", "Wastage Report", "Request Report", "Hospital Report", "Expiry Report" };
            string[] icons = { "📄", "📦", "🩸", "📋", "🏢", "⏰" };
            Color[] colors = { Color.FromArgb(47, 128, 237), Color.FromArgb(39, 174, 96), Color.FromArgb(235, 87, 87), Color.FromArgb(47, 128, 237), Color.FromArgb(155, 81, 224), Color.FromArgb(242, 153, 74) };

            for (int i = 0; i < titles.Length; i++)
                reportGrid.Controls.Add(CreateReportCard(titles[i], icons[i], colors[i]));

            mainPanel.Controls.Add(reportGrid);

            // FILTER SECTION
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 90;
            filterPanel.BackColor = Color.White;
            filterPanel.Padding = new Padding(15);
            filterPanel.Paint += (s, e) =>
            {
                using (GraphicsPath path = RoundRect(((Panel)s).ClientRectangle, 12))
                    ((Panel)s).Region = new Region(path);
            };

            var fromLabel = new Label { Text = "From Date", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 90), Location = new Point(15, 12), AutoSize = true };
            fromDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 11), Size = new Size(180, 35), Location = new Point(15, 35), Value = DateTime.Today.AddDays(-30) };
            var toLabel = new Label { Text = "To Date", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(80, 80, 90), Location = new Point(220, 12), AutoSize = true };
            toDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 11), Size = new Size(180, 35), Location = new Point(220, 35), Value = DateTime.Today };

            generateBtn = new Button { Text = "Generate Report", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(120, 22, 27), FlatStyle = FlatStyle.Flat, Size = new Size(160, 40), Location = new Point(430, 28), Cursor = Cursors.Hand };
            generateBtn.FlatAppearance.BorderSize = 0;
            generateBtn.Click += GenerateBtn_Click;

            filterPanel.Controls.Add(fromLabel);
            filterPanel.Controls.Add(fromDate);
            filterPanel.Controls.Add(toLabel);
            filterPanel.Controls.Add(toDate);
            filterPanel.Controls.Add(generateBtn);

            mainPanel.Controls.Add(filterPanel);

            // TOPBAR
            Panel topbarPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            var titleLabel = new Label { Text = "Reports", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(34, 34, 34), AutoSize = true, Location = new Point(0, 5) };
            topbarPanel.Controls.Add(titleLabel);

            mainPanel.Controls.Add(topbarPanel);
            this.Controls.Add(mainPanel);
        }

        private Panel CreateReportCard(string title, string icon, Color color)
        {
            Panel card = new Panel { Size = new Size(280, 110), Margin = new Padding(0, 0, 15, 15), BackColor = Color.White, Cursor = Cursors.Hand };
            card.Paint += (s, e) =>
            {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(p.ClientRectangle, 15))
                {
                    p.Region = new Region(path);
                    using (var br = new SolidBrush(Color.White)) e.Graphics.FillPath(br, path);
                    using (var pen = new Pen(Color.FromArgb(220, 220, 225), 1)) e.Graphics.DrawPath(pen, path);
                }
            };
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(250, 251, 253);
            card.MouseLeave += (s, e) => card.BackColor = Color.White;

            Panel iconBox = new Panel { Size = new Size(55, 55), Location = new Point(18, 27), BackColor = color };
            iconBox.Paint += (s, e) =>
            {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundRect(p.ClientRectangle, 12))
                { p.Region = new Region(path); using (var br = new SolidBrush(p.BackColor)) e.Graphics.FillPath(br, path); }
            };
            var iconLbl = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 20), ForeColor = Color.White, BackColor = Color.Transparent, Size = new Size(55, 55), Location = new Point(0, 0), TextAlign = ContentAlignment.MiddleCenter };
            iconBox.Controls.Add(iconLbl);

            var titleLbl = new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(34, 34, 34), Location = new Point(88, 25), AutoSize = true };
            var viewLink = new Label { Text = "View Report →", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(13, 110, 253), Location = new Point(88, 55), AutoSize = true, Cursor = Cursors.Hand };
            viewLink.Click += (s, e) => ViewReport(title);

            card.Controls.Add(iconBox);
            card.Controls.Add(titleLbl);
            card.Controls.Add(viewLink);
            return card;
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime from = fromDate.Value;
                DateTime to = toDate.Value;

                if (from > to)
                {
                    MessageBox.Show("From Date cannot be greater than To Date!",
                        "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get all report data
                DataTable dtDonation = ReportDAL.GetDonationReport(from, to);
                DataTable dtRequisition = ReportDAL.GetRequisitionReport(from, to);
                DataTable dtInventory = ReportDAL.GetInventoryReport();
                DataTable dtExpiry = ReportDAL.GetBloodGroupSummary();
                DataTable dtHospital = HospitalDAL.GetAllHospitals();

                int donationCount = dtDonation?.Rows.Count ?? 0;
                int requisitionCount = dtRequisition?.Rows.Count ?? 0;

                if (donationCount == 0 && requisitionCount == 0 && dtInventory?.Rows.Count == 0)
                {
                    MessageBox.Show($"No data found for period {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}.\n\nPlease change date range.",
                        "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create report form with tabs
                Form reportForm = new Form();
                reportForm.Text = $"Report Summary - {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}";
                reportForm.Size = new Size(1200, 800);
                reportForm.StartPosition = FormStartPosition.CenterScreen;
                reportForm.BackColor = Color.White;

                // Header Panel
                Panel headerPanel = new Panel();
                headerPanel.Dock = DockStyle.Top;
                headerPanel.Height = 100;
                headerPanel.BackColor = Color.FromArgb(120, 22, 27);
                headerPanel.Padding = new Padding(20);

                Label lblTitle = new Label();
                lblTitle.Text = "📊 GENERATED REPORTS";
                lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
                lblTitle.ForeColor = Color.White;
                lblTitle.Dock = DockStyle.Top;
                lblTitle.Height = 45;
                lblTitle.TextAlign = ContentAlignment.MiddleLeft;
                headerPanel.Controls.Add(lblTitle);

                Label lblPeriod = new Label();
                lblPeriod.Text = $"Period: {from:dd-MMM-yyyy} to {to:dd-MMM-yyyy}  |  Generated: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}";
                lblPeriod.Font = new Font("Segoe UI", 11);
                lblPeriod.ForeColor = Color.FromArgb(220, 220, 220);
                lblPeriod.Dock = DockStyle.Top;
                lblPeriod.Height = 30;
                lblPeriod.TextAlign = ContentAlignment.MiddleLeft;
                headerPanel.Controls.Add(lblPeriod);

                // Tab Control
                TabControl tabControl = new TabControl();
                tabControl.Dock = DockStyle.Fill;
                tabControl.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                // Tab 1: Donation Report
                if (dtDonation != null && dtDonation.Rows.Count > 0)
                {
                    TabPage tabDonation = new TabPage("📄 Donation Report");
                    DataGridView dgvDonation = CreateDataGridView();
                    dgvDonation.DataSource = dtDonation;
                    tabDonation.Controls.Add(dgvDonation);
                    tabControl.TabPages.Add(tabDonation);
                }

                // Tab 2: Requisition Report
                if (dtRequisition != null && dtRequisition.Rows.Count > 0)
                {
                    TabPage tabRequisition = new TabPage("📋 Requisition Report");
                    DataGridView dgvRequisition = CreateDataGridView();
                    dgvRequisition.DataSource = dtRequisition;
                    tabRequisition.Controls.Add(dgvRequisition);
                    tabControl.TabPages.Add(tabRequisition);
                }

                // Tab 3: Inventory Report
                if (dtInventory != null && dtInventory.Rows.Count > 0)
                {
                    TabPage tabInventory = new TabPage("📦 Inventory Report");
                    DataGridView dgvInventory = CreateDataGridView();
                    dgvInventory.DataSource = dtInventory;
                    tabInventory.Controls.Add(dgvInventory);
                    tabControl.TabPages.Add(tabInventory);
                }

                // Tab 4: Expiry Report
                if (dtExpiry != null && dtExpiry.Rows.Count > 0)
                {
                    TabPage tabExpiry = new TabPage("⏰ Expiry Report");
                    DataGridView dgvExpiry = CreateDataGridView();
                    dgvExpiry.DataSource = dtExpiry;
                    tabExpiry.Controls.Add(dgvExpiry);
                    tabControl.TabPages.Add(tabExpiry);
                }

                // Tab 5: Hospital Report
                if (dtHospital != null && dtHospital.Rows.Count > 0)
                {
                    TabPage tabHospital = new TabPage("🏥 Hospital Report");
                    DataGridView dgvHospital = CreateDataGridView();
                    dgvHospital.DataSource = dtHospital;
                    tabHospital.Controls.Add(dgvHospital);
                    tabControl.TabPages.Add(tabHospital);
                }

                // Export All Button
                Button btnExportAll = new Button();
                btnExportAll.Text = "📎 Export All Reports";
                btnExportAll.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btnExportAll.BackColor = Color.White;
                btnExportAll.ForeColor = Color.FromArgb(120, 22, 27);
                btnExportAll.FlatStyle = FlatStyle.Flat;
                btnExportAll.Size = new Size(160, 35);
                btnExportAll.Location = new Point(headerPanel.Width - 180, 55);
                btnExportAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnExportAll.Click += (s, ev) => ExportAllReports(dtDonation, dtRequisition, dtInventory, dtExpiry, dtHospital);
                headerPanel.Controls.Add(btnExportAll);

                reportForm.Controls.Add(tabControl);
                reportForm.Controls.Add(headerPanel);
                reportForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridView CreateDataGridView()
        {
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Font = new Font("Segoe UI", 9);
            dgv.RowTemplate.Height = 32;
            dgv.GridColor = Color.FromArgb(220, 223, 230);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.EnableHeadersVisualStyles = false;
            return dgv;
        }

        private void ExportAllReports(DataTable dtDonation, DataTable dtRequisition, DataTable dtInventory, DataTable dtExpiry, DataTable dtHospital)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.DefaultExt = "csv";
            sfd.FileName = $"Complete_Report_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string basePath = sfd.FileName.Replace(".csv", "");
                int exportCount = 0;

                // Export Donation Report
                if (dtDonation != null && dtDonation.Rows.Count > 0)
                {
                    ExportHelper.ExportToCSV(dtDonation, $"{basePath}_Donation.csv");
                    exportCount++;
                }

                // Export Requisition Report
                if (dtRequisition != null && dtRequisition.Rows.Count > 0)
                {
                    ExportHelper.ExportToCSV(dtRequisition, $"{basePath}_Requisition.csv");
                    exportCount++;
                }

                // Export Inventory Report
                if (dtInventory != null && dtInventory.Rows.Count > 0)
                {
                    ExportHelper.ExportToCSV(dtInventory, $"{basePath}_Inventory.csv");
                    exportCount++;
                }

                // Export Expiry Report
                if (dtExpiry != null && dtExpiry.Rows.Count > 0)
                {
                    ExportHelper.ExportToCSV(dtExpiry, $"{basePath}_Expiry.csv");
                    exportCount++;
                }

                // Export Hospital Report
                if (dtHospital != null && dtHospital.Rows.Count > 0)
                {
                    ExportHelper.ExportToCSV(dtHospital, $"{basePath}_Hospital.csv");
                    exportCount++;
                }

                MessageBox.Show($"{exportCount} report(s) exported successfully!\n\nLocation: {basePath}_*.csv",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ViewReport(string title)
        {
            try
            {
                DateTime from = fromDate.Value;
                DateTime to = toDate.Value;
                DataTable dt = new DataTable();

                switch (title)
                {
                    case "Donation Report":
                        dt = ReportDAL.GetDonationReport(from, to);
                        break;
                    case "Inventory Report":
                        dt = ReportDAL.GetInventoryReport();
                        break;
                    case "Wastage Report":
                        dt = ReportDAL.GetInventoryReport();
                        break;
                    case "Request Report":
                        dt = ReportDAL.GetRequisitionReport(from, to);
                        break;
                    case "Hospital Report":
                        dt = HospitalDAL.GetAllHospitals();
                        break;
                    case "Expiry Report":
                        dt = ReportDAL.GetBloodGroupSummary();
                        break;
                    default:
                        MessageBox.Show("Report type not implemented yet.", "Info",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                }

                ShowReportWindow(title, dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowReportWindow(string title, DataTable data)
        {
            if (data == null || data.Rows.Count == 0)
            {
                MessageBox.Show($"No data found for {title}.\n\nPlease check your date range.",
                    title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form reportForm = new Form();
            reportForm.Text = title;
            reportForm.Size = new Size(1100, 700);
            reportForm.StartPosition = FormStartPosition.CenterScreen;
            reportForm.BackColor = Color.White;

            // Header Panel
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 85;
            headerPanel.BackColor = Color.FromArgb(120, 22, 27);
            headerPanel.Padding = new Padding(20, 0, 20, 0);

            Label lblReportTitle = new Label();
            lblReportTitle.Text = title;
            lblReportTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblReportTitle.ForeColor = Color.White;
            lblReportTitle.Location = new Point(20, 12);
            lblReportTitle.AutoSize = true;
            headerPanel.Controls.Add(lblReportTitle);

            Label lblDate = new Label();
            lblDate.Text = $"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}";
            lblDate.Font = new Font("Segoe UI", 9);
            lblDate.ForeColor = Color.FromArgb(210, 210, 210);
            lblDate.Location = new Point(22, 50);
            lblDate.AutoSize = true;
            headerPanel.Controls.Add(lblDate);

            Button btnExport = new Button();
            btnExport.Text = "📎 Export to Excel";
            btnExport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnExport.BackColor = Color.White;
            btnExport.ForeColor = Color.FromArgb(120, 22, 27);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Size = new Size(155, 36);
            btnExport.Cursor = Cursors.Hand;
            btnExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (s, e) => ExportToExcel(data, title);
            headerPanel.Controls.Add(btnExport);

            headerPanel.Resize += (s, e) =>
                btnExport.Location = new Point(headerPanel.Width - btnExport.Width - 20, 24);

            DataGridView dgv = CreateDataGridView();
            dgv.DataSource = data;

            reportForm.Controls.Add(dgv);
            reportForm.Controls.Add(headerPanel);
            reportForm.ShowDialog();
        }

        private void ExportToExcel(DataTable data, string title)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.DefaultExt = "csv";
            sfd.FileName = $"{title}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportHelper.ExportToCSV(data, sfd.FileName);
                MessageBox.Show($"Report exported successfully!\n\nLocation: {sfd.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}