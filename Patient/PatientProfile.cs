using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;
using PatientModel = BloodBankManagementSystem.Classes.Models.Patient;

namespace BloodBankManagementSystem.Forms.Patient
{
    public partial class PatientProfile : Form
    {
        private PatientModel currentPatient;
        private int currentUserId;
        private PictureBox picProfile;
        private byte[] profilePictureBytes;

        // Form controls
        private TextBox txtFullName, txtEmail, txtPhone, txtAddress, txtBloodGroup, txtCNIC, txtAge;
        private ComboBox cmbGender;
        private DateTimePicker dtpDateOfBirth;
        private Label lblStatus;
        private Button btnSave, btnChangePicture, btnClose, btnEdit;

        public PatientProfile()
        {
            InitializeComponent();
            this.Size = new Size(750, 700);
            this.MinimumSize = new Size(750, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "My Profile - Blood Bank";

            LoadPatientData();
            BuildUI();
        }

        private void LoadPatientData()
        {
            try
            {
                currentUserId = SessionManager.CurrentUserID;
                currentPatient = PatientDAL.GetByUserID(currentUserId);

                if (currentPatient == null)
                {
                    var user = AdminDAL.GetUserByID(currentUserId);
                    if (user != null)
                    {
                        currentPatient = new PatientModel
                        {
                            UserID = currentUserId,
                            FullName = user.FullName,
                            Email = user.Email,
                            Phone = user.Phone,
                            BloodGroup = "Not Set",
                            RegistrationDate = DateTime.Now,
                            IsActive = true,
                            DateOfBirth = new DateTime(1990, 1, 1),
                            CNIC = "00000-0000000-0",
                            Age = DateTime.Now.Year - 1990
                        };

                        bool inserted = PatientDAL.Insert(currentPatient);
                        if (inserted)
                            currentPatient = PatientDAL.GetByUserID(currentUserId);
                    }
                }

                if (currentPatient != null)
                {
                    if (currentPatient.DateOfBirth < new DateTime(1753, 1, 1) || currentPatient.DateOfBirth == DateTime.MinValue)
                        currentPatient.DateOfBirth = new DateTime(1990, 1, 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void BuildUI()
        {
            // ── Outer scroll panel ──────────────────────────────────────────────
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(20)
            };
            this.Controls.Add(scrollPanel);

            // ── White card ──────────────────────────────────────────────────────
            Panel card = new Panel
            {
                Width = 680,
                BackColor = Color.White,
                Padding = new Padding(30, 20, 30, 20)
            };
            // Round card corners
            card.Paint += (s, e) =>
            {
                GraphicsPath path = RoundedRect(card.ClientRectangle, 15);
                card.Region = new Region(path);
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    e.Graphics.DrawPath(pen, path);
            };
            scrollPanel.Controls.Add(card);
            card.Location = new Point(15, 15);

            // We stack controls manually with a running Y cursor
            int y = 20;

            // ── Title ───────────────────────────────────────────────────────────
            Label lblTitle = new Label
            {
                Text = "👤  My Profile",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(card.Width - 60, 50),
                Location = new Point(0, y)
            };
            card.Controls.Add(lblTitle);
            y += lblTitle.Height + 10;

            // ── Divider ─────────────────────────────────────────────────────────
            Panel divider = new Panel
            {
                Size = new Size(card.Width - 60, 1),
                Location = new Point(0, y),
                BackColor = Color.FromArgb(230, 230, 230)
            };
            card.Controls.Add(divider);
            y += 15;

            // ── Profile picture ─────────────────────────────────────────────────
            picProfile = new PictureBox
            {
                Size = new Size(110, 110),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 240, 240),
                Cursor = Cursors.Hand
            };
            picProfile.Location = new Point((card.Width - 60 - picProfile.Width) / 2, y);
            picProfile.Click += (s, e) => ChangeProfilePicture();
            picProfile.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, picProfile.Width, picProfile.Height);
                picProfile.Region = new Region(path);
            };
            card.Controls.Add(picProfile);
            y += picProfile.Height + 10;

            // ── Change Photo button ─────────────────────────────────────────────
            btnChangePicture = CreateButton("📷  Change Photo", Color.FromArgb(120, 22, 27), Color.White, 130, 32);
            btnChangePicture.Location = new Point((card.Width - 60 - btnChangePicture.Width) / 2, y);
            btnChangePicture.Click += (s, e) => ChangeProfilePicture();
            card.Controls.Add(btnChangePicture);
            y += btnChangePicture.Height + 20;

            // ── Status label ────────────────────────────────────────────────────
            lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Size = new Size(card.Width - 60, 24),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblStatus);
            y += lblStatus.Height + 10;

            // ── Form fields ─────────────────────────────────────────────────────
            int labelW = 140;
            int fieldW = card.Width - 60 - labelW - 10;
            int rowH = 42;
            int rowGap = 10;

            // Helper: add one label+textbox row
            void AddRow(string labelText, out TextBox tb, bool readOnly = true, bool isAddress = false)
            {
                Label lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 90),
                    Size = new Size(labelW, isAddress ? 70 : rowH),
                    Location = new Point(0, y),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                card.Controls.Add(lbl);

                tb = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size = new Size(fieldW, isAddress ? 70 : rowH),
                    Location = new Point(labelW + 10, y),
                    ReadOnly = readOnly,
                    BackColor = readOnly ? Color.FromArgb(248, 248, 250) : Color.White,
                    Multiline = isAddress,
                };
                card.Controls.Add(tb);
                y += (isAddress ? 70 : rowH) + rowGap;
            }

