using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Optimizer.Controls;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class DashboardView : UserControl
    {
        private Label lblHealthTitle;
        private Label lblHealthScore;
        private Label lblHealthStatus;
        private ModernCard healthCard;

        private FlowLayoutPanel statsPanel;
        
        private WaveChartControl cpuWave;
        private WaveChartControl ramWave;
        private WaveChartControl diskWave;
        private WaveChartControl gpuWave;
        
        private Label lblCpuValue;
        private Label lblRamValue;
        private Label lblDiskValue;
        private Label lblGpuValue;
        private Panel containerPanel;
        
        public event Action UsageAssessmentRequested;
        
        public DashboardView()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
        }

        private void InitializeComponent()
        {
            this.containerPanel = new Panel();
            this.healthCard = new ModernCard();
            this.lblHealthTitle = new Label();
            this.lblHealthScore = new Label();
            this.lblHealthStatus = new Label();
            this.statsPanel = new FlowLayoutPanel();
            
            this.healthCard.SuspendLayout();
            this.containerPanel.SuspendLayout();
            this.SuspendLayout();

            // Container Panel
            this.containerPanel.Width = 740;
            this.containerPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            this.containerPanel.Location = new Point(20, 20);

            // Health Card
            this.healthCard.BorderRadius = 15;
            this.healthCard.BackColor = Color.FromArgb(30, 30, 30);
            this.healthCard.Dock = DockStyle.Top;
            this.healthCard.Height = 150;
            this.healthCard.Padding = new Padding(20);
            this.healthCard.Controls.Add(this.lblHealthStatus);
            this.healthCard.Controls.Add(this.lblHealthScore);
            this.healthCard.Controls.Add(this.lblHealthTitle);

            // Health Title
            this.lblHealthTitle.Text = L.Get("dashboard.systemHealth");
            this.lblHealthTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            this.lblHealthTitle.Dock = DockStyle.Top;
            this.lblHealthTitle.Height = 30;

            // Health Score
            this.lblHealthScore.Text = "...";
            this.lblHealthScore.Font = new Font("Segoe UI", 48, FontStyle.Bold);
            this.lblHealthScore.Dock = DockStyle.Left;
            this.lblHealthScore.Width = 150;
            this.lblHealthScore.TextAlign = ContentAlignment.MiddleCenter;

            // Health Status
            this.lblHealthStatus.Text = L.Get("dashboard.notAssessed");
            this.lblHealthStatus.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            this.lblHealthStatus.Dock = DockStyle.Fill;
            this.lblHealthStatus.TextAlign = ContentAlignment.MiddleLeft;

            // Stats Panel
            this.statsPanel.Dock = DockStyle.Fill;
            this.statsPanel.Padding = new Padding(0, 20, 0, 0);
            this.statsPanel.AutoScroll = false; // No scroll needed

            // Add to Container
            this.containerPanel.Controls.Add(this.statsPanel);
            this.containerPanel.Controls.Add(this.healthCard);

            // Add to UserControl
            this.Controls.Add(this.containerPanel);
            this.Padding = new Padding(20);

            this.healthCard.ResumeLayout(false);
            this.containerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

            this.VisibleChanged += DashboardView_VisibleChanged;
            this.Load += DashboardView_Load;
            this.Resize += DashboardView_Resize;
            this.statsPanel.WrapContents = true;
        }

        private void DashboardView_Resize(object sender, EventArgs e)
        {
            if (this.containerPanel != null)
            {
                this.containerPanel.Left = (this.ClientSize.Width - this.containerPanel.Width) / 2;
                this.containerPanel.Height = this.ClientSize.Height - this.Padding.Top - this.Padding.Bottom;
            }
        }

        private void DashboardView_Load(object sender, EventArgs e)
        {
            // Force start on initial load since VisibleChanged might not fire when transitioning from not-created to created
            DashboardView_VisibleChanged(this, EventArgs.Empty);
        }

        public async void LoadDataAsync()
        {
            try
            {
                var sysInfo = ServiceLocator.SystemInfoService;
                if (sysInfo == null) return;

                // Health Card Logic
                var cacheService = new Optimizer.Infrastructure.SystemInfo.SystemHealthCacheService();
                var cachedResult = await cacheService.GetCachedAssessmentAsync();
                
                // Ensure buttons exist
                var btnCheck = healthCard.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnCheckSystem");
                var btnUsage = healthCard.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnUsageAssessment");
                
                if (btnCheck == null)
                {
                    btnCheck = new Button();
                    btnCheck.Name = "btnCheckSystem";
                    btnCheck.Text = L.Get("dashboard.checkSystem");
                    btnCheck.FlatStyle = FlatStyle.Flat;
                    btnCheck.BackColor = Color.FromArgb(10, 132, 255);
                    btnCheck.ForeColor = Color.White;
                    btnCheck.Size = new Size(130, 35);
                    btnCheck.Location = new Point(Math.Max(0, healthCard.Width - 150), 30);
                    btnCheck.Cursor = Cursors.Hand;
                    btnCheck.Click += (s, e) => RunAssessment();
                    healthCard.Controls.Add(btnCheck);

                    btnUsage = new Button();
                    btnUsage.Name = "btnUsageAssessment";
                    btnUsage.Text = L.Get("dashboard.usageAssessment");
                    btnUsage.FlatStyle = FlatStyle.Flat;
                    btnUsage.BackColor = Color.FromArgb(40, 40, 40);
                    btnUsage.ForeColor = Color.White;
                    btnUsage.Size = new Size(130, 35);
                    btnUsage.Location = new Point(Math.Max(0, healthCard.Width - 150), 75);
                    btnUsage.Cursor = Cursors.Hand;
                    btnUsage.Click += (s, e) => UsageAssessmentRequested?.Invoke();
                    healthCard.Controls.Add(btnUsage);

                    // Handle resize to keep buttons on the right
                    healthCard.Resize += (s, e) => 
                    {
                        btnCheck.Left = healthCard.Width - btnCheck.Width - 20;
                        btnUsage.Left = healthCard.Width - btnUsage.Width - 20;
                    };
                    // Trigger initial position
                    btnCheck.Left = healthCard.Width > 0 ? healthCard.Width - btnCheck.Width - 20 : 500;
                    btnUsage.Left = btnCheck.Left;
                }

                if (cachedResult == null)
                {
                    lblHealthTitle.Text = L.Get("dashboard.systemHealth");
                    lblHealthScore.Text = "?";
                    lblHealthScore.Font = new Font("Segoe UI", 48, FontStyle.Bold);
                    lblHealthScore.ForeColor = Color.DarkGray;
                    lblHealthStatus.Text = L.Get("dashboard.notAssessed");
                    btnCheck.Text = L.Get("dashboard.checkSystem");
                    btnCheck.BringToFront();
                    btnUsage?.BringToFront();
                }
                else
                {
                    lblHealthTitle.Text = L.Get("dashboard.systemHealth");
                    lblHealthScore.Text = cachedResult.Score.ToString();
                    lblHealthScore.Font = new Font("Segoe UI", 48, FontStyle.Bold);
                    
                    if (cachedResult.Score >= 90) lblHealthScore.ForeColor = Color.FromArgb(191, 90, 242);
                    else if (cachedResult.Score >= 70) lblHealthScore.ForeColor = Color.FromArgb(48, 209, 88);
                    else if (cachedResult.Score >= 40) lblHealthScore.ForeColor = Color.Orange;
                    else lblHealthScore.ForeColor = Color.Red;
                    
                    lblHealthStatus.Text = $"{cachedResult.Rating}\n{L.Get("dashboard.lastAssessment")} {cachedResult.LastAssessment.ToString("dd/MM/yyyy HH:mm:ss")}";
                    btnCheck.Text = L.Get("dashboard.recheck");
                    btnCheck.BringToFront();
                    btnUsage?.BringToFront();
                }

                // Load individual stats
                string computerName = await sysInfo.GetComputerNameAsync();
                string osVersion = await sysInfo.GetWindowsVersionAsync();
                string cpuName = await sysInfo.GetCpuNameAsync();
                string gpuName = await sysInfo.GetGpuNameAsync();
                string ram = await sysInfo.GetRamInstalledAsync();
                string disk = await sysInfo.GetDiskUsageAsync();

                for (int i = statsPanel.Controls.Count - 1; i >= 0; i--)
                {
                    var c = statsPanel.Controls[i];
                    statsPanel.Controls.RemoveAt(i);
                    c.Dispose();
                }
                
                // System Info Merged Card
                statsPanel.Controls.Add(CreateSystemInfoCard(computerName, osVersion));

                // Wave Cards
                statsPanel.Controls.Add(CreateWaveCard(L.Get("dashboard.hw.processor"), cpuName, Color.FromArgb(10, 132, 255), out cpuWave, out lblCpuValue)); // Blue
                statsPanel.Controls.Add(CreateWaveCard(L.Get("dashboard.hw.graphics"), gpuName, Color.FromArgb(191, 90, 242), out gpuWave, out lblGpuValue)); // Purple
                statsPanel.Controls.Add(CreateWaveCard(L.Get("dashboard.hw.memory"), ram, Color.FromArgb(48, 209, 88), out ramWave, out lblRamValue)); // Green
                statsPanel.Controls.Add(CreateWaveCard(L.Get("dashboard.hw.primaryDrive"), disk, Color.FromArgb(255, 159, 10), out diskWave, out lblDiskValue)); // Orange
                
            }
            catch (Exception ex)
            {
                ServiceLocator.LoggerService?.Error($"Failed to load dashboard data: {ex.Message}", "DashboardView", ex.StackTrace);
            }
        }

        private void RunAssessment()
        {
            var form = new Optimizer.Forms.AssessmentProgressForm();
            form.ShowDialog(this.FindForm());
            LoadDataAsync();
        }

        private ModernCard CreateSystemInfoCard(string computerName, string osEdition)
        {
            ModernCard card = new ModernCard();
            card.Width = 720;
            card.Height = 90;
            card.Margin = new Padding(0, 0, 20, 20);
            card.Padding = new Padding(20);
            card.BorderRadius = 15;
            card.BackColor = Color.FromArgb(30, 30, 30);

            Label lblDevice = new Label();
            lblDevice.Text = $"{L.Get("dashboard.hw.device")} {computerName.ToUpper()}";
            lblDevice.Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold);
            lblDevice.Dock = DockStyle.Top;
            lblDevice.Height = 30;

            Label lblOS = new Label();
            lblOS.Text = $"{L.Get("dashboard.hw.os")} {osEdition}";
            lblOS.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            lblOS.ForeColor = Color.DarkGray;
            lblOS.Dock = DockStyle.Top;
            lblOS.Height = 30;

            card.Controls.Add(lblOS);
            card.Controls.Add(lblDevice);
            return card;
        }

        private ModernCard CreateWaveCard(string title, string staticValue, Color waveColor, out WaveChartControl wave, out Label lblValue)
        {
            ModernCard card = new ModernCard();
            card.Width = 350;
            card.Height = 190;
            card.Margin = new Padding(0, 0, 20, 20);
            card.Padding = new Padding(20);
            card.BorderRadius = 15;
            card.BackColor = Color.FromArgb(30, 30, 30);

            Label lblTitle = new Label();
            lblTitle.Text = title.ToUpper();
            lblTitle.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkGray;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 25;

            Label lblStatic = new Label();
            lblStatic.Text = staticValue;
            lblStatic.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblStatic.Dock = DockStyle.Top;
            lblStatic.Height = 30;

            lblValue = new Label();
            lblValue.Text = "0%";
            lblValue.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblValue.ForeColor = waveColor;
            lblValue.Dock = DockStyle.Top;
            lblValue.Height = 45;

            wave = new WaveChartControl();
            wave.Dock = DockStyle.Fill;
            wave.LineColor = waveColor;
            wave.GradientTopColor = Color.FromArgb(80, waveColor.R, waveColor.G, waveColor.B);
            wave.GradientBottomColor = Color.FromArgb(0, waveColor.R, waveColor.G, waveColor.B);

            card.Controls.Add(wave);
            card.Controls.Add(lblValue);
            card.Controls.Add(lblStatic);
            card.Controls.Add(lblTitle);

            return card;
        }

        private void DashboardView_VisibleChanged(object sender, EventArgs e)
        {
            var monitor = ServiceLocator.RealtimeMonitorService;
            if (monitor == null) return;

            if (this.Visible)
            {
                monitor.MetricsUpdated -= Monitor_MetricsUpdated;
                monitor.MetricsUpdated += Monitor_MetricsUpdated;
                monitor.StartMonitoring();
            }
            else
            {
                monitor.MetricsUpdated -= Monitor_MetricsUpdated;
                monitor.StopMonitoring();
            }
        }

        private void Monitor_MetricsUpdated(Domain.Models.RealtimeMetrics metrics)
        {
            if (this.IsDisposed || !this.Visible) return;

            this.Invoke(new Action(() =>
            {
                if (cpuWave != null && lblCpuValue != null)
                {
                    cpuWave.AddValue(metrics.CpuUsage);
                    lblCpuValue.Text = $"{metrics.CpuUsage}%";
                }

                if (ramWave != null && lblRamValue != null)
                {
                    ramWave.AddValue(metrics.RamUsage);
                    lblRamValue.Text = $"{metrics.RamUsage}%";
                }

                if (diskWave != null && lblDiskValue != null)
                {
                    diskWave.AddValue(metrics.DiskUsage);
                    lblDiskValue.Text = $"{metrics.DiskUsage}%";
                }

                if (gpuWave != null && lblGpuValue != null)
                {
                    gpuWave.AddValue(metrics.GpuUsage);
                    lblGpuValue.Text = $"{metrics.GpuUsage}%";
                }
            }));
        }


    }
}

