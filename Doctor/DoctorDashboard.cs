using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class DoctorDashboard : Form
    {
        private Panel sidebar;
        private Panel topbar;
        private Panel contentPanel;
        private Panel notificationPanel;
        private Button activeMenuButton = null;
        private TextBox searchBox;
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color brickRedDark = Color.FromArgb(153, 27, 27);
        private readonly Color brickRedHover = Color.FromArgb(145, 25, 30);
        private readonly Color brickRedLight = Color.FromArgb(254, 242, 242);
        private DataTable doctorRequisitions; // Store requisitions data

        public DoctorDashboard()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            // =========================================================
            // FORM SETTINGS
            // =========================================================
            this.Text = "Blood Bank Management - Doctor";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 700);
            this.Font = new Font("Segoe UI", 10);

            // =========================================================
            // SIDEBAR
            // =========================================================
            sidebar = new Panel();
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 240;
            sidebar.BackColor = brickRed;
            this.Controls.Add(sidebar);

            // =========================================================
            // LOGO PANEL
            // =========================================================
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

            // =========================================================
            // DIVIDER
            // =========================================================
            Panel divider = new Panel();
            divider.Width = 200;
            divider.Height = 1;
            divider.BackColor = brickRedDark;
            divider.Location = new Point(20, 165);
            sidebar.Controls.Add(divider);

            // =========================================================
            // MENU ITEMS
            // =========================================================
            string[] menus =
            {
                "Overview",
                "New Requisition",
                "Requisition Status",
                "Cross Matching",
                "Transfusion History",
                "Patients",
                "Notifications",
            };

            int top = 175;
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

                if (item == "Overview")
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

            // =========================================================
            // LOGOUT BUTTON
            // =========================================================
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
                    this.Close();
                    Application.Restart();
                }
            };
            sidebar.Controls.Add(logoutBtn);

            // =========================================================
            // TOPBAR
            // =========================================================
            topbar = new Panel();
            topbar.Dock = DockStyle.Top;
            topbar.Height = 80;
            topbar.BackColor = Color.White;
            this.Controls.Add(topbar);
            topbar.BringToFront();

            // WELCOME TEXT - Dynamic from SessionManager
            Label welcome = new Label();
            welcome.Text = $"Welcome back, {SessionManager.CurrentFullName}";
            welcome.Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
            welcome.ForeColor = Color.FromArgb(31, 41, 55);
            welcome.Location = new Point(20, 22);
            welcome.AutoSize = true;
            topbar.Controls.Add(welcome);

            // RIGHT SIDE PANEL
            Panel rightPanel = new Panel();
            rightPanel.Size = new Size(550, 60);
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
            notifyBtn.Location = new Point(275, 11);
            notifyBtn.FlatStyle = FlatStyle.Flat;
            notifyBtn.FlatAppearance.BorderSize = 0;
            notifyBtn.BackColor = Color.FromArgb(245, 247, 250);
            notifyBtn.Font = new Font("Segoe UI Emoji", 13);
            notifyBtn.Cursor = Cursors.Hand;
            notifyBtn.Click += (s, e) =>
            {
                NavigateToMenu("Notifications");
                ToggleNotificationPanel();
            };
            rightPanel.Controls.Add(notifyBtn);

            // USER PANEL
            Panel userPanel = new Panel();
            userPanel.Size = new Size(210, 50);
            userPanel.Location = new Point(330, 5);
            userPanel.BackColor = Color.FromArgb(245, 247, 250);
            rightPanel.Controls.Add(userPanel);

            Panel avatar = new Panel();
            avatar.Size = new Size(40, 40);
            avatar.Location = new Point(10, 5);
            avatar.BackColor = Color.FromArgb(254, 226, 226);
            userPanel.Controls.Add(avatar);

            Label avatarText = new Label();
            string initial = SessionManager.CurrentFullName?.Length > 0 ? SessionManager.CurrentFullName.Substring(0, 1).ToUpper() : "DR";
            avatarText.Text = initial;
            avatarText.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            avatarText.ForeColor = brickRed;
            avatarText.Dock = DockStyle.Fill;
            avatarText.TextAlign = ContentAlignment.MiddleCenter;
            avatar.Controls.Add(avatarText);

            Label doctorName = new Label();
            doctorName.Text = SessionManager.CurrentFullName;
            doctorName.Font = new Font("Segoe UI Semibold", 10);
            doctorName.Location = new Point(58, 7);
            doctorName.AutoSize = true;
            userPanel.Controls.Add(doctorName);

            Label roleLabel = new Label();
            roleLabel.Text = "Doctor";
            roleLabel.Font = new Font("Segoe UI", 8);
            roleLabel.ForeColor = Color.Gray;
            roleLabel.Location = new Point(58, 26);
            roleLabel.AutoSize = true;
            userPanel.Controls.Add(roleLabel);

            // =========================================================
            // NOTIFICATION PANEL
            // =========================================================
            notificationPanel = new Panel();
            notificationPanel.Size = new Size(350, 400);
            notificationPanel.BackColor = Color.White;
            notificationPanel.BorderStyle = BorderStyle.FixedSingle;
            notificationPanel.Visible = false;
            notificationPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Controls.Add(notificationPanel);
            notificationPanel.BringToFront();

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
            notifList.Items.Add("New requisition REQ-1026 received");
            notifList.Items.Add("Cross match completed for REQ-1022");
            notifList.Items.Add("Low stock alert: O- blood group");
            notifList.Items.Add("Patient Sara Khan discharged");
            notifList.Items.Add("Weekly report generated");
            notificationPanel.Controls.Add(notifList);

            // =========================================================
            // CONTENT PANEL
            // =========================================================
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(245, 247, 250);
            contentPanel.AutoScroll = true;
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            LoadOverviewPage();
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Button clickedBtn = sender as Button;
            string menuItem = clickedBtn.Tag.ToString();
            NavigateToMenu(menuItem);
        }

        public void NavigateToMenu(string menuItem)
        {
            // Update active button styling
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

            // Navigate to respective page
            switch (menuItem)
            {
                case "Overview":
                    LoadOverviewPage();
                    break;
                case "New Requisition":
                    NewRequisition reqPage = new NewRequisition();
                    contentPanel.Controls.Add(reqPage);
                    break;
                case "Requisition Status":
                    RequisitionStatus statusPage = new RequisitionStatus();
                    contentPanel.Controls.Add(statusPage);
                    break;
                case "Cross Matching":
                    CrossMatchingResult crossPage = new CrossMatchingResult();
                    contentPanel.Controls.Add(crossPage);
                    break;
                case "Transfusion History":
                    DoctorTransfusionHistory transfusionPage = new DoctorTransfusionHistory();
                    transfusionPage.Dock = DockStyle.Fill;
                    contentPanel.Controls.Add(transfusionPage);
                    break;
                case "Patients":
                    DoctorPatients patientsPage = new DoctorPatients();
                    patientsPage.Dock = DockStyle.Fill;
                    contentPanel.Controls.Add(patientsPage);
                    break;
                case "Notifications":
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(new DoctorNotifications());
                    break;
            }
        }

        private void ToggleNotificationPanel()
        {
            notificationPanel.Visible = !notificationPanel.Visible;
        }

        private void LoadOverviewPage()
        {
            // =========================================================
            // STATS CARDS
            // =========================================================
            Panel cardsContainer = new Panel();
            cardsContainer.Location = new Point(20, 20);
            cardsContainer.Height = 150;
            cardsContainer.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            cardsContainer.BackColor = Color.Transparent;
            contentPanel.Controls.Add(cardsContainer);

            // Load doctor's requisitions from database
            int doctorId = SessionManager.CurrentUserID;
            doctorRequisitions = RequisitionDAL.GetRequisitionsByDoctor(doctorId);

            int totalRequests = doctorRequisitions != null ? doctorRequisitions.Rows.Count : 0;
            int pendingRequests = 0;
            int approvedRequests = 0;
            int crossMatchRequests = 0;

            if (doctorRequisitions != null)
            {
                foreach (DataRow row in doctorRequisitions.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status == "Pending") pendingRequests++;
                    else if (status == "Approved") approvedRequests++;
                    else if (status == "Cross Matching") crossMatchRequests++;
                }
            }

            string[] titles = { "Total Requests", "Pending", "Approved", "Cross Matching" };
            string[] values = { totalRequests.ToString(), pendingRequests.ToString(), approvedRequests.ToString(), crossMatchRequests.ToString() };
            Color[] colors = { brickRed, Color.FromArgb(245, 158, 11), Color.FromArgb(34, 197, 94), Color.FromArgb(59, 130, 246) };

            Panel[] cards = new Panel[4];
            for (int i = 0; i < 4; i++)
            {
                Panel card = new Panel();
                card.BackColor = Color.White;
                cardsContainer.Controls.Add(card);
                cards[i] = card;

                TableLayoutPanel centerPanel = new TableLayoutPanel();
                centerPanel.Dock = DockStyle.Fill;
                centerPanel.RowCount = 2;
                centerPanel.ColumnCount = 1;
                centerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                centerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                card.Controls.Add(centerPanel);

                Label valueLbl = new Label();
                valueLbl.Text = values[i];
                valueLbl.Font = new Font("Segoe UI", 30, FontStyle.Bold);
                valueLbl.ForeColor = colors[i];
                valueLbl.Dock = DockStyle.Fill;
                valueLbl.TextAlign = ContentAlignment.BottomCenter;
                centerPanel.Controls.Add(valueLbl, 0, 0);

                Label titleLbl = new Label();
                titleLbl.Text = titles[i];
                titleLbl.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                titleLbl.ForeColor = Color.FromArgb(75, 85, 99);
                titleLbl.Dock = DockStyle.Fill;
                titleLbl.TextAlign = ContentAlignment.TopCenter;
                centerPanel.Controls.Add(titleLbl, 0, 1);
            }

            Action layoutCards = () =>
            {
                int containerWidth = contentPanel.ClientSize.Width - 40;
                cardsContainer.Width = containerWidth;
                int gap = 14;
                int cardW = (containerWidth - gap * 3) / 4;
                for (int i = 0; i < 4; i++)
                {
                    cards[i].Size = new Size(cardW, 140);
                    cards[i].Location = new Point(i * (cardW + gap), 0);
                }
            };

            contentPanel.Resize += (s, e) => layoutCards();
            layoutCards();

            // =========================================================
            // TABLE PANEL - Shows only doctor's own requisitions
            // =========================================================
            Panel tablePanel = new Panel();
            tablePanel.Location = new Point(20, 185);
            tablePanel.Height = 500;
            tablePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            tablePanel.BackColor = Color.White;
            contentPanel.Controls.Add(tablePanel);

            contentPanel.Resize += (s, e) => { tablePanel.Width = contentPanel.ClientSize.Width - 40; };
            tablePanel.Width = contentPanel.ClientSize.Width - 40;

            Label tableTitle = new Label();
            tableTitle.Text = "My Requisitions";
            tableTitle.Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
            tableTitle.ForeColor = Color.FromArgb(31, 41, 55);
            tableTitle.Location = new Point(22, 18);
            tableTitle.AutoSize = true;
            tablePanel.Controls.Add(tableTitle);

            // SEARCH BOX
            TextBox txtSearch = new TextBox();
            txtSearch.Width = 220;
            txtSearch.Height = 34;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.Text = "Search patient...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tablePanel.Controls.Add(txtSearch);

            // NEW REQUISITION BUTTON
            Button reqBtn = new Button();
            reqBtn.Text = "+ New Requisition";
            reqBtn.Size = new Size(180, 40);
            reqBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            reqBtn.BackColor = brickRed;
            reqBtn.ForeColor = Color.White;
            reqBtn.FlatStyle = FlatStyle.Flat;
            reqBtn.FlatAppearance.BorderSize = 0;
            reqBtn.Font = new Font("Segoe UI Semibold", 10);
            reqBtn.Cursor = Cursors.Hand;
            reqBtn.Click += (s, e) => NavigateToMenu("New Requisition");
            tablePanel.Controls.Add(reqBtn);

            Action positionControls = () =>
            {
                reqBtn.Location = new Point(tablePanel.Width - 200, 16);
                txtSearch.Location = new Point(tablePanel.Width - 430, 19);
            };
            tablePanel.Resize += (s, e) => positionControls();
            positionControls();

            // =========================================================
            // DATAGRIDVIEW - Load from database
            // =========================================================
            DataGridView dgv = new DataGridView();
            dgv.Location = new Point(22, 72);
            dgv.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 48;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 11);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(55, 65, 81);

            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 226, 226);
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;
            dgv.RowTemplate.Height = 52;
            dgv.GridColor = Color.FromArgb(229, 231, 235);
            dgv.ScrollBars = ScrollBars.Vertical;
            tablePanel.Controls.Add(dgv);

            tablePanel.Resize += (s, e) => { dgv.Size = new Size(tablePanel.Width - 44, tablePanel.Height - 80); };
            dgv.Size = new Size(tablePanel.Width - 44, tablePanel.Height - 80);

            // COLUMNS
            dgv.Columns.Add("id", "Request ID");
            dgv.Columns.Add("patient", "Patient");
            dgv.Columns.Add("blood", "Blood Group");
            dgv.Columns.Add("status", "Status");
            dgv.Columns.Add("date", "Request Date");
            dgv.Columns.Add("actions", "Actions");
            dgv.Columns["actions"].ReadOnly = true;
            dgv.Columns["actions"].FillWeight = 130;

            // ✅ LOAD DATA FROM DATABASE (Doctor's own requisitions only)
            var allData = new List<string[]>();

            if (doctorRequisitions != null && doctorRequisitions.Rows.Count > 0)
            {
                foreach (DataRow row in doctorRequisitions.Rows)
                {
                    allData.Add(new string[]
                    {
                        row["RequisitionNumber"]?.ToString() ?? "N/A",
                        row["PatientName"]?.ToString() ?? "N/A",
                        row["BloodGroup"]?.ToString() ?? "N/A",
                        row["Status"]?.ToString() ?? "Pending",
                        Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy"),
                        ""
                    });
                }
            }
            else
            {
                allData.Add(new string[] { "---", "No requests found", "---", "---", "---", "" });
            }

            // Filter function
            Action<string> filterData = (searchText) =>
            {
                dgv.Rows.Clear();
                var filtered = string.IsNullOrWhiteSpace(searchText) || searchText == "Search patient..."
                    ? allData
                    : allData.Where(r => r[1].ToLower().Contains(searchText.ToLower()) ||
                                        r[0].ToLower().Contains(searchText.ToLower())).ToList();

                foreach (var row in filtered)
                    dgv.Rows.Add(row);
            };

            filterData("");

            // Search event
            txtSearch.TextChanged += (s, e) => filterData(txtSearch.Text);
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Search patient...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search patient...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            int selectedColumnIndex = -1;

            // STATUS COLOR + CUSTOM PAINT
            dgv.CellPainting += (s, e) =>
            {
                // Status column color
                if (e.ColumnIndex == 3 && e.RowIndex >= 0)
                {
                    e.PaintBackground(e.CellBounds, true);
                    string status = e.Value?.ToString();
                    Color foreColor = status == "Approved" ? Color.FromArgb(34, 197, 94) :
                                     status == "Pending" ? Color.FromArgb(245, 158, 11) :
                                     status == "Cross Matching" ? Color.FromArgb(59, 130, 246) :
                                     status == "Rejected" ? Color.FromArgb(220, 38, 38) :
                                     Color.FromArgb(55, 65, 81);

                    TextRenderer.DrawText(e.Graphics, status,
                        new Font("Segoe UI Semibold", 10), e.CellBounds, foreColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                    e.Handled = true;
                }
                // Actions buttons
                else if (e.ColumnIndex == 5 && e.RowIndex >= 0)
                {
                    e.PaintBackground(e.CellBounds, true);

                    int btnW = 70;
                    int btnH = 30;
                    int gap = 8;
                    int totalW = btnW * 2 + gap;
                    int startX = e.CellBounds.Left + (e.CellBounds.Width - totalW) / 2;
                    int startY = e.CellBounds.Top + (e.CellBounds.Height - btnH) / 2;

                    Rectangle viewRect = new Rectangle(startX, startY, btnW, btnH);
                    using (SolidBrush br = new SolidBrush(brickRed))
                        e.Graphics.FillRectangle(br, viewRect);
                    TextRenderer.DrawText(e.Graphics, "View",
                        new Font("Segoe UI Semibold", 9), viewRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    Rectangle modRect = new Rectangle(startX + btnW + gap, startY, btnW, btnH);
                    using (SolidBrush br = new SolidBrush(brickRed))
                        e.Graphics.FillRectangle(br, modRect);
                    TextRenderer.DrawText(e.Graphics, "Status",
                        new Font("Segoe UI Semibold", 9), modRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    e.Handled = true;
                }
                // Header paint
                else if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                {
                    bool isSelectedColumn = (e.ColumnIndex == selectedColumnIndex);
                    Color backColor = isSelectedColumn ? brickRedLight : Color.FromArgb(248, 250, 252);
                    Color foreColor = isSelectedColumn ? brickRed : Color.FromArgb(55, 65, 81);

                    using (SolidBrush backBrush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(backBrush, e.CellBounds);
                    }

                    TextRenderer.DrawText(e.Graphics, e.FormattedValue.ToString(),
                        new Font("Segoe UI Semibold", 11), e.CellBounds, foreColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                    e.Handled = true;
                }
                else if (e.RowIndex >= 0)
                {
                    e.PaintBackground(e.ClipBounds, true);
                    e.PaintContent(e.ClipBounds);
                    e.Handled = true;
                }
            };

            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    selectedColumnIndex = e.ColumnIndex;
                    dgv.Invalidate();
                }

                if (e.ColumnIndex == 5 && e.RowIndex >= 0)
                {
                    Rectangle cellBounds = dgv.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    Point mousePos = dgv.PointToClient(Cursor.Position);

                    int btnW = 70;
                    int btnH = 30;
                    int gap = 8;
                    int totalW = btnW * 2 + gap;
                    int startX = cellBounds.Left + (cellBounds.Width - totalW) / 2;
                    int startY = cellBounds.Top + (cellBounds.Height - btnH) / 2;

                    Rectangle viewRect = new Rectangle(startX, startY, btnW, btnH);
                    Rectangle modRect = new Rectangle(startX + btnW + gap, startY, btnW, btnH);

                    string reqId = dgv.Rows[e.RowIndex].Cells["id"].Value?.ToString();

                    if (viewRect.Contains(mousePos))
                    {
                        MessageBox.Show($"Requisition: {reqId}\n\nPatient: {dgv.Rows[e.RowIndex].Cells["patient"].Value}\nBlood Group: {dgv.Rows[e.RowIndex].Cells["blood"].Value}\nStatus: {dgv.Rows[e.RowIndex].Cells["status"].Value}",
                            "View Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (modRect.Contains(mousePos))
                    {
                        string currentStatus = dgv.Rows[e.RowIndex].Cells["status"].Value?.ToString() ?? "Pending";
                        MessageBox.Show($"Current Status: {currentStatus}\n\nYou can update status from the Requisition Status page.",
                            "Status Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };
        }
    }
}