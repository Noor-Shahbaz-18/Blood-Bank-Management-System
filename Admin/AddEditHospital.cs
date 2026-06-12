using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class AddEditHospital : Form
    {
        private int _hospitalId = 0;
        private bool _isEditMode = false;

        private TextBox txtName, txtCity, txtAddress, txtPhone, txtEmail, txtContactPerson;
        private Button btnSave, btnCancel;
        private Label lblTitle;

        public AddEditHospital(int hospitalId = 0)
        {
            InitializeComponent();
            _hospitalId = hospitalId;
            _isEditMode = hospitalId > 0;
            BuildUI();
            if (_isEditMode) LoadHospitalData();
        }

        private void BuildUI()
        {
            this.Text = _isEditMode ? "Edit Hospital" : "Add Hospital";
            this.Size = new Size(560, 530);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ── Ek hi Panel, sab absolute coordinates mein ──────────
            Panel mainPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(560, 530),
                BackColor = Color.White,
                AutoScroll = false
            };
            this.Controls.Add(mainPanel);

            int leftX = 30;
            int labelW = 130;
            int fieldW = 350;
            int fieldX = leftX + labelW + 10;
            int y = 20;

            // ── Title ────────────────────────────────────────────────
            lblTitle = new Label
            {
                Text = _isEditMode ? "✏️  Edit Hospital" : "🏥  Add New Hospital",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                Width = fieldW + labelW + 10,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(lblTitle);
            y += 55;

            // ── Helper lambda ────────────────────────────────────────
            TextBox AddRow(string labelText)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Location = new Point(leftX, y + 5),
                    Size = new Size(labelW, 24),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 70)
                };
                var txt = new TextBox
                {
                    Location = new Point(fieldX, y),
                    Size = new Size(fieldW, 30),
                    Font = new Font("Segoe UI", 11),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.FromArgb(250, 250, 252)
                };
                mainPanel.Controls.Add(lbl);
                mainPanel.Controls.Add(txt);
                y += 52;
                return txt;
            }

            // ── Fields ───────────────────────────────────────────────
            txtName = AddRow("Hospital Name:");
            txtCity = AddRow("City:");
            txtAddress = AddRow("Address:");
            txtPhone = AddRow("Phone:");
            txtEmail = AddRow("Email:");
            txtContactPerson = AddRow("Contact Person:");

            y += 15; // extra space fields aur buttons ke beech

            // ── Buttons — fields ke NEECHE ───────────────────────────
            int btnY = y;
            int totalBtns = 240;
            int startX = (this.ClientSize.Width - totalBtns) / 2;

            btnSave = new Button
            {
                Text = "💾  Save",
                Location = new Point(startX, btnY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(startX + 130, btnY),
                Size = new Size(110, 42),
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);

            // Form ki height content ke mutabiq
            this.ClientSize = new Size(560, btnY + 42 + 25);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter hospital name.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                bool success;
                if (_isEditMode)
                    success = HospitalDAL.UpdateHospital(
                        _hospitalId,
                        txtName.Text.Trim(), txtCity.Text.Trim(),
                        txtAddress.Text.Trim(), txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(), txtContactPerson.Text.Trim());
                else
                    success = HospitalDAL.InsertHospital(
                        txtName.Text.Trim(), txtCity.Text.Trim(),
                        txtAddress.Text.Trim(), txtPhone.Text.Trim(),
                        txtEmail.Text.Trim(), txtContactPerson.Text.Trim());

                if (success)
                {
                    MessageBox.Show(
                        $"Hospital {(_isEditMode ? "updated" : "added")} successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save hospital. Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHospitalData()
        {
            try
            {
                var h = HospitalDAL.GetHospitalByID(_hospitalId);
                if (h == null) return;
                txtName.Text = h.Name ?? "";
                txtCity.Text = h.City ?? "";
                txtAddress.Text = h.Address ?? "";
                txtPhone.Text = h.Phone ?? "";
                txtEmail.Text = h.Email ?? "";
                txtContactPerson.Text = h.ContactPerson ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}