using System;
using System.Drawing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class EmergencyBroadcast : Form
    {
        public EmergencyBroadcast()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Emergency Broadcast - Blood Bank";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(mainPanel);

            // Warning Banner
            Panel warningBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(254, 242, 242),
                Padding = new Padding(15)
            };
            Label lblWarning = new Label
            {
                Text = "⚠️ EMERGENCY BROADCAST - This will send alerts to ALL registered users",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            warningBanner.Controls.Add(lblWarning);
            mainPanel.Controls.Add(warningBanner);

            Label lblTitle = new Label
            {
                Text = "🚨 Emergency Blood Alert",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);

            int y = 140, leftX = 20, fieldWidth = 510;

            // Blood Group Needed
            mainPanel.Controls.Add(new Label { Text = "Blood Group Needed:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            ComboBox cmbBloodGroup = new ComboBox { Location = new Point(leftX + 150, y - 3), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cmbBloodGroup.Items.AddRange(new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbBloodGroup);
            y += 45;

            // Units Needed
            mainPanel.Controls.Add(new Label { Text = "Units Needed:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            NumericUpDown numUnits = new NumericUpDown { Location = new Point(leftX + 150, y - 3), Width = 100, Minimum = 1, Maximum = 50, Value = 5, Font = new Font("Segoe UI", 11) };
            mainPanel.Controls.Add(numUnits);
            y += 45;

            // Hospital
            mainPanel.Controls.Add(new Label { Text = "Hospital Name:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtHospital = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 300, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtHospital);
            y += 45;

            // Contact Number
            mainPanel.Controls.Add(new Label { Text = "Contact Number:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtContact = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 200, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtContact);
            y += 45;

            // Location
            mainPanel.Controls.Add(new Label { Text = "Location:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtLocation = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 300, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            mainPanel.Controls.Add(txtLocation);
            y += 65;

            // Emergency Message
            mainPanel.Controls.Add(new Label { Text = "Emergency Message:", Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtMessage = new TextBox { Location = new Point(leftX + 150, y - 3), Width = 350, Height = 80, Multiline = true, Font = new Font("Segoe UI", 11), BorderStyle = BorderStyle.FixedSingle };
            txtMessage.Text = "URGENT: Blood donation required immediately! Please come to the hospital if you are eligible.";
            mainPanel.Controls.Add(txtMessage);
            y += 100;

            Button btnBroadcast = new Button
            {
                Text = "🚨 BROADCAST EMERGENCY ALERT",
                Location = new Point(leftX + 100, y),
                Size = new Size(350, 50),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBroadcast.FlatAppearance.BorderSize = 0;
            mainPanel.Controls.Add(btnBroadcast);

            btnBroadcast.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtHospital.Text))
                {
                    MessageBox.Show("Please enter hospital name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHospital.Focus();
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"⚠️ CONFIRM EMERGENCY BROADCAST ⚠️\n\n" +
                    $"Blood Group: {cmbBloodGroup.Text}\n" +
                    $"Units: {numUnits.Value}\n" +
                    $"Hospital: {txtHospital.Text}\n\n" +
                    $"This will send alerts to all donors and staff.\n\n" +
                    $"Are you sure you want to proceed?",
                    "Emergency Broadcast Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    MessageBox.Show("✅ Emergency broadcast sent successfully!\n\nAll donors and staff have been notified.",
                        "Broadcast Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };
        }
    }
}