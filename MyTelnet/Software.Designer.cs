namespace MyGpnSoftware
{
    partial class Software
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Software));
            this.Textsharp = new MetroFramework.Controls.MetroTextBox();
            this.metroLink1 = new MetroFramework.Controls.MetroLink();
            this.metroButStartVnc = new MetroFramework.Controls.MetroButton();
            this.myProgressBarjindu = new MetroFramework.Controls.MetroProgressBar();
            this.SuspendLayout();
            // 
            // Textsharp
            // 
            this.Textsharp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // 
            // 
            this.Textsharp.CustomButton.Image = null;
            this.Textsharp.CustomButton.Location = new System.Drawing.Point(553, 2);
            this.Textsharp.CustomButton.Name = "";
            this.Textsharp.CustomButton.Size = new System.Drawing.Size(231, 231);
            this.Textsharp.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.Textsharp.CustomButton.TabIndex = 1;
            this.Textsharp.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.Textsharp.CustomButton.UseSelectable = true;
            this.Textsharp.CustomButton.Visible = false;
            this.Textsharp.Lines = new string[0];
            this.Textsharp.Location = new System.Drawing.Point(23, 63);
            this.Textsharp.MaxLength = 32767;
            this.Textsharp.Multiline = true;
            this.Textsharp.Name = "Textsharp";
            this.Textsharp.PasswordChar = '\0';
            this.Textsharp.ReadOnly = true;
            this.Textsharp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Textsharp.SelectedText = "";
            this.Textsharp.SelectionLength = 0;
            this.Textsharp.SelectionStart = 0;
            this.Textsharp.ShortcutsEnabled = true;
            this.Textsharp.Size = new System.Drawing.Size(787, 236);
            this.Textsharp.TabIndex = 5;
            this.Textsharp.UseSelectable = true;
            this.Textsharp.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.Textsharp.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLink1
            // 
            this.metroLink1.Location = new System.Drawing.Point(23, 332);
            this.metroLink1.Name = "metroLink1";
            this.metroLink1.Size = new System.Drawing.Size(181, 36);
            this.metroLink1.TabIndex = 7;
            this.metroLink1.Text = "官方网站唯一下载网站";
            this.metroLink1.UseSelectable = true;
            this.metroLink1.Click += new System.EventHandler(this.metroLink1_Click);
            // 
            // metroButStartVnc
            // 
            this.metroButStartVnc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButStartVnc.Location = new System.Drawing.Point(650, 333);
            this.metroButStartVnc.Name = "metroButStartVnc";
            this.metroButStartVnc.Size = new System.Drawing.Size(160, 35);
            this.metroButStartVnc.TabIndex = 43;
            this.metroButStartVnc.Text = "绿色版下载";
            this.metroButStartVnc.UseSelectable = true;
            this.metroButStartVnc.Click += new System.EventHandler(this.metroButStartVnc_Click);
            // 
            // myProgressBarjindu
            // 
            this.myProgressBarjindu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.myProgressBarjindu.Location = new System.Drawing.Point(24, 306);
            this.myProgressBarjindu.Name = "myProgressBarjindu";
            this.myProgressBarjindu.Size = new System.Drawing.Size(786, 23);
            this.myProgressBarjindu.TabIndex = 44;
            // 
            // Software
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 397);
            this.Controls.Add(this.myProgressBarjindu);
            this.Controls.Add(this.metroButStartVnc);
            this.Controls.Add(this.metroLink1);
            this.Controls.Add(this.Textsharp);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Software";
            this.Text = "官方网站";
            this.Load += new System.EventHandler(this.Software_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private MetroFramework.Controls.MetroTextBox Textsharp;
        private MetroFramework.Controls.MetroLink metroLink1;
        private MetroFramework.Controls.MetroButton metroButStartVnc;
        private MetroFramework.Controls.MetroProgressBar myProgressBarjindu;
    }
}