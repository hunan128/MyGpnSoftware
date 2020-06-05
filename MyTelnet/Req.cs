using MetroFramework.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class Req : MetroForm
    {
        public string ShebeiIp;

        public Req()
        {
            InitializeComponent();


            strSec = Path.GetFileNameWithoutExtension(strFilePath);
            textname.Text = ContentValue(strSec, "Name");
            comcity.Text = ContentValue(strSec, "City");
            textphone.Text = ContentValue(strSec, "Phone");

            if (ContentValue(strSec, "CcMail") != "")
            {
                textCcMail.Text = ContentValue(strSec, "CcMail");
            }


        }

    
    private void Butsend_Click(object sender, EventArgs e)
        {

            Thread CycleThread = new Thread(SendMail)
            {
                IsBackground = true
            };
            CycleThread.Start();


        }

        #region 声明ini变量
        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节点名称[如[TypeName]]</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
        // private string strFilePath = Application.StartupPath + "\\Config.ini";//获取INI文件路径
        private string strFilePath = @"C:\gpn\Config.ini";
        private string strSec = ""; //INI文件名
        private string ContentValue(string Section, string key)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, strFilePath);
            return temp.ToString();
        }
        #endregion


        private void SendMail() {


            try
            {
                //根据INI文件名设置要写入INI文件的节点名称
                //此处的节点名称完全可以根据实际需要进行配置
                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                WritePrivateProfileString(strSec, "Name", textname.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "City", comcity.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "Phone", textphone.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "CcMail", textCcMail.Text.Trim(), strFilePath);



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }




            string EmailUser = "sxhunan@163.com";
            string EmailPassd = "hunan7420716";
            string Smtp = "smtp.163.com";
            string ToMail = "sxhunan@163.com";
            string CcMail = textCcMail.Text;
            string EmailFromName = "排故好帮手";
            string EmailBody = "姓名：" + textname.Text
                 + "\r\n" + "办事处：" + comcity.Text
                 + "\r\n" + "联系电话：" + textphone.Text
                 + "\r\n" + "反馈内容：" + textreq.Text
                 + "\r\n";
            string EmailSub = comcity.Text + "：" + textname.Text + "的问题反馈";


            string pLocalFilePath = @"C:\gpn\Logs\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + ShebeiIp + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";//要复制的文件路径




            // 如有有密码 选中复选框
            try
            {
                if (string.IsNullOrWhiteSpace(ToMail))
                {
                    labelMesg.Text = "收件人不能为空";
                    return;
                    // MessageBox.Show(@"收件人不能为空！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (string.IsNullOrWhiteSpace(EmailBody))
                {
                    labelMesg.Text = "邮件内容不能为空";
                    return;
                    //MessageBox.Show(@"邮件内容不能为空！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (string.IsNullOrWhiteSpace(EmailSub))
                {
                    labelMesg.Text = "邮件主题不能为空";
                    return;
                    //MessageBox.Show(@"邮件内容不能为空！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (string.IsNullOrWhiteSpace(Smtp))
                {
                    labelMesg.Text = "请检查SMTP配置";
                    return;
                    //MessageBox.Show(@"邮件内容不能为空！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //實例化附件類

                

                var Sub = EmailSub;
                var Body = EmailBody;
                MailMessage message = new MailMessage();
                //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                MailAddress fromAddr = new MailAddress(EmailUser, EmailFromName);
                message.From = fromAddr;
                //设置收件人,可添加多个,添加方法与下面的一样

                Attachment amAnnex = new Attachment(pLocalFilePath);
                message.Attachments.Add(amAnnex);
                //设置抄送人
                if (metroCheckme.Checked == true)
                {
                    message.To.Add(CcMail);

                }
                else
                {
                    message.To.Add(ToMail);
                    if (CcMail.Contains("@"))
                    {
                        message.CC.Add(CcMail);
                    }

                }
                //设置邮件标题
                message.Subject = Sub;
                //附件



                //设置邮件内容
                message.Body = Body;
                //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                SmtpClient client = new SmtpClient(Smtp, 25);
                //设置发送人的邮箱账号和密码
                client.Credentials = new NetworkCredential(EmailUser, EmailPassd);
                //启用ssl,也就是安全发送
                client.EnableSsl = false;
                //发送邮件
                client.Send(message);
                labelMesg.Text = "邮件发送成功!";
                MessageBox.Show("邮件发送成功!");
                amAnnex.ContentStream.Close();

            }
            catch (Exception ex)
            {

                labelMesg.Text = ex.Message;
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }

        }


        private void Req_Load(object sender, EventArgs e)
        {

        }
    }
}
