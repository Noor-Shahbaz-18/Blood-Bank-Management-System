using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using DonorModel = BloodBankManagementSystem.Classes.Models.Donor;

namespace BloodBankManagementSystem.Forms.Donor
{
    public class DonationHistory : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private Color brickRed = Color.FromArgb(120, 22, 27);
        private FlowLayoutPanel contentPanel;
        private Label lblStatus;

        public DonationHistory()
        {
            this.Text = "Blood Donor - Donation History";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 244, 244);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            BuildMainContent();
            BuildSidebar();
            LoadDonationHistory();
        }

        // =====================================================
        // LOAD DATA
        // =====================================================
        private void LoadDonationHistory()
        {
            try
            {
                contentPanel.Controls.Clear();

                int userID = SessionManager.CurrentUserID;
                DonorModel donor = DonorDAL.GetDonorByUserID(userID);

                if (donor == null)
                {
                    ShowMessage("No donation records found. Please complete your donor profile first.");
                    return;
                }

                // Try by DonorID first, then fallback to name
                DataTable dt = DonationHistoryDAL.GetByDonorID(donor.DonorID);
                if (dt == null || dt.Rows.Count == 0)
                    dt = DonationHistoryDAL.GetByDonorName(donor.FullName);

                if (dt == null || dt.Rows.Count == 0)
                {
                    ShowMessage($"No donation history found for {donor.FullName}.\n\nYour donations will appear here once you complete a donation.");

                    Button btnAddTest = new Button();
                    btnAddTest.Text = "➕ Add Test Donation (For Demo)";
                    btnAddTest.Font = new Font("Segoe UI", 12);
                    btnAddTest.BackColor = brickRed;
                    btnAddTest.ForeColor = Color.White;
                    btnAddTest.FlatStyle = FlatStyle.Flat;
                    btnAddTest.FlatAppearance.BorderSize = 0;
                    btnAddTest.Size = new Size(270, 48);
                    btnAddTest.Cursor = Cursors.Hand;
                    btnAddTest.Margin = new Padding(0, 12, 0, 0);
                    btnAddTest.Click += (s, e) => AddTestDonation(donor);
                    contentPanel.Controls.Add(btnAddTest);
                    return;
                }

                UpdateDonorInfo(donor);

                int donationNumber = dt.Rows.Count;

                foreach (DataRow row in dt.Rows)
                {
                    string donationDate = Convert.ToDateTime(row["DonationDate"]).ToString("dd MMM yyyy");
                    string location = row["DonationLocation"].ToString();
                    string units = row["Units"].ToString() + " Unit" + (Convert.ToInt32(row["Units"]) > 1 ? "s" : "");
                    int donationId = Convert.ToInt32(row["DonationID"]);
                    string bloodGroup = row["BloodGroup"].ToString();
                    string bagId = row["BagID"]?.ToString() ?? "";
                    bool certIssued = Convert.ToBoolean(row["CertificateIssued"]);

                    Panel card = CreateHistoryCard(donationDate, location, units, donationId, donationNumber, bloodGroup, bagId, certIssued);
                    card.Margin = new Padding(0, 0, 0, 14);
                    contentPanel.Controls.Add(card);
                    donationNumber--;
                }

                lblStatus.Text = $"✅ Total Donations: {dt.Rows.Count}";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading donation history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Error loading donation history";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void ShowMessage(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 13);
            lbl.ForeColor = Color.Gray;
            lbl.AutoSize = false;
            lbl.Size = new Size(700, 80);
            lbl.Margin = new Padding(0, 20, 0, 0);
            contentPanel.Controls.Add(lbl);
        }

        private void AddTestDonation(DonorModel donor)
        {
            try
            {
                bool added = DonationHistoryDAL.InsertDonation(
                    donor.DonorID,
                    donor.FullName,
                    donor.BloodGroup ?? "O+",
                    DateTime.Now,
                    "City Blood Bank, Lahore",
                    1,
                    450,
                    $"TEST-{DateTime.Now:yyyyMMdd}"
                );

                if (added)
                {
                    MessageBox.Show("✅ Test donation added successfully! Click OK to refresh.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDonationHistory();
                }
                else
                {
                    MessageBox.Show("Failed to add test donation.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // UPDATE TOP BAR DONOR INFO
        // =====================================================
        private void UpdateDonorInfo(DonorModel donor)
        {
            foreach (Control c1 in mainPanel.Controls)
            {
                if (c1.Name != "topBar") continue;
                foreach (Control c2 in c1.Controls)
                {
                    if (c2.Name != "userInfoPanel") continue;
                    foreach (Control lbl in c2.Controls)
                    {
                        if (lbl.Name == "lblUserName")
                            lbl.Text = donor.FullName;
                        else if (lbl.Name == "lblBloodGroup")
                            lbl.Text = $"Blood Group : {donor.BloodGroup ?? "Not Set"}";
                    }
                }
            }
        }

        // =====================================================
        // HISTORY CARD
        // =====================================================
        private Panel CreateHistoryCard(string date, string location, string units,
            int donationId, int donationNumber, string bloodGroup, string bagId, bool certificateIssued)
        {
            Panel card = new Panel();
            card.Size = new Size(860, 110);
            card.BackColor = Color.White;
            card.Tag = donationId;
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                using (GraphicsPath path = RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12))
                    e.Graphics.DrawPath(pen, path);
            };

            // ── Number badge ──
            Panel numberBadge = new Panel();
            numberBadge.Size = new Size(50, 50);
            numberBadge.Location = new Point(15, 30);
            numberBadge.BackColor = brickRed;
            numberBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, 50, 50);
                    numberBadge.Region = new Region(gp);
                }
                TextRenderer.DrawText(e.Graphics, $"#{donationNumber}",
                    new Font("Segoe UI", 12, FontStyle.Bold),
                    new Rectangle(0, 0, 50, 50), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            card.Controls.Add(numberBadge);

            // ── Blood drop icon circle ──
            Panel calIcon = new Panel();
            calIcon.Size = new Size(44, 44);
            calIcon.Location = new Point(78, 33);
            calIcon.BackColor = Color.FromArgb(255, 235, 235);
            calIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, 44, 44);
                    calIcon.Region = new Region(gp);
                }
            };
            card.Controls.Add(calIcon);

            Label calLabel = new Label();
            calLabel.Text = "🩸";
            calLabel.Font = new Font("Segoe UI Emoji", 16);
            calLabel.Dock = DockStyle.Fill;
            calLabel.TextAlign = ContentAlignment.MiddleCenter;
            calIcon.Controls.Add(calLabel);

            // ── Text info ──
            Label dateLabel = new Label();
            dateLabel.Text = date;
            dateLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            dateLabel.ForeColor = Color.FromArgb(34, 34, 34);
            dateLabel.Location = new Point(135, 14);
            dateLabel.AutoSize = true;
            card.Controls.Add(dateLabel);

            Label bloodLabel = new Label();
            bloodLabel.Text = $"🩸 Blood Group: {bloodGroup}";
            bloodLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            bloodLabel.ForeColor = brickRed;
            bloodLabel.Location = new Point(135, 44);
            bloodLabel.AutoSize = true;
            card.Controls.Add(bloodLabel);

            Label locLabel = new Label();
            locLabel.Text = $"📍 {location}";
            locLabel.Font = new Font("Segoe UI", 10);
            locLabel.ForeColor = Color.FromArgb(110, 110, 110);
            locLabel.Location = new Point(135, 70);
            locLabel.AutoSize = true;
            card.Controls.Add(locLabel);

            // ── Units badge ──
            Panel unitBadge = new Panel();
            unitBadge.Size = new Size(85, 28);
            unitBadge.BackColor = Color.FromArgb(255, 235, 235);
            card.Controls.Add(unitBadge);

            Label unitLabel = new Label();
            unitLabel.Text = units;
            unitLabel.Font = new Font("Segoe UI Semibold", 9);
            unitLabel.ForeColor = brickRed;
            unitLabel.Dock = DockStyle.Fill;
            unitLabel.TextAlign = ContentAlignment.MiddleCenter;
            unitBadge.Controls.Add(unitLabel);

            // ── Certificate button ──
            Button downloadBtn = new Button();
            downloadBtn.Size = new Size(195, 38);
            downloadBtn.FlatStyle = FlatStyle.Flat;
            downloadBtn.FlatAppearance.BorderSize = 1;
            downloadBtn.Font = new Font("Segoe UI Semibold", 10);
            downloadBtn.Cursor = Cursors.Hand;

            if (certificateIssued)
            {
                downloadBtn.Text = "📥 Download Certificate";
                downloadBtn.BackColor = Color.White;
                downloadBtn.ForeColor = brickRed;
                downloadBtn.FlatAppearance.BorderColor = brickRed;
            }
            else
            {
                downloadBtn.Text = "📥 Generate Certificate";
                downloadBtn.BackColor = brickRed;
                downloadBtn.ForeColor = Color.White;
                downloadBtn.FlatAppearance.BorderColor = brickRed;
            }

            string donorName = GetDonorName();
            downloadBtn.Click += (s, e) =>
            {
                string donationNumberStr = $"DON-{donationId}-{DateTime.Now:yyyyMMdd}";
                if (!certificateIssued)
                    DonationHistoryDAL.MarkCertificateIssued(donationId);

                DonationCertificate certForm = new DonationCertificate(donorName, date, bloodGroup, donationNumberStr);
                certForm.ShowDialog();
            };

            downloadBtn.MouseEnter += (s, e) =>
            {
                downloadBtn.BackColor = certificateIssued ? brickRed : Color.FromArgb(196, 7, 17);
                downloadBtn.ForeColor = Color.White;
            };
            downloadBtn.MouseLeave += (s, e) =>
            {
                if (certificateIssued) { downloadBtn.BackColor = Color.White; downloadBtn.ForeColor = brickRed; }
                else { downloadBtn.BackColor = brickRed; downloadBtn.ForeColor = Color.White; }
            };
            card.Controls.Add(downloadBtn);

            // Position right-side elements responsively
            void PositionRight()
            {
                unitBadge.Location = new Point(card.Width - 105, 18);
                downloadBtn.Location = new Point(card.Width - 210, 62);
            }
            PositionRight();
            card.Resize += (s, e) => PositionRight();

            return card;
        }

        private string GetDonorName()
        {
            DonorModel donor = DonorDAL.GetDonorByUserID(SessionManager.CurrentUserID);
            return donor?.FullName ?? "Valued Donor";
        }

        // =====================================================
        // SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = brickRed;
            sidebarPanel.Padding = new Padding(18, 20, 18, 25);

            // Logo card
            Panel logoContainer = new Panel();
            logoContainer.Dock = DockStyle.Top;
            logoContainer.Height = 130;
            logoContainer.BackColor = Color.White;
            logoContainer.Margin = new Padding(0, 0, 0, 10);
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

            // Menu
            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(0, 15, 0, 0);
            menuPanel.BackColor = Color.Transparent;

            AddDonorMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                this.Hide();
                new DonorDashboard().ShowDialog();
                this.Close();
            });

            AddDonorMenuItem(menuPanel, "📅", "Appointments", false, () =>
            {
                this.Hide();
                new BookAppointment().ShowDialog();
                this.Close();
            });

            AddDonorMenuItem(menuPanel, "📄", "Donation History", true, null);

            AddDonorMenuItem(menuPanel, "📍", "Nearby Camps", false, () =>
            {
                NearbyCamps campsForm = new NearbyCamps();
                campsForm.Owner = this;
                this.Hide();
                campsForm.FormClosed += (s, args) => this.Show();
                campsForm.Show();
            });

            AddDonorMenuItem(menuPanel, "🔔", "Notifications", false, () =>
            {
                DonorNotifications notifForm = new DonorNotifications();
                notifForm.Owner = this;
                this.Hide();
                notifForm.FormClosed += (s, args) => this.Show();
                notifForm.Show();
            });

            AddDonorMenuItem(menuPanel, "👤", "Profile Settings", false, () =>
            {
                DonorProfileSetting profileForm = new DonorProfileSetting();
                profileForm.Owner = this;
                this.Hide();
                profileForm.FormClosed += (s, args) => this.Show();
                profileForm.Show();
            });

            // Spacer
            Panel spacer = new Panel();
            spacer.Size = new Size(210, 1);
            spacer.Margin = new Padding(0, 20, 0, 0);
            spacer.BackColor = Color.Transparent;
            menuPanel.Controls.Add(spacer);

            AddDonorMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Logout Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Application.Exit();
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoContainer);
            this.Controls.Add(sidebarPanel);
        }

        private void AddDonorMenuItem(FlowLayoutPanel parent, string icon, string text, bool isActive, Action onClick)
        {
            Panel menuItem = new Panel();
            menuItem.Size = new Size(210, 46);
            menuItem.Margin = new Padding(0, 3, 0, 3);
            menuItem.Cursor = Cursors.Hand;
            menuItem.BackColor = isActive ? Color.FromArgb(50, Color.White) : Color.Transparent;

            menuItem.MouseEnter += (s, e) => menuItem.BackColor = Color.FromArgb(70, Color.White);
            menuItem.MouseLeave += (s, e) => menuItem.BackColor = isActive
                ? Color.FromArgb(50, Color.White) : Color.Transparent;

            menuItem.Click += (s, e) =>
            {
                menuItem.BackColor = Color.FromArgb(100, Color.White);
                Timer t = new Timer { Interval = 100 };
                t.Tick += (sender, args) =>
                {
                    t.Stop(); t.Dispose();
                    menuItem.BackColor = isActive ? Color.FromArgb(50, Color.White) : Color.Transparent;
                    onClick?.Invoke();
                };
                t.Start();
            };

            Label iconLabel = new Label();
            iconLabel.Text = icon;
            iconLabel.Font = new Font("Segoe UI Emoji", 14);
            iconLabel.Size = new Size(30, 46);
            iconLabel.Location = new Point(10, 0);
            iconLabel.TextAlign = ContentAlignment.MiddleCenter;
            iconLabel.BackColor = Color.Transparent;
            iconLabel.ForeColor = Color.White;
            iconLabel.Click += (s, e) => onClick?.Invoke();
            menuItem.Controls.Add(iconLabel);

            Label textLabel = new Label();
            textLabel.Text = text;
            textLabel.Font = new Font("Segoe UI Semibold", 11);
            textLabel.Size = new Size(155, 46);
            textLabel.Location = new Point(42, 0);
            textLabel.TextAlign = ContentAlignment.MiddleLeft;
            textLabel.BackColor = Color.Transparent;
            textLabel.ForeColor = Color.White;
            textLabel.Click += (s, e) => onClick?.Invoke();
            menuItem.Controls.Add(textLabel);

            if (isActive)
            {
                Panel indicator = new Panel();
                indicator.Size = new Size(3, 46);
                indicator.Location = new Point(0, 0);
                indicator.BackColor = Color.White;
                indicator.Click += (s, e) => onClick?.Invoke();
                menuItem.Controls.Add(indicator);
            }

            parent.Controls.Add(menuItem);
        }

        // =====================================================
        // MAIN CONTENT
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 244, 244);
            mainPanel.Padding = new Padding(20, 15, 20, 15);
            this.Controls.Add(mainPanel);

            // ── Top Bar ──
            Panel topBar = new Panel();
            topBar.Name = "topBar";
            topBar.Dock = DockStyle.Top;
            topBar.Height = 80;
            topBar.BackColor = Color.White;
            topBar.Padding = new Padding(18, 0, 18, 0);
            topBar.Paint += (s, e) =>
            {
                // bottom border shadow line
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawLine(pen, 0, topBar.Height - 1, topBar.Width, topBar.Height - 1);
            };
            mainPanel.Controls.Add(topBar);

            // User info (left)
            Panel userInfo = new Panel();
            userInfo.Name = "userInfoPanel";
            userInfo.Dock = DockStyle.Left;
            userInfo.Width = 520;
            userInfo.BackColor = Color.Transparent;
            topBar.Controls.Add(userInfo);

            Label lblUserName = new Label();
            lblUserName.Name = "lblUserName";
            lblUserName.Text = "Loading...";
            lblUserName.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblUserName.ForeColor = Color.FromArgb(34, 34, 34);
            lblUserName.Location = new Point(0, 10);
            lblUserName.AutoSize = true;
            userInfo.Controls.Add(lblUserName);

            Label lblBloodGroup = new Label();
            lblBloodGroup.Name = "lblBloodGroup";
            lblBloodGroup.Text = "Blood Group : Loading...";
            lblBloodGroup.Font = new Font("Segoe UI Semibold", 11);
            lblBloodGroup.ForeColor = brickRed;
            lblBloodGroup.Location = new Point(2, 44);
            lblBloodGroup.AutoSize = true;
            userInfo.Controls.Add(lblBloodGroup);

            // Status label (center-ish via Fill)
            lblStatus = new Label();
            lblStatus.Text = "Loading donation history...";
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            topBar.Controls.Add(lblStatus);

            // Right icons
            Panel iconsPanel = new Panel();
            iconsPanel.Dock = DockStyle.Right;
            iconsPanel.Width = 110;
            iconsPanel.BackColor = Color.Transparent;
            topBar.Controls.Add(iconsPanel);

            Button bellBtn = new Button();
            bellBtn.Text = "🔔";
            bellBtn.Size = new Size(44, 44);
            bellBtn.Location = new Point(5, 18);
            bellBtn.FlatStyle = FlatStyle.Flat;
            bellBtn.FlatAppearance.BorderSize = 0;
            bellBtn.BackColor = Color.White;
            bellBtn.Font = new Font("Segoe UI Emoji", 16);
            bellBtn.Cursor = Cursors.Hand;
            iconsPanel.Controls.Add(bellBtn);

            Panel avatarPanel = new Panel();
            avatarPanel.Size = new Size(44, 44);
            avatarPanel.Location = new Point(58, 18);
            avatarPanel.BackColor = brickRed;
            iconsPanel.Controls.Add(avatarPanel);
            avatarPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 44, 44);
                    avatarPanel.Region = new Region(path);
                }
                string initial = SessionManager.CurrentFullName?.Length > 0
                    ? SessionManager.CurrentFullName.Substring(0, 1) : "D";
                TextRenderer.DrawText(e.Graphics, initial,
                    new Font("Segoe UI", 16, FontStyle.Bold),
                    new Rectangle(0, 0, 44, 44), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // ── Page title strip (separate panel — never cleared) ──
            Panel titleStrip = new Panel();
            titleStrip.Name = "titleStrip";
            titleStrip.Dock = DockStyle.Top;
            titleStrip.Height = 72;
            titleStrip.BackColor = Color.FromArgb(244, 244, 244);
            mainPanel.Controls.Add(titleStrip);

            Label pageTitle = new Label();
            pageTitle.Text = "📄 Donation History";
            pageTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            pageTitle.ForeColor = Color.FromArgb(34, 34, 34);
            pageTitle.Location = new Point(5, 8);
            pageTitle.AutoSize = true;
            titleStrip.Controls.Add(pageTitle);

            Label pageSubtitle = new Label();
            pageSubtitle.Text = "Your past blood donation records";
            pageSubtitle.Font = new Font("Segoe UI", 11);
            pageSubtitle.ForeColor = Color.FromArgb(119, 119, 119);
            pageSubtitle.Location = new Point(7, 42);
            pageSubtitle.AutoSize = true;
            titleStrip.Controls.Add(pageSubtitle);

            // ── Scrollable cards panel ──
            contentPanel = new FlowLayoutPanel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.AutoScroll = true;
            contentPanel.FlowDirection = FlowDirection.TopDown;
            contentPanel.WrapContents = false;
            contentPanel.Padding = new Padding(5, 8, 5, 10);
            mainPanel.Controls.Add(contentPanel);

            // Correct docking order: Fill rendered first, then Tops from bottom-up
            mainPanel.Controls.SetChildIndex(contentPanel, 0);
            mainPanel.Controls.SetChildIndex(titleStrip, 1);
            mainPanel.Controls.SetChildIndex(topBar, 2);
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private GraphicsPath RoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}