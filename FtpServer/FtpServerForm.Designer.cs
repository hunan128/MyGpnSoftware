namespace FtpServer
{
    partial class FtpServerForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lstboxStatus = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbxFtpServerIp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxFtpServerPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbxFtpRoot = new System.Windows.Forms.TextBox();
            this.btnFtpServerStartStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstboxStatus
            // 
            this.lstboxStatus.BackColor = System.Drawing.SystemColors.WindowText;
            this.lstboxStatus.ForeColor = System.Drawing.SystemColors.Window;
            this.lstboxStatus.FormattingEnabled = true;
            this.lstboxStatus.ItemHeight = 12;
            this.lstboxStatus.Location = new System.Drawing.Point(-4, 0);
            this.lstboxStatus.Name = "lstboxStatus";
            this.lstboxStatus.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstboxStatus.Size = new System.Drawing.Size(536, 256);
            this.lstboxStatus.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 276);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "服务地址：";
            // 
            // tbxFtpServerIp
            // 
            this.tbxFtpServerIp.Location = new System.Drawing.Point(71, 273);
            this.tbxFtpServerIp.Name = "tbxFtpServerIp";
            this.tbxFtpServerIp.Size = new System.Drawing.Size(100, 21);
            this.tbxFtpServerIp.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 279);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "：";
            // 
            // tbxFtpServerPort
            // 
            this.tbxFtpServerPort.Location = new System.Drawing.Point(191, 273);
            this.tbxFtpServerPort.Name = "tbxFtpServerPort";
            this.tbxFtpServerPort.Size = new System.Drawing.Size(46, 21);
            this.tbxFtpServerPort.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(252, 279);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "主目录";
            // 
            // tbxFtpRoot
            // 
            this.tbxFtpRoot.Location = new System.Drawing.Point(299, 273);
            this.tbxFtpRoot.Name = "tbxFtpRoot";
            this.tbxFtpRoot.Size = new System.Drawing.Size(153, 21);
            this.tbxFtpRoot.TabIndex = 6;
            // 
            // btnFtpServerStartStop
            // 
            this.btnFtpServerStartStop.Location = new System.Drawing.Point(458, 274);
            this.btnFtpServerStartStop.Name = "btnFtpServerStartStop";
            this.btnFtpServerStartStop.Size = new System.Drawing.Size(60, 23);
            this.btnFtpServerStartStop.TabIndex = 7;
            this.btnFtpServerStartStop.Text = "启动";
            this.btnFtpServerStartStop.UseVisualStyleBackColor = true;
            this.btnFtpServerStartStop.Click += new System.EventHandler(this.btnFtpServerStartStop_Click);
            // 
            // FtpServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 328);
            this.Controls.Add(this.btnFtpServerStartStop);
            this.Controls.Add(this.tbxFtpRoot);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbxFtpServerPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbxFtpServerIp);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstboxStatus);
            this.Name = "FtpServerForm";
            this.Text = "Ftp服务器";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstboxStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbxFtpServerIp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxFtpServerPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbxFtpRoot;
        private System.Windows.Forms.Button btnFtpServerStartStop;
    }
}

