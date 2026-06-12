using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class NotificationsForm : Form
    {
        private FlowLayoutPanel notificationsPanel;
        private Panel statsPanel;
        private ComboBox cmbFilter;
        private List<NotificationData> notificationsList;

        private class NotificationData
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string Type { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsRead { get; set; }
        }

        public NotificationsForm()
        {
            InitializeComponent();
            LoadNotifications();
            BuildUI();
        }

        private void LoadNotifications()
        {
            notificationsList = new List<NotificationData>
            {
                new NotificationData { Id = 1, Title = "Blood Request Approved", Message = "Your blood request has been approved.", Type = "Success", CreatedAt = DateTime.Now.AddHours(-2), IsRead = false },
                new NotificationData { Id = 2, Title = "Donation Camp Near You", Message = "A blood donation camp is happening near your area.", Type = "Info", CreatedAt = DateTime.Now.AddDays(-1), IsRead = false },
                new NotificationData { Id = 3, Title = "Low Stock Alert", Message = "O- blood stock is running low.", Type = "Warning", CreatedAt = DateTime.Now.AddDays(-2), IsRead = true },
                new NotificationData { Id = 4, Title = "Donation Successful", Message = "Thank you for your donation!", Type = "Success", CreatedAt = DateTime.Now.AddDays(-3), IsRead = true }
            };
        }

        private void BuildUI()
        {
            this.Text = "Notifications";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            this.Controls.Add(mainPanel);

            // Header
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White, Padding = new Padding(15) };
            mainPanel.Controls.Add(headerPanel);

            Label lblTitle = new Label
            {
                Text = "🔔 Notifications",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(10, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            // Filter
            Label lblFilter = new Label { Text = "Filter:", Location = new Point(10, 55), AutoSize = true };
            cmbFilter = new ComboBox
            {
                Location = new Point(60, 52),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbFilter.Items.AddRange(new string[] { "All", "Unread", "Read", "Success", "Warning", "Info" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => RefreshNotifications();
            headerPanel.Controls.Add(lblFilter);
            headerPanel.Controls.Add(cmbFilter);

            // Mark All Read Button
            Button btnMarkAll = new Button
            {
                Text = "✓ Mark All Read",
                Location = new Point(headerPanel.Width - 130, 50),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnMarkAll.FlatAppearance.BorderSize = 0;
            btnMarkAll.Click += (s, e) => MarkAllRead();
            headerPanel.Controls.Add(btnMarkAll);

            // Stats Panel
            statsPanel = new Panel { Dock = DockStyle.Top, Height = 80, Margin = new Padding(0, 10, 0, 10) };
            mainPanel.Controls.Add(statsPanel);
            UpdateStats();

            // Notifications Panel
            notificationsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(notificationsPanel);
            RefreshNotifications();

            headerPanel.Resize += (s, e) =>
            {
                btnMarkAll.Location = new Point(headerPanel.Width - 130, 50);
            };
        }

        private void UpdateStats()
        {
            statsPanel.Controls.Clear();

            int total = notificationsList.Count;
            int unread = notificationsList.FindAll(n => !n.IsRead).Count;
            int success = notificationsList.FindAll(n => n.Type == "Success").Count;
            int warning = notificationsList.FindAll(n => n.Type == "Warning").Count;

            var stats = new (string title, string value, Color color)[]
            {
                ("Total", total.ToString(), Color.FromArgb(120, 22, 27)),
                ("Unread", unread.ToString(), Color.FromArgb(245, 158, 11)),
                ("Success", success.ToString(), Color.FromArgb(34, 197, 94)),
                ("Warning", warning.ToString(), Color.FromArgb(220, 38, 38))
            };

            int cardWidth = (statsPanel.Width - 60) / 4;
            for (int i = 0; i < stats.Length; i++)
            {
                Panel card = new Panel
                {
                    Size = new Size(cardWidth, 60),
                    Location = new Point(i * (cardWidth + 15), 10),
                    BackColor = Color.White
                };
                statsPanel.Controls.Add(card);

                Label lblValue = new Label
                {
                    Text = stats[i].value,
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = stats[i].color,
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                card.Controls.Add(lblValue);

                Label lblTitle = new Label
                {
                    Text = stats[i].title,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(10, 38),
                    AutoSize = true
                };
                card.Controls.Add(lblTitle);
            }

            statsPanel.Resize += (s, e) =>
            {
                int newWidth = (statsPanel.Width - 60) / 4;
                for (int i = 0; i < statsPanel.Controls.Count; i++)
                {
                    statsPanel.Controls[i].Width = newWidth;
                    statsPanel.Controls[i].Location = new Point(i * (newWidth + 15), 10);
                }
            };
        }

        private void RefreshNotifications()
        {
            notificationsPanel.Controls.Clear();

            string filter = cmbFilter.SelectedItem?.ToString();
            var filtered = notificationsList;

            switch (filter)
            {
                case "Unread": filtered = notificationsList.FindAll(n => !n.IsRead); break;
                case "Read": filtered = notificationsList.FindAll(n => n.IsRead); break;
                case "Success": filtered = notificationsList.FindAll(n => n.Type == "Success"); break;
                case "Warning": filtered = notificationsList.FindAll(n => n.Type == "Warning"); break;
                case "Info": filtered = notificationsList.FindAll(n => n.Type == "Info"); break;
            }

            foreach (var notif in filtered)
            {
                Panel card = CreateNotificationCard(notif);
                notificationsPanel.Controls.Add(card);
            }
        }

        private Panel CreateNotificationCard(NotificationData notif)
        {
            Color borderColor;
            Color bgColor;
            if (notif.Type == "Success")
            {
                borderColor = Color.FromArgb(34, 197, 94);
                bgColor = Color.FromArgb(240, 253, 244);
            }
            else if (notif.Type == "Warning")
            {
                borderColor = Color.FromArgb(245, 158, 11);
                bgColor = Color.FromArgb(254, 252, 232);
            }
            else if (notif.Type == "Info")
            {
                borderColor = Color.FromArgb(59, 130, 246);
                bgColor = Color.FromArgb(239, 246, 255);
            }
            else
            {
                borderColor = Color.FromArgb(120, 22, 27);
                bgColor = Color.White;
            }

            Panel card = new Panel
            {
                Width = notificationsPanel.Width - 20,
                Height = 85,
                BackColor = bgColor,
                Margin = new Padding(0, 0, 0, 8)
            };
            card.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int radius = 10;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                card.Region = new Region(path);
            };

            Panel borderPanel = new Panel
            {
                Width = 5,
                Height = 85,
                BackColor = borderColor,
                Dock = DockStyle.Left
            };
            card.Controls.Add(borderPanel);

            Label lblTitle = new Label
            {
                Text = notif.Title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Location = new Point(20, 12),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            Label lblMessage = new Label
            {
                Text = notif.Message,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(80, 80, 90),
                Location = new Point(20, 38),
                AutoSize = true
            };
            card.Controls.Add(lblMessage);

            Label lblTime = new Label
            {
                Text = notif.CreatedAt.ToString("dd-MMM-yyyy hh:mm tt"),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(card.Width - 130, 12),
                AutoSize = true
            };
            card.Controls.Add(lblTime);

            if (!notif.IsRead)
            {
                Panel unreadDot = new Panel
                {
                    Size = new Size(10, 10),
                    Location = new Point(card.Width - 25, 15),
                    BackColor = Color.FromArgb(120, 22, 27)
                };
                unreadDot.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 22, 27)))
                        e.Graphics.FillEllipse(brush, 0, 0, 10, 10);
                };
                card.Controls.Add(unreadDot);
            }

            Button btnMarkRead = new Button
            {
                Text = notif.IsRead ? "Read" : "Mark Read",
                Size = new Size(90, 28),
                Location = new Point(card.Width - 115, 45),
                BackColor = borderColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = !notif.IsRead
            };
            btnMarkRead.FlatAppearance.BorderSize = 0;
            btnMarkRead.Click += (s, e) =>
            {
                notif.IsRead = true;
                RefreshNotifications();
                UpdateStats();
            };
            card.Controls.Add(btnMarkRead);

            card.Resize += (s, e) =>
            {
                lblTime.Location = new Point(card.Width - 130, 12);
                btnMarkRead.Location = new Point(card.Width - 115, 45);
                if (!notif.IsRead)
                {
                    var dot = card.Controls[card.Controls.Count - 2];
                    if (dot is Panel) dot.Location = new Point(card.Width - 25, 15);
                }
            };

            return card;
        }

        private void MarkAllRead()
        {
            foreach (var notif in notificationsList)
                notif.IsRead = true;
            RefreshNotifications();
            UpdateStats();
        }
    }
}