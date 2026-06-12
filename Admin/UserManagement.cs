using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public class UserManagement : Form
    {
        private Panel sidebarPanel;
        private Panel mainPanel;
        private DataGridView userTable;
        private TextBox searchInput;

        public UserManagement()
        {
            this.Text = "User Management - Blood Bank";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(244, 247, 252);
            this.MinimumSize = new Size(1100, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildMainContent();
            BuildSidebar();
            LoadUsers();  // Load users from database
        }

        // =====================================================
        // LOAD USERS FROM DATABASE
        // =====================================================
        private void LoadUsers()
        {
            try
            {
                DataTable dt = AdminDAL.GetAllUsers();
                userTable.Rows.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    userTable.Rows.Add(
                        row["UserID"],                                    // UserID (hidden)
                        row["FullName"].ToString(),                       // Name
                        row["Username"].ToString(),                       // Username
                        row["Email"].ToString(),                          // Email
                        row["Role"].ToString(),                           // Role
                        row["IsActive"].ToString() == "True" ? "Active" : "Inactive",  // Status
                        "Edit",                                           // Edit button
                        "Delete"                                          // Delete button
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================
        // FILTER TABLE (SEARCH)
        // =====================================================
        private void FilterTable(string query)
        {
            if (userTable == null) return;
            string q = query.ToLower().Trim();
            bool isEmpty = string.IsNullOrEmpty(q) || q == "search user...";

            foreach (DataGridViewRow row in userTable.Rows)
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
            titleLabel.Text = "User Management";
            titleLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(20, 20, 30);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(0, 14);

            // Search box
            Panel searchBox = new Panel();
            searchBox.Size = new Size(240, 38);
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
            searchIcon.Location = new Point(8, 8);

            searchInput = new TextBox();
            searchInput.BorderStyle = BorderStyle.None;
            searchInput.Font = new Font("Segoe UI", 10);
            searchInput.Size = new Size(185, 22);
            searchInput.Location = new Point(36, 10);
            searchInput.Text = "Search user...";
            searchInput.ForeColor = Color.Gray;
            searchInput.Enter += (s, e) => { if (searchInput.Text == "Search user...") { searchInput.Text = ""; searchInput.ForeColor = Color.Black; } };
            searchInput.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(searchInput.Text)) { searchInput.Text = "Search user..."; searchInput.ForeColor = Color.Gray; } };
            searchInput.TextChanged += (s, e) => FilterTable(searchInput.Text);

            searchBox.Controls.Add(searchIcon);
            searchBox.Controls.Add(searchInput);

            // Add User Button
            Panel addButton = new Panel();
            addButton.Size = new Size(130, 38);
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
            addLabel.Text = "+ Add User";
            addLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            addLabel.ForeColor = Color.White;
            addLabel.BackColor = Color.Transparent;
            addLabel.AutoSize = false;
            addLabel.Size = new Size(130, 38);
            addLabel.Location = new Point(0, 0);
            addLabel.TextAlign = ContentAlignment.MiddleCenter;
            addLabel.Cursor = Cursors.Hand;

            // FIXED: Add User Click Event
            EventHandler addClick = (s, e) =>
            {
                using (AddEditUser addUserForm = new AddEditUser())  // 0 = new user
                {
                    if (addUserForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadUsers();  // Refresh grid
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
            headerLabel.Text = "All Users";
            headerLabel.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            headerLabel.ForeColor = Color.FromArgb(20, 20, 30);
            headerLabel.BackColor = Color.Transparent;
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Height = 36;
            headerLabel.TextAlign = ContentAlignment.MiddleLeft;
            headerLabel.Padding = new Padding(4, 0, 0, 0);

            // ----- DATAGRIDVIEW -----
            userTable = new DataGridView();
            userTable.Dock = DockStyle.Fill;
            userTable.BackgroundColor = Color.White;
            userTable.BorderStyle = BorderStyle.None;
            userTable.RowHeadersVisible = false;
            userTable.AllowUserToAddRows = false;
            userTable.AllowUserToDeleteRows = false;
            userTable.AllowUserToResizeRows = false;
            userTable.AllowUserToResizeColumns = false;
            userTable.ReadOnly = true;
            userTable.EditMode = DataGridViewEditMode.EditProgrammatically;
            userTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            userTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            userTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            userTable.ColumnHeadersHeight = 42;
            userTable.RowTemplate.Height = 42;
            userTable.GridColor = Color.FromArgb(235, 235, 240);
            userTable.EnableHeadersVisualStyles = false;
            userTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            userTable.DefaultCellStyle = new DataGridViewCellStyle();
            userTable.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            userTable.DefaultCellStyle.BackColor = Color.White;
            userTable.DefaultCellStyle.ForeColor = Color.FromArgb(30, 30, 40);
            userTable.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 244, 255);
            userTable.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 30, 40);
            userTable.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            userTable.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle();
            userTable.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            userTable.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
            userTable.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 70);
            userTable.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            // ============================================
            // COLUMNS - UserID hidden column sab se pehle
            // ============================================

            // 1. UserID Column (HIDDEN - Edit/Delete ke liye)
            var colUserId = new DataGridViewTextBoxColumn();
            colUserId.Name = "UserID";
            colUserId.HeaderText = "ID";
            colUserId.Visible = false;  // Hidden
            colUserId.ReadOnly = true;

            // 2. Name Column
            var colName = new DataGridViewTextBoxColumn();
            colName.Name = "Name";
            colName.HeaderText = "Name";
            colName.FillWeight = 20;
            colName.ReadOnly = true;

            // 3. Username Column
            var colUsername = new DataGridViewTextBoxColumn();
            colUsername.Name = "Username";
            colUsername.HeaderText = "Username";
            colUsername.FillWeight = 15;
            colUsername.ReadOnly = true;

            // 4. Email Column
            var colEmail = new DataGridViewTextBoxColumn();
            colEmail.Name = "Email";
            colEmail.HeaderText = "Email";
            colEmail.FillWeight = 25;
            colEmail.ReadOnly = true;

            // 5. Role Column
            var colRole = new DataGridViewTextBoxColumn();
            colRole.Name = "Role";
            colRole.HeaderText = "Role";
            colRole.FillWeight = 15;
            colRole.ReadOnly = true;

            // 6. Status Column
            var colStatus = new DataGridViewTextBoxColumn();
            colStatus.Name = "Status";
            colStatus.HeaderText = "Status";
            colStatus.FillWeight = 10;
            colStatus.ReadOnly = true;

            // 7. Edit Button Column
            var btnEdit = new DataGridViewButtonColumn();
            btnEdit.Name = "Edit";
            btnEdit.HeaderText = "Actions";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FillWeight = 8;

            // 8. Delete Button Column
            var btnDelete = new DataGridViewButtonColumn();
            btnDelete.Name = "Delete";
            btnDelete.HeaderText = "";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FillWeight = 8;

            // Add columns to DataGridView (UserID sab se pehle)
            userTable.Columns.Add(colUserId);
            userTable.Columns.Add(colName);
            userTable.Columns.Add(colUsername);
            userTable.Columns.Add(colEmail);
            userTable.Columns.Add(colRole);
            userTable.Columns.Add(colStatus);
            userTable.Columns.Add(btnEdit);
            userTable.Columns.Add(btnDelete);

            // ============================================
            // CELL FORMATTING - Status ke liye
            // ============================================
            userTable.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                if (e.ColumnIndex == userTable.Columns["Status"].Index && e.Value != null)
                {
                    bool act = e.Value.ToString() == "Active";
                    e.CellStyle.ForeColor = act ? Color.FromArgb(6, 95, 70) : Color.FromArgb(153, 27, 27);
                    e.CellStyle.BackColor = act ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (e.ColumnIndex == userTable.Columns["Edit"].Index)
                {
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.BackColor = Color.FromArgb(37, 99, 235);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(30, 80, 200);
                    e.CellStyle.SelectionForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }

                if (e.ColumnIndex == userTable.Columns["Delete"].Index)
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
            // CELL CLICK EVENT - Edit aur Delete
            // ============================================
            userTable.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= userTable.Rows.Count) return;

                // Get UserID from hidden column
                int userId = Convert.ToInt32(userTable.Rows[e.RowIndex].Cells["UserID"].Value);
                string userName = userTable.Rows[e.RowIndex].Cells["Name"].Value?.ToString() ?? "User";

                // Edit button click
                if (e.ColumnIndex == userTable.Columns["Edit"].Index)
                {
                    using (AddEditUser editForm = new AddEditUser(userId))
                    {
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            LoadUsers();  // Refresh grid after edit
                        }
                    }
                }
                // Delete button click
                else if (e.ColumnIndex == userTable.Columns["Delete"].Index)
                {
                    DialogResult result = MessageBox.Show($"Are you sure you want to delete '{userName}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        bool success = AdminDAL.DeleteUser(userId);
                        if (success)
                        {
                            MessageBox.Show("User deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUsers();  // Refresh grid after delete
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete user. Please try again.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            tablePanel.Controls.Add(userTable);
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
            logoText.Location = new Point(56, 22);
            logoText.TextAlign = ContentAlignment.MiddleLeft;

            logoPanel.Controls.Add(bloodIcon);
            logoPanel.Controls.Add(logoText);
            bloodIcon.BringToFront();
            logoText.BringToFront();

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

            // User Management (Active)
            AddSidebarMenuItem(menuPanel, "👥", "User Management", true, null);

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

            // Hospitals
            AddSidebarMenuItem(menuPanel, "🏥", "Hospitals", false, () =>
            {
                using (HospitalManagement hospitalForm = new HospitalManagement())
                {
                    this.Hide();
                    hospitalForm.ShowDialog();
                    this.Show();
                }
            });

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

        // =====================================================
        // ADD SIDEBAR MENU ITEM
        // =====================================================
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

        // =====================================================
        // HELPER - Rounded Rectangle
        // =====================================================
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