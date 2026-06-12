using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class PatientTransfusionHistory : Form
    {
        private DataGridView dgvHistory;
        private Panel pnlDetails;
        private Label lblStatus;
        private ComboBox cmbFilter;
        private TextBox txtSearch;
        private Button btnRefresh;
        private int currentPatientId;
        private int currentUserId;
        private DataTable transfusionData;

        public PatientTransfusionHistory()
        {
            InitializeComponent();
            LoadCurrentPatient();
            BuildUI();
            LoadTransfusionHistory();
        }

        private void LoadCurrentPatient()
        {
            try
            {
                currentUserId = SessionManager.CurrentUserID;
                var patient = PatientDAL.GetByUserID(currentUserId);
                if (patient != null)
                {
                    currentPatientId = patient.PatientID;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCurrentPatient Error: {ex.Message}");
            }
        }

        private void BuildUI()
        {
            this.Text = "My Transfusion History - Patient";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(mainPanel);

            // Title
            Label lblTitle = new Label
            {
                Text = "💉 My Transfusion History",
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
            txtSearch.TextChanged += (s, e) => FilterHistory();

            Label lblFilter = new Label { Text = "Filter:", Location = new Point(310, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbFilter = new ComboBox
            {
                Location = new Point(360, 15),
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbFilter.Items.AddRange(new string[] { "All", "Last 30 Days", "Last 6 Months", "Last Year", "Completed", "Reaction" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => FilterHistory();

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(520, 13),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            MakeRounded(btnRefresh, 8);
            btnRefresh.Click += (s, e) => LoadTransfusionHistory();

            filterPanel.Controls.Add(lblSearch);
            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(lblFilter);
            filterPanel.Controls.Add(cmbFilter);
            filterPanel.Controls.Add(btnRefresh);

            // Main Content Split
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 350,
                SplitterWidth = 5
            };
            mainPanel.Controls.Add(splitContainer);

            // Top: History List
            Panel listPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            MakeRounded(listPanel, 12);
            splitContainer.Panel1.Controls.Add(listPanel);

            Label lblListTitle = new Label
            {
                Text = "Transfusion Records",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 34, 34),
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            listPanel.Controls.Add(lblListTitle);

            // DataGridView
            dgvHistory = new DataGridView
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
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgvHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvHistory.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 226, 226);
            dgvHistory.DefaultCellStyle.SelectionForeColor = Color.FromArgb(120, 22, 27);
            dgvHistory.CellClick += DgvHistory_CellClick;
            dgvHistory.CellFormatting += DgvHistory_CellFormatting;
            listPanel.Controls.Add(dgvHistory);

            // Add columns
            dgvHistory.Columns.Add("Date", "Date");
            dgvHistory.Columns.Add("BloodGroup", "Blood Group");
            dgvHistory.Columns.Add("Units", "Units");
            dgvHistory.Columns.Add("Hospital", "Hospital");
            dgvHistory.Columns.Add("Doctor", "Doctor");
            dgvHistory.Columns.Add("Status", "Status");
            dgvHistory.Columns.Add("ID", "ID");
            dgvHistory.Columns["ID"].Visible = false;

            // Bottom: Details Panel
            pnlDetails = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20), AutoScroll = true };
            MakeRounded(pnlDetails, 12);
            splitContainer.Panel2.Controls.Add(pnlDetails);

            SetupDetailsPanel();

            // Status Label
            lblStatus = new Label
            {
                Text = "Loading transfusion history...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblStatus);
        }

        private void UpdateStatsCards(Panel parent)
        {
            parent.Controls.Clear();

            int total = 0, last30Days = 0, last6Months = 0, withReaction = 0;

            if (transfusionData != null)
            {
                total = transfusionData.Rows.Count;
                DateTime today = DateTime.Today;

                foreach (DataRow row in transfusionData.Rows)
                {
                    DateTime transDate = Convert.ToDateTime(row["TransfusionDate"]);
                    if ((today - transDate).Days <= 30) last30Days++;
                    if ((today - transDate).Days <= 180) last6Months++;

                    string reaction = row["ReactionType"]?.ToString();
                    if (!string.IsNullOrEmpty(reaction) && reaction != "None")
                        withReaction++;
                }
            }

            var stats = new (string title, string value, Color color)[]
            {
                ("Total Transfusions", total.ToString(), Color.FromArgb(120, 22, 27)),
                ("Last 30 Days", last30Days.ToString(), Color.FromArgb(34, 197, 94)),
                ("Last 6 Months", last6Months.ToString(), Color.FromArgb(59, 130, 246)),
                ("With Reaction", withReaction.ToString(), Color.FromArgb(245, 158, 11))
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
                Text = "Transfusion Details",
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
                ("Patient Name:", "lblPatientName"),
                ("Blood Group:", "lblBloodGroup"),
                ("Units:", "lblUnits"),
                ("Hospital:", "lblHospital"),
                ("Doctor:", "lblDoctor"),
                ("Transfusion Date:", "lblDate"),
                ("Status:", "lblStatus"),
                ("Reaction Type:", "lblReaction"),
                ("Notes:", "lblNotes")
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

                if (item.name == "lblNotes")
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
                        Font = new Font("Segoe UI", 10),
                        ForeColor = Color.FromArgb(34, 34, 34)
                    };
                    pnlDetails.Controls.Add(lblValue);
                    y += 32;
                }
            }
        }

        private void LoadTransfusionHistory()
        {
            try
            {
                if (currentPatientId == 0)
                {
                    lblStatus.Text = "⚠️ Patient profile not found. Please complete your profile first.";
                    lblStatus.ForeColor = Color.Orange;
                    return;
                }

                transfusionData = TransfusionHistoryDAL.GetByPatientID(currentPatientId);

                if (transfusionData == null || transfusionData.Rows.Count == 0)
                {
                    dgvHistory.Rows.Clear();
                    dgvHistory.Rows.Add("---", "---", "---", "No transfusion records found", "---", "---");
                    lblStatus.Text = "📭 No transfusion history found.";
                    lblStatus.ForeColor = Color.Gray;

                    // Show empty message in details panel
                    pnlDetails.Controls.Clear();
                    Label lblNoData = new Label
                    {
                        Text = "📭 No transfusion records found.\n\nWhen you receive blood, your transfusion history will appear here.",
                        Font = new Font("Segoe UI", 14),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };
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

                lblStatus.Text = $"💉 {transfusionData.Rows.Count} transfusion record(s) found";
                lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading transfusion history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Error loading data";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void RefreshDataGridView()
        {
            dgvHistory.Rows.Clear();

            foreach (DataRow row in transfusionData.Rows)
            {
                string date = Convert.ToDateTime(row["TransfusionDate"]).ToString("dd-MMM-yyyy");
                string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                string units = row["Units"]?.ToString() ?? "1";
                string hospital = row["Hospital"]?.ToString() ?? "N/A";
                string doctor = row["DoctorName"]?.ToString() ?? "N/A";
                string status = row["Status"]?.ToString() ?? "Completed";
                int transId = Convert.ToInt32(row["TransfusionID"]);

                dgvHistory.Rows.Add(date, bloodGroup, $"{units} Unit(s)", hospital, doctor, status, transId);
            }
        }

        private void FilterHistory()
        {
            if (transfusionData == null) return;

            string searchText = txtSearch.Text.Trim().ToLower();
            string filterType = cmbFilter.SelectedItem?.ToString();

            dgvHistory.Rows.Clear();

            foreach (DataRow row in transfusionData.Rows)
            {
                string hospital = row["Hospital"]?.ToString()?.ToLower() ?? "";
                string doctor = row["DoctorName"]?.ToString()?.ToLower() ?? "";
                DateTime transDate = Convert.ToDateTime(row["TransfusionDate"]);
                string status = row["Status"]?.ToString() ?? "";
                string reaction = row["ReactionType"]?.ToString() ?? "";

                bool matchSearch = string.IsNullOrEmpty(searchText) ||
                                   hospital.Contains(searchText) ||
                                   doctor.Contains(searchText);

                bool matchFilter = true;
                if (filterType == "Last 30 Days")
                    matchFilter = (DateTime.Today - transDate).Days <= 30;
                else if (filterType == "Last 6 Months")
                    matchFilter = (DateTime.Today - transDate).Days <= 180;
                else if (filterType == "Last Year")
                    matchFilter = (DateTime.Today - transDate).Days <= 365;
                else if (filterType == "Completed")
                    matchFilter = status == "Completed";
                else if (filterType == "Reaction")
                    matchFilter = !string.IsNullOrEmpty(reaction) && reaction != "None";

                if (matchSearch && matchFilter)
                {
                    string date = transDate.ToString("dd-MMM-yyyy");
                    string bloodGroup = row["BloodGroup"]?.ToString() ?? "N/A";
                    string units = row["Units"]?.ToString() ?? "1";
                    string hospitalName = row["Hospital"]?.ToString() ?? "N/A";
                    string doctorName = row["DoctorName"]?.ToString() ?? "N/A";
                    string statusVal = row["Status"]?.ToString() ?? "Completed";
                    int transId = Convert.ToInt32(row["TransfusionID"]);

                    dgvHistory.Rows.Add(date, bloodGroup, $"{units} Unit(s)", hospitalName, doctorName, statusVal, transId);
                }
            }

            if (dgvHistory.Rows.Count == 0)
            {
                dgvHistory.Rows.Add("---", "---", "---", "No matching records", "---", "---");
            }
        }

        private void DgvHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvHistory.Rows.Count)
            {
                int transId = Convert.ToInt32(dgvHistory.Rows[e.RowIndex].Cells["ID"].Value);
                DisplayTransfusionDetails(transId);
            }
        }

        private void DisplayTransfusionDetails(int transId)
        {
            try
            {
                DataRow row = TransfusionHistoryDAL.GetByID(transId);
                if (row == null) return;

                foreach (Control ctrl in pnlDetails.Controls)
                {
                    if (ctrl.Name == "lblPatientName")
                        ctrl.Text = row["PatientName"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblBloodGroup")
                        ctrl.Text = row["BloodGroup"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblUnits")
                        ctrl.Text = $"{row["Units"]} Unit(s)";
                    else if (ctrl.Name == "lblHospital")
                        ctrl.Text = row["Hospital"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblDoctor")
                        ctrl.Text = row["DoctorName"]?.ToString() ?? "-";
                    else if (ctrl.Name == "lblDate")
                        ctrl.Text = Convert.ToDateTime(row["TransfusionDate"]).ToString("dd-MMM-yyyy hh:mm tt");
                    else if (ctrl.Name == "lblStatus")
                    {
                        string status = row["Status"]?.ToString() ?? "Completed";
                        ctrl.Text = status;
                        if (ctrl is Label lbl)
                        {
                            if (status == "Completed")
                                lbl.ForeColor = Color.FromArgb(34, 197, 94);
                            else
                                lbl.ForeColor = Color.FromArgb(245, 158, 11);
                        }
                    }
                    else if (ctrl.Name == "lblReaction")
                    {
                        string reaction = row["ReactionType"]?.ToString();
                        if (string.IsNullOrEmpty(reaction)) reaction = "None";
                        ctrl.Text = reaction;
                        if (ctrl is Label lbl && reaction != "None")
                            lbl.ForeColor = Color.FromArgb(220, 38, 38);
                    }
                    else if (ctrl.Name == "lblNotes" && ctrl is TextBox txt)
                    {
                        txt.Text = row["Notes"]?.ToString() ?? "No notes available";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DisplayTransfusionDetails Error: {ex.Message}");
            }
        }

        private void DgvHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5 && e.Value != null) // Status column
            {
                string status = e.Value.ToString();
                if (status == "Completed")
                    e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);

                e.CellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
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