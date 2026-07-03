namespace Optimizer.Views
{
    partial class CleanupGroupCard
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.CheckBox chkGroup;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblArrow;
        private System.Windows.Forms.FlowLayoutPanel pnlItems;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.chkGroup = new System.Windows.Forms.CheckBox();
            this.lblIcon = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDesc = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblArrow = new System.Windows.Forms.Label();
            this.pnlItems = new System.Windows.Forms.FlowLayoutPanel();
            
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            
            // pnlHeader
            this.pnlHeader.Controls.Add(this.chkGroup);
            this.pnlHeader.Controls.Add(this.lblIcon);
            this.pnlHeader.Controls.Add(this.lblName);
            this.pnlHeader.Controls.Add(this.lblDesc);
            this.pnlHeader.Controls.Add(this.lblSize);
            this.pnlHeader.Controls.Add(this.lblArrow);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(600, 65);
            this.pnlHeader.TabIndex = 0;
            
            // chkGroup
            this.chkGroup.AutoSize = true;
            this.chkGroup.Location = new System.Drawing.Point(15, 23);
            this.chkGroup.Name = "chkGroup";
            this.chkGroup.Size = new System.Drawing.Size(15, 14);
            this.chkGroup.TabIndex = 0;
            this.chkGroup.UseVisualStyleBackColor = true;
            
            // lblIcon
            this.lblIcon.AutoSize = true;
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIcon.ForeColor = System.Drawing.Color.White;
            this.lblIcon.Location = new System.Drawing.Point(35, 15);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(30, 30);
            this.lblIcon.TabIndex = 1;
            this.lblIcon.Text = "📁";
            
            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.White;
            this.lblName.Location = new System.Drawing.Point(75, 10);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(56, 21);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "Name";
            
            // lblDesc
            this.lblDesc.AutoSize = true;
            this.lblDesc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblDesc.Location = new System.Drawing.Point(75, 35);
            this.lblDesc.Name = "lblDesc";
            this.lblDesc.Size = new System.Drawing.Size(67, 15);
            this.lblDesc.TabIndex = 3;
            this.lblDesc.Text = "Description";
            
            // lblSize
            this.lblSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSize.AutoSize = true;
            this.lblSize.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSize.ForeColor = System.Drawing.Color.LightGray;
            this.lblSize.Location = new System.Drawing.Point(490, 20);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(46, 20);
            this.lblSize.TabIndex = 4;
            this.lblSize.Text = "0 MB";
            
            // lblArrow
            this.lblArrow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblArrow.AutoSize = true;
            this.lblArrow.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblArrow.ForeColor = System.Drawing.Color.Gray;
            this.lblArrow.Location = new System.Drawing.Point(565, 22);
            this.lblArrow.Name = "lblArrow";
            this.lblArrow.Size = new System.Drawing.Size(23, 19);
            this.lblArrow.TabIndex = 5;
            this.lblArrow.Text = "▼";
            
            // pnlItems
            this.pnlItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItems.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlItems.Location = new System.Drawing.Point(0, 65);
            this.pnlItems.Name = "pnlItems";
            this.pnlItems.Size = new System.Drawing.Size(600, 35);
            this.pnlItems.TabIndex = 1;
            this.pnlItems.WrapContents = false;
            
            // CleanupGroupCard
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlItems);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "CleanupGroupCard";
            this.Size = new System.Drawing.Size(600, 100);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
