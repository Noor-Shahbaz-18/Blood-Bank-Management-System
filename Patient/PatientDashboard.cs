using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using PatientModel = BloodBankManagementSystem.Classes.Models.Patient;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class PatientDashboard : Form
    {
        private Panel sidebar;
        private Panel topbar;
        private Panel contentPanel;
        private Panel notificationPanel;
        private Button activeMenuButton = null;
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color brickRedDark = Color.FromArgb(153, 27, 27);
        private readonly Color brickRedHover = Color.FromArgb(145, 25, 30);

        // Dynamic patient data
        private PatientModel currentPatient;
        private int currentUserId;

        // UI controls that need dynamic update
        private Label welcomeLabel;
        private Label patientNameLabel;
        private Label avatarTextLabel;

        public PatientDashboard()
        {
            InitializeComponent();
            BuildUI();
            LoadPatientData();
        }

        // =========================================================
        // LOAD PATIENT DATA FROM DATABASE
        // =========================================================
        private void LoadPatientData()
        {
            try
            {
                currentUserId = SessionManager.CurrentUserID;
                currentPatient = PatientDAL.GetByUserID(currentUserId);

                if (currentPatient != null)
                {
                    // Update welcome label
                    if (welcomeLabel != null)
                        welcomeLabel.Text = $"Welcome back, {currentPatient.FullName}";

                    // Update patient name in user panel
                    if (patientNameLabel != null)
                        patientNameLabel.Text = currentPatient.FullName;

                    // Update avatar initial (first letter of name)
                    if (avatarTextLabel != null)
                    {
                        string initial = currentPatient.FullName.Length > 0
                            ? currentPatient.FullName.Substring(0, 1).ToUpper()
                            : "P";
                        avatarTextLabel.Text = initial;
                    }
                }
                else
                {
                    // Fallback to SessionManager
                    if (welcomeLabel != null)
                        welcomeLabel.Text = $"Welcome back, {SessionManager.CurrentFullName}";
                    if (patientNameLabel != null)
                        patientNameLabel.Text = SessionManager.CurrentFullName;
                    if (avatarTextLabel != null)
                    {
                        string initial = SessionManager.CurrentFullName?.Length > 0
                            ? SessionManager.CurrentFullName.Substring(0, 1).ToUpper()
                            : "P";
                        avatarTextLabel.Text = initial;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPatientData Error: {ex.Message}");
                // Fallback
                if (welcomeLabel != null)
                    welcomeLabel.Text = $"Welcome back, {SessionManager.CurrentFullName}";
                if (patientNameLabel != null)
                    patientNameLabel.Text = SessionManager.CurrentFullName;
            }
        }

        private void BuildUI()
        {
            // FORM SETTINGS
            this.Text = "Blood Bank Management - Patient";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 700);
            this.Font = new Font("Segoe UI", 10);

            // TOPBAR
            topbar = new Panel();
            topbar.Dock = DockStyle.Top;
            topbar.Height = 80;
            topbar.BackColor = Color.White;
            this.Controls.Add(topbar);
            BuildTopbar();

            // SIDEBAR
            sidebar = new Panel();
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 240;
            sidebar.BackColor = brickRed;
            sidebar.Location = new Point(0, 80);
            this.Controls.Add(sidebar);
            BuildSidebar();

            // NOTIFICATION PANEL
            notificationPanel = new Panel();
            notificationPanel.Size = new Size(350, 400);
            notificationPanel.BackColor = Color.White;
            notificationPanel.BorderStyle = BorderStyle.FixedSingle;
            notificationPanel.Visible = false;
            notificationPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Controls.Add(notificationPanel);
            notificationPanel.BringToFront();
            BuildNotificationPanel();

            // CONTENT PANEL
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(245, 247, 250);
            contentPanel.AutoScroll = true;
            contentPanel.Padding = new Padding(20, 20, 20, 20);
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            LoadPage(new DashboardUC());
        }

        private void BuildTopbar()
        {
            // WELCOME TEXT - Dynamic
            welcomeLabel = new Label();
            welcomeLabel.Text = "Welcome back, Loading...";
            welcomeLabel.Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
            welcomeLabel.ForeColor = Color.FromArgb(31, 41, 55);
            welcomeLabel.Location = new Point(20, 22);
            welcomeLabel.AutoSize = true;
            topbar.Controls.Add(welcomeLabel);

            // RIGHT SIDE PANEL
            Panel rightPanel = new Panel();
            rightPanel.Size = new Size(300, 60);
            rightPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rightPanel.BackColor = Color.Transparent;
            topbar.Controls.Add(rightPanel);

            Action positionRightPanel = () =>
            {
                rightPanel.Location = new Point(topbar.Width - rightPanel.Width - 20, 10);
            };
            topbar.Resize += (s, e) => positionRightPanel();
            this.Load += (s, e) => positionRightPanel();

            // NOTIFICATION BUTTON
            Button notifyBtn = new Button();
            notifyBtn.Text = "🔔";
            notifyBtn.Width = 44;
            notifyBtn.Height = 38;
            notifyBtn.Location = new Point(30, 11);
            notifyBtn.FlatStyle = FlatStyle.Flat;
            notifyBtn.FlatAppearance.BorderSize = 0;
            notifyBtn.BackColor = Color.FromArgb(245, 247, 250);
            notifyBtn.Font = new Font("Segoe UI Emoji", 13);
            notifyBtn.Cursor = Cursors.Hand;
            notifyBtn.Click += (s, e) => ToggleNotificationPanel();
            rightPanel.Controls.Add(notifyBtn);

            // USER PANEL
            Panel userPanel = new Panel();
            userPanel.Size = new Size(210, 50);
            userPanel.Location = new Point(85, 5);
            userPanel.BackColor = Color.FromArgb(245, 247, 250);
            rightPanel.Controls.Add(userPanel);

            // AVATAR
            Panel avatar = new Panel();
            avatar.Size = new Size(40, 40);
            avatar.Location = new Point(10, 5);
            avatar.BackColor = Color.FromArgb(254, 226, 226);
            userPanel.Controls.Add(avatar);

            avatarTextLabel = new Label();
            avatarTextLabel.Text = "P";
            avatarTextLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            avatarTextLabel.ForeColor = brickRed;
            avatarTextLabel.Dock = DockStyle.Fill;
            avatarTextLabel.TextAlign = ContentAlignment.MiddleCenter;
            avatar.Controls.Add(avatarTextLabel);

            // PATIENT NAME - Dynamic
            patientNameLabel = new Label();
            patientNameLabel.Text = "Loading...";
            patientNameLabel.Font = new Font("Segoe UI Semibold", 10);
            patientNameLabel.Location = new Point(58, 7);
            patientNameLabel.AutoSize = true;
            userPanel.Controls.Add(patientNameLabel);

            // ROLE LABEL
            Label roleLabel = new Label();
            roleLabel.Text = "Patient";
            roleLabel.Font = new Font("Segoe UI", 8);
            roleLabel.ForeColor = Color.Gray;
            roleLabel.Location = new Point(58, 26);
            roleLabel.AutoSize = true;
            userPanel.Controls.Add(roleLabel);
        }

        private void BuildSidebar()
        {
            // LOGO PANEL
            Panel logoContainer = new Panel();
            logoContainer.Size = new Size(200, 130);
            logoContainer.Location = new Point(20, 20);
            logoContainer.BackColor = Color.White;
            sidebar.Controls.Add(logoContainer);

            TableLayoutPanel logoTable = new TableLayoutPanel();
            logoTable.Dock = DockStyle.Fill;
            logoTable.RowCount = 3;
            logoTable.ColumnCount = 1;
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            logoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 23));
            logoTable.Padding = new Padding(5);
            logoContainer.Controls.Add(logoTable);

            PictureBox bloodLogo = new PictureBox();
            bloodLogo.SizeMode = PictureBoxSizeMode.Zoom;
            bloodLogo.Dock = DockStyle.Fill;
            bloodLogo.Margin = new Padding(0, 5, 0, 5);
            bloodLogo.BackColor = Color.White;

            Bitmap bmp = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                using (SolidBrush brush = new SolidBrush(brickRed))
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddEllipse(25, 45, 50, 50);
                    Point[] points = { new Point(50, 15), new Point(32, 55), new Point(68, 55) };
                    path.AddPolygon(points);
                    path.FillMode = FillMode.Winding;
                    g.FillPath(brush, path);
                }
            }
            bloodLogo.Image = bmp;
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

            // DIVIDER
            Panel divider = new Panel();
            divider.Width = 200;
            divider.Height = 1;
            divider.BackColor = brickRedDark;
            divider.Location = new Point(20, 145);
            sidebar.Controls.Add(divider);

            // MENU ITEMS
            string[] menus =
            {
                "Dashboard",
                "Request Blood",
                "Check Blood Availability",
                "Nearest Blood Banks",
                "Request Status",
                "Notifications",
                "Transfusion History",
                "My Profile",
                "Feedback",
                "File a Complaint"
            };

            int top = 160;
            foreach (string item in menus)
            {
                Button btn = new Button();
                btn.Text = item;
                btn.Width = 210;
                btn.Height = 46;
                btn.Location = new Point(15, top);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI Semibold", 10);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(18, 0, 0, 0);
                btn.Cursor = Cursors.Hand;
                btn.Tag = item;
                btn.BackColor = brickRed;

                if (item == "Dashboard")
                {
                    btn.BackColor = brickRedDark;
                    activeMenuButton = btn;
                }

                btn.MouseEnter += (s, e) =>
                {
                    if (btn != activeMenuButton)
                        btn.BackColor = brickRedHover;
                };
                btn.MouseLeave += (s, e) =>
                {
                    if (btn != activeMenuButton)
                        btn.BackColor = brickRed;
                };
                btn.Click += MenuButton_Click;

                sidebar.Controls.Add(btn);
                top += 52;
            }

            // LOGOUT BUTTON
            Button logoutBtn = new Button();
            logoutBtn.Text = "Logout";
            logoutBtn.Width = 210;
            logoutBtn.Height = 46;
            logoutBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            logoutBtn.Location = new Point(15, sidebar.Height - 61);
            logoutBtn.FlatStyle = FlatStyle.Flat;
            logoutBtn.FlatAppearance.BorderSize = 0;
            logoutBtn.ForeColor = Color.White;
            logoutBtn.BackColor = brickRed;
            logoutBtn.Font = new Font("Segoe UI Semibold", 10);
            logoutBtn.TextAlign = ContentAlignment.MiddleLeft;
            logoutBtn.Padding = new Padding(18, 0, 0, 0);
            logoutBtn.Cursor = Cursors.Hand;
            logoutBtn.MouseEnter += (s, e) => logoutBtn.BackColor = brickRedHover;
            logoutBtn.MouseLeave += (s, e) => logoutBtn.BackColor = brickRed;
            logoutBtn.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    this.Hide();
                    var loginForm = new Shared.LoginForm();
                    loginForm.ShowDialog();
                    this.Close();
                }
            };
            sidebar.Controls.Add(logoutBtn);
        }

        private void BuildNotificationPanel()
        {
            Action positionNotificationPanel = () =>
            {
                notificationPanel.Location = new Point(this.Width - notificationPanel.Width - 30, 90);
            };
            this.Resize += (s, e) => positionNotificationPanel();
            this.Load += (s, e) => positionNotificationPanel();

            Label notifTitle = new Label();
            notifTitle.Text = "Notifications";
            notifTitle.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold);
            notifTitle.Location = new Point(15, 15);
            notifTitle.AutoSize = true;
            notificationPanel.Controls.Add(notifTitle);

            ListBox notifList = new ListBox();
            notifList.Location = new Point(15, 50);
            notifList.Size = new Size(320, 335);
            notifList.BorderStyle = BorderStyle.None;
            notifList.Font = new Font("Segoe UI", 9);

            // Load notifications from database
            try
            {
                DataTable dt = NotificationDAL.GetUnreadByUser(SessionManager.CurrentUserID);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string title = row["Title"]?.ToString() ?? "";
                        notifList.Items.Add(title);
                    }
                }
                else
                {
                    notifList.Items.Add("No new notifications");
                }
            }
            catch
            {
                notifList.Items.Add("Your request REQ-1026 is approved");
                notifList.Items.Add("Blood dispatched for REQ-1025");
                notifList.Items.Add("New blood available: O+");
            }
            notificationPanel.Controls.Add(notifList);
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button clickedBtn = sender as Button;
            string menuItem = clickedBtn.Tag.ToString();
            NavigateToMenu(menuItem);
        }

        public void NavigateToMenu(string menuItem)
        {
            // Update active button
            foreach (Control ctrl in sidebar.Controls)
            {
                if (ctrl is Button btn && btn.Tag != null && btn.Tag.ToString() == menuItem)
                {
                    if (activeMenuButton != null)
                        activeMenuButton.BackColor = brickRed;
                    btn.BackColor = brickRedDark;
                    activeMenuButton = btn;
                    break;
                }
            }

            contentPanel.Controls.Clear();

            switch (menuItem)
            {
                case "Dashboard":
                    LoadPage(new DashboardUC());
                    break;
                case "Request Blood":
                    LoadPage(new RequestBloodUC());
                    break;
                case "Check Blood Availability":
                    LoadPage(new CheckBloodAvailabilityUC());
                    break;
                case "Nearest Blood Banks":
                    LoadPage(new NearestBloodBanksUC());
                    break;
                case "Request Status":
                    LoadPage(new RequestStatusUC());
                    break;
                case "Notifications":
                    LoadPage(new NotificationsUC());
                    break;
                case "Transfusion History":
                    LoadPage(new TransfusionHistoryUC());
                    break;
                case "My Profile":
                    // ✅ FIXED: Correct form name is PatientProfile (not PatientProfileForm)
                    using (PatientProfile profileForm = new PatientProfile())
                    {
                        profileForm.ShowDialog();
                        LoadPatientData(); // Refresh dashboard data after profile update
                    }
                    break;
                case "Feedback":
                    LoadPage(new FeedbackUC());
                    break;
                case "File a Complaint":
                    LoadPage(new FileComplaintUC());
                    break;
            }
        }

        private void LoadPage(UserControl page)
        {
            page.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(page);
        }

        private void ToggleNotificationPanel()
        {
            notificationPanel.Visible = !notificationPanel.Visible;
            notificationPanel.BringToFront();
        }
    }
}