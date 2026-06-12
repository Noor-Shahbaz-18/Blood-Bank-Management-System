using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;
using DonorModel = BloodBankManagementSystem.Classes.Models.Donor;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonorDashboard : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Color brickRed = Color.FromArgb(120, 22, 27);
        private Label userNameLabel;
        private Label bloodGroupLabel;
        private Label statsNumberLabel;
        private Label nextDonationDateLabel;
        private Label welcomeMessageLabel;
        private DonorModel currentDonor;
        private Timer refreshTimer;
        private int unreadNotificationCount = 0;
        private Label lblUnreadBadge;

        public DonorDashboard()
        {
            this.Text = "Blood Donor Dashboard";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 244, 244);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            LoadCurrentDonor();
            BuildMainContent();
            BuildSidebar();
            LoadRealData();
            LoadUnreadNotificationCount();

            // Auto-refresh every 30 seconds
            refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) => { LoadRealData(); LoadUnreadNotificationCount(); };
            refreshTimer.Start();
        }

        // =====================================================
        // LOAD CURRENT DONOR FROM DATABASE
        // =====================================================
        private void LoadCurrentDonor()
        {
            try
            {
                int userID = SessionManager.CurrentUserID;
                currentDonor = DonorDAL.GetDonorByUserID(userID);

                if (currentDonor == null)
                {
                    var user = AdminDAL.GetUserByID(userID);
                    if (user != null && user.Role == Classes.Enums.UserRole.Donor)
                    {
                        currentDonor = new DonorModel
                        {
                            UserID = userID,
                            FullName = user.FullName,
                            BloodGroup = "Not Set",
                            IsActive = true,
                            RegistrationDate = DateTime.Now
                        };
                        DonorDAL.Insert(currentDonor);
                        currentDonor = DonorDAL.GetDonorByUserID(userID);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading donor: " + ex.Message);
            }
        }

        // =====================================================
        // LOAD REAL DATA FROM DATABASE
        // =====================================================
        private void LoadRealData()
        {
            try
            {
                // Update user name
                if (userNameLabel != null)
                {
                    string donorName = currentDonor?.FullName ?? SessionManager.CurrentFullName ?? "Valued Donor";
                    userNameLabel.Text = donorName;

                    // Welcome message
                    string firstName = donorName.Split(' ')[0];
                    if (welcomeMessageLabel != null)
                        welcomeMessageLabel.Text = $"Welcome Back {firstName} 👋";
                }

                // Update blood group
                if (bloodGroupLabel != null)
                {
                    string bloodGroup = currentDonor?.BloodGroup ?? "Not Set";
                    bloodGroupLabel.Text = $"Blood Group : {bloodGroup}";
                }

                // Update total donations count
                if (statsNumberLabel != null)
                {
                    int donationCount = GetTotalDonationsCount();
                    statsNumberLabel.Text = donationCount.ToString("00");
                }

                // Update next eligible donation date
                if (nextDonationDateLabel != null)
                {
                    string nextDate = GetNextEligibleDate();
                    nextDonationDateLabel.Text = nextDate;
                }

                // Update unread notification badge
                UpdateNotificationBadge();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadRealData Error: " + ex.Message);
            }
        }

        private int GetTotalDonationsCount()
        {
            try
            {
                if (currentDonor == null) return 0;

                // First try from Donor table
                if (currentDonor.TotalDonations > 0)
                    return currentDonor.TotalDonations;

                // Otherwise count from DonationHistory
                DataTable dt = DonationHistoryDAL.GetByDonorID(currentDonor.DonorID);
                return dt?.Rows.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetNextEligibleDate()
        {
            try
            {
                if (currentDonor == null || !currentDonor.LastDonationDate.HasValue)
                    return "Available Now";

                DateTime nextDate = currentDonor.LastDonationDate.Value.AddMonths(3);
                if (nextDate <= DateTime.Today)
                    return "Available Now";

                return nextDate.ToString("dd MMM yyyy");
            }
            catch
            {
                return "Available Now";
            }
        }

        private void LoadUnreadNotificationCount()
        {
            try
            {
                int userID = SessionManager.CurrentUserID;
                unreadNotificationCount = NotificationDAL.GetUnreadCount(userID);
                UpdateNotificationBadge();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadUnreadNotificationCount Error: " + ex.Message);
            }
        }

        private void UpdateNotificationBadge()
        {
            if (lblUnreadBadge != null)
            {
                if (unreadNotificationCount > 0)
                {
                    lblUnreadBadge.Text = unreadNotificationCount.ToString();
                    lblUnreadBadge.Visible = true;
                }
                else
                {
                    lblUnreadBadge.Visible = false;
                }
            }
        }

        // =====================================================
        // BUILD SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = brickRed;
            sidebarPanel.Padding = new Padding(18, 20, 18, 25);

            // LOGO PANEL
            Panel logoContainer = new Panel();
            logoContainer.Dock = DockStyle.Top;
            logoContainer.Height = 130;
            logoContainer.BackColor = Color.White;
            logoContainer.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            TableLayoutPanel logoTable = new TableLayoutPanel();
            logoTable.Dock = DockStyle.Fill;
            logoTable.RowCount = 3;
            logoTable.ColumnCount = 1;
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32f));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23f));
            logoTable.Padding = new Padding(5);
            logoTable.BackColor = Color.Transparent;
            logoContainer.Controls.Add(logoTable);

            PictureBox bloodLogo = new PictureBox();
            bloodLogo.SizeMode = PictureBoxSizeMode.Zoom;
            bloodLogo.Dock = DockStyle.Fill;
            bloodLogo.Margin = new Padding(0, 5, 0, 5);
            bloodLogo.BackColor = Color.White;
            bloodLogo.Image = CreateBloodDropImage();
            logoTable.Controls.Add(bloodLogo, 0, 0);

            Label systemName = new Label();
            systemName.Text = "Blood Bank";
            systemName.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            systemName.ForeColor = brickRed;
            systemName.Dock = DockStyle.Fill;
            systemName.TextAlign = ContentAlignment.MiddleCenter;
            logoTable.Controls.Add(systemName, 0, 1);

            Label systemSubName = new Label();
            systemSubName.Text = "Management System";
            systemSubName.Font = new Font("Segoe UI", 9);
            systemSubName.ForeColor = Color.FromArgb(107, 114, 128);
            systemSubName.Dock = DockStyle.Fill;
            systemSubName.TextAlign = ContentAlignment.TopCenter;
            logoTable.Controls.Add(systemSubName, 0, 2);

            // Menu Panel
            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(0, 15, 0, 0);
            menuPanel.BackColor = Color.Transparent;

            AddMenuItem(menuPanel, "🏠", "Dashboard", true, null);
            AddMenuItem(menuPanel, "📅", "Appointments", false, () =>
            {
                using (BookAppointment apptForm = new BookAppointment())
                    apptForm.ShowDialog();
                LoadRealData();
            });
            AddMenuItem(menuPanel, "📄", "Donation History", false, () =>
            {
                using (DonationHistory historyForm = new DonationHistory())
                    historyForm.ShowDialog();
                LoadRealData();
            });
            AddMenuItem(menuPanel, "📍", "Nearby Camps", false, () =>
            {
                using (NearbyCamps campsForm = new NearbyCamps())
                    campsForm.ShowDialog();
            });
            AddMenuItem(menuPanel, "🔔", "Notifications", false, () =>
            {
                using (DonorNotifications notifForm = new DonorNotifications())
                {
                    notifForm.ShowDialog();
                    LoadUnreadNotificationCount();
                }
            });
            AddMenuItem(menuPanel, "👤", "Profile Settings", false, () =>
            {
                using (DonorProfileSetting profileForm = new DonorProfileSetting())
                {
                    profileForm.ShowDialog();
                    LoadCurrentDonor();
                    LoadRealData();
                }
            });
            AddMenuItem(menuPanel, "✅", "Check Eligibility", false, () =>
            {
                using (DonorEligibility eligibilityForm = new DonorEligibility())
                    eligibilityForm.ShowDialog();
            });

            Panel spacer = new Panel();
            spacer.Size = new Size(210, 1);
            spacer.Margin = new Padding(0, 20, 0, 0);
            spacer.BackColor = Color.Transparent;
            menuPanel.Controls.Add(spacer);

            AddMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Logout Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoContainer);
            this.Controls.Add(sidebarPanel);
        }

        private void AddMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 45);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.White : Color.Transparent;

            if (active)
            {
                item.Paint += (s, e) =>
                {
                    Panel p = (Panel)s;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath gp = RoundRect(p.ClientRectangle, 10))
                        p.Region = new Region(gp);
                };
            }

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = active ? brickRed : Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(35, 35);
            iconLbl.Location = new Point(10, 5);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 10);
            textLbl.ForeColor = active ? brickRed : Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(155, 35);
            textLbl.Location = new Point(48, 5);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            EventHandler enter = (s, e) =>
            {
                if (!active)
                {
                    item.BackColor = Color.FromArgb(200, 50, 50);
                    iconLbl.ForeColor = Color.White;
                    textLbl.ForeColor = Color.White;
                }
            };
            EventHandler leave = (s, e) =>
            {
                if (!active)
                {
                    item.BackColor = Color.Transparent;
                    iconLbl.ForeColor = Color.White;
                    textLbl.ForeColor = Color.White;
                }
            };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter;
            iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter;
            textLbl.MouseLeave += leave;
            item.MouseEnter += enter;
            item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        // =====================================================
        // BUILD MAIN CONTENT
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 244, 244);
            mainPanel.Padding = new Padding(20);
            mainPanel.AutoScroll = true;

            // TOP BAR
            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Top;
            topBar.Height = 80;
            topBar.BackColor = Color.White;
            topBar.Margin = new Padding(0, 0, 0, 20);
            topBar.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 18))
                    p.Region = new Region(path);
            };

            // User Name
            userNameLabel = new Label();
            userNameLabel.Text = "Loading...";
            userNameLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            userNameLabel.ForeColor = Color.FromArgb(34, 34, 34);
            userNameLabel.BackColor = Color.Transparent;
            userNameLabel.AutoSize = true;
            userNameLabel.Location = new Point(20, 10);

            // Blood Group
            bloodGroupLabel = new Label();
            bloodGroupLabel.Text = "Blood Group : Loading...";
            bloodGroupLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            bloodGroupLabel.ForeColor = brickRed;
            bloodGroupLabel.BackColor = Color.Transparent;
            bloodGroupLabel.AutoSize = true;
            bloodGroupLabel.Location = new Point(20, 44);

            // Bell Panel with Notification Badge
            Panel bellPanel = new Panel();
            bellPanel.Size = new Size(44, 44);
            bellPanel.Location = new Point(topBar.Width - 110, 18);
            bellPanel.BackColor = Color.FromArgb(244, 244, 244);
            bellPanel.Cursor = Cursors.Hand;
            bellPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 44, 44);
                    p.Region = new Region(path);
                }
            };
            bellPanel.Click += (s, e) =>
            {
                using (DonorNotifications notifForm = new DonorNotifications())
                {
                    notifForm.ShowDialog();
                    LoadUnreadNotificationCount();
                }
            };

            Label bellIcon = new Label();
            bellIcon.Text = "🔔";
            bellIcon.Font = new Font("Segoe UI Emoji", 18);
            bellIcon.BackColor = Color.Transparent;
            bellIcon.AutoSize = true;
            bellIcon.Location = new Point(7, 8);
            bellIcon.Cursor = Cursors.Hand;
            bellIcon.Click += (s, e) =>
            {
                using (DonorNotifications notifForm = new DonorNotifications())
                {
                    notifForm.ShowDialog();
                    LoadUnreadNotificationCount();
                }
            };
            bellPanel.Controls.Add(bellIcon);

            // Unread Notification Badge
            lblUnreadBadge = new Label();
            lblUnreadBadge.Text = "0";
            lblUnreadBadge.Size = new Size(20, 20);
            lblUnreadBadge.Location = new Point(30, 2);
            lblUnreadBadge.BackColor = Color.Red;
            lblUnreadBadge.ForeColor = Color.White;
            lblUnreadBadge.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblUnreadBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblUnreadBadge.Visible = false;
            lblUnreadBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 20, 20);
                    lblUnreadBadge.Region = new Region(path);
                }
            };
            bellPanel.Controls.Add(lblUnreadBadge);

            // Avatar Panel
            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(44, 44);
            avatarPanel.Location = new Point(topBar.Width - 55, 18);
            avatarPanel.BackColor = brickRed;
            avatarPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 44, 44);
                    p.Region = new Region(path);
                }
            };

            string donorName = currentDonor?.FullName ?? SessionManager.CurrentFullName ?? "D";
            string initial = donorName.Length > 0 ? donorName.Substring(0, 1).ToUpper() : "D";
            Label avatarText = new Label();
            avatarText.Text = initial;
            avatarText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            avatarText.ForeColor = Color.White;
            avatarText.BackColor = Color.Transparent;
            avatarText.AutoSize = false;
            avatarText.Size = new Size(44, 44);
            avatarText.Location = new Point(0, 0);
            avatarText.TextAlign = ContentAlignment.MiddleCenter;
            avatarPanel.Controls.Add(avatarText);

            topBar.Resize += (s, e) =>
            {
                bellPanel.Location = new Point(topBar.Width - 110, 18);
                avatarPanel.Location = new Point(topBar.Width - 55, 18);
            };

            topBar.Controls.Add(userNameLabel);
            topBar.Controls.Add(bloodGroupLabel);
            topBar.Controls.Add(bellPanel);
            topBar.Controls.Add(avatarPanel);

            // GRID PANEL
            Panel gridPanel = new Panel();
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.BackColor = Color.Transparent;

            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = (int)(this.Width * 0.63);
            leftPanel.BackColor = Color.Transparent;

            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.Transparent;
            rightPanel.AutoScroll = true;

            // WELCOME CARD
            Panel welcomeCard = new Panel();
            welcomeCard.Dock = DockStyle.Top;
            welcomeCard.Height = 120;
            welcomeCard.Margin = new Padding(0, 0, 10, 15);
            welcomeCard.BackColor = brickRed;
            welcomeCard.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 22))
                    p.Region = new Region(path);
            };

            welcomeMessageLabel = new Label();
            welcomeMessageLabel.Text = "Welcome Back! 👋";
            welcomeMessageLabel.Font = new Font("Segoe UI", 12);
            welcomeMessageLabel.ForeColor = Color.White;
            welcomeMessageLabel.BackColor = Color.Transparent;
            welcomeMessageLabel.AutoSize = true;
            welcomeMessageLabel.Location = new Point(20, 18);

            Label readyText = new Label();
            readyText.Text = "Ready To Save Lives?";
            readyText.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            readyText.ForeColor = Color.White;
            readyText.BackColor = Color.Transparent;
            readyText.AutoSize = true;
            readyText.Location = new Point(20, 48);

            Panel bloodBox = new Panel();
            bloodBox.Size = new Size(95, 95);
            bloodBox.Location = new Point(welcomeCard.Width - 120, 12);
            bloodBox.BackColor = brickRed;
            bloodBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 20))
                    p.Region = new Region(path);
            };

            Label bloodGroup = new Label();
            bloodGroup.Text = currentDonor?.BloodGroup ?? "?";
            bloodGroup.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            bloodGroup.ForeColor = Color.White;
            bloodGroup.BackColor = Color.Transparent;
            bloodGroup.AutoSize = false;
            bloodGroup.Size = new Size(95, 55);
            bloodGroup.Location = new Point(0, 8);
            bloodGroup.TextAlign = ContentAlignment.MiddleCenter;

            Label donorLabel = new Label();
            donorLabel.Text = "Donor";
            donorLabel.Font = new Font("Segoe UI", 9);
            donorLabel.ForeColor = Color.White;
            donorLabel.BackColor = Color.Transparent;
            donorLabel.AutoSize = false;
            donorLabel.Size = new Size(95, 20);
            donorLabel.Location = new Point(0, 60);
            donorLabel.TextAlign = ContentAlignment.MiddleCenter;

            bloodBox.Controls.Add(bloodGroup);
            bloodBox.Controls.Add(donorLabel);

            welcomeCard.Resize += (s, e) =>
            {
                bloodBox.Location = new Point(welcomeCard.Width - 120, 12);
            };

            welcomeCard.Controls.Add(welcomeMessageLabel);
            welcomeCard.Controls.Add(readyText);
            welcomeCard.Controls.Add(bloodBox);

            // DONATION CARD
            Panel donationCard = new Panel();
            donationCard.Dock = DockStyle.Top;
            donationCard.Height = 100;
            donationCard.Margin = new Padding(0, 0, 10, 15);
            donationCard.BackColor = Color.White;
            donationCard.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 18))
                    p.Region = new Region(path);
            };

            Panel calIconBox = new Panel();
            calIconBox.Size = new Size(55, 55);
            calIconBox.Location = new Point(18, 22);
            calIconBox.BackColor = Color.FromArgb(255, 240, 240);
            calIconBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 16))
                    p.Region = new Region(path);
            };

            Label calIcon = new Label();
            calIcon.Text = "📅";
            calIcon.Font = new Font("Segoe UI Emoji", 20);
            calIcon.BackColor = Color.Transparent;
            calIcon.AutoSize = false;
            calIcon.Size = new Size(55, 55);
            calIcon.Location = new Point(0, 0);
            calIcon.TextAlign = ContentAlignment.MiddleCenter;
            calIconBox.Controls.Add(calIcon);

            Label nextDonationLabel = new Label();
            nextDonationLabel.Text = "Your Next Eligible Donation";
            nextDonationLabel.Font = new Font("Segoe UI", 9);
            nextDonationLabel.ForeColor = Color.FromArgb(119, 119, 119);
            nextDonationLabel.BackColor = Color.Transparent;
            nextDonationLabel.AutoSize = true;
            nextDonationLabel.Location = new Point(85, 18);

            nextDonationDateLabel = new Label();
            nextDonationDateLabel.Text = "Loading...";
            nextDonationDateLabel.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            nextDonationDateLabel.ForeColor = Color.FromArgb(34, 34, 34);
            nextDonationDateLabel.BackColor = Color.Transparent;
            nextDonationDateLabel.AutoSize = true;
            nextDonationDateLabel.Location = new Point(85, 40);

            Label afterText = new Label();
            afterText.Text = "After 3 Months";
            afterText.Font = new Font("Segoe UI", 9);
            afterText.ForeColor = Color.FromArgb(153, 153, 153);
            afterText.BackColor = Color.Transparent;
            afterText.AutoSize = true;
            afterText.Location = new Point(85, 65);

            donationCard.Controls.Add(calIconBox);
            donationCard.Controls.Add(nextDonationLabel);
            donationCard.Controls.Add(nextDonationDateLabel);
            donationCard.Controls.Add(afterText);

            // SECTION TITLE
            Label sectionTitle = new Label();
            sectionTitle.Text = "Quick Actions";
            sectionTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            sectionTitle.ForeColor = Color.FromArgb(34, 34, 34);
            sectionTitle.BackColor = Color.Transparent;
            sectionTitle.Dock = DockStyle.Top;
            sectionTitle.Height = 35;
            sectionTitle.Margin = new Padding(0, 10, 10, 10);

            // ACTIONS GRID
            TableLayoutPanel actionsGrid = new TableLayoutPanel();
            actionsGrid.Dock = DockStyle.Fill;
            actionsGrid.ColumnCount = 2;
            actionsGrid.RowCount = 2;
            actionsGrid.BackColor = Color.Transparent;
            actionsGrid.Margin = new Padding(0, 0, 10, 0);
            actionsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            actionsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            actionsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            actionsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            actionsGrid.Controls.Add(CreateActionBox("📅", "Book Appointment", () =>
            {
                using (BookAppointment apptForm = new BookAppointment())
                    apptForm.ShowDialog();
                LoadRealData();
            }), 0, 0);

            actionsGrid.Controls.Add(CreateActionBox("📄", "Donation History", () =>
            {
                using (DonationHistory historyForm = new DonationHistory())
                    historyForm.ShowDialog();
            }), 1, 0);

            actionsGrid.Controls.Add(CreateActionBox("📍", "Nearby Camps", () =>
            {
                using (NearbyCamps campsForm = new NearbyCamps())
                    campsForm.ShowDialog();
            }), 0, 1);

            actionsGrid.Controls.Add(CreateActionBox("👤", "Profile Settings", () =>
            {
                using (DonorProfileSetting profileForm = new DonorProfileSetting())
                {
                    profileForm.ShowDialog();
                    LoadCurrentDonor();
                    LoadRealData();
                }
            }), 1, 1);

            leftPanel.Controls.Add(actionsGrid);
            leftPanel.Controls.Add(sectionTitle);
            leftPanel.Controls.Add(donationCard);
            leftPanel.Controls.Add(welcomeCard);

            // RIGHT PANEL - STATS CARD
            Panel statsCard = new Panel();
            statsCard.Dock = DockStyle.Top;
            statsCard.Height = 200;
            statsCard.BackColor = Color.White;
            statsCard.Margin = new Padding(10, 0, 0, 15);
            statsCard.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 22))
                    p.Region = new Region(path);
            };

            Label statsTitle = new Label();
            statsTitle.Text = "Total Donations";
            statsTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            statsTitle.ForeColor = Color.FromArgb(68, 68, 68);
            statsTitle.BackColor = Color.Transparent;
            statsTitle.TextAlign = ContentAlignment.MiddleCenter;
            statsTitle.Dock = DockStyle.Top;
            statsTitle.Height = 40;

            statsNumberLabel = new Label();
            statsNumberLabel.Text = "00";
            statsNumberLabel.Font = new Font("Segoe UI", 48, FontStyle.Bold);
            statsNumberLabel.ForeColor = brickRed;
            statsNumberLabel.BackColor = Color.Transparent;
            statsNumberLabel.TextAlign = ContentAlignment.MiddleCenter;
            statsNumberLabel.Dock = DockStyle.Top;
            statsNumberLabel.Height = 70;

            Label statsDesc = new Label();
            statsDesc.Text = "You have saved many lives ❤️";
            statsDesc.Font = new Font("Segoe UI", 9);
            statsDesc.ForeColor = Color.FromArgb(119, 119, 119);
            statsDesc.BackColor = Color.Transparent;
            statsDesc.TextAlign = ContentAlignment.MiddleCenter;
            statsDesc.Dock = DockStyle.Top;
            statsDesc.Height = 25;

            statsCard.Controls.Add(statsDesc);
            statsCard.Controls.Add(statsNumberLabel);
            statsCard.Controls.Add(statsTitle);

            // NOTIFICATION CARD (Recent Notification)
            Panel notifCard = new Panel();
            notifCard.Dock = DockStyle.Fill;
            notifCard.BackColor = Color.White;
            notifCard.Margin = new Padding(10, 0, 0, 0);
            notifCard.Padding = new Padding(18);
            notifCard.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 22))
                    p.Region = new Region(path);
            };

            Label notifTitle = new Label();
            notifTitle.Text = "Recent Notification";
            notifTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            notifTitle.ForeColor = Color.FromArgb(34, 34, 34);
            notifTitle.BackColor = Color.Transparent;
            notifTitle.Dock = DockStyle.Top;
            notifTitle.Height = 35;

            Panel notifItem = LoadRecentNotification();
            notifCard.Controls.Add(notifItem);
            notifCard.Controls.Add(notifTitle);

            rightPanel.Controls.Add(notifCard);
            rightPanel.Controls.Add(statsCard);

            gridPanel.Controls.Add(rightPanel);
            gridPanel.Controls.Add(leftPanel);

            mainPanel.Controls.Add(gridPanel);
            mainPanel.Controls.Add(topBar);
            this.Controls.Add(mainPanel);
        }

        private Panel LoadRecentNotification()
        {
            Panel notifItem = new Panel();
            notifItem.Dock = DockStyle.Top;
            notifItem.Height = 80;
            notifItem.BackColor = Color.Transparent;
            notifItem.Margin = new Padding(0, 10, 0, 0);

            try
            {
                int userID = SessionManager.CurrentUserID;
                DataTable dt = NotificationDAL.GetUnreadByUser(userID);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string title = row["Title"].ToString();
                    string message = row["Message"].ToString();
                    DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);

                    Panel checkCircle = new Panel();
                    checkCircle.Size = new Size(38, 38);
                    checkCircle.Location = new Point(0, 5);
                    checkCircle.BackColor = Color.FromArgb(232, 255, 240);
                    checkCircle.Paint += (s, e) =>
                    {
                        Panel p = (Panel)s;
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddEllipse(0, 0, 38, 38);
                            p.Region = new Region(path);
                        }
                    };

                    Label checkIcon = new Label();
                    checkIcon.Text = "🔔";
                    checkIcon.Font = new Font("Segoe UI Emoji", 14);
                    checkIcon.ForeColor = brickRed;
                    checkIcon.BackColor = Color.Transparent;
                    checkIcon.AutoSize = false;
                    checkIcon.Size = new Size(38, 38);
                    checkIcon.Location = new Point(0, 0);
                    checkIcon.TextAlign = ContentAlignment.MiddleCenter;
                    checkCircle.Controls.Add(checkIcon);

                    Label notifHead = new Label();
                    notifHead.Text = title.Length > 50 ? title.Substring(0, 47) + "..." : title;
                    notifHead.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    notifHead.ForeColor = Color.FromArgb(34, 34, 34);
                    notifHead.BackColor = Color.Transparent;
                    notifHead.AutoSize = true;
                    notifHead.Location = new Point(50, 5);

                    Label notifDesc = new Label();
                    notifDesc.Text = message.Length > 60 ? message.Substring(0, 57) + "..." : message;
                    notifDesc.Font = new Font("Segoe UI", 9);
                    notifDesc.ForeColor = Color.FromArgb(102, 102, 102);
                    notifDesc.BackColor = Color.Transparent;
                    notifDesc.AutoSize = true;
                    notifDesc.Location = new Point(50, 28);

                    Label notifDate = new Label();
                    notifDate.Text = GetTimeAgo(createdAt);
                    notifDate.Font = new Font("Segoe UI", 8);
                    notifDate.ForeColor = Color.FromArgb(153, 153, 153);
                    notifDate.BackColor = Color.Transparent;
                    notifDate.AutoSize = true;
                    notifDate.Location = new Point(50, 50);

                    notifItem.Controls.Add(checkCircle);
                    notifItem.Controls.Add(notifHead);
                    notifItem.Controls.Add(notifDesc);
                    notifItem.Controls.Add(notifDate);
                }
                else
                {
                    // No notifications
                    Label lblNoNotif = new Label();
                    lblNoNotif.Text = "No new notifications";
                    lblNoNotif.Font = new Font("Segoe UI", 10);
                    lblNoNotif.ForeColor = Color.Gray;
                    lblNoNotif.TextAlign = ContentAlignment.MiddleCenter;
                    lblNoNotif.Dock = DockStyle.Fill;
                    notifItem.Controls.Add(lblNoNotif);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadRecentNotification Error: " + ex.Message);
                Label lblError = new Label();
                lblError.Text = "Unable to load notifications";
                lblError.Font = new Font("Segoe UI", 10);
                lblError.ForeColor = Color.Gray;
                lblError.TextAlign = ContentAlignment.MiddleCenter;
                lblError.Dock = DockStyle.Fill;
                notifItem.Controls.Add(lblError);
            }

            return notifItem;
        }

        private string GetTimeAgo(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)} weeks ago";

            return date.ToString("dd MMM yyyy");
        }

        private Panel CreateActionBox(string icon, string text, Action clickAction = null)
        {
            Panel box = new Panel();
            box.Dock = DockStyle.Fill;
            box.Margin = new Padding(0, 0, 8, 8);
            box.BackColor = Color.White;
            box.Cursor = Cursors.Hand;
            box.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 18))
                    p.Region = new Region(path);
            };

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 28);
            iconLbl.ForeColor = brickRed;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(60, 40);
            iconLbl.Location = new Point((box.Width - 60) / 2, 25);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            textLbl.ForeColor = Color.FromArgb(34, 34, 34);
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(120, 25);
            textLbl.Location = new Point((box.Width - 120) / 2, 70);
            textLbl.TextAlign = ContentAlignment.MiddleCenter;

            box.Resize += (s, e) =>
            {
                iconLbl.Location = new Point((box.Width - 60) / 2, 25);
                textLbl.Location = new Point((box.Width - 120) / 2, 70);
            };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                box.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            EventHandler hoverEnter = (s, e) => box.BackColor = Color.FromArgb(255, 245, 245);
            EventHandler hoverLeave = (s, e) => box.BackColor = Color.White;

            box.MouseEnter += hoverEnter;
            box.MouseLeave += hoverLeave;
            iconLbl.MouseEnter += hoverEnter;
            iconLbl.MouseLeave += hoverLeave;
            textLbl.MouseEnter += hoverEnter;
            textLbl.MouseLeave += hoverLeave;

            box.Controls.Add(iconLbl);
            box.Controls.Add(textLbl);

            return box;
        }

        private Image CreateBloodDropImage()
        {
            Bitmap bmp = new Bitmap(80, 80);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(20, 30, 40, 40);
                    path.AddPolygon(new Point[] { new Point(40, 5), new Point(22, 40), new Point(58, 40) });
                    path.FillMode = FillMode.Winding;
                    g.FillPath(brush, path);
                }
            }
            return bmp;
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