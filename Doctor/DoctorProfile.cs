using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class DoctorProfile : Form
    {
        private int _doctorId;
        private bool _isEditMode = false;

        public DoctorProfile()
        {
            InitializeComponent();
            _doctorId = SessionManager.CurrentUserID;
            BuildUI();
            LoadProfile();
        }

        private void BuildUI()
        {
            this.Text = "Doctor Profile";
            this.Size = new Size(700, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25), AutoScroll = true };
            this.Controls.Add(mainPanel);

            // Profile Header
            Panel headerPanel = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.White };
            mainPanel.Controls.Add(headerPanel);

            // Avatar
            Panel avatarPanel = new Panel
            {
                Size = new Size(80, 80),
                Location = new Point(25, 10),
                BackColor = Color.FromArgb(120, 22, 27)
            };
            avatarPanel.Paint += (s, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, 80, 80);
                    avatarPanel.Region = new Region(path);
                }
                TextRenderer.DrawText(e.Graphics, "DR", new Font("Segoe UI", 24, FontStyle.Bold), new Rectangle(0, 0, 80, 80), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            headerPanel.Controls.Add(avatarPanel);

            Label lblName = new Label
            {
                Text = SessionManager.CurrentFullName,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Location = new Point(120, 25),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblName);

            Label lblRole = new Label
            {
                Text = "Doctor • Hematologist",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(122, 60),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblRole);

            Button btnEdit = new Button
            {
                Text = "✏️ Edit Profile",
                Location = new Point(headerPanel.Width - 130, 30),
                Size = new Size(110, 38),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => ToggleEditMode();
            headerPanel.Controls.Add(btnEdit);

            // Profile Details Card
            Panel detailsCard = new Panel { Dock = DockStyle.Top, Height = 450, BackColor = Color.White, Margin = new Padding(0, 15, 0, 0), Padding = new Padding(25) };
            mainPanel.Controls.Add(detailsCard);

            int y = 20, leftX = 20, labelWidth = 150, fieldWidth = 400;

            // Personal Information Section
            detailsCard.Controls.Add(new Label { Text = "Personal Information", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(120, 22, 27), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            AddProfileField(detailsCard, "Full Name:", SessionManager.CurrentFullName, ref y, leftX, labelWidth, fieldWidth, "txtFullName");
            AddProfileField(detailsCard, "Email:", SessionManager.CurrentEmail, ref y, leftX, labelWidth, fieldWidth, "txtEmail");
            AddProfileField(detailsCard, "Phone:", SessionManager.CurrentPhone, ref y, leftX, labelWidth, fieldWidth, "txtPhone");

            y += 10;
            detailsCard.Controls.Add(new Label { Text = "Professional Information", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(120, 22, 27), Location = new Point(leftX, y), AutoSize = true });
            y += 35;

            AddProfileField(detailsCard, "Specialization:", "Hematology", ref y, leftX, labelWidth, fieldWidth, "txtSpecialization");
            AddProfileField(detailsCard, "PMDC Number:", "12345", ref y, leftX, labelWidth, fieldWidth, "txtPMDC");
            AddProfileField(detailsCard, "Hospital:", "City Hospital", ref y, leftX, labelWidth, fieldWidth, "txtHospital");
            AddProfileField(detailsCard, "Experience:", "8 years", ref y, leftX, labelWidth, fieldWidth, "txtExperience");
            AddProfileField(detailsCard, "Qualification:", "MBBS, FCPS", ref y, leftX, labelWidth, fieldWidth, "txtQualification");

            // Save Button (initially hidden)
            Button btnSave = new Button
            {
                Text = "💾 Save Changes",
                Location = new Point(leftX + 250, y + 30),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => SaveProfile();
            detailsCard.Controls.Add(btnSave);

            detailsCard.Tag = btnSave;
        }

        private void AddProfileField(Panel parent, string label, string value, ref int y, int leftX, int labelWidth, int fieldWidth, string controlName)
        {
            Label lbl = new Label
            {
                Text = label,
                Location = new Point(leftX, y),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90)
            };
            parent.Controls.Add(lbl);

            TextBox txt = new TextBox
            {
                Name = controlName,
                Text = value,
                Location = new Point(leftX + labelWidth, y),
                Size = new Size(fieldWidth, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            parent.Controls.Add(txt);
            y += 45;
        }

        private void ToggleEditMode()
        {
            _isEditMode = !_isEditMode;
            Button btnSave = null;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel p)
                {
                    foreach (Control inner in p.Controls)
                    {
                        if (inner is Panel innerP)
                        {
                            foreach (Control innerCtrl in innerP.Controls)
                            {
                                if (innerCtrl is TextBox txt)
                                {
                                    txt.ReadOnly = !_isEditMode;
                                    txt.BackColor = _isEditMode ? Color.White : Color.FromArgb(250, 250, 252);
                                }
                                if (innerCtrl is Button btn && btn.Text == "💾 Save Changes")
                                    btnSave = btn;
                            }
                        }
                        if (inner is TextBox txt2)
                        {
                            txt2.ReadOnly = !_isEditMode;
                            txt2.BackColor = _isEditMode ? Color.White : Color.FromArgb(250, 250, 252);
                        }
                        if (inner is Button btn2 && btn2.Text == "💾 Save Changes")
                            btnSave = btn2;
                    }
                }
            }

            if (btnSave != null)
                btnSave.Visible = _isEditMode;
        }

        private void SaveProfile()
        {
            // Get updated values and save to database
            MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _isEditMode = false;
            ToggleEditMode();
        }

        private void LoadProfile()
        {
            // Load doctor data from database using _doctorId
        }
    }
}