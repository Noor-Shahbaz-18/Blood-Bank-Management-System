using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Forms.Doctor
{
    public partial class CrossMatchingResult : UserControl
    {
        // ── Colours ──────────────────────────────────────────────────────────────
        private readonly Color brickRed = Color.FromArgb(120, 22, 27);
        private readonly Color lightBrickRed = Color.FromArgb(254, 226, 226);

        // ── State ────────────────────────────────────────────────────────────────
        private DataTable allResults;
        private DataTable filteredResults;
        private int selectedColumnIndex = -1;

        // ── Controls ─────────────────────────────────────────────────────────────
        private DataGridView dgv;
        private TextBox txtSearch;
        private TextBox txtPatientName;
        private ComboBox cmbBloodGroup;
        private ComboBox cmbDonorUnit;
        private ComboBox cmbTechnician;
        private ComboBox cmbRequisitionNo;
        private ComboBox cmbResult;
        private Label[] valLabels = new Label[4];
        private Label lblLoading;
        private Label lblNoData;

        // ── Right-column controls ────────────────────────────────────────────────
        private Label lblDonorUnit;
        private Label lblTechnician;
        private Label lblTestResult;
        private Button btnReset;
        private Button btnRun;

        // ── Layout refs ───────────────────────────────────────────────────────────
        private FlowLayoutPanel cardsRow;
        private Panel formCard;
        private Panel tableCard;
        private FlowLayoutPanel stack;
        private Panel scroll;

        private const int CARD_H = 120;
        private const int ROW_H = CARD_H + 8;
        private const int COL_A_LBL = 20;
        private const int COL_A_FLD = 155;
        private const int FLD_W = 240;

        // ── Constructor ──────────────────────────────────────────────────────────
        public CrossMatchingResult()
        {
            InitializeComponent();
            BuildUI();
            LoadData();
        }

        // =========================================================
        // LOAD DATA FROM DATABASE (NO HARDCODED DATA)
        // =========================================================
        private void LoadData()
        {
            ShowLoading(true);
            LoadCrossMatchResults();
            LoadPendingRequisitions();
            LoadDonorUnits();
            LoadTechnicians();
            ShowLoading(false);
        }

        private void LoadCrossMatchResults()
        {
            try
            {
                string query = @"SELECT 
                                    r.RequisitionNumber, 
                                    r.PatientName, 
                                    r.BloodGroup,
                                    r.Status,
                                    r.CrossMatchResult,
                                    r.CrossMatchDate,
                                    r.CrossMatchPerformedBy,
                                    ISNULL(b.BagID, 'Not Assigned') AS DonorUnit
                                FROM Requisitions r
                                LEFT JOIN BloodBags b ON r.AssignedBagID = b.BagID
                                WHERE r.Status IN ('Cross Matching', 'Approved', 'Rejected')
                                ORDER BY r.RequestDate DESC";

                DataTable dbData = CommonDAL.ExecuteReader(query);

                allResults = new DataTable();
                allResults.Columns.Add("RequisitionNumber", typeof(string));
                allResults.Columns.Add("PatientName", typeof(string));
                allResults.Columns.Add("BloodGroup", typeof(string));
                allResults.Columns.Add("DonorUnit", typeof(string));
                allResults.Columns.Add("Status", typeof(string));
                allResults.Columns.Add("TestDate", typeof(string));
                allResults.Columns.Add("Technician", typeof(string));

                if (dbData != null && dbData.Rows.Count > 0)
                {
                    foreach (DataRow row in dbData.Rows)
                    {
                        string st = row["Status"]?.ToString() ?? "Pending";
                        string crossResult = row["CrossMatchResult"]?.ToString() ?? "";

                        // Determine display status
                        string displayStatus = "Pending";
                        if (st == "Approved" || crossResult == "Compatible")
                            displayStatus = "Compatible";
                        else if (st == "Rejected" || crossResult == "Incompatible")
                            displayStatus = "Incompatible";
                        else if (st == "Cross Matching")
                            displayStatus = "Pending";

                        string testDate = row["CrossMatchDate"] != DBNull.Value ?
                            Convert.ToDateTime(row["CrossMatchDate"]).ToString("dd-MMM-yyyy") : "-";
                        string technician = row["CrossMatchPerformedBy"]?.ToString() ?? "-";
                        string donorUnit = row["DonorUnit"]?.ToString() ?? "Not Assigned";

                        allResults.Rows.Add(
                            row["RequisitionNumber"],
                            row["PatientName"],
                            row["BloodGroup"],
                            donorUnit,
                            displayStatus,
                            testDate,
                            technician);
                    }
                }

                filteredResults = allResults.Copy();

                if (allResults.Rows.Count == 0)
                {
                    ShowNoDataMessage();
                }
                else
                {
                    HideNoDataMessage();
                }

                PopulateGrid();
                UpdateCards();
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading cross match results", ex);
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowNoDataMessage();
            }
        }

        private void LoadPendingRequisitions()
        {
            if (cmbRequisitionNo == null) return;

            cmbRequisitionNo.Items.Clear();
            cmbRequisitionNo.Items.Add("Select Requisition");

            try
            {
                // Get requisitions that need cross matching
                string query = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup
                                FROM Requisitions 
                                WHERE Status = 'Cross Matching' OR Status = 'Pending'
                                ORDER BY RequestDate DESC";

                DataTable reqs = CommonDAL.ExecuteReader(query);

                if (reqs != null && reqs.Rows.Count > 0)
                {
                    foreach (DataRow r in reqs.Rows)
                    {
                        string rn = r["RequisitionNumber"]?.ToString() ?? "";
                        string pn = r["PatientName"]?.ToString() ?? "";
                        string bg = r["BloodGroup"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(rn))
                        {
                            cmbRequisitionNo.Items.Add($"{rn} - {pn} ({bg})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading pending requisitions", ex);
            }

            if (cmbRequisitionNo.Items.Count == 0)
            {
                cmbRequisitionNo.Items.Add("No pending requisitions");
            }

            cmbRequisitionNo.SelectedIndex = 0;
        }

        private void LoadDonorUnits()
        {
            if (cmbDonorUnit == null) return;

            cmbDonorUnit.Items.Clear();
            cmbDonorUnit.Items.Add("Select Donor Unit");

            try
            {
                // Get available blood bags
                string query = @"SELECT BagID, BloodGroup FROM BloodBags 
                                WHERE Status = 'Available' 
                                ORDER BY BagID";

                DataTable bags = CommonDAL.ExecuteReader(query);

                if (bags != null && bags.Rows.Count > 0)
                {
                    foreach (DataRow r in bags.Rows)
                    {
                        string id = r["BagID"]?.ToString() ?? "";
                        string bg = r["BloodGroup"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(id))
                        {
                            cmbDonorUnit.Items.Add($"{id} ({bg})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading donor units", ex);
            }

            if (cmbDonorUnit.Items.Count == 0)
            {
                cmbDonorUnit.Items.Add("No available donor units");
            }

            cmbDonorUnit.SelectedIndex = 0;
        }

        private void LoadTechnicians()
        {
            if (cmbTechnician == null) return;

            cmbTechnician.Items.Clear();
            cmbTechnician.Items.Add("Select Technician");

            try
            {
                string query = @"SELECT FullName FROM Technicians WHERE IsActive = 1 ORDER BY FullName";
                DataTable techs = CommonDAL.ExecuteReader(query);

                if (techs != null && techs.Rows.Count > 0)
                {
                    foreach (DataRow r in techs.Rows)
                    {
                        string name = r["FullName"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(name))
                        {
                            cmbTechnician.Items.Add(name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading technicians", ex);
            }

            if (cmbTechnician.Items.Count == 0)
            {
                cmbTechnician.Items.Add("No technicians found");
            }

            cmbTechnician.SelectedIndex = 0;
        }

        private void ShowLoading(bool show)
        {
            if (lblLoading == null)
            {
                lblLoading = new Label();
                lblLoading.Text = "⏳ Loading cross match data...";
                lblLoading.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblLoading.ForeColor = brickRed;
                lblLoading.TextAlign = ContentAlignment.MiddleCenter;
                lblLoading.BackColor = Color.White;
                lblLoading.Size = new Size(250, 50);
                lblLoading.Anchor = AnchorStyles.None;
                this.Controls.Add(lblLoading);
            }

            lblLoading.Visible = show;
            if (show)
            {
                lblLoading.Location = new Point((this.Width - 250) / 2, (this.Height - 50) / 2);
                lblLoading.BringToFront();
            }
        }

        private void ShowNoDataMessage()
        {
            if (lblNoData == null)
            {
                lblNoData = new Label();
                lblNoData.Name = "lblNoData";
                lblNoData.Text = "📋 No cross match records found\n\nWhen cross matching tests are performed, they will appear here.";
                lblNoData.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblNoData.ForeColor = Color.Gray;
                lblNoData.TextAlign = ContentAlignment.MiddleCenter;
                lblNoData.BackColor = Color.White;
                lblNoData.Size = new Size(450, 100);
                lblNoData.Anchor = AnchorStyles.None;
                this.Controls.Add(lblNoData);
            }

            lblNoData.Visible = true;
            lblNoData.Location = new Point((this.Width - 450) / 2, (this.Height - 100) / 2);
            lblNoData.BringToFront();
        }

        private void HideNoDataMessage()
        {
            if (lblNoData != null)
            {
                lblNoData.Visible = false;
            }
        }

        // =========================================================
        // BUILD UI
        // =========================================================
        private void BuildUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 247, 250);

            scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(25, 15, 25, 20)
            };
            this.Controls.Add(scroll);

            stack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(stack);

            // ── Page title ────────────────────────────────────────────────────────
            Label pageTitle = new Label
            {
                Text = "🧪  Cross Matching",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = brickRed,
                AutoSize = false,
                Height = 48,
                Margin = new Padding(0, 0, 0, 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            stack.Controls.Add(pageTitle);

            // ── Summary cards ─────────────────────────────────────────────────────
            string[] cardTitles = { "Total Tests", "Pending", "Compatible", "Incompatible" };
            Color[] cardColors =
            {
                brickRed,
                Color.FromArgb(245, 158,  11),
                Color.FromArgb( 34, 197,  94),
                Color.FromArgb(220,  38,  38)
            };

            cardsRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Height = ROW_H,
                Margin = new Padding(0, 0, 0, 14),
                BackColor = Color.Transparent
            };
            stack.Controls.Add(cardsRow);

            for (int i = 0; i < 4; i++)
            {
                int ci = i;
                Panel card = new Panel
                {
                    Size = new Size(200, CARD_H),
                    BackColor = Color.White,
                    Margin = new Padding(0, 0, 14, 0)
                };
                card.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush b = new SolidBrush(cardColors[ci]))
                        e.Graphics.FillRectangle(b, 0, 0, 5, card.Height);
                    using (Pen p = new Pen(Color.FromArgb(220, 220, 220), 1))
                        e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                };

                Label valLbl = new Label
                {
                    Text = "0",
                    Font = new Font("Segoe UI", 26, FontStyle.Bold),
                    ForeColor = cardColors[i],
                    AutoSize = false,
                    Size = new Size(190, 62),
                    Location = new Point(5, 8),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Label titLbl = new Label
                {
                    Text = cardTitles[i],
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(75, 85, 99),
                    AutoSize = false,
                    Size = new Size(190, 44),
                    Location = new Point(5, 70),
                    TextAlign = ContentAlignment.TopCenter
                };

                card.Controls.Add(valLbl);
                card.Controls.Add(titLbl);
                cardsRow.Controls.Add(card);
                valLabels[i] = valLbl;
            }

            // ── Form card ─────────────────────────────────────────────────────────
            formCard = new Panel
            {
                BackColor = Color.White,
                AutoSize = false,
                Height = 285,
                Margin = new Padding(0, 0, 0, 14)
            };
            formCard.Paint += CardBorder;
            stack.Controls.Add(formCard);
            BuildFormCard();

            // ── Table card ────────────────────────────────────────────────────────
            tableCard = new Panel
            {
                BackColor = Color.White,
                AutoSize = false,
                Height = 380,
                Margin = new Padding(0)
            };
            tableCard.Paint += CardBorder;
            stack.Controls.Add(tableCard);
            BuildTableCard();

            // ── Reflow ────────────────────────────────────────────────────────────
            scroll.Resize += (s, e) => Reflow(pageTitle);
            this.Load += (s, e) => Reflow(pageTitle);
        }

        // =========================================================
        // BUILD FORM CARD
        // =========================================================
        private void BuildFormCard()
        {
            formCard.Controls.Add(new Label
            {
                Text = "Run Cross Match Test",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = false,
                Size = new Size(400, 40),
                Location = new Point(20, 14),
                TextAlign = ContentAlignment.MiddleLeft
            });

            formCard.Controls.Add(new Panel
            {
                Name = "divForm",
                BackColor = Color.FromArgb(230, 230, 230),
                Height = 1,
                Location = new Point(0, 60)
            });

            int[] rowY = { 76, 130, 184 };

            // LEFT COLUMN
            AddLabel(formCard, "Requisition No:", COL_A_LBL, rowY[0]);
            cmbRequisitionNo = AddCombo(formCard, COL_A_FLD, rowY[0], FLD_W);
            cmbRequisitionNo.SelectedIndexChanged += CmbRequisitionNo_SelectedIndexChanged;

            AddLabel(formCard, "Patient Name:", COL_A_LBL, rowY[1]);
            txtPatientName = new TextBox
            {
                Location = new Point(COL_A_FLD, rowY[1]),
                Size = new Size(FLD_W, 32),
                Font = new Font("Segoe UI", 10),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            formCard.Controls.Add(txtPatientName);

            AddLabel(formCard, "Blood Group:", COL_A_LBL, rowY[2]);
            cmbBloodGroup = AddCombo(formCard, COL_A_FLD, rowY[2], FLD_W);
            cmbBloodGroup.Items.AddRange(new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
            cmbBloodGroup.Enabled = false;

            // RIGHT COLUMN
            lblDonorUnit = MakeLabel("Donor Unit:");
            formCard.Controls.Add(lblDonorUnit);
            cmbDonorUnit = AddCombo(formCard, 0, rowY[0], FLD_W);

            lblTechnician = MakeLabel("Technician:");
            formCard.Controls.Add(lblTechnician);
            cmbTechnician = AddCombo(formCard, 0, rowY[1], FLD_W);

            lblTestResult = MakeLabel("Test Result:");
            formCard.Controls.Add(lblTestResult);
            cmbResult = AddCombo(formCard, 0, rowY[2], FLD_W);
            cmbResult.Items.AddRange(new string[] { "Select Result", "Compatible", "Incompatible" });
            cmbResult.SelectedIndex = 0;

            btnReset = new Button
            {
                Text = "Reset",
                Size = new Size(100, 36),
                Location = new Point(COL_A_FLD, 238),
                BackColor = Color.FromArgb(243, 244, 246),
                ForeColor = Color.FromArgb(55, 65, 81),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btnReset.Click += (s, e) => ResetForm();
            formCard.Controls.Add(btnReset);

            btnRun = new Button
            {
                Text = "▶  Run Test",
                Size = new Size(130, 36),
                Location = new Point(COL_A_FLD + 110, 238),
                BackColor = brickRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10),
                Cursor = Cursors.Hand
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += (s, e) => RunTest();
            formCard.Controls.Add(btnRun);
        }

        private Label MakeLabel(string text) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 10),
            ForeColor = Color.FromArgb(55, 65, 81),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        // =========================================================
        // BUILD TABLE CARD
        // =========================================================
        private void BuildTableCard()
        {
            tableCard.Controls.Add(new Label
            {
                Text = "Cross Matching Results",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize = false,
                Size = new Size(350, 40),
                Location = new Point(20, 14),
                TextAlign = ContentAlignment.MiddleLeft
            });

            Button btnRefresh = new Button
            {
                Text = "🔄  Refresh",
                Size = new Size(105, 36),
                BackColor = brickRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadData();
            tableCard.Controls.Add(btnRefresh);

            txtSearch = new TextBox
            {
                Text = "Search patient or requisition...",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Size = new Size(220, 36),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text.StartsWith("Search")) { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Search patient or requisition..."; txtSearch.ForeColor = Color.Gray; } };
            txtSearch.TextChanged += (s, e) => FilterData();
            tableCard.Controls.Add(txtSearch);

            tableCard.Controls.Add(new Panel
            {
                Name = "divTable",
                BackColor = Color.FromArgb(230, 230, 230),
                Height = 1,
                Location = new Point(0, 62)
            });

            dgv = new DataGridView
            {
                Location = new Point(0, 63),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 44,
                GridColor = Color.FromArgb(229, 231, 235),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            dgv.RowTemplate.Height = 46;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.SelectionBackColor = lightBrickRed;
            dgv.DefaultCellStyle.SelectionForeColor = brickRed;

            dgv.Columns.Add("reqno", "Request No");
            dgv.Columns.Add("patient", "Patient");
            dgv.Columns.Add("blood", "Blood Group");
            dgv.Columns.Add("donor", "Donor Unit");
            dgv.Columns.Add("status", "Status");
            dgv.Columns.Add("date", "Test Date");
            dgv.Columns.Add("tech", "Technician");

            dgv.Columns["reqno"].FillWeight = 110;
            dgv.Columns["patient"].FillWeight = 120;
            dgv.Columns["blood"].FillWeight = 85;
            dgv.Columns["donor"].FillWeight = 95;
            dgv.Columns["status"].FillWeight = 110;
            dgv.Columns["date"].FillWeight = 100;
            dgv.Columns["tech"].FillWeight = 120;

            dgv.CellPainting += Dgv_CellPainting;
            dgv.CellClick += Dgv_CellClick;

            tableCard.Controls.Add(dgv);
        }

        // =========================================================
        // REFLOW
        // =========================================================
        private void Reflow(Label pageTitle)
        {
            int usable = scroll.ClientSize.Width - scroll.Padding.Horizontal;
            if (usable < 300) return;

            stack.Width = usable;
            pageTitle.Width = usable;

            cardsRow.Width = usable;
            const int gap = 14;
            int cardW = (usable - gap * 3) / 4;
            cardW = Math.Max(cardW, 140);
            for (int i = 0; i < cardsRow.Controls.Count; i++)
            {
                Panel c = (Panel)cardsRow.Controls[i];
                c.Size = new Size(cardW, CARD_H);
                c.Margin = new Padding(0, 0, i < 3 ? gap : 0, 0);
                foreach (Control lbl in c.Controls) lbl.Width = cardW - 10;
            }

            formCard.Width = usable;
            foreach (Control c in formCard.Controls)
                if (c is Panel p && p.Name == "divForm") p.Width = usable;

            int mid = usable / 2;
            int colB_lbl = mid + 10;
            int colB_fld = colB_lbl + 115;
            int rightFldW = usable - colB_fld - 20;
            rightFldW = Math.Max(rightFldW, 140);

            int[] rowY = { 76, 130, 184 };

            lblDonorUnit.Location = new Point(colB_lbl, rowY[0] + 5);
            lblTechnician.Location = new Point(colB_lbl, rowY[1] + 5);
            lblTestResult.Location = new Point(colB_lbl, rowY[2] + 5);

            cmbDonorUnit.Location = new Point(colB_fld, rowY[0]);
            cmbTechnician.Location = new Point(colB_fld, rowY[1]);
            cmbResult.Location = new Point(colB_fld, rowY[2]);

            cmbDonorUnit.Width = rightFldW;
            cmbTechnician.Width = rightFldW;
            cmbResult.Width = rightFldW;

            int leftFldW = mid - COL_A_FLD - 20;
            leftFldW = Math.Max(leftFldW, 140);
            cmbRequisitionNo.Width = leftFldW;
            txtPatientName.Width = leftFldW;
            cmbBloodGroup.Width = leftFldW;

            tableCard.Width = usable;
            tableCard.Height = Math.Max(380,
                scroll.ClientSize.Height
                - 48 - 10
                - ROW_H - 14
                - 285 - 14
                - 20);

            int right = tableCard.Width - 15;
            Button tblRefresh = null;
            TextBox tblSearch = null;
            foreach (Control c in tableCard.Controls)
            {
                if (c is Button b && b.Text.Contains("Refresh")) tblRefresh = b;
                if (c is TextBox tx) tblSearch = tx;
                if (c is Panel p && p.Name == "divTable") p.Width = tableCard.Width;
            }
            if (tblRefresh != null)
                tblRefresh.Location = new Point(right - tblRefresh.Width, 13);
            if (tblSearch != null && tblRefresh != null)
                tblSearch.Location = new Point(tblRefresh.Left - tblSearch.Width - 10, 13);

            dgv.Width = tableCard.Width;
            dgv.Height = tableCard.Height - 63;
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = Color.FromArgb(55, 65, 81),
                AutoSize = true,
                Location = new Point(x, y + 5)
            });
        }

        private ComboBox AddCombo(Control parent, int x, int y, int width)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            parent.Controls.Add(cmb);
            return cmb;
        }

        private void CardBorder(object sender, PaintEventArgs e)
        {
            var p = (Panel)sender;
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        }

        private void PopulateGrid()
        {
            if (dgv == null || filteredResults == null) return;
            dgv.Rows.Clear();
            foreach (DataRow r in filteredResults.Rows)
            {
                dgv.Rows.Add(
                    r["RequisitionNumber"], r["PatientName"],
                    r["BloodGroup"], r["DonorUnit"],
                    r["Status"], r["TestDate"],
                    r["Technician"]);
            }
        }

        private void FilterData()
        {
            if (allResults == null) return;
            string search = txtSearch.Text.Trim().ToLower();
            if (search.StartsWith("search")) search = "";

            filteredResults = allResults.Clone();
            foreach (DataRow r in allResults.Rows)
            {
                bool m = string.IsNullOrEmpty(search)
                    || r["PatientName"].ToString().ToLower().Contains(search)
                    || r["RequisitionNumber"].ToString().ToLower().Contains(search);
                if (m) filteredResults.ImportRow(r);
            }
            PopulateGrid();
        }

        private void UpdateCards()
        {
            if (allResults == null || valLabels[0] == null) return;

            int total = allResults.Rows.Count, pending = 0, compatible = 0, incompatible = 0;
            foreach (DataRow r in allResults.Rows)
            {
                string st = r["Status"]?.ToString() ?? "";
                if (st == "Pending") pending++;
                else if (st == "Compatible") compatible++;
                else if (st == "Incompatible") incompatible++;
            }
            if (valLabels[0] != null) valLabels[0].Text = total.ToString();
            if (valLabels[1] != null) valLabels[1].Text = pending.ToString();
            if (valLabels[2] != null) valLabels[2].Text = compatible.ToString();
            if (valLabels[3] != null) valLabels[3].Text = incompatible.ToString();
        }

        // =========================================================
        // EVENTS
        // =========================================================
        private void CmbRequisitionNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRequisitionNo.SelectedIndex > 0 && cmbRequisitionNo.SelectedItem.ToString() != "No pending requisitions")
            {
                string sel = cmbRequisitionNo.SelectedItem.ToString();
                int ob = sel.IndexOf("("), cb = sel.IndexOf(")");
                if (ob > 0 && cb > ob)
                {
                    string bg = sel.Substring(ob + 1, cb - ob - 1);
                    string[] parts = sel.Split(new string[] { " - " }, StringSplitOptions.None);
                    txtPatientName.Text = parts.Length > 1
                        ? parts[1].Substring(0, parts[1].LastIndexOf("(")).Trim()
                        : "";
                    int idx = cmbBloodGroup.FindStringExact(bg);
                    if (idx >= 0) cmbBloodGroup.SelectedIndex = idx;
                }
            }
            else
            {
                txtPatientName.Text = "";
                cmbBloodGroup.SelectedIndex = -1;
            }
        }

        private async void RunTest()
        {
            if (cmbRequisitionNo.SelectedIndex == 0 || cmbRequisitionNo.SelectedItem.ToString() == "No pending requisitions")
            { Warn("Please select a requisition."); return; }
            if (cmbDonorUnit.SelectedIndex == 0 || cmbDonorUnit.SelectedItem.ToString() == "No available donor units")
            { Warn("Please select a donor unit."); return; }
            if (cmbTechnician.SelectedIndex == 0 || cmbTechnician.SelectedItem.ToString() == "No technicians found")
            { Warn("Please select a technician."); return; }
            if (cmbResult.SelectedIndex == 0)
            { Warn("Please select a test result."); return; }

            string result = cmbResult.SelectedItem.ToString();
            string donorUnit = cmbDonorUnit.SelectedItem.ToString().Split('(')[0].Trim();
            string technician = cmbTechnician.SelectedItem.ToString();
            string selectedRequisition = cmbRequisitionNo.SelectedItem.ToString();
            string reqNumber = selectedRequisition.Split('-')[0].Trim() + "-" + selectedRequisition.Split('-')[1].Trim();

            try
            {
                // Update requisition in database
                string query = @"UPDATE Requisitions 
                                SET Status = @Status,
                                    CrossMatchResult = @Result,
                                    CrossMatchPerformedBy = @Technician,
                                    CrossMatchDate = GETDATE(),
                                    AssignedBagID = @DonorUnit
                                WHERE RequisitionNumber = @ReqNumber";

                var parameters = new System.Data.SqlClient.SqlParameter[]
                {
                    new System.Data.SqlClient.SqlParameter("@Status", result == "Compatible" ? "Approved" : "Rejected"),
                    new System.Data.SqlClient.SqlParameter("@Result", result),
                    new System.Data.SqlClient.SqlParameter("@Technician", technician),
                    new System.Data.SqlClient.SqlParameter("@DonorUnit", donorUnit),
                    new System.Data.SqlClient.SqlParameter("@ReqNumber", reqNumber)
                };

                int rowsAffected = CommonDAL.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"Cross match test completed!\n\nResult: {result}\nDonor Unit: {donorUnit}\nTechnician: {technician}",
                        "Test Complete", MessageBoxButtons.OK,
                        result == "Compatible" ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

                    ResetForm();
                    await System.Threading.Tasks.Task.Delay(500);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Failed to update requisition. Please try again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error running cross match test", ex);
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetForm()
        {
            cmbRequisitionNo.SelectedIndex = 0;
            txtPatientName.Text = "";
            cmbBloodGroup.SelectedIndex = -1;
            cmbDonorUnit.SelectedIndex = 0;
            cmbTechnician.SelectedIndex = 0;
            cmbResult.SelectedIndex = 0;
        }

        private void Warn(string msg) =>
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // =========================================================
        // CELL PAINTING
        // =========================================================
        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool sel = e.ColumnIndex == selectedColumnIndex;
                e.PaintBackground(e.CellBounds, true);
                using (SolidBrush b = new SolidBrush(sel ? lightBrickRed : Color.FromArgb(248, 250, 252)))
                    e.Graphics.FillRectangle(b, e.CellBounds);
                TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString(),
                    new Font("Segoe UI Semibold", 10), e.CellBounds,
                    sel ? brickRed : Color.FromArgb(55, 65, 81),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
                e.Handled = true;
                return;
            }

            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 4)
            {
                e.PaintBackground(e.CellBounds, true);
                string st = e.Value?.ToString() ?? "";
                Color fc = st == "Compatible" ? Color.FromArgb(34, 197, 94)
                          : st == "Incompatible" ? Color.FromArgb(220, 38, 38)
                          : st == "Pending" ? Color.FromArgb(245, 158, 11)
                          : Color.FromArgb(55, 65, 81);

                Rectangle pr = new Rectangle(
                    e.CellBounds.Left + 8, e.CellBounds.Top + 10,
                    e.CellBounds.Width - 16, e.CellBounds.Height - 20);
                using (SolidBrush pb = new SolidBrush(Color.FromArgb(30, fc)))
                    e.Graphics.FillRectangle(pb, pr);
                TextRenderer.DrawText(e.Graphics, st,
                    new Font("Segoe UI Semibold", 9), pr, fc,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }

        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            { selectedColumnIndex = e.ColumnIndex; dgv.Invalidate(); }
        }
    }
}