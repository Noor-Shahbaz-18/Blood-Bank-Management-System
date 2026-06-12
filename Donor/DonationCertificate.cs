using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Forms.Donor
{
    public partial class DonationCertificate : Form
    {
        private string _donorName;
        private string _donationDate;
        private string _bloodGroup;
        private string _donationNumber;

        private readonly Color darkRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBg = Color.FromArgb(252, 248, 248);

        public DonationCertificate(string donorName, string donationDate, string bloodGroup, string donationNumber)
        {
            _donorName = donorName;
            _donationDate = donationDate;
            _bloodGroup = bloodGroup;
            _donationNumber = donationNumber;
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Donation Certificate";
            this.Size = new Size(820, 700);
            this.MinimumSize = new Size(820, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // ── Outer padding panel ──
            Panel outerPad = new Panel();
            outerPad.Dock = DockStyle.Fill;
            outerPad.Padding = new Padding(30, 25, 30, 25);
            this.Controls.Add(outerPad);

            // ── Certificate card ──
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                // Outer border
                using (Pen p = new Pen(darkRed, 3))
                    g.DrawRectangle(p, 8, 8, card.Width - 17, card.Height - 17);
                // Inner border
                using (Pen p = new Pen(darkRed, 1))
                    g.DrawRectangle(p, 14, 14, card.Width - 29, card.Height - 29);
            };
            outerPad.Controls.Add(card);

            // ── TableLayoutPanel inside the card ──
            TableLayoutPanel tbl = new TableLayoutPanel();
            tbl.Dock = DockStyle.Fill;
            tbl.ColumnCount = 1;
            tbl.RowCount = 10;
            tbl.BackColor = Color.Transparent;
            tbl.Padding = new Padding(40, 30, 40, 20);

            // Row heights (px unless Percent)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));   // 0: blood drop icon
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // 1: title
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));   // 2: divider line
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // 3: "presented to"
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // 4: donor name
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));   // 5: "for their generous…"
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));   // 6: spacer
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // 7: details box
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // 8: thank you
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 9: footer + buttons (fill remaining)

            card.Controls.Add(tbl);

            // ── ROW 0: Blood drop icon ──
            Label lblIcon = MakeLabel("🩸", new Font("Segoe UI Emoji", 40), darkRed);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblIcon.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblIcon, 0, 0);

            // ── ROW 1: Title ──
            Label lblTitle = MakeLabel("CERTIFICATE OF APPRECIATION",
                new Font("Georgia", 20, FontStyle.Bold), darkRed);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblTitle, 0, 1);

            // ── ROW 2: Decorative divider ──
            Panel divider = new Panel();
            divider.Dock = DockStyle.None;
            divider.Anchor = AnchorStyles.None;
            divider.Size = new Size(300, 2);
            divider.BackColor = darkRed;
            // Wrap in a fill panel to centre it
            Panel divWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            divWrap.Controls.Add(divider);
            divWrap.Resize += (s, e) =>
                divider.Location = new Point((divWrap.Width - divider.Width) / 2,
                                             (divWrap.Height - divider.Height) / 2);
            tbl.Controls.Add(divWrap, 0, 2);

            // ── ROW 3: Presented to ──
            Label lblPresented = MakeLabel("This certificate is proudly presented to",
                new Font("Segoe UI", 12), Color.Gray);
            lblPresented.TextAlign = ContentAlignment.MiddleCenter;
            lblPresented.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblPresented, 0, 3);

            // ── ROW 4: Donor name ──
            Label lblName = MakeLabel(_donorName,
                new Font("Georgia", 26, FontStyle.Bold | FontStyle.Italic),
                Color.FromArgb(34, 34, 34));
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            lblName.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblName, 0, 4);

            // ── ROW 5: "for their generous…" ──
            Label lblFor = MakeLabel("for their generous blood donation",
                new Font("Segoe UI", 12), Color.Gray);
            lblFor.TextAlign = ContentAlignment.MiddleCenter;
            lblFor.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblFor, 0, 5);

            // ── ROW 6: spacer ──
            tbl.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent }, 0, 6);

            // ── ROW 7: Details box ──
            Panel detailsBox = new Panel();
            detailsBox.Anchor = AnchorStyles.None;
            detailsBox.Size = new Size(420, 105);
            detailsBox.BackColor = lightBg;
            detailsBox.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(200, 180, 180), 1))
                    e.Graphics.DrawRectangle(p, 0, 0, detailsBox.Width - 1, detailsBox.Height - 1);
            };

            int iy = 12;
            AddDetailLine(detailsBox, "Donation Date:", _donationDate, ref iy);
            AddDetailLine(detailsBox, "Blood Group:", _bloodGroup, ref iy);
            AddDetailLine(detailsBox, "Donation Number:", $"#{_donationNumber}", ref iy);

            Panel detailsWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            detailsWrap.Controls.Add(detailsBox);
            detailsWrap.Resize += (s, e) =>
                detailsBox.Location = new Point(
                    (detailsWrap.Width - detailsBox.Width) / 2,
                    (detailsWrap.Height - detailsBox.Height) / 2);
            tbl.Controls.Add(detailsWrap, 0, 7);

            // ── ROW 8: Thank you ──
            Label lblThanks = MakeLabel("— Thank you for saving lives! —",
                new Font("Georgia", 14, FontStyle.Italic), darkRed);
            lblThanks.TextAlign = ContentAlignment.MiddleCenter;
            lblThanks.Dock = DockStyle.Fill;
            tbl.Controls.Add(lblThanks, 0, 8);

            // ── ROW 9: Footer + Buttons ──
            Panel footerRow = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tbl.Controls.Add(footerRow, 0, 9);

            // Footer text (bottom-left)
            Label lblFooter = new Label();
            lblFooter.Text = $"Issued on: {DateTime.Now:dd MMM yyyy}     Blood Bank Management System";
            lblFooter.Font = new Font("Segoe UI", 9);
            lblFooter.ForeColor = Color.Gray;
            lblFooter.AutoSize = true;
            lblFooter.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblFooter.Location = new Point(0, 30);
            footerRow.Controls.Add(lblFooter);

            // Buttons (bottom-right)
            Button btnPrint = MakeButton("🖨️  Print");
            btnPrint.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPrint.Size = new Size(140, 40);
            btnPrint.Location = new Point(footerRow.Width - 300, 20);
            btnPrint.Click += (s, e) => PrintCertificate();
            footerRow.Controls.Add(btnPrint);

            Button btnSave = MakeButton("💾  Save Image");
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Size = new Size(150, 40);
            btnSave.Location = new Point(footerRow.Width - 155, 20);
            btnSave.Click += (s, e) => DownloadCertificate();
            footerRow.Controls.Add(btnSave);

            // Re-position buttons when footer row resizes
            footerRow.Resize += (s, e) =>
            {
                btnPrint.Location = new Point(footerRow.Width - 305, (footerRow.Height - 40) / 2);
                btnSave.Location = new Point(footerRow.Width - 158, (footerRow.Height - 40) / 2);
            };
        }

        // =====================================================
        // HELPERS
        // =====================================================
        private Label MakeLabel(string text, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = false
            };
        }

        private Button MakeButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = Color.FromArgb(120, 22, 27);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI Semibold", 10);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void AddDetailLine(Panel parent, string label, string value, ref int y)
        {
            Label lblLabel = new Label
            {
                Text = label,
                Location = new Point(25, y),
                Size = new Size(145, 26),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblLabel);

            Label lblValue = new Label
            {
                Text = value,
                Location = new Point(175, y),
                Size = new Size(220, 26),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(34, 34, 34),
                TextAlign = ContentAlignment.MiddleLeft
            };
            parent.Controls.Add(lblValue);
            y += 30;
        }

        // =====================================================
        // PRINT / SAVE
        // =====================================================
        private void PrintCertificate()
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, e) =>
            {
                Bitmap bmp = new Bitmap(this.Width, this.Height);
                this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
                e.Graphics.DrawImage(bmp, 0, 0);
            };

            PrintDialog dlg = new PrintDialog { Document = pd };
            if (dlg.ShowDialog() == DialogResult.OK)
                pd.Print();
        }

        private void DownloadCertificate()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                DefaultExt = "png",
                FileName = $"Donation_Certificate_{_donorName}_{DateTime.Now:yyyyMMdd}"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(this.Width, this.Height);
                this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
                bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show(
                    $"Certificate saved successfully!\n\nLocation: {sfd.FileName}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}