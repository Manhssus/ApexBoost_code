using System;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;
using Optimizer.Localization;

namespace Optimizer.Views
{
    public class ProfilePreviewDialog : Form
    {
        public OptimizationProfile Profile { get; private set; }
        private FlowLayoutPanel pnlActions;
        private Button btnApply;
        private Button btnCancel;

        public ProfilePreviewDialog(OptimizationProfile profile)
        {
            Profile = profile;
            
            this.Text = L.Get("dialog.applyProfile.title", profile.Title);
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lblHeader = new Label
            {
                Text = Profile.Title,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            Label lblDesc = new Label
            {
                Text = Profile.Description,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(22, 55),
                AutoSize = true,
                MaximumSize = new Size(this.Width - 60, 0)
            };
            this.Controls.Add(lblDesc);

            string rawRisk = Profile.RiskLevel ?? "";
            string riskKey = "safe";
            if (rawRisk.IndexOf("Low-to-Medium", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "lowToMedium";
            else if (rawRisk.IndexOf("Zero", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "zero";
            else if (rawRisk.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "medium";
            else if (rawRisk.IndexOf("Safe", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "safe";
            else if (rawRisk.IndexOf("Review", StringComparison.OrdinalIgnoreCase) >= 0) riskKey = "review";
            else riskKey = rawRisk.ToLower().Replace(" risk", "").Replace("-", "");

            Label lblInfo = new Label
            {
                Text = L.Get("dialog.applyProfile.infoFormat", L.Get($"common.risk.{riskKey}"), Profile.Actions.Count),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.Orange,
                Location = new Point(22, 90),
                AutoSize = true
            };
            this.Controls.Add(lblInfo);

            pnlActions = new FlowLayoutPanel
            {
                Location = new Point(20, 120),
                Size = new Size(this.Width - 60, this.Height - 220),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            foreach (var action in Profile.Actions)
            {
                var card = new Panel
                {
                    Width = pnlActions.Width - 25,
                    Height = 60,
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(50, 50, 50)
                };

                Label name = new Label
                {
                    Text = action.Name,
                    Font = new Font("Segoe UI Semibold", 10),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                
                Label desc = new Label
                {
                    Text = action.Description,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray,
                    Location = new Point(10, 32),
                    AutoSize = false,
                    Size = new Size(card.Width - 200, 20),
                    AutoEllipsis = true
                };

                Label risk = new Label
                {
                    Text = L.Get("dialog.applyProfile.riskFormat", L.Get("common.risk." + (action.RiskLevel ?? "safe").ToLower())),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = action.RiskLevel == "Safe" ? Color.LightGreen : (action.RiskLevel == "Review" ? Color.Orange : Color.Red),
                    Location = new Point(card.Width - 180, 10),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                Label rev = new Label
                {
                    Text = action.IsReversible ? L.Get("dialog.applyProfile.reversibleYes") : L.Get("dialog.applyProfile.reversibleNo"),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.LightSkyBlue,
                    Location = new Point(card.Width - 180, 32),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                card.Controls.Add(name);
                card.Controls.Add(desc);
                card.Controls.Add(risk);
                card.Controls.Add(rev);
                pnlActions.Controls.Add(card);
            }

            this.Controls.Add(pnlActions);

            btnCancel = new Button
            {
                Text = L.Get("common.cancel"),
                DialogResult = DialogResult.Cancel,
                Location = new Point(this.Width - 220, this.Height - 80),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            btnApply = new Button
            {
                Text = L.Get("dialog.applyProfile.btnApply"),
                DialogResult = DialogResult.OK,
                Location = new Point(this.Width - 130, this.Height - 80),
                Size = new Size(100, 30),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.FlatAppearance.BorderSize = 0;

            this.Controls.Add(btnCancel);
            this.Controls.Add(btnApply);
            this.AcceptButton = btnApply;
            this.CancelButton = btnCancel;
        }
    }
}
