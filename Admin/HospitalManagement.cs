using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class HospitalManagement : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private DataGridView hospitalTable;
        private TextBox searchInput;

        public HospitalManagement()
        {
            this.Text = "Hospital Management - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 247, 252);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildMainContent();
            BuildSidebar();
            LoadHospitals();
        }

        // =====================================================
        // LOAD HOSPITALS FROM DATABASE
        // =====================================================
        private void LoadHospitals()
        {
            try
            {
                DataTable dt = HospitalDAL.GetAllHospitals();
                hospitalTable.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    hospitalTable.Rows.Add(
                        row["HospitalID"],
                        row["Name"],
                        row["City"],
                        row["Email"],
                        row["Phone"],
                        row["IsActive"].ToString() == "True" ? "Active" : "Inactive",
                        "Edit",
                        "Delete"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading hospitals: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // FILTER TABLE (SEARCH)
        // =====================================================
        private void FilterTable(string query)
        {
            if (hospitalTable == null) return;
            string q = query.ToLower().Trim();
            bool isEmpty = string.IsNullOrEmpty(q) || q == "search hospital...";

            foreach (DataGridViewRow row in hospitalTable.Rows)
            {
                if (isEmpty) { row.Visible = true; continue; }
                bool match = false;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLower().Contains(q))
                    {
                        match = true;
                        break;
                    }
                }
                row.Visible = match;
            }
        }

        // =====================================================
        // MAIN CONTENT
        // =====================================================
        private void BuildMainContent()
        {
            mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.BackColor = Color.FromArgb(244, 247, 252);
            mainPanel.Padding = new Padding(18, 12, 18, 12);

            // ----- TOPBAR -----
            Panel topbarPanel = new Panel();
            topbarPanel.Dock = DockStyle.Top;
            topbarPanel.Height = 65;
            topbarPanel.BackColor = Color.Transparent;

            Label titleLabel = new Label();
            titleLabel.Text = "Hospital Management";
            titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(20, 20, 30);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 14);

            // Search box
            Panel searchBox = new Panel();
            searchBox.Size = new Size(260, 38);
            searchBox.BackColor = Color.White;
            searchBox.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 8))
                {
                    p.Region = new Region(gp);
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 228), 1f))
                        e.Graphics.DrawPath(pen, gp);
                }
            };

            Label searchIcon = new Label();
            searchIcon.Text = "🔍";
            searchIcon.Font = new Font("Segoe UI Emoji", 11, FontStyle.Regular);
            searchIcon.BackColor = Color.Transparent;
            searchIcon.AutoSize = true;
            searchIcon.Location = new Point(10, 9);

            searchInput = new TextBox();
            searchInput.BorderStyle = BorderStyle.None;
            searchInput.Font = new Font("Segoe UI", 10);
            searchInput.Size = new Size(200, 22);
            searchInput.Location = new Point(38, 10);
            searchInput.Text = "Search hospital...";
            searchInput.ForeColor = Color.Gray;
            searchInput.Enter += (s, e) => { if (searchInput.Text == "Search hospital...") { searchInput.Text = ""; searchInput.ForeColor = Color.Black; } };
            searchInput.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(searchInput.Text)) { searchInput.Text = "Search hospital..."; searchInput.ForeColor = Color.Gray; } };
            searchInput.TextChanged += (s, e) => FilterTable(searchInput.Text);

            searchBox.Controls.Add(searchIcon);
            searchBox.Controls.Add(searchInput);

            // Add Hospital Button
            Panel addButton = new Panel();
            addButton.Size = new Size(150, 38);
            addButton.BackColor = Color.FromArgb(37, 99, 235);
            addButton.Cursor = Cursors.Hand;
            addButton.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 8))
                using (SolidBrush br = new SolidBrush(p.BackColor))
                {
                    p.Region = new Region(gp);
                    e.Graphics.FillPath(br, gp);
                }
            };

            Label addLabel = new Label();
            addLabel.Text = "+ Add Hospital";
            addLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            addLabel.ForeColor = Color.White;
            addLabel.BackColor = Color.Transparent;
            addLabel.AutoSize = false;
            addLabel.Size = new Size(150, 38);
            addLabel.Location = new Point(0, 0);
            addLabel.TextAlign = ContentAlignment.MiddleCenter;
            addLabel.Cursor = Cursors.Hand;

            // Add Hospital Click Event
            EventHandler addClick = (s, e) =>
            {
                using (AddEditHospital addForm = new AddEditHospital())
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadHospitals();
                    }
                }
            };
            addButton.Click += addClick;
            addLabel.Click += addClick;

            addButton.Controls.Add(addLabel);

            topbarPanel.Controls.Add(titleLabel);
            topbarPanel.Controls.Add(searchBox);
            topbarPanel.Controls.Add(addButton);

            EventHandler reposition = (s, e) => {
                int r = topbarPanel.ClientSize.Width - 12;
                addButton.Location = new Point(r - addButton.Width, 13);
                searchBox.Location = new Point(addButton.Left - searchBox.Width - 12, 13);
            };
            topbarPanel.Resize += reposition;
            topbarPanel.HandleCreated += reposition;

            // ----- TABLE PANEL -----
            Panel tablePanel = new Panel();
            tablePanel.Dock = DockStyle.Fill;
            tablePanel.BackColor = Color.White;
            tablePanel.Padding = new Padding(12, 8, 12, 12);
            tablePanel.Paint += (s, e) => {
                var p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = RoundRect(p.ClientRectangle, 12))
                {
                    p.Region = new Region(gp);
                    using (SolidBrush br = new SolidBrush(Color.White))
                        e.Graphics.FillPath(br, gp);
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 225), 1f))
                        e.Graphics.DrawPath(pen, gp);
                }
            };

            Label headerLabel = new Label();
            headerLabel.Text = "All Hospitals";
            headerLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            headerLabel.ForeColor = Color.FromArgb(20, 20, 30);
            headerLabel.BackColor = Color.Transparent;
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Height = 36;
            headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            headerLabel.Padding = new Padding(4, 0, 0, 0);

            // ----- DATAGRIDVIEW -----
            hospitalTable = new DataGridView();
            hospitalTable.Dock = DockStyle.Fill;
            hospitalTable.BackgroundColor = Color.White;
            hospitalTable.BorderStyle = BorderStyle.None;
            hospitalTable.RowHeadersVisible = false;
            hospitalTable.AllowUserToAddRows = false;
            hospitalTable.AllowUserToDeleteRows = false;
            hospitalTable.AllowUserToResizeRows = false;
            hospitalTable.AllowUserToResizeColumns = false;
            hospitalTable.ReadOnly = true;
            hospitalTable.EditMode = DataGridViewEditMode.EditProgrammatically;
            hospitalTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            hospitalTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            hospitalTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            hospitalTable.ColumnHeadersHeight = 42;
            hospitalTable.RowTemplate.Height = 42;
            hospitalTable.GridColor = Color.FromArgb(235, 235, 240);
            hospitalTable.EnableHeadersVisualStyles = false;
            hospitalTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            hospitalTable.DefaultCellStyle = new DataGridViewCellStyle();
            hospitalTable.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            hospitalTable.DefaultCellStyle.BackColor = Color.White;
            hospitalTable.DefaultCellStyle.ForeColor = Color.FromArgb(30, 30, 40);
            hospitalTable.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 244, 255);
            hospitalTable.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 30, 40);
            hospitalTable.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            hospitalTable.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle();
            hospitalTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            hospitalTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
            hospitalTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 70);
            hospitalTable.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            // ============================================
            // COLUMNS - Exactly like UserManagement
            // ============================================

            // 1. HospitalID Column (HIDDEN)
            var colId = new DataGridViewTextBoxColumn();
            colId.Name = "HospitalID";
            colId.HeaderText = "ID";
            colId.Visible = false;
            colId.ReadOnly = true;
            hospitalTable.Columns.Add(colId);

            // 2. Hospital Name Column
            var colName = new DataGridViewTextBoxColumn();
            colName.Name = "HospitalName";
            colName.HeaderText = "Hospital Name";
            colName.FillWeight = 25;
            colName.ReadOnly = true;
            hospitalTable.Columns.Add(colName);

            // 3. City Column
            var colCity = new DataGridViewTextBoxColumn();
            colCity.Name = "City";
            colCity.HeaderText = "City";
            colCity.FillWeight = 15;
            colCity.ReadOnly = true;
            hospitalTable.Columns.Add(colCity);

            // 4. Email Column
            var colEmail = new DataGridViewTextBoxColumn();
            colEmail.Name = "Email";
            colEmail.HeaderText = "Email";
            colEmail.FillWeight = 20;
            colEmail.ReadOnly = true;
            hospitalTable.Columns.Add(colEmail);

            // 5. Contact Column
            var colContact = new DataGridViewTextBoxColumn();
            colContact.Name = "Contact";
            colContact.HeaderText = "Contact";
            colContact.FillWeight = 15;
            colContact.ReadOnly = true;
            hospitalTable.Columns.Add(colContact);

            // 6. Status Column
            var colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Status";
            colStatus.HeaderText = "Status";
            colStatus.FillWeight = 10;
            colStatus.ReadOnly = true;
            hospitalTable.Columns.Add(colStatus);

            // 7. Edit Button Column (Exactly like UserManagement)
            var btnEdit = new DataGridViewButtonColumn();
            btnEdit.Name = "Edit";
            btnEdit.HeaderText = "Actions";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FillWeight = 8;
            hospitalTable.Columns.Add(btnEdit);

            // 8. Delete Button Column (Exactly like UserManagement)
            var btnDelete = new DataGridViewButtonColumn();
            btnDelete.Name = "Delete";
            btnDelete.HeaderText = "";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FillWeight = 8;
            hospitalTable.Columns.Add(btnDelete);

            // ============================================
            // CELL FORMATTING - Exactly like UserManagement
            // ============================================
            hospitalTable.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                // Status column formatting (like UserManagement)
                if (e.ColumnIndex == hospitalTable.Columns["Status"].Index && e.Value != null)
                {
                    bool active = e.Value.ToString() == "Active";
                    e.CellStyle.ForeColor = active ? Color.FromArgb(6, 95, 70) : Color.FromArgb(153, 27, 27);
                    e.CellStyle.BackColor = active ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // Edit button formatting (like UserManagement)
                if (e.ColumnIndex == hospitalTable.Columns["Edit"].Index)
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(37, 99, 235);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(30, 80, 200);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                // Delete button formatting (like UserManagement)
                if (e.ColumnIndex == hospitalTable.Columns["Delete"].Index)
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(190, 28, 28);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            };

            // ============================================
            // CELL CLICK EVENT - Exactly like UserManagement
            // ============================================
            hospitalTable.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= hospitalTable.Rows.Count) return;

                int hospitalId = Convert.ToInt32(hospitalTable.Rows[e.RowIndex].Cells["HospitalID"].Value);
                string hospitalName = hospitalTable.Rows[e.RowIndex].Cells["HospitalName"].Value?.ToString() ?? "Hospital";

                // Edit button click
                if (e.ColumnIndex == hospitalTable.Columns["Edit"].Index)
                {
                    using (AddEditHospital editForm = new AddEditHospital(hospitalId))
                    {
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadHospitals();
                        }
                    }
                }
                // Delete button click
                else if (e.ColumnIndex == hospitalTable.Columns["Delete"].Index)
                {
                    DialogResult result = MessageBox.Show($"Are you sure you want to delete '{hospitalName}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        bool success = HospitalDAL.DeleteHospital(hospitalId);
                        if (success)
                        {
                            MessageBox.Show("Hospital deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadHospitals();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete hospital. Please try again.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            tablePanel.Controls.Add(hospitalTable);
            tablePanel.Controls.Add(headerLabel);

            mainPanel.Controls.Add(tablePanel);
            mainPanel.Controls.Add(topbarPanel);
            this.Controls.Add(mainPanel);
        }

        // =====================================================
        // SIDEBAR
        // =====================================================
        private void BuildSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 230;
            sidebarPanel.BackColor = Color.FromArgb(5, 31, 64);

            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 72;
            logoPanel.BackColor = Color.Transparent;

            Label bloodIcon = new Label();
            bloodIcon.Text = "🩸";
            bloodIcon.Font = new Font("Segoe UI Emoji", 22, FontStyle.Regular);
            bloodIcon.ForeColor = Color.FromArgb(220, 50, 50);
            bloodIcon.BackColor = Color.Transparent;
            bloodIcon.AutoSize = true;
            bloodIcon.Location = new Point(16, 18);
            bloodIcon.TextAlign = ContentAlignment.MiddleCenter;

            Label logoText = new Label();
            logoText.Text = "Blood Bank";
            logoText.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            logoText.ForeColor = Color.White;
            logoText.BackColor = Color.Transparent;
            logoText.AutoSize = true;
            logoText.Location = new Point(66, 22);
            logoText.TextAlign = ContentAlignment.MiddleLeft;

            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Dock = DockStyle.Fill;
            menuPanel.FlowDirection = FlowDirection.TopDown;
            menuPanel.WrapContents = false;
            menuPanel.AutoScroll = true;
            menuPanel.Padding = new Padding(8, 8, 8, 8);
            menuPanel.BackColor = Color.Transparent;

            // Dashboard
            AddSidebarMenuItem(menuPanel, "🏠", "Dashboard", false, () =>
            {
                AdminDashboard dashboard = new AdminDashboard();
                dashboard.Show();
                this.Close();
            });

            // User Management
            AddSidebarMenuItem(menuPanel, "👥", "User Management", false, () =>
            {
                using (UserManagement userMgmt = new UserManagement())
                {
                    this.Hide();
                    userMgmt.ShowDialog();
                    this.Show();
                }
            });

            // Roles & Permissions
            AddSidebarMenuItem(menuPanel, "🔐", "Roles & Permissions", false, () =>
            {
                using (RolesPermissions rolesForm = new RolesPermissions())
                {
                    this.Hide();
                    rolesForm.ShowDialog();
                    this.Show();
                }
            });

            // Hospitals (Active)
            AddSidebarMenuItem(menuPanel, "🏥", "Hospitals", true, null);

            // Blood Components
            AddSidebarMenuItem(menuPanel, "🩸", "Blood Components", false, () =>
            {
                using (BloodComponents bloodForm = new BloodComponents())
                {
                    this.Hide();
                    bloodForm.ShowDialog();
                    this.Show();
                }
            });

            // Reports
            AddSidebarMenuItem(menuPanel, "📊", "Reports", false, () =>
            {
                using (Reports reportsForm = new Reports())
                {
                    this.Hide();
                    reportsForm.ShowDialog();
                    this.Show();
                }
            });

            // Settings
            AddSidebarMenuItem(menuPanel, "⚙", "Settings", false, () =>
            {
                using (Settings settingsForm = new Settings())
                {
                    this.Hide();
                    settingsForm.ShowDialog();
                    this.Show();
                }
            });

            // Logout
            AddSidebarMenuItem(menuPanel, "🚪", "Logout", false, () =>
            {
                Logout logoutForm = new Logout();
                DialogResult result = logoutForm.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    SessionManager.Logout();
                    Application.Exit();
                }
            });

            sidebarPanel.Controls.Add(menuPanel);
            sidebarPanel.Controls.Add(logoPanel);
            this.Controls.Add(sidebarPanel);
        }

        private void AddSidebarMenuItem(FlowLayoutPanel panel, string icon, string text, bool active, Action clickAction)
        {
            Panel item = new Panel();
            item.Size = new Size(210, 46);
            item.Margin = new Padding(0, 2, 0, 2);
            item.Cursor = Cursors.Hand;
            item.BackColor = active ? Color.FromArgb(230, 57, 70) : Color.Transparent;

            Label iconLbl = new Label();
            iconLbl.Text = icon;
            iconLbl.Font = new Font("Segoe UI Emoji", 14, FontStyle.Regular);
            iconLbl.ForeColor = Color.White;
            iconLbl.BackColor = Color.Transparent;
            iconLbl.AutoSize = false;
            iconLbl.Size = new Size(35, 30);
            iconLbl.Location = new Point(10, 8);
            iconLbl.TextAlign = ContentAlignment.MiddleCenter;

            Label textLbl = new Label();
            textLbl.Text = text;
            textLbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            textLbl.ForeColor = Color.White;
            textLbl.BackColor = Color.Transparent;
            textLbl.AutoSize = false;
            textLbl.Size = new Size(155, 30);
            textLbl.Location = new Point(48, 8);
            textLbl.TextAlign = ContentAlignment.MiddleLeft;

            EventHandler enter = (s, e) => { if (!active) item.BackColor = Color.FromArgb(200, 57, 70); };
            EventHandler leave = (s, e) => { if (!active) item.BackColor = Color.Transparent; };

            if (clickAction != null)
            {
                EventHandler click = (s, e) => clickAction();
                item.Click += click;
                iconLbl.Click += click;
                textLbl.Click += click;
            }

            iconLbl.MouseEnter += enter;
            iconLbl.MouseLeave += leave;
            textLbl.MouseEnter += enter;
            textLbl.MouseLeave += leave;
            item.MouseEnter += enter;
            item.MouseLeave += leave;

            item.Controls.Add(iconLbl);
            item.Controls.Add(textLbl);
            panel.Controls.Add(item);
        }

        private GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }
    }
}