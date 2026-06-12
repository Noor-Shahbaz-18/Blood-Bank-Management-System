using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class AddEditBloodComponent : Form
    {
        private int _componentId = 0;
        private bool _isEditMode = false;

        private ComboBox cmbComponent, cmbBloodGroup;
        private NumericUpDown numQuantity;
        private DateTimePicker dtpExpiry;
        private TextBox txtLocation;
        private Button btnSave, btnCancel;

        public AddEditBloodComponent(int componentId = 0)
        {
            InitializeComponent();
            _componentId = componentId;
            _isEditMode = componentId > 0;
            BuildUI();
            if (_isEditMode) LoadComponentData();
        }

        private void BuildUI()
        {
            this.Text = _isEditMode ? "Edit Blood Component" : "Add Blood Component";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ── Ek panel, sab absolute coordinates ──────────────────
            Panel main = new Panel
            {
                Location = new Point(0, 0),
                BackColor = Color.White,
                AutoScroll = false
            };
            this.Controls.Add(main);

            int leftX = 30;
            int labelW = 145;
            int fieldX = leftX + labelW + 10;
            int fieldW = 340;
            int y = 20;

            // ── Title ────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = _isEditMode ? "✏️  Edit Blood Component" : "🩸  Add New Blood Component",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                Width = labelW + 10 + fieldW,
                Height = 42,
                TextAlign = ContentAlignment.MiddleCenter
            };
            main.Controls.Add(lblTitle);
            y += 55;

            // ── Helper: label + control row ──────────────────────────
            void AddLabel(string text, int row_y)
            {
                main.Controls.Add(new Label
                {
                    Text = text,
                    Location = new Point(leftX, row_y + 5),
                    Size = new Size(labelW, 24),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(60, 60, 70)
                });
            }

            // ── Component Name ───────────────────────────────────────
            AddLabel("Component Name:", y);
            cmbComponent = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbComponent.Items.AddRange(new string[] {
                "Whole Blood", "Red Blood Cells", "Platelets",
                "Plasma", "Cryoprecipitate", "White Blood Cells",
                "Fresh Frozen Plasma"
            });
            cmbComponent.SelectedIndex = 0;
            main.Controls.Add(cmbComponent);
            y += 55;

            // ── Blood Group ──────────────────────────────────────────
            AddLabel("Blood Group:", y);
            cmbBloodGroup = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbBloodGroup.Items.AddRange(new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            main.Controls.Add(cmbBloodGroup);
            y += 55;

            // ── Quantity ─────────────────────────────────────────────
            AddLabel("Quantity (Units):", y);
            numQuantity = new NumericUpDown
            {
                Location = new Point(fieldX, y),
                Size = new Size(130, 32),
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Font = new Font("Segoe UI", 11)
            };
            main.Controls.Add(numQuantity);
            y += 55;

            // ── Expiry Date ──────────────────────────────────────────
            AddLabel("Expiry Date:", y);
            dtpExpiry = new DateTimePicker
            {
                Location = new Point(fieldX, y),
                Size = new Size(220, 32),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 11),
                Value = DateTime.Now.AddDays(35)
            };
            main.Controls.Add(dtpExpiry);
            y += 55;

            // ── Storage Location ─────────────────────────────────────
            AddLabel("Storage Location:", y);
            txtLocation = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtLocation);
            y += 55;

            y += 18; // breathing room

            // ── Buttons — bilkul neeche ──────────────────────────────
            int totalW = 250;
            int btnX = (leftX + labelW + 10 + fieldW / 2) - totalW / 2;

            btnSave = new Button
            {
                Text = "💾  Save",
                Location = new Point(btnX, y),
                Size = new Size(115, 42),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            main.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "❌  Cancel",
                Location = new Point(btnX + 130, y),
                Size = new Size(115, 42),
                BackColor = Color.FromArgb(220, 220, 225),
                ForeColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();
            main.Controls.Add(btnCancel);

            // ── Form size — content ke mutabiq ───────────────────────
            int formH = y + 42 + 30;
            int formW = leftX + labelW + 10 + fieldW + leftX;
            this.ClientSize = new Size(formW, formH);
            main.Size = new Size(formW, formH);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbComponent.SelectedIndex < 0)
            {
                MessageBox.Show("Please select component name.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbComponent.Focus(); return;
            }
            if (cmbBloodGroup.SelectedIndex < 0)
            {
                MessageBox.Show("Please select blood group.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBloodGroup.Focus(); return;
            }

            try
            {
                string status = numQuantity.Value < 5 ? "Low Stock" : "Available";
                bool success;

                if (_isEditMode)
                    success = BloodComponentDAL.UpdateComponent(
                        _componentId, cmbComponent.Text, cmbBloodGroup.Text,
                        (int)numQuantity.Value, dtpExpiry.Value,
                        txtLocation.Text.Trim(), status);
                else
                    success = BloodComponentDAL.InsertComponent(
                        cmbComponent.Text, cmbBloodGroup.Text,
                        (int)numQuantity.Value, dtpExpiry.Value,
                        txtLocation.Text.Trim(), status);

                if (success)
                {
                    MessageBox.Show(
                        $"Blood component {(_isEditMode ? "updated" : "added")} successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save component. Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadComponentData()
        {
            try
            {
                var c = BloodComponentDAL.GetComponentByID(_componentId);
                if (c == null) return;
                cmbComponent.Text = c.ComponentName ?? "";
                cmbBloodGroup.Text = c.BloodGroup ?? "";
                numQuantity.Value = Math.Max(1, Math.Min(9999, c.Quantity));
                dtpExpiry.Value = c.ExpiryDate;
                txtLocation.Text = c.StorageLocation ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}