using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Technician
{
    public partial class BaseTechnicianForm : Form
    {
        protected Panel pnlSidebar, pnlMain, pnlTopBar, pnlContent;
        protected Button _activeNavBtn = null;
        protected FlowLayoutPanel navPanel;
        protected Button btnLogoutSidebar;
        protected Label lblWelcome;
        protected Timer sessionTimer;

        // Colors
        protected static readonly Color C_SidebarBg1 = Color.FromArgb(22, 33, 62);
        protected static readonly Color C_SidebarBg2 = Color.FromArgb(15, 23, 42);
        protected static readonly Color brickRed = Color.FromArgb(120, 22, 27);
        protected static readonly Color brickRedLight = Color.FromArgb(254, 242, 242);
        protected static readonly Color C_White = Color.White;
        protected static readonly Color C_ContentBg = Color.FromArgb(245, 247, 250);
        protected static readonly Color C_CardBg = Color.White;
        protected static readonly Color C_TextDark = Color.FromArgb(31, 41, 55);
        protected static readonly Color C_TextMid = Color.FromArgb(107, 114, 128);
        protected static readonly Color C_Border = Color.FromArgb(229, 231, 235);
        protected static readonly Color C_Success = Color.FromArgb(16, 185, 129);
        protected static readonly Color C_Warning = Color.FromArgb(245, 158, 11);
        protected static readonly Color C_Error = Color.FromArgb(220, 38, 38);

        protected const int SIDEBAR_W = 280;
        protected const int TOPBAR_H = 80;

        // Session tracking
        private DateTime lastActivityTime;
        private const int SESSION_TIMEOUT_MINUTES = 30;

        public BaseTechnicianForm()
        {
            this.BackColor = C_ContentBg;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1150, 700);
            this.Font = new Font("Segoe UI", 9.25f);

            // Track user activity
            this.MouseMove += (s, e) => ResetActivityTimer();
            this.KeyPress += (s, e) => ResetActivityTimer();

            // Start session timer
            StartSessionTimer();
        }

        protected void BuildLayout()
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

        protected void BuildSidebar(string activePage)
        {
            // Logo Container
            Panel logoContainer = new Panel { Size = new Size(220, 130), Location = new Point(30, 20), BackColor = C_White };
            SetRoundedRegion(logoContainer, 12);
            pnlSidebar.Controls.Add(logoContainer);

            TableLayoutPanel logoTable = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(5) };
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23));
            logoContainer.Controls.Add(logoTable);

            // Blood Bank Logo
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

            // Separator
            pnlSidebar.Controls.Add(new Panel { Size = new Size(220, 1), BackColor = brickRed, Location = new Point(30, 165) });
            pnlSidebar.Controls.Add(new Label { Text = "MAIN MENU", Location = new Point(45, 185), AutoSize = true, ForeColor = Color.FromArgb(150, 170, 190), Font = new Font("Segoe UI", 8f, FontStyle.Bold) });

            // Navigation Container
            Panel navContainer = new Panel { Location = new Point(15, 215), Size = new Size(250, pnlSidebar.Height - 280), BackColor = Color.Transparent, AutoScroll = true };
            pnlSidebar.Controls.Add(navContainer);

            navPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };
            navContainer.Controls.Add(navPanel);

            // Menu Items
            (string label, string icon, Type formType)[] items = {
                ("Dashboard", "🏠", typeof(TechnicianDashboard)),
                ("Assign Storage", "📦", typeof(AssignStorageLocation)),
                ("Medical Screening", "🩺", typeof(DonorMedicalScreening)),
                ("Receive Bag", "📥", typeof(ReceiveBag)),
                ("Component Prep", "⚙️", typeof(ComponentPreparation)),
                ("Blood Collection", "🩸", typeof(BloodCollectionDetails)),
                ("Defer Donor", "🚫", typeof(DeferDonor)),
                ("Discard Blood", "🗑️", typeof(DiscardBlood)),
                ("Print Labels", "🖨️", typeof(PrintBarcodeLabels)),
                ("Quality Control", "🔬", typeof(QualityControl)),
                ("TTI Screening", "🧪", typeof(TTIScreeningResults)),
                ("Cross Matching", "🔄", typeof(TechnicianCrossMatching)),
                ("Bag Tracking", "📊", typeof(BloodBagTracking)),
                ("Expiry Alerts", "⚠️", typeof(ExpiryAlerts)),
                ("Inventory", "📋", typeof(RecentInventoryStatus))
            };

            foreach (var item in items)
            {
                Button btn = new Button
                {
                    Text = $"   {item.icon}   {item.label}",
                    Size = new Size(220, 42),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    ForeColor = C_White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9.5f),
                    Cursor = Cursors.Hand,
                    Padding = new Padding(12, 0, 0, 0),
                    Margin = new Padding(0, 0, 0, 2),
                    Tag = item.formType
                };

                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 65, 85);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

                if (item.label == activePage)
                {
                    btn.BackColor = Color.FromArgb(50, 65, 85);
                    _activeNavBtn = btn;
                }

                btn.Click += (s, e) =>
                {
                    SetActiveNav(btn);
                    if (btn.Tag is Type formType)
                    {
                        NavigateToForm(formType);
                    }
                };

                navPanel.Controls.Add(btn);
            }

            Panel spacer = new Panel { Height = 20, BackColor = Color.Transparent };
            navPanel.Controls.Add(spacer);

            // Logout Button
            btnLogoutSidebar = new Button
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
            btnLogoutSidebar.Click += (s, e) => Logout();
            navPanel.Controls.Add(btnLogoutSidebar);

            navContainer.Height = pnlSidebar.Height - 280;
            pnlSidebar.Resize += (s, e) => { navContainer.Height = pnlSidebar.Height - 280; };
        }

        protected void SetActiveNav(Button btn)
        {
            if (_activeNavBtn != null)
            {
                _activeNavBtn.BackColor = Color.Transparent;
                _activeNavBtn.ForeColor = C_White;
            }
            _activeNavBtn = btn;
            btn.BackColor = Color.FromArgb(50, 65, 85);
            btn.ForeColor = C_White;
        }

        protected void NavigateToForm(Type formType)
        {
            try
            {
                // Check if form is already open
                foreach (Form form in Application.OpenForms)
                {
                    if (form.GetType() == formType && form != this)
                    {
                        form.BringToFront();
                        form.WindowState = FormWindowState.Normal;
                        this.Hide();
                        return;
                    }
                }

                Form newForm = (Form)Activator.CreateInstance(formType);
                newForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening form: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void BuildTopBar(string pageTitle)
        {
            // Page Title
            Label lblPage = new Label
            {
                Text = pageTitle,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = C_TextDark,
                AutoSize = true,
                Location = new Point(30, 22)
            };
            pnlTopBar.Controls.Add(lblPage);

            // Welcome Label
            string welcomeText = $"Welcome, {SessionManager.CurrentFullName}";
            lblWelcome = new Label
            {
                Text = welcomeText,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = true,
                Location = new Point(30, 55)
            };
            pnlTopBar.Controls.Add(lblWelcome);

            // Date Label
            Label lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = C_TextMid,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlTopBar.Width - 350, 25)
            };
            pnlTopBar.Controls.Add(lblDate);

            // Time Label
            Label lblTime = new Label
            {
                Text = DateTime.Now.ToString("hh:mm:ss tt"),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlTopBar.Width - 350, 50)
            };
            pnlTopBar.Controls.Add(lblTime);

            // Timer for clock (SYNC - no await needed)
            Timer clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) =>
            {
                lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
                lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            };
            clockTimer.Start();

            // Notification Button
            Button btnNotif = new Button
            {
                Text = "🔔",
                Font = new Font("Segoe UI Emoji", 14f),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRedLight,
                ForeColor = brickRed,
                Size = new Size(38, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnNotif.FlatAppearance.BorderSize = 0;
            btnNotif.Location = new Point(pnlTopBar.Width - 160, 20);
            SetRoundedRegion(btnNotif, 8);
            btnNotif.Click += (s, e) => ShowNotifications();
            pnlTopBar.Controls.Add(btnNotif);

            // Notification Badge
            Label notifBadge = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = C_White,
                BackColor = C_Error,
                Size = new Size(18, 18),
                Location = new Point(pnlTopBar.Width - 140, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            SetRoundedRegion(notifBadge, 9);
            pnlTopBar.Controls.Add(notifBadge);

            // Load notification count (SYNC version)
            LoadNotificationCountSync(notifBadge);

            // Settings Button
            Button btnSettings = new Button
            {
                Text = "⚙️",
                Font = new Font("Segoe UI Emoji", 14f),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRedLight,
                ForeColor = brickRed,
                Size = new Size(38, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Location = new Point(pnlTopBar.Width - 110, 20);
            SetRoundedRegion(btnSettings, 8);
            btnSettings.Click += (s, e) => ShowSettingsMenu();
            pnlTopBar.Controls.Add(btnSettings);

            // Resize event
            pnlTopBar.Resize += (s, e) =>
            {
                lblDate.Location = new Point(pnlTopBar.Width - 350, 25);
                lblTime.Location = new Point(pnlTopBar.Width - 350, 50);
                btnNotif.Location = new Point(pnlTopBar.Width - 160, 20);
                notifBadge.Location = new Point(pnlTopBar.Width - 140, 15);
                btnSettings.Location = new Point(pnlTopBar.Width - 110, 20);
            };
        }

        // SYNC version - no async
        private void LoadNotificationCountSync(Label badge)
        {
            try
            {
                int count = NotificationDAL.GetUnreadCount(SessionManager.CurrentUserID);
                badge.Text = count.ToString();
                badge.Visible = count > 0;

                // Change button color if has notifications
                if (count > 0)
                {
                    foreach (Control ctrl in pnlTopBar.Controls)
                    {
                        if (ctrl is Button btn && btn.Text == "🔔")
                        {
                            btn.BackColor = brickRed;
                            btn.ForeColor = C_White;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadNotificationCountSync Error: {ex.Message}");
            }
        }

        private void ShowNotifications()
        {
            try
            {
                var notifications = NotificationDAL.GetByUser(SessionManager.CurrentUserID);
                MessageBox.Show($"You have {notifications.Rows.Count} notifications", "Notifications",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowNotifications Error: {ex.Message}");
            }
        }

        private void ShowSettingsMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Change Password", null, (s, e) => ChangePassword());
            menu.Items.Add("My Profile", null, ShowProfile);
            menu.Items.Add("-");
            menu.Items.Add("Database Status", null, (s, e) => ShowDatabaseStatus());
            menu.Items.Add("-");
            menu.Items.Add("About", null, ShowAbout);
            menu.Show(Cursor.Position);
        }

        private void ChangePassword()
        {
            MessageBox.Show("Change Password feature will be implemented.", "Coming Soon",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowProfile(object sender, EventArgs e)
        {
            MessageBox.Show("Profile feature will be implemented.", "Coming Soon",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            MessageBox.Show("Blood Bank Management System\nVersion 2.0\n\n© 2024 All Rights Reserved",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected Panel WrapInCard(DataGridView dgv, Panel parent, Point location, int dgvHeight, string footerText = null)
        {
            int pad = 20;
            int btnH = footerText != null ? 46 : 0;
            int cardH = dgvHeight + pad * 2 + btnH;

            var card = new Panel
            {
                Location = location,
                Width = parent.ClientSize.Width - 60,
                Height = cardH,
                BackColor = C_CardBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            SetRoundedRegion(card, 12);
            AddDropShadow(card);

            dgv.Location = new Point(pad, pad);
            dgv.Width = card.Width - pad * 2;
            dgv.Height = dgvHeight;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(dgv);

            if (footerText != null)
            {
                Button btn = new Button
                {
                    Text = footerText,
                    Location = new Point(pad, dgvHeight + pad + 6),
                    Size = new Size(180, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = brickRed,
                    ForeColor = C_White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                SetRoundedRegion(btn, 6);
                btn.Click += (s, e) => MessageBox.Show(footerText + " (to be implemented)");
                card.Controls.Add(btn);
            }

            parent.Controls.Add(card);
            return card;
        }

        protected void FixDataGridView(DataGridView dgv)
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

        protected void SetRoundedRegion(Control ctrl, int radius)
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

        protected void AddDropShadow(Panel card)
        {
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(20, 0, 0, 0), 3))
                    e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
            };
        }

        // =========================================================
        // SESSION MANAGEMENT (SYNC version)
        // =========================================================
        private void StartSessionTimer()
        {
            ResetActivityTimer();
            sessionTimer = new Timer { Interval = 60000 }; // Check every minute
            sessionTimer.Tick += CheckSessionTimeout;
            sessionTimer.Start();
        }

        private void ResetActivityTimer()
        {
            lastActivityTime = DateTime.Now;
        }

        private void CheckSessionTimeout(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastActivityTime).TotalMinutes > SESSION_TIMEOUT_MINUTES)
            {
                sessionTimer.Stop();
                this.Invoke(new Action(Logout));
            }
        }

        protected void Logout()
        {
            try
            {
                // Log the logout
                AuditHelper.Log("Logout", "Session", $"User {SessionManager.CurrentUsername} logged out");

                // Clear session
                SessionManager.Logout();

                // Stop timer
                sessionTimer?.Stop();

                // Show login form
                var loginForm = new Shared.LoginForm();
                loginForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout Error: {ex.Message}");
                Application.Exit();
            }
        }

        // =========================================================
        // DATABASE CONNECTION STATUS (SYNC version)
        // =========================================================
        protected bool IsDatabaseConnected()
        {
            try
            {
                return DatabaseHelper.TestConnection();
            }
            catch
            {
                return false;
            }
        }

        protected void ShowDatabaseStatus()
        {
            if (IsDatabaseConnected())
            {
                MessageBox.Show("✅ Database connection is active.", "Connection Status",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("❌ Database connection failed. Please check your connection string.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            sessionTimer?.Stop();
            sessionTimer?.Dispose();
            base.OnFormClosed(e);
        }

        protected bool CheckTableExists(string tableName)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TableName", tableName);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        protected void ShowLoading(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(ShowLoading), message);
                return;
            }

            Cursor = Cursors.WaitCursor;
            // Optional: Show loading overlay
        }

        protected void HideLoading()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideLoading));
                return;
            }

            Cursor = Cursors.Default;
            // Optional: Hide loading overlay
        }
    }
}