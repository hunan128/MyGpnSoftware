namespace 排故好帮手下载工具
{
    partial class GPN
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GPN));
            this.Textsharp = new System.Windows.Forms.RichTextBox();
            this.textver1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textver2 = new System.Windows.Forms.TextBox();
            this.textver3 = new System.Windows.Forms.TextBox();
            this.textver4 = new System.Windows.Forms.TextBox();
            this.butdownload = new System.Windows.Forms.Button();
            this.myProgressBarjindu = new System.Windows.Forms.ProgressBar();
            this.butnet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Textsharp
            // 
            this.Textsharp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Textsharp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Textsharp.Location = new System.Drawing.Point(28, 12);
            this.Textsharp.Name = "Textsharp";
            this.Textsharp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.Textsharp.Size = new System.Drawing.Size(816, 309);
            this.Textsharp.TabIndex = 0;
            this.Textsharp.Text = "";
            // 
            // textver1
            // 
            this.textver1.Location = new System.Drawing.Point(549, 366);
            this.textver1.Name = "textver1";
            this.textver1.Size = new System.Drawing.Size(32, 21);
            this.textver1.TabIndex = 1;
            this.textver1.Text = "4";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(500, 371);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "版本号";
            // 
            // textver2
            // 
            this.textver2.Location = new System.Drawing.Point(587, 366);
            this.textver2.Name = "textver2";
            this.textver2.Size = new System.Drawing.Size(32, 21);
            this.textver2.TabIndex = 3;
            this.textver2.Text = "1";
            // 
            // textver3
            // 
            this.textver3.Location = new System.Drawing.Point(625, 366);
            this.textver3.Name = "textver3";
            this.textver3.Size = new System.Drawing.Size(32, 21);
            this.textver3.TabIndex = 4;
            this.textver3.Text = "2";
            // 
            // textver4
            // 
            this.textver4.Location = new System.Drawing.Point(663, 366);
            this.textver4.Name = "textver4";
            this.textver4.Size = new System.Drawing.Size(32, 21);
            this.textver4.TabIndex = 5;
            this.textver4.Text = "50";
            // 
            // butdownload
            // 
            this.butdownload.Location = new System.Drawing.Point(720, 356);
            this.butdownload.Name = "butdownload";
            this.butdownload.Size = new System.Drawing.Size(124, 39);
            this.butdownload.TabIndex = 6;
            this.butdownload.Text = "下载排故好帮手";
            this.butdownload.UseVisualStyleBackColor = true;
            this.butdownload.Click += new System.EventHandler(this.butdownload_Click);
            // 
            // myProgressBarjindu
            // 
            this.myProgressBarjindu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.myProgressBarjindu.Location = new System.Drawing.Point(28, 327);
            this.myProgressBarjindu.Name = "myProgressBarjindu";
            this.myProgressBarjindu.Size = new System.Drawing.Size(816, 23);
            this.myProgressBarjindu.TabIndex = 7;
            // 
            // butnet
            // 
            this.butnet.Location = new System.Drawing.Point(28, 354);
            this.butnet.Name = "butnet";
            this.butnet.Size = new System.Drawing.Size(124, 39);
            this.butnet.TabIndex = 8;
            this.butnet.Text = ".Net4.5框架 下载";
            this.butnet.UseVisualStyleBackColor = true;
            this.butnet.Click += new System.EventHandler(this.butnet_Click);
            // 
            // GPN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 440);
            this.Controls.Add(this.butnet);
            this.Controls.Add(this.myProgressBarjindu);
            this.Controls.Add(this.butdownload);
            this.Controls.Add(this.textver4);
            this.Controls.Add(this.textver3);
            this.Controls.Add(this.textver2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textver1);
            this.Controls.Add(this.Textsharp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GPN";
            this.Text = "排故好帮手下载工具";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Textsharp;
        private System.Windows.Forms.TextBox textver1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textver2;
        private System.Windows.Forms.TextBox textver3;
        private System.Windows.Forms.TextBox textver4;
        private System.Windows.Forms.Button butdownload;
        private System.Windows.Forms.ProgressBar myProgressBarjindu;
        private System.Windows.Forms.Button butnet;
    }
}

