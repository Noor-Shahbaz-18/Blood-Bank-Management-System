using System;
using System.Drawing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Admin
{
    public partial class ViewComplaintDetail : Form
    {
        private int _complaintId;
        private string _complaintData;

        public ViewComplaintDetail(int complaintId = 0, string complaintData = "")
        {
            _complaintId = complaintId;
            _complaintData = complaintData;
            InitializeComponent();
            BuildUI();
            LoadComplaintData();
        }

        private void BuildUI()
        {
            this.Text = "Complaint Details";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25), AutoScroll = true };
            this.Controls.Add(mainPanel);

            Label lblTitle = new Label
            {
                Text = $"📋 Complaint Details #{_complaintId}",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblTitle);

            int y = 70, leftX = 20, labelWidth = 140;

            // Complaint Info Panel
            Panel infoPanel = new Panel { Location = new Point(leftX, y), Width = mainPanel.Width - 50, Height = 300, BackColor = Color.FromArgb(248, 249, 252) };
            mainPanel.Controls.Add(infoPanel);

            int innerY = 20;

            AddDetailRow(infoPanel, "Complaint ID:", _complaintId.ToString(), ref innerY, leftX);
            AddDetailRow(infoPanel, "Submitted By:", "Ali Raza", ref innerY, leftX);
            AddDetailRow(infoPanel, "Complaint Type:", "Staff Behavior", ref innerY, leftX);
            AddDetailRow(infoPanel, "Subject:", "Rude Staff Behavior", ref innerY, leftX);
            AddDetailRow(infoPanel, "Date Submitted:", DateTime.Now.AddDays(-2).ToString("dd-MMM-yyyy HH:mm"), ref innerY, leftX);
            AddDetailRow(infoPanel, "Status:", "Pending", ref innerY, leftX);

            innerY += 10;
            infoPanel.Controls.Add(new Label { Text = "Description:", Location = new Point(leftX, innerY), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            TextBox txtDescription = new TextBox
            {
                Location = new Point(leftX + 120, innerY),
                Width = 500,
                Height = 80,
                Multiline = true,
                ReadOnly = true,
                Text = "Staff member was rude and unhelpful during my visit. I requested information about blood donation but was treated poorly.",
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            infoPanel.Controls.Add(txtDescription);
            innerY += 100;

            // Resolution Panel
            Label lblResolution = new Label
            {
                Text = "Resolution / Response",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(leftX, y + 320),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblResolution);

            TextBox txtResolution = new TextBox
            {
                Location = new Point(leftX, y + 350),
                Width = 600,
                Height = 100,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Controls.Add(txtResolution);

            Button btnUpdate = new Button
            {
                Text = "✅ Update Resolution",
                Location = new Point(leftX + 200, y + 470),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += (s, e) =>
            {
                MessageBox.Show("Complaint resolution updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            mainPanel.Controls.Add(btnUpdate);

            Button btnClose = new Button
            {
                Text = "❌ Close",
                Location = new Point(leftX + 400, y + 470),
                Size = new Size(100, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnClose);
        }

        private void AddDetailRow(Panel panel, string label, string value, ref int y, int leftX)
        {
            panel.Controls.Add(new Label { Text = label, Location = new Point(leftX, y), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            panel.Controls.Add(new Label { Text = value, Location = new Point(leftX + 120, y), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 90) });
            y += 30;
        }

        private void LoadComplaintData()
        {
            // Load data based on _complaintId
        }
    }
}