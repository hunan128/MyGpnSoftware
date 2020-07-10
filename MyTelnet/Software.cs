using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class Software : MetroForm
    {
        public Software()
        {
            InitializeComponent();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText("yhdl");
        }


        private void metroLink1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://hunan128.com/");

        }

        private void metroLink2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://pan.baidu.com/s/1j6B3QX6Zue9d7XoLRPlNQg");

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(@"https://pan.baidu.com/s/1j6B3QX6Zue9d7XoLRPlNQg");

        }

        private void Software_Load(object sender, EventArgs e)
        {
            metroTextBox2.Text = "官方唯一网站：www.hunan128.com \r\n官方唯一公网：hunan128.com";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://hunan128.com/");
        }
    }
}
