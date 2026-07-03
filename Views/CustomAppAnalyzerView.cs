using System;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Infrastructure.SystemInfo;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class CustomAppAnalyzerView : UserControl
    {
        public event EventHandler BackRequested;
        
        private CustomAppAnalyzerService _analyzerService;
        private CustomAppCleanupExecutor _executorService;
        private CustomAppAnalysisResult _currentResult;
        
        private Panel topPanel;
        private Panel infoPanel;
        private FlowLayoutPanel resultsPanel;
        private Panel actionPanel;
        
        private Label lblAppInfo;
        private Button btnBrowse;
        private Button btnScan;
        private Button btnClean;
        private Label lblTotalSize;
        
        private string _selectedExePath;

        public CustomAppAnalyzerView()
        {
            _analyzerService = new CustomAppAnalyzerService();
            _executorService = new CustomAppCleanupExecutor();

            this.BackColor = Color.FromArgb(20, 20, 20);
            this.Dock = DockStyle.Fill;
            this.DoubleBuffered = true;
            this.Padding = new Padding(20);

            InitializeUI();
        }

        private void InitializeUI()
        {
            // TOP PANEL (Back Button + Title)
            topPanel = new Panel { Dock = DockStyle.Top, Height = 80 };
            
            var btnBack = new Button
            {
                Text = L.Get("appAnalyzer.back"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(40, 40, 40),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(0, 15),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);

            var lblTitle = new Label
            {
                Text = L.Get("appAnalyzer.title"),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(100, 10),
                AutoSize = true
            };
            
            var lblSubtitle = new Label
            {
                Text = L.Get("appAnalyzer.subtitle"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(105, 45),
                AutoSize = true
            };

            topPanel.Controls.Add(btnBack);
            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblSubtitle);

            // INFO PANEL
            infoPanel = new Panel { Dock = DockStyle.Top, Height = 180, Padding = new Padding(0, 20, 0, 20) };
            
            btnBrowse = new Button
            {
                Text = L.Get("appAnalyzer.browse"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.MediumSlateBlue,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(0, 20),
                Cursor = Cursors.Hand
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;

            lblAppInfo = new Label
            {
                Text = L.Get("appAnalyzer.noApp"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(170, 20),
                AutoSize = true
            };

            btnScan = new Button
            {
                Text = L.Get("appAnalyzer.scan"),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(0, 80),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;

            infoPanel.Controls.Add(btnBrowse);
            infoPanel.Controls.Add(lblAppInfo);
            infoPanel.Controls.Add(btnScan);

            // ACTION PANEL (Bottom)
            actionPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            
            lblTotalSize = new Label
            {
                Text = L.Get("appAnalyzer.selectedSpace", "0 MB"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(0, 30),
                AutoSize = true,
                Visible = false
            };

            btnClean = new Button
            {
                Text = L.Get("appAnalyzer.cleanSelected"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(232, 17, 35),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnClean.Location = new Point(this.Width - 250, 20); // Manual loc for designer logic
            btnClean.FlatAppearance.BorderSize = 0;
            btnClean.Click += BtnClean_Click;

            actionPanel.Controls.Add(lblTotalSize);
            actionPanel.Controls.Add(btnClean);
            
            // Fix resize
            this.Resize += (s, e) => {
                if (btnClean != null) btnClean.Location = new Point(this.Width - 250, 20);
            };

            // RESULTS PANEL
            resultsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            resultsPanel.MouseEnter += (s, e) => resultsPanel.Focus();

            this.Controls.Add(actionPanel);
            this.Controls.Add(infoPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(resultsPanel);
            
            resultsPanel.BringToFront();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable Files (*.exe)|*.exe";
                ofd.Title = "Select Application Executable";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedExePath = ofd.FileName;
                    var meta = _analyzerService.GetMetadata(_selectedExePath);
                    
                    lblAppInfo.Text = L.Get("appAnalyzer.appInfo", meta.AppName, meta.Publisher ?? L.Get("appAnalyzer.unknown"), meta.Version ?? L.Get("appAnalyzer.unknown"), meta.ExePath, meta.InstallFolder);
                    
                    btnScan.Enabled = true;
                    for (int i = resultsPanel.Controls.Count - 1; i >= 0; i--)
                    {
                        var c = resultsPanel.Controls[i];
                        resultsPanel.Controls.RemoveAt(i);
                        c.Dispose();
                    }
                    lblTotalSize.Visible = false;
                    btnClean.Visible = false;
                }
            }
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedExePath)) return;

            btnScan.Enabled = false;
            btnBrowse.Enabled = false;
            btnClean.Enabled = false;
            for (int i = resultsPanel.Controls.Count - 1; i >= 0; i--)
            {
                var c = resultsPanel.Controls[i];
                resultsPanel.Controls.RemoveAt(i);
                c.Dispose();
            }
            lblTotalSize.Visible = false;

            var progress = new Progress<string>(msg =>
            {
                var label = new Label
                {
                    Text = msg,
                    ForeColor = Color.DarkGray,
                    AutoSize = true,
                    Margin = new Padding(0, 5, 0, 5),
                    Font = new Font("Segoe UI", 10)
                };
                resultsPanel.Controls.Add(label);
                resultsPanel.ScrollControlIntoView(label);
            });

            _currentResult = await _analyzerService.ScanAsync(_selectedExePath, progress);

            for (int i = resultsPanel.Controls.Count - 1; i >= 0; i--)
            {
                var c = resultsPanel.Controls[i];
                resultsPanel.Controls.RemoveAt(i);
                c.Dispose();
            }

            if (_currentResult.Categories.Count == 0)
            {
                resultsPanel.Controls.Add(new Label { Text = L.Get("appAnalyzer.noSafeItems"), ForeColor = Color.Gray, AutoSize = true, Font = new Font("Segoe UI", 12) });
            }
            else
            {
                foreach (var cat in _currentResult.Categories)
                {
                    var card = new CustomAppGroupCard(cat);
                    card.Width = resultsPanel.ClientSize.Width - 30;
                    card.SelectionChanged += (s, ev) => RecalculateSelectedSize();
                    
                    card.MouseEnter += (s, ev) => resultsPanel.Focus();
                    foreach (Control child in card.Controls)
                    {
                        child.MouseEnter += (s, ev) => resultsPanel.Focus();
                    }

                    resultsPanel.Controls.Add(card);
                }
                
                lblTotalSize.Visible = true;
                btnClean.Visible = true;
                RecalculateSelectedSize();
            }

            btnScan.Enabled = true;
            btnBrowse.Enabled = true;
        }

        private void RecalculateSelectedSize()
        {
            if (_currentResult == null) return;
            lblTotalSize.Text = L.Get("appAnalyzer.selectedSpace", _currentResult.FormattedSelectedSize);
            btnClean.Enabled = _currentResult.TotalSelectedSizeBytes > 0;
        }

        private async void BtnClean_Click(object sender, EventArgs e)
        {
            if (_currentResult == null || _currentResult.TotalSelectedSizeBytes == 0) return;

            var confirm = MessageBox.Show(L.Get("dialog.cleanupAppWarn", _currentResult.FormattedSelectedSize, _currentResult.Metadata.AppName), 
                L.Get("dialog.confirmCleanup"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            
            if (confirm != DialogResult.OK) return;

            btnScan.Enabled = false;
            btnBrowse.Enabled = false;
            btnClean.Enabled = false;
            for (int i = resultsPanel.Controls.Count - 1; i >= 0; i--)
            {
                var c = resultsPanel.Controls[i];
                resultsPanel.Controls.RemoveAt(i);
                c.Dispose();
            }
            lblTotalSize.Visible = false;

            var progress = new Progress<string>(msg =>
            {
                var label = new Label
                {
                    Text = msg,
                    ForeColor = Color.Orange,
                    AutoSize = true,
                    Margin = new Padding(0, 5, 0, 5),
                    Font = new Font("Segoe UI", 10)
                };
                resultsPanel.Controls.Add(label);
                resultsPanel.ScrollControlIntoView(label);
            });

            var execResult = await _executorService.ExecuteAsync(_currentResult, progress);

            for (int i = resultsPanel.Controls.Count - 1; i >= 0; i--)
            {
                var c = resultsPanel.Controls[i];
                resultsPanel.Controls.RemoveAt(i);
                c.Dispose();
            }
            
            var lblComplete = new Label { Text = L.Get("appAnalyzer.complete"), ForeColor = Color.LimeGreen, AutoSize = true, Margin = new Padding(0, 10, 0, 10), Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            var lblFreed = new Label { Text = L.Get("appAnalyzer.freedSpace", execResult.FormattedFreedSize), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 12) };
            var lblDeleted = new Label { Text = L.Get("appAnalyzer.deletedFiles", execResult.DeletedFiles), ForeColor = Color.LightGray, AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 10) };
            var lblSkipped = new Label { Text = L.Get("appAnalyzer.skippedFiles", execResult.SkippedFiles), ForeColor = Color.DarkGray, AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 10) };
            var lblDuration = new Label { Text = L.Get("appAnalyzer.duration", Math.Round(execResult.DurationSeconds, 1)), ForeColor = Color.DarkGray, AutoSize = true, Margin = new Padding(0, 5, 0, 5), Font = new Font("Segoe UI", 10) };

            resultsPanel.Controls.Add(lblComplete);
            resultsPanel.Controls.Add(lblFreed);
            resultsPanel.Controls.Add(lblDeleted);
            resultsPanel.Controls.Add(lblSkipped);
            resultsPanel.Controls.Add(lblDuration);

            btnScan.Enabled = true;
            btnBrowse.Enabled = true;
            _currentResult = null; // reset
        }
    }
}