            // Full Name
            AddRow("Full Name:", out txtFullName);
            txtFullName.Text = currentPatient?.FullName ?? "";

            // CNIC
            AddRow("CNIC:", out txtCNIC);
            txtCNIC.Text = currentPatient?.CNIC ?? "";

            // Blood Group (always read-only)
            AddRow("Blood Group:", out txtBloodGroup);
            txtBloodGroup.Text = currentPatient?.BloodGroup ?? "Not Set";
            txtBloodGroup.ReadOnly = true;
            txtBloodGroup.BackColor = Color.FromArgb(248, 248, 250);

            // Date of Birth  ────────────────────────────────────────────────────
            Label lblDob = new Label
            {
                Text = "Date of Birth:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90),
                Size = new Size(labelW, rowH),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblDob);

            dtpDateOfBirth = new DateTimePicker
            {
                Font = new Font("Segoe UI", 11),
                Format = DateTimePickerFormat.Short,
                Size = new Size(fieldW, rowH),
                Location = new Point(labelW + 10, y),
                Enabled = false,
                Value = (currentPatient?.DateOfBirth > DateTime.MinValue)
                            ? currentPatient.DateOfBirth
                            : new DateTime(1990, 1, 1)
            };
            card.Controls.Add(dtpDateOfBirth);
            y += rowH + rowGap;

            // Age  ───────────────────────────────────────────────────────────────
            AddRow("Age:", out txtAge, readOnly: true);
            txtAge.Text = CalculateAge(dtpDateOfBirth.Value).ToString();

            dtpDateOfBirth.ValueChanged += (s, e) =>
                txtAge.Text = CalculateAge(dtpDateOfBirth.Value).ToString();

            // Gender  ────────────────────────────────────────────────────────────
            Label lblGender = new Label
            {
                Text = "Gender:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 90),
                Size = new Size(labelW, rowH),
                Location = new Point(0, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblGender);

            cmbGender = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(fieldW, rowH),
                Location = new Point(labelW + 10, y),
                Enabled = false,
                BackColor = Color.FromArgb(248, 248, 250)
            };
            cmbGender.Items.AddRange(new string[] { "Male", "Female", "Other" });
            if (!string.IsNullOrEmpty(currentPatient?.Gender))
                cmbGender.Text = currentPatient.Gender;
            card.Controls.Add(cmbGender);
            y += rowH + rowGap;

            // Email
            AddRow("Email:", out txtEmail, readOnly: false);
            txtEmail.Text = currentPatient?.Email ?? "";

            // Phone
            AddRow("Phone:", out txtPhone, readOnly: false);
            txtPhone.Text = currentPatient?.Phone ?? "";

            // Address (multiline)
            AddRow("Address:", out txtAddress, readOnly: false, isAddress: true);
            txtAddress.Text = currentPatient?.Address ?? "";

            y += 10;

            // ── Buttons row ─────────────────────────────────────────────────────
            btnEdit = CreateButton("✏️  Edit Profile", Color.FromArgb(59, 130, 246), Color.White, 130, 42);
            btnEdit.Location = new Point(0, y);
            btnEdit.Click += (s, e) => EnableEditing();
            card.Controls.Add(btnEdit);

            btnSave = CreateButton("💾  Save Changes", Color.FromArgb(120, 22, 27), Color.White, 145, 42);
            btnSave.Location = new Point(145, y);
            btnSave.Visible = false;
            btnSave.Click += BtnSave_Click;
            card.Controls.Add(btnSave);

            btnClose = CreateButton("✕  Close", Color.FromArgb(243, 244, 246), Color.FromArgb(55, 65, 81), 100, 42);
            btnClose.Location = new Point(card.Width - 60 - 100, y);
            btnClose.Click += (s, e) => this.Close();
            card.Controls.Add(btnClose);

            y += 42 + 20;

