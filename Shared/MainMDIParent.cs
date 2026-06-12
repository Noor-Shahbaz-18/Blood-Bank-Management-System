using System;
using System.Drawing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Shared
{
    public partial class MainMDIParent : Form
    {
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem windowMenu;
        private ToolStripMenuItem helpMenu;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        public MainMDIParent()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Blood Bank Management System";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Menu Strip
            MenuStrip menuStrip = new MenuStrip();

            // File Menu
            fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Login", null, (s, e) => OpenLoginForm());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Application.Exit());
            menuStrip.Items.Add(fileMenu);

            // Window Menu
            windowMenu = new ToolStripMenuItem("Window");
            windowMenu.DropDownItems.Add("Cascade", null, (s, e) => this.LayoutMdi(MdiLayout.Cascade));
            windowMenu.DropDownItems.Add("Tile Horizontal", null, (s, e) => this.LayoutMdi(MdiLayout.TileHorizontal));
            windowMenu.DropDownItems.Add("Tile Vertical", null, (s, e) => this.LayoutMdi(MdiLayout.TileVertical));
            windowMenu.DropDownItems.Add(new ToolStripSeparator());
            windowMenu.DropDownItems.Add("Arrange Icons", null, (s, e) => this.LayoutMdi(MdiLayout.ArrangeIcons));
            menuStrip.Items.Add(windowMenu);

            // Help Menu
            helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, (s, e) => OpenAboutForm());
            menuStrip.Items.Add(helpMenu);

            this.Controls.Add(menuStrip);

            // Status Strip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel($"Ready | User: {Classes.Helpers.SessionManager.CurrentUsername ?? "Not Logged In"} | Date: {DateTime.Now:dd-MMM-yyyy HH:mm}");
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // Timer for status update
            Timer statusTimer = new Timer();
            statusTimer.Interval = 1000;
            statusTimer.Tick += (s, e) =>
            {
                statusLabel.Text = $"Ready | User: {Classes.Helpers.SessionManager.CurrentUsername ?? "Not Logged In"} | Date: {DateTime.Now:dd-MMM-yyyy HH:mm}";
            };
            statusTimer.Start();
        }

        private void OpenLoginForm()
        {
            LoginForm login = new LoginForm();
            login.ShowDialog();
        }

        private void OpenAboutForm()
        {
            AboutForm about = new AboutForm();
            about.ShowDialog();
        }
    }
}