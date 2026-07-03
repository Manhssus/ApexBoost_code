using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Forms;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class NetworkToolsView : UserControl
    {
        private Panel _headerPanel;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Button _btnBack;
        private Panel _warningBanner;
        private Label _lblWarningText;

        // Health Dashboard
        private TableLayoutPanel _dashboardPanel;
        private NetworkGaugeControl _healthMeterControl;

        // Stats Grid
        private TableLayoutPanel _statsGrid;
        private Label _lblInternetVal;
        private Label _lblPrimaryAdapterVal;
        private Label _lblGatewayVal;
        private Label _lblDnsVal;

        // Toolbar
        private Panel _toolbarPanel;
        private Button _btnRefresh;
        private Button _btnFlushDns;
        private Button _btnCopyReport;

        // Network Tools (Ping / DNS)
        private Panel _toolsContainer;
        private TextBox _txtPingHost;
        private Label _lblPingResult;
        private Button _btnPing;
        private TextBox _txtDnsDomain;
        private Label _lblDnsResult;
        private Button _btnDns;

        // Adapters List
        private Panel _clippingContainer;
        private Panel _scrollArea;
        private FlowLayoutPanel _flpItems;
        private ModernScrollBar _customScrollBar;

        private NetworkOverviewResult _lastOverview;
        private string _lastFlushResult = "Not Run";
        private string _lastFlushTime = "";

        public event EventHandler BackRequested;

        public NetworkToolsView()
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
                Text = L.Get("advanced.network.title"),
                Location = new Point(135, 16),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
                ForeColor = Color.White
            };
            _headerPanel.Controls.Add(_lblTitle);

            _lblSubtitle = new Label
            {
                Text = L.Get("advanced.network.subtitle"),
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
                Text = L.Get("network.readonly"),
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

            _healthMeterControl = new NetworkGaugeControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0)
            };
            _dashboardPanel.Controls.Add(_healthMeterControl, 0, 0);

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

            _lblInternetVal = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 25) };
            _lblPrimaryAdapterVal = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightSkyBlue, AutoSize = true, Location = new Point(15, 25) };
            _lblGatewayVal = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };
            _lblDnsVal = new Label { Text = "-", Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.LightGray, AutoSize = true, Location = new Point(15, 25) };

            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("network.internetStatus"), _lblInternetVal, false), 0, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("network.primaryAdapter"), _lblPrimaryAdapterVal, false), 1, 0);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("network.gateway"), _lblGatewayVal, true), 0, 1);
            _statsGrid.Controls.Add(CreateCompactStatCard(L.Get("network.primaryDns"), _lblDnsVal, true), 1, 1);

            _dashboardPanel.Controls.Add(_statsGrid, 1, 0);
            dashboardContainer.Controls.Add(_dashboardPanel);

            // 4. Toolbar & Filters
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };

            _btnRefresh = CreateActionButton(L.Get("common.refresh"), true);
            _btnRefresh.Click += async (s, e) => await LoadDataAsync();

            _btnFlushDns = CreateActionButton(L.Get("network.flushDns"), false);
            _btnFlushDns.Click += async (s, e) => await FlushDnsCacheAsync();

            _btnCopyReport = CreateActionButton(L.Get("common.copyReport"), false);
            _btnCopyReport.Click += (s, e) => CopyReport();

            _toolbarPanel.Controls.Add(_btnRefresh);
            _toolbarPanel.Controls.Add(_btnFlushDns);
            _toolbarPanel.Controls.Add(_btnCopyReport);

            // 5. Tools Container (Ping and DNS side by side)
            _toolsContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = Color.Transparent,
                Padding = new Padding(40, 10, 40, 10)
            };

            TableLayoutPanel toolsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            toolsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            toolsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Ping Tool
            Panel pnlPing = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 25), Margin = new Padding(0, 0, 10, 0) };
            Label lblPingTitle = new Label { Text = L.Get("network.pingTest"), Font = new Font("Segoe UI Semibold", 10F), ForeColor = Color.White, Location = new Point(15, 10), AutoSize = true };
            _txtPingHost = new TextBox { Text = "8.8.8.8", Width = 150, Location = new Point(15, 35), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            _btnPing = CreateActionButton(L.Get("common.run"), false);
            _btnPing.Location = new Point(175, 33);
            _btnPing.Size = new Size(60, 26);
            _btnPing.Click += async (s, e) => await RunPingTestAsync();
            _lblPingResult = new Label { Text = L.Get("common.ready"), Location = new Point(15, 65), AutoSize = true, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 9F) };
            
            pnlPing.Controls.Add(lblPingTitle);
            pnlPing.Controls.Add(_txtPingHost);
            pnlPing.Controls.Add(_btnPing);
            pnlPing.Controls.Add(_lblPingResult);

            // DNS Tool
            Panel pnlDns = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 25), Margin = new Padding(10, 0, 0, 0) };
            Label lblDnsTitle = new Label { Text = L.Get("network.dnsLookup"), Font = new Font("Segoe UI Semibold", 10F), ForeColor = Color.White, Location = new Point(15, 10), AutoSize = true };
            _txtDnsDomain = new TextBox { Text = "google.com", Width = 150, Location = new Point(15, 35), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            _btnDns = CreateActionButton(L.Get("common.lookup"), false);
            _btnDns.Location = new Point(175, 33);
            _btnDns.Size = new Size(60, 26);
            _btnDns.Click += async (s, e) => await RunDnsLookupAsync();
            _lblDnsResult = new Label { Text = L.Get("common.ready"), Location = new Point(15, 65), AutoSize = true, ForeColor = Color.DarkGray, Font = new Font("Segoe UI", 9F) };

            pnlDns.Controls.Add(lblDnsTitle);
            pnlDns.Controls.Add(_txtDnsDomain);
            pnlDns.Controls.Add(_btnDns);
            pnlDns.Controls.Add(_lblDnsResult);

            toolsGrid.Controls.Add(pnlPing, 0, 0);
            toolsGrid.Controls.Add(pnlDns, 1, 0);

            _toolsContainer.Controls.Add(toolsGrid);

            // 6. Main Content Area (Clipping Container)
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

            Label lblAdaptersTitle = new Label
            {
                Text = L.Get("network.networkAdapters"),
                Font = new Font("Segoe UI Semibold", 14F),
                ForeColor = Color.Orange,
                Location = new Point(40, 10),
                AutoSize = true
            };
            _scrollArea.Controls.Add(lblAdaptersTitle);

            _flpItems = new FlowLayoutPanel
            {
                Location = new Point(40, 45),
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
            this.Controls.Add(_toolsContainer);
            this.Controls.Add(_toolbarPanel);
            this.Controls.Add(dashboardContainer);
            this.Controls.Add(warningContainer);
            this.Controls.Add(_headerPanel);

            this.Resize += NetworkToolsView_Resize;
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

        private void NetworkToolsView_Resize(object sender, EventArgs e)
        {
            if (_clippingContainer == null) return;
            
            int safeWidth = this.Width - 80;
            if (safeWidth < 500) safeWidth = 500;

            _warningBanner.Width = safeWidth;
            _dashboardPanel.Width = safeWidth;

            // Action Toolbar Right Align
            _btnRefresh.Location = new Point(40 + safeWidth - _btnRefresh.Width, 14);
            _btnFlushDns.Location = new Point(_btnRefresh.Left - _btnFlushDns.Width - 10, 14);
            _btnCopyReport.Location = new Point(_btnFlushDns.Left - _btnCopyReport.Width - 10, 14);

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

        public async Task LoadDataAsync()
        {
            if (this.IsDisposed || this.Disposing) return;
            if (ServiceLocator.NetworkToolsService == null) return;

            _btnRefresh.Enabled = false;
            
            try
            {
                _lastOverview = await ServiceLocator.NetworkToolsService.GetNetworkOverviewAsync();
                if (this.IsDisposed || this.Disposing) return;

                _healthMeterControl.IsConnected = _lastOverview.InternetStatus == "Connected";
                _healthMeterControl.IssueCount = _lastOverview.InternetStatus == "Connected" ? 0 : 1;
                _healthMeterControl.Invalidate();

                _lblInternetVal.Text = _lastOverview.InternetStatus == "Connected" ? L.Get("common.status.connected") : L.Get("common.status.disconnected");
                _lblInternetVal.ForeColor = _lastOverview.InternetStatus == "Connected" ? Color.FromArgb(48, 209, 88) : Color.IndianRed;

                var activeAdapter = _lastOverview.Adapters.FirstOrDefault(a => a.IsActive && a.Status == "Up");
                if (activeAdapter != null)
                {
                    _lblPrimaryAdapterVal.Text = activeAdapter.Name.Length > 20 ? activeAdapter.Name.Substring(0, 17) + "..." : activeAdapter.Name;
                    _lblGatewayVal.Text = string.IsNullOrWhiteSpace(activeAdapter.Gateway) ? L.Get("common.none") : activeAdapter.Gateway;
                    _lblDnsVal.Text = activeAdapter.DnsServers.Count > 0 ? activeAdapter.DnsServers[0] : L.Get("common.none");
                }
                else
                {
                    _lblPrimaryAdapterVal.Text = L.Get("common.none");
                    _lblGatewayVal.Text = L.Get("common.none");
                    _lblDnsVal.Text = L.Get("common.none");
                }

                RenderAdapters();
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                    _btnRefresh.Enabled = true;
            }
        }

        private void RenderAdapters()
        {
            ClearAndDisposeControls(_flpItems);

            if (_lastOverview?.Adapters != null)
            {
                var sortedAdapters = _lastOverview.Adapters.OrderByDescending(a => a.IsActive && a.Status == "Up").ThenBy(a => a.Name);

                foreach (var adapter in sortedAdapters)
                {
                    var card = CreateAdapterCard(adapter);
                    _flpItems.Controls.Add(card);
                }
            }

            NetworkToolsView_Resize(this, EventArgs.Empty);
        }

        private Panel CreateAdapterCard(NetworkAdapterInfo adapter)
        {
            Panel card = new Panel
            {
                Height = 65,
                BackColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0)
            };

            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(50, 50, 50), ButtonBorderStyle.Solid);
            };

            bool isConnected = adapter.IsActive && adapter.Status == "Up";
            Color statusColor = isConnected ? Color.FromArgb(48, 209, 88) : Color.IndianRed;

            Panel leftAccent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = statusColor
            };
            card.Controls.Add(leftAccent);

            Label lblBadge = new Label
            {
                Text = isConnected ? L.Get("common.status.connected") : L.Get("common.status.disconnected"),
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = statusColor,
                AutoSize = true,
                Location = new Point(20, 25),
                Padding = new Padding(2)
            };
            card.Controls.Add(lblBadge);

            Label lblName = new Label
            {
                Text = adapter.Name,
                Font = new Font("Segoe UI Semibold", 11F),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(100, 12)
            };
            card.Controls.Add(lblName);

            Label lblDesc = new Label
            {
                Text = adapter.Description,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.DarkGray,
                AutoSize = true,
                Location = new Point(100, 35)
            };
            card.Controls.Add(lblDesc);

            Panel actionsPanel = new Panel
            {
                Name = "ActionsPanel",
                Height = 35,
                Width = 40,
                BackColor = Color.Transparent
            };

            Button btnDetails = new Button
            {
                Text = "\u25BC",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.LightGray,
                BackColor = Color.FromArgb(40, 40, 40),
                Size = new Size(30, 26),
                Location = new Point(0, 5),
                Cursor = Cursors.Hand
            };
            btnDetails.FlatAppearance.BorderSize = 0;
            
            Panel detailsPanel = CreateAdapterDetailsPanel(adapter);
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

        private Panel CreateAdapterDetailsPanel(NetworkAdapterInfo adapter)
        {
            Panel details = new Panel
            {
                BackColor = Color.FromArgb(25, 25, 25),
                Location = new Point(4, 65),
                Width = 1000, 
                Height = 110,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            AddDetailLabel(details, L.Get("network.ipv4", (string.IsNullOrEmpty(adapter.Ipv4) ? L.Get("common.none") : adapter.Ipv4)), 15, 10);
            AddDetailLabel(details, L.Get("network.ipv6", (string.IsNullOrEmpty(adapter.Ipv6) ? L.Get("common.none") : adapter.Ipv6)), 15, 30);
            AddDetailLabel(details, L.Get("network.macAddress", adapter.MacAddress), 15, 50);
            
            AddDetailLabel(details, L.Get("network.gatewayValue", (string.IsNullOrEmpty(adapter.Gateway) ? L.Get("common.none") : adapter.Gateway)), 300, 10);
            AddDetailLabel(details, L.Get("network.dnsValue", (adapter.DnsServers.Count > 0 ? string.Join(", ", adapter.DnsServers) : L.Get("common.none"))), 300, 30);
            AddDetailLabel(details, L.Get("network.dhcpEnabled", (adapter.DhcpEnabled ? L.Get("common.yes") : L.Get("common.no"))), 300, 50);

            AddDetailLabel(details, L.Get("network.speed", adapter.Speed), 550, 10);
            AddDetailLabel(details, L.Get("network.type", adapter.Type), 550, 30);

            return details;
        }

        private void AddDetailLabel(Panel parent, string text, int x, int y)
        {
            Label lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Location = new Point(x, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
        }

        private async Task RunPingTestAsync()
        {
            if (ServiceLocator.NetworkToolsService == null) return;
            string host = _txtPingHost.Text.Trim();
            if (string.IsNullOrEmpty(host))
            {
                _lblPingResult.Text = L.Get("network.hostEmpty");
                _lblPingResult.ForeColor = Color.IndianRed;
                return;
            }

            _lblPingResult.Text = L.Get("network.pinging");
            _lblPingResult.ForeColor = Color.Gray;

            _btnPing.Enabled = false;
            try
            {
                var result = await ServiceLocator.NetworkToolsService.RunPingTestAsync(host);
                if (this.IsDisposed || this.Disposing) return;

                if (result.Success)
                {
                    _lblPingResult.ForeColor = Color.FromArgb(48, 209, 88);
                    string ttlDisplay = result.Ttl > 0 ? result.Ttl.ToString() : "N/A";
                    _lblPingResult.Text = L.Get("network.pingSuccess", result.ResolvedIp, result.LatencyMs, ttlDisplay);
                }
                else
                {
                    _lblPingResult.ForeColor = Color.IndianRed;
                    _lblPingResult.Text = L.Get("network.pingFailed", result.ErrorMessage);
                }
            }
            finally
            {
                _btnPing.Enabled = true;
            }
        }

        private async Task RunDnsLookupAsync()
        {
            if (ServiceLocator.NetworkToolsService == null) return;
            string domain = _txtDnsDomain.Text.Trim();
            if (string.IsNullOrEmpty(domain))
            {
                _lblDnsResult.Text = L.Get("network.domainEmpty");
                _lblDnsResult.ForeColor = Color.IndianRed;
                return;
            }

            _lblDnsResult.Text = L.Get("network.lookingUp");
            _lblDnsResult.ForeColor = Color.Gray;
            
            _btnDns.Enabled = false;
            try
            {
                var result = await ServiceLocator.NetworkToolsService.RunDnsLookupAsync(domain);
                if (this.IsDisposed || this.Disposing) return;

                if (result.Success)
                {
                    _lblDnsResult.ForeColor = Color.FromArgb(48, 209, 88);
                    string ips = string.Join(", ", result.ResolvedIps);
                    _lblDnsResult.Text = L.Get("network.dnsSuccess", ips);
                }
                else
                {
                    _lblDnsResult.ForeColor = Color.IndianRed;
                    _lblDnsResult.Text = L.Get("network.dnsFailed", result.ErrorMessage);
                }
            }
            finally
            {
                _btnDns.Enabled = true;
            }
        }

        private async Task FlushDnsCacheAsync()
        {
            if (ServiceLocator.NetworkToolsService == null) return;
            _btnFlushDns.Enabled = false;
            try
            {
                var result = await ServiceLocator.NetworkToolsService.FlushDnsCacheAsync();
                if (this.IsDisposed || this.Disposing) return;

                _lastFlushTime = DateTime.Now.ToString("g");
                _lastFlushResult = result.success ? L.Get("common.status.success") : L.Get("common.status.failed");

                if (result.success)
                {
                    MessageBox.Show(L.Get("network.dnsFlushSuccess"), L.Get("common.status.success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(L.Get("network.dnsFlushFailed", result.output), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(L.Get("network.dnsFlushError", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnFlushDns.Enabled = true;
            }
        }

        private void CopyReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(L.Get("network.reportTitle"));
            sb.AppendLine(L.Get("network.reportGenerated", DateTime.Now.ToString("F")));
            sb.AppendLine();
            
            if (_lastOverview != null)
            {
                sb.AppendLine(L.Get("network.reportOverview"));
                sb.AppendLine(L.Get("network.reportInternet", _lastOverview.InternetStatus == "Connected" ? L.Get("common.status.connected") : L.Get("common.status.disconnected")));
                sb.AppendLine();
                sb.AppendLine(L.Get("network.reportAdapters"));
                foreach(var a in _lastOverview.Adapters)
                {
                    sb.AppendLine($"- {a.Name} ({a.Status}): IP={a.Ipv4}, MAC={a.MacAddress}");
                }
            }
            else
            {
                sb.AppendLine(L.Get("network.reportNoData"));
            }

            sb.AppendLine();
            sb.AppendLine(L.Get("network.reportPingTitle"));
            if (_lblPingResult.Text.Contains("Ready") || _lblPingResult.Text.Contains("Pinging"))
            {
                sb.AppendLine(L.Get("network.reportStatusNotRun"));
            }
            else
            {
                sb.AppendLine(L.Get("network.reportHost", _txtPingHost.Text.Trim()));
                sb.AppendLine(_lblPingResult.Text);
            }
            sb.AppendLine();

            sb.AppendLine(L.Get("network.reportDnsTitle"));
            if (_lblDnsResult.Text.Contains("Ready") || _lblDnsResult.Text.Contains("Looking up"))
            {
                sb.AppendLine(L.Get("network.reportStatusNotRun"));
            }
            else
            {
                sb.AppendLine(L.Get("network.reportDomain", _txtDnsDomain.Text.Trim()));
                sb.AppendLine(_lblDnsResult.Text);
            }
            sb.AppendLine();

            sb.AppendLine(L.Get("network.reportFlushLastRun", (_lastFlushTime == "" ? L.Get("common.never") : _lastFlushTime), _lastFlushResult));
            sb.AppendLine("===========================================");

            Clipboard.SetText(sb.ToString());
            MessageBox.Show(L.Get("network.reportCopied"), L.Get("common.copied"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