            // ── Set card height ─────────────────────────────────────────────────
            card.Height = y;

            LoadProfilePicture();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private Button CreateButton(string text, Color back, Color fore, int w, int h)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                GraphicsPath path = RoundedRect(btn.ClientRectangle, 8);
                btn.Region = new Region(path);
            };
            return btn;
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            if (dateOfBirth == DateTime.MinValue) return 0;
            int age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;
            return age;
        }

        private void EnableEditing()
        {
            txtFullName.ReadOnly = false; txtFullName.BackColor = Color.White;
            txtEmail.ReadOnly = false; txtEmail.BackColor = Color.White;
            txtPhone.ReadOnly = false; txtPhone.BackColor = Color.White;
            txtAddress.ReadOnly = false; txtAddress.BackColor = Color.White;
            txtCNIC.ReadOnly = false; txtCNIC.BackColor = Color.White;
            dtpDateOfBirth.Enabled = true;
            cmbGender.Enabled = true;
            cmbGender.BackColor = Color.White;

            btnSave.Visible = true;
            btnEdit.Visible = false;

            lblStatus.Text = "✏️  Edit mode enabled. Make changes and click Save.";
            lblStatus.ForeColor = Color.FromArgb(245, 158, 11);
        }

        private void LoadProfilePicture()
        {
            try
            {
                if (currentPatient?.PatientID > 0)
                {
                    byte[] imageData = PatientDAL.GetProfilePicture(currentPatient.PatientID);
                    if (imageData != null && imageData.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(imageData))
                            picProfile.Image = Image.FromStream(ms);
                        return;
                    }
                }
                picProfile.Image = CreateDefaultAvatar();
            }
            catch { picProfile.Image = CreateDefaultAvatar(); }
        }

        private Image CreateDefaultAvatar()
        {
            Bitmap bmp = new Bitmap(110, 110);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(230, 230, 230));
                string initial = currentPatient?.FullName?.Length > 0
                    ? currentPatient.FullName.Substring(0, 1).ToUpper() : "P";
                using (Font font = new Font("Segoe UI", 38, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 22, 27)))
                {
                    SizeF sz = g.MeasureString(initial, font);
                    g.DrawString(initial, font, brush, (110 - sz.Width) / 2, (110 - sz.Height) / 2);
                }
            }
            return bmp;
        }

        private void ChangeProfilePicture()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Select Profile Picture";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image original = Image.FromFile(ofd.FileName);
                        Image resized = ResizeImage(original, 200, 200);
                        picProfile.Image = resized;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            resized.Save(ms, ImageFormat.Jpeg);
                            profilePictureBytes = ms.ToArray();
                        }
                        lblStatus.Text = "✅  New picture selected. Click Save to apply.";
                        lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resized;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPatient == null) return;

                currentPatient.FullName = txtFullName.Text.Trim();
                currentPatient.Email = txtEmail.Text.Trim();
                currentPatient.Phone = txtPhone.Text.Trim();
                currentPatient.Address = txtAddress.Text.Trim();
                currentPatient.CNIC = txtCNIC.Text.Trim();
                currentPatient.DateOfBirth = dtpDateOfBirth.Value;
                currentPatient.Age = CalculateAge(dtpDateOfBirth.Value);
                currentPatient.Gender = cmbGender.Text;

                bool updated = PatientDAL.UpdatePatientProfile(currentPatient);
                if (updated)
                {
                    if (profilePictureBytes != null && profilePictureBytes.Length > 0)
                        PatientDAL.UpdateProfilePicture(currentPatient.PatientID, profilePictureBytes);

                    var user = AdminDAL.GetUserByID(currentUserId);
                    if (user != null) SessionManager.Login(user);

                    lblStatus.Text = "✅  Profile updated successfully!";
                    lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
                    MessageBox.Show("Profile updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Back to read-only
                    txtFullName.ReadOnly = true; txtFullName.BackColor = Color.FromArgb(248, 248, 250);
                    txtEmail.ReadOnly = true; txtEmail.BackColor = Color.FromArgb(248, 248, 250);
                    txtPhone.ReadOnly = true; txtPhone.BackColor = Color.FromArgb(248, 248, 250);
                    txtAddress.ReadOnly = true; txtAddress.BackColor = Color.FromArgb(248, 248, 250);
                    txtCNIC.ReadOnly = true; txtCNIC.BackColor = Color.FromArgb(248, 248, 250);
                    dtpDateOfBirth.Enabled = false;
                    cmbGender.Enabled = false;
                    btnSave.Visible = false;
                    btnEdit.Visible = true;
                }
                else
                {
                    lblStatus.Text = "❌  Failed to update profile. Please try again.";
                    lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌  Error: {ex.Message}";
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}