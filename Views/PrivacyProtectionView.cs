using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Domain.Interfaces;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class PrivacyProtectionView : UserControl
    {
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Button _btnBack;
        private Panel _warningBanner;
        private Label _lblWarningText;

        // Health Dashboard
        private TableLayoutPanel _dashboardPanel;
        private PrivacyHealthGaugeControl _healthMeterControl;

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
        private Button _btnFilterSafe;
        private Button _btnRefresh;
        private Button _btnCopyReport;
        private Button _btnOpenSettings;

        // Actions
        private Panel _actionsPanel;
        private Button _btnPreviewChanges;
        private Button _btnRestoreBackup;

        // List
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpItems;
        private ModernScrollBar _customScrollBar;

        private IPrivacyProtectionService _service;
        private PrivacyScanResult _latestResult;

        private string _currentFilter = "All";

        public event EventHandler BackRequested;

        public PrivacyProtectionView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            _service = ServiceLocator.PrivacyProtectionService;

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
                Text = L.Get("advanced.privacy.title"),
                Location = new Point(135, 16),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = L.Get("advanced.privacy.subtitle"),
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
                Text = L.Get("privacy.readonly"),
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
            _healthMeterControl = new PrivacyHealthGaugeControl
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
            _lblLastScanVal = new Label { Text = "Never", Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };

            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("privacy.totalChecked"), _lblTotalVal, false), 0, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("privacy.reviewItems"), _lblReviewVal, false), 1, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("privacy.unknownItems"), _lblUnknownVal, true), 0, 1);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("privacy.lastScan"), _lblLastScanVal, true), 1, 1);

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
            _btnFilterAll = CreateFilterButton(L.Get("privacy.filterAll", 0));
            _btnFilterAll.Location = new Point(40, 14);
            _btnFilterAll.Click += (s, e) => SetFilter("All");

            _btnFilterRisks = CreateFilterButton(L.Get("privacy.filterRisks", 0));
            _btnFilterRisks.Location = new Point(_btnFilterAll.Right + 10, 14);
            _btnFilterRisks.Click += (s, e) => SetFilter("Risks");

            _btnFilterSafe = CreateFilterButton(L.Get("privacy.filterSafe", 0));
            _btnFilterSafe.Location = new Point(_btnFilterRisks.Right + 10, 14);
            _btnFilterSafe.Click += (s, e) => SetFilter("Safe");

            _toolbarPanel.Controls.Add(_btnFilterAll);
            _toolbarPanel.Controls.Add(_btnFilterRisks);
            _toolbarPanel.Controls.Add(_btnFilterSafe);

            // Right Actions
            _btnOpenSettings = new Button
            {
                Text = L.Get("privacy.openSettings"),
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

            // 4.5 Actions Panel
            _actionsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            _btnPreviewChanges = new Button
            {
                Text = L.Get("common.previewChanges"),
                Size = new Size(160, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 50, 150),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            _btnPreviewChanges.FlatAppearance.BorderSize = 0;
            _btnPreviewChanges.Click += BtnPreviewChanges_Click;

            _btnRestoreBackup = new Button
            {
                Text = L.Get("privacy.restoreLast"),
                Size = new Size(200, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(180, 100, 0), // Amber caution
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9F)
            };
            _btnRestoreBackup.FlatAppearance.BorderSize = 0;
            _btnRestoreBackup.Click += BtnRestoreBackup_Click;

            _actionsPanel.Controls.Add(_btnPreviewChanges);
            _actionsPanel.Controls.Add(_btnRestoreBackup);

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
            this.Controls.Add(_actionsPanel);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(dashboardContainer);
            this.Controls.Add(warningContainer);
            this.Controls.Add(_headerPanel);

            this.Resize += PrivacyProtectionView_Resize;
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
            _btnFilterSafe.BackColor = _currentFilter == "Safe" ? Color.FromArgb(80, 50, 150) : Color.FromArgb(30, 30, 30);

            _btnFilterAll.ForeColor = _currentFilter == "All" ? Color.White : Color.LightGray;
            _btnFilterRisks.ForeColor = _currentFilter == "Risks" ? Color.White : Color.LightGray;
            _btnFilterSafe.ForeColor = _currentFilter == "Safe" ? Color.White : Color.LightGray;
        }

        private void PrivacyProtectionView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null) return;
            
            int safeWidth = this.Width - 80; // 40px padding each side
            if (safeWidth < 500) safeWidth = 500;

            _warningBanner.Width = safeWidth;
            _dashboardPanel.Width = safeWidth;

            _btnOpenSettings.Location = new Point(40 + safeWidth - _btnOpenSettings.Width, 14);
            _btnCopyReport.Location = new Point(_btnOpenSettings.Left - _btnCopyReport.Width - 10, 14);
            _btnRefresh.Location = new Point(_btnCopyReport.Left - _btnRefresh.Width - 10, 14);

            _btnPreviewChanges.Location = new Point(40 + safeWidth - _btnPreviewChanges.Width, 14);
            _btnRestoreBackup.Location = new Point(_btnPreviewChanges.Left - _btnRestoreBackup.Width - 10, 14);

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
            int riskCount = _latestResult.Items.Count(x => x.RiskLevel == PrivacyRiskLevel.Review || x.RiskLevel == PrivacyRiskLevel.Advanced || x.CurrentStatus == PrivacySettingStatus.Unknown || x.CurrentStatus == PrivacySettingStatus.Unsupported);
            int safeCount = _latestResult.Items.Count(x => x.RiskLevel == PrivacyRiskLevel.Safe && x.CurrentStatus != PrivacySettingStatus.Unknown && x.CurrentStatus != PrivacySettingStatus.Unsupported);

            _btnFilterAll.Text = L.Get("privacy.filterAll", _latestResult.TotalItems);
            _btnFilterRisks.Text = L.Get("privacy.filterRisks", riskCount);
            _btnFilterSafe.Text = L.Get("privacy.filterSafe", safeCount);

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
                        return x.RiskLevel == PrivacyRiskLevel.Review || x.RiskLevel == PrivacyRiskLevel.Advanced || x.CurrentStatus == PrivacySettingStatus.Unknown || x.CurrentStatus == PrivacySettingStatus.Unsupported;
                    if (_currentFilter == "Safe")
                        return x.RiskLevel == PrivacyRiskLevel.Safe && x.CurrentStatus != PrivacySettingStatus.Unknown && x.CurrentStatus != PrivacySettingStatus.Unsupported;
                    return true;
                });

                foreach (var item in filteredItems)
                {
                    var card = CreateItemCard(item);
                    _flpItems.Controls.Add(card);
                }
            }

            PrivacyProtectionView_Resize(this, EventArgs.Empty);
        }

        private Panel CreateItemCard(PrivacySettingItem item)
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
            Color statusColor = item.RiskLevel == PrivacyRiskLevel.Safe ? Color.FromArgb(48, 209, 88) : 
                                item.RiskLevel == PrivacyRiskLevel.Review ? Color.Orange : Color.DarkGray;
            
            Panel leftAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = statusColor
            };
            card.Controls.Add(leftAccent);

            // Icon Badge
            Label lblIcon = new Label
            {
                Text = GetCategoryIcon(item.Category),
                Font = new Font("Segoe UI", 16F),
                ForeColor = Color.DarkGray,
                Location = new Point(15, 25),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

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
                Text = L.Get("privacy.recommended", L.Get($"common.status.{item.RecommendedStatus.ToString().ToLower()}")),
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
                Text = L.Get("common.details") + " ▼",
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
                Text = $"Source: {item.Source}\nRaw Value: {item.RawValue}\nNotes: {item.Notes}",
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
                    btnDetails.Text = L.Get("common.details") + " ▼";
                }
                else
                {
                    detailsPanel.Visible = true;
                    card.Height = 85 + detailsPanel.Height + 10;
                    btnDetails.Text = L.Get("common.details") + " ▲";
                }
                PrivacyProtectionView_Resize(this, EventArgs.Empty);
            };

            card.Resize += (s, e) =>
            {
                lblStatusBadge.Location = new Point(card.Width - lblStatusBadge.Width - 20, 15);
                btnDetails.Location = new Point(card.Width - btnDetails.Width - 20, 45);
                detailsPanel.Width = card.Width - 75; // 55 padding + 20 right
            };

            return card;
        }

        private string GetCategoryIcon(string category)
        {
            if (category.Contains("Advertising") || category.Contains("Personalization")) return "ID";
            if (category.Contains("Diagnostics") || category.Contains("Telemetry")) return "DX";
            if (category.Contains("Activity") || category.Contains("History")) return "HI";
            if (category.Contains("App Permissions")) return "AP";
            if (category.Contains("Suggestions")) return "SG";
            return "PV";
        }

        private async void BtnPreviewChanges_Click(object sender, EventArgs e)
        {
            if (_latestResult == null) return;
            var items = _service.BuildApplyPreview(_latestResult).ToList();
            if (items.Count == 0)
            {
                MessageBox.Show(L.Get("privacy.noChanges"), L.Get("common.previewChanges"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = L.Get("privacy.previewChangesTitle");
                dlg.Size = new Size(700, 500);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(30, 30, 30);
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9F);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                var lblInfo = new Label
                {
                    Text = L.Get("privacy.previewInfo"),
                    Dock = DockStyle.Top,
                    Height = 55,
                    Padding = new Padding(10),
                    ForeColor = Color.LightSkyBlue
                };
                dlg.Controls.Add(lblInfo);

                var clb = new CheckedListBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(40, 40, 40),
                    ForeColor = Color.White,
                    CheckOnClick = true,
                    FormattingEnabled = true
                };

                foreach (var item in items)
                {
                    clb.Items.Add($"{item.Title} (Target: {item.ProposedValue})", true);
                }

                dlg.Controls.Add(clb);

                var panel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
                var btnApply = new Button { Text = L.Get("common.applySelected"), DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 120, BackColor = Color.FromArgb(80, 50, 150), FlatStyle = FlatStyle.Flat };
                var btnCancel = new Button { Text = L.Get("common.cancel"), DialogResult = DialogResult.Cancel, Dock = DockStyle.Right, Width = 100, BackColor = Color.FromArgb(60, 60, 60), FlatStyle = FlatStyle.Flat };
                panel.Controls.Add(btnApply);
                panel.Controls.Add(btnCancel);
                dlg.Controls.Add(panel);

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var selectedItems = items.Where((item, index) => clb.GetItemChecked(index)).ToList();
                    if (selectedItems.Any())
                    {
                        var backupService = ServiceLocator.ChangeBackupService;
                        var execService = ServiceLocator.ChangeExecutionService;

                        var backup = await backupService.CreateBackupPackageAsync(L.Get("advanced.privacy.title"), selectedItems, "Privacy Apply v1");
                        var result = await execService.ExecuteAsync(L.Get("advanced.privacy.title"), selectedItems);

                        MessageBox.Show(L.Get("common.execComplete", result.HasFailures ? L.Get("common.execError") : L.Get("common.execSuccess"), result.LogPath), L.Get("common.applySelected"), MessageBoxButtons.OK, result.HasFailures ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                        _lblWarningText.Text = L.Get("privacy.reviewModeAvailable");
                        await LoadDataAsync();
                    }
                }
            }
        }

        private async void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            var backupService = ServiceLocator.ChangeBackupService;
            var packages = await backupService.GetBackupPackagesAsync(L.Get("advanced.privacy.title"));
            var latest = packages.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            if (latest == null)
            {
                MessageBox.Show(L.Get("privacy.noBackupFound"), L.Get("common.restore"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(L.Get("privacy.confirmRestore", latest.CreatedAt.ToLocalTime()), L.Get("common.confirmRestore"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var execService = ServiceLocator.ChangeExecutionService;
                var result = await execService.RestoreAsync(latest);

                MessageBox.Show(L.Get("common.restoreComplete", result.HasFailures ? L.Get("common.execError") : L.Get("common.execSuccess"), result.LogPath), L.Get("common.restoreChanges"), MessageBoxButtons.OK, result.HasFailures ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                await LoadDataAsync();
            }
        }

        private void BtnCopyReport_Click(object sender, EventArgs e)
        {
            if (_latestResult == null) return;
            string report = _service.BuildReport(_latestResult);
            Clipboard.SetText(report);
            MessageBox.Show(L.Get("privacy.reportCopied"), "ApexBoost", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnOpenSettings_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:privacy") { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(L.Get("dialog.privacySettingsError"), "ApexBoost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private class PrivacyHealthGaugeControl : Control
        {
            public int TotalItems { get; set; } = 0;
            public int ReviewItems { get; set; } = 0;
            public int UnknownItems { get; set; } = 0;

            public PrivacyHealthGaugeControl()
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
                        mainLabel = L.Get("privacy.issuesDetected");
                        subLabel = L.Get("privacy.actionRecommended");
                        
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
                        mainLabel = L.Get("privacy.reviewNeeded");
                        subLabel = L.Get("privacy.unknownItemsFound");
                        
                        using (var arcPen = new Pen(accentColor, ringThickness) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            g.DrawArc(arcPen, rect, -90, 360);
                        }
                    }
                    else
                    {
                        accentColor = Color.FromArgb(48, 209, 88); // Green
                        mainLabel = L.Get("privacy.allClear");
                        subLabel = L.Get("privacy.noActionNeeded");
                        
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

