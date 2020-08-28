using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace 排故好帮手下载工具
{
    public partial class GPN : Form
    {
        public GPN()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void butdownload_Click(object sender, EventArgs e)
        {
            string ver1 = textver1.Text;
            string ver2 = textver2.Text;
            string ver3 = textver3.Text;
            string ver4 = textver4.Text;
            string apppath =  ver1 + "." + ver2 + "." + ver3 + "." + ver4;

            DialogResult dr = MessageBox.Show("是否下载此版本："+apppath +" ？", "提示", MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                Thread download = new Thread(downloadapp);
                download.Start();
                //户选择确认的操作
            }
            else if (dr == DialogResult.No)
            {

                //户选择取消的操作
                return;
            }
            if (dr == DialogResult.Cancel)
            {
                return;
                //户选择确认的操作
            }


        }
        private void downloadapp()
        {


            if (!Directory.Exists(@"C:\gpn\download"))
            {
                Directory.CreateDirectory(@"C:\gpn\download");
            }
            // Textsharp.Text = "程序集版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n";
           // string appversion = Application.ProductVersion.ToString();

           // string[] appver = appversion.Split(new string[] { "." }, StringSplitOptions.None);
            string ver1 = textver1.Text;
            string ver2 = textver2.Text;
            string ver3 = textver3.Text;
            string ver4 = textver4.Text;
            string apppath = @"c:\gpn\download\" + ver1 + "." + ver2 + "." + ver3 + "." + ver4 + @"\";
            if (!Directory.Exists(apppath))
            {
                Directory.CreateDirectory(apppath);
            }
            //Textsharp.AppendText(ver1 + "\r\n");
            //Textsharp.AppendText(ver2 + "\r\n");
            //Textsharp.AppendText(ver3 + "\r\n");
            //Textsharp.AppendText(ver4 + "\r\n");

            string a1 = apppath + "BouncyCastle.Crypto.dll";
            string a2 = apppath + "DotNetZip.dll";
            string a3 = apppath + "ICSharpCode.SharpZipLib.dll";

            string a4 = apppath + "MetroFramework.dll";
            string a5 = apppath + "MetroFramework.Fonts.dll";
            string a6 = apppath + "MySql.Data.dll";
            string a7 = apppath + "NPOI.dll";

            string a8 = apppath + "NPOI.OOXML.dll";
            string a9 = apppath + "NPOI.OpenXml4Net.dll";
            string a10 = apppath + "NPOI.OpenXmlFormats.dll";
            string a11 = apppath + "排故好帮手.exe";
            //string a13 = @"C:\gpn\download\" + "tvnserver.exe";
            if (File.Exists(a11))//读取时先要判读INI文件是否存在
            {
                Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "免安装版已下载，路径是：" + a11 + "\r\n");

                return;
            }
            FileStream stream;
            //string gpnname = comgpn76list.Text;
            string b1 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/BouncyCastle.Crypto.dll";
            string b2 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/DotNetZip.dll";
            string b3 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/ICSharpCode.SharpZipLib.dll";
            string b4 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/MetroFramework.dll";
            string b5 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/MetroFramework.Fonts.dll";
            string b6 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/MySql.Data.dll";
            string b7 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/NPOI.dll";
            string b8 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/NPOI.OOXML.dll";
            string b9 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/NPOI.OpenXml4Net.dll";
            string b10 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/NPOI.OpenXmlFormats.dll";
            string b11 = "http://hunan128.com/ftp/gpn/Application%20Files/排故好帮手_" + ver1 + "_" + ver2 + "_" + ver3 + "_" + ver4 + "/排故好帮手.exe";
            string strZipPath = "";
            string url = "";
            for (int i = 0; i < 11; i++)
            {
                //string strZipPath = @"C:\gpn\" + "frpc_start.exe";
                //   string strUnZipPath = @"C:\gpn\";
                if (i == 0)
                {
                    url = b1;
                    strZipPath = a1;
                }
                if (i == 1)
                {
                    url = b2;
                    strZipPath = a2;
                }
                if (i == 2)
                {
                    url = b3;
                    strZipPath = a3;
                }
                if (i == 3)
                {
                    url = b4;
                    strZipPath = a4;
                }
                if (i == 4)
                {
                    url = b5;
                    strZipPath = a5;
                }
                if (i == 5)
                {
                    url = b6;
                    strZipPath = a6;
                }
                if (i == 6)
                {
                    url = b7;
                    strZipPath = a7;
                }
                if (i == 7)
                {
                    url = b8;
                    strZipPath = a8;
                }
                if (i == 8)
                {
                    url = b9;
                    strZipPath = a9;
                }
                if (i == 9)
                {
                    url = b10;
                    strZipPath = a10;
                }
                if (i == 10)
                {
                    url = b11;
                    strZipPath = a11;
                }

                int percent = 0;
                stream = new FileStream(strZipPath, FileMode.Create);
                // bool overWrite = true;
                try
                {
                    Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + url + " 下载中......" + "\r\n");
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    Stream responseStream = response.GetResponseStream();
                    long totalDownloadedByte = 0;
                    long totalBytes = response.ContentLength;
                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, bArr.Length);
                    while (size > 0)
                    {
                        totalDownloadedByte = size + totalDownloadedByte;
                        stream.Write(bArr, 0, size);
                        size = responseStream.Read(bArr, 0, bArr.Length);
                        //p = (int)Math.Floor((double)100 / a);
                        percent = (int)Math.Floor((float)totalDownloadedByte / (float)totalBytes * 100);
                        // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+percent.ToString() + "\r\n");
                        //labjindu.Text = percent.ToString() + "%";
                        myProgressBarjindu.Value = percent;
                        // System.Windows.Forms.Application.DoEvents();
                    }
                    stream.Close();
                    responseStream.Close();
                    //Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "免安装版下载成功============================================OK" + "\r\n");
                    // Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存路径：" + strZipPath + "\r\n");

                }
                catch (Exception)
                {
                    stream.Close();
                    MessageBox.Show("无法进行下载，请检查下载链接！");
                    //flag = false;       //返回false下载失败
                }
            }
            Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "免安装版下载完成============================================OK" + "\r\n");
            Textsharp.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存路径：" + strZipPath + "\r\n");
            MessageBox.Show("绿色免安装版下载完成，请到 " + apppath + " 文件夹获取。");

        }

        private void butnet_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://download.microsoft.com/download/9/A/7/9A78F13F-FD62-4F6D-AB6B-1803508A9F56/51209.34209.03/web/NDP452-KB2901954-Web.exe");
        }
    }
}
