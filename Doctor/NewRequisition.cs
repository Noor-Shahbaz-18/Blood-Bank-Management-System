using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class NewRequisition : UserControl
    {
        // ── Brand colour ─────────────────────────────────────────────────────────
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private string selectedFilePath = "";

        // ── Controls declared as fields → compiler always treats them as assigned ─
        private Label lblTitle;
        private Panel divider;
        private Label lblPatient, lblCnic, lblBlood, lblHospital;
        private Label lblUrgency, lblAttach;
        private TextBox txtPatient, txtCNIC, txtHospital;
        private ComboBox cmbBlood;
        private RadioButton rbNormal, rbUrgent, rbEmergency;
        private Panel uploadPanel;
        private Label lblUploadText;
        private Button btnReset, btnSubmit;

        // ── Constructor ──────────────────────────────────────────────────────────
        public NewRequisition()
        {
            InitializeComponent();
            BuildUI();
        }

        // ── BuildUI ──────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Outer scroll container
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(scrollPanel);

            // White card
            Panel card = new Panel
            {
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 30)
            };
            scrollPanel.Controls.Add(card);
            card.Location = new Point(30, 25);

            // Reflow on resize
            scrollPanel.Resize += (s, e) =>
            {
                card.Width = scrollPanel.ClientSize.Width - 60;
                card.Location = new Point(30, 25);
                RepositionCardChildren();
            };

            int y = 30;

            // ── Title ─────────────────────────────────────────────────────────────
            lblTitle = new Label
            {
                Text = "🩸  New Requisition",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = false,
                Height = 45,
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblTitle);
            y += 50;

            // Divider line
            divider = new Panel
            {
                Height = 1,
                BackColor = Color.FromArgb(230, 230, 230),
                Location = new Point(0, y)
            };
            card.Controls.Add(divider);
            y += 18;

            // ── Row 1 : Patient Name | CNIC ───────────────────────────────────────
            lblPatient = MakeLabel("Patient Name", new Point(0, y));
            card.Controls.Add(lblPatient);

            txtPatient = MakeTextBox(new Point(0, y + 24), 200, "Enter patient name");
            txtPatient.KeyPress += OnlyLetters_KeyPress;
            card.Controls.Add(txtPatient);

            lblCnic = MakeLabel("CNIC", new Point(220, y));
            card.Controls.Add(lblCnic);

            txtCNIC = MakeTextBox(new Point(220, y + 24), 200, "xxxxx-xxxxxxx-x");
            txtCNIC.MaxLength = 15;
            txtCNIC.KeyPress += CNIC_KeyPress;
            card.Controls.Add(txtCNIC);

            y += 24 + 38 + 18;

            // ── Row 2 : Blood Group | Hospital ────────────────────────────────────
            lblBlood = MakeLabel("Blood Group", new Point(0, y));
            card.Controls.Add(lblBlood);

            cmbBlood = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(200, 36),
                Location = new Point(0, y + 24)
            };
            cmbBlood.Items.AddRange(new string[]
                { "Select Blood Group", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBlood.SelectedIndex = 0;
            card.Controls.Add(cmbBlood);

            lblHospital = MakeLabel("Hospital", new Point(220, y));
            card.Controls.Add(lblHospital);

            txtHospital = MakeTextBox(new Point(220, y + 24), 200, "Enter hospital name");
            txtHospital.KeyPress += OnlyLetters_KeyPress;
            card.Controls.Add(txtHospital);

            y += 24 + 38 + 18;

            // ── Row 3 : Urgency Level ─────────────────────────────────────────────
            lblUrgency = MakeLabel("Urgency Level", new Point(0, y));
            card.Controls.Add(lblUrgency);
            y += 26;

            rbNormal = new RadioButton
            {
                Text = "Normal",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(55, 65, 81),
                Checked = true,
                AutoSize = true,
                Location = new Point(0, y)
            };
            rbUrgent = new RadioButton
            {
                Text = "Urgent",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(55, 65, 81),
                AutoSize = true,
                Location = new Point(110, y)
            };
            rbEmergency = new RadioButton
            {
                Text = "Emergency",
                Font = new Font("Segoe UI", 10),
                ForeColor = brickRed,
                AutoSize = true,
                Location = new Point(220, y)
            };
            card.Controls.Add(rbNormal);
            card.Controls.Add(rbUrgent);
            card.Controls.Add(rbEmergency);
            y += 32 + 18;

            // ── Row 4 : Attach Documents ──────────────────────────────────────────
            lblAttach = MakeLabel("Attach Documents", new Point(0, y));
            card.Controls.Add(lblAttach);
            y += 26;

            uploadPanel = new Panel
            {
                Height = 80,
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand,
                AllowDrop = true,
                Location = new Point(0, y)
            };
            card.Controls.Add(uploadPanel);

            lblUploadText = new Label
            {
                Text = "📎  Drop files here or click to upload",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(107, 114, 128),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            uploadPanel.Controls.Add(lblUploadText);

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|PDF Files (*.pdf)|*.pdf|Images (*.jpg;*.png)|*.jpg;*.png"
            };

            uploadPanel.Click += (s, e) => { if (ofd.ShowDialog() == DialogResult.OK) HandleFile(ofd.FileName); };
            lblUploadText.Click += (s, e) => { if (ofd.ShowDialog() == DialogResult.OK) HandleFile(ofd.FileName); };
            uploadPanel.DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            uploadPanel.DragDrop += (s, e) =>
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0) HandleFile(files[0]);
            };

            y += 80 + 24;

            // ── Row 5 : Buttons ───────────────────────────────────────────────────
            btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(0, y)
            };
            btnReset.FlatAppearance.BorderSize = 1;
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            card.Controls.Add(btnReset);

            btnSubmit = new Button
            {
                Text = "Submit Requisition",
                Size = new Size(165, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Location = new Point(130, y)
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            card.Controls.Add(btnSubmit);

            y += 40 + 30;
            card.Height = y;

            // Initial layout pass (card already has a Width from scrollPanel)
            card.Width = (scrollPanel.ClientSize.Width > 60)
                         ? scrollPanel.ClientSize.Width - 60
                         : 760;
            RepositionCardChildren();

            // ── Event handlers ────────────────────────────────────────────────────
            btnSubmit.Click += BtnSubmit_Click;
            btnReset.Click += (s, e) => ResetForm();
        }

        // ── Reposition all controls based on current card width ───────────────────
        private void RepositionCardChildren()
        {
            // Find the card (parent of lblTitle)
            if (lblTitle == null) return;
            Panel card = lblTitle.Parent as Panel;
            if (card == null) return;

            int w = card.ClientSize.Width - 80;   // usable width inside padding
            int half = (w - 20) / 2;                 // half column width

            lblTitle.Width = w;
            divider.Width = w;

            // Row 1
            lblPatient.Width = half;
            txtPatient.Width = half;
            lblCnic.Left = half + 20; lblCnic.Width = half;
            txtCNIC.Left = half + 20; txtCNIC.Width = half;

            // Row 2
            lblBlood.Width = half;
            cmbBlood.Width = half;
            lblHospital.Left = half + 20; lblHospital.Width = half;
            txtHospital.Left = half + 20; txtHospital.Width = half;

            // Row 3
            lblUrgency.Width = w;

            // Row 4
            lblAttach.Width = w;
            uploadPanel.Width = w;

            // Buttons – right-aligned
            btnSubmit.Left = w - btnSubmit.Width;
            btnReset.Left = w - btnSubmit.Width - btnReset.Width - 12;
        }

        // ── File handler ─────────────────────────────────────────────────────────
        private void HandleFile(string path)
        {
            selectedFilePath = path;
            lblUploadText.Text = "✓  " + Path.GetFileName(path);
            lblUploadText.ForeColor = Color.FromArgb(34, 197, 94);
        }

        // ── Submit ───────────────────────────────────────────────────────────────
        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPatient.Text) || txtPatient.Text == "Enter patient name")
            { MessageBox.Show("Please enter Patient Name", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtPatient.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtCNIC.Text) || txtCNIC.Text == "xxxxx-xxxxxxx-x" || txtCNIC.Text.Length < 15)
            { MessageBox.Show("Please enter valid CNIC", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCNIC.Focus(); return; }

            if (cmbBlood.SelectedIndex == 0)
            { MessageBox.Show("Please select Blood Group", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); cmbBlood.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtHospital.Text) || txtHospital.Text == "Enter hospital name")
            { MessageBox.Show("Please enter Hospital Name", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtHospital.Focus(); return; }

            string urgency = rbNormal.Checked ? "Normal" : rbUrgent.Checked ? "Urgent" : "Emergency";

            bool saved = RequisitionDAL.SaveRequisition(
                txtPatient.Text.Trim(),
                txtCNIC.Text.Trim(),
                cmbBlood.Text,
                txtHospital.Text.Trim(),
                urgency,
                SessionManager.CurrentUserID);

            if (saved)
                MessageBox.Show("Requisition submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Failed to save. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            ResetForm();
        }

        // ── Reset form ───────────────────────────────────────────────────────────
        private void ResetForm()
        {
            SetPlaceholder(txtPatient, "Enter patient name");
            SetPlaceholder(txtCNIC, "xxxxx-xxxxxxx-x");
            SetPlaceholder(txtHospital, "Enter hospital name");
            cmbBlood.SelectedIndex = 0;
            rbNormal.Checked = true;
            selectedFilePath = "";
            lblUploadText.Text = "📎  Drop files here or click to upload";
            lblUploadText.ForeColor = Color.FromArgb(107, 114, 128);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private Label MakeLabel(string text, Point loc) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            AutoSize = false,
            Height = 22,
            Location = loc
        };

        private TextBox MakeTextBox(Point loc, int width, string placeholder)
        {
            TextBox tb = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(width, 36),
                Location = loc
            };
            SetPlaceholder(tb, placeholder);
            return tb;
        }

        private void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.Text = placeholder;
            tb.ForeColor = Color.Gray;
            tb.GotFocus += (s, e) => { if (tb.Text == placeholder) { tb.Text = ""; tb.ForeColor = Color.Black; } };
            tb.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = placeholder; tb.ForeColor = Color.Gray; } };
        }

        private void OnlyLetters_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
                e.Handled = true;
        }

        private void CNIC_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (char.IsControl(e.KeyChar)) return;
            if (!char.IsDigit(e.KeyChar)) { e.Handled = true; return; }
            if (txt.Text.Length == 5 || txt.Text.Length == 13)
            { txt.Text += "-"; txt.SelectionStart = txt.Text.Length; }
            if (txt.Text.Length >= 15) e.Handled = true;
        }
    }
}