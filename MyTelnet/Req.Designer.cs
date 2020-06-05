namespace MyGpnSoftware
{
    partial class Req
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Req));
            this.comcity = new MetroFramework.Controls.MetroComboBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroCheckFile = new MetroFramework.Controls.MetroCheckBox();
            this.metroCheckme = new MetroFramework.Controls.MetroCheckBox();
            this.Butsend = new MetroFramework.Controls.MetroButton();
            this.metroLabel6 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.labelMesg = new MetroFramework.Controls.MetroTextBox();
            this.textCcMail = new MetroFramework.Controls.MetroTextBox();
            this.textphone = new MetroFramework.Controls.MetroTextBox();
            this.textname = new MetroFramework.Controls.MetroTextBox();
            this.textreq = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // comcity
            // 
            this.comcity.FormattingEnabled = true;
            this.comcity.ItemHeight = 23;
            this.comcity.Items.AddRange(new object[] {
            "一区-福建办",
            "一区-北京办",
            "一区-湖南办",
            "一区-河北办",
            "二区-甘肃办",
            "二区-黑龙江办",
            "二区-四川办",
            "三区-上海办",
            "三区-广东办",
            "三区-河南办",
            "三区-安徽办",
            "总部-技术支持部",
            "总部-测试部",
            "总部-软件部",
            "总部-硬件部",
            "总部-网管部"});
            this.comcity.Location = new System.Drawing.Point(103, 269);
            this.comcity.Name = "comcity";
            this.comcity.Size = new System.Drawing.Size(341, 29);
            this.comcity.TabIndex = 280;
            this.comcity.UseSelectable = true;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(24, 66);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(65, 19);
            this.metroLabel1.TabIndex = 271;
            this.metroLabel1.Text = "问题描述";
            // 
            // metroCheckFile
            // 
            this.metroCheckFile.AutoSize = true;
            this.metroCheckFile.Checked = true;
            this.metroCheckFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.metroCheckFile.Location = new System.Drawing.Point(104, 317);
            this.metroCheckFile.Name = "metroCheckFile";
            this.metroCheckFile.Size = new System.Drawing.Size(75, 15);
            this.metroCheckFile.TabIndex = 282;
            this.metroCheckFile.Text = "附件日志";
            this.metroCheckFile.UseSelectable = true;
            // 
            // metroCheckme
            // 
            this.metroCheckme.AutoSize = true;
            this.metroCheckme.Location = new System.Drawing.Point(180, 317);
            this.metroCheckme.Name = "metroCheckme";
            this.metroCheckme.Size = new System.Drawing.Size(114, 15);
            this.metroCheckme.TabIndex = 281;
            this.metroCheckme.Text = "邮件只发送自己";
            this.metroCheckme.UseSelectable = true;
            // 
            // Butsend
            // 
            this.Butsend.BackColor = System.Drawing.Color.DodgerBlue;
            this.Butsend.Location = new System.Drawing.Point(103, 342);
            this.Butsend.Name = "Butsend";
            this.Butsend.Size = new System.Drawing.Size(343, 50);
            this.Butsend.TabIndex = 277;
            this.Butsend.Text = "发送邮件";
            this.Butsend.UseSelectable = true;
            this.Butsend.Click += new System.EventHandler(this.Butsend_Click);
            // 
            // metroLabel6
            // 
            this.metroLabel6.AutoSize = true;
            this.metroLabel6.Location = new System.Drawing.Point(23, 251);
            this.metroLabel6.Name = "metroLabel6";
            this.metroLabel6.Size = new System.Drawing.Size(65, 19);
            this.metroLabel6.TabIndex = 276;
            this.metroLabel6.Text = "抄送自己";
            // 
            // metroLabel4
            // 
            this.metroLabel4.AutoSize = true;
            this.metroLabel4.Location = new System.Drawing.Point(24, 217);
            this.metroLabel4.Name = "metroLabel4";
            this.metroLabel4.Size = new System.Drawing.Size(65, 19);
            this.metroLabel4.TabIndex = 274;
            this.metroLabel4.Text = "联系电话";
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(23, 273);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(51, 19);
            this.metroLabel3.TabIndex = 273;
            this.metroLabel3.Text = "办事处";
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(25, 188);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(37, 19);
            this.metroLabel2.TabIndex = 272;
            this.metroLabel2.Text = "姓名";
            // 
            // labelMesg
            // 
            // 
            // 
            // 
            this.labelMesg.CustomButton.Image = null;
            this.labelMesg.CustomButton.Location = new System.Drawing.Point(124, 1);
            this.labelMesg.CustomButton.Name = "";
            this.labelMesg.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.labelMesg.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.labelMesg.CustomButton.TabIndex = 1;
            this.labelMesg.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.labelMesg.CustomButton.UseSelectable = true;
            this.labelMesg.CustomButton.Visible = false;
            this.labelMesg.Enabled = false;
            this.labelMesg.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.labelMesg.Lines = new string[] {
        "发送邮件需等待5-10秒"};
            this.labelMesg.Location = new System.Drawing.Point(295, 307);
            this.labelMesg.MaxLength = 32767;
            this.labelMesg.Name = "labelMesg";
            this.labelMesg.PasswordChar = '\0';
            this.labelMesg.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.labelMesg.SelectedText = "";
            this.labelMesg.SelectionLength = 0;
            this.labelMesg.SelectionStart = 0;
            this.labelMesg.ShortcutsEnabled = true;
            this.labelMesg.Size = new System.Drawing.Size(152, 29);
            this.labelMesg.TabIndex = 270;
            this.labelMesg.Text = "发送邮件需等待5-10秒";
            this.labelMesg.UseSelectable = true;
            this.labelMesg.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.labelMesg.WaterMarkFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // textCcMail
            // 
            // 
            // 
            // 
            this.textCcMail.CustomButton.Image = null;
            this.textCcMail.CustomButton.Location = new System.Drawing.Point(314, 1);
            this.textCcMail.CustomButton.Name = "";
            this.textCcMail.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.textCcMail.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textCcMail.CustomButton.TabIndex = 1;
            this.textCcMail.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textCcMail.CustomButton.UseSelectable = true;
            this.textCcMail.CustomButton.Visible = false;
            this.textCcMail.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textCcMail.Lines = new string[] {
        "@qq.com"};
            this.textCcMail.Location = new System.Drawing.Point(103, 241);
            this.textCcMail.MaxLength = 32767;
            this.textCcMail.Name = "textCcMail";
            this.textCcMail.PasswordChar = '\0';
            this.textCcMail.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textCcMail.SelectedText = "";
            this.textCcMail.SelectionLength = 0;
            this.textCcMail.SelectionStart = 0;
            this.textCcMail.ShortcutsEnabled = true;
            this.textCcMail.Size = new System.Drawing.Size(342, 29);
            this.textCcMail.TabIndex = 269;
            this.textCcMail.Text = "@qq.com";
            this.textCcMail.UseSelectable = true;
            this.textCcMail.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textCcMail.WaterMarkFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // textphone
            // 
            // 
            // 
            // 
            this.textphone.CustomButton.Image = null;
            this.textphone.CustomButton.Location = new System.Drawing.Point(314, 1);
            this.textphone.CustomButton.Name = "";
            this.textphone.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.textphone.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textphone.CustomButton.TabIndex = 1;
            this.textphone.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textphone.CustomButton.UseSelectable = true;
            this.textphone.CustomButton.Visible = false;
            this.textphone.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textphone.Lines = new string[0];
            this.textphone.Location = new System.Drawing.Point(103, 213);
            this.textphone.MaxLength = 32767;
            this.textphone.Name = "textphone";
            this.textphone.PasswordChar = '\0';
            this.textphone.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textphone.SelectedText = "";
            this.textphone.SelectionLength = 0;
            this.textphone.SelectionStart = 0;
            this.textphone.ShortcutsEnabled = true;
            this.textphone.Size = new System.Drawing.Size(342, 29);
            this.textphone.TabIndex = 268;
            this.textphone.UseSelectable = true;
            this.textphone.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textphone.WaterMarkFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // textname
            // 
            // 
            // 
            // 
            this.textname.CustomButton.Image = null;
            this.textname.CustomButton.Location = new System.Drawing.Point(314, 1);
            this.textname.CustomButton.Name = "";
            this.textname.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.textname.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textname.CustomButton.TabIndex = 1;
            this.textname.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textname.CustomButton.UseSelectable = true;
            this.textname.CustomButton.Visible = false;
            this.textname.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.textname.Lines = new string[0];
            this.textname.Location = new System.Drawing.Point(103, 186);
            this.textname.MaxLength = 32767;
            this.textname.Name = "textname";
            this.textname.PasswordChar = '\0';
            this.textname.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.textname.SelectedText = "";
            this.textname.SelectionLength = 0;
            this.textname.SelectionStart = 0;
            this.textname.ShortcutsEnabled = true;
            this.textname.Size = new System.Drawing.Size(342, 29);
            this.textname.TabIndex = 266;
            this.textname.UseSelectable = true;
            this.textname.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textname.WaterMarkFont = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // textreq
            // 
            // 
            // 
            // 
            this.textreq.CustomButton.Image = null;
            this.textreq.CustomButton.Location = new System.Drawing.Point(226, 1);
            this.textreq.CustomButton.Name = "";
            this.textreq.CustomButton.Size = new System.Drawing.Size(115, 115);
            this.textreq.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.textreq.CustomButton.TabIndex = 1;
            this.textreq.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.textreq.CustomButton.UseSelectable = true;
            this.textreq.CustomButton.Visible = false;
            this.textreq.Lines = new string[] {
        "问题描述"};
            this.textreq.Location = new System.Drawing.Point(104, 63);
            this.textreq.MaxLength = 32767;
            this.textreq.Multiline = true;
            this.textreq.Name = "textreq";
            this.textreq.PasswordChar = '\0';
            this.textreq.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textreq.SelectedText = "";
            this.textreq.SelectionLength = 0;
            this.textreq.SelectionStart = 0;
            this.textreq.ShortcutsEnabled = true;
            this.textreq.Size = new System.Drawing.Size(342, 117);
            this.textreq.TabIndex = 265;
            this.textreq.Text = "问题描述";
            this.textreq.UseSelectable = true;
            this.textreq.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.textreq.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // Req
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 456);
            this.Controls.Add(this.metroCheckFile);
            this.Controls.Add(this.metroCheckme);
            this.Controls.Add(this.Butsend);
            this.Controls.Add(this.metroLabel6);
            this.Controls.Add(this.metroLabel4);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.labelMesg);
            this.Controls.Add(this.textCcMail);
            this.Controls.Add(this.textphone);
            this.Controls.Add(this.comcity);
            this.Controls.Add(this.textname);
            this.Controls.Add(this.textreq);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Req";
            this.Text = "问题反馈";
            this.Load += new System.EventHandler(this.Req_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MetroFramework.Controls.MetroComboBox comcity;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroCheckBox metroCheckFile;
        private MetroFramework.Controls.MetroCheckBox metroCheckme;
        private MetroFramework.Controls.MetroButton Butsend;
        private MetroFramework.Controls.MetroLabel metroLabel6;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox labelMesg;
        private MetroFramework.Controls.MetroTextBox textCcMail;
        private MetroFramework.Controls.MetroTextBox textphone;
        private MetroFramework.Controls.MetroTextBox textname;
        private MetroFramework.Controls.MetroTextBox textreq;
    }
}