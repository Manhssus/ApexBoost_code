using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Domain.Interfaces;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class AppRemovalReviewView : UserControl
    {
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Button _btnBack;
        private Panel _warningBanner;
        private Label _lblWarningText;

        // Health Dashboard
        private TableLayoutPanel _dashboardPanel;
        private AppRemovalGaugeControl _healthMeterControl;

        // Stats Grid
        private TableLayoutPanel _statsGrid;
        private Label _lblTotalVal;
        private Label _lblReviewVal;
        private Label _lblUnknownVal;
        private Label _lblLastScanVal;

        // Toolbar & Filters
        private Panel _toolbarPanel;
        private Button _btnFilterAll;
        private Button _btnFilterRisks;
        private Button _btnFilterLarge;
        private Button _btnRefresh;
        private Button _btnCopyReport;
        private Button _btnOpenSettings;

        // List
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpItems;
        private ModernScrollBar _customScrollBar;

        private IAppRemovalReviewService _service;
        private AppRemovalReviewResult _latestResult;

        private string _currentFilter = "All";
        private Dictionary<string, Image> _iconCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler BackRequested;

        public AppRemovalReviewView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            _service = ServiceLocator.AppRemovalReviewService;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // 1. Header
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            _btnBack = new Button
            {
                Text = L.Get("appAnalyzer.back"),
                Location = new Point(40, 20),
                Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Cursor = Cursors.Hand
            };
            _btnBack.FlatAppearance.BorderSize = 0;
            _btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);
            _headerPanel.Controls.Add(_btnBack);

            _lblTitle = new Label
            {
                Text = L.Get("advanced.appRemoval.title"),
                Location = new Point(135, 16),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = L.Get("advanced.appRemoval.subtitle"),
                Location = new Point(140, 56),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = Color.DarkGray
            };
            _headerPanel.Controls.Add(_lblSubtitle);

            // 2. Warning Banner
            Panel warningContainer = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.Transparent };
            _warningBanner = new Panel
            {
                Location = new Point(40, 0),
                Height = 32,
                BackColor = Color.FromArgb(50, 40, 10)
            };
            _lblWarningText = new Label
            {
                Text = L.Get("appRemoval.readonly"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.Orange,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            _warningBanner.Controls.Add(_lblWarningText);
            warningContainer.Controls.Add(_warningBanner);

            // 3. Visual Health Dashboard
            Panel dashboardContainer = new Panel { Dock = DockStyle.Top, Height = 180, BackColor = Color.Transparent };
            _dashboardPanel = new TableLayoutPanel
            {
                Location = new Point(40, 0),
                Height = 160,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            _dashboardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            _dashboardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            // Left Zone: Health Meter
            _healthMeterControl = new AppRemovalGaugeControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0)
            };
            _dashboardPanel.Controls.Add(_healthMeterControl, 0, 0);

            // Right Zone: 2x2 Stats Grid
            _statsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                RowCount = 2,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _statsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _lblTotalVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 25) };
            _lblReviewVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.Orange, AutoSize = true, Location = new Point(15, 25) };
            _lblUnknownVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.DarkGray, AutoSize = true, Location = new Point(15, 25) };
            _lblLastScanVal = new Label { Text = L.Get("common.never"), Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };

            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("appRemoval.totalChecked"), _lblTotalVal, false), 0, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("appRemoval.reviewItems"), _lblReviewVal, false), 1, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("appRemoval.unknownPublisher"), _lblUnknownVal, true), 0, 1);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("appRemoval.lastScan"), _lblLastScanVal, true), 1, 1);

            _dashboardPanel.Controls.Add(_statsGrid, 1, 0);
            dashboardContainer.Controls.Add(_dashboardPanel);

            // 4. Toolbar & Filters
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            // Left Filters
            _btnFilterAll = CreateFilterButton(L.Get("system.filterAll", 0));
            _btnFilterAll.Location = new Point(40, 14);
            _btnFilterAll.Click += (s, e) => SetFilter("All");

            _btnFilterRisks = CreateFilterButton(L.Get("appRemoval.filterRisks", 0));
            _btnFilterRisks.Location = new Point(_btnFilterAll.Right + 10, 14);
            _btnFilterRisks.Click += (s, e) => SetFilter("Risks");

            _btnFilterLarge = CreateFilterButton(L.Get("appRemoval.filterSafe", 0));
            _btnFilterLarge.Location = new Point(_btnFilterRisks.Right + 10, 14);
            _btnFilterLarge.Click += (s, e) => SetFilter("Large");

            _toolbarPanel.Controls.Add(_btnFilterAll);
            _toolbarPanel.Controls.Add(_btnFilterRisks);
            _toolbarPanel.Controls.Add(_btnFilterLarge);

            // Right Actions
            _btnOpenSettings = new Button
            {
                Text = L.Get("appRemoval.appsAndFeatures"),
                Size = new Size(180, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            _btnOpenSettings.FlatAppearance.BorderSize = 0;
            _btnOpenSettings.Click += BtnOpenSettings_Click;

            _btnCopyReport = new Button
            {
                Text = L.Get("common.copyReport"),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 40),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            _btnCopyReport.FlatAppearance.BorderSize = 0;
            _btnCopyReport.Click += BtnCopyReport_Click;

            _btnRefresh = new Button
            {
                Text = L.Get("common.refresh"),
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _toolbarPanel.Controls.Add(_btnOpenSettings);
            _toolbarPanel.Controls.Add(_btnCopyReport);
            _toolbarPanel.Controls.Add(_btnRefresh);

            // 5. Main Content Area (Clipping Container)
            _clippingContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _scrollArea = new Panel
            {
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            _clippingContainer.Controls.Add(_scrollArea);

            _flpItems = new FlowLayoutPanel
            {
                Location = new Point(40, 0),
                AutoScroll = false,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 30)
            };
            _scrollArea.Controls.Add(_flpItems);

            _customScrollBar = new ModernScrollBar(_scrollArea)
            {
                Width = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };
            _clippingContainer.Controls.Add(_customScrollBar);
            _customScrollBar.BringToFront();

            this.Controls.Add(_clippingContainer);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(dashboardContainer);
            this.Controls.Add(warningContainer);
            this.Controls.Add(_headerPanel);

            this.Resize += AppRemovalReviewView_Resize;
            UpdateFilterUI();
        }

        private Panel CreateCompactStatCard(string title, Label valueLabel, bool isBottomRow)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, isBottomRow ? 0 : 8),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                Location = new Point(15, 8),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);

            return card;
        }

        private Button CreateFilterButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(80, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(30, 30, 30),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }



        private void SetFilter(string filter)
        {
            _currentFilter = filter;
            UpdateFilterUI();
            RenderList();
        }

        private void UpdateFilterUI()
        {
            _btnFilterAll.BackColor = _currentFilter == "All" ? Color.FromArgb(80, 50, 150) : Color.FromArgb(30, 30, 30);
            _btnFilterRisks.BackColor = _currentFilter == "Risks" ? Color.FromArgb(80, 50, 150) : Color.FromArgb(30, 30, 30);
            _btnFilterLarge.BackColor = _currentFilter == "Large" ? Color.FromArgb(80, 50, 150) : Color.FromArgb(30, 30, 30);

            _btnFilterAll.ForeColor = _currentFilter == "All" ? Color.White : Color.LightGray;
            _btnFilterRisks.ForeColor = _currentFilter == "Risks" ? Color.White : Color.LightGray;
            _btnFilterLarge.ForeColor = _currentFilter == "Large" ? Color.White : Color.LightGray;
        }

        private void AppRemovalReviewView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null) return;
            
            int safeWidth = this.Width - 80; // 40px padding each side
            if (safeWidth < 500) safeWidth = 500;

            _warningBanner.Width = safeWidth;
            _dashboardPanel.Width = safeWidth;

            _btnOpenSettings.Location = new Point(40 + safeWidth - _btnOpenSettings.Width, 14);
            _btnCopyReport.Location = new Point(_btnOpenSettings.Left - _btnCopyReport.Width - 10, 14);
            _btnRefresh.Location = new Point(_btnCopyReport.Left - _btnRefresh.Width - 10, 14);

            _scrollArea.Location = new Point(0, 0);
            _scrollArea.Size = new Size(_clippingContainer.Width + 25, _clippingContainer.Height); // hide native scrollbar
            
            _customScrollBar.Location = new Point(_clippingContainer.Width - 10, 0);
            _customScrollBar.Height = _clippingContainer.Height;

            int panelWidth = safeWidth;
            foreach (Control c in _flpItems.Controls)
            {
                c.Width = panelWidth;
            }

            _customScrollBar.UpdateScrollParams();
        }

        public async System.Threading.Tasks.Task LoadDataAsync()
        {
            if (this.IsDisposed || this.Disposing) return;

            _btnRefresh.Enabled = false;
            
            try
            {
                _latestResult = await _service.ScanAsync();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                    _btnRefresh.Enabled = true;
            }
            
            if (this.IsDisposed || this.Disposing) return;

            // Update stats
            _lblTotalVal.Text = _latestResult.TotalItems.ToString();
            _lblReviewVal.Text = _latestResult.ReviewCount.ToString();
_lblUnknownVal.Text = _latestResult.UnknownPublisherCount.ToString();

            _lblUnknownVal.Text = _latestResult.UnknownCount.ToString();
            _lblLastScanVal.Text = _latestResult.ScanTime.ToString("M/d/yy h:mm tt");

            // Update Health Meter
            if (_latestResult != null)
            {
                _healthMeterControl.TotalItems = _latestResult.TotalItems;
                _healthMeterControl.ReviewItems = _latestResult.ReviewCount;
                _healthMeterControl.UnknownItems = _latestResult.UnknownCount;
            }
            _healthMeterControl.Invalidate();

            // Update Filter Tab counts
            int riskCount = _latestResult.Items.Count(x => x.RiskLevel == AppRiskLevel.Review || x.RiskLevel == AppRiskLevel.Advanced || x.CurrentStatus == AppItemStatus.Unknown || x.CurrentStatus == AppItemStatus.Unsupported);
            int safeCount = _latestResult.Items.Count(x => x.RiskLevel == AppRiskLevel.Safe && x.CurrentStatus != AppItemStatus.Unknown && x.CurrentStatus != AppItemStatus.Unsupported);

            _btnFilterAll.Text = L.Get("system.filterAll", _latestResult.TotalItems);
            _btnFilterRisks.Text = L.Get("appRemoval.filterRisks", riskCount);
            _btnFilterLarge.Text = L.Get("appRemoval.filterSafe", safeCount);

            RenderList();
        }

        private void RenderList()
        {
            _scrollArea.AutoScrollPosition = new Point(0, 0);

            ClearAndDisposeControls(_flpItems);

            if (_latestResult?.Items != null)
            {
                var filteredItems = _latestResult.Items.Where(x => 
                {
                    if (_currentFilter == "Risks")
                        return x.RiskLevel == AppRiskLevel.Review || x.RiskLevel == AppRiskLevel.Advanced || x.CurrentStatus == AppItemStatus.Unknown || x.CurrentStatus == AppItemStatus.Unsupported;
                    if (_currentFilter == "Large")
                        return x.RiskLevel == AppRiskLevel.Safe && x.CurrentStatus != AppItemStatus.Unknown && x.CurrentStatus != AppItemStatus.Unsupported;
                    return true;
                });

                foreach (var item in filteredItems)
                {
                    var card = CreateItemCard(item);
                    _flpItems.Controls.Add(card);
                }
            }

            AppRemovalReviewView_Resize(this, EventArgs.Empty);
        }

        private Panel CreateItemCard(AppRemovalItem item)
        {
            Panel card = new Panel
            {
                Height = 85,
                BackColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            // Subtle border
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(50, 50, 50), ButtonBorderStyle.Solid);
            };

            // Left vertical accent bar
            Color statusColor = item.RiskLevel == AppRiskLevel.Safe ? Color.FromArgb(48, 209, 88) : 
                                item.RiskLevel == AppRiskLevel.Review ? Color.Orange : Color.DarkGray;
            
            Panel leftAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = statusColor
            };
            card.Controls.Add(leftAccent);

            // Icon Extraction & Display
            Image appIcon = SystemIcons.Application.ToBitmap();
            if (!string.IsNullOrWhiteSpace(item.DisplayIconPath))
            {
                if (_iconCache.TryGetValue(item.DisplayIconPath, out var cachedImg))
                {
                    appIcon = cachedImg;
                }
                else if (File.Exists(item.DisplayIconPath))
                {
                    try
                    {
                        var extracted = Icon.ExtractAssociatedIcon(item.DisplayIconPath);
                        if (extracted != null)
                        {
                            var bmp = extracted.ToBitmap();
                            _iconCache[item.DisplayIconPath] = bmp;
                            appIcon = bmp;
                            extracted.Dispose();
                        }
                    }
                    catch { }
                }
            }

            PictureBox picIcon = new PictureBox
            {
                Image = appIcon,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(15, 20),
                Size = new Size(32, 32),
                BackColor = Color.Transparent
            };
            card.Controls.Add(picIcon);

            // Center details
            Label lblName = new Label
            {
                Text = item.Name,
                Font = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                Location = new Point(55, 15),
                AutoSize = true
            };
            card.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = item.Description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.LightGray,
                Location = new Point(55, 38),
                AutoSize = true
            };
            card.Controls.Add(lblDesc);

            Label lblRec = new Label
            {
                Text = L.Get("appRemoval.recommended", item.RecommendedStatus),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                Location = new Point(55, 58),
                AutoSize = true
            };
            card.Controls.Add(lblRec);

            // Right status pill
            Label lblStatusBadge = new Label
            {
                Text = L.Get($"common.status.{item.CurrentStatus.ToString().ToLower()}"),
                Font = new Font("Segoe UI Semibold", 9F),
                ForeColor = statusColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };
            card.Controls.Add(lblStatusBadge);

            // Details Toggle
            Button btnDetails = new Button
            {
                Text = L.Get("common.details") + " \u25BC",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.DarkGray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            btnDetails.FlatAppearance.BorderSize = 0;
            card.Controls.Add(btnDetails);

            // Details Panel (Hidden by default)
            Panel detailsPanel = new Panel
            {
                Location = new Point(55, 85),
                Width = 500, // will stretch on resize
                Height = 80,
                BackColor = Color.FromArgb(25, 25, 25),
                Visible = false
            };

            Label lblDetailsContent = new Label
            {
                Text = L.Get("appRemoval.detailsFormat", item.Publisher, item.DisplayVersion, item.InstallDate, item.Category, item.Source) + $"\nDisplayIcon: {item.DisplayIconPath}",
                Font = new Font("Consolas", 9F),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            detailsPanel.Controls.Add(lblDetailsContent);
            card.Controls.Add(detailsPanel);

            btnDetails.Click += (s, e) =>
            {
                if (detailsPanel.Visible)
                {
                    detailsPanel.Visible = false;
                    card.Height = 85;
                    btnDetails.Text = L.Get("common.details") + " \u25BC";
                }
                else
                {
                    detailsPanel.Visible = true;
                    card.Height = 85 + detailsPanel.Height + 10;
                    btnDetails.Text = L.Get("common.details") + " \u25B2";
                }
                AppRemovalReviewView_Resize(this, EventArgs.Empty);
            };

            card.Resize += (s, e) =>
            {
                lblStatusBadge.Location = new Point(card.Width - lblStatusBadge.Width - 20, 15);
                btnDetails.Location = new Point(card.Width - btnDetails.Width - 20, 45);
                detailsPanel.Width = card.Width - 75; // 55 padding + 20 right
            };

            return card;
        }

        private void BtnCopyReport_Click(object sender, EventArgs e)
        {
            if (_latestResult == null) return;
            string report = _service.BuildReport(_latestResult);
            Clipboard.SetText(report);
            MessageBox.Show(L.Get("appRemoval.reportCopied"), "ApexBoost", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnOpenSettings_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:privacy") { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(L.Get("appRemoval.unableToOpenSettings"), "ApexBoost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearAndDisposeControls(Control container)
        {
            for (int i = container.Controls.Count - 1; i >= 0; i--)
            {
                var c = container.Controls[i];
                container.Controls.RemoveAt(i);
                c.Dispose();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var img in _iconCache.Values)
                {
                    img?.Dispose();
                }
                _iconCache.Clear();
            }
            base.Dispose(disposing);
        }

        private class AppRemovalGaugeControl : Control
        {
            public int TotalItems { get; set; } = 0;
            public int ReviewItems { get; set; } = 0;
            public int UnknownItems { get; set; } = 0;

            public AppRemovalGaugeControl()
            {
                this.DoubleBuffered = true;
                this.BackColor = Color.FromArgb(18, 18, 18);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Border
                using (var borderPen = new Pen(Color.FromArgb(45, 45, 45), 1))
                {
                    g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }

                int ringThickness = 12;
                int diameter = 96;
                int centerX = this.Width / 2;
                
                // Fixed top offset to ensure it doesn't get pushed down too far
                int ringTop = 16;
                int centerY = ringTop + (diameter / 2);

                Rectangle rect = new Rectangle(centerX - (diameter / 2), ringTop, diameter, diameter);

                // Background track
                using (var trackPen = new Pen(Color.FromArgb(35, 35, 35), ringThickness))
                {
                    g.DrawEllipse(trackPen, rect);
                }

                // Active arc and center text
                Color accentColor = Color.FromArgb(255, 165, 0); // Orange
                string centerText = "-";
                string mainLabel = L.Get("common.status.notScanned");
                string subLabel = "";

                if (TotalItems > 0)
                {
                    centerText = ReviewItems > 0 ? ReviewItems.ToString() : (UnknownItems > 0 ? "?" : "0");
                    
                    if (ReviewItems > 0)
                    {
                        accentColor = Color.FromArgb(255, 165, 0); // Orange
                        mainLabel = L.Get("appRemoval.gaugeLabel");
                        subLabel = L.Get("appRemoval.gaugeSubtitle");
                        
                        float ratio = (float)ReviewItems / TotalItems;
                        float sweepAngle = 360f * ratio;
                        
                        using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            g.DrawArc(arcPen, rect, -90, sweepAngle);
                        }
                    }
                    else if (UnknownItems > 0)
                    {
                        accentColor = Color.FromArgb(200, 200, 50); // Muted amber
                        mainLabel = L.Get("system.reviewNeeded");
                        subLabel = L.Get("appRemoval.unknownPublisher").ToUpper();
                        
                        using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            g.DrawArc(arcPen, rect, -90, 360);
                        }
                    }
                    else
                    {
                        accentColor = Color.FromArgb(48, 209, 88); // Green
                        mainLabel = L.Get("common.status.allClear");
                        subLabel = L.Get("system.noActionNeeded");
                        
                        using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            g.DrawArc(arcPen, rect, -90, 360);
                        }
                    }
                }

                // Draw center number
                using (var centerFont = new Font("Segoe UI", 24, FontStyle.Bold))
                using (var centerBrush = new SolidBrush(accentColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(centerText, centerFont, centerBrush, rect, sf);
                }

                // Draw labels below ring
                int labelY = ringTop + diameter + 10;
                
                using (var mainFont = new Font("Segoe UI", 10, FontStyle.Regular))
                using (var mainBrush = new SolidBrush(Color.White))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(mainLabel, mainFont, mainBrush, new Point(centerX, labelY), sf);
                }

                if (!string.IsNullOrEmpty(subLabel))
                {
                    using (var subFont = new Font("Segoe UI", 8, FontStyle.Bold))
                    using (var subBrush = new SolidBrush(accentColor))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                    {
                        g.DrawString(subLabel, subFont, subBrush, new Point(centerX, labelY + 20), sf);
                    }
                }
            }
        }
    }
}






