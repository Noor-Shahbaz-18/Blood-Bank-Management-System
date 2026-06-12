using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class PatientRequestStatus : Form
    {
        private DataGridView dgvRequests;
        private Panel pnlDetails;
        private Label lblStatus;
        private ComboBox cmbStatusFilter;
        private TextBox txtSearch;
        private Button btnRefresh;
        private int currentUserId;
        private DataTable userRequests;

        public PatientRequestStatus()
        {
            InitializeComponent();
            currentUserId = SessionManager.CurrentUserID;
            BuildUI();
            LoadRequestsFromDatabase();
        }

        private void BuildUI()
        {
            this.Text = "My Blood Requests - Patient";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(mainPanel);

            // Title
            Label lblTitle = new Label
            {
                Text = "📋 My Blood Requests",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            // Stats Cards Panel
            Panel statsPanel = new Panel { Dock = DockStyle.Top, Height = 120, Margin = new Padding(0, 0, 0, 15) };
            mainPanel.Controls.Add(statsPanel);

            // Stats cards will be populated dynamically
            UpdateStatsCards(statsPanel);

            // Filter Panel
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 15)
            };
            MakeRounded(filterPanel, 10);
            mainPanel.Controls.Add(filterPanel);

            Label lblSearch = new Label { Text = "🔍 Search:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtSearch = new TextBox
            {
                Location = new Point(80, 17),
                Width = 200,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => FilterRequests();

            Label lblStatusFilter = new Label { Text = "Status:", Location = new Point(310, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbStatusFilter = new ComboBox
            {
                Location = new Point(360, 15),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbStatusFilter.Items.AddRange(new string[] { "All", "Pending", "Approved", "Rejected", "Completed", "Cross Matching" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => FilterRequests();

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(510, 13),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            MakeRounded(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadRequestsFromDatabase();

            filterPanel.Controls.Add(lblSearch);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(lblStatusFilter);
            filterPanel.Controls.Add(cmbStatusFilter);
            filterPanel.Controls.Add(btnRefresh);

            // Main Content Split
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 380,
                SplitterWidth = 5
            };
            mainPanel.Controls.Add(splitContainer);

            // Top: Requests List
            Panel listPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            MakeRounded(listPanel, 12);
            splitContainer.Panel1.Controls.Add(listPanel);

            Label lblListTitle = new Label
            {
                Text = "My Blood Requests",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            listPanel.Controls.Add(lblListTitle);

            // DataGridView for requests
            dgvRequests = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                RowTemplate = { Height = 45 },
                ColumnHeadersHeight = 40
            };
            dgvRequests.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgvRequests.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRequests.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvRequests.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvRequests.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 226, 226);
            dgvRequests.DefaultCellStyle.SelectionForeColor = Color.FromArgb(120, 22, 27);
            dgvRequests.CellClick += DgvRequests_CellClick;
            dgvRequests.CellFormatting += DgvRequests_CellFormatting;
            listPanel.Controls.Add(dgvRequests);

            // Add columns
            dgvRequests.Columns.Add("ReqNo", "Request No");
            dgvRequests.Columns.Add("BloodGroup", "Blood Group");
            dgvRequests.Columns.Add("Units", "Units");
            dgvRequests.Columns.Add("Hospital", "Hospital");
            dgvRequests.Columns.Add("Urgency", "Urgency");
            dgvRequests.Columns.Add("Status", "Status");
            dgvRequests.Columns.Add("Date", "Request Date");
            dgvRequests.Columns.Add("ReqID", "ID");
            dgvRequests.Columns["ReqID"].Visible = false;

            // Bottom: Details Panel
            pnlDetails = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), AutoScroll = true };
            MakeRounded(pnlDetails, 12);
            splitContainer.Panel2.Controls.Add(pnlDetails);

            SetupDetailsPanel();
        }

        private void UpdateStatsCards(Panel parent)
        {
            parent.Controls.Clear();

            int total = 0, pending = 0, approved = 0, completed = 0, rejected = 0;

            if (userRequests != null)
            {
                total = userRequests.Rows.Count;
                foreach (DataRow row in userRequests.Rows)
                {
                    string status = row["Status"]?.ToString() ?? "";
                    if (status == "Pending") pending++;
                    else if (status == "Approved") approved++;
                    else if (status == "Completed") completed++;
                    else if (status == "Rejected") rejected++;
                }
            }

            var stats = new (string title, string value, Color color)[]
            {
                ("Total Requests", total.ToString(), Color.FromArgb(120, 22, 27)),
                ("Pending", pending.ToString(), Color.FromArgb(245, 158, 11)),
                ("Approved", approved.ToString(), Color.FromArgb(34, 197, 94)),
                ("Completed", completed.ToString(), Color.FromArgb(59, 130, 246))
            };

            int cardWidth = (parent.Width - 60) / 4;
            for (int i = 0; i < stats.Length; i++)
            {
                Panel card = new Panel
                {
                    Size = new Size(cardWidth, 95),
                    Location = new Point(i * (cardWidth + 15), 10),
                    BackColor = Color.White
                };
                MakeRounded(card, 10);
                parent.Controls.Add(card);

                Label lblValue = new Label
                {
                    Text = stats[i].value,
                    Font = new Font("Segoe UI", 28, FontStyle.Bold),
                    ForeColor = stats[i].color,
                    Dock = DockStyle.Top,
                    Height = 55,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblValue);

                Label lblTitle = new Label
                {
                    Text = stats[i].title,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                card.Controls.Add(lblTitle);
            }

            parent.Resize += (s, e) =>
            {
                int newWidth = (parent.Width - 60) / 4;
                for (int i = 0; i < parent.Controls.Count; i++)
                {
                    parent.Controls[i].Width = newWidth;
                    parent.Controls[i].Location = new Point(i * (newWidth + 15), 10);
                }
            };
        }

        private void SetupDetailsPanel()
        {
            pnlDetails.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "Request Details",
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
                ("Request Number:", "lblReqNo"),
                ("Patient Name:", "lblPatientName"),
                ("Blood Group:", "lblBloodGroup"),
                ("Units Required:", "lblUnits"),
                ("Hospital:", "lblHospital"),
                ("Urgency:", "lblUrgency"),
                ("Status:", "lblStatus"),
                ("Request Date:", "lblDate"),
                ("Approval Date:", "lblApprovalDate"),
                ("Remarks:", "lblRemarks")
            };

            foreach (var item in details)
            {
                Label lblLabel = new Label
                {
                    Text = item.label,
                    Location = new Point(leftX, y),
                    Size = new Size(labelWidth, 25),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 90)
                };
                pnlDetails.Controls.Add(lblLabel);

                if (item.name == "lblRemarks")
                {
                    TextBox txtValue = new TextBox
                    {
                        Name = item.name,
                        Location = new Point(leftX + labelWidth, y),
                        Width = 450,
                        Height = 60,
                        Multiline = true,
                        ReadOnly = true,
                        BackColor = Color.FromArgb(250, 250, 252),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    pnlDetails.Controls.Add(txtValue);
                    y += 75;
                }
                else
                {
                    Label lblValue = new Label
                    {
                        Name = item.name,
                        Text = "-",
                        Location = new Point(leftX + labelWidth, y + 3),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(34, 34, 34)
                    };
                    pnlDetails.Controls.Add(lblValue);
                    y += 32;
                }
            }
        }

        private void LoadRequestsFromDatabase()
        {
            try
            {
                userRequests = RequisitionDAL.GetRequisitionsByPatient(currentUserId);

                if (userRequests == null || userRequests.Rows.Count == 0)
                {
                    dgvRequests.Rows.Clear();
                    dgvRequests.Rows.Add("---", "---", "---", "No requests found", "---", "---", "---");

                    Label lblNoData = new Label
                    {
                        Text = "📭 You haven't submitted any blood requests yet.\n\nGo to 'Request Blood' page to submit your first request.",
                        Font = new Font("Segoe UI", 14),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
                    pnlDetails.Controls.Clear();
                    pnlDetails.Controls.Add(lblNoData);
                    return;
                }

                RefreshDataGridView();

                // Update stats panel
                foreach (Control parent in this.Controls)
                {
                    if (parent is Panel mainPanel)
                    {
                        foreach (Control ctrl in mainPanel.Controls)
                        {
                            if (ctrl is Panel statsPanel && statsPanel.Height == 120)
                            {
                                UpdateStatsCards(statsPanel);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshDataGridView()
        {
            dgvRequests.Rows.Clear();

            foreach (DataRow row in userRequests.Rows)
            {
                string reqNo = row["RequisitionNumber"]?.ToString() ?? "N/A";
                string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                string units = row["UnitsNeeded"]?.ToString() ?? "1";
                string hospital = row["Hospital"]?.ToString() ?? "N/A";
                string urgency = row["Urgency"]?.ToString() ?? "Normal";
                string status = row["Status"]?.ToString() ?? "Pending";
                string date = Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy");
                int reqId = Convert.ToInt32(row["RequisitionID"]);

                dgvRequests.Rows.Add(reqNo, bloodGroup, $"{units} Unit(s)", hospital, urgency, status, date, reqId);
            }
        }

        private void FilterRequests()
        {
            if (userRequests == null) return;

            string searchText = txtSearch.Text.Trim().ToLower();
            string statusFilter = cmbStatusFilter.SelectedItem?.ToString();

            dgvRequests.Rows.Clear();

            foreach (DataRow row in userRequests.Rows)
            {
                string reqNo = row["RequisitionNumber"]?.ToString() ?? "";
                string hospital = row["Hospital"]?.ToString() ?? "";
                string status = row["Status"]?.ToString() ?? "";

                bool matchSearch = string.IsNullOrEmpty(searchText) ||
                                   reqNo.ToLower().Contains(searchText) ||
                                   hospital.ToLower().Contains(searchText);

                bool matchStatus = statusFilter == "All" || status == statusFilter;

                if (matchSearch && matchStatus)
                {
                    string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                    string units = row["UnitsNeeded"]?.ToString() ?? "1";
                    string urgency = row["Urgency"]?.ToString() ?? "Normal";
                    string date = Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy");
                    int reqId = Convert.ToInt32(row["RequisitionID"]);

                    dgvRequests.Rows.Add(reqNo, bloodGroup, $"{units} Unit(s)", hospital, urgency, status, date, reqId);
                }
            }

            if (dgvRequests.Rows.Count == 0)
            {
                dgvRequests.Rows.Add("---", "---", "---", "No matching requests", "---", "---", "---");
            }
        }

        private void DgvRequests_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvRequests.Rows.Count)
            {
                int reqId = Convert.ToInt32(dgvRequests.Rows[e.RowIndex].Cells["ReqID"].Value);
                DisplayRequestDetails(reqId);
            }
        }

        private void DisplayRequestDetails(int reqId)
        {
            try
            {
                DataRow row = RequisitionDAL.GetRequisitionByID(reqId);
                if (row == null) return;

                foreach (Control ctrl in pnlDetails.Controls)
                {
                    string value = "-";

                    if (ctrl.Name == "lblReqNo")
                        value = row["RequisitionNumber"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblPatientName")
                        value = row["PatientName"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblBloodGroup")
                        value = row["BloodGroup"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblUnits")
                        value = row["UnitsNeeded"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblHospital")
                        value = row["Hospital"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblUrgency")
                    {
                        value = row["Urgency"]?.ToString() ?? "Normal";
                        if (ctrl is Label lbl)
                        {
                            if (value == "Emergency")
                                lbl.ForeColor = Color.FromArgb(220, 38, 38);
                            else if (value == "Urgent")
                                lbl.ForeColor = Color.FromArgb(245, 158, 11);
                            else
                                lbl.ForeColor = Color.FromArgb(34, 197, 94);
                        }
                    }
                    else if (ctrl.Name == "lblStatus")
                    {
                        value = row["Status"]?.ToString() ?? "Pending";
                        if (ctrl is Label lbl)
                        {
                            if (value == "Approved")
                                lbl.ForeColor = Color.FromArgb(34, 197, 94);
                            else if (value == "Pending")
                                lbl.ForeColor = Color.FromArgb(245, 158, 11);
                            else if (value == "Rejected")
                                lbl.ForeColor = Color.FromArgb(220, 38, 38);
                            else if (value == "Completed")
                                lbl.ForeColor = Color.FromArgb(59, 130, 246);
                            else
                                lbl.ForeColor = Color.FromArgb(80, 80, 90);
                        }
                    }
                    else if (ctrl.Name == "lblDate")
                        value = Convert.ToDateTime(row["RequestDate"]).ToString("dd-MMM-yyyy hh:mm tt");
                    else if (ctrl.Name == "lblApprovalDate")
                    {
                        if (row["ApprovalDate"] != DBNull.Value)
                            value = Convert.ToDateTime(row["ApprovalDate"]).ToString("dd-MMM-yyyy hh:mm tt");
                        else
                            value = "Not yet approved";
                    }
                    else if (ctrl.Name == "lblRemarks" && ctrl is TextBox txt)
                    {
                        txt.Text = row["Remarks"]?.ToString() ?? "No remarks";
                        continue;
                    }

                    if (ctrl is Label lblCtrl && ctrl.Name != "lblUrgency" && ctrl.Name != "lblStatus")
                    {
                        lblCtrl.Text = value;
                    }
                    else if (ctrl is Label lbl && (ctrl.Name == "lblUrgency" || ctrl.Name == "lblStatus"))
                    {
                        lbl.Text = value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DisplayRequestDetails Error: {ex.Message}");
            }
        }

        private void DgvRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5 && e.Value != null) // Status column
            {
                string status = e.Value.ToString();
                if (status == "Approved")
                    e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94);
                else if (status == "Pending")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (status == "Rejected")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (status == "Completed")
                    e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(80, 80, 90);

                e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == 4 && e.Value != null) // Urgency column
            {
                string urgency = e.Value.ToString();
                if (urgency == "Emergency")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (urgency == "Urgent")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94);
            }
        }

        private void MakeRounded(Control control, int radius)
        {
            control.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(control.Width - d, 0, d, d, 270, 90);
                path.AddArc(control.Width - d, control.Height - d, d, d, 0, 90);
                path.AddArc(0, control.Height - d, d, d, 90, 90);
                control.Region = new Region(path);
            };
        }
    }
}