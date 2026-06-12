using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class FeedbackUC : UserControl
    {
        private Button[] stars = new Button[5];
        private int selectedRating = 0;
        private TextBox txtFeedback;
        private Button btnSubmit, btnReset;
        private Label lblStatus;

        public FeedbackUC()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.AutoScroll = false;

            // Jab bhi UserControl resize ho, sab controls reposition karo
            this.Resize += (s, e) => LayoutControls();

            // =========================================================
            // MAIN CARD
            // =========================================================
            Panel formCard = new Panel();
            formCard.Name = "formCard";
            formCard.BackColor = Color.White;
            formCard.Location = new Point(0, 0);
            formCard.Size = new Size(100, 100); // Will be set in LayoutControls
            MakeRounded(formCard, 15);
            this.Controls.Add(formCard);

            // =========================================================
            // TITLE
            // =========================================================
            Label lblTitle = new Label();
            lblTitle.Name = "lblTitle";
            lblTitle.Text = "⭐  Share Your Feedback";
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(120, 22, 27);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            formCard.Controls.Add(lblTitle);

            // =========================================================
            // RATING QUESTION
            // =========================================================
            Label lblRating = new Label();
            lblRating.Name = "lblRating";
            lblRating.Text = "How was your experience with our service?";
            lblRating.Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold);
            lblRating.ForeColor = Color.FromArgb(31, 41, 55);
            lblRating.TextAlign = ContentAlignment.MiddleLeft;
            formCard.Controls.Add(lblRating);

            // =========================================================
            // STARS
            // =========================================================
            for (int i = 0; i < 5; i++)
            {
                stars[i] = new Button();
                stars[i].Name = "star" + i;
                stars[i].Text = "☆";
                stars[i].Font = new Font("Segoe UI", 26);
                stars[i].Size = new Size(50, 50);
                stars[i].FlatStyle = FlatStyle.Flat;
                stars[i].BackColor = Color.Transparent;
                stars[i].ForeColor = Color.Gray;
                stars[i].Cursor = Cursors.Hand;
                stars[i].Tag = i + 1;
                stars[i].FlatAppearance.BorderSize = 0;
                stars[i].Click += Star_Click;
                formCard.Controls.Add(stars[i]);
            }

            // =========================================================
            // FEEDBACK LABEL
            // =========================================================
            Label lblFeedback = new Label();
            lblFeedback.Name = "lblFeedback";
            lblFeedback.Text = "Write your feedback";
            lblFeedback.Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold);
            lblFeedback.ForeColor = Color.FromArgb(55, 65, 81);
            lblFeedback.TextAlign = ContentAlignment.MiddleLeft;
            formCard.Controls.Add(lblFeedback);

            // =========================================================
            // TEXTBOX
            // =========================================================
            txtFeedback = new TextBox();
            txtFeedback.Name = "txtFeedback";
            txtFeedback.Multiline = true;
            txtFeedback.Font = new Font("Segoe UI", 11);
            txtFeedback.Text = "Write your feedback here...";
            txtFeedback.ForeColor = Color.Gray;
            txtFeedback.BorderStyle = BorderStyle.FixedSingle;
            txtFeedback.ScrollBars = ScrollBars.Vertical;
            formCard.Controls.Add(txtFeedback);

            txtFeedback.GotFocus += (s, e) =>
            {
                if (txtFeedback.Text == "Write your feedback here...")
                {
                    txtFeedback.Text = "";
                    txtFeedback.ForeColor = Color.Black;
                }
            };
            txtFeedback.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFeedback.Text))
                {
                    txtFeedback.Text = "Write your feedback here...";
                    txtFeedback.ForeColor = Color.Gray;
                }
            };

            // =========================================================
            // STATUS LABEL
            // =========================================================
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "";
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            formCard.Controls.Add(lblStatus);

            // =========================================================
            // RESET BUTTON
            // =========================================================
            btnReset = new Button();
            btnReset.Name = "btnReset";
            btnReset.Text = "Reset";
            btnReset.Size = new Size(120, 44);
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.BackColor = Color.FromArgb(243, 244, 246);
            btnReset.ForeColor = Color.FromArgb(55, 65, 81);
            btnReset.Font = new Font("Segoe UI Semibold", 11);
            btnReset.FlatAppearance.BorderSize = 1;
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnReset.Cursor = Cursors.Hand;
            btnReset.Click += (s, e) => ResetForm();
            formCard.Controls.Add(btnReset);

            // =========================================================
            // SUBMIT BUTTON
            // =========================================================
            btnSubmit = new Button();
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Text = "Submit Feedback";
            btnSubmit.Size = new Size(160, 44);
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.BackColor = Color.FromArgb(120, 22, 27);
            btnSubmit.ForeColor = Color.White;
            btnSubmit.Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Cursor = Cursors.Hand;
            btnSubmit.Click += BtnSubmit_Click;
            formCard.Controls.Add(btnSubmit);

            // First layout
            LayoutControls();
        }

        private void LayoutControls()
        {
            int margin = 25;
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            if (W < 10 || H < 10) return;

            // Card fills entire UserControl with small margin
            Panel formCard = (Panel)this.Controls["formCard"];
            formCard.Location = new Point(margin, margin);
            formCard.Size = new Size(W - margin * 2, H - margin * 2);

            int cw = formCard.Width;   // card width
            int ch = formCard.Height;  // card height
            int pad = 30;              // inner padding

            int innerW = cw - pad * 2; // usable width inside card

            // ---- Row positions (top-down) ----
            int y = 15;

            // TITLE
            Label lblTitle = (Label)formCard.Controls["lblTitle"];
            lblTitle.Location = new Point(pad, y);
            lblTitle.Size = new Size(innerW, 50);
            y += 55;

            // RATING QUESTION
            Label lblRating = (Label)formCard.Controls["lblRating"];
            lblRating.Location = new Point(pad, y);
            lblRating.Size = new Size(innerW, 35);
            y += 38;

            // STARS
            for (int i = 0; i < 5; i++)
            {
                stars[i].Location = new Point(pad + i * 56, y);
            }
            y += 58;

            // FEEDBACK LABEL
            Label lblFeedback = (Label)formCard.Controls["lblFeedback"];
            lblFeedback.Location = new Point(pad, y);
            lblFeedback.Size = new Size(innerW, 30);
            y += 33;

            // Calculate remaining space for textbox
            int bottomReserved = 10 + 25 + 8 + 44 + 15; // status + gap + btn
            int txtHeight = ch - y - bottomReserved;
            if (txtHeight < 60) txtHeight = 60;

            // TEXTBOX
            txtFeedback.Location = new Point(pad, y);
            txtFeedback.Size = new Size(innerW, txtHeight);
            y += txtHeight + 8;

            // STATUS LABEL
            lblStatus.Location = new Point(pad, y);
            lblStatus.Size = new Size(innerW, 25);
            y += 30;

            // BUTTONS
            btnReset.Location = new Point(pad, y);
            btnSubmit.Location = new Point(cw - pad - btnSubmit.Width, y);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (selectedRating == 0)
            {
                MessageBox.Show("Please select a rating (1-5 stars).", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFeedback.Text) ||
                txtFeedback.Text == "Write your feedback here...")
            {
                MessageBox.Show("Please enter your feedback message.", "Validation Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFeedback.Focus();
                return;
            }

            try
            {
                Feedback feedback = new Feedback
                {
                    UserID = SessionManager.CurrentUserID,
                    UserName = SessionManager.CurrentFullName,
                    Rating = selectedRating,
                    Comment = txtFeedback.Text.Trim(),
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                bool saved = FeedbackDAL.Insert(feedback);

                if (saved)
                {
                    lblStatus.Text = "✅ Feedback submitted successfully!";
                    lblStatus.ForeColor = Color.FromArgb(34, 197, 94);

                    MessageBox.Show(
                        $"✅ Feedback Submitted Successfully!\n\nRating: {selectedRating} star(s)\nMessage: {txtFeedback.Text.Trim()}\n\nThank you for your valuable feedback!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ResetForm();

                    Timer timer = new Timer();
                    timer.Interval = 5000;
                    timer.Tick += (s, ev) => { lblStatus.Text = ""; timer.Stop(); };
                    timer.Start();
                }
                else
                {
                    lblStatus.Text = "❌ Failed to submit. Please try again.";
                    lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                    MessageBox.Show("Failed to submit feedback. Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Error: " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            selectedRating = 0;
            for (int i = 0; i < 5; i++)
            {
                stars[i].Text = "☆";
                stars[i].ForeColor = Color.Gray;
            }
            txtFeedback.Text = "Write your feedback here...";
            txtFeedback.ForeColor = Color.Gray;
            lblStatus.Text = "";
        }

        private void Star_Click(object sender, EventArgs e)
        {
            selectedRating = (int)((Button)sender).Tag;
            for (int i = 0; i < 5; i++)
            {
                stars[i].Text = i < selectedRating ? "★" : "☆";
                stars[i].ForeColor = i < selectedRating ? Color.Orange : Color.Gray;
            }
        }

        private void MakeRounded(Panel panel, int radius)
        {
            panel.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                int d = radius * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(panel.Width - d, 0, d, d, 270, 90);
                path.AddArc(panel.Width - d, panel.Height - d, d, d, 0, 90);
                path.AddArc(0, panel.Height - d, d, d, 90, 90);
                panel.Region = new Region(path);
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawPath(pen, path);
            };
        }
    }
}