using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BloodBankManagementSystem.Forms.Common
{
    public partial class ReportViewerForm : Form
    {
        private DataTable _reportData;
        private string _reportTitle;
        private string _reportType;

        public ReportViewerForm(DataTable data, string title, string reportType = "Table")
        {
            _reportData = data;
            _reportTitle = title;
            _reportType = reportType;
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = $"Report Viewer - {_reportTitle}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Panel mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            this.Controls.Add(mainPanel);

            // Title Panel
            Panel titlePanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            titlePanel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 225), 1))
                    e.Graphics.DrawLine(pen, 0, titlePanel.Height - 1, titlePanel.Width, titlePanel.Height - 1);
            };
            mainPanel.Controls.Add(titlePanel);

            Label lblTitle = new Label
            {
                Text = _reportTitle,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 22, 27),
                Location = new Point(25, 20),
                AutoSize = true
            };
            titlePanel.Controls.Add(lblTitle);

            Label lblDate = new Label
            {
                Text = $"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(25, 55),
                AutoSize = true
            };
            titlePanel.Controls.Add(lblDate);

            // Export Buttons
            Button btnExportCSV = new Button
            {
                Text = "📄 Export CSV",
                Location = new Point(titlePanel.Width - 220, 25),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnExportCSV.FlatAppearance.BorderSize = 0;
            btnExportCSV.Click += (s, e) => ExportToCSV();
            titlePanel.Controls.Add(btnExportCSV);

            Button btnPrint = new Button
            {
                Text = "🖨️ Print",
                Location = new Point(titlePanel.Width - 110, 25),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(120, 22, 27),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => PrintReport();
            titlePanel.Controls.Add(btnPrint);

            // Content
            if (_reportType == "Chart" && _reportData != null && _reportData.Rows.Count > 0)
            {
                Chart chart = new Chart { Dock = DockStyle.Fill };
                ChartArea chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);

                Series series = new Series
                {
                    Name = "Data",
                    ChartType = SeriesChartType.Column,
                    BorderWidth = 2,
                    ShadowOffset = 1
                };

                for (int i = 0; i < _reportData.Rows.Count && i < 10; i++)
                {
                    series.Points.AddXY(_reportData.Rows[i][0].ToString(), Convert.ToDouble(_reportData.Rows[i][1]));
                }

                chart.Series.Add(series);
                chart.Titles.Add(_reportTitle);
                chart.BackColor = Color.White;

                mainPanel.Controls.Add(chart);
            }
            else
            {
                DataGridView dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect
                };
                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(120, 22, 27);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgv.EnableHeadersVisualStyles = false;

                if (_reportData != null)
                    dgv.DataSource = _reportData;

                mainPanel.Controls.Add(dgv);
            }
        }

        private void ExportToCSV()
        {
            if (_reportData == null) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"{_reportTitle}_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                // Headers
                for (int i = 0; i < _reportData.Columns.Count; i++)
                {
                    sb.Append(_reportData.Columns[i].ColumnName);
                    if (i < _reportData.Columns.Count - 1) sb.Append(",");
                }
                sb.AppendLine();

                // Data
                foreach (DataRow row in _reportData.Rows)
                {
                    for (int i = 0; i < _reportData.Columns.Count; i++)
                    {
                        sb.Append(row[i].ToString());
                        if (i < _reportData.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();
                }

                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show($"Report exported successfully!\n\nLocation: {sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PrintReport()
        {
            MessageBox.Show("Print functionality will be implemented soon.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}