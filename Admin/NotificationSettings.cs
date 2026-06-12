using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class NotificationSettings : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private CheckBox emailToggle, smsToggle, donorToggle, hospitalToggle;
        private TextBox messageText;
        private Button saveBtn;
        private Label lblStatus;

        public NotificationSettings()
        {
            this.Text = "Notification Settings - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildContent();
            BuildSidebar();
            LoadSettings();
        }

        // =====================================================
        // LOAD SETTINGS FROM DATABASE
        // =====================================================
        private void LoadSettings()
        {
            try
            {
                // Load Email Settings
                string emailSetting = GetSettingValue("EmailNotifications");
                emailToggle.Checked = emailSetting == "ON";

                // Load SMS Settings
                string smsSetting = GetSettingValue("SMSNotifications");
                smsToggle.Checked = smsSetting == "ON";

                // Load Donor Alerts
                string donorSetting = GetSettingValue("DonorAlerts");
                donorToggle.Checked = donorSetting == "ON";

                // Load Hospital Notifications
                string hospitalSetting = GetSettingValue("HospitalNotifications");
                hospitalToggle.Checked = hospitalSetting == "ON";

                // Load Default Message
                string defaultMessage = GetSettingValue("DefaultMessage");
                if (!string.IsNullOrEmpty(defaultMessage) && defaultMessage != "Enter default notification message...")
                {
                    messageText.Text = defaultMessage;
                    messageText.ForeColor = Color.FromArgb(30, 30, 40);
                }
                else
                {
                    messageText.Text = "Enter default notification message...";
                    messageText.ForeColor = Color.Gray;
                }

                lblStatus.Text = "✅ Settings loaded from database";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSettings Error: {ex.Message}");
                lblStatus.Text = "⚠️ Using default settings";
                lblStatus.ForeColor = Color.Orange;
            }
        }

        private string GetSettingValue(string settingKey)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT SettingValue FROM NotificationSettings WHERE SettingKey = @Key";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Key", settingKey);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "OFF";
                }
            }
            catch
            {
                return "OFF";
            }
        }

        // =====================================================
        // SAVE SETTINGS TO DATABASE
        // =====================================================
        private void SaveSettings()
        {
            try
            {
                UpdateSettingValue("EmailNotifications", emailToggle.Checked ? "ON" : "OFF");
                UpdateSettingValue("SMSNotifications", smsToggle.Checked ? "ON" : "OFF");
                UpdateSettingValue("DonorAlerts", donorToggle.Checked ? "ON" : "OFF");
                UpdateSettingValue("HospitalNotifications", hospitalToggle.Checked ? "ON" : "OFF");

                string message = messageText.Text;
                if (message == "Enter default notification message...")
                    message = "";
                UpdateSettingValue("DefaultMessage", message);

                lblStatus.Text = "✅ Settings saved successfully to database!";
                lblStatus.ForeColor = Color.Green;

                // Show success message
                string status = $"Email: {(emailToggle.Checked ? "ON" : "OFF")}\n" +
                    $"SMS: {(smsToggle.Checked ? "ON" : "OFF")}\n" +
                    $"Donor Alerts: {(donorToggle.Checked ? "ON" : "OFF")}\n" +
                    $"Hospital: {(hospitalToggle.Checked ? "ON" : "OFF")}";

                MessageBox.Show($"✅ Notification Settings Saved!\n\n{status}\n\nMessage: {message}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Log the action
                AuditHelper.Log("Update", "NotificationSettings", "Notification settings updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Failed to save settings";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void UpdateSettingValue(string settingKey, string settingValue)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if setting exists
                    string checkSql = "SELECT COUNT(*) FROM NotificationSettings WHERE SettingKey = @Key";
                    var checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@Key", settingKey);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Update existing
                        string updateSql = @"UPDATE NotificationSettings 
                                            SET SettingValue = @Value, 
                                                UpdatedAt = GETDATE(), 
                                                UpdatedBy = @UpdatedBy 
                                            WHERE SettingKey = @Key";
                        var updateCmd = new SqlCommand(updateSql, conn);
                        updateCmd.Parameters.AddWithValue("@Key", settingKey);
                        updateCmd.Parameters.AddWithValue("@Value", settingValue);
                        updateCmd.Parameters.AddWithValue("@UpdatedBy", SessionManager.CurrentUserID);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Insert new
                        string insertSql = @"INSERT INTO NotificationSettings (SettingKey, SettingValue, UpdatedBy) 
                                            VALUES (@Key, @Value, @UpdatedBy)";
                        var insertCmd = new SqlCommand(insertSql, conn);
                        insertCmd.Parameters.AddWithValue("@Key", settingKey);
                        insertCmd.Parameters.AddWithValue("@Value", settingValue);
                        insertCmd.Parameters.AddWithValue("@UpdatedBy", SessionManager.CurrentUserID);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSettingValue Error: {ex.Message}");
            }
        }

        // =====================================================
        // LEFT SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 260;
            sidebarPanel.BackColor = Color.White;

            Label sidebarTitle = new Label();
            sidebarTitle.Text = "☰  System Settings";
            sidebarTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            sidebarTitle.ForeColor = Color.FromArgb(34, 34, 34);
            sidebarTitle.BackColor = Color.White;
            sidebarTitle.Dock = DockStyle.Top;
            sidebarTitle.Height = 60;
            sidebarTitle.TextAlign = ContentAlignment.MiddleLeft;
            sidebarTitle.Padding = new Padding(20, 0, 0, 0);

            Panel titleSeparator = new Panel();
            titleSeparator.Dock = DockStyle.Top;
            titleSeparator.Height = 1;
            titleSeparator.BackColor = Color.FromArgb(230, 230, 235);

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(10, 15, 10, 15);
            menuPanel.BackColor = Color.White;

            AddSettingsMenuItem(menuPanel, "⚙", "General Settings", false, () =>
            {
                using (Settings settingsForm = new Settings())
                {
                    this.Hide();
                    settingsForm.ShowDialog();
                    this.Show();
                }
            });

            AddSettingsMenuItem(menuPanel, "🚨", "Emergency Alert", false, () =>
            {
                using (EmergencyAlert emergencyForm = new EmergencyAlert())
                {
                    this.Hide();
                    emergencyForm.ShowDialog();
                    this.Show();
                }
            });

            AddSettingsMenuItem(menuPanel, "🔔", "Notification Settings", true, null);

            AddSettingsMenuItem(menuPanel, "💾", "Backup Settings", false, () =>
            {
                using (BackupSettings backupForm = new BackupSettings())
                {
                    this.Hide();
                    backupForm.ShowDialog();
                    this.Show();
                }
            });

            AddSettingsMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                AdminDashboard dashboard = new AdminDashboard();
                dashboard.Show();
                this.Close();
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(titleSeparator);
            sidebarPanel.Controls.Add(sidebarTitle);
            this.Controls.Add(sidebarPanel);
        }

        private void AddSettingsMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(235, 45);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(220, 233, 255) : Color.Transparent;
            item.Paint += (s, e) =>
            {
                if (active)
                {
                    Panel p = (Panel)s;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                        p.Region = new Region(path);
                }
            };

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14);
            iconLbl.ForeColor = active ? Color.FromArgb(13, 110, 253) : Color.FromArgb(80, 80, 90);
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(35, 35);
            iconLbl.Location = new Point(10, 5);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 11, active ? FontStyle.Bold : FontStyle.Regular);
            textLbl.ForeColor = active ? Color.FromArgb(13, 110, 253) : Color.FromArgb(60, 60, 70);
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(180, 35);
            textLbl.Location = new Point(48, 5);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            EventHandler enter = (s, e) => { if (!active) item.BackColor = Color.FromArgb(240, 245, 255); };
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click; iconLbl.Click += click; textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter; iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter; textLbl.MouseLeave += leave;
            item.MouseEnter += enter; item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        // =====================================================
        // RIGHT CONTENT
        // =====================================================
        private void BuildContent()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(245, 247, 251);
            contentPanel.Padding = new Padding(40, 30, 40, 30);
            contentPanel.AutoScroll = true;

            Panel cardPanel = new Panel();
            cardPanel.Dock = DockStyle.Fill;
            cardPanel.BackColor = Color.White;
            cardPanel.Padding = new Padding(30, 25, 30, 25);
            cardPanel.AutoScroll = true;
            cardPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            // Card Title
            Label cardTitle = new Label();
            cardTitle.Text = "Notification Settings";
            cardTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            cardTitle.ForeColor = Color.FromArgb(34, 34, 34);
            cardTitle.Dock = DockStyle.Top;
            cardTitle.Height = 45;
            cardTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Loading settings...";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 25;
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            cardPanel.Controls.Add(lblStatus);

            // Notification Items Panel
            FlowLayoutPanel notifPanel = new FlowLayoutPanel();
            notifPanel.Dock = DockStyle.Top;
            notifPanel.FlowDirection = FlowDirection.TopDown;
            notifPanel.WrapContents = false;
            notifPanel.AutoSize = true;
            notifPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            notifPanel.BackColor = Color.Transparent;
            notifPanel.Padding = new Padding(0, 10, 0, 0);

            // Email Notifications
            emailToggle = new CheckBox();
            notifPanel.Controls.Add(CreateNotificationItem("✉", "Email Notifications",
                "Send alerts and updates through email.", true, emailToggle));

            // SMS Notifications
            smsToggle = new CheckBox();
            notifPanel.Controls.Add(CreateNotificationItem("💬", "SMS Notifications",
                "Send emergency alerts via SMS.", true, smsToggle));

            // Donor Alerts
            donorToggle = new CheckBox();
            notifPanel.Controls.Add(CreateNotificationItem("👥", "Donor Alerts",
                "Notify donors for urgent blood requirements.", true, donorToggle));

            // Hospital Notifications
            hospitalToggle = new CheckBox();
            notifPanel.Controls.Add(CreateNotificationItem("🏥", "Hospital Notifications",
                "Send stock and emergency notifications to hospitals.", false, hospitalToggle));

            // Message Card
            Panel messageCard = new Panel();
            messageCard.Dock = DockStyle.Top;
            messageCard.BackColor = Color.Transparent;
            messageCard.Height = 160;
            messageCard.Padding = new Padding(0, 25, 0, 0);

            Label msgTitle = new Label();
            msgTitle.Text = "Default Notification Message";
            msgTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            msgTitle.ForeColor = Color.FromArgb(34, 34, 34);
            msgTitle.Dock = DockStyle.Top;
            msgTitle.Height = 25;

            messageText = new TextBox();
            messageText.Text = "Enter default notification message...";
            messageText.Font = new Font("Segoe UI", 11);
            messageText.ForeColor = Color.Gray;
            messageText.BackColor = Color.White;
            messageText.BorderStyle = BorderStyle.FixedSingle;
            messageText.Multiline = true;
            messageText.ScrollBars = ScrollBars.Vertical;
            messageText.Dock = DockStyle.Fill;
            messageText.Margin = new Padding(0, 10, 0, 0);

            string placeholderText = "Enter default notification message...";
            messageText.Enter += (s, e) =>
            {
                if (messageText.Text == placeholderText)
                {
                    messageText.Text = "";
                    messageText.ForeColor = Color.FromArgb(30, 30, 40);
                }
            };
            messageText.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(messageText.Text))
                {
                    messageText.Text = placeholderText;
                    messageText.ForeColor = Color.Gray;
                }
            };

            messageCard.Controls.Add(messageText);
            messageCard.Controls.Add(msgTitle);

            // Save Button Area
            Panel btnArea = new Panel();
            btnArea.Dock = DockStyle.Top;
            btnArea.Height = 55;
            btnArea.BackColor = Color.Transparent;
            btnArea.Padding = new Padding(0, 20, 0, 0);

            saveBtn = new Button();
            saveBtn.Text = "💾 Save Settings";
            saveBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            saveBtn.ForeColor = Color.White;
            saveBtn.BackColor = Color.FromArgb(120, 22, 27);
            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.FlatAppearance.BorderSize = 0;
            saveBtn.Size = new Size(180, 42);
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.Dock = DockStyle.Right;
            saveBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(btn.ClientRectangle, 8))
                    btn.Region = new Region(path);
            };
            saveBtn.Click += (s, e) => SaveSettings();
            saveBtn.MouseEnter += (s, e) => saveBtn.BackColor = Color.FromArgb(100, 18, 23);
            saveBtn.MouseLeave += (s, e) => saveBtn.BackColor = Color.FromArgb(120, 22, 27);

            btnArea.Controls.Add(saveBtn);

            cardPanel.Controls.Add(btnArea);
            cardPanel.Controls.Add(messageCard);
            cardPanel.Controls.Add(notifPanel);
            cardPanel.Controls.Add(cardTitle);
            cardPanel.Controls.Add(lblStatus);

            contentPanel.Controls.Add(cardPanel);
            this.Controls.Add(contentPanel);
        }

        private Panel CreateNotificationItem(string icon, string title, string description, bool isChecked, CheckBox toggle)
        {
            Panel item = new Panel();
            item.Size = new Size(780, 80);
            item.Margin = new Padding(0, 0, 0, 10);
            item.BackColor = Color.White;
            item.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle,
                    Color.FromArgb(238, 238, 238), ButtonBorderStyle.Solid);
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                    p.Region = new Region(path);
            };

            // Icon Box
            Panel iconBox = new Panel();
            iconBox.Size = new Size(50, 50);
            iconBox.Location = new Point(15, 15);
            iconBox.BackColor = Color.FromArgb(238, 244, 255);
            iconBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                    p.Region = new Region(path);
            };

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 18);
            iconLbl.ForeColor = Color.FromArgb(13, 110, 253);
            iconLbl.Size = new Size(50, 50);
            iconLbl.Location = new Point(0, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;
            iconBox.Controls.Add(iconLbl);

            Label titleLbl = new Label();
            titleLbl.Text = title;
            titleLbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(34, 34, 34);
            titleLbl.Location = new Point(78, 15);
            titleLbl.AutoSize = true;

            Label descLbl = new Label();
            descLbl.Text = description;
            descLbl.Font = new Font("Segoe UI", 9);
            descLbl.ForeColor = Color.FromArgb(102, 102, 102);
            descLbl.Location = new Point(78, 42);
            descLbl.AutoSize = true;

            toggle.Checked = isChecked;
            toggle.AutoSize = false;
            toggle.Size = new Size(50, 26);
            toggle.Location = new Point(item.Width - 70, 27);
            toggle.FlatStyle = FlatStyle.Flat;
            toggle.FlatAppearance.BorderSize = 0;
            toggle.Appearance = Appearance.Button;
            toggle.Text = "";
            toggle.BackColor = Color.Transparent;
            toggle.Cursor = Cursors.Hand;
            toggle.TabStop = false;

            toggle.CheckedChanged += (s, e) => toggle.Invalidate();

            toggle.Paint += (s, e) =>
            {
                CheckBox cb = (CheckBox)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Color trackColor = cb.Checked ? Color.FromArgb(13, 110, 253) : Color.FromArgb(204, 204, 204);
                Rectangle trackRect = new Rectangle(0, 3, 50, 20);

                using (GraphicsPath trackPath = new GraphicsPath())
                {
                    trackPath.AddArc(trackRect.X, trackRect.Y, 20, 20, 90, 180);
                    trackPath.AddArc(trackRect.Right - 20, trackRect.Y, 20, 20, 270, 180);
                    trackPath.CloseFigure();
                    using (SolidBrush trackBrush = new SolidBrush(trackColor))
                        e.Graphics.FillPath(trackBrush, trackPath);
                }

                int knobX = cb.Checked ? 27 : 3;
                Rectangle knobRect = new Rectangle(knobX, 1, 22, 22);
                using (GraphicsPath knobPath = new GraphicsPath())
                {
                    knobPath.AddEllipse(knobRect);
                    using (SolidBrush knobBrush = new SolidBrush(Color.White))
                        e.Graphics.FillPath(knobBrush, knobPath);
                }
            };

            item.Resize += (s, e) => toggle.Location = new Point(item.Width - 70, 27);

            item.Controls.Add(toggle);
            item.Controls.Add(descLbl);
            item.Controls.Add(titleLbl);
            item.Controls.Add(iconBox);

            return item;
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