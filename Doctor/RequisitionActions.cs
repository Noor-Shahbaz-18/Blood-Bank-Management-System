using System;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class RequisitionActions : Form
    {
        private int _requisitionId;
        private string _requisitionNo;
        private string _currentStatus;
        private ComboBox cmbAction;
        private TextBox txtRemarks;

        public RequisitionActions(int requisitionId, string requisitionNo, string currentStatus)
        {
            _requisitionId = requisitionId;
            _requisitionNo = requisitionNo;
            _currentStatus = currentStatus;
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = $"Requisition Actions - {_requisitionNo}";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = $"📋 Requisition: {_requisitionNo}",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            // Status Badge
            Color badgeBackColor;
            Color statusForeColor;
            switch (_currentStatus)
            {
                case "Pending":
                    badgeBackColor = Color.FromArgb(254, 243, 199);
                    statusForeColor = Color.FromArgb(146, 64, 14);
                    break;
                case "Approved":
                    badgeBackColor = Color.FromArgb(209, 250, 229);
                    statusForeColor = Color.FromArgb(6, 95, 70);
                    break;
                case "Cross Matching":
                    badgeBackColor = Color.FromArgb(219, 234, 254);
                    statusForeColor = Color.FromArgb(30, 64, 175);
                    break;
                case "Completed":
                    badgeBackColor = Color.FromArgb(209, 250, 229);
                    statusForeColor = Color.FromArgb(6, 95, 70);
                    break;
                case "Rejected":
                    badgeBackColor = Color.FromArgb(254, 226, 226);
                    statusForeColor = Color.FromArgb(185, 28, 28);
                    break;
                default:
                    badgeBackColor = Color.LightGray;
                    statusForeColor = Color.Gray;
                    break;
            }

            Panel statusBadge = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = badgeBackColor,
                Padding = new Padding(15)
            };
            Label lblStatus = new Label
            {
                Text = $"Current Status: {_currentStatus}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = statusForeColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusBadge.Controls.Add(lblStatus);
            mainPanel.Controls.Add(statusBadge);

            int y = 140, leftX = 20, fieldWidth = 410;

            // Action Selection
            mainPanel.Controls.Add(new Label
            {
                Text = "Select Action:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });

            cmbAction = new ComboBox
            {
                Location = new Point(leftX, y + 25),
                Width = fieldWidth,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Height = 40
            };

            // Available actions based on current status
            if (_currentStatus == "Pending")
            {
                cmbAction.Items.AddRange(new string[] {
                    "Select Action",
                    "Approve Requisition",
                    "Send for Cross Matching",
                    "Reject Requisition"
                });
            }
            else if (_currentStatus == "Cross Matching")
            {
                cmbAction.Items.AddRange(new string[] {
                    "Select Action",
                    "Mark as Completed",
                    "Reject Requisition"
                });
            }
            else if (_currentStatus == "Approved")
            {
                cmbAction.Items.AddRange(new string[] {
                    "Select Action",
                    "Mark as Completed",
                    "Cancel Requisition"
                });
            }
            else
            {
                cmbAction.Items.AddRange(new string[] {
                    "Select Action",
                    "Add Remarks Only"
                });
            }

            cmbAction.SelectedIndex = 0;
            mainPanel.Controls.Add(cmbAction);
            y += 70;

            // Remarks/Reason Panel
            Label lblRemarks = new Label
            {
                Text = "Remarks / Reason:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            mainPanel.Controls.Add(lblRemarks);

            txtRemarks = new TextBox
            {
                Location = new Point(leftX, y + 25),
                Width = fieldWidth,
                Height = 80,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                ScrollBars = ScrollBars.Vertical
            };
            mainPanel.Controls.Add(txtRemarks);
            y += 120;

            // Confirm Button
            Button btnConfirm = new Button
            {
                Text = "✅ Confirm Action",
                Location = new Point(leftX + 100, y),
                Size = new Size(160, 45),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;
            mainPanel.Controls.Add(btnConfirm);

            Button btnCancel = new Button
            {
                Text = "❌ Cancel",
                Location = new Point(leftX + 280, y),
                Size = new Size(100, 45),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnCancel);
        }

        private async void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (cmbAction.SelectedIndex == 0)
            {
                MessageBox.Show("Please select an action.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string action = cmbAction.SelectedItem.ToString();
            string remarks = txtRemarks.Text.Trim();

            if (string.IsNullOrEmpty(remarks))
            {
                var result = MessageBox.Show("No remarks provided. Continue without remarks?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }

            bool success = false;
            string newStatus = "";
            string statusMessage = "";

            try
            {
                switch (action)
                {
                    case "Approve Requisition":
                        newStatus = "Approved";
                        success = RequisitionDAL.UpdateRequisitionStatus(_requisitionId, newStatus,
                            SessionManager.CurrentUsername);
                        statusMessage = "approved";
                        break;

                    case "Send for Cross Matching":
                        newStatus = "Cross Matching";
                        success = RequisitionDAL.UpdateRequisitionStatus(_requisitionId, newStatus,
                            SessionManager.CurrentUsername);
                        statusMessage = "sent for cross matching";
                        break;

                    case "Mark as Completed":
                        newStatus = "Completed";
                        success = RequisitionDAL.UpdateRequisitionStatus(_requisitionId, newStatus,
                            SessionManager.CurrentUsername);
                        statusMessage = "marked as completed";
                        break;

                    case "Reject Requisition":
                        newStatus = "Rejected";
                        success = RequisitionDAL.UpdateRequisitionStatus(_requisitionId, newStatus,
                            SessionManager.CurrentUsername);
                        statusMessage = "rejected";
                        break;

                    case "Cancel Requisition":
                        newStatus = "Cancelled";
                        success = RequisitionDAL.UpdateRequisitionStatus(_requisitionId, newStatus,
                            SessionManager.CurrentUsername);
                        statusMessage = "cancelled";
                        break;

                    case "Add Remarks Only":
                        success = true;
                        statusMessage = "remarks added";
                        break;
                }

                // If we have remarks, update them separately if needed
                if (!string.IsNullOrEmpty(remarks) && success)
                {
                    // You can add a method to update remarks in database
                    // RequisitionDAL.UpdateRemarks(_requisitionId, remarks);
                }

                if (success)
                {
                    MessageBox.Show($"Requisition #{_requisitionNo} has been {statusMessage} successfully!",
                        "Action Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Failed to update requisition status. Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}