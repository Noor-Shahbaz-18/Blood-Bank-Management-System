using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.BusinessLogic;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class Settings : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;

        // General Settings Fields
        private TextBox txtSystemName, txtBankCode, txtEmail, txtPhone, txtAddress;

        // Email Settings Fields
        private TextBox txtSmtpServer, txtSmtpPort, txtSenderEmail, txtSenderPassword;

        // SMS Settings Fields
        private TextBox txtTwilioSID, txtTwilioToken, txtTwilioNumber;

        private Button saveBtn;
        private Label lblStatus;
        private TabControl tabControl;

        public Settings()
        {
            this.Text = "System Settings - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // ✅ FIX: BuildContent() PEHLE, BuildSidebar() BAAD MEIN
            BuildContent();
            BuildSidebar();
            LoadAllSettings();
        }

        // =====================================================
        // LOAD ALL SETTINGS FROM DATABASE
        // =====================================================
        private void LoadAllSettings()
        {
            try
            {
                txtSystemName.Text = GetSettingValue("SystemName");
                if (string.IsNullOrEmpty(txtSystemName.Text)) txtSystemName.Text = "Enter system name...";
                txtSystemName.ForeColor = txtSystemName.Text == "Enter system name..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtBankCode.Text = GetSettingValue("BankCode");
                if (string.IsNullOrEmpty(txtBankCode.Text)) txtBankCode.Text = "Enter bank code...";
                txtBankCode.ForeColor = txtBankCode.Text == "Enter bank code..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtEmail.Text = GetSettingValue("ContactEmail");
                if (string.IsNullOrEmpty(txtEmail.Text)) txtEmail.Text = "Enter email address...";
                txtEmail.ForeColor = txtEmail.Text == "Enter email address..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtPhone.Text = GetSettingValue("ContactPhone");
                if (string.IsNullOrEmpty(txtPhone.Text)) txtPhone.Text = "Enter phone number...";
                txtPhone.ForeColor = txtPhone.Text == "Enter phone number..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtAddress.Text = GetSettingValue("Address");
                if (string.IsNullOrEmpty(txtAddress.Text)) txtAddress.Text = "Enter full address...";
                txtAddress.ForeColor = txtAddress.Text == "Enter full address..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtSmtpServer.Text = GetSettingValue("SmtpServer");
                if (string.IsNullOrEmpty(txtSmtpServer.Text)) txtSmtpServer.Text = "smtp.gmail.com";
                txtSmtpServer.ForeColor = txtSmtpServer.Text == "smtp.gmail.com" ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtSmtpPort.Text = GetSettingValue("SmtpPort");
                if (string.IsNullOrEmpty(txtSmtpPort.Text)) txtSmtpPort.Text = "587";
                txtSmtpPort.ForeColor = txtSmtpPort.Text == "587" ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtSenderEmail.Text = GetSettingValue("SenderEmail");
                if (string.IsNullOrEmpty(txtSenderEmail.Text)) txtSenderEmail.Text = "Enter email...";
                txtSenderEmail.ForeColor = txtSenderEmail.Text == "Enter email..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                string senderPass = GetSettingValue("SenderPassword");
                if (!string.IsNullOrEmpty(senderPass) && senderPass != "Enter password...")
                {
                    txtSenderPassword.Text = senderPass;
                    txtSenderPassword.ForeColor = Color.FromArgb(30, 30, 40);
                }
                else
                {
                    txtSenderPassword.Text = "Enter password...";
                    txtSenderPassword.ForeColor = Color.Gray;
                }

                txtTwilioSID.Text = GetSettingValue("TwilioAccountSID");
                if (string.IsNullOrEmpty(txtTwilioSID.Text)) txtTwilioSID.Text = "Enter Account SID...";
                txtTwilioSID.ForeColor = txtTwilioSID.Text == "Enter Account SID..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtTwilioToken.Text = GetSettingValue("TwilioAuthToken");
                if (string.IsNullOrEmpty(txtTwilioToken.Text)) txtTwilioToken.Text = "Enter Auth Token...";
                txtTwilioToken.ForeColor = txtTwilioToken.Text == "Enter Auth Token..." ? Color.Gray : Color.FromArgb(30, 30, 40);

                txtTwilioNumber.Text = GetSettingValue("TwilioPhoneNumber");
                if (string.IsNullOrEmpty(txtTwilioNumber.Text)) txtTwilioNumber.Text = "+1234567890";
                txtTwilioNumber.ForeColor = txtTwilioNumber.Text == "+1234567890" ? Color.Gray : Color.FromArgb(30, 30, 40);

                lblStatus.Text = "✅ Settings loaded from database";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllSettings Error: {ex.Message}");
                lblStatus.Text = "⚠️ Using default settings";
                lblStatus.ForeColor = Color.Orange;
            }
        }

        private string GetSettingValue(string settingKey)
        {
            try { return SettingsDAL.GetSetting(settingKey); }
            catch { return ""; }
        }

        // =====================================================
        // SAVE ALL SETTINGS TO DATABASE
        // =====================================================
        private void SaveAllSettings()
        {
            try
            {
                SettingsDAL.UpdateSetting("SystemName", GetActualText(txtSystemName, "Enter system name..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("BankCode", GetActualText(txtBankCode, "Enter bank code..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("ContactEmail", GetActualText(txtEmail, "Enter email address..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("ContactPhone", GetActualText(txtPhone, "Enter phone number..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("Address", GetActualText(txtAddress, "Enter full address..."), SessionManager.CurrentUserID);

                SettingsDAL.UpdateSetting("SmtpServer", GetActualText(txtSmtpServer, "smtp.gmail.com"), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("SmtpPort", GetActualText(txtSmtpPort, "587"), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("SenderEmail", GetActualText(txtSenderEmail, "Enter email..."), SessionManager.CurrentUserID);

                string password = txtSenderPassword.Text;
                if (password == "Enter password..." || string.IsNullOrEmpty(password)) password = "";
                SettingsDAL.UpdateSetting("SenderPassword", password, SessionManager.CurrentUserID);

                SettingsDAL.UpdateSetting("TwilioAccountSID", GetActualText(txtTwilioSID, "Enter Account SID..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("TwilioAuthToken", GetActualText(txtTwilioToken, "Enter Auth Token..."), SessionManager.CurrentUserID);
                SettingsDAL.UpdateSetting("TwilioPhoneNumber", GetActualText(txtTwilioNumber, "+1234567890"), SessionManager.CurrentUserID);

                EmailManager.LoadEmailSettings();
                SMSSender.LoadSMSSettings();

                lblStatus.Text = "✅ All settings saved successfully!";
                lblStatus.ForeColor = Color.Green;

                MessageBox.Show("✅ All settings saved successfully to database!\n\n" +
                    "📧 Email: " + (string.IsNullOrEmpty(txtSenderEmail.Text) ? "Not configured" : txtSenderEmail.Text) + "\n" +
                    "📱 SMS: " + (string.IsNullOrEmpty(txtTwilioSID.Text) ? "Not configured" : "Configured") + "\n\n" +
                    "Note: Email/SMS will work only if credentials are correct.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AuditHelper.Log("Update", "Settings", "System settings updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Failed to save settings";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private string GetActualText(TextBox txt, string placeholder)
        {
            string value = txt.Text.Trim();
            if (string.IsNullOrEmpty(value) || value == placeholder) return "";
            return value;
        }

        // =====================================================
        // TEST CONNECTION METHODS
        // =====================================================
        private async void TestEmailConnection()
        {
            string email = GetActualText(txtSenderEmail, "Enter email...");
            string password = txtSenderPassword.Text;
            if (password == "Enter password...") password = "";
            string smtp = GetActualText(txtSmtpServer, "smtp.gmail.com");
            string port = GetActualText(txtSmtpPort, "587");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter email credentials first!", "Test Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "⏳ Testing email configuration...";
            lblStatus.ForeColor = Color.Orange;

            bool result = await EmailManager.TestEmailConfigurationAsync(smtp, int.Parse(port), email, password);

            if (result)
            {
                lblStatus.Text = "✅ Email test successful!";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("✅ Email configuration is working!\n\nTest email sent successfully.",
                    "Test Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "❌ Email test failed!";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("❌ Email test failed!\n\nPlease check your credentials and try again.\n\n" +
                    "For Gmail:\n1. Enable 2-Step Verification\n2. Generate App Password\n3. Use that password here",
                    "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TestSMSConnection()
        {
            string sid = GetActualText(txtTwilioSID, "Enter Account SID...");
            string token = GetActualText(txtTwilioToken, "Enter Auth Token...");
            string number = GetActualText(txtTwilioNumber, "+1234567890");

            if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Please enter Twilio credentials first!", "Test Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblStatus.Text = "⏳ Testing SMS configuration...";
            lblStatus.ForeColor = Color.Orange;

            bool result = await SMSSender.TestSMSConfigurationAsync(sid, token, number);

            if (result)
            {
                lblStatus.Text = "✅ SMS test successful!";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("✅ SMS configuration is working!", "Test Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "❌ SMS test failed!";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("❌ SMS test failed!\n\nPlease check your Twilio credentials.\n\nSign up at: https://www.twilio.com",
                    "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            AddSettingsMenuItem(menuPanel, "⚙", "General Settings", true, null);
            AddSettingsMenuItem(menuPanel, "📧", "Email Settings", false, () => tabControl.SelectedIndex = 1);
            AddSettingsMenuItem(menuPanel, "📱", "SMS Settings", false, () => tabControl.SelectedIndex = 2);
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

            // ✅ FIX: Sidebar BAAD MEIN add hoga
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
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter; iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter; textLbl.MouseLeave += leave;
            item.MouseEnter += enter; item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        // =====================================================
        // RIGHT CONTENT WITH TABS
        // =====================================================
        private void BuildContent()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(245, 247, 251);
            contentPanel.Padding = new Padding(40, 30, 40, 30);

            Panel cardPanel = new Panel();
            cardPanel.Dock = DockStyle.Fill;
            cardPanel.BackColor = Color.White;
            cardPanel.Padding = new Padding(30, 25, 30, 25);
            cardPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            Label cardTitle = new Label();
            cardTitle.Text = "⚙️ System Configuration";
            cardTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            cardTitle.ForeColor = Color.FromArgb(34, 34, 34);
            cardTitle.Dock = DockStyle.Top;
            cardTitle.Height = 50;

            lblStatus = new Label();
            lblStatus.Text = "Ready";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 25;
            lblStatus.TextAlign = ContentAlignment.MiddleRight;

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10);
            tabControl.SelectedIndexChanged += (s, e) => lblStatus.Text = $"Viewing: {tabControl.SelectedTab?.Text}";

            TabPage tabGeneral = new TabPage("⚙ General");
            tabGeneral.Controls.Add(BuildGeneralTab());
            tabControl.Controls.Add(tabGeneral);

            TabPage tabEmail = new TabPage("📧 Email");
            tabEmail.Controls.Add(BuildEmailTab());
            tabControl.Controls.Add(tabEmail);

            TabPage tabSMS = new TabPage("📱 SMS");
            tabSMS.Controls.Add(BuildSMSTab());
            tabControl.Controls.Add(tabSMS);

            Panel btnArea = new Panel();
            btnArea.Dock = DockStyle.Top;
            btnArea.Height = 70;
            btnArea.BackColor = Color.Transparent;
            btnArea.Padding = new Padding(0, 20, 0, 0);

            saveBtn = new Button();
            saveBtn.Text = "💾 Save All Settings";
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
            saveBtn.Click += (s, e) => SaveAllSettings();
            saveBtn.MouseEnter += (s, e) => saveBtn.BackColor = Color.FromArgb(100, 18, 23);
            saveBtn.MouseLeave += (s, e) => saveBtn.BackColor = Color.FromArgb(120, 22, 27);

            btnArea.Controls.Add(saveBtn);

            cardPanel.Controls.Add(tabControl);
            cardPanel.Controls.Add(btnArea);
            cardPanel.Controls.Add(lblStatus);
            cardPanel.Controls.Add(cardTitle);

            contentPanel.Controls.Add(cardPanel);

            // ✅ FIX: Content PEHLE add hoga
            this.Controls.Add(contentPanel);
        }

        // =====================================================
        // GENERAL SETTINGS TAB
        // =====================================================
        private Panel BuildGeneralTab()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20);
            panel.AutoScroll = true;

            TableLayoutPanel formTable = new TableLayoutPanel();
            formTable.Dock = DockStyle.Top;
            formTable.ColumnCount = 2;
            formTable.RowCount = 3;
            formTable.AutoSize = true;
            formTable.Padding = new Padding(0, 10, 0, 0);
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            formTable.Controls.Add(CreateField("System Name", "Enter system name...", true, out txtSystemName), 0, 0);
            formTable.Controls.Add(CreateField("Blood Bank Code", "Enter bank code...", false, out txtBankCode), 1, 0);
            formTable.Controls.Add(CreateField("Contact Email", "Enter email address...", true, out txtEmail), 0, 1);
            formTable.Controls.Add(CreateField("Contact Phone", "Enter phone number...", false, out txtPhone), 1, 1);

            Panel addressPanel = CreateField("Address", "Enter full address...", false, out txtAddress);
            formTable.Controls.Add(addressPanel, 0, 2);
            formTable.SetColumnSpan(addressPanel, 2);

            panel.Controls.Add(formTable);
            return panel;
        }

        // =====================================================
        // EMAIL SETTINGS TAB
        // =====================================================
        private Panel BuildEmailTab()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20);
            panel.AutoScroll = true;

            Panel infoBox = new Panel();
            infoBox.Dock = DockStyle.Top;
            infoBox.Height = 80;
            infoBox.BackColor = Color.FromArgb(255, 244, 229);
            infoBox.Margin = new Padding(0, 0, 0, 15);
            infoBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 8))
                    p.Region = new Region(path);
            };

            Label infoIcon = new Label();
            infoIcon.Text = "ℹ️";
            infoIcon.Font = new Font("Segoe UI Emoji", 16);
            infoIcon.ForeColor = Color.FromArgb(204, 122, 0);
            infoIcon.Location = new Point(12, 12);
            infoIcon.AutoSize = true;

            Label infoText = new Label();
            infoText.Text = "For Gmail: Enable 2-Step Verification and generate an App Password.\n" +
                           "Use the App Password (not your regular password) here.";
            infoText.Font = new Font("Segoe UI", 9);
            infoText.ForeColor = Color.FromArgb(204, 122, 0);
            infoText.Location = new Point(45, 12);
            infoText.Size = new Size(550, 35);
            infoText.AutoSize = false;

            infoBox.Controls.Add(infoIcon);
            infoBox.Controls.Add(infoText);

            TableLayoutPanel formTable = new TableLayoutPanel();
            formTable.Dock = DockStyle.Top;
            formTable.ColumnCount = 2;
            formTable.RowCount = 4;
            formTable.AutoSize = true;
            formTable.Padding = new Padding(0, 10, 0, 0);
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            formTable.Controls.Add(CreateField("SMTP Server", "smtp.gmail.com", true, out txtSmtpServer), 0, 0);
            formTable.Controls.Add(CreateField("SMTP Port", "587", false, out txtSmtpPort), 1, 0);
            formTable.Controls.Add(CreateField("Sender Email", "your-email@gmail.com", true, out txtSenderEmail), 0, 1);
            formTable.Controls.Add(CreateField("Sender Password", "Enter password...", false, out txtSenderPassword), 1, 1);
            txtSenderPassword.UseSystemPasswordChar = true;

            Panel btnPanel = new Panel();
            btnPanel.Dock = DockStyle.Top;
            btnPanel.Height = 60;
            btnPanel.BackColor = Color.Transparent;
            btnPanel.Padding = new Padding(0, 15, 0, 0);

            Button testBtn = new Button();
            testBtn.Text = "📧 Test Email Configuration";
            testBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            testBtn.ForeColor = Color.White;
            testBtn.BackColor = Color.FromArgb(13, 110, 253);
            testBtn.FlatStyle = FlatStyle.Flat;
            testBtn.FlatAppearance.BorderSize = 0;
            testBtn.Size = new Size(220, 38);
            testBtn.Cursor = Cursors.Hand;
            testBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(btn.ClientRectangle, 8))
                    btn.Region = new Region(path);
            };
            testBtn.Click += (s, e) => TestEmailConnection();
            testBtn.MouseEnter += (s, e) => testBtn.BackColor = Color.FromArgb(10, 90, 220);
            testBtn.MouseLeave += (s, e) => testBtn.BackColor = Color.FromArgb(13, 110, 253);

            btnPanel.Controls.Add(testBtn);

            panel.Controls.Add(btnPanel);
            panel.Controls.Add(formTable);
            panel.Controls.Add(infoBox);
            return panel;
        }

        // =====================================================
        // SMS SETTINGS TAB (Twilio)
        // =====================================================
        private Panel BuildSMSTab()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(20);
            panel.AutoScroll = true;

            Panel infoBox = new Panel();
            infoBox.Dock = DockStyle.Top;
            infoBox.Height = 80;
            infoBox.BackColor = Color.FromArgb(229, 244, 255);
            infoBox.Margin = new Padding(0, 0, 0, 15);
            infoBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 8))
                    p.Region = new Region(path);
            };

            Label infoIcon = new Label();
            infoIcon.Text = "ℹ️";
            infoIcon.Font = new Font("Segoe UI Emoji", 16);
            infoIcon.ForeColor = Color.FromArgb(0, 100, 200);
            infoIcon.Location = new Point(12, 12);
            infoIcon.AutoSize = true;

            Label infoText = new Label();
            infoText.Text = "Sign up at Twilio.com to get Account SID, Auth Token, and a phone number.\n" +
                           "Free trial gives you $15 credit (about 150 SMS messages).";
            infoText.Font = new Font("Segoe UI", 9);
            infoText.ForeColor = Color.FromArgb(0, 100, 200);
            infoText.Location = new Point(45, 12);
            infoText.Size = new Size(550, 35);
            infoText.AutoSize = false;

            infoBox.Controls.Add(infoIcon);
            infoBox.Controls.Add(infoText);

            TableLayoutPanel formTable = new TableLayoutPanel();
            formTable.Dock = DockStyle.Top;
            formTable.ColumnCount = 2;
            formTable.RowCount = 3;
            formTable.AutoSize = true;
            formTable.Padding = new Padding(0, 10, 0, 0);
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            formTable.Controls.Add(CreateField("Twilio Account SID", "Enter Account SID...", true, out txtTwilioSID), 0, 0);
            formTable.Controls.Add(CreateField("Twilio Auth Token", "Enter Auth Token...", false, out txtTwilioToken), 1, 0);

            Panel phonePanel = CreateField("Twilio Phone Number", "+1234567890", false, out txtTwilioNumber);
            formTable.Controls.Add(phonePanel, 0, 1);
            formTable.SetColumnSpan(phonePanel, 2);

            Panel btnPanel = new Panel();
            btnPanel.Dock = DockStyle.Top;
            btnPanel.Height = 60;
            btnPanel.BackColor = Color.Transparent;
            btnPanel.Padding = new Padding(0, 15, 0, 0);

            Button testBtn = new Button();
            testBtn.Text = "📱 Test SMS Configuration";
            testBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            testBtn.ForeColor = Color.White;
            testBtn.BackColor = Color.FromArgb(13, 110, 253);
            testBtn.FlatStyle = FlatStyle.Flat;
            testBtn.FlatAppearance.BorderSize = 0;
            testBtn.Size = new Size(220, 38);
            testBtn.Cursor = Cursors.Hand;
            testBtn.Paint += (s, e) =>
            {
                Button btn = (Button)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(btn.ClientRectangle, 8))
                    btn.Region = new Region(path);
            };
            testBtn.Click += (s, e) => TestSMSConnection();
            testBtn.MouseEnter += (s, e) => testBtn.BackColor = Color.FromArgb(10, 90, 220);
            testBtn.MouseLeave += (s, e) => testBtn.BackColor = Color.FromArgb(13, 110, 253);

            btnPanel.Controls.Add(testBtn);

            panel.Controls.Add(btnPanel);
            panel.Controls.Add(formTable);
            panel.Controls.Add(infoBox);
            return panel;
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================
        private Panel CreateField(string label, string placeholder, bool hasRightMargin, out TextBox textBox)
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.Transparent;
            container.Height = 90;
            container.Margin = new Padding(0, 0, hasRightMargin ? 15 : 0, 20);

            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.ForeColor = Color.FromArgb(85, 85, 85);
            lbl.BackColor = Color.Transparent;
            lbl.Dock = DockStyle.Top;
            lbl.Height = 20;

            TextBox txt = new TextBox();
            txt.Text = placeholder;
            txt.Font = new Font("Segoe UI", 11);
            txt.ForeColor = Color.Gray;
            txt.BackColor = Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(0, 10, 0, 0);
            txt.Tag = placeholder;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == txt.Tag?.ToString())
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(30, 30, 40);
                    if (label.Contains("Password"))
                        txt.UseSystemPasswordChar = true;
                }
            };

            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = txt.Tag?.ToString();
                    txt.ForeColor = Color.Gray;
                    if (label.Contains("Password"))
                        txt.UseSystemPasswordChar = false;
                }
            };

            textBox = txt;
            container.Controls.Add(txt);
            container.Controls.Add(lbl);
            return container;
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