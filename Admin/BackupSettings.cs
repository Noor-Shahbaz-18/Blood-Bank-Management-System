using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class BackupSettings : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private CheckBox autoBackupToggle;
        private ComboBox frequencyCombo;
        private DateTimePicker timePicker;
        private ComboBox storageCombo;
        private Button saveBtn, downloadBtn;
        private Label lblLastBackup, lblStatus;

        public BackupSettings()
        {
            this.Text = "Backup Settings - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildContent();
            BuildSidebar();
            LoadSettings();
            UpdateLastBackupInfo();
        }

        private void LoadSettings()
        {
            try
            {
                autoBackupToggle.Checked = GetSettingValue("AutoBackup") == "ON";

                string frequency = GetSettingValue("BackupFrequency");
                if (frequency == "Daily") frequencyCombo.SelectedIndex = 0;
                else if (frequency == "Weekly") frequencyCombo.SelectedIndex = 1;
                else if (frequency == "Monthly") frequencyCombo.SelectedIndex = 2;
                else frequencyCombo.SelectedIndex = 0;

                string backupTime = GetSettingValue("BackupTime");
                if (!string.IsNullOrEmpty(backupTime))
                {
                    try
                    {
                        timePicker.Value = DateTime.Parse(backupTime);
                    }
                    catch { }
                }

                string storage = GetSettingValue("StorageLocation");
                if (storage == "Local Server") storageCombo.SelectedIndex = 0;
                else if (storage == "Cloud Storage") storageCombo.SelectedIndex = 1;
                else if (storage == "External Drive") storageCombo.SelectedIndex = 2;
                else storageCombo.SelectedIndex = 0;

                string backupPath = GetSettingValue("BackupPath");
                if (!string.IsNullOrEmpty(backupPath))
                    BackupHelper.SetBackupPath(backupPath);

                lblStatus.Text = "✅ Settings loaded";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
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
                    string sql = "SELECT SettingValue FROM BackupSettings WHERE SettingKey = @Key";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Key", settingKey);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
            catch { return ""; }
        }

        private void SaveSettingsToDb()
        {
            try
            {
                UpdateSetting("AutoBackup", autoBackupToggle.Checked ? "ON" : "OFF");
                UpdateSetting("BackupFrequency", frequencyCombo.Text);
                UpdateSetting("BackupTime", timePicker.Value.ToString("HH:mm"));
                UpdateSetting("StorageLocation", storageCombo.Text);
                UpdateSetting("BackupPath", BackupHelper.GetBackupPath());

                lblStatus.Text = "✅ Settings saved to database!";
                lblStatus.ForeColor = Color.Green;

                MessageBox.Show($"✅ Backup Settings Saved!\n\n" +
                    $"Auto Backup: {(autoBackupToggle.Checked ? "ON" : "OFF")}\n" +
                    $"Frequency: {frequencyCombo.Text}\n" +
                    $"Time: {timePicker.Value:HH:mm}\n" +
                    $"Storage: {storageCombo.Text}\n" +
                    $"Path: {BackupHelper.GetBackupPath()}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AuditHelper.Log("Update", "BackupSettings", "Backup settings updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Failed to save";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void UpdateSetting(string key, string value)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string checkSql = "SELECT COUNT(*) FROM BackupSettings WHERE SettingKey = @Key";
                var checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@Key", key);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    string updateSql = "UPDATE BackupSettings SET SettingValue = @Value, UpdatedAt = GETDATE(), UpdatedBy = @UserId WHERE SettingKey = @Key";
                    var updateCmd = new SqlCommand(updateSql, conn);
                    updateCmd.Parameters.AddWithValue("@Key", key);
                    updateCmd.Parameters.AddWithValue("@Value", value);
                    updateCmd.Parameters.AddWithValue("@UserId", SessionManager.CurrentUserID);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    string insertSql = "INSERT INTO BackupSettings (SettingKey, SettingValue, UpdatedBy) VALUES (@Key, @Value, @UserId)";
                    var insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@Key", key);
                    insertCmd.Parameters.AddWithValue("@Value", value);
                    insertCmd.Parameters.AddWithValue("@UserId", SessionManager.CurrentUserID);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private void PerformBackup()
        {
            try
            {
                downloadBtn.Enabled = false;
                saveBtn.Enabled = false;
                lblStatus.Text = "⏳ Creating backup...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                string backupPath, error;
                bool success = BackupHelper.CreateBackup(out backupPath, out error);

                if (success)
                {
                    lblStatus.Text = "✅ Backup created successfully!";
                    lblStatus.ForeColor = Color.Green;
                    UpdateLastBackupInfo();

                    MessageBox.Show($"✅ Database Backup Created Successfully!\n\n" +
                        $"📍 Location: {backupPath}\n" +
                        $"📅 Date: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}\n\n" +
                        $"The backup file has been saved to your selected location.",
                        "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "❌ Backup failed!";
                    lblStatus.ForeColor = Color.Red;
                    MessageBox.Show($"❌ Backup Failed!\n\nError: {error}\n\n" +
                        $"Please check:\n" +
                        $"1. SQL Server is running\n" +
                        $"2. You have write permissions to backup folder\n" +
                        $"3. Enough disk space available",
                        "Backup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Backup failed!";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                downloadBtn.Enabled = true;
                saveBtn.Enabled = true;
            }
        }

        private void UpdateLastBackupInfo()
        {
            string info = BackupHelper.GetLastBackupInfo();
            lblLastBackup.Text = info;
        }

        private void SelectBackupFolder()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Backup Folder Location";
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    BackupHelper.SetBackupPath(fbd.SelectedPath);
                    lblStatus.Text = $"✅ Backup path updated to: {fbd.SelectedPath}";
                    lblStatus.ForeColor = Color.Green;
                }
            }
        }

        // =====================================================
        // SIDEBAR METHODS
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

            AddSettingsMenuItem(menuPanel, "🔔", "Notification Settings", false, () =>
            {
                using (NotificationSettings notifForm = new NotificationSettings())
                {
                    this.Hide();
                    notifForm.ShowDialog();
                    this.Show();
                }
            });

            AddSettingsMenuItem(menuPanel, "💾", "Backup Settings", true, null);

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
            cardTitle.Text = "Database Backup Settings";
            cardTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            cardTitle.ForeColor = Color.FromArgb(34, 34, 34);
            cardTitle.Dock = DockStyle.Top;
            cardTitle.Height = 45;
            cardTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 25;
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            cardPanel.Controls.Add(lblStatus);

            // Auto Backup Option
            Panel autoBackupPanel = CreateAutoBackupOption();
            autoBackupPanel.Dock = DockStyle.Top;

            // Form Fields Panel
            Panel formPanel = new Panel();
            formPanel.Dock = DockStyle.Top;
            formPanel.BackColor = Color.Transparent;
            formPanel.Height = 250;
            formPanel.Padding = new Padding(0, 20, 0, 0);

            Label freqLabel = new Label();
            freqLabel.Text = "Backup Frequency";
            freqLabel.Font = new Font("Segoe UI", 10);
            freqLabel.ForeColor = Color.FromArgb(85, 85, 85);
            freqLabel.Dock = DockStyle.Top;
            freqLabel.Height = 22;

            frequencyCombo = new ComboBox();
            frequencyCombo.Font = new Font("Segoe UI", 11);
            frequencyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            frequencyCombo.Dock = DockStyle.Top;
            frequencyCombo.Height = 38;
            frequencyCombo.Margin = new Padding(0, 0, 0, 18);
            frequencyCombo.Items.AddRange(new string[] { "Daily", "Weekly", "Monthly" });
            frequencyCombo.SelectedIndex = 0;

            Label timeLabel = new Label();
            timeLabel.Text = "Backup Time";
            timeLabel.Font = new Font("Segoe UI", 10);
            timeLabel.ForeColor = Color.FromArgb(85, 85, 85);
            timeLabel.Dock = DockStyle.Top;
            timeLabel.Height = 22;

            timePicker = new DateTimePicker();
            timePicker.Format = DateTimePickerFormat.Time;
            timePicker.Font = new Font("Segoe UI", 11);
            timePicker.Dock = DockStyle.Top;
            timePicker.Height = 38;
            timePicker.Margin = new Padding(0, 0, 0, 18);
            timePicker.Value = DateTime.Parse("23:00");

            Label storageLabel = new Label();
            storageLabel.Text = "Storage Location";
            storageLabel.Font = new Font("Segoe UI", 10);
            storageLabel.ForeColor = Color.FromArgb(85, 85, 85);
            storageLabel.Dock = DockStyle.Top;
            storageLabel.Height = 22;

            storageCombo = new ComboBox();
            storageCombo.Font = new Font("Segoe UI", 11);
            storageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            storageCombo.Dock = DockStyle.Top;
            storageCombo.Height = 38;
            storageCombo.Items.AddRange(new string[] { "Local Server", "Cloud Storage", "External Drive" });
            storageCombo.SelectedIndex = 0;

            Button btnChangePath = new Button();
            btnChangePath.Text = "📁 Change Backup Folder";
            btnChangePath.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnChangePath.BackColor = Color.FromArgb(240, 242, 245);
            btnChangePath.FlatStyle = FlatStyle.Flat;
            btnChangePath.Size = new Size(180, 32);
            btnChangePath.Location = new Point(20, 200);
            btnChangePath.Cursor = Cursors.Hand;
            btnChangePath.Click += (s, e) => SelectBackupFolder();

            formPanel.Controls.Add(btnChangePath);
            formPanel.Controls.Add(storageCombo);
            formPanel.Controls.Add(storageLabel);
            formPanel.Controls.Add(timePicker);
            formPanel.Controls.Add(timeLabel);
            formPanel.Controls.Add(frequencyCombo);
            formPanel.Controls.Add(freqLabel);

            // Last Backup Info Panel
            Panel lastBackupPanel = CreateLastBackupPanel();
            lastBackupPanel.Dock = DockStyle.Top;

            // Buttons Area
            Panel btnArea = new Panel();
            btnArea.Dock = DockStyle.Top;
            btnArea.Height = 65;
            btnArea.BackColor = Color.Transparent;
            btnArea.Padding = new Padding(0, 25, 0, 0);

            downloadBtn = new Button();
            downloadBtn.Text = "⬇ Create Backup Now";
            downloadBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            downloadBtn.ForeColor = Color.White;
            downloadBtn.BackColor = Color.FromArgb(120, 22, 27);
            downloadBtn.FlatStyle = FlatStyle.Flat;
            downloadBtn.Size = new Size(200, 42);
            downloadBtn.Cursor = Cursors.Hand;
            downloadBtn.Dock = DockStyle.Right;
            downloadBtn.Click += (s, e) => PerformBackup();

            saveBtn = new Button();
            saveBtn.Text = "💾 Save Settings";
            saveBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            saveBtn.ForeColor = Color.White;
            saveBtn.BackColor = Color.FromArgb(13, 110, 253);
            saveBtn.FlatStyle = FlatStyle.Flat;
            saveBtn.Size = new Size(160, 42);
            saveBtn.Cursor = Cursors.Hand;
            saveBtn.Dock = DockStyle.Right;
            saveBtn.Margin = new Padding(0, 0, 10, 0);
            saveBtn.Click += (s, e) => SaveSettingsToDb();

            btnArea.Controls.Add(downloadBtn);
            btnArea.Controls.Add(saveBtn);

            cardPanel.Controls.Add(btnArea);
            cardPanel.Controls.Add(lastBackupPanel);
            cardPanel.Controls.Add(formPanel);
            cardPanel.Controls.Add(autoBackupPanel);
            cardPanel.Controls.Add(cardTitle);
            cardPanel.Controls.Add(lblStatus);

            contentPanel.Controls.Add(cardPanel);
            this.Controls.Add(contentPanel);
        }

        private Panel CreateAutoBackupOption()
        {
            Panel option = new Panel();
            option.Height = 80;
            option.BackColor = Color.White;
            option.Margin = new Padding(0, 0, 0, 5);
            option.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle,
                    Color.FromArgb(238, 238, 238), ButtonBorderStyle.Solid);
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                    p.Region = new Region(path);
            };

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
            iconLbl.Text = "🗄";
            iconLbl.Font = new Font("Segoe UI Emoji", 20);
            iconLbl.ForeColor = Color.FromArgb(13, 110, 253);
            iconLbl.Size = new Size(50, 50);
            iconLbl.Location = new Point(0, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;
            iconBox.Controls.Add(iconLbl);

            Label titleLbl = new Label();
            titleLbl.Text = "Automatic Backup";
            titleLbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(34, 34, 34);
            titleLbl.Location = new Point(78, 16);
            titleLbl.AutoSize = true;

            Label descLbl = new Label();
            descLbl.Text = "Enable automatic daily database backups.";
            descLbl.Font = new Font("Segoe UI", 9);
            descLbl.ForeColor = Color.FromArgb(102, 102, 102);
            descLbl.Location = new Point(78, 42);
            descLbl.AutoSize = true;

            autoBackupToggle = new CheckBox();
            autoBackupToggle.Checked = true;
            autoBackupToggle.AutoSize = false;
            autoBackupToggle.Size = new Size(50, 26);
            autoBackupToggle.Location = new Point(option.Width - 70, 27);
            autoBackupToggle.FlatStyle = FlatStyle.Flat;
            autoBackupToggle.FlatAppearance.BorderSize = 0;
            autoBackupToggle.Appearance = Appearance.Button;
            autoBackupToggle.Text = "";
            autoBackupToggle.BackColor = Color.Transparent;
            autoBackupToggle.Cursor = Cursors.Hand;

            option.Resize += (s, e) => autoBackupToggle.Location = new Point(option.Width - 70, 27);

            option.Controls.Add(autoBackupToggle);
            option.Controls.Add(descLbl);
            option.Controls.Add(titleLbl);
            option.Controls.Add(iconBox);

            return option;
        }

        private Panel CreateLastBackupPanel()
        {
            Panel panel = new Panel();
            panel.Height = 100;
            panel.BackColor = Color.FromArgb(248, 249, 252);
            panel.Margin = new Padding(0, 25, 0, 0);
            panel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                    p.Region = new Region(path);
            };

            Panel iconBox = new Panel();
            iconBox.Size = new Size(50, 50);
            iconBox.Location = new Point(15, 25);
            iconBox.BackColor = Color.FromArgb(220, 233, 255);
            iconBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 10))
                    p.Region = new Region(path);
            };

            Label iconLbl = new Label();
            iconLbl.Text = "🕐";
            iconLbl.Font = new Font("Segoe UI Emoji", 20);
            iconLbl.ForeColor = Color.FromArgb(13, 110, 253);
            iconLbl.Size = new Size(50, 50);
            iconLbl.Location = new Point(0, 0);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;
            iconBox.Controls.Add(iconLbl);

            Label titleLbl = new Label();
            titleLbl.Text = "Last Backup";
            titleLbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLbl.ForeColor = Color.FromArgb(34, 34, 34);
            titleLbl.Location = new Point(78, 16);
            titleLbl.AutoSize = true;

            lblLastBackup = new Label();
            lblLastBackup.Text = "Loading...";
            lblLastBackup.Font = new Font("Segoe UI", 10);
            lblLastBackup.ForeColor = Color.FromArgb(102, 102, 102);
            lblLastBackup.Location = new Point(78, 45);
            lblLastBackup.AutoSize = true;

            panel.Controls.Add(lblLastBackup);
            panel.Controls.Add(titleLbl);
            panel.Controls.Add(iconBox);

            return panel;
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