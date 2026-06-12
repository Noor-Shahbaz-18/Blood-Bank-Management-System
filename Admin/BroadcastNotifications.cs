using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class BroadcastNotifications : Form
    {
        public BroadcastNotifications()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Broadcast Notifications";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = "📢 Broadcast Notifications",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            int y = 70, leftX = 20, fieldWidth = 510;

            // Select Audience
            mainPanel.Controls.Add(new Label
            {
                Text = "Send To:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });

            ComboBox cmbAudience = new ComboBox
            {
                Location = new Point(leftX, y + 22),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbAudience.Items.AddRange(new string[] {
                "All Users", "All Donors", "All Patients",
                "All Doctors", "All Technicians", "All Managers"
            });
            cmbAudience.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbAudience);
            y += 65;

            // Title
            mainPanel.Controls.Add(new Label
            {
                Text = "Notification Title:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });
            TextBox txtTitle = new TextBox
            {
                Location = new Point(leftX, y + 22),
                Width = fieldWidth,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtTitle);
            y += 65;

            // Message
            mainPanel.Controls.Add(new Label
            {
                Text = "Message:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });
            TextBox txtMessage = new TextBox
            {
                Location = new Point(leftX, y + 22),
                Width = fieldWidth,
                Height = 120,
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtMessage);
            y += 155;

            // Priority
            mainPanel.Controls.Add(new Label
            {
                Text = "Priority:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });
            ComboBox cmbPriority = new ComboBox
            {
                Location = new Point(leftX, y + 22),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11)
            };
            cmbPriority.Items.AddRange(new string[] { "Normal", "Important", "Urgent", "Emergency" });
            cmbPriority.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbPriority);
            y += 65;

            // Recipient Count Label
            Label lblCount = new Label
            {
                Text = "📊 Recipients: Calculating...",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            mainPanel.Controls.Add(lblCount);
            y += 35;

            // Update count when audience changes
            cmbAudience.SelectedIndexChanged += (s, e) =>
            {
                int count = GetRecipientCount(cmbAudience.Text);
                lblCount.Text = $"📊 Recipients: {count} user(s)";
            };

            int initialCount = GetRecipientCount("All Users");
            lblCount.Text = $"📊 Recipients: {initialCount} user(s)";

            Button btnSend = new Button
            {
                Text = "📨 Send Notification",
                Location = new Point(leftX + 150, y),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnSend);

            // ✅ SEND BUTTON - SAVES TO DATABASE
            btnSend.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Please enter notification title.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTitle.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtMessage.Text))
                {
                    MessageBox.Show("Please enter notification message.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMessage.Focus();
                    return;
                }

                int sentCount = SendNotificationToAudience(
                    cmbAudience.Text,
                    txtTitle.Text.Trim(),
                    txtMessage.Text.Trim(),
                    cmbPriority.Text
                );

                if (sentCount > 0)
                {
                    MessageBox.Show($"✅ Notification sent successfully!\n\n" +
                        $"📨 Title: {txtTitle.Text}\n" +
                        $"🎯 Audience: {cmbAudience.Text}\n" +
                        $"📊 Sent to: {sentCount} recipient(s)\n" +
                        $"⚡ Priority: {cmbPriority.Text}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("❌ Failed to send notification. No recipients found.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }

        private int GetRecipientCount(string audience)
        {
            try
            {
                switch (audience)
                {
                    case "All Users":
                        return AdminDAL.GetTotalUserCount();
                    case "All Donors":
                        return CommonDAL.GetTotalDonors();
                    case "All Patients":
                        return CommonDAL.GetTotalPatients();
                    case "All Doctors":
                        return AdminDAL.GetUserCountByRole("Doctor");
                    case "All Technicians":
                        return AdminDAL.GetUserCountByRole("Technician");
                    case "All Managers":
                        return AdminDAL.GetUserCountByRole("Manager");
                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private int SendNotificationToAudience(string audience, string title, string message, string priority)
        {
            int sentCount = 0;
            try
            {
                DataTable users = new DataTable();

                switch (audience)
                {
                    case "All Users":
                        users = AdminDAL.GetAllUsers();
                        break;
                    case "All Donors":
                        users = GetUsersByRole("Donor");
                        break;
                    case "All Patients":
                        users = GetUsersByRole("Patient");
                        break;
                    case "All Doctors":
                        users = GetUsersByRole("Doctor");
                        break;
                    case "All Technicians":
                        users = GetUsersByRole("Technician");
                        break;
                    case "All Managers":
                        users = GetUsersByRole("Manager");
                        break;
                }

                if (users != null && users.Rows.Count > 0)
                {
                    foreach (DataRow row in users.Rows)
                    {
                        int userId = Convert.ToInt32(row["UserID"]);

                        Notification notif = new Notification
                        {
                            UserID = userId,
                            Title = title,
                            Message = message,
                            Type = "Broadcast",
                            Priority = priority,
                            IsRead = false,
                            CreatedAt = DateTime.Now,
                            SentBy = SessionManager.CurrentUsername
                        };

                        if (NotificationDAL.Insert(notif))
                        {
                            sentCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sentCount;
        }

        private DataTable GetUsersByRole(string role)
        {
            try
            {
                return AdminDAL.GetUsersByRole(role);
            }
            catch
            {
                return new DataTable();
            }
        }
    }
}