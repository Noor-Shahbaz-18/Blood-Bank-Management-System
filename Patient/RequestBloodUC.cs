using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class RequestBloodUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);

        private TextBox txtFullName, txtCNIC, txtAge, txtContact, txtHospital, txtDepartment;
        private ComboBox cmbBloodGroup;
        private RadioButton rbNormal, rbUrgent, rbEmergency;
        private Button btnSubmit, btnReset;  // ✅ Buttons as class fields

        public RequestBloodUC()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.AutoScroll = true;

            // Outer scroll panel
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30, 25, 30, 25),
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(scrollPanel);

            // White card
            Panel formCard = new Panel
            {
                BackColor = Color.White,
                Width = 900,
                Height = 620,
                Location = new Point(0, 0),
                Padding = new Padding(40, 20, 40, 20)
            };
            scrollPanel.Controls.Add(formCard);

            // Auto-resize formCard to fit scrollPanel
            scrollPanel.Resize += (s, e) =>
            {
                formCard.Width = scrollPanel.ClientSize.Width - 60;
                formCard.Height = Math.Max(620, scrollPanel.ClientSize.Height - 50);
                formCard.Location = new Point(0, 0);
            };

            int y = 10;

            // ===== TITLE =====
            Label lblTitle = new Label
            {
                Text = "🩸 Request Blood",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = brickRed,
                Height = 50,
                Dock = DockStyle.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, y)
            };
            formCard.Controls.Add(lblTitle);
            formCard.Resize += (s, e) => { lblTitle.Width = formCard.ClientSize.Width - 80; };

            y += 60;

            // ===== ROW 1: Full Name | CNIC =====
            int colWidth = 380;
            int leftX = 0;
            int rightX = colWidth + 20;

            AddLabel(formCard, "Full Name", leftX, y);
            txtFullName = AddTextBox(formCard, "Enter full name", leftX, y + 22, colWidth);
            txtFullName.KeyPress += OnlyLetters_KeyPress;

            AddLabel(formCard, "CNIC", rightX, y);
            txtCNIC = AddTextBox(formCard, "xxxxx-xxxxxxx-x", rightX, y + 22, colWidth);
            txtCNIC.MaxLength = 15;
            txtCNIC.KeyPress += CNIC_KeyPress;

            y += 75;

            // ===== ROW 2: Age | Contact =====
            AddLabel(formCard, "Age", leftX, y);
            txtAge = AddTextBox(formCard, "Enter age", leftX, y + 22, colWidth);
            txtAge.KeyPress += OnlyNumbers_KeyPress;

            AddLabel(formCard, "Contact Number", rightX, y);
            txtContact = AddTextBox(formCard, "03XXXXXXXXX", rightX, y + 22, colWidth);
            txtContact.MaxLength = 11;
            txtContact.KeyPress += OnlyNumbers_KeyPress;

            y += 75;

            // ===== ROW 3: Hospital | Blood Group =====
            AddLabel(formCard, "Hospital Name", leftX, y);
            txtHospital = AddTextBox(formCard, "Enter hospital name", leftX, y + 22, colWidth);
            txtHospital.KeyPress += OnlyLetters_KeyPress;

            AddLabel(formCard, "Blood Group", rightX, y);
            cmbBloodGroup = new ComboBox
            {
                Location = new Point(rightX, y + 22),
                Size = new Size(colWidth, 36),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            cmbBloodGroup.Items.AddRange(new string[] { "Select Blood Group", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            formCard.Controls.Add(cmbBloodGroup);

            y += 75;

            // ===== ROW 4: Department (full width) =====
            AddLabel(formCard, "Department / Ward", leftX, y);
            txtDepartment = AddTextBox(formCard, "Optional", leftX, y + 22, colWidth * 2 + 20);

            y += 75;

            // ===== ROW 5: Urgency =====
            AddLabel(formCard, "Urgency Level", leftX, y);

            rbNormal = new RadioButton { Text = "Normal", Checked = true, AutoSize = true, Location = new Point(leftX, y + 26), Font = new Font("Segoe UI", 10) };
            rbUrgent = new RadioButton { Text = "Urgent", AutoSize = true, Location = new Point(leftX + 90, y + 26), Font = new Font("Segoe UI", 10) };
            rbEmergency = new RadioButton { Text = "Emergency", AutoSize = true, Location = new Point(leftX + 175, y + 26), Font = new Font("Segoe UI", 10), ForeColor = brickRed };

            formCard.Controls.Add(rbNormal);
            formCard.Controls.Add(rbUrgent);
            formCard.Controls.Add(rbEmergency);

            y += 75;

            // ===== ROW 6: Buttons =====
            btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(100, 42),
                Location = new Point(leftX + colWidth * 2 + 20 - 230, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI Semibold", 11),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnReset.FlatAppearance.BorderSize = 1;

            btnSubmit = new Button
            {
                Text = "Submit Request",
                Size = new Size(155, 42),
                Location = new Point(leftX + colWidth * 2 + 20 - 120, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;

            formCard.Controls.Add(btnReset);
            formCard.Controls.Add(btnSubmit);

            // ✅ BUTTON CLICK EVENTS
            btnSubmit.Click += BtnSubmit_Click;
            btnReset.Click += (s, e) => ResetForm();

            // Dynamically reposition buttons and resize textboxes on resize
            formCard.Resize += (s, e) =>
            {
                int cardW = formCard.ClientSize.Width - 80;
                int half = (cardW - 20) / 2;

                lblTitle.Width = cardW;

                if (txtFullName != null) txtFullName.Width = half;
                if (txtCNIC != null) { txtCNIC.Location = new Point(half + 20, txtCNIC.Location.Y); txtCNIC.Width = half; }
                if (txtAge != null) txtAge.Width = half;
                if (txtContact != null) { txtContact.Location = new Point(half + 20, txtContact.Location.Y); txtContact.Width = half; }
                if (txtHospital != null) txtHospital.Width = half;
                if (cmbBloodGroup != null) { cmbBloodGroup.Location = new Point(half + 20, cmbBloodGroup.Location.Y); cmbBloodGroup.Width = half; }
                if (txtDepartment != null) txtDepartment.Width = cardW;

                // Update label positions for right column
                foreach (Control c in formCard.Controls)
                {
                    if (c is Label lbl && lbl != lblTitle && lbl.Location.X > half)
                        lbl.Location = new Point(half + 20, lbl.Location.Y);
                }

                if (rbUrgent != null) rbUrgent.Location = new Point(rbNormal.Location.X + 100, rbUrgent.Location.Y);
                if (rbEmergency != null) rbEmergency.Location = new Point(rbNormal.Location.X + 185, rbEmergency.Location.Y);

                // ✅ BUTTON POSITIONS - always visible
                btnReset.Location = new Point(cardW - 230, btnReset.Location.Y);
                btnSubmit.Location = new Point(cardW - 120, btnSubmit.Location.Y);
            };

            // Placeholder setup
            SetPlaceholder(txtFullName, "Enter full name");
            SetPlaceholder(txtCNIC, "xxxxx-xxxxxxx-x");
            SetPlaceholder(txtAge, "Enter age");
            SetPlaceholder(txtContact, "03XXXXXXXXX");
            SetPlaceholder(txtHospital, "Enter hospital name");
            SetPlaceholder(txtDepartment, "Optional");
        }

        private Label AddLabel(Panel parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox AddTextBox(Panel parent, string placeholder, int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 36),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private string GetSelectedUrgency()
        {
            if (rbEmergency.Checked) return "Emergency";
            if (rbUrgent.Checked) return "Urgent";
            return "Normal";
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || txtFullName.Text == "Enter full name")
            {
                MessageBox.Show("Please enter patient full name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCNIC.Text) || txtCNIC.Text == "xxxxx-xxxxxxx-x" || txtCNIC.Text.Length < 15)
            {
                MessageBox.Show("Please enter valid CNIC (xxxxx-xxxxxxx-x).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCNIC.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAge.Text) || txtAge.Text == "Enter age")
            {
                MessageBox.Show("Please enter patient age.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAge.Focus();
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age) || age < 0 || age > 120)
            {
                MessageBox.Show("Please enter valid age (0-120).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAge.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContact.Text) || txtContact.Text == "03XXXXXXXXX" || txtContact.Text.Length < 11)
            {
                MessageBox.Show("Please enter valid contact number (11 digits).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtContact.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHospital.Text) || txtHospital.Text == "Enter hospital name")
            {
                MessageBox.Show("Please enter hospital name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHospital.Focus();
                return;
            }

            if (cmbBloodGroup.SelectedIndex == 0)
            {
                MessageBox.Show("Please select blood group.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBloodGroup.Focus();
                return;
            }

            try
            {
                string urgency = GetSelectedUrgency();
                int patientUserId = SessionManager.CurrentUserID;

                // ✅ DATABASE SAVE - Call the method
                bool saved = RequisitionDAL.SavePatientRequisition(
                    txtFullName.Text.Trim(),
                    txtCNIC.Text.Trim(),
                    cmbBloodGroup.Text,
                    1,  // Units
                    txtHospital.Text.Trim(),
                    urgency,
                    patientUserId,
                    txtDepartment.Text.Trim()
                );

                if (saved)
                {
                    MessageBox.Show(
                        $"✅ Blood Request Submitted Successfully!\n\n" +
                        $"Patient: {txtFullName.Text}\n" +
                        $"Blood Group: {cmbBloodGroup.Text}\n" +
                        $"Hospital: {txtHospital.Text}\n" +
                        $"Urgency: {urgency}\n\n" +
                        $"Your request is now pending approval.\n" +
                        $"You can track status in 'Request Status' page.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ResetForm();
                }
                else
                {
                    MessageBox.Show("Failed to submit request. Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nPlease check database connection.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            SetPlaceholder(txtFullName, "Enter full name");
            SetPlaceholder(txtCNIC, "xxxxx-xxxxxxx-x");
            SetPlaceholder(txtAge, "Enter age");
            SetPlaceholder(txtContact, "03XXXXXXXXX");
            SetPlaceholder(txtHospital, "Enter hospital name");
            SetPlaceholder(txtDepartment, "Optional");
            cmbBloodGroup.SelectedIndex = 0;
            rbNormal.Checked = true;
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                { textBox.Text = ""; textBox.ForeColor = Color.Black; }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                { textBox.Text = placeholder; textBox.ForeColor = Color.Gray; }
            };
        }

        private void OnlyLetters_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                e.Handled = true;
        }

        private void OnlyNumbers_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void CNIC_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsDigit(e.KeyChar)) { e.Handled = true; return; }
            if (txt.Text.Length == 5 || txt.Text.Length == 13)
            { txt.Text += "-"; txt.SelectionStart = txt.Text.Length; }
            if (txt.Text.Length >= 15)
                e.Handled = true;
        }
    }
}