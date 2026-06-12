using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AddEditRequisition : Form
    {
        private int _requisitionId = 0;
        private bool _isEditMode = false;

        private TextBox txtPatientName, txtCNIC, txtHospital, txtRemarks;
        private ComboBox cmbBloodGroup, cmbUrgency, cmbStatus;
        private NumericUpDown numUnits;
        private Button btnSave, btnCancel;

        public AddEditRequisition(int requisitionId = 0)
        {
            _requisitionId = requisitionId;
            _isEditMode = requisitionId > 0;
            BuildUI();
            if (_isEditMode) LoadRequisitionData();
        }

        private void BuildUI()
        {
            this.Text = _isEditMode ? "✏️ Edit Requisition" : "📋 New Blood Requisition";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel main = new Panel { Location = new Point(0, 0), BackColor = Color.White };
            this.Controls.Add(main);

            int leftX = 30;
            int labelW = 130;
            int fieldX = leftX + labelW + 10;
            int fieldW = 280;
            int y = 20;

            Label lblTitle = new Label
            {
                Text = _isEditMode ? "✏️ Edit Blood Requisition" : "📋 New Blood Requisition",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y),
                Width = 600,
                Height = 42,
                TextAlign = ContentAlignment.MiddleLeft
            };
            main.Controls.Add(lblTitle);
            y += 55;

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

            // Patient Name
            AddLabel("Patient Name:", y);
            txtPatientName = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtPatientName);
            y += 50;

            // CNIC
            AddLabel("CNIC:", y);
            txtCNIC = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtCNIC);
            y += 50;

            // Blood Group
            AddLabel("Blood Group:", y);
            cmbBloodGroup = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbBloodGroup.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.SelectedIndex = 0;
            main.Controls.Add(cmbBloodGroup);
            y += 50;

            // Units Needed
            AddLabel("Units Needed:", y);
            numUnits = new NumericUpDown
            {
                Location = new Point(fieldX, y),
                Size = new Size(80, 32),
                Minimum = 1,
                Maximum = 20,
                Value = 1,
                Font = new Font("Segoe UI", 11)
            };
            main.Controls.Add(numUnits);
            y += 50;

            // Hospital
            AddLabel("Hospital:", y);
            txtHospital = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 32),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtHospital);
            y += 50;

            // Urgency
            AddLabel("Urgency:", y);
            cmbUrgency = new ComboBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(120, 32),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            cmbUrgency.Items.AddRange(new[] { "Normal", "Urgent", "Emergency" });
            cmbUrgency.SelectedIndex = 0;
            main.Controls.Add(cmbUrgency);
            y += 50;

            // Status (only for edit mode)
            if (_isEditMode)
            {
                AddLabel("Status:", y);
                cmbStatus = new ComboBox
                {
                    Location = new Point(fieldX, y),
                    Size = new Size(120, 32),
                    Font = new Font("Segoe UI", 11),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(250, 250, 252)
                };
                cmbStatus.Items.AddRange(new[] { "Pending", "Approved", "Rejected", "Completed", "Cancelled" });
                cmbStatus.SelectedIndex = 0;
                main.Controls.Add(cmbStatus);
                y += 50;
            }

            // Remarks
            AddLabel("Remarks:", y);
            txtRemarks = new TextBox
            {
                Location = new Point(fieldX, y),
                Size = new Size(fieldW, 60),
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            main.Controls.Add(txtRemarks);
            y += 75;

            y += 18;

            int totalW = 250;
            int btnX = (this.ClientSize.Width / 2) - (totalW / 2);

            btnSave = new Button
            {
                Text = "💾 Save Requisition",
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
                Text = "❌ Cancel",
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

            int formH = y + 42 + 30;
            int formW = leftX + labelW + 10 + fieldW + leftX;
            this.ClientSize = new Size(Math.Max(600, formW), Math.Min(650, formH));
            main.Size = new Size(this.ClientSize.Width, formH);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPatientName.Text))
            {
                MessageBox.Show("Please enter patient name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHospital.Text))
            {
                MessageBox.Show("Please enter hospital name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var requisition = new Requisition
                {
                    PatientName = txtPatientName.Text.Trim(),
                    CNIC = txtCNIC.Text.Trim(),
                    BloodGroup = cmbBloodGroup.SelectedItem?.ToString(),
                    UnitsNeeded = (int)numUnits.Value,
                    Hospital = txtHospital.Text.Trim(),
                    Urgency = cmbUrgency.SelectedItem?.ToString(),
                    Remarks = txtRemarks.Text.Trim()
                };

                bool success;
                if (_isEditMode)
                {
                    requisition.RequisitionID = _requisitionId;
                    requisition.Status = cmbStatus.SelectedItem?.ToString();
                    success = RequisitionDAL.Update(requisition);
                }
                else
                {
                    requisition.Status = "Pending";
                    requisition.RequestDate = DateTime.Now;
                    requisition.PatientUserID = 0;
                    requisition.DoctorID = 0;
                    success = RequisitionDAL.Insert(requisition);
                }

                if (success)
                {
                    MessageBox.Show($"Requisition {(_isEditMode ? "updated" : "created")} successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save requisition.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRequisitionData()
        {
            try
            {
                var req = RequisitionDAL.GetByID(_requisitionId);
                if (req == null) return;

                txtPatientName.Text = req.PatientName;
                txtCNIC.Text = req.CNIC;
                cmbBloodGroup.Text = req.BloodGroup;
                numUnits.Value = req.UnitsNeeded;
                txtHospital.Text = req.Hospital;
                cmbUrgency.Text = req.Urgency;
                txtRemarks.Text = req.Remarks;
                if (cmbStatus != null) cmbStatus.Text = req.Status;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}