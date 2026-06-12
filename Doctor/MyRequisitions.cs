using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class MyRequisitions : Form
    {
        private DataGridView dgvRequisitions;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private Panel pnlDetails;
        private int selectedReqId = 0;
        private DataTable allRequisitions;

        public MyRequisitions()
        {
            InitializeComponent();
            BuildUI();
            LoadRequisitionsFromDatabase();
        }

        private void BuildUI()
        {
            this.Text = "My Requisitions - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = "📋 My Blood Requisitions",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            // Stats Cards Panel
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120 };
            mainPanel.Controls.Add(statsPanel);

            // Filter Panel
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(15), Margin = new Padding(0, 10, 0, 10) };
            mainPanel.Controls.Add(filterPanel);

            Label lblSearch = new Label { Text = "🔍 Search:", Location = new Point(10, 20), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(70, 17), Width = 250, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => FilterRequisitions();

            Label lblStatus = new Label { Text = "Status:", Location = new Point(350, 20), AutoSize = true };
            cmbStatus = new ComboBox { Location = new Point(410, 15), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cmbStatus.Items.AddRange(new string[] { "All", "Pending", "Approved", "Rejected", "Completed", "Cross Matching" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.SelectedIndexChanged += (s, e) => FilterRequisitions();

            Button btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(560, 13),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadRequisitionsFromDatabase();

            filterPanel.Controls.Add(lblSearch);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(lblStatus);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(btnRefresh);

            // Split Container
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 380
            };
            mainPanel.Controls.Add(splitContainer);

            // Requisitions List
            dgvRequisitions = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvRequisitions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgvRequisitions.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequisitions.EnableHeadersVisualStyles = false;
            dgvRequisitions.CellClick += DgvRequisitions_CellClick;
            splitContainer.Panel1.Controls.Add(dgvRequisitions);

            // Details Panel
            pnlDetails = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), AutoScroll = true };
            splitContainer.Panel2.Controls.Add(pnlDetails);

            SetupDetailsPanel();
            CreateStatsCards(statsPanel);
        }

        private void CreateStatsCards(Panel parent)
        {
            // Will be populated after data load
        }

        private void UpdateStatsCards(Panel parent)
        {
            if (parent == null || allRequisitions == null) return;
            parent.Controls.Clear();

            int total = allRequisitions.Rows.Count;
            int pending = 0, approved = 0, crossMatch = 0;

            foreach (DataRow row in allRequisitions.Rows)
            {
                string status = row["Status"]?.ToString() ?? "";
                if (status == "Pending") pending++;
                else if (status == "Approved") approved++;
                else if (status == "Cross Matching") crossMatch++;
            }

            string[] titles = { "Total Requisitions", "Pending", "Approved", "Cross Matching" };
            int[] values = { total, pending, approved, crossMatch };
            Color[] colors = { Color.FromArgb(120, 22, 27), Color.FromArgb(245, 158, 11), Color.FromArgb(34, 197, 94), Color.FromArgb(59, 130, 246) };

            int cardWidth = (parent.Width - 60) / 4;
            for (int i = 0; i < 4; i++)
            {
                Panel card = new Panel
                {
                    Size = new Size(cardWidth, 100),
                    Location = new Point(i * (cardWidth + 15), 10),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                parent.Controls.Add(card);

                Label lblValue = new Label
                {
                    Text = values[i].ToString(),
                    Font = new Font("Segoe UI", 28, FontStyle.Bold),
                    ForeColor = colors[i],
                    Dock = DockStyle.Top,
                    Height = 55,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblValue);

                Label lblTitle = new Label
                {
                    Text = titles[i],
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblTitle);
            }
        }

        private void SetupDetailsPanel()
        {
            pnlDetails.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Requisition Details",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlDetails.Controls.Add(lblTitle);

            int y = 55, leftX = 20, labelWidth = 140;

            var details = new (string label, string name)[]
            {
                ("Requisition No:", "lblReqNo"),
                ("Patient Name:", "lblPatientName"),
                ("CNIC:", "lblCNIC"),
                ("Blood Group:", "lblBloodGroup"),
                ("Units Needed:", "lblUnits"),
                ("Hospital:", "lblHospital"),
                ("Urgency:", "lblUrgency"),
                ("Status:", "lblStatus"),
                ("Request Date:", "lblDate")
            };

            foreach (var item in details)
            {
                pnlDetails.Controls.Add(new Label
                {
                    Text = item.label,
                    Location = new Point(leftX, y),
                    Size = new Size(labelWidth, 25),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 90)
                });

                Label lblValue = new Label
                {
                    Name = item.name,
                    Text = "-",
                    Location = new Point(leftX + labelWidth, y),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(55, 65, 81)
                };
                pnlDetails.Controls.Add(lblValue);
                y += 30;
            }

            y += 10;
            pnlDetails.Controls.Add(new Label
            {
                Text = "Remarks:",
                Location = new Point(leftX, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            });

            TextBox txtRemarks = new TextBox
            {
                Name = "txtRemarks",
                Location = new Point(leftX + labelWidth, y),
                Width = 400,
                Height = 60,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 252),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlDetails.Controls.Add(txtRemarks);

            // Action Buttons Panel
            y += 80;
            Panel actionPanel = new Panel
            {
                Location = new Point(leftX + labelWidth, y),
                Height = 45,
                Width = 300
            };
            pnlDetails.Controls.Add(actionPanel);

            Button btnUpdateStatus = new Button
            {
                Text = "📝 Update Status",
                Size = new Size(130, 38),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            btnUpdateStatus.Click += BtnUpdateStatus_Click;
            actionPanel.Controls.Add(btnUpdateStatus);

            Button btnViewFull = new Button
            {
                Text = "🔍 View Full Details",
                Size = new Size(140, 38),
                Location = new Point(145, 0),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewFull.FlatAppearance.BorderSize = 0;
            btnViewFull.Click += (s, e) =>
            {
                MessageBox.Show($"Full details for requisition #{selectedReqId}\n\nOpen Requisition Details Form.",
                    "View Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            actionPanel.Controls.Add(btnViewFull);
        }

        private async void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (selectedReqId == 0)
            {
                MessageBox.Show("Please select a requisition first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get current status
            string currentStatus = "";
            foreach (Control ctrl in pnlDetails.Controls)
            {
                if (ctrl.Name == "lblStatus")
                {
                    currentStatus = ctrl.Text;
                    break;
                }
            }

            using (var actionForm = new RequisitionActions(selectedReqId,
                GetControlValue("lblReqNo"), currentStatus))
            {
                if (actionForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh the data
                    await System.Threading.Tasks.Task.Delay(500);
                    LoadRequisitionsFromDatabase();
                }
            }
        }

        private string GetControlValue(string controlName)
        {
            foreach (Control ctrl in pnlDetails.Controls)
            {
                if (ctrl.Name == controlName && ctrl is Label lbl)
                    return lbl.Text;
            }
            return "";
        }

        private void LoadRequisitionsFromDatabase()
        {
            try
            {
                int doctorId = SessionManager.CurrentUserID;

                // Get requisitions from database
                DataTable dt = RequisitionDAL.GetRequisitionsByDoctor(doctorId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    // Create empty table with proper columns
                    allRequisitions = new DataTable();
                    allRequisitions.Columns.Add("RequisitionID", typeof(int));
                    allRequisitions.Columns.Add("RequisitionNumber", typeof(string));
                    allRequisitions.Columns.Add("PatientName", typeof(string));
                    allRequisitions.Columns.Add("BloodGroup", typeof(string));
                    allRequisitions.Columns.Add("UnitsNeeded", typeof(int));
                    allRequisitions.Columns.Add("RequestDate", typeof(DateTime));
                    allRequisitions.Columns.Add("Status", typeof(string));
                    allRequisitions.Columns.Add("Urgency", typeof(string));
                    allRequisitions.Columns.Add("Hospital", typeof(string));
                    allRequisitions.Columns.Add("CNIC", typeof(string));
                    allRequisitions.Columns.Add("Remarks", typeof(string));
                }
                else
                {
                    allRequisitions = dt;
                }

                // Setup columns for display
                dgvRequisitions.Columns.Clear();
                dgvRequisitions.Columns.Add("ReqID", "ID");
                dgvRequisitions.Columns.Add("ReqNo", "Requisition No");
                dgvRequisitions.Columns.Add("Patient", "Patient Name");
                dgvRequisitions.Columns.Add("BloodGroup", "Blood Group");
                dgvRequisitions.Columns.Add("Units", "Units");
                dgvRequisitions.Columns.Add("Date", "Date");
                dgvRequisitions.Columns.Add("Status", "Status");
                dgvRequisitions.Columns.Add("Urgency", "Urgency");

                // Hide ID column
                dgvRequisitions.Columns["ReqID"].Visible = false;

                // Load data
                dgvRequisitions.Rows.Clear();
                foreach (DataRow row in allRequisitions.Rows)
                {
                    dgvRequisitions.Rows.Add(
                        row["RequisitionID"],
                        row["RequisitionNumber"],
                        row["PatientName"],
                        row["BloodGroup"],
                        row["UnitsNeeded"],
                        Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy"),
                        row["Status"],
                        row["Urgency"]
                    );
                }

                // Update stats cards
                Panel statsPanel = null;
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel p && p.Dock == DockStyle.Top && p.Height == 120)
                    {
                        statsPanel = p;
                        break;
                    }
                }
                if (statsPanel != null)
                    UpdateStatsCards(statsPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requisitions: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterRequisitions()
        {
            if (allRequisitions == null) return;

            string searchText = txtSearch.Text.ToLower();
            string statusFilter = cmbStatus.SelectedItem?.ToString();

            dgvRequisitions.Rows.Clear();

            foreach (DataRow row in allRequisitions.Rows)
            {
                bool matchSearch = string.IsNullOrEmpty(searchText) || searchText == "search..." ||
                    row["PatientName"].ToString().ToLower().Contains(searchText) ||
                    row["RequisitionNumber"].ToString().ToLower().Contains(searchText);

                bool matchStatus = statusFilter == "All" || row["Status"].ToString() == statusFilter;

                if (matchSearch && matchStatus)
                {
                    dgvRequisitions.Rows.Add(
                        row["RequisitionID"],
                        row["RequisitionNumber"],
                        row["PatientName"],
                        row["BloodGroup"],
                        row["UnitsNeeded"],
                        Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy"),
                        row["Status"],
                        row["Urgency"]
                    );
                }
            }
        }

        private async void DgvRequisitions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedReqId = Convert.ToInt32(dgvRequisitions.Rows[e.RowIndex].Cells["ReqID"].Value);
                string reqNumber = dgvRequisitions.Rows[e.RowIndex].Cells["ReqNo"].Value.ToString();

                // Load full details from database
                DataRow requisition = RequisitionDAL.GetRequisitionByID(selectedReqId);

                if (requisition != null)
                {
                    // Update details panel
                    UpdateLabel("lblReqNo", requisition["RequisitionNumber"]?.ToString() ?? "-");
                    UpdateLabel("lblPatientName", requisition["PatientName"]?.ToString() ?? "-");
                    UpdateLabel("lblCNIC", requisition["CNIC"]?.ToString() ?? "-");
                    UpdateLabel("lblBloodGroup", requisition["BloodGroup"]?.ToString() ?? "-");
                    UpdateLabel("lblUnits", requisition["UnitsNeeded"]?.ToString() ?? "-");
                    UpdateLabel("lblHospital", requisition["Hospital"]?.ToString() ?? "-");
                    UpdateLabel("lblUrgency", requisition["Urgency"]?.ToString() ?? "-");
                    UpdateLabel("lblStatus", requisition["Status"]?.ToString() ?? "-");

                    DateTime reqDate = requisition["RequestDate"] != DBNull.Value ?
                        Convert.ToDateTime(requisition["RequestDate"]) : DateTime.Now;
                    UpdateLabel("lblDate", reqDate.ToString("dd-MMM-yyyy hh:mm tt"));

                    UpdateTextBox("txtRemarks", requisition["Remarks"]?.ToString() ?? "No remarks");

                    // Color code status label
                    UpdateStatusColor(requisition["Status"]?.ToString() ?? "Pending");
                }
            }
        }

        private void UpdateLabel(string labelName, string value)
        {
            foreach (Control ctrl in pnlDetails.Controls)
            {
                if (ctrl.Name == labelName && ctrl is Label lbl)
                {
                    lbl.Text = value;
                    break;
                }
            }
        }

        private void UpdateTextBox(string textBoxName, string value)
        {
            foreach (Control ctrl in pnlDetails.Controls)
            {
                if (ctrl.Name == textBoxName && ctrl is TextBox txt)
                {
                    txt.Text = value;
                    break;
                }
            }
        }

        private void UpdateStatusColor(string status)
        {
            foreach (Control ctrl in pnlDetails.Controls)
            {
                if (ctrl.Name == "lblStatus" && ctrl is Label lbl)
                {
                    if (status == "Approved")
                        lbl.ForeColor = Color.FromArgb(34, 197, 94);
                    else if (status == "Pending")
                        lbl.ForeColor = Color.FromArgb(245, 158, 11);
                    else if (status == "Rejected")
                        lbl.ForeColor = Color.FromArgb(220, 38, 38);
                    else if (status == "Cross Matching")
                        lbl.ForeColor = Color.FromArgb(59, 130, 246);
                    else
                        lbl.ForeColor = Color.FromArgb(55, 65, 81);
                    break;
                }
            }
        }
    }
}