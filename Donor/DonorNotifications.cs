using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonorNotifications : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private FlowLayoutPanel notificationsContainer;
        private Label lblStatus;
        private Button btnMarkAllRead, btnClearAll;
        private ComboBox cmbFilter;
        private Color brickRed = Color.FromArgb(120, 22, 27);
        private int currentDonorID = 0;
        private int currentUserID = 0;
        private DataTable allNotifications;

        public DonorNotifications()
        {
            this.Text = "Blood Donor - Notifications";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            LoadCurrentDonor();
            BuildMainContent();  // ✅ Fill wala pehle
            BuildSidebar();      // ✅ Left wala baad mein
            LoadNotifications();
        }

        private void LoadCurrentDonor()
        {
            try
            {
                currentUserID = SessionManager.CurrentUserID;
                var donor = DonorDAL.GetDonorByUserID(currentUserID);
                if (donor != null)
                    currentDonorID = donor.DonorID;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading donor: " + ex.Message);
            }
        }

        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = brickRed;
            sidebarPanel.Padding = new Padding(18, 20, 18, 25);

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

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(0, 15, 0, 0);
            menuPanel.BackColor = Color.Transparent;

            AddMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                DonorDashboard dashboard = new DonorDashboard();
                this.Hide();
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            });
            AddMenuItem(menuPanel, "📅", "Appointments", false, () =>
            {
                BookAppointment apptForm = new BookAppointment();
                this.Hide();
                apptForm.FormClosed += (s, args) => this.Show();
                apptForm.Show();
            });
            AddMenuItem(menuPanel, "📄", "Donation History", false, () =>
            {
                DonationHistory historyForm = new DonationHistory();
                this.Hide();
                historyForm.FormClosed += (s, args) => this.Show();
                historyForm.Show();
            });
            AddMenuItem(menuPanel, "📍", "Nearby Camps", false, () =>
            {
                NearbyCamps campsForm = new NearbyCamps();
                this.Hide();
                campsForm.FormClosed += (s, args) => this.Show();
                campsForm.Show();
            });
            AddMenuItem(menuPanel, "🔔", "Notifications", true, () => { });
            AddMenuItem(menuPanel, "👤", "Profile Settings", false, () =>
            {
                DonorProfileSetting profileForm = new DonorProfileSetting();
                this.Hide();
                profileForm.FormClosed += (s, args) => this.Show();
                profileForm.Show();
            });

            Panel spacer = new Panel();
            spacer.Size = new Size(210, 1);
            spacer.Margin = new Padding(0, 20, 0, 0);
            spacer.BackColor = Color.Transparent;
            menuPanel.Controls.Add(spacer);

            AddMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                if (MessageBox.Show("Are you sure you want to logout?", "Logout Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoContainer);
            this.Controls.Add(sidebarPanel); // ✅ Sidebar BAAD MEIN add
        }

        private void AddMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 46);
            item.Margin = new Padding(0, 3, 0, 3);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(50, Color.White) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.Size = new Size(30, 46);
            iconLbl.Location = new Point(10, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI Semibold", 11);
            textLbl.ForeColor = Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.Size = new Size(155, 46);
            textLbl.Location = new Point(42, 0);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

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

        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(245, 247, 250);
            mainPanel.Padding = new Padding(25);
            this.Controls.Add(mainPanel); // ✅ Fill wala PEHLE

            // ✅ FIX: WinForms DockStyle.Top order — ULTA add karo
            // Jo LAST add hoga wo UPAR dikhega, jo PEHLE add hoga wo NEECHE

            // STEP 1: Notifications container PEHLE add karo (yeh Fill lega)
            notificationsContainer = new FlowLayoutPanel();
            notificationsContainer.Dock = DockStyle.Fill;
            notificationsContainer.FlowDirection = FlowDirection.TopDown;
            notificationsContainer.WrapContents = false;
            notificationsContainer.AutoScroll = true;
            notificationsContainer.Padding = new Padding(5);
            notificationsContainer.BackColor = Color.Transparent;
            mainPanel.Controls.Add(notificationsContainer);

            // STEP 2: Status label
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 30;
            lblStatus.Text = "Loading notifications...";
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Padding = new Padding(5, 8, 0, 0);
            mainPanel.Controls.Add(lblStatus);

            // STEP 3: Filter Panel
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 55;
            filterPanel.BackColor = Color.White;
            filterPanel.Margin = new Padding(0, 10, 0, 10);
            filterPanel.Padding = new Padding(15, 0, 15, 0);
            MakeRounded(filterPanel, 10);
            mainPanel.Controls.Add(filterPanel);

            Label lblFilter = new Label();
            lblFilter.Text = "Filter:";
            lblFilter.Location = new Point(15, 16);
            lblFilter.AutoSize = true;
            lblFilter.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            filterPanel.Controls.Add(lblFilter);

            cmbFilter = new ComboBox();
            cmbFilter.Location = new Point(65, 13);
            cmbFilter.Width = 130;
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Items.AddRange(new string[] { "All", "Unread", "Read" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => FilterNotifications();
            filterPanel.Controls.Add(cmbFilter);

            btnMarkAllRead = new Button();
            btnMarkAllRead.Text = "✓ Mark All Read";
            btnMarkAllRead.Size = new Size(130, 32);
            btnMarkAllRead.BackColor = Color.FromArgb(59, 130, 246);
            btnMarkAllRead.ForeColor = Color.White;
            btnMarkAllRead.FlatStyle = FlatStyle.Flat;
            btnMarkAllRead.FlatAppearance.BorderSize = 0;
            btnMarkAllRead.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMarkAllRead.Cursor = Cursors.Hand;
            btnMarkAllRead.Click += (s, e) => MarkAllRead();
            filterPanel.Controls.Add(btnMarkAllRead);

            btnClearAll = new Button();
            btnClearAll.Text = "🗑️ Clear All";
            btnClearAll.Size = new Size(100, 32);
            btnClearAll.BackColor = Color.FromArgb(220, 38, 38);
            btnClearAll.ForeColor = Color.White;
            btnClearAll.FlatStyle = FlatStyle.Flat;
            btnClearAll.FlatAppearance.BorderSize = 0;
            btnClearAll.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClearAll.Cursor = Cursors.Hand;
            btnClearAll.Click += (s, e) => ClearAllNotifications();
            filterPanel.Controls.Add(btnClearAll);

            // Buttons ko right side mein position karo
            filterPanel.Resize += (s, e) =>
            {
                btnClearAll.Location = new Point(filterPanel.Width - 115, 11);
                btnMarkAllRead.Location = new Point(filterPanel.Width - 255, 11);
            };
            filterPanel.HandleCreated += (s, e) =>
            {
                btnClearAll.Location = new Point(filterPanel.Width - 115, 11);
                btnMarkAllRead.Location = new Point(filterPanel.Width - 255, 11);
            };

            // STEP 4: Header LAST add karo (yeh sabse upar dikhega)
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 90;
            headerPanel.BackColor = Color.White;
            headerPanel.Padding = new Padding(20, 10, 20, 10);
            MakeRounded(headerPanel, 12);
            mainPanel.Controls.Add(headerPanel);

            Label lblTitle = new Label();
            lblTitle.Text = "🔔 Notifications";
            lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitle.ForeColor = brickRed;
            lblTitle.Location = new Point(10, 8);
            lblTitle.AutoSize = true;
            headerPanel.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Stay updated with your blood donation activities";
            lblSubtitle.Font = new Font("Segoe UI", 10);
            lblSubtitle.ForeColor = Color.Gray;
            lblSubtitle.Location = new Point(12, 48);
            lblSubtitle.AutoSize = true;
            headerPanel.Controls.Add(lblSubtitle);
        }

        private void LoadNotifications()
        {
            try
            {
                allNotifications = NotificationDAL.GetByUser(currentUserID);

                if (allNotifications == null || allNotifications.Rows.Count == 0)
                {
                    lblStatus.Text = "📭 No notifications found";
                    lblStatus.ForeColor = Color.Gray;

                    notificationsContainer.Controls.Clear();
                    Label lblNoData = new Label();
                    lblNoData.Text = "You have no notifications.\n\nWhen you donate blood or book appointments, you'll see notifications here.";
                    lblNoData.Font = new Font("Segoe UI", 14);
                    lblNoData.ForeColor = Color.Gray;
                    lblNoData.TextAlign = ContentAlignment.MiddleCenter;
                    lblNoData.Size = new Size(600, 100);
                    lblNoData.Margin = new Padding(0, 100, 0, 0);
                    notificationsContainer.Controls.Add(lblNoData);
                    return;
                }

                int unreadCount = NotificationDAL.GetUnreadCount(currentUserID);
                lblStatus.Text = $"📋 {allNotifications.Rows.Count} notifications ({unreadCount} unread)";
                lblStatus.ForeColor = unreadCount > 0 ? Color.Orange : Color.Green;

                DisplayNotifications(allNotifications);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading notifications: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Error loading notifications";
            }
        }

        private void DisplayNotifications(DataTable dt)
        {
            notificationsContainer.Controls.Clear();

            foreach (DataRow row in dt.Rows)
            {
                Panel card = CreateNotificationCard(row);
                notificationsContainer.Controls.Add(card);
            }
        }

        private Panel CreateNotificationCard(DataRow row)
        {
            string title = row["Title"].ToString();
            string message = row["Message"].ToString();
            string type = row["Type"].ToString();
            bool isRead = Convert.ToBoolean(row["IsRead"]);
            DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);
            int notificationID = Convert.ToInt32(row["NotificationID"]);

            Color borderColor;
            switch (type)
            {
                case "Success": borderColor = Color.FromArgb(34, 197, 94); break;
                case "Warning": borderColor = Color.FromArgb(245, 158, 11); break;
                case "Emergency": borderColor = Color.FromArgb(220, 38, 38); break;
                default: borderColor = Color.FromArgb(59, 130, 246); break;
            }

            Panel card = new Panel();
            card.Width = notificationsContainer.ClientSize.Width - 20;
            card.Height = 100;
            card.BackColor = Color.White;
            card.Margin = new Padding(0, 0, 0, 10);
            card.Tag = notificationID;
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                GraphicsPath path = new GraphicsPath();
                int radius = 10;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(card.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(card.Width - radius * 2, card.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, card.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
                using (SolidBrush bg = new SolidBrush(Color.White))
                    e.Graphics.FillPath(bg, path);
                using (Pen border = new Pen(Color.FromArgb(220, 220, 225), 1))
                    e.Graphics.DrawPath(border, path);
            };

            // Left color border
            Panel leftBorder = new Panel();
            leftBorder.Width = 5;
            leftBorder.Dock = DockStyle.Left;
            leftBorder.BackColor = borderColor;
            card.Controls.Add(leftBorder);

            string icon = type == "Success" ? "✅" : type == "Warning" ? "⚠️" : type == "Emergency" ? "🚨" : "ℹ️";

            Label lblIcon = new Label();
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI Emoji", 20);
            lblIcon.Location = new Point(20, 28);
            lblIcon.Size = new Size(40, 40);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(lblIcon);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(34, 34, 34);
            lblTitle.Location = new Point(72, 12);
            lblTitle.AutoSize = true;
            card.Controls.Add(lblTitle);

            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Font = new Font("Segoe UI", 9);
            lblMessage.ForeColor = Color.FromArgb(80, 80, 90);
            lblMessage.Location = new Point(72, 38);
            lblMessage.AutoSize = false;
            lblMessage.Size = new Size(500, 30);
            card.Controls.Add(lblMessage);

            Label lblTime = new Label();
            lblTime.Text = "🕐 " + GetTimeAgo(createdAt);
            lblTime.Font = new Font("Segoe UI", 8);
            lblTime.ForeColor = Color.Gray;
            lblTime.Location = new Point(72, 70);
            lblTime.AutoSize = true;
            card.Controls.Add(lblTime);

            // Unread dot
            Panel unreadDot = new Panel();
            unreadDot.Size = new Size(10, 10);
            unreadDot.BackColor = brickRed;
            unreadDot.Visible = !isRead;
            unreadDot.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(brickRed))
                    e.Graphics.FillEllipse(brush, 0, 0, 10, 10);
            };
            card.Controls.Add(unreadDot);

            if (!isRead)
            {
                Button btnMarkRead = new Button();
                btnMarkRead.Text = "Mark Read";
                btnMarkRead.Size = new Size(85, 28);
                btnMarkRead.BackColor = borderColor;
                btnMarkRead.ForeColor = Color.White;
                btnMarkRead.FlatStyle = FlatStyle.Flat;
                btnMarkRead.FlatAppearance.BorderSize = 0;
                btnMarkRead.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                btnMarkRead.Cursor = Cursors.Hand;
                btnMarkRead.Tag = notificationID;
                btnMarkRead.Click += (s, e) => MarkAsRead(notificationID);
                card.Controls.Add(btnMarkRead);

                card.Resize += (s, e) =>
                {
                    btnMarkRead.Location = new Point(card.Width - 100, 60);
                    unreadDot.Location = new Point(card.Width - 25, 12);
                };
                card.HandleCreated += (s, e) =>
                {
                    btnMarkRead.Location = new Point(card.Width - 100, 60);
                    unreadDot.Location = new Point(card.Width - 25, 12);
                };
            }
            else
            {
                card.Resize += (s, e) => unreadDot.Location = new Point(card.Width - 25, 12);
                card.HandleCreated += (s, e) => unreadDot.Location = new Point(card.Width - 25, 12);
            }

            // Card width resize
            notificationsContainer.Resize += (s, e) =>
            {
                card.Width = notificationsContainer.ClientSize.Width - 20;
            };

            return card;
        }

        private string GetTimeAgo(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
            return date.ToString("dd MMM yyyy");
        }

        private void FilterNotifications()
        {
            if (allNotifications == null) return;

            string filter = cmbFilter.SelectedItem.ToString();
            DataTable filtered = allNotifications.Clone();

            foreach (DataRow row in allNotifications.Rows)
            {
                bool isRead = Convert.ToBoolean(row["IsRead"]);
                bool match = filter == "All"
                    || (filter == "Unread" && !isRead)
                    || (filter == "Read" && isRead);

                if (match) filtered.ImportRow(row);
            }

            DisplayNotifications(filtered);
        }

        private void MarkAsRead(int notificationID)
        {
            if (NotificationDAL.MarkAsRead(notificationID))
                LoadNotifications();
        }

        private void MarkAllRead()
        {
            if (MessageBox.Show("Mark all notifications as read?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (NotificationDAL.MarkAllAsRead(currentUserID))
                    LoadNotifications();
            }
        }

        private void ClearAllNotifications()
        {
            if (MessageBox.Show("Are you sure you want to delete ALL notifications?\n\nThis action cannot be undone.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (NotificationDAL.ClearAllNotifications(currentUserID))
                {
                    LoadNotifications();
                    MessageBox.Show("All notifications cleared successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void MakeRounded(Control control, int radius)
        {
            control.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(control.Width - d, 0, d, d, 270, 90);
                path.AddArc(control.Width - d, control.Height - d, d, d, 0, 90);
                path.AddArc(0, control.Height - d, d, d, 90, 90);
                control.Region = new Region(path);
            };
        }

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