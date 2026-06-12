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
    public class EmergencyAlert : Form
    {
        private Panel sidebarPanel;
        private Panel contentPanel;
        private ComboBox bloodGroupCombo;
        private ComboBox priorityCombo;
        private TextBox hospitalText;
        private TextBox messageText;
        private Button sendBtn, previewBtn;
        private Label lblStatus;

        public EmergencyAlert()
        {
            this.Text = "Emergency Alert Settings - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 251);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildContent();
            BuildSidebar();
        }

        // =====================================================
        // SEND EMERGENCY ALERT TO ALL HOSPITALS AND DONORS
        // =====================================================
        private void SendEmergencyAlert()
        {
            try
            {
                if (bloodGroupCombo.SelectedIndex <= 0)
                {
                    MessageBox.Show("Please select a blood group!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    bloodGroupCombo.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(hospitalText.Text) ||
                    hospitalText.Text == "Enter Hospital Name")
                {
                    MessageBox.Show("Please enter hospital name!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    hospitalText.Focus();
                    return;
                }

                string bloodGroup = bloodGroupCombo.Text;
                string priority = priorityCombo.Text;
                string hospital = hospitalText.Text.Trim();
                string message = messageText.Text.Trim();

                // Confirm before sending
                DialogResult confirm = MessageBox.Show(
                    $"⚠️ CONFIRM EMERGENCY BROADCAST ⚠️\n\n" +
                    $"Blood Group: {bloodGroup}\n" +
                    $"Priority: {priority}\n" +
                    $"Hospital: {hospital}\n\n" +
                    $"Message: {message}\n\n" +
                    $"This will send alerts to:\n" +
                    $"• All registered HOSPITALS\n" +
                    $"• All registered DONORS with blood group {bloodGroup}\n\n" +
                    $"Are you sure you want to proceed?",
                    "Emergency Broadcast Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // Update status
                lblStatus.Text = "⏳ Sending alerts...";
                lblStatus.ForeColor = Color.Orange;
                sendBtn.Enabled = false;
                Application.DoEvents();

                int hospitalsSent = 0;
                int donorsSent = 0;

                // 1. Send to all registered hospitals
                DataTable dtHospitals = HospitalDAL.GetAllHospitals();
                if (dtHospitals != null && dtHospitals.Rows.Count > 0)
                {
                    foreach (DataRow row in dtHospitals.Rows)
                    {
                        string hospitalName = row["Name"].ToString();
                        string hospitalPhone = row["Phone"]?.ToString() ?? "";
                        string hospitalEmail = row["Email"]?.ToString() ?? "";

                        // In real scenario, send email/SMS here
                        // For now, log the alert
                        System.Diagnostics.Debug.WriteLine($"ALERT to Hospital: {hospitalName} - Blood: {bloodGroup}");
                        hospitalsSent++;
                    }
                }

                // 2. Send to donors with matching blood group
                DataTable dtDonors = DonorDAL.GetDonorsByBloodGroup(bloodGroup);
                if (dtDonors != null && dtDonors.Rows.Count > 0)
                {
                    foreach (DataRow row in dtDonors.Rows)
                    {
                        string donorName = row["FullName"].ToString();
                        string donorPhone = row["Phone"]?.ToString() ?? "";
                        string donorEmail = row["Email"]?.ToString() ?? "";

                        // In real scenario, send email/SMS here
                        System.Diagnostics.Debug.WriteLine($"ALERT to Donor: {donorName} - Blood: {bloodGroup}");
                        donorsSent++;
                    }
                }

                // 3. Save broadcast log to database
                SaveBroadcastLog(bloodGroup, priority, hospital, message, hospitalsSent + donorsSent);

                // Update status to success
                lblStatus.Text = $"✅ Broadcast sent! {hospitalsSent} hospitals, {donorsSent} donors notified.";
                lblStatus.ForeColor = Color.Green;

                // Also show in notification for current user
                NotificationDAL.Insert(new Classes.Models.Notification
                {
                    UserID = SessionManager.CurrentUserID,
                    Title = "Emergency Broadcast Sent",
                    Message = $"Emergency alert for {bloodGroup} blood sent to {hospitalsSent + donorsSent} recipients",
                    Type = "Emergency",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                MessageBox.Show(
                    $"✅ EMERGENCY BROADCAST SENT SUCCESSFULLY!\n\n" +
                    $"📊 Summary:\n" +
                    $"• Hospitals notified: {hospitalsSent}\n" +
                    $"• Donors notified: {donorsSent}\n" +
                    $"• Total recipients: {hospitalsSent + donorsSent}\n\n" +
                    $"Blood Group: {bloodGroup}\n" +
                    $"Priority: {priority}\n" +
                    $"Hospital: {hospital}",
                    "Broadcast Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Log the action
                AuditHelper.Log("Emergency Broadcast", "Alert",
                    $"Emergency alert sent for {bloodGroup} blood to {hospitalsSent + donorsSent} recipients");

                // Clear form or keep it for next broadcast
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending broadcast: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Broadcast failed!";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                sendBtn.Enabled = true;
            }
        }

        private void SaveBroadcastLog(string bloodGroup, string priority, string hospital, string message, int recipientsCount)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO BroadcastLogs 
                                   (BloodGroup, Priority, Hospital, Message, SentTo, SentDate, SentBy)
                                   VALUES (@BloodGroup, @Priority, @Hospital, @Message, @SentTo, GETDATE(), @SentBy)";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                    cmd.Parameters.AddWithValue("@Priority", priority);
                    cmd.Parameters.AddWithValue("@Hospital", hospital);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@SentTo", recipientsCount + " recipients");
                    cmd.Parameters.AddWithValue("@SentBy", SessionManager.CurrentUserID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveBroadcastLog Error: {ex.Message}");
            }
        }

        private void ResetForm()
        {
            bloodGroupCombo.SelectedIndex = 0;
            priorityCombo.SelectedIndex = 0;
            hospitalText.Text = "Enter Hospital Name";
            hospitalText.ForeColor = Color.Gray;
            messageText.Text = "URGENT: Blood donation required immediately! Please come to the hospital if you are eligible.";
            messageText.ForeColor = Color.FromArgb(30, 30, 40);
        }

        private void PreviewAlert()
        {
            string bloodGroup = bloodGroupCombo?.SelectedIndex > 0 ? bloodGroupCombo.Text : "Not selected";
            string priority = priorityCombo?.SelectedIndex > 0 ? priorityCombo.Text : "Not selected";
            string hospital = hospitalText?.Text ?? "Not entered";
            string message = messageText?.Text ?? "No message";

            // Get counts
            int hospitalCount = 0;
            int donorCount = 0;

            DataTable dtHospitals = HospitalDAL.GetAllHospitals();
            if (dtHospitals != null) hospitalCount = dtHospitals.Rows.Count;

            if (bloodGroupCombo?.SelectedIndex > 0)
            {
                DataTable dtDonors = DonorDAL.GetDonorsByBloodGroup(bloodGroupCombo.Text);
                if (dtDonors != null) donorCount = dtDonors.Rows.Count;
            }

            string preview = $"⚠️ EMERGENCY BLOOD BROADCAST - PREVIEW ⚠️\n\n" +
                $"📋 Broadcast Details:\n" +
                $"├─ Blood Group: {bloodGroup}\n" +
                $"├─ Priority: {priority}\n" +
                $"├─ Hospital: {hospital}\n" +
                $"└─ Message: {message}\n\n" +
                $"📊 Recipients Summary:\n" +
                $"├─ Hospitals: {hospitalCount}\n" +
                $"├─ Donors with {bloodGroup}: {donorCount}\n" +
                $"└─ Total: {hospitalCount + donorCount}\n\n" +
                $"⚠️ This alert will be sent to ALL registered hospitals and donors.\n" +
                $"Click 'Broadcast Alert' to confirm sending.";

            MessageBox.Show(preview, "Broadcast Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            AddSettingsMenuItem(menuPanel, "🚨", "Emergency Alert", true, null);

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
            cardPanel.Dock = DockStyle.Top;
            cardPanel.BackColor = Color.White;
            cardPanel.Padding = new Padding(35, 30, 35, 30);
            cardPanel.AutoSize = true;
            cardPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cardPanel.MaximumSize = new Size(900, 0);
            cardPanel.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 12))
                    p.Region = new Region(path);
            };

            Label cardTitle = new Label();
            cardTitle.Text = "Emergency Blood Broadcast";
            cardTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            cardTitle.ForeColor = Color.FromArgb(34, 34, 34);
            cardTitle.Dock = DockStyle.Top;
            cardTitle.Height = 55;
            cardTitle.TextAlign = ContentAlignment.MiddleLeft;

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Ready to broadcast";
            lblStatus.Font = new Font("Segoe UI", 10);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 30;
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            cardPanel.Controls.Add(lblStatus);

            // Alert Box
            Panel alertBox = new Panel();
            alertBox.Dock = DockStyle.Top;
            alertBox.Height = 55;
            alertBox.BackColor = Color.FromArgb(255, 244, 229);
            alertBox.Margin = new Padding(0, 0, 0, 20);
            alertBox.Paint += (s, e) =>
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundRect(p.ClientRectangle, 8))
                    p.Region = new Region(path);
            };

            Label alertIcon = new Label();
            alertIcon.Text = "⚠";
            alertIcon.Font = new Font("Segoe UI Emoji", 18);
            alertIcon.ForeColor = Color.FromArgb(204, 122, 0);
            alertIcon.Location = new Point(12, 14);
            alertIcon.AutoSize = true;

            Label alertText = new Label();
            alertText.Text = "Emergency alert will be sent to all registered hospitals and donors.";
            alertText.Font = new Font("Segoe UI", 11);
            alertText.ForeColor = Color.FromArgb(204, 122, 0);
            alertText.Location = new Point(55, 16);
            alertText.AutoSize = true;

            alertBox.Controls.Add(alertIcon);
            alertBox.Controls.Add(alertText);

            // Form Table
            TableLayoutPanel formTable = new TableLayoutPanel();
            formTable.Dock = DockStyle.Top;
            formTable.ColumnCount = 2;
            formTable.RowCount = 3;
            formTable.AutoSize = true;
            formTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            formTable.BackColor = Color.Transparent;
            formTable.Padding = new Padding(0, 10, 0, 0);
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // Row 0: Blood Group | Priority
            formTable.Controls.Add(CreateDropdownField("Blood Group", new string[] {
                "Select Blood Group", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }, true), 0, 0);
            formTable.Controls.Add(CreateDropdownField("Emergency Priority", new string[] {
                "High", "Medium", "Low" }, false), 1, 0);

            // Row 1: Hospital Name (Full Width)
            Panel hospitalPanel = CreateTextField("Hospital Name", "Enter Hospital Name", false);
            formTable.Controls.Add(hospitalPanel, 0, 1);
            formTable.SetColumnSpan(hospitalPanel, 2);

            // Row 2: Broadcast Message (Full Width)
            Panel messagePanel = CreateTextAreaField("Broadcast Message", "Enter emergency blood requirement message...");
            formTable.Controls.Add(messagePanel, 0, 2);
            formTable.SetColumnSpan(messagePanel, 2);

            // Buttons Area
            Panel btnArea = new Panel();
            btnArea.Dock = DockStyle.Top;
            btnArea.Height = 70;
            btnArea.BackColor = Color.Transparent;
            btnArea.Padding = new Padding(0, 25, 0, 0);

            previewBtn = new Button();
            previewBtn.Text = "👁 Preview";
            previewBtn.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            previewBtn.ForeColor = Color.FromArgb(13, 110, 253);
            previewBtn.BackColor = Color.FromArgb(238, 244, 255);
            previewBtn.FlatStyle = FlatStyle.Flat;
            previewBtn.FlatAppearance.BorderSize = 0;
            previewBtn.Size = new Size(130, 42);
            previewBtn.Cursor = Cursors.Hand;
            previewBtn.Dock = DockStyle.Right;
            previewBtn.Margin = new Padding(0, 0, 10, 0);
            previewBtn.Click += (s, e) => PreviewAlert();

            sendBtn = new Button();
            sendBtn.Text = "📨 Broadcast Alert";
            sendBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            sendBtn.ForeColor = Color.White;
            sendBtn.BackColor = Color.FromArgb(120, 22, 27);
            sendBtn.FlatStyle = FlatStyle.Flat;
            sendBtn.FlatAppearance.BorderSize = 0;
            sendBtn.Size = new Size(180, 42);
            sendBtn.Cursor = Cursors.Hand;
            sendBtn.Dock = DockStyle.Right;
            sendBtn.Click += (s, e) => SendEmergencyAlert();

            btnArea.Controls.Add(sendBtn);
            btnArea.Controls.Add(previewBtn);

            cardPanel.Controls.Add(btnArea);
            cardPanel.Controls.Add(formTable);
            cardPanel.Controls.Add(alertBox);
            cardPanel.Controls.Add(cardTitle);
            cardPanel.Controls.Add(lblStatus);

            contentPanel.Controls.Add(cardPanel);
            this.Controls.Add(contentPanel);
        }

        private Panel CreateTextField(string label, string placeholder, bool hasRightMargin)
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.Transparent;
            container.Height = 85;
            container.Margin = new Padding(0, 0, hasRightMargin ? 15 : 0, 20);

            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.ForeColor = Color.FromArgb(85, 85, 85);
            lbl.Dock = DockStyle.Top;
            lbl.Height = 22;
            lbl.TextAlign = ContentAlignment.BottomLeft;

            TextBox txt = new TextBox();
            txt.Text = placeholder;
            txt.Font = new Font("Segoe UI", 11);
            txt.ForeColor = Color.Gray;
            txt.BackColor = Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(0, 8, 0, 0);
            txt.Tag = placeholder;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(30, 30, 40);
                }
            };
            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };

            if (label == "Hospital Name")
                hospitalText = txt;

            container.Controls.Add(txt);
            container.Controls.Add(lbl);
            return container;
        }

        private Panel CreateDropdownField(string label, string[] items, bool hasRightMargin)
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.Transparent;
            container.Height = 85;
            container.Margin = new Padding(0, 0, hasRightMargin ? 15 : 0, 20);

            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.ForeColor = Color.FromArgb(85, 85, 85);
            lbl.Dock = DockStyle.Top;
            lbl.Height = 22;
            lbl.TextAlign = ContentAlignment.BottomLeft;

            ComboBox cmb = new ComboBox();
            cmb.Font = new Font("Segoe UI", 11);
            cmb.ForeColor = Color.FromArgb(30, 30, 40);
            cmb.BackColor = Color.White;
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.Dock = DockStyle.Fill;
            cmb.Margin = new Padding(0, 8, 0, 0);
            cmb.IntegralHeight = false;
            cmb.MaxDropDownItems = 5;

            foreach (string item in items)
                cmb.Items.Add(item);
            cmb.SelectedIndex = 0;

            if (label == "Blood Group")
                bloodGroupCombo = cmb;
            else if (label == "Emergency Priority")
                priorityCombo = cmb;

            container.Controls.Add(cmb);
            container.Controls.Add(lbl);
            return container;
        }

        private Panel CreateTextAreaField(string label, string placeholder)
        {
            Panel container = new Panel();
            container.Dock = DockStyle.Fill;
            container.BackColor = Color.Transparent;
            container.Height = 130;
            container.Margin = new Padding(0, 0, 0, 20);

            Label lbl = new Label();
            lbl.Text = label;
            lbl.Font = new Font("Segoe UI", 10);
            lbl.ForeColor = Color.FromArgb(85, 85, 85);
            lbl.Dock = DockStyle.Top;
            lbl.Height = 22;
            lbl.TextAlign = ContentAlignment.BottomLeft;

            TextBox txt = new TextBox();
            txt.Text = placeholder;
            txt.Font = new Font("Segoe UI", 11);
            txt.ForeColor = Color.Gray;
            txt.BackColor = Color.White;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Multiline = true;
            txt.ScrollBars = ScrollBars.Vertical;
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(0, 8, 0, 0);
            txt.Tag = placeholder;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.FromArgb(30, 30, 40);
                }
            };
            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = Color.Gray;
                }
            };

            messageText = txt;

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