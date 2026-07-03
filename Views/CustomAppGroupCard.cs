using System;
using System.Drawing;
using System.Windows.Forms;
using Optimizer.Domain.Models;

namespace Optimizer.Views
{
    public class CustomAppGroupCard : UserControl
    {
        public CustomAppJunkCategory Category { get; private set; }
        public event EventHandler SelectionChanged;
        private CheckBox chkSelect;

        public CustomAppGroupCard(CustomAppJunkCategory category)
        {
            Category = category;
            
            this.BackColor = Color.FromArgb(26, 26, 26);
            this.Height = 80;
            this.Margin = new Padding(0, 0, 0, 10);
            
            chkSelect = new CheckBox
            {
                Location = new Point(15, 30),
                AutoSize = true,
                Checked = category.IsSelected && category.SafetyLevel != CleanupSafetyLevel.Protected,
                Enabled = category.SafetyLevel != CleanupSafetyLevel.Protected,
                Cursor = Cursors.Hand,
                Text = ""
            };
            chkSelect.CheckedChanged += (s, e) =>
            {
                if (category.SafetyLevel == CleanupSafetyLevel.Protected) chkSelect.Checked = false;
                Category.IsSelected = chkSelect.Checked;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            };

            var lblName = new Label
            {
                Text = category.CategoryName,
                Font = new Font("Segoe UI Semibold", 12),
                ForeColor = Color.White,
                Location = new Point(45, 15),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = category.Description,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(45, 40),
                AutoSize = true
            };

            var lblSize = new Label
            {
                Text = category.FormattedSize,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.MediumSlateBlue,
                Location = new Point(this.Width - 350, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };

            var lblCount = new Label
            {
                Text = $"{category.FileCount} files",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkGray,
                Location = new Point(this.Width - 350, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true
            };

            var lblSafety = SafetyBadgeHelper.CreateBadge(category.SafetyLevel);
            lblSafety.Location = new Point(this.Width - 200, 28);
            lblSafety.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            var btnPreview = new Button
            {
                Text = "Preview",
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(this.Width - 100, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnPreview.FlatAppearance.BorderSize = 0;
            btnPreview.Click += (s, e) =>
            {
                var dialog = new PreviewDialog(category);
                dialog.ShowDialog();
            };

            this.Controls.Add(chkSelect);
            this.Controls.Add(lblName);
            this.Controls.Add(lblDesc);
            this.Controls.Add(lblSize);
            this.Controls.Add(lblCount);
            this.Controls.Add(lblSafety);
            this.Controls.Add(btnPreview);
            
            // Fix resizing anchors
            this.Resize += (s, e) =>
            {
                lblSize.Location = new Point(this.Width - 350, 15);
                lblCount.Location = new Point(this.Width - 350, 40);
                lblSafety.Location = new Point(this.Width - 200, 30);
                btnPreview.Location = new Point(this.Width - 100, 25);
            };
        }
        
        public void SetChecked(bool state)
        {
            chkSelect.Checked = state;
        }
    }
}
