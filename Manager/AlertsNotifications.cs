using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Manager
{
    public partial class AlertsNotifications : Form
    {
        private TableLayoutPanel mainLayout;
        private Panel topPanel;
        private Label lblStatus;
        private DataGridView dgvNotifications;
        private ComboBox cmbPriorityFilter, cmbStatusFilter;
        private DateTimePicker dtpFromDate, dtpToDate;
        private Button btnRefresh, btnMarkRead, btnDelete, btnExport;
        private DataTable notificationData;
        private Timer refreshTimer;

        public AlertsNotifications()
        {
            InitializeComponent();
            this.Text = "Alerts & Notifications";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            LoadNotificationsFromDatabase();

            refreshTimer = new Timer { Interval = 60000 };
            refreshTimer.Tick += (s, e) => LoadNotificationsFromDatabase();
            refreshTimer.Start();
        }

        private void BuildUI()
        {
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.Transparent
            };
            mainLayout.RowStyles.Clear();
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            // Top panel
            topPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(5, 31, 64) };
            Label title = new Label
            {
                Text = "Alerts & Notifications",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 18)
            };
            topPanel.Controls.Add(title);

            lblStatus = new Label
            {
                Text = "✅ Ready",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(16, 185, 129),
                AutoSize = true,
                Location = new Point(topPanel.Width - 220, 22)
            };
            topPanel.Controls.Add(lblStatus);
            topPanel.Resize += (s, e) => lblStatus.Location = new Point(topPanel.Width - 220, 22);
            mainLayout.Controls.Add(topPanel, 0, 0);

            // Filter panel
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 10),
                Margin = new Padding(20, 10, 20, 10)
            };

            Label lblPriority = new Label
            {
                Text = "Priority:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(20, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblPriority);

            cmbPriorityFilter = new ComboBox
            {
                Location = new Point(80, 15),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbPriorityFilter.Items.AddRange(new[] { "All", "Emergency", "Urgent", "Important", "Normal" });
            cmbPriorityFilter.SelectedIndex = 0;
            cmbPriorityFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            filterPanel.Controls.Add(cmbPriorityFilter);

            Label lblStatusFilter = new Label
            {
                Text = "Status:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(200, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblStatusFilter);

            cmbStatusFilter = new ComboBox
            {
                Location = new Point(245, 15),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbStatusFilter.Items.AddRange(new[] { "All", "Unread", "Read" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            filterPanel.Controls.Add(cmbStatusFilter);

            Label lblFrom = new Label
            {
                Text = "From:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(370, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblFrom);

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(410, 15),
                Size = new Size(110, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now.AddDays(-30)
            };
            dtpFromDate.ValueChanged += (s, e) => ApplyFilter();
            filterPanel.Controls.Add(dtpFromDate);

            Label lblTo = new Label
            {
                Text = "To:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(535, 18),
                AutoSize = true
            };
            filterPanel.Controls.Add(lblTo);

            dtpToDate = new DateTimePicker
            {
                Location = new Point(565, 15),
                Size = new Size(110, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Now
            };
            dtpToDate.ValueChanged += (s, e) => ApplyFilter();
            filterPanel.Controls.Add(dtpToDate);

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(700, 13),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(60, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadNotificationsFromDatabase();
            filterPanel.Controls.Add(btnRefresh);
            mainLayout.Controls.Add(filterPanel, 0, 1);

            // Heading
            Label lblHeading = new Label
            {
                Text = "🔔 Notification History",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 50),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                BackColor = Color.Transparent
            };
            mainLayout.Controls.Add(lblHeading, 0, 2);

            // DataGridView
            dgvNotifications = CreateModernDataGridView();
            dgvNotifications.CellFormatting += DgvNotifications_CellFormatting;
            mainLayout.Controls.Add(dgvNotifications, 0, 3);

            // Buttons panel
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnMarkRead = CreateButton("✅ Mark as Read", Color.FromArgb(34, 197, 94));
            btnMarkRead.Click += BtnMarkRead_Click;

            btnDelete = CreateButton("🗑️ Delete", Color.FromArgb(220, 38, 38));
            btnDelete.Click += BtnDelete_Click;

            btnExport = CreateButton("📊 Export", Color.FromArgb(59, 130, 246));
            btnExport.Click += BtnExport_Click;

            buttonPanel.Controls.Add(btnMarkRead);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnExport);
            mainLayout.Controls.Add(buttonPanel, 0, 4);

            Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 38),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private DataGridView CreateModernDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(229, 231, 235),
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 40,
                Dock = DockStyle.Fill
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 242, 242);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(120, 22, 27);
            dgv.DefaultCellStyle.Padding = new Padding(10, 5, 10, 5);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 252);

            return dgv;
        }

        private void LoadNotificationsFromDatabase()
        {
            try
            {
                UpdateStatus("🔄 Loading notifications...", Color.FromArgb(59, 130, 246));

                notificationData = NotificationDAL.GetByUser(SessionManager.CurrentUserID);

                if (notificationData != null && notificationData.Rows.Count > 0)
                {
                    dgvNotifications.DataSource = notificationData;
                    FormatGrid();
                }
                else
                {
                    dgvNotifications.DataSource = null;
                    dgvNotifications.Rows.Clear();
                    dgvNotifications.Columns.Clear();
                    dgvNotifications.Columns.Add("Message", "Information");
                    dgvNotifications.Rows.Add("No notifications found.");
                }

                UpdateStatus($"✅ Loaded {(notificationData?.Rows.Count ?? 0)} notification(s)", Color.FromArgb(16, 185, 129));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadNotificationsFromDatabase Error: {ex.Message}");
                dgvNotifications.DataSource = null;
                UpdateStatus($"❌ Error: {ex.Message}", Color.FromArgb(220, 38, 38));
            }
        }

        private void FormatGrid()
        {
            if (dgvNotifications.Columns.Count == 0) return;

            if (dgvNotifications.Columns.Contains("NotificationID"))
                dgvNotifications.Columns["NotificationID"].Visible = false;

            if (dgvNotifications.Columns.Contains("Title"))
                dgvNotifications.Columns["Title"].HeaderText = "Title";

            if (dgvNotifications.Columns.Contains("Message"))
                dgvNotifications.Columns["Message"].HeaderText = "Message";

            if (dgvNotifications.Columns.Contains("Priority"))
                dgvNotifications.Columns["Priority"].HeaderText = "Priority";

            if (dgvNotifications.Columns.Contains("Type"))
                dgvNotifications.Columns["Type"].HeaderText = "Type";

            if (dgvNotifications.Columns.Contains("IsRead"))
            {
                dgvNotifications.Columns["IsRead"].HeaderText = "Status";
                dgvNotifications.Columns["IsRead"].Visible = false;
            }

            if (dgvNotifications.Columns.Contains("CreatedAt"))
                dgvNotifications.Columns["CreatedAt"].HeaderText = "Date/Time";
        }

        private void ApplyFilter()
        {
            if (notificationData == null) return;

            string priority = cmbPriorityFilter.SelectedItem?.ToString();
            string statusFilter = cmbStatusFilter.SelectedItem?.ToString();
            DateTime fromDate = dtpFromDate.Value.Date;
            DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

            DataTable filteredDt = notificationData.Clone();

            foreach (DataRow row in notificationData.Rows)
            {
                bool match = true;

                if (priority != "All")
                {
                    string rowPriority = row["Priority"]?.ToString() ?? "";
                    if (rowPriority != priority) match = false;
                }

                if (match && statusFilter != "All")
                {
                    bool isRead = row["IsRead"] != DBNull.Value && Convert.ToBoolean(row["IsRead"]);
                    if (statusFilter == "Unread" && isRead) match = false;
                    if (statusFilter == "Read" && !isRead) match = false;
                }

                if (match && row["CreatedAt"] != DBNull.Value)
                {
                    DateTime createdAt = Convert.ToDateTime(row["CreatedAt"]);
                    if (createdAt < fromDate || createdAt > toDate) match = false;
                }

                if (match) filteredDt.ImportRow(row);
            }

            dgvNotifications.DataSource = filteredDt;
            UpdateStatus($"🔍 Showing {filteredDt.Rows.Count} of {notificationData.Rows.Count} notifications", Color.FromArgb(59, 130, 246));
        }

        private void DgvNotifications_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && dgvNotifications.Columns[e.ColumnIndex].HeaderText == "Priority" && e.Value != null)
            {
                string priority = e.Value.ToString();
                if (priority == "Emergency")
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                else if (priority == "Urgent")
                    e.CellStyle.ForeColor = Color.FromArgb(245, 158, 11);
                else if (priority == "Important")
                    e.CellStyle.ForeColor = Color.FromArgb(59, 130, 246);
                else
                    e.CellStyle.ForeColor = Color.FromArgb(16, 185, 129);
            }
        }

        private void BtnMarkRead_Click(object sender, EventArgs e)
        {
            if (dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a notification to mark as read.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int notificationID = 0;
                if (dgvNotifications.SelectedRows[0].DataBoundItem is DataRowView row)
                {
                    notificationID = Convert.ToInt32(row["NotificationID"]);
                }

                if (notificationID > 0 && NotificationDAL.MarkAsRead(notificationID))
                {
                    UpdateStatus("✅ Notification marked as read", Color.FromArgb(16, 185, 129));
                    LoadNotificationsFromDatabase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvNotifications.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a notification to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this notification?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                int notificationID = 0;
                if (dgvNotifications.SelectedRows[0].DataBoundItem is DataRowView row)
                {
                    notificationID = Convert.ToInt32(row["NotificationID"]);
                }

                if (notificationID > 0 && NotificationDAL.Delete(notificationID))
                {
                    UpdateStatus("🗑️ Notification deleted", Color.FromArgb(16, 185, 129));
                    LoadNotificationsFromDatabase();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable exportData = dgvNotifications.DataSource as DataTable;
                if (exportData == null || exportData.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xls)|*.xls",
                    DefaultExt = "csv",
                    FileName = $"Notifications_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ExportHelper.ExportToCSV(exportData, sfd.FileName);
                    MessageBox.Show($"Notifications exported successfully!\n\n{sfd.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                lblStatus.ForeColor = color;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}