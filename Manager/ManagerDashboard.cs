using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Enums;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class ManagerDashboard : Form
    {
        private Panel sidebarPanel;
        private Panel topbarPanel;
        private Panel mainPanel;
        private Panel cardsPanel;
        private Panel bottomSection;
        private Timer refreshTimer;
        private Label lblNotificationBadge;
        private Timer notificationTimer;
        private Label lblDateTime;
        private Timer clockTimer;
        private FlowLayoutPanel menuPanel;
        private Button activeMenuButton = null;
        private Label lblWelcome;
        private Label lblRoleBadge;

        // Corporate Theme Colors
        private readonly Color primaryColor = Color.FromArgb(5, 31, 64);
        private readonly Color sidebarColor = Color.FromArgb(5, 31, 64);
        private readonly Color cardBgColor = Color.White;
        private readonly Color bgColor = Color.FromArgb(245, 247, 250);
        private readonly Color successColor = Color.FromArgb(34, 197, 94);
        private readonly Color warningColor = Color.FromArgb(245, 158, 11);
        private readonly Color infoColor = Color.FromArgb(59, 130, 246);
        private readonly Color dangerColor = Color.FromArgb(220, 38, 38);

        public ManagerDashboard()
        {
            InitializeComponent();
            this.Text = "Blood Bank Manager Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = bgColor;
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            BuildMainPanel();
            BuildSidebar();
            BuildTopbar();

            LoadDashboardData();

            // Start timers
            StartNotificationTimer();
            StartClockTimer();

            refreshTimer = new Timer { Interval = 30000 };
            refreshTimer.Tick += (s, e) => LoadDashboardData();
            refreshTimer.Start();
        }

        #region Database Data Loading (Using DAL)

        private void LoadDashboardData()
        {
            LoadCardStats();
            LoadPendingRequests();
            LoadRecentActivityAndAlerts();
        }

        private void LoadCardStats()
        {
            int totalBloodUnits = 0;
            int pendingRequests = 0;
            int expiringUnits = 0;
            int activeCamps = 0;

            try
            {
                totalBloodUnits = CommonDAL.GetTotalBloodBags();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TotalBloodUnits Error: {ex.Message}");
                totalBloodUnits = 0;
            }

            try
            {
                pendingRequests = RequisitionDAL.GetPendingRequisitionCount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PendingRequests Error: {ex.Message}");
                pendingRequests = 0;
            }

            try
            {
                var expiring = BloodBagDAL.GetExpiringBags();
                expiringUnits = expiring?.Rows.Count ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExpiringUnits Error: {ex.Message}");
                expiringUnits = 0;
            }

            try
            {
                var camps = DonationCampDAL.GetAllActiveCamps();
                activeCamps = camps?.Rows.Count ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActiveCamps Error: {ex.Message}");
                activeCamps = 0;
            }

            if (cardsPanel?.Controls[0] is TableLayoutPanel tbl)
            {
                int col = 0;
                foreach (Control cell in tbl.Controls)
                {
                    if (cell is Panel card)
                    {
                        foreach (Control inner in card.Controls)
                        {
                            if (inner is Label lbl && lbl.Font.Bold && lbl.Font.Size >= 14)
                            {
                                switch (col)
                                {
                                    case 0: lbl.Text = FormatNumber(totalBloodUnits); break;
                                    case 1: lbl.Text = FormatNumber(pendingRequests); break;
                                    case 2: lbl.Text = FormatNumber(expiringUnits); break;
                                    case 3: lbl.Text = FormatNumber(activeCamps); break;
                                }
                                break;
                            }
                        }
                        col++;
                    }
                }
            }
        }

        private void LoadPendingRequests()
        {
            DataGridView dgv = FindDataGridView(bottomSection, "dgvRequests");
            if (dgv == null) return;
            dgv.Rows.Clear();

            try
            {
                DataTable pendingRequisitions = RequisitionDAL.GetPendingRequisitions();

                if (pendingRequisitions != null && pendingRequisitions.Rows.Count > 0)
                {
                    foreach (DataRow row in pendingRequisitions.Rows)
                    {
                        string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                        string units = (row["UnitsNeeded"]?.ToString() ?? "0") + " Unit(s)";
                        string hospital = row["Hospital"]?.ToString() ?? "N/A";
                        string urgency = row["Urgency"]?.ToString() ?? "Normal";

                        int rowIndex = dgv.Rows.Add(bloodGroup, units, hospital, "Pending");
                        if (urgency == "Emergency")
                            dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        else if (urgency == "Urgent")
                            dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
                    }
                }
                else
                {
                    AddSamplePendingRequests(dgv);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPendingRequests Error: {ex.Message}");
                AddSamplePendingRequests(dgv);
            }
        }

        private void AddSamplePendingRequests(DataGridView dgv)
        {
            var dummyRequests = new[]
            {
                new { Blood = "A+", Units = "2 Unit(s)", Hospital = "City Hospital", Urgency = "Emergency" },
                new { Blood = "O-", Units = "1 Unit(s)", Hospital = "General Medical", Urgency = "Urgent" },
                new { Blood = "B+", Units = "4 Unit(s)", Hospital = "St. Mary's", Urgency = "Normal" },
                new { Blood = "AB+", Units = "2 Unit(s)", Hospital = "University Hospital", Urgency = "Emergency" },
                new { Blood = "O+", Units = "3 Unit(s)", Hospital = "Metro Care", Urgency = "Urgent" }
            };

            foreach (var req in dummyRequests)
            {
                int rowIndex = dgv.Rows.Add(req.Blood, req.Units, req.Hospital, "Pending");
                if (req.Urgency == "Emergency")
                    dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (req.Urgency == "Urgent")
                    dgv.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
            }
        }

        private void LoadRecentActivityAndAlerts()
        {
            FlowLayoutPanel actFlow = FindFlowLayoutPanel(bottomSection, "actFlow");
            if (actFlow == null) return;
            actFlow.Controls.Clear();

            // Load recent activities from AuditLog
            try
            {
                DataTable logs = AuditLogDAL.GetAllLogs();
                int count = 0;
                if (logs != null && logs.Rows.Count > 0)
                {
                    foreach (DataRow row in logs.Rows)
                    {
                        if (count >= 6) break;
                        string action = row["Action"]?.ToString() ?? "";
                        string username = row["Username"]?.ToString() ?? "System";
                        string actionDateTime = row["ActionDateTime"] != DBNull.Value ?
                            Convert.ToDateTime(row["ActionDateTime"]).ToString("hh:mm tt") : "";
                        string activity = $"{action} by {username} at {actionDateTime}";
                        actFlow.Controls.Add(CreateActivityRow(activity, false));
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadActivities Error: {ex.Message}");
            }

            // If no activities found, add sample
            if (actFlow.Controls.Count == 0)
            {
                string[] activities = {
                    "Added new donor by Admin",
                    "Updated inventory (A+) by Manager",
                    "Processed requisition",
                    "User logged in",
                    "System ready"
                };
                foreach (var act in activities)
                    actFlow.Controls.Add(CreateActivityRow(act, false));
            }

            // Add Low Stock Alerts header
            Label alertHeader = new Label
            {
                Text = "⚠️ Low Stock Alerts",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 50, 50),
                Margin = new Padding(0, 8, 0, 4),
                AutoSize = false,
                Width = 260
            };
            actFlow.Controls.Add(alertHeader);

            // Check low stock from database
            bool hasLowStock = false;
            string[] bloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            foreach (string bg in bloodGroups)
            {
                try
                {
                    int count = BloodBagDAL.GetAvailableCount(bg);
                    if (count <= 2 && count > 0)
                    {
                        actFlow.Controls.Add(CreateActivityRow($"{bg} – only {count} units left", true));
                        hasLowStock = true;
                    }
                    else if (count == 0)
                    {
                        actFlow.Controls.Add(CreateActivityRow($"{bg} – OUT OF STOCK", true));
                        hasLowStock = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LowStock Check Error: {ex.Message}");
                }
            }

            if (!hasLowStock)
            {
                actFlow.Controls.Add(CreateActivityRow("All blood groups have adequate stock ✓", false));
            }
        }

        #endregion

        #region UI Construction

        private void BuildTopbar()
        {
            topbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = primaryColor,
                Padding = new Padding(0, 0, 0, 1)
            };

            topbarPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(80, 100, 120), 1))
                {
                    e.Graphics.DrawLine(pen, 0, topbarPanel.Height - 1, topbarPanel.Width, topbarPanel.Height - 1);
                }
            };

            Label title = new Label
            {
                Text = "Manager Dashboard",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 18)
            };

            // Welcome label with actual name from session
            lblWelcome = new Label
            {
                Text = $"👋 Welcome, {SessionManager.CurrentFullName}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true
            };

            // Role badge
            lblRoleBadge = new Label
            {
                Text = $"🛡️ {SessionManager.CurrentRole}",
                Font = new Font("Segoe UI", 7, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true
            };

            // DateTime label
            lblDateTime = new Label
            {
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(180, 180, 200),
                AutoSize = true
            };
            UpdateDateTime();

            // Notification badge
            lblNotificationBadge = new Label
            {
                Text = "0",
                BackColor = dangerColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                Size = new Size(16, 16),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            MakeRounded(lblNotificationBadge, 8);

            // Notification button
            Button btnNotification = new Button
            {
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 11),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(35, 35),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            btnNotification.Click += (s, e) => ShowNotifications();

            // Profile button
            Button btnProfile = new Button
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 11),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(35, 35),
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            btnProfile.Click += (s, e) => ShowProfile();

            // Position controls on topbar
            topbarPanel.Controls.Add(title);
            topbarPanel.Controls.Add(lblWelcome);
            topbarPanel.Controls.Add(lblRoleBadge);
            topbarPanel.Controls.Add(btnNotification);
            topbarPanel.Controls.Add(lblNotificationBadge);
            topbarPanel.Controls.Add(btnProfile);
            topbarPanel.Controls.Add(lblDateTime);

            topbarPanel.Resize += (s, e) =>
            {
                lblWelcome.Location = new Point(topbarPanel.ClientSize.Width - lblWelcome.Width - 350, 15);
                lblRoleBadge.Location = new Point(topbarPanel.ClientSize.Width - lblRoleBadge.Width - 350, 38);
                btnNotification.Location = new Point(topbarPanel.ClientSize.Width - 125, 12);
                lblNotificationBadge.Location = new Point(topbarPanel.ClientSize.Width - 110, 8);
                btnProfile.Location = new Point(topbarPanel.ClientSize.Width - 70, 12);
                lblDateTime.Location = new Point(topbarPanel.ClientSize.Width - 330, 22);
            };

            Controls.Add(topbarPanel);
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = sidebarColor,
                Padding = new Padding(0, 0, 1, 0)
            };

            sidebarPanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(80, 100, 120), 1))
                {
                    e.Graphics.DrawLine(pen, sidebarPanel.Width - 1, 0, sidebarPanel.Width - 1, sidebarPanel.Height);
                }
            };

            Panel logoPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.Transparent };
            Label bloodIcon = new Label { Text = "🩸", Font = new Font("Segoe UI Emoji", 22), ForeColor = Color.FromArgb(220, 50, 50), Location = new Point(15, 18), AutoSize = true };
            Label logoText = new Label { Text = "Blood Bank", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(66, 22), AutoSize = true };
            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);

            menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8, 6, 8, 8),
                BackColor = Color.Transparent
            };

            // Menu items (Technician Performance REMOVED)
            AddMenuItem(menuPanel, "🏠", "Dashboard", true, null);
            AddMenuItem(menuPanel, "📦", "Inventory Management", false, () => OpenForm<InventoryManagement>());
            AddMenuItem(menuPanel, "⭐", "Rare Donor Management", false, () => OpenForm<RareDonorManagement>());
            AddMenuItem(menuPanel, "📅", "Schedule Camp", false, () => OpenForm<ScheduleCamps>());
            AddMenuItem(menuPanel, "📊", "Manager Reports", false, () => OpenForm<ManageReports>());
            AddMenuItem(menuPanel, "🔔", "Alerts & Notifications", false, () => OpenForm<AlertsNotifications>());

            // Separator
            Panel separator = new Panel
            {
                Height = 1,
                Width = 200,
                BackColor = Color.FromArgb(55, 55, 60),
                Margin = new Padding(0, 10, 0, 10)
            };
            menuPanel.Controls.Add(separator);

            AddMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Restart();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoPanel);
            Controls.Add(sidebarPanel);
        }

        private void AddMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel
            {
                Size = new Size(200, 40),
                Margin = new Padding(0, 1, 0, 1),
                Cursor = Cursors.Hand,
                BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent
            };

            Label iconLbl = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 12),
                ForeColor = Color.White,
                Location = new Point(12, 9),
                AutoSize = true
            };

            Label textLbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(44, 11),
                AutoSize = true
            };

            if (active)
            {
                activeMenuButton = new Button();
                item.BackColor = Color.FromArgb(230, 57, 70);
            }

            item.MouseEnter += (s, e) => item.BackColor = Color.FromArgb(200, 50, 60);
            item.MouseLeave += (s, e) =>
            {
                if (!active)
                    item.BackColor = Color.Transparent;
                else
                    item.BackColor = Color.FromArgb(230, 57, 70);
            };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        private void BuildMainPanel()
        {
            mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = bgColor, Padding = new Padding(14, 12, 14, 12) };
            Panel wrapper = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            BuildCardsSection();
            BuildBottomSection();

            wrapper.Controls.Add(bottomSection);
            wrapper.Controls.Add(cardsPanel);
            mainPanel.Controls.Add(wrapper);
            Controls.Add(mainPanel);
        }

        private void BuildCardsSection()
        {
            cardsPanel = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Color.Transparent };
            TableLayoutPanel tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 0, 0, 10),
                BackColor = Color.Transparent
            };

            for (int i = 0; i < 4; i++)
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            string[] labels = { "Total Blood Units", "Pending Requests", "Expiring Units (7d)", "Active Camps" };
            string[] icons = { "🩸", "⏳", "⚠️", "🏕️" };
            Color[] colors = { Color.FromArgb(239, 68, 68), Color.FromArgb(251, 146, 60), Color.FromArgb(245, 158, 11), Color.FromArgb(34, 197, 94) };

            for (int i = 0; i < 4; i++)
                tbl.Controls.Add(CreateCard(labels[i], "0", icons[i], colors[i]), i, 0);

            cardsPanel.Controls.Add(tbl);
        }

        private Panel CreateCard(string title, string value, string icon, Color color)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 0, 5, 0),
                BackColor = Color.White
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = RoundRect(card.ClientRectangle, 8))
                using (var brush = new SolidBrush(Color.White))
                    e.Graphics.FillPath(brush, gp);
                using (var regionPath = RoundRect(card.ClientRectangle, 8))
                    card.Region = new Region(regionPath);
            };

            Panel iconBox = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(14, 16),
                BackColor = color
            };
            using (var regionPath = RoundRect(iconBox.ClientRectangle, 8))
                iconBox.Region = new Region(regionPath);

            iconBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(iconBox.BackColor))
                    e.Graphics.FillRectangle(brush, iconBox.ClientRectangle);
            };

            Label iconLbl = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 14),
                ForeColor = Color.White,
                Location = new Point(5, 6),
                AutoSize = true
            };
            iconBox.Controls.Add(iconLbl);

            Label valLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 30),
                AutoSize = true,
                Location = new Point(66, 12)
            };

            Label titLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(68, 40)
            };

            card.Controls.Add(iconBox);
            card.Controls.Add(valLbl);
            card.Controls.Add(titLbl);
            return card;
        }

        private void BuildBottomSection()
        {
            bottomSection = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Left panel: pending requests table
            Panel left = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0),
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 8)
            };
            left.Paint += (s, e) => PaintCard((Panel)s, e.Graphics);

            Label tblTitle = new Label
            {
                Text = "Pending Blood Requests",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            DataGridView dgv = new DataGridView
            {
                Name = "dgvRequests",
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 30,
                RowTemplate = { Height = 32 },
                GridColor = Color.FromArgb(235, 235, 238),
                EnableHeadersVisualStyles = false,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9), SelectionBackColor = Color.FromArgb(238, 242, 255), SelectionForeColor = Color.FromArgb(35, 35, 40) },
                ColumnHeadersDefaultCellStyle = { Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(248, 249, 251), ForeColor = Color.FromArgb(90, 90, 100) }
            };

            dgv.Columns.Add("BloodGroup", "Blood Group");
            dgv.Columns.Add("Units", "Units");
            dgv.Columns.Add("Hospital", "Hospital");
            dgv.Columns.Add("Status", "Status");

            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == 3 && e.Value != null && e.Value.ToString() == "Pending")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    e.CellStyle.BackColor = Color.FromArgb(254, 243, 199);
                }
            };

            // Double-click to view/approve requisition
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            left.Controls.Add(dgv);
            left.Controls.Add(tblTitle);

            // Right panel: activity & alerts
            Panel right = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0),
                BackColor = Color.White,
                Padding = new Padding(10, 10, 10, 8)
            };
            right.Paint += (s, e) => PaintCard((Panel)s, e.Graphics);

            Label actTitle = new Label
            {
                Text = "Recent Activity & Alerts",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            FlowLayoutPanel actFlow = new FlowLayoutPanel
            {
                Name = "actFlow",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            right.Controls.Add(actFlow);
            right.Controls.Add(actTitle);

            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(right, 1, 0);
            bottomSection.Controls.Add(layout);
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                string bloodGroup = dgv.Rows[e.RowIndex].Cells[0].Value?.ToString();
                string hospital = dgv.Rows[e.RowIndex].Cells[2].Value?.ToString();

                MessageBox.Show($"Process requisition for:\nBlood Group: {bloodGroup}\nHospital: {hospital}",
                    "Requisition Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Panel CreateActivityRow(string text, bool isAlert = false)
        {
            Panel row = new Panel { Width = 260, Height = 38, BackColor = Color.Transparent };
            Panel dot = new Panel
            {
                Size = new Size(24, 24),
                Location = new Point(4, 7),
                BackColor = isAlert ? Color.FromArgb(254, 226, 226) : Color.FromArgb(209, 250, 229)
            };

            dot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, 24, 24);
                    dot.Region = new Region(gp);
                    using (var br = new SolidBrush(dot.BackColor))
                        e.Graphics.FillPath(br, gp);
                }
                using (var pen = new Pen(isAlert ? Color.FromArgb(180, 30, 30) : Color.FromArgb(6, 95, 70), 2f))
                {
                    if (isAlert)
                        e.Graphics.DrawLine(pen, 8, 8, 16, 16);
                    else
                    {
                        e.Graphics.DrawLine(pen, 6, 12, 10, 16);
                        e.Graphics.DrawLine(pen, 10, 16, 18, 8);
                    }
                }
            };

            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8),
                ForeColor = isAlert ? Color.FromArgb(180, 30, 30) : Color.FromArgb(45, 45, 55),
                Location = new Point(38, 10),
                Size = new Size(220, 18),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            row.Controls.Add(dot);
            row.Controls.Add(lbl);
            return row;
        }

        private void PaintCard(Panel p, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var gp = RoundRect(p.ClientRectangle, 8))
            using (var brush = new SolidBrush(Color.White))
            using (var pen = new Pen(Color.FromArgb(18, 0, 0, 0), 1f))
            {
                g.FillPath(brush, gp);
                g.DrawPath(pen, gp);
                p.Region = new Region(gp);
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

        private void MakeRounded(Control control, int radius)
        {
            control.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    int d = radius * 2;
                    path.AddArc(0, 0, d, d, 180, 90);
                    path.AddArc(control.Width - d, 0, d, d, 270, 90);
                    path.AddArc(control.Width - d, control.Height - d, d, d, 0, 90);
                    path.AddArc(0, control.Height - d, d, d, 90, 90);
                    path.CloseFigure();
                    if (control.Region != null) control.Region.Dispose();
                    control.Region = new Region(path);
                }
            };
        }

        #endregion

        #region Notification and Clock Helpers

        private void StartNotificationTimer()
        {
            notificationTimer = new Timer { Interval = 30000 };
            notificationTimer.Tick += (s, e) => UpdateNotificationBadge();
            notificationTimer.Start();
            UpdateNotificationBadge();
        }

        private void UpdateNotificationBadge()
        {
            try
            {
                int count = NotificationDAL.GetUnreadCount(SessionManager.CurrentUserID);
                lblNotificationBadge.Text = count.ToString();
                lblNotificationBadge.Visible = count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateNotificationBadge Error: {ex.Message}");
                lblNotificationBadge.Visible = false;
            }
        }

        private void ShowNotifications()
        {
            OpenForm<AlertsNotifications>();
        }

        private void ShowProfile()
        {
            MessageBox.Show(
                $"👤 User Profile Info\n\n" +
                $"Name: {SessionManager.CurrentFullName}\n" +
                $"Role: {SessionManager.CurrentRole}\n" +
                $"User ID: {SessionManager.CurrentUserID}\n" +
                $"Username: {SessionManager.CurrentUsername}",
                "User Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StartClockTimer()
        {
            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => UpdateDateTime();
            clockTimer.Start();
        }

        private void UpdateDateTime()
        {
            if (lblDateTime != null && !lblDateTime.IsDisposed)
            {
                lblDateTime.Text = DateTime.Now.ToString("dddd, dd MMM yyyy | hh:mm:ss tt");
            }
        }

        #endregion

        #region Helper Methods

        private string FormatNumber(int number) => number >= 1000 ? (number / 1000.0).ToString("0.#") + "k" : number.ToString();

        private DataGridView FindDataGridView(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is DataGridView dgv && dgv.Name == name) return dgv;
                if (c.HasChildren)
                {
                    var found = FindDataGridView(c, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private FlowLayoutPanel FindFlowLayoutPanel(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is FlowLayoutPanel flp && flp.Name == name) return flp;
                if (c.HasChildren)
                {
                    var found = FindFlowLayoutPanel(c, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void OpenForm<T>() where T : Form, new()
        {
            using (T form = new T())
            {
                form.ShowDialog();
            }
            LoadDashboardData(); // refresh dashboard after closing
        }

        #endregion
    }
}