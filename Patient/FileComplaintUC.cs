using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class FileComplaintUC : UserControl
    {
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);

        private ComboBox cmbType;
        private TextBox txtIssue;
        private Label lblDropText;
        private Label lblStatus;
        private Button btnSubmit, btnReset;
        private string selectedFilePath = "";
        private string selectedFileName = "";

        public FileComplaintUC()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ===== SCROLL PANEL =====
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            this.Controls.Add(scrollPanel);

            // ===== WHITE CARD =====
            Panel card = new Panel
            {
                BackColor = Color.White,
                Location = new Point(20, 20),
                Width = 900,
                Height = 700
            };
            scrollPanel.Controls.Add(card);

            // Keep card width in sync with scrollPanel
            scrollPanel.Resize += (s, e) =>
            {
                int w = scrollPanel.ClientSize.Width - 40;
                if (w < 400) w = 400;
                card.Width = w;
            };

            int pad = 40;
            int y = 20;

            // ===== TITLE =====
            Label lblTitle = new Label
            {
                Text = "📝 File a Complaint",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = brickRed,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(pad, y)
            };
            card.Controls.Add(lblTitle);
            card.Resize += (s, e) => lblTitle.Width = card.Width - pad * 2;
            y += 60;

            // ===== COMPLAINT TYPE =====
            card.Controls.Add(MakeLabel("Complaint Type", pad, y));
            y += 28;

            cmbType = new ComboBox
            {
                Location = new Point(pad, y),
                Height = 36,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                FlatStyle = FlatStyle.Flat
            };
            cmbType.Items.AddRange(new string[]
            {
                "Select complaint type",
                "Service Delay",
                "Staff Behavior",
                "Blood Quality Issue",
                "Wrong Blood Group",
                "Hospital Issue",
                "Other"
            });
            cmbType.SelectedIndex = 0;
            card.Controls.Add(cmbType);
            card.Resize += (s, e) => cmbType.Width = card.Width - pad * 2;
            y += 50;

            // ===== DESCRIBE ISSUE =====
            card.Controls.Add(MakeLabel("Describe your issue in detail", pad, y));
            y += 28;

            txtIssue = new TextBox
            {
                Location = new Point(pad, y),
                Height = 130,
                Multiline = true,
                Font = new Font("Segoe UI", 11),
                Text = "Write your complaint in detail...",
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };
            card.Controls.Add(txtIssue);
            card.Resize += (s, e) => txtIssue.Width = card.Width - pad * 2;

            txtIssue.GotFocus += (s, e) =>
            {
                if (txtIssue.Text == "Write your complaint in detail...")
                { txtIssue.Text = ""; txtIssue.ForeColor = Color.Black; }
            };
            txtIssue.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtIssue.Text))
                { txtIssue.Text = "Write your complaint in detail..."; txtIssue.ForeColor = Color.Gray; }
            };
            y += 145;

            // ===== UPLOAD SECTION =====
            card.Controls.Add(MakeLabel("Upload Supporting File (Optional)", pad, y));
            y += 28;

            Panel uploadBox = new Panel
            {
                Location = new Point(pad, y),
                Height = 75,
                BackColor = Color.FromArgb(249, 250, 251),
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true,
                Cursor = Cursors.Hand
            };
            card.Controls.Add(uploadBox);
            card.Resize += (s, e) => uploadBox.Width = card.Width - pad * 2;

            lblDropText = new Label
            {
                Text = "📎 Drop files here or click to upload  (Images, PDF, Documents)",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            uploadBox.Controls.Add(lblDropText);

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "All Files|*.*|Images|*.jpg;*.png;*.jpeg|PDF Files|*.pdf|Documents|*.docx;*.txt"
            };

            void HandleFile(string path)
            {
                selectedFilePath = path;
                selectedFileName = Path.GetFileName(path);
                lblDropText.Text = $"✅ {selectedFileName}  (Click to change)";
                lblDropText.ForeColor = Color.FromArgb(21, 128, 61);
            }

            uploadBox.Click += (s, e) => { if (ofd.ShowDialog() == DialogResult.OK) HandleFile(ofd.FileName); };
            lblDropText.Click += (s, e) => { if (ofd.ShowDialog() == DialogResult.OK) HandleFile(ofd.FileName); };
            uploadBox.DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            uploadBox.DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0) HandleFile(files[0]);
            };
            y += 90;

            // ===== STATUS LABEL =====
            lblStatus = new Label
            {
                Text = "",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(pad, y),
                Height = 22,
                AutoSize = false
            };
            card.Controls.Add(lblStatus);
            card.Resize += (s, e) => lblStatus.Width = card.Width - pad * 2;
            y += 30;

            // ===== BUTTONS =====
            int btnY = y;

            // Reset button — LEFT SIDE
            btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(120, 44),
                Location = new Point(pad, btnY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI Semibold", 11),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnReset.FlatAppearance.BorderSize = 1;
            btnReset.Click += (s, e) => ResetForm();
            card.Controls.Add(btnReset);

            // Submit button — RIGHT SIDE
            btnSubmit = new Button
            {
                Text = "Submit Complaint",
                Size = new Size(165, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = brickRed,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;
            card.Controls.Add(btnSubmit);

            // Position submit on right, reset stays on left
            card.Resize += (s, e) =>
            {
                btnReset.Location = new Point(pad, btnY);
                btnSubmit.Location = new Point(card.Width - pad - 165, btnY);
            };

            card.Height = btnY + 70;
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (cmbType.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a complaint type.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbType.Focus(); return;
            }

            if (string.IsNullOrWhiteSpace(txtIssue.Text) || txtIssue.Text == "Write your complaint in detail...")
            {
                MessageBox.Show("Please describe your issue in detail.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIssue.Focus(); return;
            }

            try
            {
                string savedFilePath = "";
                string savedFileName = "";

                if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
                {
                    try
                    {
                        string folder = Path.Combine(Application.StartupPath, "Complaints");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                        savedFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(selectedFilePath)}";
                        savedFilePath = Path.Combine(folder, savedFileName);
                        File.Copy(selectedFilePath, savedFilePath, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"File copy error: {ex.Message}");
                    }
                }

                Complaint complaint = new Complaint
                {
                    UserID = SessionManager.CurrentUserID,
                    UserName = SessionManager.CurrentFullName,
                    ComplaintType = cmbType.SelectedItem.ToString(),
                    IssueDescription = txtIssue.Text.Trim(),
                    FilePath = savedFilePath,
                    FileName = savedFileName,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                bool saved = ComplaintDAL.Insert(complaint);

                if (saved)
                {
                    lblStatus.Text = "✅ Complaint submitted. We will review it shortly.";
                    lblStatus.ForeColor = Color.FromArgb(21, 128, 61);

                    MessageBox.Show(
                        $"✅ Complaint Submitted!\n\nType: {cmbType.SelectedItem}\n\nWe will respond within 2-3 business days.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ResetForm();

                    Timer t = new Timer { Interval = 5000 };
                    t.Tick += (s, ev) => { lblStatus.Text = ""; t.Stop(); };
                    t.Start();
                }
                else
                {
                    lblStatus.Text = "❌ Failed to submit. Please try again.";
                    lblStatus.ForeColor = Color.FromArgb(185, 28, 28);
                    MessageBox.Show("Failed to submit complaint.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(185, 28, 28);
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            cmbType.SelectedIndex = 0;
            txtIssue.Text = "Write your complaint in detail...";
            txtIssue.ForeColor = Color.Gray;
            selectedFilePath = "";
            selectedFileName = "";
            lblDropText.Text = "📎 Drop files here or click to upload  (Images, PDF, Documents)";
            lblDropText.ForeColor = Color.Gray;
            lblStatus.Text = "";
        }
    }
}