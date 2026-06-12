using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class NotificationsUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private FlowLayoutPanel notificationsPanel;
        private Panel statsPanel;
        private Panel headerPanel;
        private ComboBox cmbFilter;
        private Button btnRefresh;
        private Label lblStatus;
        private DataTable notificationsData;
        private int currentUserId;

        public NotificationsUC()
        {
            InitializeComponent();
            currentUserId = SessionManager.CurrentUserID;
            BuildUI();
            LoadNotificationsFromDatabase();

            // Refresh every 30 seconds
            Timer refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) => LoadNotificationsFromDatabase();
            refreshTimer.Start();
        }

        // =========================================================
        // LOAD NOTIFICATIONS FROM DATABASE
        // =========================================================
        private void LoadNotificationsFromDatabase()
        {
            try
            {
                notificationsData = NotificationDAL.GetByUser(currentUserId);

                if (notificationsData == null || notificationsData.Rows.Count == 0)
                {
                    lblStatus.Text = "📭 No notifications found";
                    lblStatus.ForeColor = Color.Gray;
                    notificationsPanel.Controls.Clear();

                    Panel emptyCard = CreateEmptyNotificationCard();
                    notificationsPanel.Controls.Add(emptyCard);
                    UpdateStatsCards();
                    return;
                }

                UpdateStatsCards();
                FilterNotifications();

                // Update notification badge
                int unreadCount = NotificationDAL.GetUnreadCount(currentUserId);
                lblStatus.Text = $"📋 {notificationsData.Rows.Count} notification(s) ({unreadCount} unread)";
                lblStatus.ForeColor = unreadCount > 0 ? Color.Orange : Color.Green;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadNotifications Error: {ex.Message}");
                lblStatus.Text = "❌ Error loading notifications";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void MarkAsRead(int notificationId, Button btn, Panel card)
        {
            try
            {
                bool success = NotificationDAL.MarkAsRead(notificationId);
                if (success)
                {
                    // Update in DataTable
                    foreach (DataRow row in notificationsData.Rows)
                    {
                        if (Convert.ToInt32(row["NotificationID"]) == notificationId)
                        {
                            row["IsRead"] = true;
                            break;
                        }
                    }

                    btn.Text = "Read";
                    btn.Enabled = false;
                    btn.BackColor = Color.Gray;
                    card.BackColor = Color.White;

                    UpdateStatsCards();
                    FilterNotifications();

                    int unreadCount = NotificationDAL.GetUnreadCount(currentUserId);
                    lblStatus.Text = $"📋 {notificationsData.Rows.Count} notification(s) ({unreadCount} unread)";
                    lblStatus.ForeColor = unreadCount > 0 ? Color.Orange : Color.Green;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkAsRead Error: {ex.Message}");
            }
        }

        private void MarkAllAsRead()
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Mark all notifications as read?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool success = NotificationDAL.MarkAllAsRead(currentUserId);
                    if (success)
                    {
                        foreach (DataRow row in notificationsData.Rows)
                            row["IsRead"] = true;

                        FilterNotifications();
                        UpdateStatsCards();

                        int unreadCount = NotificationDAL.GetUnreadCount(currentUserId);
                        lblStatus.Text = $"📋 {notificationsData.Rows.Count} notification(s) ({unreadCount} unread)";
                        lblStatus.ForeColor = unreadCount > 0 ? Color.Orange : Color.Green;

                        MessageBox.Show("All notifications marked as read!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkAllAsRead Error: {ex.Message}");
            }
        }

        private void DeleteNotification(int notificationId)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Delete this notification?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    bool success = NotificationDAL.Delete(notificationId);
                    if (success)
                        LoadNotificationsFromDatabase();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteNotification Error: {ex.Message}");
            }
        }

        private void ClearAllNotifications()
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete ALL notifications?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    bool success = NotificationDAL.ClearAllNotifications(currentUserId);
                    if (success)
                        LoadNotificationsFromDatabase();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearAllNotifications Error: {ex.Message}");
            }
        }

        // =========================================================
        // BUILD UI
        // =========================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(5, 3, 5, 1);

            // HEADER
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 110;
            this.Controls.Add(headerPanel);

            Label title = new Label();
            title.Text = "🔔 Notifications";
            title.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(31, 41, 55);
            title.AutoSize = true;
            title.Location = new Point(0, 5);
            headerPanel.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Stay updated with your blood request activities and system alerts.";
            subtitle.Font = new Font("Segoe UI", 10);
            subtitle.ForeColor = Color.Gray;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(3, 50);
            headerPanel.Controls.Add(subtitle);

            // Filter Panel
            Panel filterPanel = new Panel();
            filterPanel.Location = new Point(0, 75);
            filterPanel.Height = 32;
            filterPanel.Width = headerPanel.Width;
            filterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            headerPanel.Controls.Add(filterPanel);

            Label lblFilter = new Label();
            lblFilter.Text = "Filter:";
            lblFilter.Location = new Point(0, 8);
            lblFilter.AutoSize = true;
            filterPanel.Controls.Add(lblFilter);

            cmbFilter = new ComboBox();
            cmbFilter.Location = new Point(45, 4);
            cmbFilter.Size = new Size(130, 28);
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Font = new Font("Segoe UI", 9);
            cmbFilter.Items.AddRange(new string[] { "All", "Unread", "Read", "Success", "Info", "Warning", "Emergency" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => FilterNotifications();
            filterPanel.Controls.Add(cmbFilter);

            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Location = new Point(190, 2);
            btnRefresh.Size = new Size(95, 30);
            btnRefresh.BackColor = Color.FromArgb(120, 22, 27);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadNotificationsFromDatabase();
            ApplyRoundedCorners(btnRefresh);
            filterPanel.Controls.Add(btnRefresh);

            Button btnMarkRead = new Button();
            btnMarkRead.Text = "✓ Mark All Read";
            btnMarkRead.Location = new Point(300, 2);
            btnMarkRead.Size = new Size(110, 30);
            btnMarkRead.BackColor = Color.FromArgb(59, 130, 246);
            btnMarkRead.ForeColor = Color.White;
            btnMarkRead.FlatStyle = FlatStyle.Flat;
            btnMarkRead.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnMarkRead.Cursor = Cursors.Hand;
            btnMarkRead.FlatAppearance.BorderSize = 0;
            btnMarkRead.Click += (s, e) => MarkAllAsRead();
            ApplyRoundedCorners(btnMarkRead);
            filterPanel.Controls.Add(btnMarkRead);

            Button btnClear = new Button();
            btnClear.Text = "🗑️ Clear All";
            btnClear.Location = new Point(425, 2);
            btnClear.Size = new Size(100, 30);
            btnClear.BackColor = brickRed;
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClear.Cursor = Cursors.Hand;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearAllNotifications();
            ApplyRoundedCorners(btnClear);
            filterPanel.Controls.Add(btnClear);

            // STATS CARDS
            statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 120;
            statsPanel.Padding = new Padding(0, 10, 0, 10);
            this.Controls.Add(statsPanel);

            // Status Label
            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 25;
            lblStatus.Text = "Loading notifications...";
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.Padding = new Padding(0, 5, 0, 0);
            this.Controls.Add(lblStatus);

            // NOTIFICATIONS PANEL
            notificationsPanel = new FlowLayoutPanel();
            notificationsPanel.Dock = DockStyle.Fill;
            notificationsPanel.AutoScroll = true;
            notificationsPanel.FlowDirection = FlowDirection.TopDown;
            notificationsPanel.WrapContents = false;
            notificationsPanel.Padding = new Padding(2, 5, 2, 10);
            notificationsPanel.BackColor = Color.Transparent;
            this.Controls.Add(notificationsPanel);
        }

        private void ApplyRoundedCorners(Button btn)
        {
            btn.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 8;
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(btn.Width - d, 0, d, d, 270, 90);
                path.AddArc(btn.Width - d, btn.Height - d, d, d, 0, 90);
                path.AddArc(0, btn.Height - d, d, d, 90, 90);
                btn.Region = new Region(path);
            };
        }

        private void ApplyRoundedCornersToPanel(Panel panel)
        {
            panel.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 10;
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(panel.Width - d, 0, d, d, 270, 90);
                path.AddArc(panel.Width - d, panel.Height - d, d, d, 0, 90);
                path.AddArc(0, panel.Height - d, d, d, 90, 90);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawPath(pen, path);
            };
        }

        private void UpdateStatsCards()
        {
            if (statsPanel == null) return;
            statsPanel.Controls.Clear();

            int total = 0, unread = 0, read = 0;

            if (notificationsData != null)
            {
                total = notificationsData.Rows.Count;
                foreach (DataRow row in notificationsData.Rows)
                {
                    bool isRead = Convert.ToBoolean(row["IsRead"]);
                    if (isRead) read++;
                    else unread++;
                }
            }

            var stats = new (string title, string value, Color color)[]
            {
                ("Total", total.ToString(), brickRed),
                ("Unread", unread.ToString(), Color.FromArgb(245, 158, 11)),
                ("Read", read.ToString(), Color.FromArgb(34, 197, 94))
            };

            int cardWidth = (statsPanel.Width - 60) / 3;
            if (cardWidth < 180) cardWidth = 180;

            for (int i = 0; i < stats.Length; i++)
            {
                Panel card = new Panel();
                card.BackColor = Color.White;
                card.Size = new Size(cardWidth, 95);
                card.Location = new Point(i * (cardWidth + 15), 10);
                ApplyRoundedCornersToPanel(card);
                statsPanel.Controls.Add(card);

                Label lblTitle = new Label();
                lblTitle.Text = stats[i].title;
                lblTitle.Font = new Font("Segoe UI Semibold", 10);
                lblTitle.ForeColor = Color.Gray;
                lblTitle.Dock = DockStyle.Top;
                lblTitle.Height = 30;
                lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(lblTitle);

                Label lblValue = new Label();
                lblValue.Text = stats[i].value;
                lblValue.Font = new Font("Segoe UI", 26, FontStyle.Bold);
                lblValue.ForeColor = stats[i].color;
                lblValue.Dock = DockStyle.Fill;
                lblValue.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(lblValue);
            }

            statsPanel.Resize += (s, e) =>
            {
                int newWidth = (statsPanel.Width - 60) / 3;
                if (newWidth < 180) newWidth = 180;
                for (int i = 0; i < statsPanel.Controls.Count; i++)
                {
                    statsPanel.Controls[i].Width = newWidth;
                    statsPanel.Controls[i].Location = new Point(i * (newWidth + 15), 10);
                }
            };
        }

        private void FilterNotifications()
        {
            if (notificationsPanel == null || notificationsData == null) return;

            string filter = cmbFilter.SelectedItem?.ToString() ?? "All";

            notificationsPanel.SuspendLayout();
            notificationsPanel.Controls.Clear();

            int cardWidth = notificationsPanel.ClientSize.Width - 25;
            if (cardWidth < 500) cardWidth = 500;
            if (cardWidth > 900) cardWidth = 900;

            int count = 0;
            foreach (DataRow row in notificationsData.Rows)
            {
                string title = row["Title"]?.ToString() ?? "";
                string message = row["Message"]?.ToString() ?? "";
                string type = row["Type"]?.ToString() ?? "General";
                bool isRead = Convert.ToBoolean(row["IsRead"]);
                DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);
                int notificationId = Convert.ToInt32(row["NotificationID"]);

                if (filter != "All")
                {
                    if (filter == "Unread" && isRead) continue;
                    if (filter == "Read" && !isRead) continue;
                    if (filter != "Unread" && filter != "Read" && filter != type) continue;
                }

                Panel card = CreateNotificationCard(title, message, type, isRead, createdAt, notificationId, cardWidth);
                notificationsPanel.Controls.Add(card);
                count++;
            }

            if (count == 0)
            {
                Panel emptyCard = CreateEmptyNotificationCard();
                notificationsPanel.Controls.Add(emptyCard);
            }

            notificationsPanel.ResumeLayout();
        }

        private Panel CreateEmptyNotificationCard()
        {
            Panel card = new Panel();
            card.Width = 600;
            card.Height = 150;
            card.BackColor = Color.White;
            card.Margin = new Padding(0, 20, 0, 10);
            ApplyRoundedCornersToPanel(card);

            Label lblEmpty = new Label();
            lblEmpty.Text = "📭 No notifications found\n\nWhen you receive notifications, they will appear here.";
            lblEmpty.Font = new Font("Segoe UI", 12);
            lblEmpty.ForeColor = Color.Gray;
            lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
            lblEmpty.Dock = DockStyle.Fill;
            card.Controls.Add(lblEmpty);

            return card;
        }

        private Panel CreateNotificationCard(string title, string message, string type, bool isRead,
            DateTime createdAt, int notificationId, int width)
        {
            Color borderColor;
            Color bgColor;
            string icon;

            switch (type)
            {
                case "Success":
                    borderColor = Color.FromArgb(34, 197, 94);
                    bgColor = Color.FromArgb(240, 253, 244);
                    icon = "✅";
                    break;
                case "Warning":
                    borderColor = Color.FromArgb(245, 158, 11);
                    bgColor = Color.FromArgb(254, 252, 232);
                    icon = "⚠️";
                    break;
                case "Emergency":
                    borderColor = Color.FromArgb(220, 38, 38);
                    bgColor = Color.FromArgb(254, 242, 242);
                    icon = "🚨";
                    break;
                case "Info":
                    borderColor = Color.FromArgb(59, 130, 246);
                    bgColor = Color.FromArgb(239, 246, 255);
                    icon = "ℹ️";
                    break;
                default:
                    borderColor = brickRed;
                    bgColor = Color.White;
                    icon = "📢";
                    break;
            }

            Panel card = new Panel();
            card.Width = width;
            card.Height = 100;
            card.BackColor = bgColor;
            card.Margin = new Padding(0, 0, 0, 8);
            card.Tag = notificationId;
            ApplyRoundedCornersToPanel(card);

            // Left color border
            Panel leftBorder = new Panel();
            leftBorder.Width = 5;
            leftBorder.Dock = DockStyle.Left;
            leftBorder.BackColor = borderColor;
            card.Controls.Add(leftBorder);

            // Icon
            Label lblIcon = new Label();
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI Emoji", 18);
            lblIcon.Location = new Point(15, 35);
            lblIcon.Size = new Size(40, 40);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(lblIcon);

            // Title
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(34, 34, 34);
            lblTitle.Location = new Point(65, 12);
            lblTitle.AutoSize = true;
            card.Controls.Add(lblTitle);

            // Message
            string shortMsg = message.Length > 80 ? message.Substring(0, 77) + "..." : message;
            Label lblMessage = new Label();
            lblMessage.Text = shortMsg;
            lblMessage.Font = new Font("Segoe UI", 9);
            lblMessage.ForeColor = Color.FromArgb(80, 80, 90);
            lblMessage.Location = new Point(65, 38);
            lblMessage.AutoSize = true;
            lblMessage.MaximumSize = new Size(width - 200, 40);
            card.Controls.Add(lblMessage);

            // Time
            Label lblTime = new Label();
            string timeAgo = GetTimeAgo(createdAt);
            lblTime.Text = $"🕐 {timeAgo}";
            lblTime.Font = new Font("Segoe UI", 8);
            lblTime.ForeColor = Color.Gray;
            lblTime.Location = new Point(65, 65);
            lblTime.AutoSize = true;
            card.Controls.Add(lblTime);

            // Unread indicator
            if (!isRead)
            {
                Panel unreadDot = new Panel();
                unreadDot.Size = new Size(10, 10);
                unreadDot.Location = new Point(width - 25, 12);
                unreadDot.BackColor = brickRed;
                unreadDot.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush br = new SolidBrush(brickRed))
                        e.Graphics.FillEllipse(br, 0, 0, 10, 10);
                };
                card.Controls.Add(unreadDot);
            }

            // Mark Read Button
            Button btnMarkRead = new Button();
            btnMarkRead.Text = isRead ? "Read" : "Mark Read";
            btnMarkRead.Size = new Size(85, 28);
            btnMarkRead.Location = new Point(width - 100, 60);
            btnMarkRead.BackColor = isRead ? Color.Gray : borderColor;
            btnMarkRead.ForeColor = Color.White;
            btnMarkRead.FlatStyle = FlatStyle.Flat;
            btnMarkRead.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnMarkRead.Cursor = Cursors.Hand;
            btnMarkRead.FlatAppearance.BorderSize = 0;
            btnMarkRead.Enabled = !isRead;
            ApplyRoundedCorners(btnMarkRead);
            btnMarkRead.Click += (s, e) => MarkAsRead(notificationId, btnMarkRead, card);
            card.Controls.Add(btnMarkRead);

            // Delete Button
            Button btnDelete = new Button();
            btnDelete.Text = "🗑️";
            btnDelete.Size = new Size(30, 28);
            btnDelete.Location = new Point(width - 35, 15);
            btnDelete.BackColor = Color.Transparent;
            btnDelete.ForeColor = Color.Gray;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Segoe UI", 9);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) => DeleteNotification(notificationId);
            card.Controls.Add(btnDelete);

            return card;
        }

        private string GetTimeAgo(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
            return date.ToString("dd MMM yyyy");
        }
    }
}