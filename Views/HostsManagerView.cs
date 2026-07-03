using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class HostsManagerView : UserControl
    {
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Button _btnBack;
        private Panel _warningBanner;
        private Label _lblWarningText;

        // Health Dashboard
        private TableLayoutPanel _dashboardPanel;
        private HostsGaugeControl _healthMeterControl;

        // Stats Grid
        private TableLayoutPanel _statsGrid;
        private Label _lblTotalVal;
        private Label _lblEnabledVal;
        private Label _lblDisabledVal;
        private Label _lblBackupsVal;

        // Toolbar & Filters
        private Panel _toolbarPanel;
        private Button _btnFilterAll;
        private Button _btnFilterEnabled;
        private Button _btnFilterDisabled;
        private Button _btnFilterCustom;
        private Button _btnFilterSystem;
        private Button _btnFilterReview;

        private Button _btnRefresh;
        private Button _btnAddEntry;
        private Button _btnCreateBackup;
        private Button _btnRestoreBackup;
        private Button _btnOpenFolder;

        // Search
        private TextBox _txtSearch;

        // List
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpItems;
        private ModernScrollBar _customScrollBar;

        private HostsParseResult _currentResult;
        private string _currentFilter = "All";
        private string _searchQuery = "";

        public event EventHandler BackRequested;

        public HostsManagerView()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.DoubleBuffered = true;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

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
                Text = L.Get("hosts.title"),
                Location = new Point(135, 16),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = L.Get("hosts.subtitle"),
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
                BackColor = Color.FromArgb(10, 40, 50)
            };
            _lblWarningText = new Label
            {
                Text = L.Get("hosts.readonly"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.LightSkyBlue,
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
            _healthMeterControl = new HostsGaugeControl
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
            _lblEnabledVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.FromArgb(48, 209, 88), AutoSize = true, Location = new Point(15, 25) };
            _lblDisabledVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.Orange, AutoSize = true, Location = new Point(15, 25) };
            _lblBackupsVal = new Label { Text = "0", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };

            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("hosts.totalEntries"), _lblTotalVal, false), 0, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("common.enabled"), _lblEnabledVal, false), 1, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("common.disabled"), _lblDisabledVal, true), 0, 1);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("hosts.backups"), _lblBackupsVal, true), 1, 1);

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
            _btnFilterAll = CreateFilterButton(L.Get("common.all"));
            _btnFilterAll.Location = new Point(40, 14);
            _btnFilterAll.Click += (s, e) => SetFilter("All");

            _btnFilterEnabled = CreateFilterButton(L.Get("common.enabled"));
            _btnFilterEnabled.Location = new Point(_btnFilterAll.Right + 5, 14);
            _btnFilterEnabled.Click += (s, e) => SetFilter("Enabled");

            _btnFilterDisabled = CreateFilterButton(L.Get("common.disabled"));
            _btnFilterDisabled.Location = new Point(_btnFilterEnabled.Right + 5, 14);
            _btnFilterDisabled.Click += (s, e) => SetFilter("Disabled");

            _btnFilterCustom = CreateFilterButton(L.Get("hosts.custom"));
            _btnFilterCustom.Location = new Point(_btnFilterDisabled.Right + 5, 14);
            _btnFilterCustom.Click += (s, e) => SetFilter("Custom");

            _btnFilterSystem = CreateFilterButton(L.Get("hosts.system"));
            _btnFilterSystem.Location = new Point(_btnFilterCustom.Right + 5, 14);
            _btnFilterSystem.Click += (s, e) => SetFilter("System");

            _btnFilterReview = CreateFilterButton(L.Get("common.status.review"));
            _btnFilterReview.Location = new Point(_btnFilterSystem.Right + 5, 14);
            _btnFilterReview.Click += (s, e) => SetFilter("Review");

            _toolbarPanel.Controls.Add(_btnFilterAll);
            _toolbarPanel.Controls.Add(_btnFilterEnabled);
            _toolbarPanel.Controls.Add(_btnFilterDisabled);
            _toolbarPanel.Controls.Add(_btnFilterCustom);
            _toolbarPanel.Controls.Add(_btnFilterSystem);
            _toolbarPanel.Controls.Add(_btnFilterReview);

            // Search Box (Right side above toolbar buttons)
            _txtSearch = new TextBox
            {
                Text = L.Get("hosts.search"),
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Width = 200,
                Location = new Point(0, 17) // Position updated in resize
            };
            _txtSearch.Enter += (s, e) => { if (_txtSearch.Text == L.Get("hosts.search")) { _txtSearch.Text = ""; _txtSearch.ForeColor = Color.White; } };
            _txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearch.Text)) { _txtSearch.Text = L.Get("hosts.search"); _txtSearch.ForeColor = Color.Gray; } };
            _txtSearch.TextChanged += (s, e) => {
                if (_txtSearch.Text != L.Get("hosts.search"))
                {
                    _searchQuery = _txtSearch.Text;
                    RenderList();
                }
            };
            _toolbarPanel.Controls.Add(_txtSearch);

            // Additional Action Toolbar
            Panel actionToolbar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
            
            _btnRefresh = CreateActionButton(L.Get("common.refresh"), false);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _btnOpenFolder = CreateActionButton(L.Get("hosts.openFolder"), false);
            _btnOpenFolder.Click += BtnOpenFolder_Click;

            _btnRestoreBackup = CreateActionButton(L.Get("hosts.restoreSelected"), false); // Or common.restoreBackup
            _btnRestoreBackup.Click += BtnRestoreBackup_Click;

            _btnCreateBackup = CreateActionButton(L.Get("hosts.createBackup"), false);
            _btnCreateBackup.Click += BtnCreateBackup_Click;

            _btnAddEntry = CreateActionButton(L.Get("hosts.addEntry"), true);
            _btnAddEntry.Click += BtnAddEntry_Click;

            actionToolbar.Controls.Add(_btnRefresh);
            actionToolbar.Controls.Add(_btnOpenFolder);
            actionToolbar.Controls.Add(_btnRestoreBackup);
            actionToolbar.Controls.Add(_btnCreateBackup);
            actionToolbar.Controls.Add(_btnAddEntry);

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
            this.Controls.Add(actionToolbar);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(dashboardContainer);
            this.Controls.Add(warningContainer);
            this.Controls.Add(_headerPanel);

            this.Resize += HostsManagerView_Resize;
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
                MinimumSize = new Size(60, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(30, 30, 30),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Button CreateActionButton(string text, bool isPrimary)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = isPrimary ? Color.FromArgb(148, 146, 230) : Color.FromArgb(40, 40, 40),
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
            var activeColor = Color.FromArgb(80, 50, 150);
            var inactiveColor = Color.FromArgb(30, 30, 30);

            _btnFilterAll.BackColor = _currentFilter == "All" ? activeColor : inactiveColor;
            _btnFilterEnabled.BackColor = _currentFilter == "Enabled" ? activeColor : inactiveColor;
            _btnFilterDisabled.BackColor = _currentFilter == "Disabled" ? activeColor : inactiveColor;
            _btnFilterCustom.BackColor = _currentFilter == "Custom" ? activeColor : inactiveColor;
            _btnFilterSystem.BackColor = _currentFilter == "System" ? activeColor : inactiveColor;
            _btnFilterReview.BackColor = _currentFilter == "Review" ? activeColor : inactiveColor;
        }

        private void HostsManagerView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null) return;
            
            int safeWidth = this.Width - 80;
            if (safeWidth < 500) safeWidth = 500;

            _warningBanner.Width = safeWidth;
            _dashboardPanel.Width = safeWidth;

            // Search Box Right Align
            _txtSearch.Location = new Point(40 + safeWidth - _txtSearch.Width, 14);

            // Action Toolbar Right Align
            _btnAddEntry.Location = new Point(40 + safeWidth - _btnAddEntry.Width, 10);
            _btnCreateBackup.Location = new Point(_btnAddEntry.Left - _btnCreateBackup.Width - 10, 10);
            _btnRestoreBackup.Location = new Point(_btnCreateBackup.Left - _btnRestoreBackup.Width - 10, 10);
            _btnOpenFolder.Location = new Point(_btnRestoreBackup.Left - _btnOpenFolder.Width - 10, 10);
            _btnRefresh.Location = new Point(_btnOpenFolder.Left - _btnRefresh.Width - 10, 10);

            _scrollArea.Location = new Point(0, 0);
            _scrollArea.Size = new Size(_clippingContainer.Width + 25, _clippingContainer.Height);
            
            _customScrollBar.Location = new Point(_clippingContainer.Width - 10, 0);
            _customScrollBar.Height = _clippingContainer.Height;

            int panelWidth = safeWidth;
            foreach (Control c in _flpItems.Controls)
            {
                c.Width = panelWidth;
                foreach (Control sub in c.Controls)
                {
                    if (sub.Name == "ActionsPanel")
                    {
                        sub.Location = new Point(panelWidth - sub.Width - 10, 15);
                    }
                }
            }

            _customScrollBar.UpdateScrollParams();
        }

        public async void LoadData()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (this.IsDisposed || this.Disposing) return;

            bool isWritable = ServiceLocator.HostsManagerService.IsWritable();
            _btnAddEntry.Enabled = isWritable;
            _btnRestoreBackup.Enabled = isWritable;

            _btnRefresh.Enabled = false;
            
            try
            {
                _currentResult = await ServiceLocator.HostsManagerService.LoadHostsFileAsync();
                var backups = await ServiceLocator.HostsManagerService.GetBackupsAsync();
                
                if (this.IsDisposed || this.Disposing) return;
                
                int customEntries = _currentResult.Entries.Count(x => !x.IsBlankOrCommentOnly && !x.Domain.Contains("localhost"));
                int enabled = _currentResult.Entries.Count(x => !x.IsBlankOrCommentOnly && x.IsEnabled);
                int disabled = _currentResult.Entries.Count(x => !x.IsBlankOrCommentOnly && !x.IsEnabled);

                _lblTotalVal.Text = customEntries.ToString();
                _lblEnabledVal.Text = enabled.ToString();
                _lblDisabledVal.Text = disabled.ToString();
                _lblBackupsVal.Text = backups.Count.ToString();

                _healthMeterControl.TotalEntries = customEntries;
                _healthMeterControl.EnabledEntries = enabled;
                _healthMeterControl.Invalidate();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                    _btnRefresh.Enabled = true;
            }
            
            RenderList();
        }

        private void RenderList()
        {
            _scrollArea.AutoScrollPosition = new Point(0, 0);
            ClearAndDisposeControls(_flpItems);

            if (_currentResult?.Entries != null)
            {
                var filteredItems = _currentResult.Entries.Where(x => !x.IsBlankOrCommentOnly).Where(x => 
                {
                    bool match = true;
                    if (_currentFilter == "Enabled") match = x.IsEnabled;
                    else if (_currentFilter == "Disabled") match = !x.IsEnabled;
                    else if (_currentFilter == "Custom") match = !x.Domain.Contains("localhost");
                    else if (_currentFilter == "System") match = x.Domain.Contains("localhost");
                    else if (_currentFilter == "Review") match = !x.Domain.Contains("localhost"); 

                    if (match && !string.IsNullOrWhiteSpace(_searchQuery))
                    {
                        match = x.Domain.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 || 
                                x.IPAddress.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    return match;
                });

                bool isWritable = ServiceLocator.HostsManagerService.IsWritable();

                foreach (var entry in filteredItems)
                {
                    var card = CreateItemCard(entry, isWritable);
                    _flpItems.Controls.Add(card);
                }
            }

            HostsManagerView_Resize(this, EventArgs.Empty);
        }

        private Panel CreateItemCard(HostsFileEntry entry, bool isWritable)
        {
            Panel card = new Panel
            {
                Height = 65,
                BackColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            // Subtle border
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(50, 50, 50), ButtonBorderStyle.Solid);
            };

            // Left vertical accent
            Color statusColor = entry.IsEnabled ? Color.FromArgb(48, 209, 88) : Color.Orange;
            Panel leftAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = statusColor
            };
            card.Controls.Add(leftAccent);

            // Icon/Badge
            Label lblBadge = new Label
            {
                Text = entry.IsEnabled ? L.Get("common.enabled") : L.Get("common.disabled"),
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = statusColor,
                AutoSize = true,
                Location = new Point(20, 25),
                Padding = new Padding(2)
            };
            card.Controls.Add(lblBadge);

            // Content
            Label lblDomain = new Label
            {
                Text = entry.Domain,
                Font = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(80, 12)
            };
            card.Controls.Add(lblDomain);

            Label lblIP = new Label
            {
                Text = entry.IPAddress,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                AutoSize = true,
                Location = new Point(80, 35)
            };
            card.Controls.Add(lblIP);

            // System Badge if localhost
            if (entry.Domain.Contains("localhost"))
            {
                Label lblSystem = new Label
                {
                    Text = L.Get("hosts.system"),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.LightSkyBlue,
                    AutoSize = true,
                    Location = new Point(lblDomain.Right + 10, 15)
                };
                card.Controls.Add(lblSystem);
            }

            // Actions panel (Right aligned)
            Panel actionsPanel = new Panel
            {
                Name = "ActionsPanel",
                Height = 35,
                Width = 250,
                BackColor = Color.Transparent
            };

            Button btnToggle = new Button
            {
                Text = entry.IsEnabled ? L.Get("common.disable") : L.Get("common.enable"),
                FlatStyle = FlatStyle.Flat,
                ForeColor = entry.IsEnabled ? Color.Orange : Color.FromArgb(48, 209, 88),
                BackColor = Color.FromArgb(40, 40, 40),
                Size = new Size(70, 26),
                Location = new Point(60, 5),
                Cursor = Cursors.Hand,
                Enabled = isWritable
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += async (s, e) => {
                await ToggleEntry(entry);
            };
            actionsPanel.Controls.Add(btnToggle);

            Button btnDelete = new Button
            {
                Text = L.Get("common.delete"),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.IndianRed,
                BackColor = Color.FromArgb(40, 40, 40),
                Size = new Size(60, 26),
                Location = new Point(140, 5),
                Cursor = Cursors.Hand,
                Enabled = isWritable
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += async (s, e) => {
                await DeleteEntry(entry);
            };
            actionsPanel.Controls.Add(btnDelete);

            Button btnDetails = new Button
            {
                Text = "▼",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(40, 40, 40),
                Size = new Size(30, 26),
                Location = new Point(210, 5),
                Cursor = Cursors.Hand
            };
            btnDetails.FlatAppearance.BorderSize = 0;
            
            Panel detailsPanel = CreateDetailsPanel(entry);
            detailsPanel.Visible = false;
            card.Controls.Add(detailsPanel);

            btnDetails.Click += (s, e) =>
            {
                detailsPanel.Visible = !detailsPanel.Visible;
                btnDetails.Text = detailsPanel.Visible ? "\u25B2" : "\u25BC";
                card.Height = detailsPanel.Visible ? 65 + detailsPanel.Height : 65;
            };
            actionsPanel.Controls.Add(btnDetails);

            card.Controls.Add(actionsPanel);
            
            return card;
        }

        private Panel CreateDetailsPanel(HostsFileEntry entry)
        {
            Panel details = new Panel
            {
                BackColor = Color.FromArgb(25, 25, 25),
                Location = new Point(4, 65),
                Width = 1000, 
                Height = 60,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            Label lblRaw = new Label
            {
                Text = L.Get("hosts.rawLine", entry.OriginalLine),
                Font = new Font("Consolas", 9F),
                ForeColor = Color.Gray,
                Location = new Point(15, 10),
                AutoSize = true
            };
            details.Controls.Add(lblRaw);

            Label lblComment = new Label
            {
                Text = L.Get("hosts.comment", string.IsNullOrWhiteSpace(entry.Comment) ? L.Get("common.none") : entry.Comment),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Location = new Point(15, 30),
                AutoSize = true
            };
            details.Controls.Add(lblComment);

            return details;
        }

        private async Task ToggleEntry(HostsFileEntry entry)
        {
            try
            {
                if (!ServiceLocator.HostsManagerService.IsWritable())
                {
                    MessageBox.Show(L.Get("hosts.notWritable"), L.Get("common.accessDenied"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                entry.IsEnabled = !entry.IsEnabled;
                await ServiceLocator.HostsManagerService.SaveHostsFileAsync(_currentResult.Entries);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, L.Get("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeleteEntry(HostsFileEntry entry)
        {
            try
            {
                if (!ServiceLocator.HostsManagerService.IsWritable())
                {
                    MessageBox.Show(L.Get("hosts.notWritable"), L.Get("common.accessDenied"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show(L.Get("hosts.confirmDelete"), L.Get("common.confirmDelete"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    _currentResult.Entries.Remove(entry);
                    await ServiceLocator.HostsManagerService.SaveHostsFileAsync(_currentResult.Entries);
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, L.Get("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCreateBackup_Click(object sender, EventArgs e)
        {
            var backup = await ServiceLocator.HostsManagerService.CreateBackupAsync(L.Get("hosts.manualBackup"));
            if (backup != null)
            {
                MessageBox.Show(L.Get("hosts.backupSuccess"), L.Get("common.success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(L.Get("hosts.backupFailed"), L.Get("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAddEntry_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ServiceLocator.HostsManagerService.IsWritable())
                {
                    MessageBox.Show(L.Get("hosts.notWritable"), L.Get("common.accessDenied"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var dialog = new AddHostsEntryDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_currentResult.Entries.Any(x => x.IPAddress == dialog.IPAddress && x.Domain == dialog.Domain))
                        {
                            MessageBox.Show(L.Get("hosts.entryExists"), L.Get("common.duplicate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        var newEntry = new HostsFileEntry
                        {
                            IPAddress = dialog.IPAddress,
                            Domain = dialog.Domain,
                            Comment = dialog.Comment,
                            IsEnabled = true,
                            
                        };

                        _currentResult.Entries.Add(newEntry);
                        await ServiceLocator.HostsManagerService.SaveHostsFileAsync(_currentResult.Entries);
                        await LoadDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, L.Get("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ServiceLocator.HostsManagerService.IsWritable())
                {
                    MessageBox.Show(L.Get("hosts.notWritable"), L.Get("common.accessDenied"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var backups = await ServiceLocator.HostsManagerService.GetBackupsAsync();
                if (!backups.Any())
                {
                    MessageBox.Show(L.Get("hosts.noBackups"), L.Get("common.info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var frm = new Form { Text = L.Get("hosts.selectBackup"), Size = new Size(400, 300), StartPosition = FormStartPosition.CenterParent, BackColor = Color.FromArgb(20,20,20), ForeColor = Color.White })
                {
                    ListBox lb = new ListBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(30,30,30), ForeColor = Color.White };
                    foreach(var b in backups) lb.Items.Add($"{b.CreatedAt:g} - {b.Reason}");
                    
                    Button btnRestore = new Button { Text = L.Get("hosts.restoreSelected"), Dock = DockStyle.Bottom, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(148, 146, 230), ForeColor = Color.White };
                    btnRestore.FlatAppearance.BorderSize = 0;
                    frm.Controls.Add(lb);
                    frm.Controls.Add(btnRestore);

                    if (frm.ShowDialog() == DialogResult.OK && lb.SelectedIndex >= 0)
                    {
                        if (MessageBox.Show(L.Get("hosts.confirmRestore"), L.Get("common.confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            await ServiceLocator.HostsManagerService.RestoreBackupAsync(backups[lb.SelectedIndex].BackupPath);
                            await LoadDataAsync();
                            MessageBox.Show(L.Get("hosts.restoreSuccess"), L.Get("common.success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, L.Get("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @"C:\Windows\System32\drivers\etc");
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
    }
}


