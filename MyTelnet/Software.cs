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
            System.Diagnostics.Process.Start("http://60.205.155.127/ftp/index.htm");

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
            metroTextBox2.Text = "1、百度云网盘需下载整个【排故好帮手】目录文件，方可正常运行。" +
                "\r\n"+"2、下载后按照下方图片内容进行双击运行。";
        }

    }
}
