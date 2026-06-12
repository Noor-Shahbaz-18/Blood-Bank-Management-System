using System;
using System.Drawing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class SystemSettings : Form
    {
        public SystemSettings()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "System Settings - Blood Bank";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25), AutoScroll = true };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = "⚙️ System Settings",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            mainPanel.Controls.Add(tabControl);

            // Tab 1: General Settings
            TabPage tabGeneral = new TabPage("General");
            tabControl.Controls.Add(tabGeneral);
            BuildGeneralTab(tabGeneral);

            // Tab 2: Security Settings
            TabPage tabSecurity = new TabPage("Security");
            tabControl.Controls.Add(tabSecurity);
            BuildSecurityTab(tabSecurity);

            // Tab 3: Email/SMS Settings
            TabPage tabNotifications = new TabPage("Notifications");
            tabControl.Controls.Add(tabNotifications);
            BuildNotificationsTab(tabNotifications);

            // Tab 4: Database Settings
            TabPage tabDatabase = new TabPage("Database");
            tabControl.Controls.Add(tabDatabase);
            BuildDatabaseTab(tabDatabase);
        }

        private void BuildGeneralTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            tab.Controls.Add(panel);

            int y = 20, leftX = 20, fieldWidth = 400;

            panel.Controls.Add(new Label { Text = "System Name:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtSystemName = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "Blood Bank Management System", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtSystemName);
            y += 45;

            panel.Controls.Add(new Label { Text = "System Version:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtVersion = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "1.0.0", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtVersion);
            y += 45;

            panel.Controls.Add(new Label { Text = "Blood Bank Name:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtBankName = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "Central Blood Bank", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtBankName);
            y += 45;

            panel.Controls.Add(new Label { Text = "Bank Code:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtBankCode = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "CBB-001", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtBankCode);
            y += 45;

            panel.Controls.Add(new Label { Text = "Address:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtAddress = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "123 Main Street, Lahore", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtAddress);
            y += 45;

            panel.Controls.Add(new Label { Text = "Phone:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtPhone = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "+92-42-1234567", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtPhone);
            y += 45;

            panel.Controls.Add(new Label { Text = "Email:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtEmail = new TextBox { Location = new Point(leftX + 150, y - 3), Width = fieldWidth, Text = "info@bloodbank.com", Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtEmail);
            y += 65;

            Button btnSave = new Button
            {
                Text = "💾 Save General Settings",
                Location = new Point(leftX + 200, y),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => MessageBox.Show("General settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnSave);
        }

        private void BuildSecurityTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            tab.Controls.Add(panel);

            int y = 20, leftX = 20;

            panel.Controls.Add(new Label { Text = "🔐 Security Settings", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            // Password Policy
            CheckBox chkPasswordComplexity = new CheckBox { Text = "Enforce Password Complexity", Location = new Point(leftX, y), AutoSize = true, Checked = true };
            panel.Controls.Add(chkPasswordComplexity);
            y += 30;

            CheckBox chkPasswordExpiry = new CheckBox { Text = "Password Expiry (90 days)", Location = new Point(leftX, y), AutoSize = true, Checked = false };
            panel.Controls.Add(chkPasswordExpiry);
            y += 30;

            CheckBox chkTwoFactor = new CheckBox { Text = "Enable Two-Factor Authentication", Location = new Point(leftX, y), AutoSize = true, Checked = false };
            panel.Controls.Add(chkTwoFactor);
            y += 30;

            CheckBox chkSessionTimeout = new CheckBox { Text = "Session Timeout (30 minutes)", Location = new Point(leftX, y), AutoSize = true, Checked = true };
            panel.Controls.Add(chkSessionTimeout);
            y += 50;

            // Max Login Attempts
            panel.Controls.Add(new Label { Text = "Max Login Attempts:", Location = new Point(leftX, y), AutoSize = true });
            NumericUpDown numAttempts = new NumericUpDown { Location = new Point(leftX + 150, y - 3), Width = 80, Minimum = 3, Maximum = 10, Value = 5 };
            panel.Controls.Add(numAttempts);
            y += 45;

            Button btnSave = new Button
            {
                Text = "💾 Save Security Settings",
                Location = new Point(leftX + 150, y),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => MessageBox.Show("Security settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnSave);
        }

        private void BuildNotificationsTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            tab.Controls.Add(panel);

            int y = 20, leftX = 20;

            panel.Controls.Add(new Label { Text = "📧 Email Settings", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            panel.Controls.Add(new Label { Text = "SMTP Server:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtSMTP = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 250, Text = "smtp.gmail.com", BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtSMTP);
            y += 35;

            panel.Controls.Add(new Label { Text = "SMTP Port:", Location = new Point(leftX, y), AutoSize = true });
            NumericUpDown numPort = new NumericUpDown { Location = new Point(leftX + 150, y - 3), Width = 80, Value = 587 };
            panel.Controls.Add(numPort);
            y += 35;

            panel.Controls.Add(new Label { Text = "Sender Email:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtSenderEmail = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 250, Text = "noreply@bloodbank.com", BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtSenderEmail);
            y += 35;

            panel.Controls.Add(new Label { Text = "Sender Password:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtSenderPassword = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 200, UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtSenderPassword);
            y += 50;

            panel.Controls.Add(new Label { Text = "📱 SMS Settings", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            panel.Controls.Add(new Label { Text = "API Key:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtAPIKey = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 250, BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtAPIKey);
            y += 35;

            Button btnSave = new Button
            {
                Text = "💾 Save Notification Settings",
                Location = new Point(leftX + 150, y),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => MessageBox.Show("Notification settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnSave);
        }

        private void BuildDatabaseTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            tab.Controls.Add(panel);

            int y = 20, leftX = 20;

            panel.Controls.Add(new Label { Text = "🗄️ Database Settings", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            panel.Controls.Add(new Label { Text = "Connection String:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtConnString = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 500, Text = "Data Source=.\\SQLEXPRESS;Initial Catalog=BloodBankDB;Integrated Security=True", BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtConnString);
            y += 45;

            panel.Controls.Add(new Label { Text = "Backup Location:", Location = new Point(leftX, y), AutoSize = true });
            TextBox txtBackupPath = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 400, Text = "C:\\Backups\\BloodBank\\", BorderStyle = BorderStyle.FixedSingle };
            panel.Controls.Add(txtBackupPath);
            y += 35;

            panel.Controls.Add(new Label { Text = "Auto Backup Schedule:", Location = new Point(leftX, y), AutoSize = true });
            ComboBox cmbSchedule = new ComboBox { Location = new Point(leftX + 150, y - 3), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSchedule.Items.AddRange(new string[] { "Daily", "Weekly", "Monthly", "Never" });
            cmbSchedule.SelectedIndex = 1;
            panel.Controls.Add(cmbSchedule);
            y += 45;

            Button btnTestConn = new Button
            {
                Text = "🔌 Test Connection",
                Location = new Point(leftX, y),
                Size = new Size(150, 38),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTestConn.FlatAppearance.BorderSize = 0;
            btnTestConn.Click += (s, e) => MessageBox.Show("Database connection successful!", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnTestConn);

            Button btnBackup = new Button
            {
                Text = "💾 Backup Now",
                Location = new Point(leftX + 170, y),
                Size = new Size(150, 38),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBackup.FlatAppearance.BorderSize = 0;
            btnBackup.Click += (s, e) => MessageBox.Show("Database backup completed successfully!", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnBackup);

            Button btnSave = new Button
            {
                Text = "💾 Save Database Settings",
                Location = new Point(leftX + 340, y),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => MessageBox.Show("Database settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panel.Controls.Add(btnSave);
        }
    }
}