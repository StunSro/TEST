namespace SR_PROXY
{
    partial class REWARD_FORM
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(REWARD_FORM));
            this.label1 = new System.Windows.Forms.Label();
            this.RW_TYPE = new System.Windows.Forms.ComboBox();
            this.RW_AMOUNT = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.OnlineUser = new System.Windows.Forms.CheckBox();
            this.ip = new System.Windows.Forms.RadioButton();
            this.mac = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type:";
            // 
            // RW_TYPE
            // 
            this.RW_TYPE.BackColor = System.Drawing.Color.Gainsboro;
            this.RW_TYPE.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RW_TYPE.FormattingEnabled = true;
            this.RW_TYPE.Location = new System.Drawing.Point(104, 14);
            this.RW_TYPE.Name = "RW_TYPE";
            this.RW_TYPE.Size = new System.Drawing.Size(104, 21);
            this.RW_TYPE.TabIndex = 1;
            // 
            // RW_AMOUNT
            // 
            this.RW_AMOUNT.BackColor = System.Drawing.Color.Gainsboro;
            this.RW_AMOUNT.Location = new System.Drawing.Point(104, 41);
            this.RW_AMOUNT.Name = "RW_AMOUNT";
            this.RW_AMOUNT.Size = new System.Drawing.Size(104, 21);
            this.RW_AMOUNT.TabIndex = 3;
            this.RW_AMOUNT.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RW_AMOUNT_KeyPress);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(182)))), ((int)(((byte)(255)))));
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(61)))), ((int)(((byte)(88)))));
            this.button1.Location = new System.Drawing.Point(12, 76);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Quantity";
            // 
            // OnlineUser
            // 
            this.OnlineUser.AutoSize = true;
            this.OnlineUser.Location = new System.Drawing.Point(222, 29);
            this.OnlineUser.Name = "OnlineUser";
            this.OnlineUser.Size = new System.Drawing.Size(108, 17);
            this.OnlineUser.TabIndex = 6;
            this.OnlineUser.Text = "All Online Players";
            this.OnlineUser.UseVisualStyleBackColor = true;
            this.OnlineUser.CheckedChanged += new System.EventHandler(this.OnlineUser_CheckedChanged);
            // 
            // ip
            // 
            this.ip.AutoSize = true;
            this.ip.Location = new System.Drawing.Point(7, 22);
            this.ip.Name = "ip";
            this.ip.Size = new System.Drawing.Size(77, 17);
            this.ip.TabIndex = 7;
            this.ip.Text = "Only His IP";
            this.ip.UseVisualStyleBackColor = true;
            // 
            // mac
            // 
            this.mac.AutoSize = true;
            this.mac.Location = new System.Drawing.Point(97, 23);
            this.mac.Name = "mac";
            this.mac.Size = new System.Drawing.Size(95, 17);
            this.mac.TabIndex = 7;
            this.mac.Text = "Only His HWID";
            this.mac.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.mac);
            this.groupBox1.Controls.Add(this.ip);
            this.groupBox1.Enabled = false;
            this.groupBox1.Location = new System.Drawing.Point(12, 108);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(298, 56);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Give Reward To:";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = true;
            this.radioButton3.Location = new System.Drawing.Point(210, 23);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(36, 17);
            this.radioButton3.TabIndex = 8;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "All";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // REWARD_FORM
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(332, 169);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.OnlineUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.RW_AMOUNT);
            this.Controls.Add(this.RW_TYPE);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "REWARD_FORM";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reward";
            this.Load += new System.EventHandler(this.REWARD_FORM_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RW_TYPE;
        private System.Windows.Forms.TextBox RW_AMOUNT;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox OnlineUser;
        private System.Windows.Forms.RadioButton ip;
        private System.Windows.Forms.RadioButton mac;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton3;
    }
}