using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class RecentInventoryStatus : Form
    {
        private Panel pnlSidebar, pnlMain, pnlTopBar, pnlContent;
        private Panel pnlFilterCard;
        private DataGridView dgvInventory;
        private ComboBox cmbBloodGroup, cmbComponentType, cmbStatus;
        private Button btnRefresh, btnExport, btnSearch;
        private TextBox txtSearchBagID, txtSearchDonor;
        private Label lblTotalUnits, lblAvailableBags, lblExpiringBags, lblLowStock, lblStatus;
        private Button _activeNavBtn = null;
        private Timer refreshTimer;
        private DataTable originalData;

        private static readonly Color C_SidebarBg1 = Color.FromArgb(22, 33, 62);
        private static readonly Color C_SidebarBg2 = Color.FromArgb(15, 23, 42);
        private static readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private static readonly Color brickRedLight = Color.FromArgb(254, 242, 242);
        private static readonly Color C_White = Color.White;
        private static readonly Color C_ContentBg = Color.FromArgb(245, 247, 250);
        private static readonly Color C_CardBg = Color.White;
        private static readonly Color C_TextDark = Color.FromArgb(31, 41, 55);
        private static readonly Color C_TextMid = Color.FromArgb(107, 114, 128);
        private static readonly Color C_Border = Color.FromArgb(229, 231, 235);
        private static readonly Color C_Normal = Color.FromArgb(16, 185, 129);
        private static readonly Color C_Warning = Color.FromArgb(245, 158, 11);
        private static readonly Color C_Critical = Color.FromArgb(220, 38, 38);

        private const int SIDEBAR_W = 280;
        private const int TOPBAR_H = 80;

        public RecentInventoryStatus()
        {
            this.Text = "Blood Bank Management System – Inventory Status";
            this.BackColor = C_ContentBg;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1150, 700);
            this.Font = new Font("Segoe UI", 9.25f);

            BuildLayout();
            BuildSidebar();
            BuildTopBar();
            BuildContentArea();

            // Load data from database
            LoadInventoryFromDatabase();

            // Auto-refresh every 2 minutes
            refreshTimer = new Timer { Interval = 120000 };
            refreshTimer.Tick += (s, e) => LoadInventoryFromDatabase();
            refreshTimer.Start();
        }

        private void BuildLayout()
        {
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = SIDEBAR_W, BackColor = Color.Transparent };
            pnlSidebar.Paint += (s, e) =>
            {
                using (LinearGradientBrush br = new LinearGradientBrush(pnlSidebar.ClientRectangle, C_SidebarBg1, C_SidebarBg2, 90F))
                    e.Graphics.FillRectangle(br, pnlSidebar.ClientRectangle);
            };

            pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = C_ContentBg };
            pnlTopBar = new Panel { Dock = DockStyle.Top, Height = TOPBAR_H, BackColor = C_White, Padding = new Padding(30, 0, 30, 0) };
            pnlTopBar.Paint += (s, e) => { using (var pen = new Pen(C_Border, 1)) e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1); };
            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = C_ContentBg, AutoScroll = true, Padding = new Padding(30, 20, 30, 30) };

            pnlMain.Controls.Add(pnlContent);
            pnlMain.Controls.Add(pnlTopBar);
            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlSidebar);
        }

        private void BuildSidebar()
        {
            Panel logoContainer = new Panel { Size = new Size(220, 130), Location = new Point(30, 20), BackColor = C_White };
            SetRoundedRegion(logoContainer, 12);
            pnlSidebar.Controls.Add(logoContainer);

            TableLayoutPanel logoTable = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(5) };
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23));
            logoContainer.Controls.Add(logoTable);

            PictureBox bloodLogo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill, BackColor = C_White };
            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(C_White);
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(25, 45, 50, 50);
                    path.AddPolygon(new Point[] { new Point(50, 15), new Point(32, 55), new Point(68, 55) });
                    path.FillMode = FillMode.Winding;
                    g.FillPath(brush, path);
                }
            }
            bloodLogo.Image = bmp;
            logoTable.Controls.Add(bloodLogo, 0, 0);
            logoTable.Controls.Add(new Label { Text = "Blood Bank", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = brickRed, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 0, 1);
            logoTable.Controls.Add(new Label { Text = "Management System", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(107, 114, 128), Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter }, 0, 2);

            pnlSidebar.Controls.Add(new Panel { Size = new Size(220, 1), BackColor = brickRed, Location = new Point(30, 165) });
            pnlSidebar.Controls.Add(new Label { Text = "MAIN MENU", Location = new Point(45, 185), AutoSize = true, ForeColor = Color.FromArgb(150, 170, 190), Font = new Font("Segoe UI", 8f, FontStyle.Bold) });

            Panel navContainer = new Panel { Location = new Point(15, 215), Size = new Size(250, pnlSidebar.Height - 280), BackColor = Color.Transparent, AutoScroll = true };
            pnlSidebar.Controls.Add(navContainer);
            FlowLayoutPanel navPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };
            navContainer.Controls.Add(navPanel);

            (string label, string icon, Action action)[] items = {
                ("Dashboard", "🏠", () => { TechnicianDashboard.Instance.Show(); this.Close(); }),
                ("Assign Storage", "📦", () => { new AssignStorageLocation().Show(); this.Close(); }),
                ("Medical Screening", "🩺", () => { new DonorMedicalScreening().Show(); this.Close(); }),
                ("Receive Bag", "📥", () => { new ReceiveBag().Show(); this.Close(); }),
                ("Component Prep", "⚙️", () => { new ComponentPreparation().Show(); this.Close(); }),
                ("Blood Collection", "🩸", () => { new BloodCollectionDetails().Show(); this.Close(); }),
                ("Defer Donor", "🚫", () => { new DeferDonor().Show(); this.Close(); }),
                ("Discard Blood", "🗑️", () => { new DiscardBlood().Show(); this.Close(); }),
                ("Print Labels", "🖨️", () => { new PrintBarcodeLabels().Show(); this.Close(); }),
                ("Quality Control", "🔬", () => { new QualityControl().Show(); this.Close(); }),
                ("TTI Screening", "🧪", () => { new TTIScreeningResults().Show(); this.Close(); }),
                ("Cross Matching", "🔄", () => { new TechnicianCrossMatching().Show(); this.Close(); }),
                ("Bag Tracking", "📊", () => { new BloodBagTracking().Show(); this.Close(); }),
                ("Expiry Alerts", "⚠️", () => { new ExpiryAlerts().Show(); this.Close(); }),
                ("Inventory", "📋", null),
                ("Logout", "⏻", () => Application.Exit())
            };

            int yPos = 0;
            foreach (var (label, icon, action) in items)
            {
                Button btn = new Button
                {
                    Text = $"   {icon}   {label}",
                    Size = new Size(220, 42),
                    Location = new Point(0, yPos),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    ForeColor = C_White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9.5f),
                    Cursor = Cursors.Hand,
                    Padding = new Padding(12, 0, 0, 0),
                    Margin = new Padding(0, 0, 0, 2)
                };
                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 65, 85);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
                if (label == "Inventory")
                {
                    btn.BackColor = Color.FromArgb(50, 65, 85);
                    _activeNavBtn = btn;
                }
                btn.Click += (s, e) => { if (label != "Inventory" && label != "Logout") SetActiveNav(btn); action?.Invoke(); };
                navPanel.Controls.Add(btn);
                yPos += 46;
            }

            Panel spacer = new Panel { Height = 20, BackColor = Color.Transparent };
            navPanel.Controls.Add(spacer);

            Button btnLogout = new Button
            {
                Text = "   ⏻   Logout",
                Size = new Size(220, 42),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = C_White,
                BackColor = Color.FromArgb(50, 65, 85),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 0, 0)
            };
            btnLogout.Click += (s, e) => Application.Exit();
            navPanel.Controls.Add(btnLogout);

            navContainer.Height = pnlSidebar.Height - 280;
            pnlSidebar.Resize += (s, e) => { navContainer.Height = pnlSidebar.Height - 280; };
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeNavBtn != null) { _activeNavBtn.BackColor = Color.Transparent; _activeNavBtn.ForeColor = C_White; }
            _activeNavBtn = btn;
            btn.BackColor = Color.FromArgb(50, 65, 85);
            btn.ForeColor = C_White;
        }

        private void BuildTopBar()
        {
            Label lblPage = new Label { Text = "Inventory Status", Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = C_TextDark, AutoSize = true, Location = new Point(30, 22) };
            pnlTopBar.Controls.Add(lblPage);

            Label lblDate = new Label { Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"), Font = new Font("Segoe UI", 9f), ForeColor = C_TextMid, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(pnlTopBar.Width - 380, 28) };
            pnlTopBar.Controls.Add(lblDate);

            Button btnNotif = new Button { Text = "🔔", Font = new Font("Segoe UI Emoji", 14f), FlatStyle = FlatStyle.Flat, BackColor = brickRedLight, ForeColor = brickRed, Size = new Size(38, 38), Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter };
            btnNotif.FlatAppearance.BorderSize = 0;
            btnNotif.Location = new Point(pnlTopBar.Width - 170, 18);
            SetRoundedRegion(btnNotif, 8);
            pnlTopBar.Controls.Add(btnNotif);

            Button btnSettings = new Button { Text = "⚙️", Font = new Font("Segoe UI Emoji", 14f), FlatStyle = FlatStyle.Flat, BackColor = brickRedLight, ForeColor = brickRed, Size = new Size(38, 38), Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Location = new Point(pnlTopBar.Width - 120, 18);
            SetRoundedRegion(btnSettings, 8);
            pnlTopBar.Controls.Add(btnSettings);

            pnlTopBar.Resize += (s, e) =>
            {
                lblDate.Location = new Point(pnlTopBar.Width - 380, 28);
                btnNotif.Location = new Point(pnlTopBar.Width - 170, 18);
                btnSettings.Location = new Point(pnlTopBar.Width - 120, 18);
            };
        }

        private void BuildContentArea()
        {
            int y = 20;

            // Status label
            lblStatus = new Label
            {
                Text = "✅ Loading inventory...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(pnlContent.Width - 250, y),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlContent.Controls.Add(lblStatus);

            // Stats Cards Panel
            Panel statsPanel = new Panel { Location = new Point(0, y + 25), Width = pnlContent.ClientSize.Width - 60, Height = 100, BackColor = Color.Transparent, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlContent.Controls.Add(statsPanel);

            int cardW = (statsPanel.Width - 60) / 4;
            int cardH = 85;

            Panel card1 = CreateStatCard("Total Units", "0", C_Normal, cardW, cardH);
            card1.Location = new Point(0, 0);
            statsPanel.Controls.Add(card1);
            lblTotalUnits = card1.Controls[2] as Label;

            Panel card2 = CreateStatCard("Available Bags", "0", C_Normal, cardW, cardH);
            card2.Location = new Point(cardW + 20, 0);
            statsPanel.Controls.Add(card2);
            lblAvailableBags = card2.Controls[2] as Label;

            Panel card3 = CreateStatCard("Expiring Soon", "0", C_Warning, cardW, cardH);
            card3.Location = new Point((cardW + 20) * 2, 0);
            statsPanel.Controls.Add(card3);
            lblExpiringBags = card3.Controls[2] as Label;

            Panel card4 = CreateStatCard("Low Stock", "0", C_Critical, cardW, cardH);
            card4.Location = new Point((cardW + 20) * 3, 0);
            statsPanel.Controls.Add(card4);
            lblLowStock = card4.Controls[2] as Label;

            y += statsPanel.Height + 20;

            // Search Panel
            Panel pnlSearch = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 45,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(pnlSearch, 12);
            AddDropShadow(pnlSearch);
            pnlContent.Controls.Add(pnlSearch);

            Label lblSearch = new Label { Text = "Search Bag ID:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(15, 15), AutoSize = true };
            pnlSearch.Controls.Add(lblSearch);

            txtSearchBagID = new TextBox { Location = new Point(100, 12), Width = 120, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            pnlSearch.Controls.Add(txtSearchBagID);

            Label lblSearchDonor = new Label { Text = "Donor:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(240, 15), AutoSize = true };
            pnlSearch.Controls.Add(lblSearchDonor);

            txtSearchDonor = new TextBox { Location = new Point(290, 12), Width = 120, Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.FixedSingle };
            pnlSearch.Controls.Add(txtSearchDonor);

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(430, 10),
                Size = new Size(80, 28),
                BackColor = brickRed,
                ForeColor = C_White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnSearch, 6);
            btnSearch.Click += (s, e) => SearchInventory();
            pnlSearch.Controls.Add(btnSearch);

            y += pnlSearch.Height + 15;

            // Filter Card
            pnlFilterCard = new Panel { Location = new Point(0, y), Width = pnlContent.ClientSize.Width - 60, Height = 50, BackColor = C_CardBg, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            SetRoundedRegion(pnlFilterCard, 12);
            AddDropShadow(pnlFilterCard);
            pnlContent.Controls.Add(pnlFilterCard);

            pnlFilterCard.Controls.Add(new Label { Text = "Filter:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = C_TextDark, Location = new Point(15, 18), AutoSize = true });

            pnlFilterCard.Controls.Add(new Label { Text = "Blood Group:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(80, 18), AutoSize = true });
            cmbBloodGroup = new ComboBox { Location = new Point(165, 14), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbBloodGroup.Items.AddRange(new[] { "All", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            cmbBloodGroup.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbBloodGroup);

            pnlFilterCard.Controls.Add(new Label { Text = "Component:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(285, 18), AutoSize = true });
            cmbComponentType = new ComboBox { Location = new Point(355, 14), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbComponentType.Items.AddRange(new[] { "All", "Whole Blood", "Plasma", "Red Cells", "Platelets", "Cryoprecipitate" });
            cmbComponentType.SelectedIndex = 0;
            cmbComponentType.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbComponentType);

            pnlFilterCard.Controls.Add(new Label { Text = "Status:", Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(495, 18), AutoSize = true });
            cmbStatus = new ComboBox { Location = new Point(545, 14), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbStatus.Items.AddRange(new[] { "All", "Available", "In Lab", "Issued", "Quarantined" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            pnlFilterCard.Controls.Add(cmbStatus);

            btnRefresh = new Button { Text = "🔄 Refresh", Location = new Point(pnlFilterCard.Width - 180, 12), Size = new Size(85, 30), BackColor = Color.FromArgb(60, 70, 80), ForeColor = C_White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnRefresh, 6);
            btnRefresh.Click += (s, e) => LoadInventoryFromDatabase();
            pnlFilterCard.Controls.Add(btnRefresh);

            btnExport = new Button { Text = "📊 Export", Location = new Point(pnlFilterCard.Width - 90, 12), Size = new Size(80, 30), BackColor = brickRed, ForeColor = C_White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnExport.FlatAppearance.BorderSize = 0;
            SetRoundedRegion(btnExport, 6);
            btnExport.Click += (s, e) => ExportToExcel();
            pnlFilterCard.Controls.Add(btnExport);

            y += pnlFilterCard.Height + 20;

            // Inventory Grid Header
            Panel headerPanel = new Panel
            {
                Location = new Point(0, y),
                Width = pnlContent.ClientSize.Width - 60,
                Height = 35,
                BackColor = Color.Transparent
            };
            pnlContent.Controls.Add(headerPanel);

            Label lblInventory = new Label
            {
                Text = "📋 Current Inventory Stock",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_TextDark,
                Location = new Point(0, 5),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblInventory);
            y += 35;

            dgvInventory = CreateStyledGrid();
            dgvInventory.Location = new Point(0, y);
            dgvInventory.Height = 380;
            dgvInventory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvInventory.RowPrePaint += DgvInventory_RowPrePaint;
            WrapInCard(dgvInventory, pnlContent, new Point(0, y), 380);
            FixDataGridView(dgvInventory);

            Panel bottomSpacer = new Panel { Height = 30, BackColor = Color.Transparent, Dock = DockStyle.Bottom };
            pnlContent.Controls.Add(bottomSpacer);

            // Resize event
            pnlContent.Resize += (s, e) =>
            {
                if (lblStatus != null)
                    lblStatus.Location = new Point(pnlContent.Width - 270, 20);

                if (statsPanel != null)
                {
                    int newCardW = (statsPanel.Width - 60) / 4;
                    if (newCardW > 0)
                    {
                        card1.Width = newCardW;
                        card2.Width = newCardW;
                        card3.Width = newCardW;
                        card4.Width = newCardW;

                        card2.Location = new Point(newCardW + 20, 0);
                        card3.Location = new Point((newCardW + 20) * 2, 0);
                        card4.Location = new Point((newCardW + 20) * 3, 0);
                    }
                }
            };
        }

        private Panel CreateStatCard(string title, string value, Color color, int width, int height)
        {
            Panel card = new Panel { Size = new Size(width, height), BackColor = C_CardBg, BorderStyle = BorderStyle.None };
            SetRoundedRegion(card, 10);
            AddDropShadow(card);
            Panel accentBar = new Panel { Location = new Point(0, 0), Size = new Size(5, height), BackColor = color };
            card.Controls.Add(accentBar);
            Label titleLbl = new Label { Text = title, Font = new Font("Segoe UI", 9), ForeColor = C_TextMid, Location = new Point(15, 15), AutoSize = true };
            card.Controls.Add(titleLbl);
            Label valueLbl = new Label { Text = value, Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = color, Location = new Point(15, 40), AutoSize = true };
            card.Controls.Add(valueLbl);
            return card;
        }

        // =========================================================
        // LOAD INVENTORY FROM DATABASE
        // =========================================================
        private async void LoadInventoryFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading inventory data...", Color.FromArgb(59, 130, 246));

                using (var conn = DatabaseHelper.GetConnection())
                {
                    await conn.OpenAsync();

                    string checkTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BloodBags'";
                    using (var checkCmd = new SqlCommand(checkTable, conn))
                    {
                        int tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (tableExists == 0)
                        {
                            dgvInventory.DataSource = null;
                            UpdateStatus("⚠️ Table 'BloodBags' not found", Color.FromArgb(245, 158, 11));
                            return;
                        }
                    }

                    string query = @"
                        SELECT 
                            BloodGroup as [Blood Group],
                            ComponentType as [Component Type],
                            COUNT(*) as [Quantity],
                            SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) as [Available],
                            SUM(CASE WHEN ExpiryDate < GETDATE() THEN 1 ELSE 0 END) as [Expired],
                            MIN(DATEDIFF(DAY, GETDATE(), ExpiryDate)) as [Min Days Left],
                            ISNULL(StorageLocation, 'Main Storage') as [Location]
                        FROM BloodBags 
                        WHERE Status NOT IN ('Discarded')
                        GROUP BY BloodGroup, ComponentType, StorageLocation
                        ORDER BY BloodGroup, ComponentType";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    originalData = new DataTable();
                    da.Fill(originalData);

                    dgvInventory.DataSource = originalData;
                    FormatInventoryGrid();
                    NoSort(dgvInventory);

                    UpdateStatistics();
                    UpdateStatus($"✅ Loaded {originalData.Rows.Count} inventory item(s)", Color.FromArgb(16, 185, 129));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadInventoryFromDatabase Error: {ex.Message}");
                dgvInventory.DataSource = null;
                UpdateStatus($"❌ Database error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatInventoryGrid()
        {
            if (dgvInventory.Columns.Count == 0) return;

            if (dgvInventory.Columns.Contains("Blood Group"))
                dgvInventory.Columns["Blood Group"].FillWeight = 12;
            if (dgvInventory.Columns.Contains("Component Type"))
                dgvInventory.Columns["Component Type"].FillWeight = 15;
            if (dgvInventory.Columns.Contains("Quantity"))
                dgvInventory.Columns["Quantity"].FillWeight = 10;
            if (dgvInventory.Columns.Contains("Available"))
                dgvInventory.Columns["Available"].FillWeight = 10;
            if (dgvInventory.Columns.Contains("Expired"))
                dgvInventory.Columns["Expired"].FillWeight = 10;
            if (dgvInventory.Columns.Contains("Min Days Left"))
                dgvInventory.Columns["Min Days Left"].FillWeight = 12;
            if (dgvInventory.Columns.Contains("Location"))
                dgvInventory.Columns["Location"].FillWeight = 15;
        }

        private void SearchInventory()
        {
            if (originalData == null) return;

            string bagID = txtSearchBagID.Text.Trim().ToLower();
            string donor = txtSearchDonor.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(bagID) && string.IsNullOrEmpty(donor))
            {
                dgvInventory.DataSource = originalData;
                UpdateStatus($"Showing all {originalData.Rows.Count} items", Color.FromArgb(59, 130, 246));
                return;
            }

            // Search requires scanning through all data (limited functionality in aggregated view)
            UpdateStatus("🔍 Detailed search available in Bag Tracking", Color.FromArgb(245, 158, 11));
            MessageBox.Show("For detailed bag search, please use the 'Bag Tracking' module.",
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateStatistics()
        {
            if (originalData == null) return;

            int totalUnits = 0;
            int availableBags = 0;
            int expiringBags = 0;
            int lowStockCount = 0;

            foreach (DataRow row in originalData.Rows)
            {
                int quantity = Convert.ToInt32(row["Quantity"]);
                int available = Convert.ToInt32(row["Available"]);
                int minDaysLeft = row["Min Days Left"] != DBNull.Value ? Convert.ToInt32(row["Min Days Left"]) : 999;

                totalUnits += quantity;
                availableBags += available;

                if (minDaysLeft <= 7 && minDaysLeft > 0)
                    expiringBags++;

                if (quantity < 5)
                    lowStockCount++;
            }

            if (lblTotalUnits != null) lblTotalUnits.Text = totalUnits.ToString();
            if (lblAvailableBags != null) lblAvailableBags.Text = availableBags.ToString();
            if (lblExpiringBags != null) lblExpiringBags.Text = expiringBags.ToString();
            if (lblLowStock != null) lblLowStock.Text = lowStockCount.ToString();
        }

        private void ApplyFilter()
        {
            if (originalData == null) return;

            string bloodGroup = cmbBloodGroup.SelectedItem?.ToString();
            string componentType = cmbComponentType.SelectedItem?.ToString();
            string statusFilter = cmbStatus.SelectedItem?.ToString();

            DataTable filteredDt = originalData.Clone();

            foreach (DataRow row in originalData.Rows)
            {
                string bg = row["Blood Group"].ToString();
                string comp = row["Component Type"].ToString();
                int available = Convert.ToInt32(row["Available"]);
                int expired = Convert.ToInt32(row["Expired"]);

                bool include = true;

                if (bloodGroup != "All" && bg != bloodGroup) include = false;
                if (include && componentType != "All" && comp != componentType) include = false;

                if (include && statusFilter != "All")
                {
                    if (statusFilter == "Available" && available == 0) include = false;
                    else if (statusFilter == "Low Stock" && available >= 5) include = false;
                    else if (statusFilter == "Expired" && expired == 0) include = false;
                }

                if (include)
                    filteredDt.ImportRow(row);
            }

            dgvInventory.DataSource = filteredDt;
            UpdateStatus($"🔍 Showing {filteredDt.Rows.Count} of {originalData.Rows.Count} items", Color.FromArgb(59, 130, 246));
        }

        private void DgvInventory_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvInventory.Rows[e.RowIndex].Cells["Quantity"].Value != null)
            {
                int quantity = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["Quantity"].Value);
                if (quantity < 5)
                    dgvInventory.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (quantity < 10)
                    dgvInventory.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
                else
                    dgvInventory.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 220);
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

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls";
                sfd.DefaultExt = "csv";
                sfd.FileName = $"Inventory_Status_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    DataTable exportData = dgvInventory.DataSource as DataTable;
                    if (exportData == null || exportData.Rows.Count == 0)
                    {
                        MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (sfd.FileName.EndsWith(".csv"))
                    {
                        ExportToCSV(exportData, sfd.FileName);
                    }
                    else
                    {
                        ExportToExcelHtml(exportData, sfd.FileName);
                    }

                    MessageBox.Show($"Inventory exported successfully to:\n{sfd.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AuditHelper.Log("Export Inventory", "Report", $"Exported inventory data to {sfd.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(DataTable dt, string filePath)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1) sw.Write(",");
                }
                sw.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string value = row[i].ToString().Replace(",", ";");
                        sw.Write(value);
                        if (i < dt.Columns.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();
                }
            }
        }

        private void ExportToExcelHtml(DataTable dt, string filePath)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html><head><meta charset='UTF-8'></head><body>");
            html.AppendLine("<h2>Blood Bank - Inventory Status Report</h2>");
            html.AppendLine($"<p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}</p>");
            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse:collapse;'>");

            html.AppendLine("<tr style='background-color:#78161b; color:white;'>");
            foreach (DataColumn col in dt.Columns)
            {
                html.AppendLine($"<th>{col.ColumnName}</th>");
            }
            html.AppendLine("</table>");

            foreach (DataRow row in dt.Rows)
            {
                html.AppendLine("<tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    int quantity = col.ColumnName == "Quantity" && row[col] != DBNull.Value ? Convert.ToInt32(row[col]) : 0;
                    if (quantity < 5)
                        html.AppendLine($"<td style='background-color:#ffdddd;'>{row[col]}</td>");
                    else if (quantity < 10)
                        html.AppendLine($"<td style='background-color:#ffffcc;'>{row[col]}</td>");
                    else
                        html.AppendLine($"<td>{row[col]}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body></html>");

            System.IO.File.WriteAllText(filePath, html.ToString());
        }

        private void FixDataGridView(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AutoResizeColumns();
            dgv.ScrollBars = ScrollBars.Both;
            dgv.RowTemplate.Height = 42;
            dgv.ColumnHeadersHeight = 45;
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private DataGridView CreateStyledGrid()
        {
            var dgv = new DataGridView
            {
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = C_White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = C_Border,
                RowTemplate = { Height = 42 },
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = brickRed;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 45;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.SelectionBackColor = brickRedLight;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);
            return dgv;
        }

        private void NoSort(DataGridView dgv)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void WrapInCard(DataGridView dgv, Panel parent, Point location, int dgvHeight, string footerText = null)
        {
            int pad = 20, btnH = footerText != null ? 46 : 0, cardH = dgvHeight + pad * 2 + btnH;
            var card = new Panel { Location = location, Width = parent.ClientSize.Width - 60, Height = cardH, BackColor = C_CardBg, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            SetRoundedRegion(card, 12);
            AddDropShadow(card);
            dgv.Location = new Point(pad, pad);
            dgv.Width = card.Width - pad * 2;
            dgv.Height = dgvHeight;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(dgv);
            if (footerText != null)
            {
                var btn = new Button { Text = footerText, Location = new Point(pad, dgvHeight + pad + 6), Size = new Size(180, 34), FlatStyle = FlatStyle.Flat, BackColor = brickRed, ForeColor = C_White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                SetRoundedRegion(btn, 6);
                btn.Click += (s, e) => MessageBox.Show(footerText + " (to be implemented)");
                card.Controls.Add(btn);
            }
            parent.Controls.Add(card);
        }

        private void SetRoundedRegion(Control ctrl, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(ctrl.Width - d, 0, d, d, 270, 90);
                path.AddArc(ctrl.Width - d, ctrl.Height - d, d, d, 0, 90);
                path.AddArc(0, ctrl.Height - d, d, d, 90, 90);
                path.CloseFigure();
                ctrl.Region = new Region(path);
            }
        }

        private void AddDropShadow(Panel card)
        {
            card.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0), 3)) e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3); };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}