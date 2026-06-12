using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class DoctorNotifications : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private FlowLayoutPanel notificationsPanel;
        private Panel statsPanel;
        private Panel headerPanel;
        private DataTable allNotifications;
        private Timer refreshTimer;
        private Label lblNoNotifications;

        public DoctorNotifications()
        {
            InitializeComponent();
            BuildUI();
            LoadNotificationsFromDatabase();
            StartAutoRefresh();
        }

        private void LoadNotificationsFromDatabase()
        {
            try
            {
                string query = @"SELECT NotificationID, UserID, Title, Message, Type, Priority, 
                                IsRead, CreatedAt, ReadAt, RelatedID, RelatedType, SentBy
                                FROM Notifications 
                                WHERE UserID = @UserID
                                ORDER BY 
                                    CASE IsRead WHEN 0 THEN 0 ELSE 1 END,
                                    CASE Priority 
                                        WHEN 'Emergency' THEN 1 
                                        WHEN 'Urgent' THEN 2 
                                        WHEN 'Important' THEN 3 
                                        ELSE 4 
                                    END,
                                    CreatedAt DESC";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@UserID", SessionManager.CurrentUserID)
                };

                allNotifications = CommonDAL.ExecuteReader(query, parameters);
                UpdateStatsCards();
                LoadNotificationsToPanel();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading notifications from database", ex);
                MessageBox.Show($"Error loading notifications: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowNoNotificationsMessage();
            }
        }

        private void ShowNoNotificationsMessage()
        {
            if (notificationsPanel != null)
                notificationsPanel.Controls.Clear();

            if (lblNoNotifications != null)
            {
                notificationsPanel?.Controls.Add(lblNoNotifications);
                lblNoNotifications.Visible = true;
            }
        }

        private void HideNoNotificationsMessage()
        {
            if (lblNoNotifications != null)
                lblNoNotifications.Visible = false;
        }

        private void StartAutoRefresh()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 30000;
            refreshTimer.Tick += (s, e) =>
            {
                if (this.IsHandleCreated)
                    this.BeginInvoke(new Action(() => LoadNotificationsFromDatabase()));
            };
            refreshTimer.Start();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Padding = new Padding(30, 25, 30, 25);

            // ── HEADER ──────────────────────────────────────────────────────
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.Padding = new Padding(0);
            this.Controls.Add(headerPanel);

            Label title = new Label();
            title.Text = "Notifications";
            title.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(31, 41, 55);
            title.AutoSize = true;
            title.Location = new Point(0, 0);
            headerPanel.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Stay updated with alerts and recent activities.";
            subtitle.Font = new Font("Segoe UI", 10);
            subtitle.ForeColor = Color.Gray;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(2, 42);
            headerPanel.Controls.Add(subtitle);

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Font = new Font("Segoe UI Semibold", 10);
            btnRefresh.BackColor = Color.FromArgb(59, 130, 246);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Size = new Size(110, 40);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Click += (s, e) => LoadNotificationsFromDatabase();
            headerPanel.Controls.Add(btnRefresh);

            Button btnMarkRead = new Button();
            btnMarkRead.Text = "Mark All as Read";
            btnMarkRead.Font = new Font("Segoe UI Semibold", 10);
            btnMarkRead.BackColor = Color.White;
            btnMarkRead.ForeColor = Color.FromArgb(55, 65, 81);
            btnMarkRead.FlatStyle = FlatStyle.Flat;
            btnMarkRead.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnMarkRead.Size = new Size(155, 40);
            btnMarkRead.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMarkRead.Click += BtnMarkRead_Click;
            headerPanel.Controls.Add(btnMarkRead);

            Button btnClear = new Button();
            btnClear.Text = "Clear All";
            btnClear.Font = new Font("Segoe UI Semibold", 10);
            btnClear.BackColor = brickRed;
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Size = new Size(100, 40);
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.Click += BtnClear_Click;
            headerPanel.Controls.Add(btnClear);

            headerPanel.Resize += (s, e) =>
            {
                int right = headerPanel.ClientSize.Width;
                btnClear.Location = new Point(right - btnClear.Width, 20);
                btnMarkRead.Location = new Point(right - btnClear.Width - btnMarkRead.Width - 10, 20);
                btnRefresh.Location = new Point(right - btnClear.Width - btnMarkRead.Width - btnRefresh.Width - 20, 20);
            };

            // ── STATS ────────────────────────────────────────────────────────
            statsPanel = new Panel();
            statsPanel.Dock = DockStyle.Top;
            statsPanel.Height = 120;
            statsPanel.Padding = new Padding(0, 8, 0, 8);
            statsPanel.BackColor = Color.FromArgb(245, 247, 250);
            this.Controls.Add(statsPanel);

            // ── NOTIFICATIONS PANEL ───────────────────────────────────────────
            notificationsPanel = new FlowLayoutPanel();
            notificationsPanel.Dock = DockStyle.Fill;
            notificationsPanel.AutoScroll = true;
            notificationsPanel.FlowDirection = FlowDirection.TopDown;
            notificationsPanel.WrapContents = false;
            notificationsPanel.Padding = new Padding(0, 10, 0, 10);
            notificationsPanel.BackColor = Color.FromArgb(245, 247, 250);
            this.Controls.Add(notificationsPanel);

            lblNoNotifications = new Label();
            lblNoNotifications.Text = "🔔  No notifications available\n\nYou're all caught up!";
            lblNoNotifications.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblNoNotifications.ForeColor = Color.FromArgb(180, 180, 180);
            lblNoNotifications.TextAlign = ContentAlignment.MiddleCenter;
            lblNoNotifications.Size = new Size(500, 160);
            lblNoNotifications.Margin = new Padding(80, 60, 0, 0);
            lblNoNotifications.Visible = false;
            notificationsPanel.Controls.Add(lblNoNotifications);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateStatsCards();
            LoadNotificationsToPanel();
        }

        // ── STATS CARDS ───────────────────────────────────────────────────────
        private void UpdateStatsCards()
        {
            if (statsPanel == null || statsPanel.Width < 10) return;
            statsPanel.Controls.Clear();

            int total = 0, emergency = 0, unread = 0, resolved = 0;

            if (allNotifications != null)
            {
                foreach (DataRow row in allNotifications.Rows)
                {
                    total++;
                    string type = row["Type"]?.ToString() ?? "";
                    bool isRead = Convert.ToBoolean(row["IsRead"]);
                    if (type == "Emergency") emergency++;
                    if (!isRead) unread++;
                    else resolved++;
                }
            }

            string[] titles = { "Total", "Emergency", "Unread", "Resolved" };
            string[] values = { total.ToString(), emergency.ToString(), unread.ToString(), resolved.ToString() };
            Color[] vColors = {
                Color.FromArgb(120, 22, 27),
                Color.FromArgb(220, 38, 38),
                Color.FromArgb(202, 138, 4),
                Color.FromArgb(22, 163, 74)
            };

            int gap = 15;
            int cardWidth = (statsPanel.ClientSize.Width - gap * 3) / 4;
            if (cardWidth < 120) cardWidth = 120;
            int cardH = statsPanel.ClientSize.Height - statsPanel.Padding.Top - statsPanel.Padding.Bottom;

            for (int i = 0; i < 4; i++)
            {
                Panel card = new Panel();
                card.BackColor = Color.White;
                card.Size = new Size(cardWidth, cardH);
                card.Location = new Point(i * (cardWidth + gap), statsPanel.Padding.Top);

                card.Paint += (s, pe) =>
                {
                    ControlPaint.DrawBorder(pe.Graphics, ((Panel)s).ClientRectangle,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid,
                        Color.FromArgb(229, 231, 235), 1, ButtonBorderStyle.Solid);
                };

                // Label PEHLE add karo
                Label lblTit = new Label();
                lblTit.Text = titles[i];
                lblTit.Font = new Font("Segoe UI", 10);
                lblTit.ForeColor = Color.FromArgb(107, 114, 128);
                lblTit.TextAlign = ContentAlignment.MiddleCenter;
                lblTit.Size = new Size(cardWidth, 30);
                lblTit.Location = new Point(0, cardH - 32);
                card.Controls.Add(lblTit);

                // Number BAAD mein add karo
                Label lblVal = new Label();
                lblVal.Text = values[i];
                lblVal.Font = new Font("Segoe UI", 28, FontStyle.Bold);
                lblVal.ForeColor = vColors[i];
                lblVal.TextAlign = ContentAlignment.MiddleCenter;
                lblVal.Size = new Size(cardWidth, cardH - 34);
                lblVal.Location = new Point(0, 0);
                card.Controls.Add(lblVal);

                statsPanel.Controls.Add(card);
            }
        }

        // ── LOAD NOTIFICATION CARDS ───────────────────────────────────────────
        private void LoadNotificationsToPanel()
        {
            if (notificationsPanel == null) return;

            notificationsPanel.SuspendLayout();

            var toRemove = notificationsPanel.Controls
                .Cast<Control>()
                .Where(c => c != lblNoNotifications)
                .ToList();
            foreach (var c in toRemove) { notificationsPanel.Controls.Remove(c); c.Dispose(); }

            if (allNotifications == null || allNotifications.Rows.Count == 0)
            {
                lblNoNotifications.Visible = true;
                notificationsPanel.ResumeLayout();
                return;
            }

            lblNoNotifications.Visible = false;

            int cardWidth = notificationsPanel.ClientSize.Width
                            - notificationsPanel.Padding.Horizontal - 20;
            if (cardWidth < 300) cardWidth = 700;

            foreach (DataRow row in allNotifications.Rows)
            {
                Panel card = CreateNotificationCard(row, cardWidth);
                card.Width = cardWidth;
                card.Margin = new Padding(0, 0, 0, 12);
                notificationsPanel.Controls.Add(card);
            }

            notificationsPanel.ResumeLayout();
        }

        private string GetRelativeTime(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays}d ago";
            return dt.ToString("dd-MMM-yyyy");
        }

        private Panel CreateNotificationCard(DataRow row, int width)
        {
            int notifId = Convert.ToInt32(row["NotificationID"]);
            string title = row["Title"]?.ToString() ?? "";
            string message = row["Message"]?.ToString() ?? "";
            string type = row["Type"]?.ToString() ?? "Info";
            bool isRead = Convert.ToBoolean(row["IsRead"]);
            DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);
            string sentBy = row["SentBy"]?.ToString() ?? "System";
            int relatedId = row["RelatedID"] != DBNull.Value ? Convert.ToInt32(row["RelatedID"]) : 0;
            string relatedType = row["RelatedType"]?.ToString() ?? "";

            Panel card = new Panel();
            card.Width = width;
            card.Height = 110;
            card.BackColor = GetBackgroundColor(type);

            Panel leftBorder = new Panel();
            leftBorder.Dock = DockStyle.Left;
            leftBorder.Width = 6;
            leftBorder.BackColor = GetBorderColor(type);
            card.Controls.Add(leftBorder);

            Panel content = new Panel();
            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(20, 12, 16, 10);
            card.Controls.Add(content);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(17, 24, 39);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(0, 0);
            content.Controls.Add(lblTitle);

            Label lblTime = new Label();
            lblTime.Text = GetRelativeTime(createdAt);
            lblTime.Font = new Font("Segoe UI", 9);
            lblTime.ForeColor = Color.Gray;
            lblTime.AutoSize = true;
            lblTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            content.Controls.Add(lblTime);

            Label lblSentBy = new Label();
            lblSentBy.Text = $"From: {sentBy}";
            lblSentBy.Font = new Font("Segoe UI", 8);
            lblSentBy.ForeColor = Color.FromArgb(156, 163, 175);
            lblSentBy.AutoSize = true;
            lblSentBy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            content.Controls.Add(lblSentBy);

            Label lblMsg = new Label();
            lblMsg.Text = message;
            lblMsg.Font = new Font("Segoe UI", 10);
            lblMsg.ForeColor = Color.FromArgb(55, 65, 81);
            lblMsg.Location = new Point(0, 26);
            lblMsg.AutoSize = true;
            lblMsg.MaximumSize = new Size(width - 260, 0);
            content.Controls.Add(lblMsg);

            // Read Status Badge
            Label lblBadge = new Label();
            lblBadge.Text = isRead ? "● Read" : "● Unread";
            lblBadge.Font = new Font("Segoe UI Semibold", 8);
            lblBadge.ForeColor = isRead ? Color.FromArgb(22, 163, 74) : Color.FromArgb(202, 138, 4);
            lblBadge.AutoSize = true;
            lblBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            content.Controls.Add(lblBadge);

            Button btnView = new Button();
            btnView.Text = "View Details";
            btnView.Size = new Size(105, 34);
            btnView.BackColor = brickRed;
            btnView.ForeColor = Color.White;
            btnView.FlatStyle = FlatStyle.Flat;
            btnView.FlatAppearance.BorderSize = 0;
            btnView.Font = new Font("Segoe UI Semibold", 9);
            btnView.Cursor = Cursors.Hand;
            btnView.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnView.Click += (s, e) =>
                ViewNotificationDetails(notifId, title, message, type, sentBy, relatedId, relatedType);
            content.Controls.Add(btnView);

            Button btnRead = new Button();
            btnRead.Text = isRead ? "✓ Read" : "Mark Read";
            btnRead.Size = new Size(105, 34);
            btnRead.BackColor = isRead ? Color.FromArgb(229, 231, 235) : Color.White;
            btnRead.ForeColor = isRead ? Color.Gray : brickRed;
            btnRead.FlatStyle = FlatStyle.Flat;
            btnRead.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnRead.Font = new Font("Segoe UI Semibold", 9);
            btnRead.Cursor = Cursors.Hand;
            btnRead.Enabled = !isRead;
            btnRead.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRead.Click += (s, e) => MarkAsRead(notifId, btnRead, lblBadge);
            content.Controls.Add(btnRead);

            content.Resize += (s, e) =>
            {
                int right = content.ClientSize.Width - content.Padding.Right;
                lblTime.Location = new Point(right - lblTime.Width, 2);
                lblSentBy.Location = new Point(right - lblSentBy.Width, 20);
                lblBadge.Location = new Point(right - lblBadge.Width, 40);
                btnRead.Location = new Point(right - btnRead.Width,
                                        content.ClientSize.Height - btnRead.Height - content.Padding.Bottom);
                btnView.Location = new Point(right - btnRead.Width - btnView.Width - 10, btnRead.Top);
                lblMsg.MaximumSize = new Size(right - 130, 0);

                int needed = Math.Max(110,
                    content.Padding.Top + lblMsg.Bottom + 14 + btnRead.Height + content.Padding.Bottom);
                card.Height = needed;
            };

            content.PerformLayout();
            return card;
        }

        private void ViewNotificationDetails(int id, string title, string message,
            string type, string sentBy, int relatedId, string relatedType)
        {
            string details = $"📋 NOTIFICATION DETAILS\n\n" +
                             $"Title:   {title}\n" +
                             $"Message: {message}\n" +
                             $"Type:    {type}\n" +
                             $"From:    {sentBy}\n" +
                             $"Date:    {DateTime.Now:dd-MMM-yyyy HH:mm}";

            if (relatedId > 0)
                details += $"\n\nRelated ID:   {relatedId}\nRelated Type: {relatedType}";

            MessageBox.Show(details, "Notification Details",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MarkAsRead(int notifId, Button btn, Label badge)
        {
            try
            {
                string query = "UPDATE Notifications SET IsRead=1, ReadAt=GETDATE() WHERE NotificationID=@ID";
                var p = new[] { new System.Data.SqlClient.SqlParameter("@ID", notifId) };

                if (CommonDAL.ExecuteNonQuery(query, p) > 0)
                {
                    foreach (DataRow row in allNotifications.Rows)
                    {
                        if (Convert.ToInt32(row["NotificationID"]) == notifId)
                        { row["IsRead"] = true; break; }
                    }

                    btn.Text = "✓ Read";
                    btn.Enabled = false;
                    btn.BackColor = Color.FromArgb(229, 231, 235);
                    btn.ForeColor = Color.Gray;

                    badge.Text = "● Read";
                    badge.ForeColor = Color.FromArgb(22, 163, 74);

                    UpdateStatsCards();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("MarkAsRead error", ex);
                MessageBox.Show("Error marking notification as read.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMarkRead_Click(object sender, EventArgs e)
        {
            if (allNotifications == null || allNotifications.Rows.Count == 0) return;
            try
            {
                string query = "UPDATE Notifications SET IsRead=1, ReadAt=GETDATE() WHERE UserID=@UID AND IsRead=0";
                var p = new[] { new System.Data.SqlClient.SqlParameter("@UID", SessionManager.CurrentUserID) };
                int rows = CommonDAL.ExecuteNonQuery(query, p);
                if (rows > 0)
                {
                    foreach (DataRow row in allNotifications.Rows)
                        if (!Convert.ToBoolean(row["IsRead"])) row["IsRead"] = true;

                    LoadNotificationsToPanel();
                    UpdateStatsCards();
                    MessageBox.Show($"{rows} notification(s) marked as read.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BtnMarkRead error", ex);
                MessageBox.Show("Error marking all as read.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (allNotifications == null || allNotifications.Rows.Count == 0)
            {
                MessageBox.Show("No notifications to clear.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Clear all notifications? This cannot be undone.", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                string query = "DELETE FROM Notifications WHERE UserID=@UID";
                var p = new[] { new System.Data.SqlClient.SqlParameter("@UID", SessionManager.CurrentUserID) };
                int rows = CommonDAL.ExecuteNonQuery(query, p);
                if (rows > 0)
                {
                    allNotifications.Clear();
                    LoadNotificationsToPanel();
                    UpdateStatsCards();
                    MessageBox.Show($"{rows} notification(s) cleared.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BtnClear error", ex);
                MessageBox.Show("Error clearing notifications.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Color GetBorderColor(string type)
        {
            switch (type)
            {
                case "Emergency": return Color.FromArgb(220, 38, 38);
                case "Success": return Color.FromArgb(22, 163, 74);
                case "Warning": return Color.FromArgb(234, 179, 8);
                case "Info": return Color.FromArgb(59, 130, 246);
                default: return Color.Gray;
            }
        }

        private Color GetBackgroundColor(string type)
        {
            switch (type)
            {
                case "Emergency": return Color.FromArgb(254, 242, 242);
                case "Success": return Color.FromArgb(240, 253, 244);
                case "Warning": return Color.FromArgb(254, 252, 232);
                case "Info": return Color.FromArgb(239, 246, 255);
                default: return Color.White;
            }
        }
    }
}