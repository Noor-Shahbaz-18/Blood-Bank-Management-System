using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class AuditLog : Form
    {
        private DataGridView dgvLogs;
        private TextBox txtFilter;
        private ComboBox cmbActionFilter;
        private DateTimePicker dtpFromDate, dtpToDate;
        private Button btnRefresh, btnClearFilter;
        private Label lblTitle;

        public AuditLog()
        {
            InitializeComponent();
            BuildUI();
            LoadAuditLogs();
        }

        private void BuildUI()
        {
            this.Text = "Audit Logs - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1100, 650);

            // ═══════════════════════════════════════════════════════════════
            // Windows Forms DockStyle.Top GOLDEN RULE:
            // this.Controls mein order ULTA hai —
            // Jo control AAKHIR pe dikhna chahiye, woh PEHLE add karo
            // Jo control UPAR dikhna chahiye, woh BAAD MEIN add karo
            //
            // Display order (upar → neeche):  Filter → Title → Grid
            // Add order (code mein):           Grid → Title → Filter
            // ═══════════════════════════════════════════════════════════════

            // ── STEP 1: Grid — PEHLE add karo (Fill, neeche dikhai dega) ─
            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                Font = new Font("Segoe UI", 9),
                GridColor = Color.FromArgb(220, 223, 230),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            dgvLogs.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgvLogs.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLogs.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvLogs.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgvLogs.ColumnHeadersHeight = 40;
            dgvLogs.EnableHeadersVisualStyles = false;
            dgvLogs.RowTemplate.Height = 32;
            dgvLogs.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            dgvLogs.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 252);

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColDateTime", HeaderText = "Date & Time", Width = 165 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColUser", HeaderText = "User", Width = 120 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRole", HeaderText = "Role", Width = 110 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColAction", HeaderText = "Action", Width = 100 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColModule", HeaderText = "Module", Width = 150 });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColDetails", HeaderText = "Details", Width = 260, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColStatus", HeaderText = "Status", Width = 90 });

            this.Controls.Add(dgvLogs);  // ← 1st add (Fill — sabse neeche)

            // ── STEP 2: Title — BEECH mein add karo (Top, beech mein dikhai dega) ─
            lblTitle = new Label
            {
                Text = "📋 Audit Logs",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(15, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(lblTitle);  // ← 2nd add (upar dikhega, filter ke neeche)

            // ── STEP 3: Filter Bar — AAKHIR mein add karo (Top, sabse upar dikhai dega) ─
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.White,
                Padding = new Padding(12, 0, 12, 0)
            };

            int fx = 12, fy = 13;

            filterPanel.Controls.Add(new Label
            {
                Text = "Search:",
                Location = new Point(fx, fy + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            });
            fx += 52;

            txtFilter = new TextBox
            {
                Location = new Point(fx, fy),
                Width = 185,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtFilter.TextChanged += (s, e) => LoadAuditLogs();
            filterPanel.Controls.Add(txtFilter);
            fx += 198;

            filterPanel.Controls.Add(new Label
            {
                Text = "Action:",
                Location = new Point(fx, fy + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            });
            fx += 50;

            cmbActionFilter = new ComboBox
            {
                Location = new Point(fx, fy),
                Width = 110,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbActionFilter.Items.AddRange(new string[] { "All", "Login", "Add", "Edit", "Delete", "View" });
            cmbActionFilter.SelectedIndex = 0;
            cmbActionFilter.SelectedIndexChanged += (s, e) => LoadAuditLogs();
            filterPanel.Controls.Add(cmbActionFilter);
            fx += 122;

            filterPanel.Controls.Add(new Label
            {
                Text = "From:",
                Location = new Point(fx, fy + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            });
            fx += 42;

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(fx, fy),
                Width = 112,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddDays(-30)
            };
            dtpFromDate.ValueChanged += (s, e) => LoadAuditLogs();
            filterPanel.Controls.Add(dtpFromDate);
            fx += 124;

            filterPanel.Controls.Add(new Label
            {
                Text = "To:",
                Location = new Point(fx, fy + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            });
            fx += 28;

            dtpToDate = new DateTimePicker
            {
                Location = new Point(fx, fy),
                Width = 112,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };
            dtpToDate.ValueChanged += (s, e) => LoadAuditLogs();
            filterPanel.Controls.Add(dtpToDate);
            fx += 124;

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(fx, fy - 1),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadAuditLogs();
            filterPanel.Controls.Add(btnRefresh);
            fx += 105;

            btnClearFilter = new Button
            {
                Text = "Clear Filters",
                Location = new Point(fx, fy - 1),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(100, 110, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9)
            };
            btnClearFilter.FlatAppearance.BorderSize = 0;
            btnClearFilter.Click += (s, e) => ClearFilters();
            filterPanel.Controls.Add(btnClearFilter);

            this.Controls.Add(filterPanel);  // ← 3rd (LAST) add — sabse upar dikhega
        }

        private void ClearFilters()
        {
            txtFilter.Text = "";
            cmbActionFilter.SelectedIndex = 0;
            dtpFromDate.Value = DateTime.Today.AddDays(-30);
            dtpToDate.Value = DateTime.Today;
            LoadAuditLogs();
        }

        private void LoadAuditLogs()
        {
            try
            {
                dgvLogs.Rows.Clear();

                DateTime fromDate = dtpFromDate.Value.Date;
                DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

                DataTable dt = AuditLogDAL.GetLogsByDateRange(fromDate, toDate);

                if (dt == null || dt.Rows.Count == 0)
                {
                    lblTitle.Text = "📋 Audit Logs  (0 records)";
                    return;
                }

                string searchText = txtFilter.Text.Trim().ToLower();
                string actionFilter = cmbActionFilter.SelectedItem?.ToString() ?? "All";

                int count = 0;
                foreach (DataRow row in dt.Rows)
                {
                    // Search filter
                    bool match = true;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        match = false;
                        foreach (DataColumn col in dt.Columns)
                            if (row[col]?.ToString().ToLower().Contains(searchText) == true)
                            { match = true; break; }
                    }

                    // Action filter
                    if (match && actionFilter != "All")
                        match = row["Action"]?.ToString() == actionFilter;

                    if (!match) continue;

                    string status = row["Status"]?.ToString() ?? "-";
                    string details = row["NewValue"]?.ToString();
                    if (string.IsNullOrWhiteSpace(details))
                        details = row["OldValue"]?.ToString() ?? "-";

                    int rowIdx = dgvLogs.Rows.Add(
                        Convert.ToDateTime(row["ActionDateTime"]).ToString("dd-MMM-yyyy HH:mm:ss"),
                        row["Username"]?.ToString() ?? "-",
                        row["UserRole"]?.ToString() ?? "-",
                        row["Action"]?.ToString() ?? "-",
                        row["EntityType"]?.ToString() ?? "-",
                        details,
                        status
                    );

                    // Status color
                    var cell = dgvLogs.Rows[rowIdx].Cells["ColStatus"];
                    cell.Style.ForeColor = status == "Success"
                        ? Color.FromArgb(0, 150, 70)
                        : Color.FromArgb(200, 30, 30);
                    cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                    count++;
                }

                lblTitle.Text = $"📋 Audit Logs  ({count} records)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}