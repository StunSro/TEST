namespace SR_PROXY
{
    partial class DETAILS
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DETAILS));
            this.DetailsLog = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // DetailsLog
            // 
            this.DetailsLog.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.DetailsLog.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DetailsLog.Location = new System.Drawing.Point(12, 12);
            this.DetailsLog.Name = "DetailsLog";
            this.DetailsLog.Size = new System.Drawing.Size(395, 220);
            this.DetailsLog.TabIndex = 0;
            this.DetailsLog.Text = "";
            // 
            // DETAILS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(418, 245);
            this.Controls.Add(this.DetailsLog);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DETAILS";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Feature Information:";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox DetailsLog;
    }
}