using Ionic.Zip;
using MetroFramework.Forms;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using SnmpSharpNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MyGpnSoftware
{
    public partial class GPN : MetroForm
    {
        #region 全局变量的声明
        private MySocket mysocket;
        public static string com = "";                      //
        public static string GPN7600EMSURLIP = "192.168.10.101";                                    //GPN模块服务器ip地址
        public static string GPN7600EMSURL = "http://192.168.10.101/dist/gpn7600/nightly/1.0.x/";//GPN模块获取地址
        public static string ts = "10";                //获取socket数据回复等待时间           
        public static long Filesize = 0;                              //下载生在文件流大小
        public static bool uploading = false;            //文件比对，检查是否在升级中
        public static string slot18 = "";               //18槽位状态
        public static string slot11 = "";               //11槽位状态
        public static string slotsw = "";               //SW型号状态
        public static string slot17 = "";               //17槽位状态
        public static string slot12 = "";               //12槽位状态
        public static string sw = "";                   //SW型号状态
        public static string defaultfilePath = "";       //打开文件夹默认路径
        public static string version = "";              //设备版本号
        TcpListener myTcpListener = null;
        private Thread listenThread;
        public int XHTime = 1000;                       //循环间隔时间
        public int XHCount = 720;                       //循环次数
        public static string devtype = "";              //设备类型
        public bool backupfile = false;
        public bool FtpStatusEnable = false;            //FTPserver是否为使能状态
        public bool FtpPortEnable = false;            //FTP 接口是否为使能状态
        public static string ftpCtrlFlagID = "";                //执行操作命令
        public static int LoadCountany = 0;                //上传下载设备个数
        public static int LoadCountsum = 0;                //上传下载设备个数
        public static object PiLiangShengJi = new object(); //批量升级累计完成数量加锁


        // 保存户名和密码
        Dictionary<string, string> users;
        #endregion
        public GPN()
        {
            InitializeComponent();
            mysocket = new MySocket();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            #region 打印网卡的ip地址
            ////打印网卡的ip地址
            NetworkInterface[] NetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            string ftpipall = "";
            foreach (NetworkInterface NetworkIntf in NetworkInterfaces)
            {
                IPInterfaceProperties IPInterfaceProperties = NetworkIntf.GetIPProperties();
                UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = IPInterfaceProperties.UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
                {
                    if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ftpipall = UnicastIPAddressInformation.Address.ToString();
                        if (!ftpipall.Contains("169.") && !ftpipall.Contains("127.0.0.1"))
                        {
                            comftpip.Items.Add(ftpipall.ToString());
                            if (comftpip.Items.Count > 0)
                            {
                                comftpip.SelectedIndex = comftpip.Items.Count - 1;
                            }
                        }
                    }
                }
            }
            #endregion
            #region 读取ini文件
            try
            {
                if (!Directory.Exists(@"C:\gpn"))
                {
                    Directory.CreateDirectory(@"C:\gpn");
                }
                gpnurlupdate();
                //   导入前俩列ToolStripMenuItem.PerformClick();

                if (File.Exists(strFilePath))//读取时先要判读INI文件是否存在
                {
                    strSec = Path.GetFileNameWithoutExtension(strFilePath);
                    if (comftpip.Items.Contains(ContentValue(strSec, "FTPip")))
                    {
                        comftpip.Text = ContentValue(strSec, "FTPip");
                    }
                    tbxFtpServerPort.Text = ContentValue(strSec, "FTPport");
                    textftpusr.Text = ContentValue(strSec, "FTPuser");
                    textftppsd.Text = ContentValue(strSec, "FTPpsd");
                    tbxFtpRoot.Text = ContentValue(strSec, "FTPpath");
                    if (ContentValue(strSec, "ReadCommunity") != "")
                    {
                        textReadCommunity.Text = ContentValue(strSec, "ReadCommunity");
                    }
                    if (ContentValue(strSec, "WriteCommunity") != "")
                    {
                        textWriteCommunity.Text = ContentValue(strSec, "WriteCommunity");
                    }
                    readgpnip();
                    if (ContentValue(strSec, "GPNip") != "")
                    {
                        comip.Text = ContentValue(strSec, "GPNip");
                    }
                    textusr.Text = ContentValue(strSec, "GPNuser");
                    textpsd.Text = ContentValue(strSec, "GPNpsd");
                    textpsden.Text = ContentValue(strSec, "GPNpsden");
                    if (Directory.Exists(tbxFtpRoot.Text))
                    {
                        Readfile(tbxFtpRoot.Text);
                    }
                    if (comapp.Items.Contains(ContentValue(strSec, "APP")))
                    {
                        comapp.Text = ContentValue(strSec, "APP");
                    }
                    if (comcode.Items.Contains(ContentValue(strSec, "FPFA_CODE")))
                    {
                        comcode.Text = ContentValue(strSec, "FPFA_CODE");
                    }
                    if (comnms.Items.Contains(ContentValue(strSec, "NMS")))
                    {
                        comnms.Text = ContentValue(strSec, "NMS");
                    }
                    if (comsw.Items.Contains(ContentValue(strSec, "SW")))
                    {
                        comsw.Text = ContentValue(strSec, "SW");
                    }
                    if (com760s.Items.Contains(ContentValue(strSec, "760S")))
                    {
                        com760s.Text = ContentValue(strSec, "760S");
                    }
                    if (com760b.Items.Contains(ContentValue(strSec, "760B")))
                    {
                        com760b.Text = ContentValue(strSec, "760B");
                    }
                    if (com760c.Items.Contains(ContentValue(strSec, "760C")))
                    {
                        com760c.Text = ContentValue(strSec, "760C");
                    }
                    if (com760d.Items.Contains(ContentValue(strSec, "760D")))
                    {
                        com760d.Text = ContentValue(strSec, "760D");
                    }
                    if (com760e.Items.Contains(ContentValue(strSec, "760E")))
                    {
                        com760e.Text = ContentValue(strSec, "760E");
                    }
                    if (com760f.Items.Contains(ContentValue(strSec, "760F")))
                    {
                        com760f.Text = ContentValue(strSec, "760F");
                    }
                    if (comsysfile.Items.Contains(ContentValue(strSec, "sysfile")))
                    {
                        comsysfile.Text = ContentValue(strSec, "sysfile");
                    }
                    if (comflash.Items.Contains(ContentValue(strSec, "FLASH")))
                    {
                        comflash.Text = ContentValue(strSec, "FLASH");
                    }
                    if (comyaffs.Items.Contains(ContentValue(strSec, "YAFFS")))
                    {
                        comyaffs.Text = ContentValue(strSec, "YAFFS");
                    }
                    comgpn76list.Text = ContentValue(strSec, "GPN7600EMS");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            #endregion
        }
        private Dictionary<string, Gpnip> userss = new Dictionary<string, Gpnip>();
        private Dictionary<string, Batchip> batchipread = new Dictionary<string, Batchip>();
        private void readgpnip()
        {
            //FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
            //if (fs.Length > 0)
            //{
            //    try
            //    {
            //        BinaryFormatter bf = new BinaryFormatter();
            //        //读出存在Data.bin 里的用户信息
            //        userss = bf.Deserialize(fs) as Dictionary<string, Gpnip>;
            //        //循环添加到Combox1
            //        int n = 1;
            //        foreach (Gpnip user in userss.Values)
            //        {
            //            int index = DGVSTATUS.Rows.Add();

            //            comip.Items.Add(user.GpnIP);
            //            DGVSTATUS.Rows[index].Cells["ip地址"].Value = user.GpnIP;
            //            DGVSTATUS.Rows[index].Cells["优先级"].Value = n;
            //            DGVSTATUS.Rows[index].Cells["执行"].Value = true;
            //            n++;

            //        }
            //        //combox1 用户名默认选中第一个
            //        if (comip.Items.Count > 0)
            //        {
            //            comip.SelectedIndex = comip.Items.Count - 1;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        fs.Close();
            //        File.Delete(@"C:\gpn\gpnip.bin");
            //        MessageBox.Show(ex.Message);
            //    }

            //}
            //DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //fs.Close();



            FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
            if (fs.Length > 0)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //读出存在Data.bin 里的用户信息
                    userss = bf.Deserialize(fs) as Dictionary<string, Gpnip>;
                    //循环添加到Combox1
                    int n = 1;
                    foreach (Gpnip user in userss.Values)
                    {
                        int index = DGVSTATUS.Rows.Add();

                        comip.Items.Add(user.GpnIP);
                        DGVSTATUS.Rows[index].Cells["ip地址"].Value = user.GpnIP;

                            DGVSTATUS.Rows[index].Cells["优先级"].Value = user.GpnPRY;
                            DGVSTATUS.Rows[index].Cells["执行"].Value = user.GpnZX;


                        n++;

                    }
                    //combox1 用户名默认选中第一个
                    if (comip.Items.Count > 0)
                    {
                        comip.SelectedIndex = comip.Items.Count - 1;
                    }
                }
                catch (Exception ex)
                {
                    fs.Close();
                    File.Delete(@"C:\gpn\gpnip.bin");
                    MessageBox.Show(ex.Message);
                }

            }
            DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            fs.Close();











        }
        private void readbatchip()
        {
            FileStream fs = new FileStream(@"C:\gpn\batchip.bin", FileMode.OpenOrCreate);
            if (fs.Length > 0)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //读出存在Data.bin 里的用户信息
                    batchipread = bf.Deserialize(fs) as Dictionary<string, Batchip>;
                    //循环添加到Combox1

                    foreach (Batchip ID in batchipread.Values)
                    {

                        int index = DGVSTATUS.Rows.Add();
                        DGVSTATUS.Rows[index].Cells["ip地址"].Value = ID.BatchIP;

                        DGVSTATUS.Rows[index].Cells["优先级"].Value = ID.Pry;
                        MessageBox.Show(index.ToString());
                    }

                }
                catch (Exception ex)
                {
                    fs.Close();
                    File.Delete(@"C:\gpn\batchip.bin");
                    MessageBox.Show(ex.Message);
                }

                DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            fs.Close();
        }
        #region ③启动FTP服务器



        // 启动服务器
        private void BtnFtpServerStartStop_Click(object sender, EventArgs e)
        {
            if (textftpusr.Text == "" || textftppsd.Text == "")
            {
                MessageBox.Show("请填写用户名密码后，点击③启动FTP服务器！");
                return;
            }
            if (tbxFtpRoot.Text == "")
            {
                MessageBox.Show("请选择FTP根目录！");
                butapp.PerformClick();
                //btnFtpServerStartStop.PerformClick();
            }
            else
            {
                if (myTcpListener == null)
                {
                    IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                    IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
                    foreach (IPEndPoint endPoint in ipEndPoints)
                    {
                        int A = int.Parse(tbxFtpServerPort.Text);
                        if (endPoint.Port == A)
                        {
                            FtpPortEnable = true;
                            Process[] pro = Process.GetProcesses();
                            int cunt = 0;
                            foreach (var item in pro)
                            {
                                if (item.ProcessName == "排故好帮手")
                                {
                                    cunt++;

                                }

                            }
                            if (cunt >= 2)
                            {
                                FtpStatusEnable = true;
                                lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " FTP服务器已经在另一个【排故好帮手】开启，日志信息请到所在FTP服务器中查看！");
                                return;
                            }
                            if (cunt == 1 || cunt == 0)
                            {
                                lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ③启动FTP服务器失败!--------------21号端口已被占用！");
                                MessageBox.Show(A + "号端口已占用，请关闭其它FTP软件后，再次尝试！");
                                return;
                            }

                        }
                    }
                    // 初始化户名和密码
                    users = new Dictionary<string, string>();
                    users.Add(textftpusr.Text, textftppsd.Text);
                    // 新建线程开始启动FTP
                    listenThread = new Thread(ListenClientConnect);
                    listenThread.IsBackground = true;
                    listenThread.Start();
                    lstboxStatus.Enabled = true;
                    toolStripStatusLabelreq.Text = "0";
                    //lstboxStatus.Items.Clear();
                    //lstboxStatus.Items.Add("正在③启动FTP服务器...");
                    comftpip.Enabled = false;
                    FtpStatusEnable = true;
                    FtpPortEnable = true;
                    btnFtpServerStartStop.Text = "③停止FTP服务器";
                }
                else
                {
                    myTcpListener.Stop();
                    myTcpListener = null;
                    listenThread.Abort();
                    lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ③断开FTP服务器成功!--------------IP地址是：" + comftpip.Text);
                    //lstboxStatus.TopIndex = lstboxStatus.Items.Count - 1;
                    FtpStatusEnable = false;
                    FtpPortEnable = false;
                    btnFtpServerStartStop.Text = "③启动FTP服务器";
                    comftpip.Enabled = true;
                }
            }
        }
        #endregion
        #region 监听端口
        // 监听端口，处理客户端连接
        private void ListenClientConnect()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            Ping ping = new Ping();
            int timeout = 500;
            PingReply pingReply = ping.Send(comftpip.Text, timeout);
            // MessageBox.Show(ip);
            //判断请求是否超时
            for (int j = 0; j <= 5; j++)
            {
                pingReply = ping.Send(comftpip.Text, timeout);
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                myTcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), int.Parse(tbxFtpServerPort.Text));
                // 开始监听传入的请求
                myTcpListener.Start();
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ③启动FTP服务器成功!--------------IP地址是：" + comftpip.Text);
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"开始监听用户端请求....");
                //          AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"Ftp服务器运行中...[点击”停止“按钮停止FTP服务]");
                while (true)
                {
                    try
                    {
                        // 接收连接请求
                        TcpClient tcpClient = myTcpListener.AcceptTcpClient();
                        //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+string.Format("客户端（{0}）与本机（{1}）建立FTP连接", tcpClient.Client.RemoteEndPoint, myTcpListener.LocalEndpoint));
                        User user = new User
                        {
                            CommandSession = new UserSeesion(tcpClient),
                            WorkDir = tbxFtpRoot.Text
                        };
                        ParameterizedThreadStart p = new ParameterizedThreadStart(UserProcessing);
                        Thread t = new Thread(p);
                        t.IsBackground = true;
                        t.Start(user);
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " FTP服务启动失败!--------------FTP服务启动失败!");
                btnFtpServerStartStop.Text = "③启动FTP服务器";
                MessageBox.Show("请检查FTP服务器IP地址是否正确!，FTP服务器已关闭！");
            }
        }
        #endregion
        #region 处理客户端户请求
        // 处理客户端户请求
        private void UserProcessing(object obj)
        {
            User user = (User)obj;
            int s = int.Parse(toolStripStatusLabelreq.Text);
            s = s + 1;
            toolStripStatusLabelreq.Text = s.ToString();
            string sendString = "220 FTP Server v1.0";
            RepleyCommandToUser(user, sendString);
            while (true)
            {
                string receiveString = null;
                try
                {
                    // 读取客户端发来的请求信息
                    receiveString = user.CommandSession.streamReader.ReadLine();
                }
                catch (Exception ex)
                {
                    if (user.CommandSession.tcpClient.Connected == false)
                    {
                        AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + string.Format(" 客户端({0}断开连接！)", user.CommandSession.tcpClient.Client.RemoteEndPoint));
                    }
                    else
                    {
                        AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 接收命令失败！" + ex.Message);
                    }
                    break;
                }
                if (receiveString == null)
                {
                    AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 接收字符串为null,结束线程！");
                    break;
                }
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + string.Format(" 来自{0}：[{1}]", user.CommandSession.tcpClient.Client.RemoteEndPoint, receiveString));
                // 分解客户端发来的控制信息中的命令和参数
                string command = receiveString;
                string param = string.Empty;
                int index = receiveString.IndexOf(' ');
                if (index != -1)
                {
                    command = receiveString.Substring(0, index).ToUpper();
                    param = receiveString.Substring(command.Length).Trim();
                }
                // 处理不需登录即可响应的命令（这里只处理QUIT）
                if (command == "quit" || command == "QUIT")
                {
                    // 关闭TCP连接并释放与其关联的所有资源
                    user.CommandSession.Close();
                    return;
                }
                else
                {
                    switch (user.LoginOK)
                    {
                        // 等待户输入户名：
                        case 0:
                            CommandUser(user, command, param);
                            break;
                        // 等待户输入密码
                        case 1:
                            CommandPassword(user, command, param);
                            break;
                        // 户名和密码验证正确后登陆
                        case 2:
                            switch (command)
                            {
                                case "CWD":
                                    CommandCWD(user, param);
                                    break;
                                case "PWD":
                                    CommandPWD(user);
                                    break;
                                case "PASV":
                                    CommandPASV(user);
                                    break;
                                case "PORT":
                                    CommandPORT(user, param);
                                    break;
                                case "LIST":
                                    CommandLIST(user, param);
                                    break;
                                case "NLIST":
                                    CommandLIST(user, param);
                                    break;
                                // 处理下载文件命令
                                case "RETR":
                                    CommandRETR(user, param);
                                    break;
                                // 处理上传文件命令
                                case "STOR":
                                    CommandSTOR(user, param);
                                    break;
                                // 处理删除命令
                                case "DELE":
                                    CommandDELE(user, param);
                                    break;
                                // 使Type命令在ASCII和二进制模式进行变换
                                case "TYPE":
                                    CommandTYPE(user, param);
                                    break;
                                // 退出
                                case "quit":
                                    user.CommandSession.Close();
                                    sendString = "FTP quit OK.";
                                    return;
                                default:
                                    sendString = "502 command is not implemented.";
                                    RepleyCommandToUser(user, sendString);
                                    break;
                            }
                            break;
                    }
                }
            }
        }
        #endregion
        #region 向客户端返回响应码
        // 想客户端返回响应码
        private void RepleyCommandToUser(User user, string str)
        {
            try
            {
                user.CommandSession.streamWriter.WriteLine(str);
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+string.Format("向客户端（{0}）发送[{1}]", user.commandSession.tcpClient.Client.RemoteEndPoint, str));
            }
            catch
            {
                // AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+string.Format("向客户端（{0}）发送信息失败", user.commandSession.tcpClient.Client.RemoteEndPoint));
            }
        }
        // 向屏幕输出显示状态信息（这里使了委托机制）
        private delegate void AddInfoDelegate(string str);
        #endregion
        #region list addinfo方法
        private void AddInfo(string str)
        {
            // 如果调AddInfo()方法的线程与创建ListView控件的线程不在一个线程时
            // 此时利委托在创建ListView的线程上调
            if (lstboxStatus.InvokeRequired == true)
            {
                AddInfoDelegate d = new AddInfoDelegate(AddInfo);
                this.Invoke(d, str);
            }
            else
            {
                lstboxStatus.Items.Add(str);
                lstboxStatus.TopIndex = lstboxStatus.Items.Count - 1;
                lstboxStatus.ClearSelected();
            }
        }
        #endregion
        #region 登录过程，即户身份验证过程
        // 处理USER命令，接收户名但不进行验证
        private void CommandUser(User user, string command, string param)
        {
            string sendString = string.Empty;
            if (command == "USER")
            {
                sendString = "331 USER command OK, password required.";
                user.UserName = param;
                // 设置loginOk=1为了确保后面紧接的要求输入密码
                // 1表示已接收到户名，等到接收密码
                user.LoginOK = 1;
            }
            else
            {
                sendString = "501 USER command syntax error.";
            }
            RepleyCommandToUser(user, sendString);
        }
        // 处理PASS命令，验证户名和密码
        private void CommandPassword(User user, string command, string param)
        {
            string sendString = string.Empty;
            if (command == "PASS")
            {
                string password = null;
                if (users.TryGetValue(user.UserName, out password))
                {
                    if (password == param)
                    {
                        sendString = "230 User logged in success";
                        // 2表示登录成功
                        user.LoginOK = 2;
                    }
                    else
                    {
                        sendString = "530 Password incorrect.";
                    }
                }
                else
                {
                    sendString = "530 User name or password incorrect.";
                }
            }
            else
            {
                sendString = "501 PASS command Syntax error.";
            }
            RepleyCommandToUser(user, sendString);
            // 户当前工作目录
            user.CurrentDir = user.WorkDir;
        }
        #endregion
        #region 文件管理命令
        private void CommandQUIT(User user, string temp)
        {
            user.CommandSession.Close();
            return;
        }
        // 处理CWD命令，改变工作目录
        private void CommandCWD(User user, string temp)
        {
            string sendString = string.Empty;
            try
            {
                string dir = user.WorkDir.TrimEnd('/') + temp;
                // 是否为当前目录的子目录，且不包含父目录名称
                if (Directory.Exists(dir))
                {
                    user.CurrentDir = dir;
                    sendString = "250 Directory changed to '" + dir + "' successfully";
                }
                else
                {
                    sendString = "550 Directory '" + dir + "' does not exist";
                }
            }
            catch
            {
                sendString = "502 Directory changed unsuccessfully";
            }
            RepleyCommandToUser(user, sendString);
        }
        // 处理PWD命令，显示工作目录
        private void CommandPWD(User user)
        {
            string sendString = string.Empty;
            sendString = "257 '" + user.CurrentDir + "' is the current directory";
            RepleyCommandToUser(user, sendString);
        }
        // 处理LIST/NLIST命令，想客户端发送当前或指定目录下的所有文件名和子目录名
        private void CommandLIST(User user, string parameter)
        {
            string sendString = string.Empty;
            DateTimeFormatInfo dateTimeFormat = new CultureInfo("en-US", true).DateTimeFormat;
            // 得到目录列表
            string[] dir = Directory.GetDirectories(user.CurrentDir);
            if (string.IsNullOrEmpty(parameter) == false)
            {
                if (Directory.Exists(user.CurrentDir + parameter))
                {
                    dir = Directory.GetDirectories(user.CurrentDir + parameter);
                }
                else
                {
                    string s = user.CurrentDir.TrimEnd('/');
                    user.CurrentDir = s.Substring(0, s.LastIndexOf("/") + 1);
                }
            }
            for (int i = 0; i < dir.Length; i++)
            {
                string folderName = Path.GetFileName(dir[i]);
                DirectoryInfo d = new DirectoryInfo(dir[i]);
                // 按下面的格式输出目录列表
                sendString += @"dwr-\t" + Dns.GetHostName() + "\t" + dateTimeFormat.GetAbbreviatedMonthName(d.CreationTime.Month)
                    + d.CreationTime.ToString(" dd yyyy") + "\t" + folderName + Environment.NewLine;
            }
            // 得到文件列表
            string[] files = Directory.GetFiles(user.CurrentDir);
            if (string.IsNullOrEmpty(parameter) == false)
            {
                if (Directory.Exists(user.CurrentDir + parameter + "/"))
                {
                    files = Directory.GetFiles(user.CurrentDir + parameter + "/");
                }
            }
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo f = new FileInfo(files[i]);
                string fileName = Path.GetFileName(files[i]);
                // 按下面的格式输出文件列表
                sendString += "-wr-\t" + Dns.GetHostName() + "\t" + f.Length + " "
                    + dateTimeFormat.GetAbbreviatedMonthName(f.CreationTime.Month)
                    + f.CreationTime.ToString(" dd yyyy") + "\t" + fileName + Environment.NewLine;
            }
            // List命令指示获得FTP服务器上的文件列表字符串信息
            // 所以调List命令过程，客户端接受的指示一些字符串
            // 所以isBinary是false,代表传输的是ASCII数据
            // 但是为了防止isBinary因为 设置user.isBinary = false而改变
            // 所以事先保存user.IsBinary的引（此时为true）,方便后面下载文件
            bool isBinary = user.IsBinary;
            user.IsBinary = false;
            RepleyCommandToUser(user, "150 Opening ASCII data connection");
            InitDataSession(user);
            SendByUserSession(user, sendString);
            RepleyCommandToUser(user, "226 Transfer complete");
            user.IsBinary = isBinary;
        }
        // 处理RETR命令，提供下载功能，将户请求的文件发送给户
        private void CommandRETR(User user, string filename)
        {
            string sendString = "";
            // 下载的文件全名
            string path = user.CurrentDir + filename;
            try
            {

                FileStream filestream = new FileStream(path, FileMode.Open, FileAccess.Read);


                // 发送150到户，表示服务器文件状态良好，将要打开数据连接传输文件
                if (user.IsBinary)
                {
                    sendString = "150 Opening BINARY mode data connection for download";
                }
                else
                {
                    sendString = "150 Opening ASCII mode data connection for download";
                }
                RepleyCommandToUser(user, sendString);
                InitDataSession(user);
                SendFileByUserSession(user, filestream, path);
                RepleyCommandToUser(user, "226 Transfer complete");

            }
            catch (Exception ex)
            {
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 来自"+ user.CommandSession.tcpClient.Client.RemoteEndPoint + "：[550 Directory " + path + " does not exist]");
            }

        }
        // 处理STOR命令，提供上传功能，接收客户端上传的文件
        private void CommandSTOR(User user, string filename)
        {
            string sendString = "";
            // 上传的文件全名
            string path = user.CurrentDir + filename;
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            //MessageBox.Show(path);
            // 发送150到户，表示服务器状态良好
            if (user.IsBinary)
            {
                sendString = "150 Opening BINARY mode data connection for upload";
            }
            else
            {
                sendString = "150 Opeing ASCII mode data connection for upload";
            }
            RepleyCommandToUser(user, sendString);
            InitDataSession(user);
            ReadFileByUserSession(user, fs, path);
            RepleyCommandToUser(user, "226 Transfer complete");
        }
        // 处理DELE命令，提供删除功能，删除服务器上的文件
        private void CommandDELE(User user, string filename)
        {
            string sendString = "";
            // 删除的文件全名
            string path = user.CurrentDir + filename;
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 正在删除文件" + filename + "...");
            File.Delete(path);
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 删除成功");
            sendString = "250 File " + filename + " has been deleted.";
            RepleyCommandToUser(user, sendString);
        }
        #endregion
        #region 模式设置命令
        // 处理PASV命令， 使被动模式进行传输
        private void CommandPASV(User user)
        {
            string sendString = string.Empty;
            IPAddress localip = Dns.GetHostEntry("").AddressList[1];
            // 被动模式，即服务器接收客户端的连接请求
            // 被动模式下FTP服务器使随机生成的端口进行传输数据
            // 而主动模式下FTP服务器使端口20进行数据传输
            Random random = new Random();
            int random1, random2;
            int port;
            while (true)
            {
                // 随机生成一个端口进行数据传输
                random1 = random.Next(5, 200);
                random2 = random.Next(0, 200);
                // 生成的端口号控制>1024的随机端口
                // 下面这个运算算法只是为了得到一个大于1024的端口值
                port = random1 << 8 | random2;
                try
                {
                    user.DataListener = new TcpListener(localip, port);
                    AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " TCP 数据连接已打开（被动模式）--" + localip.ToString() + "：" + port);
                }
                catch
                {
                    continue;
                }
                user.IsPassive = true;
                string temp = localip.ToString().Replace('.', ',');
                // 必须把端口号IP地址告诉客户端，客户端接收到响应命令后，
                // 再通过新的端口连接服务器的端口P，然后进行文件数据传输
                sendString = "227 Entering Passive Mode(" + temp + "," + random1 + "," + random2 + ")";
                RepleyCommandToUser(user, sendString);
                user.DataListener.Start();
                break;
            }
        }
        // 处理PORT命令，使主动模式进行传输
        private void CommandPORT(User user, string portstring)
        {
            // 主动模式时，客户端必须告知服务器接收数据的端口号，PORT 命令格式为：PORT address
            // address参数的格式为i1、i2、i3、i4、p1、p2,其中i1、i2、i3、i4表示IP地址
            // 下面通过.字符串来组合这四个参数得到IP地址
            // p1、p2表示端口号，下面通过int.Parse(temp[4]) << 8) | int.Parse(temp[5]
            // 这个算法来获得一个大于1024的端口来发送给服务器
            string sendString = string.Empty;
            string[] temp = portstring.Split(',');
            string ipString = "" + temp[0] + "." + temp[1] + "." + temp[2] + "." + temp[3];
            // 客户端发出PORT命令把客户端的IP地址和随机的端口告诉服务器
            int portNum = (int.Parse(temp[4]) << 8) | int.Parse(temp[5]);
            user.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipString), portNum);
            sendString = "200 PORT command successful.";
            // 服务器以接受到的客户端IP地址和端口为目标发起主动连接请求
            // 服务器根据客户端发送过来的IP地址和端口主动发起与客户端建立连接
            RepleyCommandToUser(user, sendString);
        }
        // 处理TYPE命令,设置数据传输方式
        private void CommandTYPE(User user, string param)
        {
            string sendstring = "";
            if (param == "I")
            {
                // 二进制
                user.IsBinary = true;
                sendstring = "220 Type set to I(Binary)";
            }
            else
            {
                // ASCII方式
                user.IsBinary = false;
                sendstring = "330 Type set to A(ASCII)";
            }
            RepleyCommandToUser(user, sendstring);
        }
        #endregion
        #region 初始化数据连接
        // 初始化数据连接
        private void InitDataSession(User user)
        {
            TcpClient client = null;
            if (user.IsPassive)
            {
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"采被动模式返回LIST目录和文件列表");
                client = user.DataListener.AcceptTcpClient();
            }
            else
            {
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"采主动模式向户发送LIST目录和文件列表");
                client = new TcpClient();
                client.Connect(user.RemoteEndPoint);
            }
            user.DataSession = new UserSeesion(client);
        }
        #endregion
        #region 使数据连接发送字符串
        // 使数据连接发送字符串
        private void SendByUserSession(User user, string sendString)
        {
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 向户发送(字符串信息)：[" + sendString + "]");
            try
            {
                user.DataSession.streamWriter.WriteLine(sendString);
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 发送完毕");
            }
            finally
            {
                user.DataSession.Close();
            }
        }
        #endregion
        #region 使数据连接发送文件流
        // 使数据连接发送文件流（客户端发送下载文件命令）
        private void SendFileByUserSession(User user, FileStream fs, string path)
        {
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 向用户发送(文件流)：[........................");
            string ipadd = comip.Text;
            try
            {
                if (user.IsBinary)
                {
                    long totalDownloadedByte = 0;
                    int percent = 0;
                    Filesize = new FileInfo(path).Length;
                    byte[] bytes = new byte[1024];
                    BinaryReader binaryReader = new BinaryReader(fs);
                    int count = binaryReader.Read(bytes, 0, bytes.Length);
                    while (count > 0)
                    {
                        user.DataSession.binaryWriter.Write(bytes, 0, count);
                        user.DataSession.binaryWriter.Flush();
                        count = binaryReader.Read(bytes, 0, bytes.Length);
                        totalDownloadedByte = count + totalDownloadedByte;
                        if (user.CommandSession.tcpClient.Client.RemoteEndPoint.ToString().Contains(ipadd))
                        {

                            percent = (int)Math.Floor((float)totalDownloadedByte / (float)Filesize * 100);
                            //labjindu.Text = percent.ToString() + "%";
                            if (percent >= 0 && percent <= 100)
                            {
                                myProgressBarjindu.Value = percent;
                            }
                        }

                    }
                }
                else
                {
                    StreamReader streamReader = new StreamReader(fs);
                    while (streamReader.Peek() > -1)
                    {
                        user.DataSession.streamWriter.WriteLine(streamReader.ReadLine());
                    }
                }
                if (user.CommandSession.tcpClient.Client.RemoteEndPoint.ToString().Contains(ipadd))
                {
                    myProgressBarjindu.Value = 100;
                    //labjindu.Text = 100 + "%";
                }

                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "                                              ...................]发送完毕！");
            }
            finally
            {
                user.DataSession.Close();
                fs.Close();
            }
        }
        #endregion
        #region 使数据连接接收文件流
        // 使数据连接接收文件流(客户端发送上传文件功能)
        private void ReadFileByUserSession(User user, FileStream fs, string path)
        {

            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 接收用户上传数据（文件流）：[..................");
            string ipadd = comip.Text;
            try
            {
                if (user.IsBinary)
                {

                    byte[] bytes = new byte[1024];
                    long totalDownloadedByte = 0;
                    int percent = 0;
                    BinaryWriter binaryWriter = new BinaryWriter(fs);
                    int count = user.DataSession.binaryReader.Read(bytes, 0, bytes.Length);
                    while (count > 0)
                    {

                        binaryWriter.Write(bytes, 0, count);
                        totalDownloadedByte = count + totalDownloadedByte;
                        count = user.DataSession.binaryReader.Read(bytes, 0, bytes.Length);
                        binaryWriter.Flush();
                        if (user.CommandSession.tcpClient.Client.RemoteEndPoint.ToString().Contains(ipadd))
                        {

                            //textDOS.AppendText("t2:"+totalBytes.ToString() + "\r\n");
                            percent = (int)Math.Floor((float)totalDownloadedByte / (float)Filesize * 100);
                            // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + header + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+percent.ToString() + "\r\np");
                            //textDOS.AppendText("d:"+totalDownloadedByte.ToString() + "\r\n");
                            //labjindu.Text = percent.ToString() + "%";
                            if (percent >= 0 && percent <= 100)
                            {
                                myProgressBarjindu.Value = percent;
                            }
                        }
                    }
                }
                else
                {
                    StreamWriter streamWriter = new StreamWriter(fs);
                    while (user.DataSession.streamReader.Peek() > -1)
                    {
                        streamWriter.Write(user.DataSession.streamReader.ReadLine());
                        streamWriter.Flush();
                    }
                }
                if (user.CommandSession.tcpClient.Client.RemoteEndPoint.ToString().Contains(ipadd))
                {
                    myProgressBarjindu.Value = 100;
                    //labjindu.Text = 100 + "%";
                }
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "                                              ...................]接收完毕！");
            }
            finally
            {
                user.DataSession.Close();
                fs.Close();
            }
        }
        #endregion
        #region 一键升级开始按钮
        bool DownLoadFile_Stop = true;
        bool DownLoadFile_On_Off = false;
        ManualResetEvent DownLoadFilePause;
        Thread DownLoadFileThread;
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Butupgrade_Click(object sender, EventArgs e)
        {
            if (butupgrade.Text == "下载升级")
            {

                if (FtpPortEnable == false || FtpStatusEnable == false)
                {
                    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                    return;
                }


                if (checkapp.Checked == false &&
                    checkcode.Checked == false &&
                    checknms.Checked == false &&
                    checksw.Checked == false &&
                    check760s.Checked == false &&
                    check760b.Checked == false &&
                    check760c.Checked == false &&
                    check760d.Checked == false &&
                    check760f.Checked == false &&
                    checksysfile.Checked == false &&
                    checkflash.Checked == false &&
                    checkyaffs.Checked == false &&
                    checkconfig.Checked == false &&
                    checkdb.Checked == false &&
                    checkslotconfig.Checked == false &&
                    check760e.Checked == false)
                {
                    MessageBox.Show("请勾选文件后继续！");
                    return;
                }
                DialogResult dr = MessageBox.Show("升级前是否进行备份设备数据？", "提示", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes)
                {
                    DownLoadFile_Stop = false;
                    DownLoadFileThread = new Thread(DownLoadFile)
                    {
                        IsBackground = true
                    };
                    DownLoadFileThread.Start();
                    backupfile = true;
                    butupgrade.Text = "停止升级";
                    //户选择确认的操作
                }
                else if (dr == DialogResult.No)
                {

                    //户选择取消的操作
                    DownLoadFile_Stop = false;
                    DownLoadFileThread = new Thread(DownLoadFile)
                    {
                        IsBackground = true
                    };
                    DownLoadFileThread.Start();
                    backupfile = false;
                    butupgrade.Text = "停止升级";
                }
                if (dr == DialogResult.Cancel)
                {
                    return;
                    //户选择确认的操作
                }
            }
            else
            {
                DownLoadFile_Stop = true;
                backupfile = false;
                butupgrade.Text = "下载升级";
            }
        }
        #endregion
        #region 定时建立telnet连接
        private void timer2_Tick(object sender, EventArgs e)
        {
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + mysocket.ReceiveData(int.Parse(ts)));
        }
        #endregion

        #region 正式升级
        //这里，就是后台进程开始工作时，调工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void DownLoadFile()
        {
            //立即开始计时，时间间隔1000毫秒
            try
            {
                TimeCount = 0;
                Mytimer.Change(0, 1000);
                Control.CheckForIllegalCrossThreadCalls = false;
                Save();
                Thread.Sleep(XHTime);
                Testftpser();
                if (DownLoadFile_Stop)
                {
                    textDOS.AppendText(DateTime.Now.ToString("\r\n" + "yyyy-MM-dd HH:mm:ss") + " " + "下载升级已停止！");
                    return;
                }
                if (backupfile)
                {
                    Backup();
                }
                Thread.Sleep(XHTime);
                int a = 0;
                int p = 0;
                if (checkapp.Checked == true)
                {
                    a++;
                }
                if (checkcode.Checked == true)
                {
                    a++;
                }
                if (checknms.Checked == true)
                {
                    a++;
                }
                if (checksw.Checked == true)
                {
                    a++;
                }
                if (check760s.Checked == true)
                {
                    a++;
                }
                if (check760b.Checked == true)
                {
                    a++;
                }
                if (check760c.Checked == true)
                {
                    a++;
                }
                if (check760d.Checked == true)
                {
                    a++;
                }
                if (check760e.Checked == true)
                {
                    a++;
                }
                if (check760f.Checked == true)
                {
                    a++;
                }
                if (checksysfile.Checked == true)
                {
                    a++;
                }
                if (checkflash.Checked == true)
                {
                    a++;
                }
                if (checkyaffs.Checked == true)
                {
                    a++;
                }
                if (checkconfig.Checked == true)
                {
                    a++;
                }
                if (checkdb.Checked == true)
                {
                    a++;
                }
                if (checkslotconfig.Checked == true)
                {
                    a++;
                }
                int s = (int)Math.Floor((double)100 / a);
                p = (int)Math.Floor((double)100 / a);
                if (checkconfig.Checked == true)
                {
                    Downlaodconfig();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkslotconfig.Checked == true)
                {
                    Downloadslot();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkdb.Checked == true)
                {
                    Downloaddb();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkapp.Checked == true)
                {
                    if (slot17 == "" && slot18 == "")
                    {
                    }
                    else
                    {
                        Rm();
                    }
                    App();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkcode.Checked == true)
                {
                    Fpgacode();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checknms.Checked == true)
                {
                    Nms();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checksw.Checked == true)
                {
                    Swfpga();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760s.Checked == true)
                {
                    Fpga760s();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760b.Checked == true)
                {
                    Fpga760b();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760c.Checked == true)
                {
                    Fpga760c();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760d.Checked == true)
                {
                    Fpga760d();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760e.Checked == true)
                {
                    Fpga760e();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (check760f.Checked == true)
                {
                    Fpga760f();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checksysfile.Checked == true)
                {
                    Sysfile();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkflash.Checked == true)
                {
                    DownloadFlash();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                if (checkyaffs.Checked == true)
                {
                    DownloadYaffs();
                    if (s == p)
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        myProgressBarjindu.Value = p;
                        //toolStripStatusLabelbar.Text = p + "%";
                        Thread.Sleep(XHTime);
                        p = s + p;
                    }
                    else
                    {
                        if (p > 95 && p <= 100)
                        {
                            p = 100;
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                        }
                        else
                        {
                            if (DownLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (DownLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                DownLoadFilePause = new ManualResetEvent(false);
                                DownLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                    }
                }
                Thread.Sleep(XHTime);
                string canyu = mysocket.ReceiveData(int.Parse(ts));
                CheckFile();
                Thread.Sleep(XHTime);
                string canyu2 = mysocket.ReceiveData(int.Parse(ts));
                toolStripStatusLabelzt.Text = "已完成";
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载结束" + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                DownLoadFile_Stop = true;
                butupgrade.Text = "下载升级";
                Mytimer.Change(Timeout.Infinite, 1000);
                Reboot();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //butsend.PerformClick();
        }
        #endregion
        #region 连接断开按钮
        /// <summary>
        /// 验证IP地址是否合法
        /// </summary>
        /// <param name="ip">要验证的IP地址</param>
        /// 
        public static bool IsIP(string ip)
        {
            //如果为空，认为验证合格
            if (string.IsNullOrEmpty(ip))
            {
                return true;
            }
            //清除要验证字符串中的空格
            ip = ip.Trim();
            //模式字符串
            string pattern = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";
            //验证
            return Regex.IsMatch(ip, pattern);
        }

        private void Butlogin_Click(object sender, EventArgs e)
        {
            Thread Linkgpn = new Thread(LinkGpn);
            if (string.Compare(ButLogin.Text, "①断开设备") == 0)
            {
                ButLogin.Text = "①连接设备";
                comip.Enabled = true;
                textcom.Enabled = false;
                butsend.Enabled = false;
                butguzhangsend.Enabled = false;
                butpaigu.Enabled = false;
                butsyslog.Enabled = false;
                textguzhangmingling.Enabled = false;
                butupgrade.Text = "下载升级";
                butupgrade.Enabled = false;
                butslectfile.Enabled = false;
                butupload.Enabled = false;
                butotnpaigu.Enabled = false;
                toolStripStatusLabelnms.Text = "17:无";
                toolStripStatusLabelnms18.Text = "18:无";
                toolStripStatusLabelswa11.Text = "11:无";
                toolStripStatusLabelswa12.Text = "12:无";
                toolStripStatusLabelver.Text = "APP:无";
                toolStripStatusLabelcpu.Text = "CPU:无";
                toolStripStatusLabelfpgaver.Text = "FPGA:无";
                toolStripStatusLabelmem.Text = "内存:无";
                toolStripStatusLabeltem.Text = "温度:无";
                toolStripStatusLabeldevtype.Text = "型号";
                timer1.Stop();

                slot18 = "";               //18槽位状态
                slot11 = "";                //11槽位状态
                sw = "";                    //SW型号状态
                slot17 = "";               //17槽位状态
                slot12 = "";               //12槽位状态
                version = "";              //设备版本号
                devtype = "";               //设备型号
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已断开==================================================OK");
                AcceptButton = ButLogin;
                mysocket.Close();
                toolStripStatusLabellinkstat.Text = "未连接";
                Linkgpn.Abort();
                Gpnsetini();
                //readgpnip();

                return;
            }
            if (string.Compare(ButLogin.Text, "①连接设备") == 0)
            {

                Linkgpn.Start();
                timer1.Start();
                //  LinkGpn();
            }
        }
        #endregion
        private void LinkGpn()
        {
            try
            {
                if (!IsIP(comip.Text.Trim()))
                {
                    MessageBox.Show("您输入了非法IP地址，请修改后再次尝试！");
                    return;
                }

                Ping ping = new Ping();
                int timeout = 120;
                PingReply pingReply = ping.Send(comip.Text, timeout);
                bool link = false;
                //判断请求是否超时
                for (int but = 0; but < int.Parse(compingcount.Text); but++)
                {
                    if (ButLogin.Text == "①连接设备")
                    {
                        pingReply = ping.Send(comip.Text, timeout);
                        if (pingReply.Status == IPStatus.Success)
                        {
                            link = true;
                            break;
                        }
                    }
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备无法ping通剩余：" + (int.Parse(compingcount.Text) - but).ToString() + "次，请检查IP地址：" + comip.Text + "  设备是否正常！");
                    Thread.Sleep(XHTime);
                }
                if (link == false)
                {
                    return;
                }
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备可以ping通，正在尝试Telnet登录，请稍等...");
                if (mysocket.Connect(comip.Text.Trim(), "23"))
                {
                    ButLogin.Text = "①断开设备";
                    comip.Enabled = false;
                    textcom.Enabled = true;
                    butsend.Enabled = true;
                    butpaigu.Enabled = true;
                    butsyslog.Enabled = true;
                    butguzhangsend.Enabled = true;
                    textguzhangmingling.Enabled = true;
                    butupgrade.Enabled = true;
                    butslectfile.Enabled = true;
                    butupload.Enabled = true;
                    butotnpaigu.Enabled = true;
                    // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+mysocket.ReceiveData(int.Parse(ts)));
                    this.AcceptButton = butsend;
                    textcom.Focus();
                    tabPageGpn.Text = comip.Text;
                    mysocket.SendData(textusr.Text);
                    for (int a = 0; a <= XHCount; a++)
                    {
                        string login = mysocket.ReceiveData(int.Parse(ts));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket.SendData(textpsd.Text);
                            break;
                        }
                        if (login.Contains("Key"))
                        {
                            MessageBox.Show("非我司设备，请更换IP重启登录！");
                            ButLogin.PerformClick();
                            return;
                        }
                        if (login.Contains("Username or password is invalid"))
                        {
                            MessageBox.Show("非我司设备，请更换IP重启登录！");
                            ButLogin.PerformClick();
                            return;
                        }
                        Thread.Sleep(XHTime / 3);
                    }
                    for (int c = 0; c <= XHCount; c++)
                    {
                        string passd = mysocket.ReceiveData(int.Parse(ts));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Bad passwords") || passd.Contains("Key"))
                        {
                            MessageBox.Show("用户名或密码错误，请断开重新尝试！");
                            ButLogin.PerformClick();
                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请断开重新尝试！");
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket.SendData(textpsd.Text);
                        }
                        Thread.Sleep(XHTime / 3);
                        if (passd.Contains(">"))
                        {
                            textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "用户名密码正确==========================================OK");
                            mysocket.SendData("enable");
                            for (int b = 0; b <= XHCount; b++)
                            {
                                string pass = mysocket.ReceiveData(int.Parse(ts));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket.SendData(textpsden.Text);
                                    //Thread.Sleep(XHTime);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket.ReceiveData(int.Parse(ts));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已经有用户登录，正在重新登录============================OK");
                                            mysocket.SendData("grosadvdebug");
                                            Thread.Sleep(XHTime);
                                            mysocket.SendData("vty user limit no");
                                            Thread.Sleep(XHTime);
                                            mysocket.SendData("exit");
                                            Thread.Sleep(XHTime);
                                            mysocket.SendData("enable");
                                            Thread.Sleep(XHTime);
                                            if (mysocket.ReceiveData(int.Parse(ts)).Contains("Pas"))
                                            {
                                                mysocket.SendData(textpsden.Text);
                                                Thread.Sleep(XHTime);
                                                if (!mysocket.ReceiveData(int.Parse(ts)).Contains("failed"))
                                                {
                                                    MessageBox.Show("用户名或密码错误，请断开重新尝试！");
                                                    ButLogin.PerformClick();
                                                    return;
                                                }
                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(XHTime / 3);
                                    }
                                    break;
                                }
                                if (pass.Contains("#"))
                                {
                                    break;
                                }
                                if (pass.Contains("configuration is locked by other user"))
                                //configuration is locked by other user
                                {
                                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已经有用户登录，正在重新登录============================OK");
                                    mysocket.SendData("grosadvdebug");
                                    Thread.Sleep(XHTime);
                                    mysocket.SendData("vty user limit no");
                                    Thread.Sleep(XHTime);
                                    mysocket.SendData("exit");
                                    Thread.Sleep(XHTime);
                                    mysocket.SendData("enable");
                                    Thread.Sleep(XHTime);
                                    if (mysocket.ReceiveData(int.Parse(ts)).Contains("Pas"))
                                    {
                                        mysocket.SendData(textpsden.Text);
                                        Thread.Sleep(XHTime);
                                        if (!mysocket.ReceiveData(int.Parse(ts)).Contains("failed"))
                                        {
                                            MessageBox.Show("用户名或密码错误，请断开重新尝试！");
                                            ButLogin.PerformClick();
                                            return;
                                        }
                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(XHTime / 3);
                            }
                            break;
                        }
                        Thread.Sleep(XHTime / 3);
                    }
                    toolStripStatusLabellinkstat.Text = "已连接";
                    // mysocket.SendData("service snmp source-ip auto");
                    Thread.Sleep(XHTime);
                    string slot = mysocket.ReceiveData(int.Parse(ts));
                    // SNMP团体名称 
                    OctetString community = new OctetString(textReadCommunity.Text);
                    //定义代理参数类 
                    AgentParameters param = new AgentParameters(SnmpVersion.Ver2, community, true);
                    //将SNMP版本设置为1（或2） 
                    //构造代理地址对象
                    //这里很容易使用IpAddress类，因为
                    //如果不
                    //解析为IP地址，它将尝试解析构造函数参数
                    IpAddress agent = new IpAddress(comip.Text);
                    IPAddress send = new IPAddress(agent);
                    //构建目标 
                    UdpTarget target = new UdpTarget(send, 161, 2000, 1);
                    //  用于所有请求PDU级 
                    Pdu pdu = new Pdu(PduType.Get);
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.11");   //11槽位主备状态
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.12");   //12槽位主备状态
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.17");   //17槽位主备状态
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.18");   //18槽位主备状态
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.1.1.1.8.1");      //APP版本
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.1.1.1.7.1");      //FPGA版本
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.1.1.1.2.1");      //设备类型


                    SnmpPacket result = null;
                    try
                    {
                        result = target.Request(pdu, param);
                    }
                    catch (SnmpException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    //SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
                    //如果结果为null，则座席未回复或我们无法解析回复。
                    if (result != null)
                    {
                        //其他的ErrorStatus然后0是通过返回一个错误
                        //代理-见SnmpConstants为错误定义
                        if (result.Pdu.ErrorStatus != 0)
                        {
                            //代理报告与所述请求的错误 
                            textDOS.Text += string.Format("\r\n" + "SNMP回复错误！错误代码：{0}，错误索引：第 {1} 行\r\n",
                                    FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                                    result.Pdu.ErrorIndex);
                        }
                        else
                        {

                            //返回变量的返回顺序与添加
                            //到VbList

                            toolStripStatusLabelver.Text = "APP:" + result.Pdu.VbList[4].Value.ToString();
                            toolStripStatusLabelfpgaver.Text = "FPGA:" + result.Pdu.VbList[5].Value.ToString();
                            devtype = result.Pdu.VbList[6].Value.ToString();
                            string str = "(" + devtype + ")";
                            FindDevType.finddevtype(str);

                            toolStripStatusLabeldevtype.Text = FindDevType.type;



                            //slot17 = "ACTIVE";
                            if (result.Pdu.VbList[2].Value.ToString() == "1")
                            {
                                slot17 = "ACTIVE";
                                toolStripStatusLabelnms.Text = "17:主";
                                toolStripStatusLabelnms.ForeColor = Color.DarkGreen;
                                Pdu pdu1 = new Pdu(PduType.Get);
                                pdu1.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.23.1.17");  //17槽位CPU利用率
                                pdu1.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.26.1.17");  //17槽位内存利用率
                                pdu1.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.20.1.17");  //17槽位温度
                                SnmpPacket result1 = null;
                                try
                                {
                                    result1 = target.Request(pdu1, param);
                                }
                                catch (SnmpException ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                catch
                                {
                                    MessageBox.Show("请检查Oid项配置信息！");
                                }
                                toolStripStatusLabelcpu.Text = "CPU:" + result1.Pdu.VbList[0].Value.ToString() + "%";
                                toolStripStatusLabelmem.Text = "内存:" + result1.Pdu.VbList[1].Value.ToString() + "%";
                                toolStripStatusLabeltem.Text = "温度:" + result1.Pdu.VbList[2].Value.ToString() + "°C";

                            }
                            if (result.Pdu.VbList[2].Value.ToString() == "2")
                            {
                                slot17 = "STANDBY";
                                toolStripStatusLabelnms.Text = "17:备";
                                toolStripStatusLabelnms.ForeColor = Color.Red;
                            }
                            if ((result.Pdu.VbList[2].Value.ToString() != "1") && (result.Pdu.VbList[3].Value.ToString() != "1"))
                            {
                                Pdu pdu2 = new Pdu(PduType.Get);
                                pdu2.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.1");   //1槽位准备状态
                                pdu2.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.23.1.1");  //1槽位CPU利用率
                                pdu2.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.26.1.1");  //1槽位内存利用率
                                pdu2.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.20.1.1");  //1槽位温度
                                SnmpPacket result2 = null;
                                try
                                {
                                    result2 = target.Request(pdu2, param);
                                }
                                catch (SnmpException ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                catch
                                {
                                    MessageBox.Show("请检查Oid项配置信息！");
                                }
                                if (result2.Pdu.VbList[0].Value.ToString() == "1")
                                {
                                    //slot17 = "ACTIVE";
                                    toolStripStatusLabelnms.Text = "1:主";
                                    toolStripStatusLabelnms.ForeColor = Color.DarkGreen;
                                }
                                toolStripStatusLabelcpu.Text = "CPU:" + result2.Pdu.VbList[1].Value.ToString() + "%";
                                toolStripStatusLabelmem.Text = "内存:" + result2.Pdu.VbList[2].Value.ToString() + "%";
                                toolStripStatusLabeltem.Text = "温度:" + result2.Pdu.VbList[3].Value.ToString() + "°C";

                            }
                            if (result.Pdu.VbList[3].Value.ToString() == "1")
                            {
                                slot18 = "ACTIVE";
                                toolStripStatusLabelnms18.Text = "18:主";
                                toolStripStatusLabelnms18.ForeColor = Color.DarkGreen;
                            }
                            if (result.Pdu.VbList[3].Value.ToString() == "2")
                            {
                                slot18 = "STANDBY";
                                toolStripStatusLabelnms18.Text = "18:备";
                                toolStripStatusLabelnms18.ForeColor = Color.Red;
                            }

                            if (result.Pdu.VbList[0].Value.ToString() == "3")
                            {
                                slot11 = "在位";
                                toolStripStatusLabelswa11.Text = "11:主";
                                toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                            }
                            if (result.Pdu.VbList[0].Value.ToString() == "4")
                            {
                                slot11 = "在位";
                                toolStripStatusLabelswa11.Text = "11:备";
                                toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                            }
                            if (result.Pdu.VbList[1].Value.ToString() == "3")
                            {
                                slot12 = "在位";
                                toolStripStatusLabelswa12.Text = "12:主";
                                toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                            }
                            if (result.Pdu.VbList[1].Value.ToString() == "4")
                            {
                                slot12 = "在位";
                                toolStripStatusLabelswa12.Text = "12:备";
                                toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                            }


                            //MessageBox.Show("ssss");
                            //toolStripStatusLabelnms.ForeColor = Color.DarkGreen;

                        }
                    }
                    else
                    {
                        textDOS.AppendText("\r\n" + "没有收到来自SNMP代理的响应！");
                    }
                    target.Close();

                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "登录成功可以使用========================================OK" + "\r\n");
                    butsend.PerformClick();
                }
                else
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "无法Telnet登录，请检查设备是否正常！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #region 发送按钮
        private void butsend_Click(object sender, EventArgs e)
        {
            if (textcom.Text == "请输入命令行查询")
            {
                textcom.Text = "";
            }
            if (mysocket.SendData(textcom.Text))
            {
                if (textcom.Text == "")
                {
                    Thread.Sleep(XHTime / 3);
                    string ctrlc = "Press any key to continue Ctrl+c to stop";
                    string DOS = textDOS.Text;
                    if (DOS.Contains(ctrlc))
                    {
                        textDOS.Text = DOS.Replace(ctrlc, "");
                        //MessageBox.Show("检测到了");
                    }
                    string str = "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                    textDOS.AppendText(str);
                    //this.textBox3.Text = str;
                }
                else
                {
                    com = textcom.Text;
                    Thread.Sleep(XHTime / 3);
                    string ss = mysocket.ReceiveData(int.Parse(ts));
                    //textBox3.Text = newkou2;
                    textDOS.AppendText(ss);
                    //this.textDOS.Text = ss;
                }
            }
            else
            {
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "连接通信故障，请断开后，重新尝试！");
                //this.butlogin.PerformClick();
            }
            textcom.Text = "";
            textDOS.Focus();
            textDOS.ScrollToCaret();
            textcom.Focus();
        }
        #endregion
        #region 保存配置 Save
        private void Save()
        {
            toolStripStatusLabelzt.Text = "正在保存配置";
            mysocket.SendData("save");
            for (int i = 1; i <= 20; i++)
            {
                string save = "successfully";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(save))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存配置===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains("erro"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存配置==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string box1 = mysocket.ReceiveData(int.Parse(ts));
        }
        #endregion
        #region 测试FTP服务器IP是否正常 Testftpser
        private void Testftpser()
        {
            toolStripStatusLabelzt.Text = "检查FTP服务器中";
            textDOS.AppendText(DateTime.Now.ToString("\r\n" + "yyyy-MM-dd HH:mm:ss") + " " + "FTP服务器连接测试中，请耐心等待,大约需要15秒钟.....");
            mysocket.SendData("ping " + comftpip.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ping = mysocket.ReceiveData(int.Parse(ts));
                if (ping.Contains("ms"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备ping服务器=========================================OK" + "\r\n");
                    mysocket.SendDate("\x03");
                    Thread.Sleep(XHTime);
                    string ctrlc = mysocket.ReceiveData(int.Parse(ts));
                    break;
                }
                if (ping.Contains("0 packets received"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备ping服务器=========================================NOK" + "\r\n");
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "FTP服务器故障，请点击停止升级后，检查FTP服务器IP地址！");
                    toolStripStatusLabelzt.Text = "FTP的IP地址故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "下载升级";
                    MessageBox.Show("请检查FTP服务IP地址后，再次尝试！");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp config " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + "FtpTest.bin");
            for (int i = 1; i <= XHCount; i++)
            {
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains("ok"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "FTP服务器测试==========================================OK" + "\r\n");
                    break;
                }
                if (box.Contains("fail"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "FTP服务器IP地址故障，请点击停止升级后，检查FTP服务器IP地址！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "下载升级";
                    MessageBox.Show("请检查FTP服务IP地址后，再次尝试！");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "FTP服务器用户名密码错误，请检查！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "下载升级";
                    MessageBox.Show("请检查FTP用户名和密码后，再次尝试！");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 删除残余文件 Rm
        private void Rm()
        {
            toolStripStatusLabelzt.Text = "清空主槽残余文件";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("rm app_code_backup.bin");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Error. Can't delete"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽app_code_backup.bin========================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm app_code_backup.bin"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("rm record.txt");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Error. Can't delete"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽record.txt=================================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm record.txt"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("rm fpga_code.bin");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Error. Can't delete"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽/flash/sys/fpga_code.bin===================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm fpga_code.bin"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("exit"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string nms = mysocket.ReceiveData(int.Parse(ts));
            if (slot18 == "STANDBY" || slot17 == "STANDBY")
            {
                toolStripStatusLabelzt.Text = "清空备槽残余文件";
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("grosadvdebug");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("grosadvdebug"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                if (slot18 == "STANDBY")
                {
                    mysocket.SendData("switch 18");
                }
                if (slot17 == "STANDBY")
                {
                    mysocket.SendData("switch 17");
                }
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Slot1"))
                    {
                        break;
                    }
                    if (command.Contains("Please try later"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽有其他户登录，已退出终止升级");
                        return;
                    }
                    if (command.Contains("Login"))
                    {
                        mysocket.SendData(textusr.Text);
                        for (int b = 1; b <= 3000; b++)
                        {
                            string pasd = mysocket.ReceiveData(int.Parse(ts));
                            if (pasd.Contains("Password"))
                            {
                                mysocket.SendData(textpsd.Text);
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("grosadvdebug");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("grosadvdebug"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("slave-config enable");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("slave-config enable"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("enable");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("enable"))
                    {
                        mysocket.SendData(textpsd.Text);
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("dosfs");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("dosfs"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("cd /flash/sys");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("cd /flash/sys"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("rm app_code_backup.bin");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Error. Can't delete"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽app_code_backup.bin======================文件不能存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm app_code_backup.bin"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("rm record.txt");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Error. Can't delete"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽record.txt=================================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm record.txt"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("rm fpga_code.bin");
                for (int a = 1; a <= XHCount; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Error. Can't delete"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽/flash/sys/fpga_code.bin===================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm fpga_code.bin"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 300; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 300; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 300; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                Thread.Sleep(XHTime);
                mysocket.SendData("exit");
                for (int a = 1; a <= 300; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    if (command.Contains(">"))
                    {
                        mysocket.SendData("enable");
                        Thread.Sleep(XHTime);
                        string pass = mysocket.ReceiveData(int.Parse(ts));
                        if (pass.Contains("Pas"))
                        {
                            mysocket.SendData(textpsd.Text);
                            Thread.Sleep(XHTime);
                            string locked = mysocket.ReceiveData(int.Parse(ts));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket.SendData("grosadvdebug");
                                Thread.Sleep(XHTime);
                                mysocket.SendData("vty user limit no");
                                Thread.Sleep(XHTime);
                                mysocket.SendData("exit");
                                Thread.Sleep(XHTime);
                                mysocket.SendData("enable");
                                Thread.Sleep(XHTime);
                                mysocket.SendData(textpsd.Text);
                            }
                        }
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
            }
            ///OK是判断==0 备槽不在位 == 1 备槽在位
            if (slot18 == "")
            {
                //for (int a = 1; a <= XHCount; a++)
                //{
                //    string command = mysocket.ReceiveData(int.Parse(ts));
                //    if (command.Contains("Press"))
                //    {
                //        break;
                //    }
                //    Thread.Sleep(XHTime);
                //}
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
            }
        }
        #endregion
        #region 下载 App  ///////////////////////////////////////////下载软件APP开始////////////////////////////////
        private void App()
        {
            ///////////////////////////////////////////下载软件APP开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载APP中";
            mysocket.SendData("download ftp app " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comapp.Text + " gpn");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽APP下载成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    toolStripStatusLabelzt.Text = "写入APP中";
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "APP写入成功============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽APP写入成功==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S12 = "爱好";
                                    string S18 = "号码";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽APP写入成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S12 = "爱好";
                                    string S18 = "号码";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽APP写入===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽APP下载==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 Fpgacode  //////////////////////////////////////下载FPGA_code开始//////////////////////////////
        private void Fpgacode()
        {
            ///////////////////////////////////////////下载FPGA_code开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载FPGA_CODE中";
            if (comapp.Text.Contains("R13"))
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "升级为R13版本执行特殊升级方式===========================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("download ftp file /flash/sys/fpga_code.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comcode.Text);
            }
            else
            {
                mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comcode.Text + " other");
            }
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入FPGA_CODE中";
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_CODE下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (comapp.Text.Contains("R13"))
                        {
                            if (download.Contains("ok"))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }
                        }
                        else
                        {
                            if (download.Contains("ok"))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备槽准备同步FPGA_CODE================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail") || up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步FPGA_CODE==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail") || up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                break;
                            }
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_CODE写入=======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_CODE下载==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 Nms  ///////////////////////////////////////////下载FPGA_NMS开始///////////////////////////////
        private void Nms()
        {
            ///////////////////////////////////////////下载FPGA_NMS开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载FPGA_NMS中";
            mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comnms.Text + " master");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入FPGA_NMS中";
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_NMS下载成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_NMS写入成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            if (slot18 == "STANDBY")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= XHCount; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 18"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步FPGA_NMS===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    }
                                    if (up18slot.Contains("18 fail"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步FPGA_NMS======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }
                                    if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            if (slot17 == "STANDBY")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= XHCount; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 17"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    }
                                    if (up18slot.Contains("17 fail"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步FPGA_NMS==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }
                                    if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步FPGA_NMS===========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_NMS写入========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽FPGA_NMS下载===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 Swfpga /////////////////////////////////////////下载SW-Afpga开始///////////////////////////////
        private void Swfpga()
        {
            ///////////////////////////////////////////下载SW-Afpga开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载SW_FPGA中";
            if (comsw.Text.Contains("SW-B") || comsw.Text.Contains("SW_B") || comsw.Text.Contains("sw_b") || comsw.Text.Contains("sw-b"))
            {
                if (!version.Contains("R13"))
                {
                    mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comsw.Text + " sw-b");
                }
                if (version.Contains("R13"))
                {
                    mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comsw.Text + " sw");
                }
            }
            if (comsw.Text.Contains("SW-A") || comsw.Text.Contains("SW_A") || comsw.Text.Contains("sw_a") || comsw.Text.Contains("sw-a"))
            {
                mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comsw.Text + " sw");
            }
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入SW_FPGA中";
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主槽SW_FPGA下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA===============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    string S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                                break;
                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "18槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    string S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主用槽SW_FPGA写入=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "主用槽SW_FPGA下载========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string jieshu = mysocket.ReceiveData(int.Parse(ts));
        }
        #endregion
        #region 下载 Fpga760s
        private void Fpga760s()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760S";
            mysocket.SendData("download ftp file /yaffs/sys/760s.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760s.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760s===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760S";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {

                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760s===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }

                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760s===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }

                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Fpga760b
        private void Fpga760b()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760B";
            mysocket.SendData("download ftp file /yaffs/sys/760b.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760b.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760b===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760B";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760b===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760b===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Fpga760c
        private void Fpga760c()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760C";
            mysocket.SendData("download ftp file /yaffs/sys/760c.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760c.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760c===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760C";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760c===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760c===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Fpga760d
        private void Fpga760d()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760D";
            mysocket.SendData("download ftp file /yaffs/sys/760d.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760d.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760d===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760D";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760d===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760d===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Fpga760e
        private void Fpga760e()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760E";
            mysocket.SendData("download ftp file /yaffs/sys/760e.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760e.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760e===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760E";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760e===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760e===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 OTN-Pack
        private void Fpga760f()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760F";
            mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760f.Text + " otn");
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string Error = "Error";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(Error))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760F===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "由于此版本不支持大于30Mb文件写入，请下载R19C07B035版本后的APP重启再次进行尝试下载，我们将在此版本支持。" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                    //mysocket.SendData("download ftp file /yaffs/sys/760f.fpga  " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comotnpack.Text);
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760F============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    Thread.Sleep(8000);
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760F";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步760F============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                Thread.Sleep(8000);
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载760F============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Sysfile
        private void Sysfile()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载sysfile";
            mysocket.SendData("download ftp sysfile " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comsysfile.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载sysfile============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "" || slot11 != "" || slot12 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步sysfile文件";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "同步sysfile============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                Thread.Sleep(5000);
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载sysfile=========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 Flash
        private void DownloadFlash()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载Flash";
            mysocket.SendData("download ftp flash " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comflash.Text);
            for (int i = 1; i <= 2 * XHCount; i++)
            {
                string ok = "100%";
                string fail = "fail";
                string downloadok = "Download file ...ok";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(downloadok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载Flash==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "写入Flash进度===========================================O%");
                }
                if (box.Contains("%"))
                {
                    toolStripStatusLabelzt.Text = "正在写入Flash";
                    string appRegex = ".*%";
                    Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
                    string jindu = r.Match(box).Groups[0].Value;
                    string strjinfu = jindu.Substring(0, jindu.Length - 1);
                    String str; //字符复制串变量
                    str = textDOS.Text; //获取文本百框中的文本赋与字符串变量
                                        //提取度去除最后一问个字符的子字答符串(参数：0(从零Index处开始)，str.Lenght-1(提取几个字符))
                    str = str.Substring(0, str.Length - 3);
                    textDOS.Text = str; //赋回已删除最后一个字符的字符串给textBox
                    textDOS.AppendText(box);
                    myProgressBarjindu.Value = int.Parse(strjinfu);
                    //labjindu.Text = jindu;
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "写入Flash==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载Flash=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 Yaffs
        private void DownloadYaffs()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载Yaffs";
            mysocket.SendData("download ftp yaffs " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comyaffs.Text);
            for (int i = 1; i <= 6 * XHCount; i++)
            {
                string ok = "100%";
                string fail = "fail";
                string downloadok = "Download file ...ok";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(downloadok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "写入Yaffs进度===========================================O%");
                }
                if (box.Contains("%"))
                {
                    toolStripStatusLabelzt.Text = "正在写入Yaffs";
                    string appRegex = ".*%";
                    Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
                    string jindu = r.Match(box).Groups[0].Value;
                    string strjinfu = jindu.Substring(0, jindu.Length - 1);
                    //String str; //字符复制串变量
                    //str = textDOS.Text; //获取文本百框中的文本赋与字符串变量
                    //                    //提取度去除最后一问个字符的子字答符串(参数：0(从零Index处开始)，str.Lenght-1(提取几个字符))
                    //str = str.Substring(0, str.Length - 4);
                    //textDOS.Text = str; //赋回已删除最后一个字符的字符串给textBox
                    //textDOS.AppendText(box);
                    myProgressBarjindu.Value = int.Parse(strjinfu);
                    //labjindu.Text = jindu;
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载Yaffs=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 config
        private void Downlaodconfig()
        {
            toolStripStatusLabelzt.Text = "正在下载config文件";
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("download ftp file /flash/sys/conf_data.txt " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comconfig.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载config========================请检查FTP户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 slotconfig
        private void Downloadslot()
        {
            toolStripStatusLabelzt.Text = "正在下载slotconfig文件";
            mysocket.SendData("download ftp file /flash/sys/slotconfig.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comslotconfig.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载lsotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 下载 db
        private void Downloaddb()
        {
            toolStripStatusLabelzt.Text = "正在下载db文件";
            mysocket.SendData("download ftp file /flash/sys/db.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comdb.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string db = "db.bin";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    for (int a = 1; a <= XHCount; a++)
                    {
                        string box2 = mysocket.ReceiveData(int.Parse(ts));
                        if (box2.Contains(db))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "下载db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 备份文件 Backup
        private void Backup()
        {
            toolStripStatusLabelzt.Text = "正在备份config文件";
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp file /flash/sys/conf_data.txt " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_config.txt");
            for (int i = 1; i <= 10000; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份config=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            //Thread.Sleep(XHTime);
            toolStripStatusLabelzt.Text = "正在备份slotconfig文件";
            mysocket.SendData("upload ftp file /flash/sys/slotconfig.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_slotconfig.bin");
            for (int i = 1; i <= 10000; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份slotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            //Thread.Sleep(XHTime);
            toolStripStatusLabelzt.Text = "正在备份db文件";
            mysocket.SendData("upload ftp file /flash/sys/db.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_db.bin");
            for (int i = 1; i <= 10000; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string db = "db.bin";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    for (int a = 1; a <= XHCount; a++)
                    {
                        string box2 = mysocket.ReceiveData(int.Parse(ts));
                        if (box2.Contains(db))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
        }
        #endregion
        #region 升级后重启 Reboot
        private void Reboot()
        {
            mysocket.SendData("reboot");
            for (int a = 1; a <= XHCount; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("reboot"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            DialogResult dr = MessageBox.Show("已完成，是否重启GPN设备？", "提示", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                mysocket.SendData("Y");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "您选择重启设备=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                Thread.Sleep(XHTime);
                string command = mysocket.ReceiveData(int.Parse(ts));
                ButLogin.PerformClick();
                //户选择确认的操作
            }
            if (dr == DialogResult.No)
            {
                //户选择取消的操作
                mysocket.SendData("N");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "您没有选择重启设备=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                Thread.Sleep(XHTime);
                string command = mysocket.ReceiveData(int.Parse(ts));
            }
        }
        #endregion
        #region ctrl+c 按钮
        private void butctrlc_Click(object sender, EventArgs e)
        {
            mysocket.SendDate("\x03");
            // mysocket.SendDate("ETX");
            // Thread.Sleep(XHTime);
            //textDOS.AppendText("\r\n"+"CTRL+C已发送");
            // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+mysocket.ReceiveData(int.Parse(ts)));
            this.butsend.PerformClick();
        }
        #endregion
        #region ctrl+q按钮
        private void butctrlq_Click(object sender, EventArgs e)
        {
            mysocket.SendDate("\x011");
            // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+"CTRL+Q已发送");
            this.butsend.PerformClick();
        }
        #endregion
        #region 上下键显示上次记忆字符串
        private void textcom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                //textcom.Text = "";
                mysocket.SendDate("\x1b\x5b\x41");
                Thread.Sleep(XHTime);
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + mysocket.ReceiveData(int.Parse(ts)) + "\n");
                //text = 
                //this.butsend.Focus();
                //textcom.Text = com.ToString();
            }
            if (e.KeyCode == Keys.Down)
            {
                //textcom.Text = "";
                mysocket.SendDate("\x1b\x5b\x42");
                Thread.Sleep(XHTime);
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + mysocket.ReceiveData(int.Parse(ts)) + "\n");
                //this.butsend.Focus();
                //this.butguzhangsend.Focus();
                //textcom.Text = "";
            }
        }
        #endregion
        #region 连接后不停的发\x0
        private void timer2_Tick_1(object sender, EventArgs e)
        {
            if (toolStripStatusLabellinkstat.Text == "已连接")
            {
                mysocket.SendDate("\x0");
            }
        }
        #endregion
        #region 打开目录
        private void butapp_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog path = new FolderBrowserDialog();
            //file.Multiselect = true;
            //MessageBox.Show("CTRL+A全选中会自动填写");
            path.SelectedPath = @"C:\";
            comconfig.Items.Clear();
            comslotconfig.Items.Clear();
            comdb.Items.Clear();
            comapp.Items.Clear();
            comnms.Items.Clear();
            comcode.Items.Clear();
            comsw.Items.Clear();
            com760s.Items.Clear();
            com760b.Items.Clear();
            com760c.Items.Clear();
            com760d.Items.Clear();
            com760e.Items.Clear();
            com760f.Items.Clear();
            comsysfile.Items.Clear();
            comflash.Items.Clear();
            comyaffs.Items.Clear();//清除之前打开的历史  
                                   //获取文件路径，不带文件名
                                   // string filePath = file.FileName;
                                   // string FilePath = Path.GetDirectoryName(filePath);
            if (defaultfilePath != "")
            {
                //设置此次默认目录为上一次选中目录  
                path.SelectedPath = defaultfilePath;
            }
            if (path.ShowDialog() == DialogResult.OK)
            {
                defaultfilePath = path.SelectedPath;
                if (path.SelectedPath == @"C:\" || path.SelectedPath == @"D:\" || path.SelectedPath == @"E:\" || path.SelectedPath == @"F:\")
                {
                    tbxFtpRoot.Text = path.SelectedPath;
                }
                else
                {
                    tbxFtpRoot.Text = path.SelectedPath + @"\";
                }

            }
            Readfile(path.SelectedPath);
            //comapp.SelectedIndex = 0;
            //this.comapp.Text = file.SafeFileName;
        }
        private void Readfile(string path)
        {



            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir == null)
            {
                MessageBox.Show("文件空，请重新选择文件夹！");
                return;
            }
            FileInfo[] fileInfo = dir.GetFiles();
            List<string> fileNames = new List<string>();
            foreach (FileInfo item in fileInfo)
            {
                fileNames.Add(item.Name);
            }
            foreach (string s in fileNames)
            {
                if (s.Contains(".bin") && !s.Contains("code") && !s.Contains("sysfile") && !s.Contains("db") && !s.Contains("slot") && !s.Contains("config") && !s.Contains(".fpga"))
                {
                    comapp.Items.Add(s);
                    if (comapp.Items.Count > 0)
                    {
                        comapp.SelectedIndex = comapp.Items.Count - 1;
                    }
                }
                if (s.Contains("code") || s.Contains("CODE"))
                {
                    comcode.Items.Add(s);
                    if (comcode.Items.Count > 0)
                    {
                        comcode.SelectedIndex = comcode.Items.Count - 1;
                    }
                }
                if (s.Contains("NMS") || s.Contains("nms"))
                {
                    comnms.Items.Add(s);
                    if (comnms.Items.Count > 0)
                    {
                        comnms.SelectedIndex = comnms.Items.Count - 1;
                    }
                }
                if (s.Contains("SW") || s.Contains("sw"))
                {
                    comsw.Items.Add(s);
                    if (comsw.Items.Count > 0)
                    {
                        comsw.SelectedIndex = comsw.Items.Count - 1;
                    }
                }
                if ((s.Contains("config") || s.Contains("Config") || s.Contains("CONFIG")) && (!s.Contains("slotconfig") && !s.Contains("SlotConfig")))
                {
                    comconfig.Items.Add(s);
                    if (comconfig.Items.Count > 0)
                    {
                        comconfig.SelectedIndex = comconfig.Items.Count - 1;
                    }
                }
                if (s.Contains("db") || s.Contains("DB") || s.Contains("Db"))
                {
                    comdb.Items.Add(s);
                    if (comdb.Items.Count > 0)
                    {
                        comdb.SelectedIndex = comdb.Items.Count - 1;
                    }
                }
                if (s.Contains("slotconfig") || s.Contains("SlotConfig") || s.Contains("SLOTCONFIG"))
                {
                    comslotconfig.Items.Add(s);
                    if (comslotconfig.Items.Count > 0)
                    {
                        comslotconfig.SelectedIndex = comslotconfig.Items.Count - 1;
                    }
                }
                if (s.Contains("760s") || s.Contains("760S"))
                {
                    com760s.Items.Add(s);
                    if (com760s.Items.Count > 0)
                    {
                        com760s.SelectedIndex = com760s.Items.Count - 1;
                    }
                }
                if (s.Contains("760b") || s.Contains("760B"))
                {
                    com760b.Items.Add(s);
                    if (com760b.Items.Count > 0)
                    {
                        com760b.SelectedIndex = com760b.Items.Count - 1;
                    }
                }
                if (s.Contains("760c") || s.Contains("760C"))
                {
                    com760c.Items.Add(s);
                    if (com760c.Items.Count > 0)
                    {
                        com760c.SelectedIndex = com760c.Items.Count - 1;
                    }
                }
                if (s.Contains("760d") || s.Contains("760D"))
                {
                    com760d.Items.Add(s);
                    if (com760d.Items.Count > 0)
                    {
                        com760d.SelectedIndex = com760d.Items.Count - 1;
                    }
                }
                if (s.Contains("760e") || s.Contains("760E"))
                {
                    com760e.Items.Add(s);
                    if (com760e.Items.Count > 0)
                    {
                        com760e.SelectedIndex = com760e.Items.Count - 1;
                    }
                }
                if (s.Contains("760f") || s.Contains("760F"))
                {
                    com760f.Items.Add(s);
                    if (com760f.Items.Count > 0)
                    {
                        com760f.SelectedIndex = com760f.Items.Count - 1;
                    }
                }
                if (s.Contains("sysfile") || s.Contains("Sysfile") || s.Contains("SYSFILE") || s.Contains("SysFile"))
                {
                    comsysfile.Items.Add(s);
                    if (comsysfile.Items.Count > 0)
                    {
                        comsysfile.SelectedIndex = comsysfile.Items.Count - 1;
                    }
                }
                if (s.Contains("flash") || s.Contains("Flash") || s.Contains("FLASH"))
                {
                    comflash.Items.Add(s);
                    if (comflash.Items.Count > 0)
                    {
                        comflash.SelectedIndex = comflash.Items.Count - 1;
                    }
                }
                if (s.Contains("yaffs") || s.Contains("Yaffs") || s.Contains("YAFFS"))
                {
                    comyaffs.Items.Add(s);
                    if (comyaffs.Items.Count > 0)
                    {
                        comyaffs.SelectedIndex = comyaffs.Items.Count - 1;
                    }
                }
            }
        }
        #endregion
        #region 保存记录内容
        private void Savecom()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "保存打印记录";
            //sfd.InitialDirectory = @"C:\";
            sfd.Filter = "文本文件| *.txt";
            sfd.FileName = comip.Text + "-" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + "所有窗口的日志记录";
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            using (FileStream fsWrite = new FileStream(sfd.FileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                string saveString = "";
                for (int i = 0; i < lstboxStatus.Items.Count; i++)
                {
                    saveString += lstboxStatus.Items[i].ToString() + "\r\n";
                }
                string paichajieguo = richTextEnd.Text.Replace("\n", "\r\n");
                string jilu = toolStripStatusLabelver.Text
                    + "\r\n" + toolStripStatusLabelnms.Text
                    + "\r\n" + toolStripStatusLabelnms18.Text
                    + "\r\n" + toolStripStatusLabelswa11.Text
                    + "\r\n" + toolStripStatusLabelswa12.Text
                    + "\r\n" + comapp.Text + ":  " + checkapp.Checked
                    + "\r\n" + comcode.Text + ":  " + checkcode.Checked
                    + "\r\n" + comnms.Text + ":  " + checknms.Checked
                    + "\r\n" + comsw.Text + ":  " + checksw.Checked
                    + "\r\n" + com760s.Text + ":  " + check760s.Checked
                    + "\r\n" + com760b.Text + ":  " + check760b.Checked
                    + "\r\n" + com760c.Text + ":  " + check760c.Checked
                    + "\r\n" + com760d.Text + ":  " + check760d.Checked
                    + "\r\n" + com760e.Text + ":  " + check760e.Checked
                    + "\r\n" + com760f.Text + ":  " + check760f.Checked
                    + "\r\n" + comsysfile.Text + ":  " + checksysfile.Checked
                    + "\r\n" + comflash.Text + ":  " + checkflash.Checked
                    + "\r\n" + comyaffs.Text + ":  " + checkyaffs.Checked
                    + "\r\n" + toolStripStatusLabeltime.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + textDOS.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + paichajieguo.ToString()
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + textlog.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + saveString;
                byte[] buffer = Encoding.Default.GetBytes(jilu);
                fsWrite.Write(buffer, 0, buffer.Length);
                MessageBox.Show("保存成功!");
            }
        }
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Savecom();
        }
        #endregion
        #region ctrl+s快捷键保存
        private void GPN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.S)
            {
                this.保存ToolStripMenuItem.PerformClick();
            }
            if (e.KeyCode == Keys.Down)
            {
                this.butsend.Focus();
                textcom.Text = "";
            }
            if (e.Control == true && e.KeyCode == Keys.C)
            {
                mysocket.SendDate("\x03");
                // Thread.Sleep(XHTime);
                //textDOS.AppendText("\r\n"+"CTRL+C已发送");
                this.butsend.PerformClick();
                butguzhangsend.PerformClick();
            }
            if (e.Control == true && e.KeyCode == Keys.Q)
            {
                mysocket.SendDate("\x011");
                // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +" "+"CTRL+Q已发送");
                this.butsend.PerformClick();
            }
            if (e.Control == true && e.KeyCode == Keys.Back)
            {
                //string msg = textDOS.Text;
                //textDOS.Text = msg.Substring(0, msg.Length - 1);
                //mysocket.SendDate("\b");
                MessageBox.Show("按下了back键");
            }
        }
        static int SubstringCount(string str, string substring)
        {
            if (str.Contains(substring))
            {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }
            return 0;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                // MessageBox.Show("抓住Tab ");
                mysocket.SendDate(textcom.Text + "\t");
                Thread.Sleep(XHTime);
                string stra = mysocket.ReceiveData(int.Parse(ts));
                string b = "\b";
                int a = SubstringCount(stra, b);
                if (textcom.Text != "")
                {
                    if (textcom.Text.Contains(" "))
                    {
                        string dos = textcom.Text;
                        string dos2 = dos.Substring(0, dos.Length - a);
                        string luanma = "\b";
                        string newSS = stra.Replace(luanma, "");
                        string newSw = newSS.Replace(dos, dos2);
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + newSw);
                    }
                    else
                    {
                        string luanma = "\b";
                        string newSS = stra.Replace(luanma, "");
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + newSS);
                    }
                }
                else
                {
                    string luanma = "\b";
                    string newSS = stra.Replace(luanma, "");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + newSS);
                }
                //string luanma = "\b";
                //string newSS = ss.Replace(luanma,"");
                textcom.Text = "";
                //this.butsend.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
        #region 耗时现显示
        System.Threading.Timer Mytimer;
        long TimeCount;
        delegate void SetValue();
        private void TimerUp(object state)
        {
            this.Invoke(new SetValue(ShowTime));
            TimeCount++;
        }
        public void ShowTime()
        {
            TimeSpan t = new TimeSpan(0, 0, (int)TimeCount);
            toolStripStatusLabeltime.Text = string.Format("{0:00}:{1:00}:{2:00}", t.Hours, t.Minutes, t.Seconds);
        }
        //开始计时  
        private void GPN_Load(object sender, EventArgs e)
        {
            Mytimer = new System.Threading.Timer(new TimerCallback(TimerUp), null, Timeout.Infinite, 1000);
            btnFtpServerStartStop.PerformClick();
            metroComreadoid.Text = "WALK";
            tbxFtpServerPort.Text = "21";
            //checkpssd.CheckedChanged = true;
            labelboard.Visible = false;
            labelslot.Visible = false;
            labelvcg.Visible = false;
            comboard.Visible = false;
            comslot.Visible = false;
            comvcg.Visible = false;
            butpaigu.Visible = false;
            butsyslog.Visible = false;
            labeleth.Visible = false;
            cometh.Visible = false;
            labelethboard.Visible = false;
            labelethslot.Visible = false;
            comethboard.Visible = false;
            comethslot.Visible = false;
            textcyclemingling.Visible = false;
            labcishu.Visible = false;
            comcishu.Visible = false;
            butCycleStart.Visible = false;
            butCycleSuspend.Visible = false;
            // this.WindowState = FormWindowState.Maximized;
        }
        #endregion
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
        #endregion
        #region 设置ini文件内容

        private void Gpnsetini()
        {

            try
            {
                //根据INI文件名设置要写入INI文件的节点名称
                //此处的节点名称完全可以根据实际需要进行配置
                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                WritePrivateProfileString(strSec, "FTPip", comftpip.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "FTPport", tbxFtpServerPort.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "FTPuser", textftpusr.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "FTPpsd", textftppsd.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "FTPpath", tbxFtpRoot.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNip", comip.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNuser", textusr.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNpsd", textpsd.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "ReadCommunity", textReadCommunity.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "WriteCommunity", textWriteCommunity.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNpsden", textpsden.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "APP", comapp.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "FPFA_CODE", comcode.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "NMS", comnms.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "SW", comsw.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760S", com760s.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760B", com760b.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760C", com760c.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760D", com760d.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760E", com760e.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760F", com760f.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "sysfile", comsysfile.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "Flash", comflash.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "Yaffs", comyaffs.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPN7600EMS", comgpn76list.Text.Trim(), strFilePath);
                int a = 0;
                bool zhixing;
                string ipadd = "";
                for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
                {
                    ipadd = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();
                    a = int.Parse(DGVSTATUS.Rows[i].Cells["优先级"].Value.ToString());
                    zhixing = bool.Parse(DGVSTATUS.Rows[i].Cells["执行"].Value.ToString());
                    DGVSTATUS.Rows[i].Cells["优先级"].Value = a;
                    Gpnip user = new Gpnip();
                    // 登录时 如果没有Data.bin文件就创建、有就打开
                    FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
                    BinaryFormatter bf = new BinaryFormatter();
                    // 保存在实体类属性中
                    user.GpnIP = ipadd;
                    user.GpnPRY = a;
                    user.GpnZX = zhixing;
                    if (userss.ContainsKey(user.GpnIP))
                    {
                        //如果有清掉
                        userss.Remove(user.GpnIP);
                        // MessageBox.Show("ip已经存在，替换完成");
                    }
                    //添加用户信息到集合
                    userss.Add(user.GpnIP, user);
                    //写入文件
                    bf.Serialize(fs, userss);

                    fs.Close();
                 //   MySocket.WriteLogs("Logs", "软件窗口所有日志：", EmailBody);



                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Gpnsetini();
        }
        private string ContentValue(string Section, string key)
        {
            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, strFilePath);
            return temp.ToString();
        }
        #endregion
        #region 退出窗口
        private void GPN_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("是否退出并保存？", "提示", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes)
                {
                    Gpnsetini();
                    ///////保存telnet 记录////////
                    Savecom();
                    //  导出前俩列ToolStripMenuItem.PerformClick();
                    if (metroButTrap.Text == "禁止Trap监听")
                    {
                        run = false;
                        trap.Abort();
                        socket.Close();
                    }
                    //socket.Shutdown(SocketShutdown.Both);
                    //    Thread.Sleep(10);
                    //socket.Close();
                    //户选择确认的操作
                }
                else if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    //return;
                }
                if (dr == DialogResult.No)
                {
                    Gpnsetini();
                    //    导出前俩列ToolStripMenuItem.PerformClick();

                    if (metroButTrap.Text == "禁止Trap监听")
                    {
                        run = false;
                        trap.Abort();
                        socket.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region 显示密码
        private void Checkpssd_CheckedChanged(object sender, EventArgs e)
        {
            if (checkpssd.Checked)
            {
                textpsd.PasswordChar = (char)0;
                textpsden.PasswordChar = (char)0;
                textftppsd.PasswordChar = (char)0;
                textReadCommunity.PasswordChar = (char)0;
                textWriteCommunity.PasswordChar = (char)0;
            }
            else
            {
                textftppsd.PasswordChar = '*';
                textpsd.PasswordChar = '*';
                textpsden.PasswordChar = '*';
                textReadCommunity.PasswordChar = '*';
                textWriteCommunity.PasswordChar = '*';
            }
        }
        #endregion
        private void Butbatch_Click(object sender, EventArgs e)
        {
            //if (string.Compare(btnFtpServerStartStop.Text, "启动FTP") == 0)
            //{
            //    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
            //    return;
            //}
            MessageBox.Show("支持了三方FTP工具，请先启动第三方FTP工具,然后点击批量升级。否则会出现卡死的情况，得重新关闭软件在打开！"
                + "\r\n" + "注意事项：FTP用户名：admin密码：admin必须一样，APP文件必须和升级的文件名一致");
            Batch batchfrm = new Batch
            {
                FTPIP = comftpip.Text,
                FTPUSR = textftpusr.Text,
                FTPPSD = textftppsd.Text,
                GPNUSR = textusr.Text,
                GPNPSD = textpsd.Text,
                GPNPSDEN = textpsden.Text,
                yanshi = ts,
                app = comapp.Text
            };//实例化窗体
            batchfrm.ShowDialog();// 将窗体显示出来
            //this.Hide();//当前窗体隐藏
        }
        private void GPN_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color FColor = Color.White;
            Color TColor = Color.Green;
            if (this.ClientRectangle.Height != 0)
            {
                Brush b = new LinearGradientBrush(this.ClientRectangle, FColor, TColor, LinearGradientMode.Vertical);
                g.FillRectangle(b, this.ClientRectangle);
            }
        }
        private void GPN_Resize(object sender, EventArgs e)
        {
            this.Invalidate();//重绘窗体
        }
        private void 关于ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void Butgpnall_Click(object sender, EventArgs e)
        {
            if (butgpnall.Text == "全部勾选")
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                check760s.Checked = true;
                check760b.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                check760e.Checked = true;
                //    check760f.Checked = true;
                checksysfile.Checked = true;
                checkconfig.Checked = true;
                checkslotconfig.Checked = true;
                checkdb.Checked = true;
                butgpnall.Text = "取消勾选";
                butgpn7600.Text = "GPN76-OTN勾选";
                butgpn800.Text = "GPN800勾选";
                butgpn7600old.Text = "GPN76-PTN勾选";
            }
            else
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                checkconfig.Checked = false;
                checkslotconfig.Checked = false;
                checkdb.Checked = false;
                butgpnall.Text = "全部勾选";
            }
        }
        private void Butgpn7600_Click(object sender, EventArgs e)
        {
            if (butgpn7600.Text == "GPN76-OTN勾选")
            {
                checkconfig.Checked = false;
                checkslotconfig.Checked = false;
                checkdb.Checked = false;
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                check760s.Checked = true;
                check760b.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                check760e.Checked = true;
                // check760f.Checked = true;
                checksysfile.Checked = true;
                butgpn7600.Text = "取消勾选";
                butgpnall.Text = "全部勾选";
                butgpn800.Text = "GPN800勾选";
                butgpn7600old.Text = "GPN76-PTN勾选";
            }
            else
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                butgpn7600.Text = "GPN76-OTN勾选";
            }
        }
        private void Butgpn800_Click(object sender, EventArgs e)
        {
            if (butgpn800.Text == "GPN800勾选")
            {
                checkconfig.Checked = false;
                checkslotconfig.Checked = false;
                checkdb.Checked = false;
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checknms.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                //   check760f.Checked = true;
                butgpn800.Text = "取消勾选";
                butgpn7600.Text = "GPN76-OTN勾选";
                butgpnall.Text = "全部勾选";
                butgpn7600old.Text = "GPN76-PTN勾选";
            }
            else
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                check760f.Checked = false;
                checksysfile.Checked = false;
                butgpn800.Text = "GPN800勾选";
            }
        }
        private void Butgpn7600old_Click(object sender, EventArgs e)
        {
            if (butgpn7600old.Text == "GPN76-PTN勾选")
            {
                checkconfig.Checked = false;
                checkslotconfig.Checked = false;
                checkdb.Checked = false;
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                butgpn7600old.Text = "取消勾选";
                butgpn7600.Text = "GPN76-OTN勾选";
                butgpnall.Text = "全部勾选";
                butgpn800.Text = "GPN800勾选";
            }
            else
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760s.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checksysfile.Checked = false;
                butgpn7600old.Text = "GPN76-PTN勾选";
            }
        }

        /// <summary>
        /// 一键导出入职主程序
        /// </summary>
        /// <param name="obj"></param>
        public void Syslog(object obj)
        {
            try
            {
                textguzhangmingling.Text = "screen lines 40";
                butguzhangsend.PerformClick();
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////版本信息//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show ver";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(XHTime / 3);
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////FPGA的SP版本/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "grosadvdebug";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "show fpga";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        break;
                    }
                    Thread.Sleep(XHTime / 3);
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////槽位信息//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show slot";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(XHTime / 3);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////配置文件信息//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show run";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(XHTime / 3);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////CPU内存利用率//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show system resource usage";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                if (toolStripStatusLabelver.Text.Contains("R13"))
                {
                    textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////进程CPU利用率//////////////////////////////" + "\r\n");
                    textguzhangmingling.Text = "show task";
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        //MessageBox.Show("进入循环");
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            break;
                        }
                        //Thread.Sleep(XHTime);
                    }
                }
                else
                {
                    textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////进程CPU利用率//////////////////////////////" + "\r\n");
                    textguzhangmingling.Text = "show task cpu";
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        //MessageBox.Show("进入循环");
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            break;
                        }
                        //Thread.Sleep(XHTime);
                    }
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////温度信息///////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show temperature";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////存储器系统日志//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show nvram syslog";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(XHTime / 3);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////当前告警//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show current alarm";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////ARP地址表//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show arp all";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////MAC地址表//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show forward-entry";
                butguzhangsend.PerformClick();
                textcurrent.AppendText("\r\n" + "此处大约要等10S钟，请耐心等待......" + "\r\n");
                Thread.Sleep(10000);
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////接口状态信息//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show port-link";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////当前系统时间//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show time";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////设备启动时间//////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show start time";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime);
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////系统告警日志/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show alarm log";
                butguzhangsend.PerformClick();
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        break;
                    }
                    Thread.Sleep(XHTime / 10);
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////内存队列信息/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "grosadvdebug";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "show que";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        break;
                    }
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////APP内部版本信息/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "grosadvdebug";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "show debug-version";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        break;
                    }
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////DCN-J2信息/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show running-config dcn-j2";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "show dcn-j2 summary";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        break;
                    }
                }
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////E1接口告警信息查询/////////////////////////////");
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////告警优先看最左侧的，因为告警级别最高/////////////////////////////");
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////AIS, 传输故障，业务未配置/////////////////////////////");
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////RDI，对端接收有AIS告警/////////////////////////////");
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////REI，对端接收有误码/////////////////////////////");
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////TIM，告警不影响业务/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "config msap";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/1";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/2";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/3";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/4";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/5";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/6";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/7";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "ioctl lpoh show 17/8";
                butguzhangsend.PerformClick();
                textcurrent.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////背板误码查询/////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "show vc4";
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        break;
                    }
                }
                textlog.AppendText(textcurrent.Text);
                textcurrent.Text = "";
                butguzhangsend.PerformClick();
                this.保存ToolStripMenuItem.PerformClick();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        /// <summary>
        /// 故障排查主程序
        /// </summary>
        /// <param name="obj"></param>
        public void GuZhangPaiCha(object obj)
        {
            try
            {


                textcurrent.Text = "当前窗口：";
                richTextEnd.Text = "故障排查结果：";
                textlog.Text = "故障排查日志：";
                textguzhangmingling.Text = "screen lines 40";
                butguzhangsend.PerformClick();
                string Tsvc12 = "";
                string Tsvc4 = "";
                string SDH = "";
                string VcgTxFrames1 = "";
                string VcgTxFrames2 = "";
                string VcgRxFrames1 = "";
                string VcgRxFrames2 = "";
                string MacTxFrames1 = "";
                string MacTxFrames2 = "";
                string MacRxFrames1 = "";
                string MacRxFrames2 = "";
                string EthOutFrames1 = "";
                string EthOutFrames2 = "";
                string EthInFrames1 = "";
                string EthInFrames2 = "";
                string Eos126vlanmismatchDropRx1 = "";
                string Eos126vlanmismatchDropRx2 = "";
                string EthInDiscardFrames1 = "";
                string EthOutDiscardFrames1 = "";
                string EthInCrcErrorPkts1 = "";
                string EthOutCrcErrorPkts1 = "";
                string EthInDiscardFrames2 = "";
                string EthOutDiscardFrames2 = "";
                string EthInCrcErrorPkts2 = "";
                string EthOutCrcErrorPkts2 = "";
                richTextEnd.AppendText("\r\n" + "板卡：" + comboard.Text + "\r\n" + "接口：" + comslot.Text + "/" + comvcg.Text + "\r\n");
                if (comboard.Text == "EOS-8FX" || comboard.Text == "EOS-8FE" || comboard.Text == "DMD-8GE" || comboard.Text == "EOS/P-126")
                {
                    textlog.AppendText("\r\n" + "///////////////////////////VCG接口信息/////////////////////////////////////////////" + "\r\n");
                    textguzhangmingling.Text = "interface vcg " + comslot.Text + "/" + comvcg.Text;
                    butguzhangsend.PerformClick();
                    if (textcurrent.Text.Contains("can not support vcg interface"))
                    {
                        richTextEnd.AppendText("VCG接口不存在，请配置后再试！" + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        butguzhangsend.PerformClick();
                        MessageBox.Show("VCG接口不存在，请配置后再试！");
                        return;
                    }
                    textguzhangmingling.Text = "show";
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            if (textcurrent.Text.Contains("vc12,"))
                            {
                                richTextEnd.AppendText("业务级别：VC12" + "\r\n");
                                if (textcurrent.Text.Contains("nul:"))
                                {
                                    Regex Ts = new Regex(@"nul:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"nul:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("sdh:"))
                                {
                                    Regex Ts = new Regex(@"sdh:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"sdh:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("vcg:"))
                                {
                                    Regex Ts = new Regex(@"vcg:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"vcg:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                            }
                            if (textcurrent.Text.Contains("vc3,"))
                            {
                                richTextEnd.AppendText("业务级别：VC3" + "\r\n");
                                if (textcurrent.Text.Contains("nul:"))
                                {
                                    Regex Ts = new Regex(@"nul:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"nul:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("sdh:"))
                                {
                                    Regex Ts = new Regex(@"sdh:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"sdh:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("vcg:"))
                                {
                                    Regex Ts = new Regex(@"vcg:\s*([\d\/\d\-\d\-\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc12 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"vcg:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc12.ToString() + "\r\n");
                                    if (Tsvc12.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                            }
                            if (textcurrent.Text.Contains("vc4,"))
                            {
                                richTextEnd.AppendText("业务级别：VC4" + "\r\n");
                                if (textcurrent.Text.Contains("nul:"))
                                {
                                    Regex Ts = new Regex(@"nul:\s*(([\d]+)\/\d\-\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc4 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"nul:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc4.ToString() + "\r\n");
                                    if (Tsvc4.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("sdh:"))
                                {
                                    Regex Ts = new Regex(@"sdh:\s*(([\d]+)\/\d\-\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc4 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"sdh:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc4.ToString() + "\r\n");
                                    if (Tsvc4.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                                if (textcurrent.Text.Contains("vcg:"))
                                {
                                    Regex Ts = new Regex(@"vcg:\s*(\d\/\d\-\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Ts0 = Ts.Match(textcurrent.Text).Groups[1].Value;
                                    Tsvc4 = Ts0.Replace("-", "/");
                                    Regex Port0 = new Regex(@"vcg:\s*([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    SDH = Port0.Match(textcurrent.Text).Groups[1].Value;
                                    richTextEnd.AppendText("上联接口：" + SDH.ToString() + "\r\n");
                                    richTextEnd.AppendText("上联时隙：" + Tsvc4.ToString() + "\r\n");
                                    if (Tsvc4.ToString() == "")
                                    {
                                        richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                        return;
                                    }
                                }
                            }
                            if (!textcurrent.Text.Contains("vc12,") && !textcurrent.Text.Contains("vc3,") && !textcurrent.Text.Contains("vc4,"))
                            {
                                richTextEnd.AppendText("交叉时隙不存在，请配置时隙后再试！" + "\r\n");
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                MessageBox.Show("交叉时隙不存在，请配置时隙后再试！");
                                return;
                            }
                            Regex r = new Regex(@"LCAS:\s*([\w]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Lcas = r.Match(textcurrent.Text).Groups[1].Value;
                            richTextEnd.AppendText("LCAS状态：" + Lcas + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            break;
                        }
                    }
                    textlog.AppendText("\r\n" + "///////////////////////////VCG成员信息和告警查询//////////////////////////////////");
                    textlog.AppendText("\r\n" + "///////////////////////////AIS，传输故障，业务未配置//////////////////////////////");
                    textlog.AppendText("\r\n" + "///////////////////////////PLM，V5或者C2字节匹配//////////////////////////////////");
                    textlog.AppendText("\r\n" + "///////////////////////////RDI，对端EOS的VCG接收有AIS告警/////////////////////////");
                    textlog.AppendText("\r\n" + "///////////////////////////REI，对接EOS的VCG接收有误码////////////////////////////");
                    textlog.AppendText("\r\n" + "///////////////////////////RxEn状态，en、OK 是正常的，err、dnu、add是传输侧VCG故障");
                    textlog.AppendText("\r\n" + "///////////////////////////TxEn状态，en、OK 是正常的，err、dnu、add是我司设备VCG故障" + "\r\n");
                    textguzhangmingling.Text = "show current-state";
                    butguzhangsend.PerformClick();
                    Thread.Sleep(XHTime);
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        //MessageBox.Show(g.ToString());
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                            //MessageBox.Show("点击按钮");
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + g.ToString());
                            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + g.ToString());
                            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + g.ToString());
                        }
                        else
                        {
                            //MessageBox.Show("跳出循环");
                            if (textcurrent.Text.Contains("AIS") || textcurrent.Text.Contains("PLM") || textcurrent.Text.Contains("REI") || textcurrent.Text.Contains("RDI") || textcurrent.Text.Contains("add") || textcurrent.Text.Contains("err") || textcurrent.Text.Contains("dnu"))
                            {
                                if (textcurrent.Text.Contains("AIS"))
                                {
                                    richTextEnd.AppendText("VCG接口告警：NOK。存在AIS告警" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("PLM") && !textcurrent.Text.Contains("AIS"))
                                {
                                    richTextEnd.AppendText("VCG接口告警：NOK。存在PLM告警" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("RDI") && !textcurrent.Text.Contains("AIS"))
                                {
                                    richTextEnd.AppendText("VCG接口告警：NOK。存在RDI告警" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("REI") && !textcurrent.Text.Contains("AIS"))
                                {
                                    richTextEnd.AppendText("VCG接口告警：NOK。存在REI告警" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("add") || textcurrent.Text.Contains("err") || textcurrent.Text.Contains("dnu") && !textcurrent.Text.Contains("AIS"))
                                {
                                    richTextEnd.AppendText("VCG接口告警：NOK 。VCG状态存在add/err/dnu，请检查俩端LCAS状态是否匹配" + "\r\n");
                                }
                            }
                            else
                            {
                                richTextEnd.AppendText("VCG接口告警：OK" + "\r\n");
                            }
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            butguzhangsend.PerformClick();
                            break;
                        }
                    }
                    if ((comboard.Text == "EOS-8FX") || (comboard.Text == "EOS-8FE"))
                    {
                        textguzhangmingling.Text = "interface msap-eth " + comslot.Text + "/" + comvcg.Text;
                        butguzhangsend.PerformClick();
                        textlog.AppendText("\r\n" + "/////////////////////////////MSAP-ETH接口状态信息/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "show configuration";
                        butguzhangsend.PerformClick();
                        if (textcurrent.Text.Contains("100TX"))
                        {
                            richTextEnd.AppendText("接口模块：100M电接口，非SFP电模块" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("100FX"))
                        {
                            richTextEnd.AppendText("接口模块：100M光接口" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("1000FX"))
                        {
                            richTextEnd.AppendText("接口模块：1000M光接口" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("1000TX"))
                        {
                            richTextEnd.AppendText("接口模块：1000M电接口，非SFP电模块" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("AUTO"))
                        {
                            richTextEnd.AppendText("自协商：使能" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("100MFULL"))
                        {
                            richTextEnd.AppendText("自协商：禁止，100M全双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("10MFULL"))
                        {
                            richTextEnd.AppendText("自协商：禁止，10M全双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("1000MFULL"))
                        {
                            richTextEnd.AppendText("自协商：禁止，1000M全双工" + "\r\n");
                        }
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        textguzhangmingling.Text = "show current";
                        butguzhangsend.PerformClick();
                        if (textcurrent.Text.Contains("up"))
                        {
                            richTextEnd.AppendText("接口Link状态：up" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("down"))
                        {
                            richTextEnd.AppendText("接口Link状态：NOK。实际是：down" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("10mFull"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：10M全双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("10mHalf"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：10M半双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("100mFull"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：100M全双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("100mHalf"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：100M半双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("1000mFull"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：1000M全双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("1000mHalf"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：1000M半双工" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("-   transparent"))
                        {
                            richTextEnd.AppendText("当前运行双工模块：不支持查询" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("no board inserted"))
                        {
                            richTextEnd.AppendText("检测到板卡未插入，请插入后再试！" + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            MessageBox.Show("检测到板卡未插入，请插入后再试！");
                            return;
                        }
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        textlog.AppendText("\r\n" + "/////////////////////////////MSAP-SFP光模块信息/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "show sfp";
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                if (textcurrent.Text.Contains("LOS"))
                                {
                                    richTextEnd.AppendText("光模块收光：NOK。LOS请检查光纤" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("OK"))
                                {
                                    //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                    //MessageBox.Show(textcurrent.Text);
                                    Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                    Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                    Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                    Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                    Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                    //MessageBox.Show(RxPower.ToString());
                                    richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                    richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                    richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                    richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                    richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                }
                                if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                {
                                    richTextEnd.AppendText("光模块收光：NOK。光模块未插入" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("NOT_SUPPORT"))
                                {
                                    richTextEnd.AppendText("光模块信息：非SFP模块或者电接口，不支持查询" + "\r\n");
                                }
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                butguzhangsend.PerformClick();
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                        //textlog.AppendText("\r\n" + "/////////////////////////////MSAP-ETH流量信息/////////////////////////////");
                        //textlog.AppendText("\r\n" + "/////////////////////////////Int方向，从客户侧来的流量/////////////////////////////");
                        //textlog.AppendText("\r\n" + "/////////////////////////////Out方向，从传输侧来的流量/////////////////////////////" + "\r\n");
                        //textguzhangmingling.Text = "show portpfm current";
                        //butguzhangsend.PerformClick();
                        //for (int g = 0; g <= 10; g++)
                        //{
                        //    if (textcurrent.Text.Contains("Current Perform Counters End"))
                        //    {
                        //        //MessageBox.Show("跳出循环");
                        //        textguzhangmingling.Text = "exit";
                        //        butguzhangsend.PerformClick();
                        //        textlog.AppendText(textcurrent.Text);
                        //        textcurrent.Text = "";
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        butguzhangsend.PerformClick();
                        //    }
                        //    Thread.Sleep(XHTime);
                        //}
                    }
                    if (comboard.Text == "EOS-8FX" || comboard.Text == "EOS-8FE" || comboard.Text == "DMD-8GE")
                    {
                        textlog.AppendText("\r\n" + "/////////////////////////////VCG流量查询/////////////////////////////");
                        textlog.AppendText("\r\n" + "/////////////////////////////RX方向，从传输侧来的流量/////////////////////////////");
                        textlog.AppendText("\r\n" + "/////////////////////////////TX方向，从客户侧来的流量/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "config msap";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "rmon " + comslot.Text + " " + comvcg.Text;
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                Regex r = new Regex(@"VCG[\s\S]*RX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                VcgRxFrames1 = r.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("VCGRx1：" + VcgRxFrames1.ToString() + "\r\n");
                                Regex t = new Regex(@"VCG[\s\S]*TX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                VcgTxFrames1 = t.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("VCGTx1：" + VcgTxFrames1.ToString() + "\r\n");
                                Regex r1 = new Regex(@"RX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MacRxFrames1 = r1.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("MacRx1：" + MacRxFrames1.ToString() + "\r\n");
                                Regex t1 = new Regex(@"TX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MacTxFrames1 = t1.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("MacTx1：" + MacTxFrames1.ToString() + "\r\n");
                                //MessageBox.Show("跳出循环");
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                        textcurrent.AppendText("\r\n" + "///////////////////////请等待中，统计第二次流量查询///////////////////////");
                        Thread.Sleep(XHTime);
                        textlog.AppendText("\r\n" + "/////////////////////////////VCG流量查询/////////////////////////////");
                        textlog.AppendText("\r\n" + "/////////////////////////////RX方向，从传输侧来的流量/////////////////////////////");
                        textlog.AppendText("\r\n" + "/////////////////////////////TX方向，从客户侧来的流量/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "config msap";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "rmon " + comslot.Text + " " + comvcg.Text;
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                Regex r = new Regex(@"VCG[\s\S]*RX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                VcgRxFrames2 = r.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("Rx2：" + VcgRxFrames2.ToString() + "\r\n");
                                Regex t = new Regex(@"VCG[\s\S]*TX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                VcgTxFrames2 = t.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("Tx2：" + VcgTxFrames2.ToString() + "\r\n");
                                Regex r1 = new Regex(@"RX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MacRxFrames2 = r1.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("MacRx1：" + MacRxFrames2.ToString() + "\r\n");
                                Regex t1 = new Regex(@"TX\s*Frames\s*([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MacTxFrames2 = t1.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("MacTx1：" + MacTxFrames2.ToString() + "\r\n");
                                if (VcgRxFrames1 == VcgRxFrames2)
                                {
                                    richTextEnd.AppendText("VCG接口Rx流量：NOK。传输侧无流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("VCG接口Rx流量：OK。传输侧流量正常" + "\r\n");
                                }
                                if (VcgTxFrames1 == VcgTxFrames2)
                                {
                                    richTextEnd.AppendText("VCG接口Tx流量：NOK。客户侧无流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("VCG接口Tx流量：OK。客户侧流量正常" + "\r\n");
                                }
                                if (MacRxFrames1 == MacRxFrames2)
                                {
                                    richTextEnd.AppendText("Mac接口Rx流量：NOK。客户侧无流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("Mac接口Rx流量：OK。客户侧流量正常" + "\r\n");
                                }
                                if (MacTxFrames1 == MacTxFrames2)
                                {
                                    richTextEnd.AppendText("Mac接口Tx流量：NOK。传输侧无流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("Mac接口Tx流量：OK。传输侧流量正常" + "\r\n");
                                }
                                //MessageBox.Show("跳出循环");
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    if (comboard.Text == "EOS-8FX" || comboard.Text == "EOS-8FE" || comboard.Text == "EOS/P-126")
                    {
                        textlog.AppendText("\r\n" + "/////////////////////////////VCG错包查询/////////////////////////////");
                        textguzhangmingling.Text = "grosadvdebug";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "debug command enable";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "config msap";
                        butguzhangsend.PerformClick();
                        if (comboard.Text == "EOS-8FX")
                        {
                            textlog.AppendText("\r\n" + "/////////////////////////////TX有计数增加，说明客户侧EOS8FX到1501S光路质量不好///////");
                            textlog.AppendText("\r\n" + "/////////////////////////////RX有计数增加，说明传输侧有误码或者VCG接口扰码不对应/////");
                            textguzhangmingling.Text = "eos8 vcg autofiforeset show " + comslot.Text;
                            butguzhangsend.PerformClick();
                        }
                        if (comboard.Text == "EOS-8FE")
                        {
                            textlog.AppendText("\r\n" + "/////////////////////////////TX有计数增加，说明客户侧EOS8FX到1501S光路质量不好///////");
                            textlog.AppendText("\r\n" + "/////////////////////////////RX有计数增加，说明传输侧有误码或者VCG接口扰码不对应/////");
                            textguzhangmingling.Text = "eos_8fe vcg autofiforeset show " + comslot.Text;
                            butguzhangsend.PerformClick();
                        }
                        if (comboard.Text == "EOS/P-126")
                        {
                            textlog.AppendText("\r\n" + "/////////////////////////////SDH到MAC方向，RX，从传输侧过来的流量/////////////////////");
                            textlog.AppendText("\r\n" + "/////////////////////////////MAC到SDH方向，RX，从客户侧过来的流量/////////////////////");
                            textguzhangmingling.Text = "eops126 gfprmon " + comslot.Text + " " + comvcg.Text + " sdram";
                            butguzhangsend.PerformClick();
                            Thread.Sleep(XHTime);
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textguzhangmingling.Text = "eops126 gfprmon " + comslot.Text + " " + comvcg.Text + " sdram";
                            butguzhangsend.PerformClick();
                            Thread.Sleep(XHTime);
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textguzhangmingling.Text = "eops126 gfprmon " + comslot.Text + " " + comvcg.Text + " sdram";
                            butguzhangsend.PerformClick();
                            Regex r = new Regex(@"MAC->SDH[\s\S]*Rx[\.]TotalFrames\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            EthInFrames1 = r.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("MAC->SDH.Rx流量：" + EthInFrames1.ToString() + "\r\n");
                            Regex t = new Regex(@"Rx[\.]TotalFrames\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            VcgRxFrames1 = t.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("SDH->MAC.Rx流量：" + VcgRxFrames1.ToString() + "\r\n");
                            Regex v = new Regex(@"Rx[\.]VlanMismatchDrop\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            Eos126vlanmismatchDropRx1 = v.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("SDH->MAC.Rx vlan不匹配，丢包数量：" + Eos126vlanmismatchDropRx1.ToString() + "\r\n");
                            //MessageBox.Show("跳出循环");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textcurrent.AppendText("\r\n" + "///////////////////////请等待5秒，统计第二次流量查询///////////////////////");
                            Thread.Sleep(5000);
                            textguzhangmingling.Text = "eops126 gfprmon " + comslot.Text + " " + comvcg.Text + " sdram";
                            butguzhangsend.PerformClick();
                            Regex r1 = new Regex(@"MAC->SDH[\s\S]*Rx[\.]TotalFrames\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            EthInFrames2 = r1.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("MAC->SDH.Rx流量：" + EthInFrames2.ToString() + "\r\n");
                            Regex t1 = new Regex(@"Rx[\.]TotalFrames\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            VcgRxFrames2 = t1.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("SDH->MAC.Rx流量：" + VcgRxFrames2.ToString() + "\r\n");
                            Regex v1 = new Regex(@"Rx[\.]VlanMismatchDrop\s*=\s*[\[]([\[\d]+)[\]]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            Eos126vlanmismatchDropRx2 = v1.Match(textcurrent.Text).Groups[1].Value;
                            //MessageBox.Show(RxPower.ToString());
                            //richTextEnd.AppendText("SDH->MAC.Rx vlan不匹配，丢包数量：" + Eos126vlanmismatchDropRx2.ToString() + "\r\n");
                            //MessageBox.Show("跳出循环");
                            if (EthInFrames1 == EthInFrames2)
                            {
                                richTextEnd.AppendText("交换Rx流量：NOK。客户侧无流量,检查4GE-BVLAN或AC流量配置是否OK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText("交换Rx流量：OK。客户侧流量正常" + "\r\n");
                            }
                            if (VcgRxFrames1 == VcgRxFrames2)
                            {
                                richTextEnd.AppendText("VCGRx流量：NOK。传输侧无流量，检查VCG接口告警，上联接口时隙告警，传输设备是否正常" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText("VCGRx流量：OK。传输侧流量正常" + "\r\n");
                            }
                            if (Eos126vlanmismatchDropRx1 == Eos126vlanmismatchDropRx2)
                            {
                                //richTextEnd.AppendText("VCGVlan配置：OK。传输侧流量正常" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText("VCGVlan配置：NOK。VCG接口VLAN丢包数量：" + Eos126vlanmismatchDropRx2 + "。SDH传输对端MSAP设备配置VLAN与我司VCG接口VLAN ID和模式必须匹配" + "\r\n");
                            }
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                        }
                    }
                    textlog.AppendText("\r\n" + "/////////////////////////////SDH上联口告警查询/////////////////////////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////告警优先看最左侧的，因为告警级别最高//////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////LOS，接口没有收光/////////////////////////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////LOF，光模块不匹配，端口速率不匹配/////////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////AIS, 传输故障，业务未配置/////////////////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////RDI，对端EOS的VCG接收有AIS告警////////////////");
                    textlog.AppendText("\r\n" + "/////////////////////////////REI，对接EOS的VCG接收有误码///////////////////" + "\r\n");
                    textguzhangmingling.Text = "config msap";
                    butguzhangsend.PerformClick();
                    textguzhangmingling.Text = "ioctl soh show " + SDH.ToString();
                    butguzhangsend.PerformClick();
                    if (textcurrent.Text.Contains("LOS") || textcurrent.Text.Contains("LOF") || textcurrent.Text.Contains("AIS") || textcurrent.Text.Contains("PLM") || textcurrent.Text.Contains("REI") || textcurrent.Text.Contains("RDI") || textcurrent.Text.Contains("add") || textcurrent.Text.Contains("err") || textcurrent.Text.Contains("dnu"))
                    {
                        if (textcurrent.Text.Contains("AIS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在AIS告警，检查上联口的对端和落地MSAP的EOS接口是否配置业务" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("PLM") && !textcurrent.Text.Contains("AIS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在PLM告警" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("RDI") && !textcurrent.Text.Contains("AIS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在RDI告警" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("REI") && !textcurrent.Text.Contains("AIS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在REI告警" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("LOS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在LOS告警，检查光纤与光模块" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("LOF") && !textcurrent.Text.Contains("LOS"))
                        {
                            richTextEnd.AppendText(SDH.ToString() + "上联接口告警：NOK。存在LOF告警，请检查SDH接口速率模式，光模块速率即可" + "\r\n");
                        }
                    }
                    else
                    {
                        richTextEnd.AppendText(SDH.ToString() + "上联接口告警：OK" + "\r\n");
                    }
                    textlog.AppendText(textcurrent.Text);
                    textcurrent.Text = "";
                    if (Tsvc12 != "")
                    {
                        textguzhangmingling.Text = "ioctl lpoh show " + Tsvc12.ToString();
                        butguzhangsend.PerformClick();
                        if (textcurrent.Text.Contains("LOS") || textcurrent.Text.Contains("LOF") || textcurrent.Text.Contains("AIS") || textcurrent.Text.Contains("PLM") || textcurrent.Text.Contains("REI") || textcurrent.Text.Contains("RDI") || textcurrent.Text.Contains("add") || textcurrent.Text.Contains("err") || textcurrent.Text.Contains("dnu"))
                        {
                            if (textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在AIS告警，检查上联口的对端和落地MSAP的EOS接口是否配置业务" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("PLM") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在PLM告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("RDI") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在RDI告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("REI") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在REI告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("LOS") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在LOS告警，检查光纤与光模块" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("LOF") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：NOK。存在LOF告警，检查光模块速率与SDH接口速率是否与对端一致" + "\r\n");
                            }
                        }
                        else
                        {
                            richTextEnd.AppendText(Tsvc12.ToString() + "上联时隙告警：OK" + "\r\n");
                        }
                    }
                    if (Tsvc4 != "")
                    {
                        textguzhangmingling.Text = "ioctl hpoh show " + Tsvc4.ToString();
                        butguzhangsend.PerformClick();
                        if (textcurrent.Text.Contains("LOS") || textcurrent.Text.Contains("LOF") || textcurrent.Text.Contains("AIS") || textcurrent.Text.Contains("PLM") || textcurrent.Text.Contains("REI") || textcurrent.Text.Contains("RDI") || textcurrent.Text.Contains("add") || textcurrent.Text.Contains("err") || textcurrent.Text.Contains("dnu"))
                        {
                            if (textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在AIS告警，检查上联口的对端和落地MSAP的EOS接口是否配置业务" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("PLM") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在PLM告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("RDI") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在RDI告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("REI") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在REI告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("LOS") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在LOS告警" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("LOF") && !textcurrent.Text.Contains("AIS"))
                            {
                                richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：NOK。存在LOF告警，检查光模块速率与SDH接口速率是否与对端一致" + "\r\n");
                            }
                        }
                        else
                        {
                            richTextEnd.AppendText(Tsvc4.ToString() + "上联时隙告警：OK" + "\r\n");
                        }
                    }
                    textlog.AppendText("\r\n" + "/////////////////////////////上行SDH接口SFP光模块信息/////////////////////////////" + "\r\n");
                    string[] SDHSFP = SDH.Split('/');
                    string SDHslot = SDHSFP[0];
                    string SDHport = SDHSFP[1];
                    textguzhangmingling.Text = "ioctl sfp show " + SDHslot + " " + SDHport;
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            if (textcurrent.Text.Contains("LOS"))
                            {
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块收光：NOK。LOS请检查光纤" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("OK"))
                            {
                                //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                //MessageBox.Show(textcurrent.Text);
                                Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块发光：" + TxPower.ToString() + "\r\n");
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块收光：" + RxPower.ToString() + "\r\n");
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                richTextEnd.AppendText(SDH.ToString() + "上联接口单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                            }
                            if (textcurrent.Text.Contains("SFP module is not inserted!"))
                            {
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块收光：NOK。光模块未插入" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("NOT_SUPPORT"))
                            {
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块信息：非SFP模块或者电接口，不支持查询" + "\r\n");
                            }
                            if (textcurrent.Text.Contains("Board not inserted!"))
                            {
                                richTextEnd.AppendText(SDH.ToString() + "上联接口光模块收光：NOK。板卡未插入" + "\r\n");
                            }
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                    textlog.AppendText(textcurrent.Text);
                    textcurrent.Text = "";
                    if (comboard.Text == "DMD-8GE")
                    {
                        textlog.AppendText("\r\n" + "/////////////////////////////VCG寄存器信息查询/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "config msap";
                        butguzhangsend.PerformClick();
                        Thread.Sleep(XHTime);
                        textguzhangmingling.Text = "ioctl vcg info " + comslot.Text + " " + comvcg.Text;
                        butguzhangsend.PerformClick();
                        //MessageBox.Show("跳出循环");
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                Thread.Sleep(XHTime);
                                string[] VCGINFOFengGe = textcurrent.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                string VCGinfo = VCGINFOFengGe[4];
                                textlog.AppendText(VCGinfo.ToString() + "\r\n");
                                VCGINFOFengGe = Regex.Split(VCGinfo, "\\s+", RegexOptions.IgnoreCase);
                                string MemberSum = VCGINFOFengGe[4];
                                string Protocol = VCGINFOFengGe[5];
                                string UNI = VCGINFOFengGe[7];
                                string L1MUX = VCGINFOFengGe[8];
                                string VLAN = VCGINFOFengGe[9];
                                string MPLS = VCGINFOFengGe[10];
                                string vlanacTxmode = VCGINFOFengGe[11];
                                string txVid = VCGINFOFengGe[12];
                                string vlanacRxmode = VCGINFOFengGe[14];
                                string rxVid = VCGINFOFengGe[15];
                                richTextEnd.AppendText("VCG绑定通道：" + MemberSum.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG封装协议：" + Protocol.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG绑定ETH接口：" + UNI.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG接口模式：" + L1MUX.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG匹配vlan号：" + VLAN.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG匹配mpls标签：" + MPLS.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG发送vlan动作：" + vlanacTxmode.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG发送vlan号：" + txVid.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG接收vlan动作：" + vlanacRxmode.ToString() + "\r\n");
                                richTextEnd.AppendText("VCG接收vlan号：" + rxVid.ToString() + "\r\n");
                                //MessageBox.Show("跳出循环");
                                Thread.Sleep(XHTime);
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                break;

                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    if (comboard.Text == "DMD-8GE")
                    {
                        textlog.AppendText("\r\n" + "/////////////////////////////以太口流量信息查询////////////////////////////" + "\r\n");
                        textlog.AppendText("\r\n" + "/////////////////////////////Int，从客户侧过来的流量///////////////////////" + "\r\n");
                        textlog.AppendText("\r\n" + "/////////////////////////////Out, 从传输侧过来的流量///////////////////////" + "\r\n");
                        textguzhangmingling.Text = "interface ethernet " + comethslot.Text + "/" + cometh.Text;
                        butguzhangsend.PerformClick();
                        textlog.AppendText("\r\n" + "//////进入ETH接口" + "interface ethernet " + comethslot.Text + "/" + cometh.Text + "\r\n");
                        textguzhangmingling.Text = "show configuration";
                        butguzhangsend.PerformClick();
                        if (textcurrent.Text.Contains("Physical status is down"))
                        {
                            richTextEnd.AppendText("接口Link状态：NOK。实际是：down" + "\r\n");
                        }
                        else
                        {
                            richTextEnd.AppendText("接口Link状态：up" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("administrator status is down"))
                        {
                            richTextEnd.AppendText("接口管理状态：NOK。实际是：down，可能人为配置了shutdown,请进入所在接口undo shutdown 使能接口" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("AutoNegotiation enabled"))
                        {
                            richTextEnd.AppendText("自协商：使能" + "\r\n");
                        }
                        else
                        {
                            richTextEnd.AppendText("自协商：禁止" + "\r\n");
                        }
                        if (textcurrent.Text.Contains("Duplex full"))
                        {
                            richTextEnd.AppendText("双工模式：全双工" + "\r\n");
                        }
                        else
                        {
                            richTextEnd.AppendText("双工模式：NOK。实际是：半双工" + "\r\n");
                        }
                        Regex speed0 = new Regex(@"current\s*speed\s*([\d\w\s]+)(,)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string Speed = speed0.Match(textcurrent.Text).Groups[1].Value;
                        richTextEnd.AppendText("当前速率：" + Speed + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        textlog.AppendText("\r\n" + "/////////////////////////////MSAP-SFP光模块信息/////////////////////////////" + "\r\n");
                        textguzhangmingling.Text = "show sfp";
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                if (textcurrent.Text.Contains("LOS"))
                                {
                                    richTextEnd.AppendText("光模块收光：NOK。LOS请检查光纤" + "\r\n");
                                }
                                if (textcurrent.Text.Contains("OK"))
                                {
                                    //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                    //MessageBox.Show(textcurrent.Text);
                                    Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                    Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.]+)\s*(dBm)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                    Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                    Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                    Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                    string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                    //MessageBox.Show(RxPower.ToString());
                                    richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                    richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                    richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                    richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                    richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                }
                                if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                {
                                    richTextEnd.AppendText("光模块收光：NOK。光模块未插入" + "\r\n");
                                }
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                butguzhangsend.PerformClick();
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                        textguzhangmingling.Text = "show statistics";
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                Regex InRate = new Regex(@"In Rate\(Last\s*\d*Sec\):([\d]+)\s*(kbps)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string InRate1 = InRate.Match(textcurrent.Text).Groups[1].Value;
                                double InRatekbps = double.Parse(InRate1);
                                double InRateMbps = Math.Round(InRatekbps / 1000, 2);
                                if (InRateMbps == 0)
                                {
                                    richTextEnd.AppendText("ETH接口I n带宽：NOK。客户侧实际带宽：" + InRateMbps.ToString() + "Mbps\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("ETH接口I n带宽：" + InRateMbps.ToString() + "Mbps\r\n");
                                }
                                Regex OutRate = new Regex(@"Out Rate\(Last\s*\d*Sec\):([\d]+)\s*(kbps)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string OutRate1 = OutRate.Match(textcurrent.Text).Groups[1].Value;
                                double OutRatekbps = double.Parse(OutRate1);
                                double OutRateMbps = Math.Round(OutRatekbps / 1000, 2);
                                if (OutRateMbps == 0)
                                {
                                    richTextEnd.AppendText("ETH接口Out带宽：NOK。传输侧实际带宽：" + OutRateMbps.ToString() + "Mbps\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("ETH接口Out带宽：" + OutRateMbps.ToString() + "Mbps\r\n");
                                }
                                Regex r = new Regex(@"In\s*Unicast\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInFrames1 = r.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("Rx1：" + RxFrames1.ToString() + "\r\n");
                                Regex t = new Regex(@"Out\s*Unicast\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutFrames1 = t.Match(textcurrent.Text).Groups[1].Value;
                                //richTextEnd.AppendText("Tx1：" + TxFrames1.ToString() + "\r\n");
                                Regex ind = new Regex(@"In\s*Discard\s*Frames\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInDiscardFrames1 = ind.Match(textcurrent.Text).Groups[1].Value;
                                Regex outd = new Regex(@"Out\s*Discard\s*Frames\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutDiscardFrames1 = outd.Match(textcurrent.Text).Groups[1].Value;
                                Regex inc = new Regex(@"In\s*CRC\s*Error\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInCrcErrorPkts1 = ind.Match(textcurrent.Text).Groups[1].Value;
                                Regex outc = new Regex(@"Out\s*CRC\s*Error\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutCrcErrorPkts1 = outc.Match(textcurrent.Text).Groups[1].Value;
                                Regex Sec = new Regex(@"Last\s*([\d]+)(Sec)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                string LastSec = Sec.Match(textcurrent.Text).Groups[1].Value;
                                Debug.WriteLine(LastSec);
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                int LastSec2 = (int.Parse(LastSec) + 5) * 1000;
                                textcurrent.AppendText("\r\n" + "///////////////////////请等待" + LastSec2 + "毫秒，统计第二次流量查询///////////////////////");
                                Thread.Sleep(LastSec2);
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    if (comboard.Text == "DMD-8GE")
                    {
                        textlog.AppendText("\r\n" + "/////////////////////////////以太口流量信息查询////////////////////////////" + "\r\n");
                        textlog.AppendText("\r\n" + "/////////////////////////////Int，从客户侧过来的流量///////////////////////" + "\r\n");
                        textlog.AppendText("\r\n" + "/////////////////////////////Out, 从传输侧过来的流量///////////////////////" + "\r\n");
                        textguzhangmingling.Text = "interface ethernet " + comethslot.Text + "/" + cometh.Text;
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "show configuration";
                        butguzhangsend.PerformClick();
                        textguzhangmingling.Text = "show statistics";
                        butguzhangsend.PerformClick();
                        for (int g = 0; g <= XHCount; g++)
                        {
                            if (textcurrent.Text.Contains("Ctrl+c"))
                            {
                                butguzhangsend.PerformClick();
                            }
                            else
                            {
                                Regex r = new Regex(@"In\s*Unicast\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInFrames2 = r.Match(textcurrent.Text).Groups[1].Value;
                                //MessageBox.Show(RxPower.ToString());
                                //richTextEnd.AppendText("Rx2：" + RxFrames2.ToString() + "\r\n");
                                Regex t = new Regex(@"Out\s*Unicast\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutFrames2 = t.Match(textcurrent.Text).Groups[1].Value;
                                //richTextEnd.AppendText("Tx2：" + TxFrames2.ToString() + "\r\n");
                                if (EthInFrames1 == EthInFrames2)
                                {
                                    richTextEnd.AppendText("ETH接口I n流量：NOK。客户侧无单播流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("ETH接口I n流量：OK。客户侧流量正常" + "\r\n");
                                }
                                if (EthOutFrames1 == EthOutFrames2)
                                {
                                    richTextEnd.AppendText("ETH接口Out流量：NOK。传输侧无单播流量" + "\r\n");
                                }
                                else
                                {
                                    richTextEnd.AppendText("ETH接口Out流量：OK。传输侧流量正常" + "\r\n");
                                }
                                Regex ind = new Regex(@"In\s*Discard\s*Frames\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInDiscardFrames2 = ind.Match(textcurrent.Text).Groups[1].Value;
                                Regex outd = new Regex(@"Out\s*Discard\s*Frames\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutDiscardFrames2 = outd.Match(textcurrent.Text).Groups[1].Value;
                                Regex inc = new Regex(@"In\s*CRC\s*Error\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthInCrcErrorPkts2 = inc.Match(textcurrent.Text).Groups[1].Value;
                                Regex outc = new Regex(@"Out\s*CRC\s*Error\s*Pkts\s*:([\d\,\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                EthOutCrcErrorPkts2 = outc.Match(textcurrent.Text).Groups[1].Value;
                                int EthInDiscardFrames01 = int.Parse(EthInDiscardFrames1);
                                int EthOutDiscardFrames01 = int.Parse(EthOutDiscardFrames1);
                                int EthInDiscardFrames02 = int.Parse(EthInDiscardFrames2);
                                int EthOutDiscardFrames02 = int.Parse(EthOutDiscardFrames2);
                                int EthInCrcErrorPkts01 = int.Parse(EthInCrcErrorPkts1);
                                int EthOutCrcErrorPkts01 = int.Parse(EthOutCrcErrorPkts1);
                                int EthInCrcErrorPkts02 = int.Parse(EthInCrcErrorPkts2);
                                int EthOutCrcErrorPkts02 = int.Parse(EthOutCrcErrorPkts2);
                                if (EthInDiscardFrames01 == EthInDiscardFrames02)
                                {
                                    richTextEnd.AppendText("ETH接口I n丢帧：OK。客户侧无丢帧" + "\r\n");
                                }
                                else
                                {
                                    int InDis = EthInDiscardFrames02 - EthInDiscardFrames01;
                                    richTextEnd.AppendText("ETH接口I n丢帧：NOK。客户侧16秒累计丢帧数量：" + InDis.ToString() + "\r\n");
                                }
                                if (EthOutDiscardFrames01 == EthOutDiscardFrames02)
                                {
                                    richTextEnd.AppendText("ETH接口Out丢帧：OK。传输侧无丢帧" + "\r\n");
                                }
                                else
                                {
                                    int OutDis = EthOutDiscardFrames02 - EthOutDiscardFrames01;
                                    richTextEnd.AppendText("ETH接口Out丢帧：NOK。传输侧16秒累计丢帧数量：" + OutDis.ToString() + "\r\n");
                                }
                                if (EthInCrcErrorPkts01 == EthInCrcErrorPkts02)
                                {
                                    richTextEnd.AppendText("ETH接口I n错包：OK。客户侧无CRC错包" + "\r\n");
                                }
                                else
                                {
                                    int InCRC = EthInCrcErrorPkts02 - EthInCrcErrorPkts01;
                                    richTextEnd.AppendText("ETH接口I n错包：NOK。客户侧16秒累CRC错包数量：" + InCRC.ToString() + "\r\n");
                                }
                                if (EthOutCrcErrorPkts01 == EthOutCrcErrorPkts02)
                                {
                                    richTextEnd.AppendText("ETH接口Out错包：OK。传输侧无CRC错包" + "\r\n");
                                }
                                else
                                {
                                    int OutCRC = EthOutCrcErrorPkts02 - EthOutCrcErrorPkts01;
                                    richTextEnd.AppendText("ETH接口Out错包：NOK。传输侧16秒累CRC错包数量：" + OutCRC.ToString() + "\r\n");
                                }
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                }
                butguzhangsend.PerformClick();
                ArrayList list = getIndexArray(richTextEnd.Text, "NOK");
                for (int i = 0; i < list.Count; i++)
                {
                    int index = (int)list[i];
                    richTextEnd.Select(index, "NOK".Length);
                    richTextEnd.SelectionColor = Color.Red;
                }
                if (richTextEnd.Text.Contains("NOK"))
                {
                    richTextEnd.AppendText("排查结果：存在故障，请排查NOK的项目，如果告警项目存在NOK，请环回所有时隙后，再次点击排查故障，确认是否为我司问题" + "\r\n");
                    MessageBox.Show("排查结果：存在故障，请排查NOK项！" + "\r\n" + "如果告警项目存在NOK，请环回所有时隙后，再次点击排查故障，确认是否为我司问题");
                }
                else
                {
                    richTextEnd.AppendText("排查结果：查无故障！" + "\r\n");
                    MessageBox.Show("排查结果：查无故障！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private ArrayList getIndexArray(String inputStr, String findStr)
        {
            ArrayList list = new ArrayList();
            int start = 0;
            while (start < inputStr.Length)
            {
                int index = inputStr.IndexOf(findStr, start);
                if (index >= 0)
                {
                    list.Add(index);
                    start = index + findStr.Length;
                }
                else
                {
                    break;
                }
            }
            return list;
        }
        private void Butsyslog_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(Syslog);
            t.Start();
        }
        private void Butpaigu_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(GuZhangPaiCha);
            t.Start();
        }
        #region 故障排查按钮
        private void Butguzhangsend_Click(object sender, EventArgs e)
        {
            if (mysocket.SendData(textguzhangmingling.Text))
            {
                if (textguzhangmingling.Text == "")
                {
                    Thread.Sleep(XHTime / 4);
                    string ctrlc = "Press any key to continue Ctrl+c to stop";
                    string DOS = textcurrent.Text;
                    if (DOS.Contains(ctrlc))
                    {
                        textcurrent.Text = DOS.Replace(ctrlc, "");
                        //MessageBox.Show("检测到了");
                    }
                    string str = "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                    textcurrent.AppendText(str);
                }
                else
                {
                    com = textguzhangmingling.Text;
                    Thread.Sleep(XHTime / 4);
                    string ss = mysocket.ReceiveData(int.Parse(ts));
                    textcurrent.AppendText(ss);
                }
            }
            else
            {
                textcurrent.AppendText("\r\n" + "连接通信故障，请断开后，重新尝试！");

            }
            textguzhangmingling.Text = "";
            textcurrent.Focus();
            textcurrent.ScrollToCaret();
            textguzhangmingling.Focus();
        }
        #endregion
        #region Tab选项卡切换
        private void TabControlDOS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlDOS.SelectedTab == tabPageGpn)
            {
                labelethboard.Visible = false;
                labelethslot.Visible = false;
                comethboard.Visible = false;
                comethslot.Visible = false;
                labelboard.Visible = false;
                labelslot.Visible = false;
                labelvcg.Visible = false;
                labeleth.Visible = false;
                cometh.Visible = false;
                comboard.Visible = false;
                comslot.Visible = false;
                comvcg.Visible = false;
                butpaigu.Visible = false;
                butsyslog.Visible = false;
                butbatch.Visible = true;
                butupgrade.Visible = true;
                textcyclemingling.Visible = false;
                labcishu.Visible = false;
                comcishu.Visible = false;
                butCycleStart.Visible = false;
                butCycleSuspend.Visible = false;
            }
            if (tabControlDOS.SelectedTab == tabPageLog)
            {
                labelethboard.Visible = true;
                labelethslot.Visible = true;
                comethboard.Visible = true;
                comethslot.Visible = true;
                labelboard.Visible = true;
                labelslot.Visible = true;
                labelvcg.Visible = true;
                labeleth.Visible = true;
                cometh.Visible = true;
                comboard.Visible = true;
                comslot.Visible = true;
                comvcg.Visible = true;
                butpaigu.Visible = true;
                butsyslog.Visible = true;
                butbatch.Visible = false;
                butupgrade.Visible = false;
                textcyclemingling.Visible = true;
                labcishu.Visible = true;
                comcishu.Visible = true;
                butCycleStart.Visible = true;
                butCycleSuspend.Visible = true;
            }
        }
        #endregion
        #region 故障排查按钮向上按钮指令
        private void Textguzhangmingling_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                butguzhangsend.PerformClick();
            }
            if (e.KeyCode == Keys.Up)
            {
                butguzhangsend.Focus();
                textguzhangmingling.Text = com.ToString();
            }
            if (e.KeyCode == Keys.Down)
            {
                butguzhangsend.Focus();
                textguzhangmingling.Text = "";
            }
        }
        #endregion
        #region 排查结果
        private void RichTextEnd_TextChanged(object sender, EventArgs e)
        {
            //将光标位置设置到当前内容的末尾
            richTextEnd.SelectionStart = richTextEnd.Text.Length;
            //滚动到光标位置
            richTextEnd.ScrollToCaret();
        }
        #endregion
        #region 循环执行命令
        bool stop = false;
        bool on_off = false;
        ManualResetEvent ma;
        Thread CycleThread;
        private void Cycle()
        {
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            // string netid = TextNeid.Text;
            for (int i = 1; i <= int.Parse(comcishu.Text); i++)
            {
                int zong = int.Parse(comcishu.Text);
                int shengyu = zong - i;
                labshengyucishu.Text = shengyu.ToString();
                //string netid = "3911065600";
                textcurrent.AppendText("\r\n循环第" + i.ToString() + "次准备开始！" + "\r\n");
                if (stop)
                {
                    textcurrent.AppendText("\r\n已经停止！");
                    return;
                }
                if (on_off)
                {
                    textcurrent.AppendText("暂停中！\r\n");
                    ma = new ManualResetEvent(false);
                    ma.WaitOne();
                }

                //LinkGpn();
                //textcurrent.AppendText("//////////////////复位5槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 5";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //Thread.Sleep(120000);
                //textcurrent.AppendText("//////////////////复位6槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 6";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //Thread.Sleep(120000);
                //textcurrent.AppendText("//////////////////复位11槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 11";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //Thread.Sleep(500000);
                //textcurrent.AppendText("//////////////////复位12槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 12";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //Thread.Sleep(500000);
                //textcurrent.AppendText("//////////////////复位17槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 17";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //mysocket.Close();
                //Thread.Sleep(1200000);

                //LinkGpn();
                //textcurrent.AppendText("//////////////////复位18槽位后检查oduk");
                //textguzhangmingling.Text = "config otn";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "show oduk";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "exit";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "reboot 18";
                //butguzhangsend.PerformClick();
                //Thread.Sleep(300);
                //textguzhangmingling.Text = "y";
                //butguzhangsend.PerformClick();
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //mysocket.Close();
                //Thread.Sleep(1200000);







                //LinkGpn();
                //textcurrent.AppendText("//////////////////Telnet登录后开始检查NEID变化");
                //textguzhangmingling.Text = textcyclemingling.Text;
                //butguzhangsend.PerformClick();
                //Thread.Sleep(500);
                //for (int v = 1; v < 10; v++)
                //{
                //    if (textcurrent.Text.Contains("Current"))
                //    {
                //        if (!textcurrent.Text.Contains("Current Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        if (!textcurrent.Text.Contains("Local   Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        break;
                //    }
                //    butguzhangsend.PerformClick();
                //    Thread.Sleep(XHTime);
                //}
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //App();
                //textcurrent.AppendText("//////////////////APP下载写入陈工后开始检查NEID变化");
                //textguzhangmingling.Text = textcyclemingling.Text;
                //butguzhangsend.PerformClick();
                //Thread.Sleep(500);
                //for (int v = 1; v < 10; v++)
                //{
                //    if (textcurrent.Text.Contains("Current"))
                //    {
                //        if (!textcurrent.Text.Contains("Current Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        if (!textcurrent.Text.Contains("Local   Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        break;
                //    }
                //    butguzhangsend.PerformClick();
                //    Thread.Sleep(XHTime);
                //}
                //textlog.AppendText(textcurrent.Text);
                //textcurrent.Text = "";
                //UploadNetid();
                //textcurrent.AppendText("//////////////////上传NEID文件后开始检查NEID变化");
                ////UploadConfig();
                //textguzhangmingling.Text = textcyclemingling.Text;
                //butguzhangsend.PerformClick();
                //Thread.Sleep(500);
                //for (int v = 1; v < 10; v++)
                //{
                //    if (textcurrent.Text.Contains("Current"))
                //    {
                //        if (!textcurrent.Text.Contains("Current Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        if (!textcurrent.Text.Contains("Local   Netid: " + netid))
                //        {
                //            textcurrent.AppendText("NEID与检查不一致，已停止");
                //            on_off = true;
                //            butCycleSuspend.Text = "继续";
                //            return;
                //        }
                //        break;
                //    }
                //    butguzhangsend.PerformClick();
                //    Thread.Sleep(XHTime);
                //}
                ////mysocket.SendData("reboot");
                ////Thread.Sleep(300);
                ////mysocket.SendData("Y");
                //butlogin.Text = "①连接设备";
                //mysocket.Close();

                //for (int g = 0; g <= 100; g++)
                //{
                //    if (textcurrent.Text.Contains("Ctrl+c"))
                //    {
                //        butguzhangsend.PerformClick();
                //    }
                //    else
                //    {
                //        break;
                //    }
                //    //Thread.Sleep(XHTime/10);
                //}
                textguzhangmingling.Text = textcyclemingling.Text;
                butguzhangsend.PerformClick();
                textlog.AppendText(textcurrent.Text);
                textcurrent.Text = "";
                int time = int.Parse(comshijian.Text.Trim()) * 1000;
                Thread.Sleep(time);
            }
            Mytimer.Change(Timeout.Infinite, 1000);
            butCycleStart.Text = "开始";
            textcurrent.AppendText("\r\n循环执行结束！");
        }
        private void ButCycleSuspend_Click(object sender, EventArgs e)
        {
            if (butCycleSuspend.Text == "暂停")
            {
                on_off = true;
                butCycleSuspend.Text = "继续";
            }
            else
            {
                on_off = false;
                if (ma != null)
                {
                    ma.Set();
                }
                butCycleSuspend.Text = "暂停";
                textcurrent.AppendText("\r\n恢复运行！");
            }
        }
        private void ButCycleStart_Click(object sender, EventArgs e)
        {
            if (butCycleStart.Text == "开始")
            {
                stop = false;
                CycleThread = new Thread(Cycle)
                {
                    IsBackground = true
                };
                CycleThread.Start();
                butCycleStart.Text = "停止";
                textcurrent.AppendText("\r\n开始运行！");
            }
            else
            {
                stop = true;
                butCycleStart.Text = "开始";
                Mytimer.Change(Timeout.Infinite, 1000);
            }
        }
        #endregion
        #region 一键改制
        private void butgaizhi_Click(object sender, EventArgs e)
        {
            textcurrent.AppendText("\r\n" + "///////////////////////////改制开始/////////////////////////////////////////////" + "\r\n");
            textguzhangmingling.Text = "grosadvdebug";
            butguzhangsend.PerformClick();
            string mode = "";
            if (comotnboardmode.Text == "8AT2")
            {
                mode = "0";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "8AST2")
            {
                mode = "3";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "2XT2")
            {
                mode = "4";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "2GT1")
            {
                mode = "5";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "T10X")
            {
                mode = "6";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "8AT2-SDH")
            {
                mode = "8";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " ext-info2 mode=" + mode;
            }
            if (comotnboardmode.Text == "V2-2XT2（8AT2改制）")
            {
                mode = "GPN7600-V2-2XT2";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " type-redefine " + mode;
            }
            if (comotnboardmode.Text == "V2-8AT2（8GE改制）")
            {
                mode = "GPN7600-V2-8AT2";
                textguzhangmingling.Text = "board-eeprom " + comotnslot.Text + " type-redefine " + mode;
            }
            butguzhangsend.PerformClick();
            Thread.Sleep(8000);
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "exit";
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "enable show boardname by e2-extinfo";
            butguzhangsend.PerformClick();
            Thread.Sleep(XHTime);
            textguzhangmingling.Text = "reboot " + comotnslot.Text;
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "Y";
            butguzhangsend.PerformClick();
            textcurrent.AppendText("\r\n" + "///////////////////////////改制结束/////////////////////////////////////////////" + "\r\n");
            MessageBox.Show("GPN800需要重启清空设备后生效，GPN7600上载后可自动识别，show 不准确");
        }
        #endregion
        #region 卸载GPN7600EMS模块
        private void butuninstall_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("卸载前请确认已经退出网管服务器？", "提示", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {

                //户选择确认的操作
                if (CheckGPN7600EMS() == false)
                {
                    MessageBox.Show("GPN76模块已卸载，请安装后进行卸载！");
                    return;
                }

                Process p = new Process();
                p.StartInfo.FileName = @"C:\Program Files (x86)\InstallShield Installation Information\{F54A1417-6804-4C74-8B36-C44592EDFEF2}\setup.exe";
                //p.StartInfo.Arguments = " -runfromtemp -l0x0409  -removeonly";
                p.Start();
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块已卸载==============================================OK" + "\r\n");

            }
            if (dr == DialogResult.No)
            {
                //户选择取消的操作
                return;
            }

        }
        #endregion
        #region 检查GPN7600EMS软件是否存在
        private bool CheckGPN7600EMS()
        {
            RegistryKey uninstallNode = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            foreach (string subKeyName in uninstallNode.GetSubKeyNames())
            {
                Microsoft.Win32.RegistryKey subKey = uninstallNode.OpenSubKey(subKeyName);
                object displayName = subKey.GetValue("DisplayName");
                if (displayName != null)
                {
                    if (displayName.ToString().Contains("GPN7600 EMS"))
                    {
                        return true;
                        // MessageBox.Show(displayName.ToString());  
                    }
                }
            }
            return false;
        }
        #endregion
        #region 安装GPN7600EMS模块
        private void butinstall_Click(object sender, EventArgs e)
        {
            if (CheckGPN7600EMS() == true)
            {
                MessageBox.Show("GPN7600 EMS已安装，请卸载后进行安装！");
                return;
            }
            if (comgpn76list.Text.Trim() == "")
            {
                MessageBox.Show("请获取GPN模块后，再次点击尝试！");
                return;
            }
            Thread installgpnems = new Thread(InstallGPN)
            {
                IsBackground = true
            };
            installgpnems.Start();
        }
        #endregion
        private void InstallGPN()
        {
            FileStream stream;
            string gpnname = comgpn76list.Text;
            string url = GPN7600EMSURL + gpnname;
            string strZipPath = @"C:\gpn\" + gpnname;
            string strUnZipPath = @"C:\gpn\";
            int percent = 0;
            stream = new FileStream(strZipPath, FileMode.Create);
            bool overWrite = true;
            try
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块下载中==============================================OK" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块下载成功============================================OK" + "\r\n");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块保存路径：" + strZipPath + "\r\n");

            }
            catch (Exception)
            {
                stream.Close();
                MessageBox.Show("无法进行下载，请检查下载链接！");
                //flag = false;       //返回false下载失败
            }
            try
            {
                ReadOptions options = new ReadOptions { StatusMessageWriter = System.Console.Out };
                using (ZipFile zip = ZipFile.Read(strZipPath, options))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        if (string.IsNullOrEmpty(strUnZipPath))
                        {
                            strUnZipPath = strZipPath.Split('.').First();
                        }
                        if (overWrite)
                        {
                            entry.Extract(strUnZipPath, ExtractExistingFileAction.OverwriteSilently);//解压文件，如果已存在就覆盖
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块解压成功============================================OK" + "\r\n");
                        }
                    }
                    // zip.ExtractAll(@tbxFtpRoot.Text.Trim());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("无法进行解压，请检查下载链接！");
                return;
            }
            System.Diagnostics.Process.Start(strUnZipPath + "setup.exe");
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块安装成功============================================OK" + "\r\n");
            // MessageBox.Show("GPN7600 EMS模块已安装成功！");
        }
        #region 获取GPN7600EMS软件目录
        private void gpnurlupdate()
        {
            try
            {
                string strCode;
                ArrayList alLinks;
                if (GPN7600EMSURL == "")
                {
                    MessageBox.Show("请输入网址");
                    return;
                }
                string strURL = GPN7600EMSURL;
                if (strURL.Substring(0, 7) != @"http://")
                {
                    //strURL = @"http://" + strURL;
                }

                Ping ping = new Ping();
                int timeout = 120;
                PingReply pingReply = ping.Send(GPN7600EMSURLIP, timeout);
                //判断请求是否超时

                //textDOS.AppendText("正在获取页面代码===========================================OK" + "\r\n");
                strCode = MyGpnSoftware.App.GetPageSource(strURL);
                //textDOS.AppendText("正在提取超链接=============================================OK" + "\r\n");
                alLinks = MyGpnSoftware.App.GetHyperLinks(strCode);
                //textDOS.AppendText("正在写入XML文件============================================OK" + "\r\n");
                MyGpnSoftware.App.WriteToXml(strURL, alLinks);
                //读取设定档百
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(@"C:\gpn\HyperLinks.xml");
                //取得节点专
                XmlNodeList node = xmlDoc.GetElementsByTagName("other");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "获取网管版本" + "===============================================OK " + node.Count.ToString() + "个版本" + "\r\n");
                for (int i = 0; i < node.Count; i++)
                {
                    comgpn76list.Items.Add(node[i].InnerText);
                }
                //textDOS.AppendText("从网管服务器获取GPN76模块链接成功==========================OK" + "\r\n");
            }
            catch (Exception ex)
            {
                //   textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "GPN模块获取失败，请链接格林威尔VPN后，再次尝试！" + "\r\n");

            }

        }
        #endregion
        #region 获取各个文件的大小
        private void comapp_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comapp.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labapp.Text = lSize.ToString();
            }
        }
        private void comflash_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comflash.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labflash.Text = lSize.ToString();
            }
        }
        private void comcode_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comcode.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labcode.Text = lSize.ToString();
            }
        }
        private void comnms_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comnms.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labnms.Text = lSize.ToString();
            }
        }
        private void comsw_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comsw.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labsw.Text = lSize.ToString();
            }
        }
        private void com760s_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760s.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760s.Text = lSize.ToString();
            }
        }
        private void com760b_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760b.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760b.Text = lSize.ToString();
            }
        }
        private void com760c_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760c.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760c.Text = lSize.ToString();
            }
        }
        private void com760d_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760d.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760d.Text = lSize.ToString();
            }
        }
        private void com760e_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760e.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760e.Text = lSize.ToString();
            }
        }
        private void com760f_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760f.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760f.Text = lSize.ToString();
            }
        }
        private void comsysfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comsysfile.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labsysfile.Text = lSize.ToString();
            }
        }
        private void comyaffs_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comyaffs.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labyaffs.Text = lSize.ToString();
            }
        }
        private void comconfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comconfig.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labconfig.Text = lSize.ToString();
            }
        }
        private void comslotconfig_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comslotconfig.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labslotconfig.Text = lSize.ToString();
            }
        }
        private void comdb_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comdb.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labdb.Text = lSize.ToString();
            }
        }
        #endregion

        #region 检查flash文件对比大小
        private void ConfigSize()
        {
            toolStripStatusLabelzt.Text = "检查Config大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*conf_data.txt";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查conf_data.txt文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labconfig.Text)
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查conf_data.txt文件比对==============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：conf_data.txt文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：conf_data.txt文件大小为： " + labconfig.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查conf_data.txt文件比对=============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：conf_data.txt文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：conf_data.txt文件大小为： " + labconfig.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void SlotconfigSize()
        {
            toolStripStatusLabelzt.Text = "检查Slotconfig大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*slotconfig.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查slotconfig.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labslotconfig.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查slotconfig.bin文件比对=============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：slotconfig.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：slotconfig.bin文件大小为： " + labslotconfig.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查slotconfig.bin文件比对============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：slotconfig.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：slotconfig.bin文件大小为： " + labslotconfig.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void DbSize()
        {
            toolStripStatusLabelzt.Text = "检查Db大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*db.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查db.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labdb.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查db.bin文件比对=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：db.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：db.bin文件大小为： " + labdb.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查db.bin文件比对====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：db.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：db.bin文件大小为： " + labdb.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void AppSize()
        {
            toolStripStatusLabelzt.Text = "检查APP大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*app_code.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查app_code.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labapp.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查app_code.bin文件比对===============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：app_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：app_code.bin文件大小为： " + labapp.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查app_code.bin文件比对==============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：app_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：app_code.bin文件大小为： " + labapp.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void NmsSize()
        {
            toolStripStatusLabelzt.Text = "检查NMS大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            // MessageBox.Show(ver);
            string appRegex = ".* nms.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查nms.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labnms.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查nms.fpga文件比对===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：nms.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：nms.fpga文件大小为： " + labnms.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查nms.fpga文件比对==================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：nms.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：nms.fpga文件大小为： " + labnms.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void SwSize()
        {
            toolStripStatusLabelzt.Text = "检查SW大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*sw.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sw.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labsw.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sw.fpga文件比对====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：sw.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：sw.fpga文件大小为： " + labsw.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sw.fpga文件比对===================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：sw.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：sw.fpga文件大小为： " + labsw.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void CodeSize()
        {
            toolStripStatusLabelzt.Text = "检查CODE大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            if (toolStripStatusLabelver.Text.Contains("R13"))
            {
                mysocket.SendData("cd /flash/sys");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("cd /flash/sys"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
            }
            else
            {
                mysocket.SendData("cd /yaffs/sys");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("cd /yaffs/sys"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*fpga_code.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查fpga_code.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labcode.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查fpga_code.bin文件比对==============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：fpga_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：fpga_code.bin文件大小为： " + labcode.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查fpga_code.bin文件比对=============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：fpga_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：fpga_code.bin文件大小为： " + labcode.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760sSize()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760S大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760s.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760s.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760s.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760s.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760s.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760s.fpga文件大小为： " + lab760s.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760s.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760s.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760s.fpga文件大小为： " + lab760s.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760bSize()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760B大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760b.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760b.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760b.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760b.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760b.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760b.fpga文件大小为： " + lab760b.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760b.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760b.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760b.fpga文件大小为： " + lab760b.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760cSize()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760C大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760c.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760c.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760c.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760c.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760c.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760c.fpga文件大小为： " + lab760c.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760c.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760c.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760c.fpga文件大小为： " + lab760c.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760dSIze()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760D大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760d.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760d.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760d.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760d.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760d.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760d.fpga文件大小为： " + lab760d.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760d.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760d.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760d.fpga文件大小为： " + lab760d.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760eSize()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760E大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760e.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760e.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760e.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760e.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760e.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760e.fpga文件大小为： " + lab760e.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760e.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760e.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760e.fpga文件大小为： " + lab760e.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void Fpga760fSize()
        {
            toolStripStatusLabelzt.Text = "检查760F大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /yaffs/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /yaffs/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*760f.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760f.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == lab760f.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760f.fpga文件比对===============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760f.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760f.fpga文件大小为： " + lab760f.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查760f.fpga文件比对==============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：760f.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：760f.fpga文件大小为： " + lab760f.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        private void SysfileSize()
        {
            toolStripStatusLabelzt.Text = "检查sysfile大小中";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string aaaa = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            string bbbb = mysocket.ReceiveData(int.Parse(ts));
            mysocket.SendData("ll");
            Thread.Sleep(XHTime);
            string ver = "";
            string ver2 = "";
            for (int a = 0; a <= 5; a++)
            {
                ver2 = mysocket.ReceiveData(int.Parse(ts));
                ver = ver + ver2;
                if (ver2.Contains("Ctrl+c"))
                {
                    mysocket.SendDate("\r\n");
                }
                if (ver2.Contains("#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            //MessageBox.Show(ver);
            string appRegex = ".*sysfile_ini.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sysfile_ini.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 200; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(appsize1, "\\s+", RegexOptions.IgnoreCase);
            string Appsize = VCGINFOFengGe[4];
            if (UpLoadFile_Stop == false)
            {
                Filesize = long.Parse(Appsize);
            }
            else
            {
                if (Appsize == labsysfile.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sysfile_ini.bin文件比对============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：sysfile_ini.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：sysfile_ini.bin文件大小为： " + labsysfile.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "检查sysfile_ini.bin文件比对===========================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "设备：sysfile_ini.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "电脑：sysfile_ini.bin文件大小为： " + labsysfile.Text + " 字节" + "\r\n");
                }
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 200; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("(config)#"))
                {
                    break;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            // MessageBox.Show(Appsize);
        }
        #endregion      
        private void butslectfile_Click(object sender, EventArgs e)
        {
            if (checkapp.Checked == false &&
                checkcode.Checked == false &&
                checknms.Checked == false &&
                checksw.Checked == false &&
                check760s.Checked == false &&
                check760b.Checked == false &&
                check760c.Checked == false &&
                check760d.Checked == false &&
                check760e.Checked == false &&
                check760f.Checked == false &&
                checksysfile.Checked == false &&
                checkconfig.Checked == false &&
                checkdb.Checked == false &&
                checkslotconfig.Checked == false)
            {
                MessageBox.Show("请勾选文件后进行比较！");
                return;
            }
            Thread checkfile = new Thread(CheckFile);
            checkfile.Start();

        }
        private void CheckFile()
        {

            try
            {
                if (devtype == "98" || devtype == "2859" || devtype == "2860" || devtype == "2861" || devtype == "2862" || devtype == "2863" || devtype == "2864" || devtype == "2865")
                {

                    textDOS.AppendText("\r\n");
                    if (checkconfig.Checked == true)
                    {
                        ConfigSize();
                    }
                    if (checkslotconfig.Checked == true)
                    {
                        SlotconfigSize();
                    }
                    if (checkdb.Checked == true)
                    {
                        DbSize();
                    }
                    if (checkapp.Checked == true)
                    {
                        AppSize();
                    }
                    if (checkcode.Checked == true)
                    {
                        CodeSize();
                    }
                    if (checknms.Checked == true)
                    {
                        NmsSize();
                    }
                    if (checksw.Checked == true)
                    {
                        SwSize();
                    }
                    if (check760s.Checked == true)
                    {
                        Fpga760sSize();
                    }
                    if (check760b.Checked == true)
                    {
                        Fpga760bSize();
                    }
                    if (check760c.Checked == true)
                    {
                        Fpga760cSize();
                    }
                    if (check760d.Checked == true)
                    {
                        Fpga760dSIze();
                    }
                    if (check760e.Checked == true)
                    {
                        Fpga760eSize();
                    }
                    if (check760f.Checked == true)
                    {
                        Fpga760fSize();
                    }
                    if (checksysfile.Checked == true)
                    {
                        SysfileSize();
                    }


                }
                else
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "该设备不支持检查flash文件大小进行比对！");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // butsend.PerformClick();
                toolStripStatusLabelzt.Text = "已完成";
            }
        }
        bool UpLoadFile_Stop = true;
        bool UpLoadFile_On_Off = false;
        ManualResetEvent UpLoadFilePause;
        Thread UpLoadFileThread;
        private void butupload_Click(object sender, EventArgs e)
        {
            if (butupload.Text == "上传备份")
            {

                if (FtpPortEnable = false || FtpStatusEnable == false)
                {
                    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                    return;

                }
                if (checkapp.Checked == false &&
checkcode.Checked == false &&
checknms.Checked == false &&
checksw.Checked == false &&
check760s.Checked == false &&
check760b.Checked == false &&
check760c.Checked == false &&
check760d.Checked == false &&
check760f.Checked == false &&
checksysfile.Checked == false &&
checkflash.Checked == false &&
checkyaffs.Checked == false &&
checkconfig.Checked == false &&
checkdb.Checked == false &&
checkslotconfig.Checked == false &&
check760e.Checked == false)
                {
                    MessageBox.Show("请勾选文件后继续！");
                    return;
                }

                UpLoadFile_Stop = false;
                UpLoadFileThread = new Thread(UpLoadFile)
                {
                    IsBackground = true
                };
                UpLoadFileThread.Start();
                butupload.Text = "停止备份";
                //textcurrent.AppendText("\r\n开始运行！");

            }
            else
            {
                UpLoadFile_Stop = true;
                butupload.Text = "上传备份";
            }
        }
        private void UpLoadFile()
        {
            //立即开始计时，时间间隔1000毫秒
            try
            {
                if (devtype == "98" || devtype == "2859" || devtype == "2860" || devtype == "2861" || devtype == "2862" || devtype == "2863" || devtype == "2864" || devtype == "2865")
                {



                    TimeCount = 0;
                    Mytimer.Change(0, 1000);
                    Control.CheckForIllegalCrossThreadCalls = false;
                    uploading = true;
                    Testftpser();
                    if (UpLoadFile_Stop)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("\r\n" + "yyyy-MM-dd HH:mm:ss") + " " + "上传备份已停止！");
                        return;
                    }
                    Uploadsave();
                    int a = 0;
                    int p = 0;
                    if (checkapp.Checked == true)
                    {
                        a++;
                    }
                    if (checkcode.Checked == true)
                    {
                        a++;
                    }
                    if (checknms.Checked == true)
                    {
                        a++;
                    }
                    if (checksw.Checked == true)
                    {
                        a++;
                    }
                    if (check760s.Checked == true)
                    {
                        a++;
                    }
                    if (check760b.Checked == true)
                    {
                        a++;
                    }
                    if (check760c.Checked == true)
                    {
                        a++;
                    }
                    if (check760d.Checked == true)
                    {
                        a++;
                    }
                    if (check760e.Checked == true)
                    {
                        a++;
                    }
                    if (check760f.Checked == true)
                    {
                        a++;
                    }
                    if (checksysfile.Checked == true)
                    {
                        a++;
                    }
                    if (checkflash.Checked == true)
                    {
                        a++;
                    }
                    if (checkyaffs.Checked == true)
                    {
                        a++;
                    }
                    if (checkconfig.Checked == true)
                    {
                        a++;
                    }
                    if (checkdb.Checked == true)
                    {
                        a++;
                    }
                    if (checkslotconfig.Checked == true)
                    {
                        a++;
                    }
                    int s = (int)Math.Floor((double)100 / a);
                    p = (int)Math.Floor((double)100 / a);
                    if (checkconfig.Checked == true)
                    {
                        ConfigSize();
                        UploadConfig();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkslotconfig.Checked == true)
                    {
                        SlotconfigSize();
                        UploadSlotConfig();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkdb.Checked == true)
                    {
                        DbSize();
                        UploadDb();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkapp.Checked == true)
                    {
                        AppSize();
                        UploadApp();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkcode.Checked == true)
                    {
                        CodeSize();
                        UploadCode();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checknms.Checked == true)
                    {
                        NmsSize();
                        UploadNms();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checksw.Checked == true)
                    {
                        SwSize();
                        UploadSw();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760s.Checked == true)
                    {
                        Fpga760sSize();
                        Upload760s();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760b.Checked == true)
                    {
                        Fpga760bSize();
                        Upload760b();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760c.Checked == true)
                    {
                        Fpga760cSize();
                        Upload760c();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760d.Checked == true)
                    {
                        Fpga760dSIze();
                        Upload760d();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760e.Checked == true)
                    {
                        Fpga760eSize();
                        Upload760e();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (check760f.Checked == true)
                    {
                        Fpga760fSize();
                        Upload760f();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checksysfile.Checked == true)
                    {
                        SysfileSize();
                        UploadSysfile();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkflash.Checked == true)
                    {
                        Filesize = 33554432;
                        UploadFlash();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    if (checkyaffs.Checked == true)
                    {
                        Filesize = 553648128;
                        UploadYaffs();
                        if (s == p)
                        {
                            if (UpLoadFile_Stop)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                return;
                            }
                            if (UpLoadFile_On_Off)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                UpLoadFilePause = new ManualResetEvent(false);
                                UpLoadFilePause.WaitOne();
                            }
                            myProgressBarjindu.Value = p;
                            //toolStripStatusLabelbar.Text = p + "%";
                            System.Threading.Thread.Sleep(XHTime);
                            p = s + p;
                        }
                        else
                        {
                            if (p > 95 && p <= 100)
                            {
                                p = 100;
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                            }
                            else
                            {
                                if (UpLoadFile_Stop)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "已停止！\r\n");
                                    return;
                                }
                                if (UpLoadFile_On_Off)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "暂停中！\r\n");
                                    UpLoadFilePause = new ManualResetEvent(false);
                                    UpLoadFilePause.WaitOne();
                                }
                                myProgressBarjindu.Value = p;
                                //toolStripStatusLabelbar.Text = p + "%";
                                System.Threading.Thread.Sleep(XHTime);
                                p = s + p;
                            }
                        }
                    }
                    Thread.Sleep(XHTime);
                    string canyu = mysocket.ReceiveData(int.Parse(ts));
                    toolStripStatusLabelzt.Text = "已完成";
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份结束" + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    butsend.PerformClick();
                    UpLoadFile_Stop = true;
                    butupload.Text = "上传备份";
                    Mytimer.Change(Timeout.Infinite, 1000);
                }
                else
                {
                    textDOS.AppendText("\r\n" + "该设备不支持文件上传，请使用其方式！");
                    return;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void UploadConfig()
        {
            toolStripStatusLabelzt.Text = "正在备份config文件";             //上传状态栏显示
            string strname = labconfigname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 1000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/conf_data.txt ";      //上传文件名
            string uploadfilenamesave = "config.txt";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadSlotConfig()
        {
            toolStripStatusLabelzt.Text = "正在备份slotconfig文件";             //上传状态栏显示
            string strname = labslotname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/slotconfig.bin ";      //上传文件名
            string uploadfilenamesave = "slotconfig.bin";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string failed = "Upload file ...failed";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(failed))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================上传config文件失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadDb()
        {
            toolStripStatusLabelzt.Text = "正在备份db文件";             //上传状态栏显示
            string strname = labdbname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/db.bin ";      //上传文件名
            string uploadfilenamesave = "db.bin";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "============================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadApp()
        {
            toolStripStatusLabelzt.Text = "正在备份App文件";             //上传状态栏显示
            string strname = labappname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " app ";      //上传文件名
            string uploadfilenamesave = version + ".bin gpn";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadCode()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga_code文件";             //上传状态栏显示
            string strname = labcodename.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/fpga_code.bin ";      //上传文件名
            string uploadfilename2 = " file /flash/sys/fpga_code.bin ";      //上传文件名
            string uploadfilenamesave = "fpga_code.bin";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            if (toolStripStatusLabelver.Text.Contains("R13"))
            {
                mysocket.SendData("upload ftp" + uploadfilename2 + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            }
            else
            {
                mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            }
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760s()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760s文件";             //上传状态栏显示
            string strname = lab760sname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760s.fpga ";      //上传文件名
            string uploadfilenamesave = "760s.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760b()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760b文件";             //上传状态栏显示
            string strname = lab760bname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760b.fpga ";      //上传文件名
            string uploadfilenamesave = "760b.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760c()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760c文件";             //上传状态栏显示
            string strname = lab760cname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760c.fpga ";      //上传文件名
            string uploadfilenamesave = "760c.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760d()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760d文件";             //上传状态栏显示
            string strname = lab760dname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760d.fpga ";      //上传文件名
            string uploadfilenamesave = "760d.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760e()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760e文件";             //上传状态栏显示
            string strname = lab760ename.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760e.fpga ";      //上传文件名
            string uploadfilenamesave = "760e.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760f()
        {
            toolStripStatusLabelzt.Text = "正在备份760F文件";             //上传状态栏显示
            string strname = lab760fname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760f.fpga ";      //上传文件名
            string uploadfilenamesave = "760f.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadNms()
        {
            toolStripStatusLabelzt.Text = "正在备份Nms文件";             //上传状态栏显示
            string strname = labnmsname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/nms.fpga ";      //上传文件名
            string uploadfilenamesave = "nms.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "===========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "============================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadSw()
        {
            toolStripStatusLabelzt.Text = "正在备份Sw文件";             //上传状态栏显示
            string strname = labswname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/sw.fpga ";      //上传文件名
            string uploadfilenamesave = "sw.fpga";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 1; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadSysfile()
        {
            toolStripStatusLabelzt.Text = "正在备份Sysfile文件";             //上传状态栏显示
            string strname = labsysfilename.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 1000;                                        //循环次数           
            string uploadfilename = " sysfile ";                            //上传文件名
            string uploadfilenamesave = "sysfile_ini.bin";                       //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + uploadfilenamesave);
            for (int i = 0; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string uploadfile = "file ...failed";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(uploadfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "================================文件上传失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadNetid()
        {
            toolStripStatusLabelzt.Text = "正在备份Netid文件";             //上传状态栏显示
            string strname = "Netid";                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 1000;                                        //循环次数           
            string uploadfilename = " file /flash/sys/netid.txt ";                            //上传文件名
            string uploadfilenamesave = "netid.txt";
            bool okstart = false;
            //上传后命名
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp" + uploadfilename + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + uploadfilenamesave);
            for (int i = 0; i <= xunhuancishu; i++)
            {
                string ok = "ok";
                string serveron = "Fail to connect to server";
                string user = "User need password";
                string foundfile = "The specified file does not exist";
                string uploadfile = "file ...failed";
                string end = "Release ftp operational popedom!";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok) || okstart == true)
                {
                    okstart = true;
                    if (box.Contains(end))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(uploadfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "备份" + strname + "================================文件上传失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        #region 上传 Flash
        private void UploadFlash()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在上传Flash";
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Flash进度===========================================O%");
            mysocket.SendData("upload ftp flash " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_flash.bin gpn");
            for (int i = 1; i <= 10000; i++)
            {
                string ok = "100%";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains("%"))
                {
                    String str; //字符复制串变量
                    str = textDOS.Text; //获取文本百框中的文本赋与字符串变量
                                        //提取度去除最后一问个字符的子字答符串(参数：0(从零Index处开始)，str.Lenght-1(提取几个字符))
                    str = str.Substring(0, str.Length - 4);
                    textDOS.Text = str; //赋回已删除最后一个字符的字符串给textBox
                    textDOS.AppendText(box);
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Flash ==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Flash=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Flash=======================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 上传 Yaffs
        private void UploadYaffs()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在上传Yaffs";
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Yaffs文件大小约有528MB,大约需要20分钟，请耐心等待！" + toolStripStatusLabeltime.Text + "\r\n");
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Yaffs进度===========================================O%");
            mysocket.SendData("upload ftp yaffs " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_yaffs.bin");
            for (int i = 1; i <= 100000; i++)
            {
                string ok = "100%";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains("%"))
                {
                    String str; //字符复制串变量
                    str = textDOS.Text; //获取文本百框中的文本赋与字符串变量
                                        //提取度去除最后一问个字符的子字答符串(参数：0(从零Index处开始)，str.Lenght-1(提取几个字符))
                    str = str.Substring(0, str.Length - 4);
                    textDOS.Text = str; //赋回已删除最后一个字符的字符串给textBox
                    textDOS.AppendText(box);
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Yaffs=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "上传Yaffs=======================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        private void Uploadsave()
        {
            toolStripStatusLabelzt.Text = "正在保存配置";
            mysocket.SendData("save");
            for (int i = 1; i <= 20; i++)
            {
                string save = "successfully";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(save))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存配置===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains("erro"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "保存配置==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string box1 = mysocket.ReceiveData(int.Parse(ts));
        }
        private void Butotnpaigu_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(OTNGuzhangpaicha);
            t.Start();
        }
        public void OTNGuzhangpaicha(object obj)
        {
            try
            {
                textcurrent.Text = "当前窗口：" + "\r\n";
                richTextEnd.Text = "故障排查结果：" + "\r\n";
                textlog.Text = "故障排查日志：" + "\r\n";
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "screen lines 40";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                Thread.Sleep(XHTime);
                if (checklpg.Checked == true && comSNC.Text == "OCH")
                {
                    textlog.AppendText("\r\n" + "///////////////////////////保护组" + comSslot.Text + "/" + comSport.Text + "状态查询/////////////////////////////////////////////" + "\r\n");
                    richTextEnd.AppendText("保护组状态查询：" + "\r\n");
                    textguzhangmingling.Text = "show lpg";
                    butguzhangsend.PerformClick();
                    //string lpgname = "lpg" + comlpgID.Text;
                    string lpggrop = ".*otn" + comSslot.Text + "/" + comSport.Text;
                    Regex lpggrop0 = new Regex(lpggrop, RegexOptions.IgnoreCase);
                    string lpggrop1 = lpggrop0.Match(textcurrent.Text).Groups[0].Value;
                    if (lpggrop1 == "")
                    {
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        MessageBox.Show("保护组端口:" + comSslot.Text + "/" + comSport.Text + "  未找到，请重新输入！");
                        return;
                    }
                    string[] VCGINFOFengGe = Regex.Split(lpggrop1, "\\s+", RegexOptions.IgnoreCase);
                    string LpgID = VCGINFOFengGe[1];
                    //string[] lpghangshu = textcurrent.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    //string LpgHang = "";
                    //string[] lpgidnem = new string[] { "" };
                    //string MemberSum = "";
                    //bool Lpgidfind = false;
                    bool WorkingAlarm = false;
                    bool ProtectAlarm = false;
                    //for (int a = 0; a < lpghangshu.Length; a++)
                    //{
                    //    LpgHang = lpghangshu[a];
                    //    lpgidnem = Regex.Split(LpgHang, "\\s+", RegexOptions.IgnoreCase);
                    //    if (lpgidnem.Length > 1)
                    //    {
                    //        MemberSum = lpgidnem[1];
                    //        if (MemberSum == comlpgID.Text)
                    //        {
                    //            richTextEnd.AppendText("保护组ID：" + MemberSum + "\r\n");
                    //            Lpgidfind = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    //if (Lpgidfind == false)
                    //{
                    //    textlog.AppendText(textcurrent.Text);
                    //    textcurrent.Text = "";
                    //    textguzhangmingling.Text = "exit";
                    //    butguzhangsend.PerformClick();
                    //    MessageBox.Show("保护组ID:" + comlpgID.Text + "  未找到，请重新输入！");
                    //    return;
                    //}
                    textguzhangmingling.Text = "create lpg " + LpgID;
                    butguzhangsend.PerformClick();
                    textguzhangmingling.Text = "show";
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            Regex protection = new Regex(@"protection\s*type:\s*([\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Protection = protection.Match(textcurrent.Text).Groups[1].Value;
                            if (Protection == "SNC")
                            {
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                textguzhangmingling.Text = "exit";
                                butguzhangsend.PerformClick();
                                MessageBox.Show("当前为SNC保护，请重新选择光口后尝试");
                                return;
                            }
                            Regex monitor = new Regex(@"monitor\s*mode:\s*([\w\d\/\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Monitor = monitor.Match(textcurrent.Text).Groups[1].Value;
                            Regex working = new Regex(@"working\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Working = working.Match(textcurrent.Text).Groups[1].Value;
                            Regex protect = new Regex(@"protect\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Protect = protect.Match(textcurrent.Text).Groups[1].Value;
                            Regex service = new Regex(@"service\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Service = service.Match(textcurrent.Text).Groups[1].Value;
                            Regex lpgstate = new Regex(@"lpg-state:\s*([\d\-\w\/\(\)\ \+\:]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string LpgState = lpgstate.Match(textcurrent.Text).Groups[1].Value;
                            Regex count = new Regex(@"switch\s*count:\s*([\d\-\w\/\(\)\ \+\:]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Count = count.Match(textcurrent.Text).Groups[1].Value;
                            if (Monitor.Contains("LOS") || Monitor.Contains("LOF") || Monitor.Contains("LOM") || Monitor.Contains("AIS") || Monitor.Contains("SSF"))
                            {
                                richTextEnd.AppendText("检测模式：" + Monitor + "  NOK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText("检测模式：" + Monitor + "\r\n");
                            }
                            if (Working.Contains("SF") || Working.Contains("SD"))
                            {
                                richTextEnd.AppendText("主用：" + Working + "  NOK" + "\r\n");
                                working = new Regex(@"working\s*port:\s*otn([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                Working = working.Match(textcurrent.Text).Groups[1].Value;
                                WorkingAlarm = true;
                            }
                            else
                            {
                                richTextEnd.AppendText("主用：" + Working + "\r\n");
                            }
                            if (Protect.Contains("SF") || Protect.Contains("SD"))
                            {
                                richTextEnd.AppendText("备用：" + Protect + "  NOK" + "\r\n");
                                protect = new Regex(@"protect\s*port:\s*otn([\d\/\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                Protect = protect.Match(textcurrent.Text).Groups[1].Value;
                                ProtectAlarm = true;
                            }
                            else
                            {
                                richTextEnd.AppendText("备用：" + Protect + "\r\n");
                            }
                            //richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "宿：" + Service + "\r\n");
                            richTextEnd.AppendText("倒换原因：" + LpgState + "\r\n");
                            richTextEnd.AppendText("倒换次数：" + Count + "\r\n");
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            if (WorkingAlarm == true)
                            {
                                textlog.AppendText("\r\n" + "///////////////////////////主用OTU接口告警查询/////////////////////////////////////////////" + "\r\n");
                                textguzhangmingling.Text = "ioctl otu show " + Working;
                                richTextEnd.AppendText("主用线路OTU接口告警查询：" + "\r\n");
                                butguzhangsend.PerformClick();
                                for (int h = 0; h <= 1000; h++)
                                {
                                    if (textcurrent.Text.Contains("Ctrl+c"))
                                    {
                                        butguzhangsend.PerformClick();
                                    }
                                    else
                                    {
                                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                                        Regex pttx = new Regex(@"PT\s*Tx:\s*([\w\d\.]+)\s*(Rx)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                                        Regex ptrx = new Regex(@"Rx:\s*([\w\d\.]+)\s*(Exp)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                                        Regex ptexp = new Regex(@"Exp:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                                        {
                                            richTextEnd.AppendText(Working + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                                            textlog.AppendText(textcurrent.Text);
                                            textcurrent.Text = "";
                                            textlog.AppendText("\r\n" + "/////////////////////////////主用OTU接口SFP光模块信息/////////////////////////////" + "\r\n");
                                            textguzhangmingling.Text = "interface otn " + Working;
                                            butguzhangsend.PerformClick();
                                            textguzhangmingling.Text = "show sfp";
                                            butguzhangsend.PerformClick();
                                            Thread.Sleep(XHTime);
                                            butguzhangsend.PerformClick();
                                            Thread.Sleep(XHTime);
                                            butguzhangsend.PerformClick();
                                            for (int r = 0; r <= 1000; r++)
                                            {
                                                if (textcurrent.Text.Contains("Ctrl+c"))
                                                {
                                                    butguzhangsend.PerformClick();
                                                }
                                                else
                                                {
                                                    if (textcurrent.Text.Contains("LOS"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：LOS  NOK" + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("invalid information"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：无效  NOK" + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("SFP"))
                                                    {
                                                        //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                                        //MessageBox.Show(textcurrent.Text);
                                                        Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                                        //MessageBox.Show(RxPower.ToString());
                                                        richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                                        richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                                        richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                                        //richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                                        //richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：NOK。光模块未插入" + "\r\n");
                                                    }
                                                    textlog.AppendText(textcurrent.Text);
                                                    textcurrent.Text = "";
                                                    butguzhangsend.PerformClick();
                                                    textguzhangmingling.Text = "exit";
                                                    butguzhangsend.PerformClick();
                                                    break;
                                                }
                                                Thread.Sleep(XHTime);
                                            }
                                        }
                                        else
                                        {
                                            richTextEnd.AppendText(Working + "告警：" + AlarmStatus + "\r\n");
                                        }
                                        richTextEnd.AppendText(Working + "PT-T x：" + PtTx + "\r\n");
                                        richTextEnd.AppendText(Working + "PT-Exp：" + PtExp + "\r\n");
                                        richTextEnd.AppendText(Working + "PT-R x：" + PtRx + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            if (ProtectAlarm == true)
                            {
                                textlog.AppendText("\r\n" + "///////////////////////////备用OTU接口告警查询/////////////////////////////////////////////" + "\r\n");
                                textguzhangmingling.Text = "ioctl otu show " + Protect;
                                richTextEnd.AppendText("备用线路OTU接口告警查询：" + "\r\n");
                                butguzhangsend.PerformClick();
                                for (int y = 0; y <= 1000; y++)
                                {
                                    if (textcurrent.Text.Contains("Ctrl+c"))
                                    {
                                        butguzhangsend.PerformClick();
                                    }
                                    else
                                    {
                                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                                        Regex pttx = new Regex(@"PT\s*Tx:\s*([\w\d\.]+)\s*(Rx)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                                        Regex ptrx = new Regex(@"Rx:\s*([\w\d\.]+)\s*(Exp)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                                        Regex ptexp = new Regex(@"Exp:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                                        {
                                            richTextEnd.AppendText(Protect + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                                            textlog.AppendText(textcurrent.Text);
                                            textcurrent.Text = "";
                                            textlog.AppendText("\r\n" + "/////////////////////////////备用OTU接口SFP光模块信息/////////////////////////////" + "\r\n");
                                            textguzhangmingling.Text = "interface otn " + Protect;
                                            butguzhangsend.PerformClick();
                                            textguzhangmingling.Text = "show sfp";
                                            butguzhangsend.PerformClick();
                                            Thread.Sleep(XHTime);
                                            butguzhangsend.PerformClick();
                                            Thread.Sleep(XHTime);
                                            butguzhangsend.PerformClick();
                                            for (int r = 0; r <= 1000; r++)
                                            {
                                                if (textcurrent.Text.Contains("Ctrl+c"))
                                                {
                                                    butguzhangsend.PerformClick();
                                                }
                                                else
                                                {
                                                    if (textcurrent.Text.Contains("LOS"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：LOS  NOK" + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("invalid information"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：无效  NOK" + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("SFP"))
                                                    {
                                                        //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                                        //MessageBox.Show(textcurrent.Text);
                                                        Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                                        Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                                        string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                                        //MessageBox.Show(RxPower.ToString());
                                                        richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                                        richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                                        richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                                        //richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                                        //richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                                    }
                                                    if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                                    {
                                                        richTextEnd.AppendText("光模块状态：NOK。光模块未插入" + "\r\n");
                                                    }
                                                    textlog.AppendText(textcurrent.Text);
                                                    textcurrent.Text = "";
                                                    butguzhangsend.PerformClick();
                                                    textguzhangmingling.Text = "exit";
                                                    butguzhangsend.PerformClick();
                                                    break;
                                                }
                                                Thread.Sleep(XHTime);
                                            }
                                        }
                                        else
                                        {
                                            richTextEnd.AppendText(Protect + "告警：" + AlarmStatus + "\r\n");
                                        }
                                        richTextEnd.AppendText(Protect + "PT-T x：" + PtTx + "\r\n");
                                        richTextEnd.AppendText(Protect + "PT-Exp：" + PtExp + "\r\n");
                                        richTextEnd.AppendText(Protect + "PT-R x：" + PtRx + "\r\n");
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            ArrayList list1 = getIndexArray(richTextEnd.Text, "NOK");
                            for (int i = 0; i < list1.Count; i++)
                            {
                                int index = (int)list1[i];
                                richTextEnd.Select(index, "NOK".Length);
                                richTextEnd.SelectionColor = Color.Red;
                            }
                            if (richTextEnd.Text.Contains("NOK"))
                            {
                                richTextEnd.AppendText("排查结果：存在故障！" + "\r\n");
                                MessageBox.Show("排查结果：存在故障，请排查NOK项！" + "\r\n" + "如果告警项目存在NOK，请环回所有时隙后，再次点击排查故障，确认是否为我司问题");
                            }
                            else
                            {
                                richTextEnd.AppendText("排查结果：查无故障！" + "\r\n");
                                MessageBox.Show("排查结果：查无故障！");
                            }
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                }
                textlog.AppendText("\r\n" + "///////////////////////////主用OTU接口告警查询/////////////////////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "ioctl otu show " + comSslot.Text + "/" + comSport.Text;
                richTextEnd.AppendText("主用线路OTU接口告警查询：" + "\r\n");
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                        Regex pttx = new Regex(@"PT\s*Tx:\s*([\w\d\.]+)\s*(Rx)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptrx = new Regex(@"Rx:\s*([\w\d\.]+)\s*(Exp)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptexp = new Regex(@"Exp:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                        {
                            richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textlog.AppendText("\r\n" + "/////////////////////////////主用OTU接口SFP光模块信息/////////////////////////////" + "\r\n");
                            textguzhangmingling.Text = "interface otn " + comSslot.Text + "/" + comSport.Text;
                            butguzhangsend.PerformClick();
                            textguzhangmingling.Text = "show sfp";
                            butguzhangsend.PerformClick();
                            Thread.Sleep(XHTime);
                            butguzhangsend.PerformClick();
                            Thread.Sleep(XHTime);
                            butguzhangsend.PerformClick();
                            for (int r = 0; r <= 1000; r++)
                            {
                                if (textcurrent.Text.Contains("Ctrl+c"))
                                {
                                    butguzhangsend.PerformClick();
                                }
                                else
                                {
                                    if (textcurrent.Text.Contains("LOS"))
                                    {
                                        richTextEnd.AppendText("光模块收光：LOS  NOK" + "\r\n");
                                    }
                                    if (textcurrent.Text.Contains("invalid information"))
                                    {
                                        richTextEnd.AppendText("光模块收光：无效  NOK" + "\r\n");
                                    }
                                    if (textcurrent.Text.Contains("SFP"))
                                    {
                                        //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                        //MessageBox.Show(textcurrent.Text);
                                        Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                        Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                        Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                        Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                        Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                        string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                        //MessageBox.Show(RxPower.ToString());
                                        richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                        richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                        richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                        //richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                        //richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                    }
                                    if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                    {
                                        richTextEnd.AppendText("光模块收光：NOK。光模块未插入" + "\r\n");
                                    }
                                    textlog.AppendText(textcurrent.Text);
                                    textcurrent.Text = "";
                                    butguzhangsend.PerformClick();
                                    textguzhangmingling.Text = "exit";
                                    butguzhangsend.PerformClick();
                                    break;
                                }
                                Thread.Sleep(XHTime);
                            }
                        }
                        else
                        {
                            richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "告警：" + AlarmStatus + "\r\n");
                        }
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "PT-T x：" + PtTx + "\r\n");
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "PT-Exp：" + PtExp + "\r\n");
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "PT-R x：" + PtRx + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                Thread.Sleep(XHTime);
                if (comSNC.Text != "没有配置保护")
                {
                    textlog.AppendText("\r\n" + "///////////////////////////备用OTU接口告警查询/////////////////////////////////////////////" + "\r\n");
                    textguzhangmingling.Text = "ioctl otu show " + comSBslot.Text + "/" + comSBport.Text;
                    richTextEnd.AppendText("备用线路OTU接口告警查询：" + "\r\n");
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                            Regex pttx = new Regex(@"PT\s*Tx:\s*([\w\d\.]+)\s*(Rx)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                            Regex ptrx = new Regex(@"Rx:\s*([\w\d\.]+)\s*(Exp)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                            Regex ptexp = new Regex(@"Exp:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                            if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                            {
                                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                textlog.AppendText("\r\n" + "/////////////////////////////备用OTU接口SFP光模块信息/////////////////////////////" + "\r\n");
                                textguzhangmingling.Text = "interface otn " + comSBslot.Text + "/" + comSBport.Text;
                                butguzhangsend.PerformClick();
                                textguzhangmingling.Text = "show sfp";
                                butguzhangsend.PerformClick();
                                Thread.Sleep(XHTime);
                                butguzhangsend.PerformClick();
                                Thread.Sleep(XHTime);
                                butguzhangsend.PerformClick();
                                for (int r = 0; r <= 1000; r++)
                                {
                                    if (textcurrent.Text.Contains("Ctrl+c"))
                                    {
                                        butguzhangsend.PerformClick();
                                    }
                                    else
                                    {
                                        if (textcurrent.Text.Contains("LOS"))
                                        {
                                            richTextEnd.AppendText("光模块收光：LOS  NOK" + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("invalid information"))
                                        {
                                            richTextEnd.AppendText("光模块收光：无效  NOK" + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("SFP"))
                                        {
                                            //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                            //MessageBox.Show(textcurrent.Text);
                                            Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                            Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                            Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                            Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                            Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                            //MessageBox.Show(RxPower.ToString());
                                            richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                            richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                            richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                            //richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                            //richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                        {
                                            richTextEnd.AppendText("光模块收光：NOK。光模块未插入" + "\r\n");
                                        }
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        butguzhangsend.PerformClick();
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            else
                            {
                                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "告警：" + AlarmStatus + "\r\n");
                            }
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "PT-T x：" + PtTx + "\r\n");
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "PT-Exp：" + PtExp + "\r\n");
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "PT-R x：" + PtRx + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                }
                Thread.Sleep(XHTime);
                textlog.AppendText("\r\n" + "///////////////////////////主用ODUK时隙告警查询/////////////////////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "ioctl " + comoduk.Text + " show " + comSslot.Text + " otu " + comSport.Text + "/" + comSts.Text;
                richTextEnd.AppendText("主用线路" + comoduk.Text + "时隙告警查询：" + "\r\n");
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                        Regex pttx = new Regex(@"Tx\s*=\s*([\w\d\.]+)\s*(,)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptrx = new Regex(@"Rx\s*=\s*([\w\d\.]+)\s*(,)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptexp = new Regex(@"Exp\s*=\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                        {
                            richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                        }
                        else
                        {
                            richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "告警：" + AlarmStatus + "\r\n");
                        }
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "PT-T x：" + PtTx + "\r\n");
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "PT-Exp：" + PtExp + "\r\n");
                        richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "PT-R x：" + PtRx + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                Thread.Sleep(XHTime);
                if (comSNC.Text != "没有配置保护")
                {
                    textlog.AppendText("\r\n" + "///////////////////////////备用ODUK时隙告警查询/////////////////////////////////////////////" + "\r\n");
                    textguzhangmingling.Text = "ioctl " + comoduk.Text + " show " + comSBslot.Text + " otu " + comSBport.Text + "/" + comSBts.Text;
                    richTextEnd.AppendText("备用线路" + comoduk.Text + "时隙告警查询：" + "\r\n");
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                            Regex pttx = new Regex(@"Tx\s*=\s*([\w\d\.]+)\s*(,)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                            Regex ptrx = new Regex(@"Rx\s*=\s*([\w\d\.]+)\s*(,)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                            Regex ptexp = new Regex(@"Exp\s*=\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                            if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                            {
                                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "/" + comSBts.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "/" + comSBts.Text + "告警：" + AlarmStatus + "\r\n");
                            }
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "/" + comSBts.Text + "PT-T x：" + PtTx + "\r\n");
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "/" + comSBts.Text + "PT-Exp：" + PtExp + "\r\n");
                            richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "/" + comSBts.Text + "PT-R x：" + PtRx + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                }
                Thread.Sleep(XHTime);
                textlog.AppendText("\r\n" + "///////////////////////////业务" + comDtype.Text + "接口告警查询/////////////////////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "ioctl " + comDtype.Text + " show " + comDslot.Text + "/" + comDport.Text;
                richTextEnd.AppendText("业务线路" + comDtype.Text + "接口告警查询：" + "\r\n");
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                        Regex pttx = new Regex(@"PT\s*Tx:\s*([\w\d\.]+)\s*(Rx)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptrx = new Regex(@"Rx:\s*([\w\d\.]+)\s*(Exp)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptexp = new Regex(@"Exp:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                        {
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                            if (comDtype.Text == "any")
                            {
                                textlog.AppendText(textcurrent.Text);
                                textcurrent.Text = "";
                                textlog.AppendText("\r\n" + "/////////////////////////////业务" + comDtype.Text + "接口SFP光模块信息/////////////////////////////" + "\r\n");
                                textguzhangmingling.Text = "interface any " + comDslot.Text + "/" + comDport.Text;
                                butguzhangsend.PerformClick();
                                textguzhangmingling.Text = "show sfp";
                                butguzhangsend.PerformClick();
                                Thread.Sleep(XHTime);
                                butguzhangsend.PerformClick();
                                Thread.Sleep(XHTime);
                                butguzhangsend.PerformClick();
                                for (int r = 0; r <= 100; r++)
                                {
                                    if (textcurrent.Text.Contains("Ctrl+c"))
                                    {
                                        butguzhangsend.PerformClick();
                                    }
                                    else
                                    {
                                        if (textcurrent.Text.Contains("LOS"))
                                        {
                                            richTextEnd.AppendText("光模块收光：LOS  NOK" + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("invalid information"))
                                        {
                                            richTextEnd.AppendText("光模块收光：无效  NOK" + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("SFP"))
                                        {
                                            //richTextEnd.AppendText("光模块收光：OK" + "\r\n");
                                            //MessageBox.Show(textcurrent.Text);
                                            Regex txpower = new Regex(@"Tx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string TxPower = txpower.Match(textcurrent.Text).Groups[1].Value;
                                            Regex rxpower = new Regex(@"Rx\s*Power:\s*([\-\d\.\w]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string RxPower = rxpower.Match(textcurrent.Text).Groups[1].Value;
                                            Regex rate = new Regex(@"Rate:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Rate = rate.Match(textcurrent.Text).Groups[1].Value;
                                            Regex wave = new Regex(@"Wave\s*length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Wave = wave.Match(textcurrent.Text).Groups[1].Value;
                                            Regex supported = new Regex(@"Supported length:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                            string Supported = supported.Match(textcurrent.Text).Groups[1].Value;
                                            //MessageBox.Show(RxPower.ToString());
                                            richTextEnd.AppendText("光模块发光：" + TxPower.ToString() + "\r\n");
                                            richTextEnd.AppendText("光模块收光：" + RxPower.ToString() + "\r\n");
                                            richTextEnd.AppendText("光模块速率：" + Rate.Replace("\r", "") + "\r\n");
                                            //richTextEnd.AppendText("单双纤波长：" + Wave.Replace("\r", "") + "\r\n");
                                            //richTextEnd.AppendText("光模块距离：" + Supported.Replace("\r", "") + "\r\n");
                                        }
                                        if (textcurrent.Text.Contains("SFP module is not inserted!"))
                                        {
                                            richTextEnd.AppendText("光模块收光：NOK。光模块未插入" + "\r\n");
                                        }
                                        textlog.AppendText(textcurrent.Text);
                                        textcurrent.Text = "";
                                        butguzhangsend.PerformClick();
                                        textguzhangmingling.Text = "exit";
                                        butguzhangsend.PerformClick();
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                        }
                        else
                        {
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "告警：" + AlarmStatus + "\r\n");
                        }
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "PT-T x：" + PtTx + "\r\n");
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "PT-Exp：" + PtExp + "\r\n");
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "PT-R x：" + PtRx + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        break;
                    }
                }
                Thread.Sleep(XHTime);
                textlog.AppendText("\r\n" + "///////////////////////////业务" + comDtype.Text + "时隙告警查询/////////////////////////////////////////////" + "\r\n");
                textguzhangmingling.Text = "ioctl " + comoduk.Text + " show " + comDslot.Text + " " + comDtype.Text + " " + comDport.Text + "/" + comDts.Text;
                richTextEnd.AppendText("业务线路" + comoduk.Text + "时隙告警查询：" + "\r\n");
                butguzhangsend.PerformClick();
                for (int g = 0; g <= XHCount; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        Regex alarm = new Regex(@"Alarm\s*Status:\s*([\w\d\|\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string AlarmStatus = alarm.Match(textcurrent.Text).Groups[1].Value;
                        Regex pttx = new Regex(@"Tx\s*=\s*([\d]+)\s*(h)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtTx = pttx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptrx = new Regex(@"Rx\s*=\s*([\d]+)\s*(h)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtRx = ptrx.Match(textcurrent.Text).Groups[1].Value;
                        Regex ptexp = new Regex(@"Exp\s*=\s*([\d]+)\s*(h)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string PtExp = ptexp.Match(textcurrent.Text).Groups[1].Value;
                        if (AlarmStatus.Contains("LOS") || AlarmStatus.Contains("LOF") || AlarmStatus.Contains("LOM") || AlarmStatus.Contains("AIS") || AlarmStatus.Contains("SSF"))
                        {
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "告警：" + AlarmStatus + "  NOK" + "\r\n");
                        }
                        else
                        {
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "告警：" + AlarmStatus + "\r\n");
                        }
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "PT-T x：" + PtTx + "\r\n");
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "PT-Exp：" + PtExp + "\r\n");
                        richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "PT-R x：" + PtRx + "\r\n");
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        break;
                    }
                    Thread.Sleep(XHTime);
                }
                Thread.Sleep(XHTime);
                if (comSNC.Text != "没有配置保护")
                {
                    textlog.AppendText("\r\n" + "///////////////////////////保护组" + comSslot.Text + "/" + comSport.Text + "/" + comSts.Text + "状态查询/////////////////////////////////////////////" + "\r\n");
                    richTextEnd.AppendText("保护组状态查询：" + "\r\n");
                    textguzhangmingling.Text = "show lpg";
                    butguzhangsend.PerformClick();
                    string lpggrop = ".*line-" + comoduk.Text + "-" + comSslot.Text + "/" + comSport.Text + "/" + comSts.Text;
                    Regex lpggrop0 = new Regex(lpggrop, RegexOptions.IgnoreCase);
                    string lpggrop1 = lpggrop0.Match(textcurrent.Text).Groups[0].Value;
                    if (lpggrop1 == "")
                    {
                        textlog.AppendText(textcurrent.Text);
                        textcurrent.Text = "";
                        textguzhangmingling.Text = "exit";
                        butguzhangsend.PerformClick();
                        MessageBox.Show("保护组端口:" + comSslot.Text + "/" + comSport.Text + "  未找到，请重新输入！");
                        return;
                    }
                    string[] VCGINFOFengGe = Regex.Split(lpggrop1, "\\s+", RegexOptions.IgnoreCase);
                    string LpgID = VCGINFOFengGe[1];
                    //string lpgname = "lpg" + comlpgID.Text;
                    //string[] lpghangshu = textcurrent.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    //string LpgHang  = "";
                    //string[] lpgidnem = new string[] {""};
                    //string MemberSum = "";
                    //bool Lpgidfind = false;
                    //for (int a = 0; a < lpghangshu.Length; a++)
                    //{
                    //    LpgHang = lpghangshu[a];
                    //    lpgidnem = Regex.Split(LpgHang, "\\s+", RegexOptions.IgnoreCase);
                    //    if (lpgidnem.Length > 1)
                    //    {
                    //        MemberSum = lpgidnem[1];
                    //        if (MemberSum == comlpgID.Text)
                    //        {
                    //            richTextEnd.AppendText("保护组ID：" + MemberSum + "\r\n");
                    //            Lpgidfind = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    //if(Lpgidfind == false){
                    //    textlog.AppendText(textcurrent.Text);
                    //    textcurrent.Text = "";
                    //    textguzhangmingling.Text = "exit";
                    //    butguzhangsend.PerformClick();
                    //    MessageBox.Show("保护组ID:" + comlpgID.Text + "  未找到，请重新输入！");
                    //    return;
                    //}
                    textguzhangmingling.Text = "create lpg " + LpgID;
                    butguzhangsend.PerformClick();
                    textguzhangmingling.Text = "show";
                    butguzhangsend.PerformClick();
                    for (int g = 0; g <= XHCount; g++)
                    {
                        if (textcurrent.Text.Contains("Ctrl+c"))
                        {
                            butguzhangsend.PerformClick();
                        }
                        else
                        {
                            Regex monitor = new Regex(@"monitor\s*mode:\s*([\w\d\/\.]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Monitor = monitor.Match(textcurrent.Text).Groups[1].Value;
                            Regex working = new Regex(@"working\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Working = working.Match(textcurrent.Text).Groups[1].Value;
                            Regex protect = new Regex(@"protect\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Protect = protect.Match(textcurrent.Text).Groups[1].Value;
                            Regex service = new Regex(@"service\s*port:\s*([\d\-\w\/\(\)\ \+]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Service = service.Match(textcurrent.Text).Groups[1].Value;
                            Regex lpgstate = new Regex(@"lpg-state:\s*([\d\-\w\/\(\)\ \+\:]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string LpgState = lpgstate.Match(textcurrent.Text).Groups[1].Value;
                            Regex count = new Regex(@"switch\s*count:\s*([\d\-\w\/\(\)\ \+\:]+)\s*( )*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string Count = count.Match(textcurrent.Text).Groups[1].Value;
                            if (Monitor.Contains("LOS") || Monitor.Contains("LOF") || Monitor.Contains("LOM") || Monitor.Contains("AIS") || Monitor.Contains("SSF"))
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "检测模式：" + Monitor + "  NOK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "检测模式：" + Monitor + "\r\n");
                            }
                            if (Working.Contains("SF") || Working.Contains("SD"))
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "主用：" + Working + "  NOK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "主用：" + Working + "\r\n");
                            }
                            if (Protect.Contains("SF") || Protect.Contains("SD"))
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "备用：" + Protect + "  NOK" + "\r\n");
                            }
                            else
                            {
                                richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "备用：" + Protect + "\r\n");
                            }
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "宿：" + Service + "\r\n");
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "倒换原因：" + LpgState + "\r\n");
                            richTextEnd.AppendText(comDslot.Text + "/" + comDport.Text + "/" + comDts.Text + "倒换次数：" + Count + "\r\n");
                            textlog.AppendText(textcurrent.Text);
                            textcurrent.Text = "";
                            textguzhangmingling.Text = "exit";
                            butguzhangsend.PerformClick();
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                }
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                ArrayList list = getIndexArray(richTextEnd.Text, "NOK");
                for (int i = 0; i < list.Count; i++)
                {
                    int index = (int)list[i];
                    richTextEnd.Select(index, "NOK".Length);
                    richTextEnd.SelectionColor = Color.Red;
                }
                if (richTextEnd.Text.Contains("NOK"))
                {
                    richTextEnd.AppendText("排查结果：存在故障！" + "\r\n");
                    MessageBox.Show("排查结果：存在故障，请排查NOK项！" + "\r\n" + "如果告警项目存在NOK，请环回所有时隙后，再次点击排查故障，确认是否为我司问题");
                }
                else
                {
                    richTextEnd.AppendText("排查结果：查无故障！" + "\r\n");
                    MessageBox.Show("排查结果：查无故障！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Butotnfast_Click(object sender, EventArgs e)
        {
            if (butotnfast.Text == "OTN保护增强使能")
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "config fast-monitor enable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText("OTN增强保护倒换已开启！" + "\r\n");
                butotnfast.Text = "OTN保护增强禁止";
                MessageBox.Show("OTN增强保护倒换已开启！");
            }
            else
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "config fast-monitor disable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText("OTN保护增强倒换已禁止！" + "\r\n");
                butotnfast.Text = "OTN保护增强使能";
                MessageBox.Show("OTN保护增强倒换已禁止！");
            }
        }
        private void Butsdhfast_Click(object sender, EventArgs e)
        {
            if (butsdhfast.Text == "SDH保护增强使能")
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config msap";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "psm switchtimer";
                butguzhangsend.PerformClick();
                if (textcurrent.Text.Contains("1000"))
                {
                    textguzhangmingling.Text = "psm switchtimer";
                    butguzhangsend.PerformClick();
                }
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                butsdhfast.Text = "SDH保护增强禁止";
                richTextEnd.AppendText("SDH增强保护倒换已使能！" + "\r\n");
                MessageBox.Show("SDH增强保护倒换已使能！");
            }
            else
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config msap";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "psm switchtimer";
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("1000"))
                {
                    textguzhangmingling.Text = "psm switchtimer";
                    butguzhangsend.PerformClick();
                }
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                butsdhfast.Text = "SDH保护增强使能";
                richTextEnd.AppendText("SDH增强保护倒换已禁止！" + "\r\n");
                MessageBox.Show("SDH增强保护倒换已禁止！");
            }
        }
        private void butTim_Click(object sender, EventArgs e)
        {
            butguzhangsend.PerformClick();
            if (!textcurrent.Text.Contains("#"))
            {
                MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                return;
            }
            textguzhangmingling.Text = "config otn";
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "show lpg";
            butguzhangsend.PerformClick();
            string lpggrop = ".*line-" + comoduk.Text + "-" + comSslot.Text + "/" + comSport.Text + "/" + comSts.Text;
            Regex lpggrop0 = new Regex(lpggrop, RegexOptions.IgnoreCase);
            string lpggrop1 = lpggrop0.Match(textcurrent.Text).Groups[0].Value;
            if (lpggrop1 == "")
            {
                textlog.AppendText(textcurrent.Text);
                textcurrent.Text = "";
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                MessageBox.Show("保护组端口:" + comSslot.Text + "/" + comSport.Text + "  未找到，请重新输入！");
                return;
            }
            string[] VCGINFOFengGe = Regex.Split(lpggrop1, "\\s+", RegexOptions.IgnoreCase);
            string LpgID = VCGINFOFengGe[1];
            //string lpgname = "lpg" + comlpgID.Text;
            //string[] lpghangshu = textcurrent.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            //string LpgHang = "";
            //string[] lpgidnem = new string[] { "" };
            //string MemberSum = "";
            //bool Lpgidfind = false;
            //for (int a = 0; a < lpghangshu.Length; a++)
            //{
            //    LpgHang = lpghangshu[a];
            //    lpgidnem = Regex.Split(LpgHang, "\\s+", RegexOptions.IgnoreCase);
            //    if (lpgidnem.Length > 1)
            //    {
            //        MemberSum = lpgidnem[1];
            //        if (MemberSum == comlpgID.Text)
            //        {
            //            richTextEnd.AppendText("保护组ID：" + MemberSum + "\r\n");
            //            textlog.AppendText(textcurrent.Text);
            //            textcurrent.Text = "";
            //            Lpgidfind = true;
            //            break;
            //        }
            //    }
            //}
            //if (Lpgidfind == false)
            //{
            //    textlog.AppendText(textcurrent.Text);
            //    textcurrent.Text = "";
            //    textguzhangmingling.Text = "exit";
            //    butguzhangsend.PerformClick();
            //    MessageBox.Show("保护组ID:" + comlpgID.Text + " 未找到，请重新输入！");
            //    return;
            //}
            textguzhangmingling.Text = "create lpg " + LpgID;
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "tim-trigger disable";
            butguzhangsend.PerformClick();
            textguzhangmingling.Text = "exit";
            butguzhangsend.PerformClick();
            textlog.AppendText(textcurrent.Text);
            textcurrent.Text = "";
            textguzhangmingling.Text = "exit";
            butguzhangsend.PerformClick();
            richTextEnd.AppendText("保护组ID:" + LpgID + " TIM告警检测已关闭！" + "\r\n");
            MessageBox.Show("保护组ID:" + LpgID + " TIM告警检测已关闭！");
        }
        private void butyingcang_Click(object sender, EventArgs e)
        {
            if (butyingcang.Text == "隐藏")
            {
                labgaizhislot.Visible = false;
                labgaizhitype.Visible = false;
                comotnboardmode.Visible = false;
                comotnslot.Visible = false;
                butgaizhi.Visible = false;
                butotnfast.Visible = false;
                butsdhfast.Visible = false;
                butyingcang.Text = "显示";
            }
            else
            {
                labgaizhislot.Visible = true;
                labgaizhitype.Visible = true;
                comotnboardmode.Visible = true;
                comotnslot.Visible = true;
                butgaizhi.Visible = true;
                butotnfast.Visible = true;
                butsdhfast.Visible = true;
                butyingcang.Text = "隐藏";
            }
        }
        private void Checklpg_CheckedChanged(object sender, EventArgs e)
        {
            if (checklpg.Checked)
            {
                comSNC.Text = "OCH";
            }
            else
            {
                comSNC.Text = "SNC-N";
            }
        }
        private void butoptoff_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("激光器关断和开启只对源光口作用，请选择光口为源光口！");
            if (butoptoff.Text == "主用激光器关断")
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "interface otn " + comSslot.Text + "/" + comSport.Text;
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "optical disable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "激光器已关断！" + "\r\n");
                butoptoff.Text = "主用激光器开启";
                //MessageBox.Show(comSslot.Text + "/" + comSport.Text + "激光器已关断！");
            }
            else
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "interface otn " + comSslot.Text + "/" + comSport.Text;
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "optical enable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText(comSslot.Text + "/" + comSport.Text + "激光器已开启！" + "\r\n");
                butoptoff.Text = "主用激光器关断";
                //MessageBox.Show(comSslot.Text + "/" + comSport.Text + "激光器已开启！");
            }

        }
        private void 日志LToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"C:\gpn\Logs\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + comip.Text + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
        }
        private void 问题反馈ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string saveString = "";
            for (int i = 0; i < lstboxStatus.Items.Count; i++)
            {
                saveString += lstboxStatus.Items[i].ToString() + "\r\n";
            }
            string paichajieguo = richTextEnd.Text.Replace("\n", "\r\n");
            string EmailBody = toolStripStatusLabelver.Text
                    + "\r\n" + toolStripStatusLabelnms.Text
                    + "\r\n" + toolStripStatusLabelnms18.Text
                    + "\r\n" + toolStripStatusLabelswa11.Text
                    + "\r\n" + toolStripStatusLabelswa12.Text
                    + "\r\n" + comapp.Text + ":  " + checkapp.Checked
                    + "\r\n" + comcode.Text + ":  " + checkcode.Checked
                    + "\r\n" + comnms.Text + ":  " + checknms.Checked
                    + "\r\n" + comsw.Text + ":  " + checksw.Checked
                    + "\r\n" + com760s.Text + ":  " + check760s.Checked
                    + "\r\n" + com760b.Text + ":  " + check760b.Checked
                    + "\r\n" + com760c.Text + ":  " + check760c.Checked
                    + "\r\n" + com760d.Text + ":  " + check760d.Checked
                    + "\r\n" + com760e.Text + ":  " + check760e.Checked
                    + "\r\n" + com760f.Text + ":  " + check760f.Checked
                    + "\r\n" + comsysfile.Text + ":  " + checksysfile.Checked
                    + "\r\n" + comflash.Text + ":  " + checkflash.Checked
                    + "\r\n" + comyaffs.Text + ":  " + checkyaffs.Checked
                    + "\r\n" + toolStripStatusLabeltime.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + textDOS.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + paichajieguo.ToString()
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + textlog.Text
                    + "\r\n ==============================================================================================================="
                    + "\r\n" + saveString;
            string pLocalFilePath = @"C:\gpn\Logs\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + comip.Text + "-" + DateTime.Now.ToString("yyyyMMdd") + ".txt";//要复制的文件路径
            if (File.Exists(pLocalFilePath))//必须判断要复制的文件是否存在
            {
                MySocket.WriteLogs("Logs", "软件窗口所有日志：", EmailBody);
            }
            Req Req = new Req
            {
                ShebeiIp = comip.Text
            };
            //实例化窗体
            Req.ShowDialog();// 将窗体显示出来
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (metroComreadoid.Text == "GET")
            {
                //string host = metroTextgpnip.Text;
                //string community = metroTextReadCommunity.Text;
                //SimpleSnmp snmp = new SimpleSnmp(host, community);
                //if (!snmp.Valid)
                //{
                //    MessageBox.Show("SNMP主机IP地址错误或者读写团体错误");
                //    return;
                //}
                //Dictionary<Oid, AsnType> result = snmp.Get(SnmpVersion.Ver2, new string[] { metroTextoid.Text });
                //if (result == null)
                //{
                //    MessageBox.Show("请求后未收到回复");
                //    return;
                //}
                //foreach (KeyValuePair<Oid, AsnType> kvp in result)
                //{

                //    ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                //    item.SubItems.Add(host);
                //    item.SubItems.Add(kvp.Key.ToString());
                //    item.SubItems.Add(SnmpConstants.GetTypeName(kvp.Value.Type));
                //    item.SubItems.Add(kvp.Value.ToString());
                //    item.EnsureVisible();




                //}
                // SNMP团体名称 
                OctetString community = new OctetString(metroTextReadCommunity.Text);
                //定义代理参数类 
                AgentParameters param = new AgentParameters(community);
                //将SNMP版本设置为1（或2） 
                param.Version = SnmpVersion.Ver2;
                param.DisableReplySourceCheck = true;
                //构造代理地址对象
                //这里很容易使用IpAddress类，因为
                //如果不
                //解析为IP地址，它将尝试解析构造函数参数
                IpAddress agent = new IpAddress(metroTextgpnip.Text);
                IPAddress send = new IPAddress(agent);
                //构建目标 
                UdpTarget target = new UdpTarget(send, 161, 2000, 1);
                UdpTransport h = new UdpTransport(true);

                //  用于所有请求PDU级 
                Pdu pdu = new Pdu(PduType.Get);
                pdu.VbList.Add(metroTextoid.Text);   //11槽位主备状态
                //SnmpAsyncResponse result = null;

                SnmpPacket result = null;
                try
                {
                    result = target.Request(pdu, param);
                }
                catch (SnmpException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
                //如果结果为null，则座席未回复或我们无法解析回复。
                if (result != null)
                {
                    //其他的ErrorStatus然后0是通过返回一个错误
                    //代理-见SnmpConstants为错误定义
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        //代理报告与所述请求的错误 
                        MessageBox.Show(String.Format("SNMP回复错误！错误代码：{0}，错误索引：第 {1} 行 \r\n",
                                FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                                result.Pdu.ErrorIndex));
                        return;
                    }
                    else
                    {

                        //返回变量的返回顺序与添加
                        //到VbList

                        ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                        item.SubItems.Add(metroTextgpnip.Text);
                        item.SubItems.Add(metroTextReadCommunity.Text);
                        item.SubItems.Add(result.Pdu.VbList.Count.ToString());
                        item.SubItems.Add(result.Pdu.VbList[0].Oid.ToString());
                        item.SubItems.Add(SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type));
                        item.SubItems.Add(result.Pdu.VbList[0].Value.ToString());
                        string[] hex = Regex.Split(result.Pdu.VbList[0].Value.ToString(), "\\s+", RegexOptions.IgnoreCase);
                        if ((hex.Length >= 8) && (hex[0] == "07") || (hex[0] == "08"))
                        {
                            string a = hex[0];
                            string b = hex[1];
                            string year = int.Parse(a + b, NumberStyles.HexNumber).ToString();
                            string month = int.Parse(hex[2], NumberStyles.HexNumber).ToString();
                            string day = int.Parse(hex[3], NumberStyles.HexNumber).ToString();
                            string hour = int.Parse(hex[4], NumberStyles.HexNumber).ToString();
                            string min = int.Parse(hex[5], NumberStyles.HexNumber).ToString();
                            string sed = int.Parse(hex[6], NumberStyles.HexNumber).ToString();
                            string mil = int.Parse(hex[7], NumberStyles.HexNumber).ToString();
                            item.SubItems.Add(year + "-" + month + "-" + day + "," + hour + ":" + min + ":" + sed + ":" + mil);

                        }
                        item.EnsureVisible();


                        //MessageBox.Show("ssss");
                        //toolStripStatusLabelnms.ForeColor = Color.DarkGreen;

                    }

                }
            }
            if (metroComreadoid.Text == "WALK")
            {
                this.lv2.Items.Clear();  //只移除所有的项。
                // SNMP community name
                OctetString community = new OctetString(metroTextReadCommunity.Text);
                // Define agent parameters class
                AgentParameters param = new AgentParameters(SnmpVersion.Ver2, community, true);
                // Set SNMP version to 2 (GET-BULK only works with SNMP ver 2 and 3)
                // Construct the agent address object
                // IpAddress class is easy to use here because
                //  it will try to resolve constructor parameter if it doesn't
                //  parse to an IP address
                IpAddress agent = new IpAddress(metroTextgpnip.Text);
                // Construct target
                UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
                // Define Oid that is the root of the MIB
                //  tree you wish to retrieve
                Oid rootOid = new Oid(metroTextoid.Text); // ifDescr
                                                          // This Oid represents last Oid returned by
                                                          //  the SNMP agent
                Oid lastOid = (Oid)rootOid.Clone();
                // Pdu class used for all requests
                Pdu pdu = new Pdu(PduType.GetBulk);
                // In this example, set NonRepeaters value to 0
                pdu.NonRepeaters = 0;
                // MaxRepetitions tells the agent how many Oid/Value pairs to return
                // in the response.
                pdu.MaxRepetitions = 5;
                // Loop through results
                while (lastOid != null)
                {
                    // When Pdu class is first constructed, RequestId is set to 0
                    // and during encoding id will be set to the random value
                    // for subsequent requests, id will be set to a value that
                    // needs to be incremented to have unique request ids for each
                    // packet
                    if (pdu.RequestId != 0)
                    {
                        pdu.RequestId += 1;
                    }
                    // Clear Oids from the Pdu class.
                    pdu.VbList.Clear();
                    // Initialize request PDU with the last retrieved Oid
                    pdu.VbList.Add(lastOid);
                    // Make SNMP request
                    SnmpPacket result = null;
                    try
                    {
                        result = target.Request(pdu, param);
                    }
                    catch (SnmpException ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    // You should catch exceptions in the Request if using in real application.
                    // If result is null then agent didn't reply or we couldn't parse the reply.
                    if (result != null)
                    {
                        // ErrorStatus other then 0 is an error returned by
                        // the Agent - see SnmpConstants for error definitions
                        if (result.Pdu.ErrorStatus != 0)
                        {
                            // agent reported an error with the request
                            MessageBox.Show(String.Format("SNMP回复错误！错误代码：{0}，错误索引：第 {1} 行 \r\n",
                                FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                                result.Pdu.ErrorIndex));
                            lastOid = null;
                            break;
                        }
                        else
                        {
                            // Walk through returned variable bindings
                            foreach (Vb v in result.Pdu.VbList)
                            {
                                // Check that retrieved Oid is "child" of the root OID
                                if (rootOid.IsRootOf(v.Oid))
                                {
                                    //Console.WriteLine("{0} ({1}): {2}",
                                    //    v.Oid.ToString(),
                                    //    SnmpConstants.GetTypeName(v.Value.Type),
                                    //    v.Value.ToString());

                                    ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                                    item.SubItems.Add(metroTextgpnip.Text);
                                    item.SubItems.Add(metroTextReadCommunity.Text);
                                    item.SubItems.Add(result.Pdu.VbList.Count.ToString());
                                    item.SubItems.Add(v.Oid.ToString());
                                    item.SubItems.Add(SnmpConstants.GetTypeName(v.Value.Type));
                                    item.SubItems.Add(v.Value.ToString());
                                    string[] hex = Regex.Split(result.Pdu.VbList[0].Value.ToString(), "\\s+", RegexOptions.IgnoreCase);
                                    if ((hex.Length >= 8) && (hex[0] == "07") || (hex[0] == "08"))
                                    {
                                        string a = hex[0];
                                        string b = hex[1];
                                        string year = int.Parse(a + b, NumberStyles.HexNumber).ToString();
                                        string month = int.Parse(hex[2], NumberStyles.HexNumber).ToString();
                                        string day = int.Parse(hex[3], NumberStyles.HexNumber).ToString();
                                        string hour = int.Parse(hex[4], NumberStyles.HexNumber).ToString();
                                        string min = int.Parse(hex[5], NumberStyles.HexNumber).ToString();
                                        string sed = int.Parse(hex[6], NumberStyles.HexNumber).ToString();
                                        string mil = int.Parse(hex[7], NumberStyles.HexNumber).ToString();
                                        item.SubItems.Add(year + "-" + month + "-" + day + "," + hour + ":" + min + ":" + sed + ":" + mil);

                                    }
                                    item.EnsureVisible();


                                    if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                        lastOid = null;
                                    else
                                        lastOid = v.Oid;
                                }
                                else
                                {
                                    // we have reached the end of the requested
                                    // MIB tree. Set lastOid to null and exit loop
                                    lastOid = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No response received from SNMP agent.");
                    }
                }
                target.Close();
            }


        }
        private void 获取本地软件SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Software software = new Software
            {
                //ShebeiIp = comip.Text
            };
            //实例化窗体
            software.ShowDialog();// 将窗体显示出来
        }
        private void comip_Leave(object sender, EventArgs e)
        {
            if (comip.Text == "")
            {
                comip.Text = "请输入设备ip地址";
            }
        }
        private void comip_Click(object sender, EventArgs e)
        {
            if (comip.Text == "请输入设备ip地址")
            {
                comip.Text = "";
            }
        }
        private void textcom_Click(object sender, EventArgs e)
        {
            if (textcom.Text == "请输入命令行查询")
            {
                textcom.Text = "";
            }
        }

        private void butoptbackoff_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("激光器关断和开启只对源光口作用，请选择光口为源光口！");
            if (butoptbackoff.Text == "备用激光器关断")
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "interface otn " + comSBslot.Text + "/" + comSBport.Text;
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "optical disable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "激光器已关断！" + "\r\n");
                butoptbackoff.Text = "备用激光器开启";
                // MessageBox.Show(comSBslot.Text + "/" + comSBport.Text + "激光器已关断！");
            }
            else
            {
                butguzhangsend.PerformClick();
                if (!textcurrent.Text.Contains("#"))
                {
                    MessageBox.Show("检测发现：未运行在(config)#模式下，请断开后重新连接，再次尝试！");
                    return;
                }
                textguzhangmingling.Text = "config otn";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "interface otn " + comSBslot.Text + "/" + comSBport.Text;
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "optical enable";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                textguzhangmingling.Text = "exit";
                butguzhangsend.PerformClick();
                richTextEnd.AppendText(comSBslot.Text + "/" + comSBport.Text + "激光器已开启！" + "\r\n");
                butoptbackoff.Text = "备用激光器关断";
                //MessageBox.Show(comSBslot.Text + "/" + comSBport.Text + "激光器已开启！");
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("");
            try
            {
                if (devtype == "")
                {
                    return;
                }
                // SNMP团体名称 
                OctetString community = new OctetString(textReadCommunity.Text);
                //定义代理参数类 
                AgentParameters param = new AgentParameters(SnmpVersion.Ver2, community, true);
                //将SNMP版本设置为1（或2） 
                //构造代理地址对象
                //这里很容易使用IpAddress类，因为
                //如果不
                //解析为IP地址，它将尝试解析构造函数参数
                IpAddress agent = new IpAddress(comip.Text);
                IPAddress send = new IPAddress(agent);
                //构建目标 
                UdpTarget target = new UdpTarget(send, 161, 2000, 1);
                //  用于所有请求PDU级 
                Pdu pdu = new Pdu(PduType.Get);
                if (slot17 == "ACTIVE")
                {
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.23.1.17");  //17槽位CPU利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.26.1.17");  //17槽位内存利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.20.1.17");  //17槽位温度
                }
                if (slot18 == "ACTIVE")
                {
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.23.1.18");  //18槽位CPU利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.26.1.18");  //18槽位内存利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.20.1.18");  //18槽位温度
                }
                if (devtype != "98" && devtype != "103" && devtype != "104" && devtype != "106" && devtype != "107"
                    && devtype != "108" && devtype != "109" && devtype != "110" && devtype != "111" && devtype != "112")
                {
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.23.1.1");  //1槽位CPU利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.26.1.1");  //1槽位内存利用率
                    pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.20.1.1");  //1槽位温度
                }



                SnmpPacket result = null;
                try
                {
                    result = target.Request(pdu, param);

                }
                catch (SnmpException ex)
                {
                    timer1.Stop();

                    textDOS.AppendText("没有收到SNMP请求后的响应！");
                }
                //SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
                //如果结果为null，则座席未回复或我们无法解析回复。
                if (result != null)
                {
                    //其他的ErrorStatus然后0是通过返回一个错误
                    //代理-见SnmpConstants为错误定义
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        //代理报告与所述请求的错误 
                        textDOS.Text += string.Format("\r\n" + "SNMP回复错误！错误代码：{0}，错误索引：第{1}项 \r\n",
                                FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                                result.Pdu.ErrorIndex);
                    }
                    else
                    {

                        //返回变量的返回顺序与添加
                        //到VbList
                        toolStripStatusLabelcpu.Text = "CPU:" + result.Pdu.VbList[0].Value.ToString() + "%";
                        toolStripStatusLabelmem.Text = "内存:" + result.Pdu.VbList[1].Value.ToString() + "%";
                        toolStripStatusLabeltem.Text = "温度:" + result.Pdu.VbList[2].Value.ToString() + "°C";
                        int cpu = int.Parse(result.Pdu.VbList[0].Value.ToString());
                        int mem = int.Parse(result.Pdu.VbList[1].Value.ToString());
                        int tem = int.Parse(result.Pdu.VbList[2].Value.ToString());
                        if (cpu <= 75)
                        {
                            toolStripStatusLabelcpu.ForeColor = Color.DarkGreen;

                        }
                        else
                        {
                            toolStripStatusLabelcpu.ForeColor = Color.Red;
                        }
                        if (mem <= 75)
                        {
                            toolStripStatusLabelmem.ForeColor = Color.DarkGreen;

                        }
                        else
                        {
                            toolStripStatusLabelmem.ForeColor = Color.Red;
                        }
                        if (tem <= 75)
                        {
                            toolStripStatusLabeltem.ForeColor = Color.DarkGreen;

                        }
                        else
                        {
                            toolStripStatusLabeltem.ForeColor = Color.Red;
                        }




                    }
                }
                else
                {
                    textDOS.AppendText("\r\n" + "没有收到来自SNMP代理的响应！");
                }

                target.Close();
            }
            catch (SnmpException ex)
            {
                timer1.Stop();

                textDOS.AppendText(ex.Message);
            }
        }

        private void metroButoidclear_Click(object sender, EventArgs e)
        {

            this.lv2.Items.Clear();  //只移除所有的项。
        }

        private void metroButset_Click(object sender, EventArgs e)
        {
            string WriteType = "";
            // SNMP团体名称 
            OctetString community = new OctetString(metroTextReadCommunity.Text);
            //定义代理参数类 
            AgentParameters param = new AgentParameters(SnmpVersion.Ver2, community, true);
            //将SNMP版本设置为1（或2） 
            //构造代理地址对象
            //这里很容易使用IpAddress类，因为
            //如果不
            //解析为IP地址，它将尝试解析构造函数参数
            IpAddress agent = new IpAddress(metroTextgpnip.Text);
            IPAddress send = new IPAddress(agent);
            //构建目标 
            UdpTarget target = new UdpTarget(send, 161, 2000, 1);
            //  用于所有请求PDU级 
            Pdu pdu = new Pdu(PduType.Get);
            pdu.VbList.Add(metroTextoid.Text);   //11槽位主备状态


            SnmpPacket result = null;
            try
            {
                result = target.Request(pdu, param);
            }
            catch (SnmpException ex)
            {
                // MessageBox.Show(ex.Message);
            }
            //SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
            //如果结果为null，则座席未回复或我们无法解析回复。
            if (result != null)
            {
                //其他的ErrorStatus然后0是通过返回一个错误
                //代理-见SnmpConstants为错误定义
                if (result.Pdu.ErrorStatus != 0)
                {
                    string b = "";
                    //代理报告与所述请求的错误 
                    MessageBox.Show(String.Format("SNMP回复错误！错误代码：{0}，错误索引：第{1}行\r\n",
                            FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                            result.Pdu.ErrorIndex));

                    return;
                }
                else
                {

                    //返回变量的返回顺序与添加
                    //到VbList

                    //toolStripStatusLabelver.Text = "APP:" + result.Pdu.VbList[4].Value.ToString();
                    WriteType = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);




                    //MessageBox.Show("ssss");
                    //toolStripStatusLabelnms.ForeColor = Color.DarkGreen;

                }
            }
            else
            {
                WriteType = "Integer32";
                MessageBox.Show("请求后未收到回复");
            }
            target.Close();








            // Prepare target
            target = new UdpTarget((IPAddress)new IpAddress(metroTextgpnip.Text));
            // Create a SET PDU
            pdu = new Pdu(PduType.Set);
            // Set sysLocation.0 to a new string
            if (WriteType == "OctetString")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new OctetString(metroTextvalue.Text));

            }
            if (WriteType == "Integer32")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new Integer32(metroTextvalue.Text));
            }
            if (WriteType == "UInteger32")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new UInteger32(metroTextvalue.Text));
            }
            if (WriteType == "Gauge32")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new Gauge32(metroTextvalue.Text));
            }
            if (WriteType == "TimeTicks")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new TimeTicks(metroTextvalue.Text));
            }
            if (WriteType == "IpAddress")
            {
                pdu.VbList.Add(new Oid(metroTextoid.Text), new IpAddress(metroTextvalue.Text));
            }
            AgentParameters aparam = aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(metroTextSetCommunity.Text), true);


            // Response packet
            SnmpV2Packet response;
            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch (Exception ex)
            {
                // If exception happens, it will be returned here
                MessageBox.Show("请求后未收到回复");
                target.Close();
                return;
            }
            // Make sure we received a response
            if (response == null)
            {
                MessageBox.Show("发送错误的SNMP请求");
            }
            else
            {
                // Check if we received an SNMP error from the agent
                if (response.Pdu.ErrorStatus != 0)
                {
                    MessageBox.Show(String.Format("SNMP回复错误！错误代码:{0}，错误索引：第{1}行\r\n",
                        FindDevType.FindErrorCode(response.Pdu.ErrorStatus), response.Pdu.ErrorIndex));
                }
                else
                {
                    // Everything is ok. Agent will return the new value for the OID we changed
                    ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                    item.SubItems.Add(metroTextgpnip.Text);
                    item.SubItems.Add(metroTextSetCommunity.Text);
                    item.SubItems.Add(result.Pdu.VbList.Count.ToString());
                    item.SubItems.Add(response.Pdu[0].Oid.ToString());
                    item.SubItems.Add(SnmpConstants.GetTypeName(response.Pdu[0].Value.Type));
                    item.SubItems.Add(response.Pdu[0].Value.ToString());
                    item.EnsureVisible();
                    target.Close();

                }
            }
        }
        Thread trap;
        bool run = false;
        private void metroButTrap_Click(object sender, EventArgs e)
        {

            if (metroButTrap.Text == "使能Trap监听")
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipEndPoints = ipProperties.GetActiveUdpListeners();
                foreach (IPEndPoint endPoint in ipEndPoints)
                {
                    int A = 162;
                    if (endPoint.Port == A)
                    {
                        MessageBox.Show(A + "号端口已占用，请关闭其它SNMP软件后，再次尝试！");
                        return;
                    }
                }




                trap = new Thread(Trap);
                run = true;
                trap.Start();
                metroButTrap.Text = "禁止Trap监听";
            }
            else
            {
                run = false;
                metroButTrap.Text = "使能Trap监听";
                trap.Abort();
                //socket.Shutdown(SocketShutdown.Both);
                //    Thread.Sleep(10);
                socket.Close();
                MessageBox.Show("关闭");
                //

            }


        }
        Socket socket; //目标socket
        //EndPoint ep; //客户端
        IPEndPoint ipep; //侦听端口

        private void Trap()
        {

            if (run == true)
            {
                ipep = new IPEndPoint(IPAddress.Any, 162);
                //定义套接字类型,在主线程中定义
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //服务端需要绑定ip
                try
                {
                    socket.Bind(ipep);
                }
                catch (Exception ex)
                {

                }
            }
            //定义侦听端口,侦听任何IP

            int inlen = -1;
            while (run)
            {
                byte[] indata = new byte[16 * 1024];
                // 16KB receive buffer int inlen = 0;
                IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
                EndPoint inep = peer;
                try
                {
                    inlen = socket.ReceiveFrom(indata, ref inep);
                }
                catch (Exception ex)
                {
                    // MessageBox.Show("Exception {0}", ex.Message);
                    inlen = -1;
                }
                if (inlen > 0)
                {
                    // Check protocol version int
                    int ver = SnmpPacket.GetProtocolVersion(indata, inlen);
                    if (ver == (int)SnmpVersion.Ver1)
                    {
                        // Parse SNMP Version 1 TRAP packet
                        //SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                        //pkt.decode(indata, inlen);
                        //metroTextBox1.AppendText(string.Format("** SNMP Version 1 TRAP received from {0}:", inep.ToString()));
                        //metroTextBox1.AppendText(string.Format("*** Trap generic: {0}", pkt.Pdu.Generic));
                        //metroTextBox1.AppendText(string.Format("*** Trap specific: {0}", pkt.Pdu.Specific));
                        //metroTextBox1.AppendText(string.Format("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString()));
                        //metroTextBox1.AppendText(string.Format("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString()));
                        //metroTextBox1.AppendText(string.Format("*** VarBind count: {0}", pkt.Pdu.VbList.Count));
                        //metroTextBox1.AppendText("*** VarBind content:");
                        //foreach (Vb v in pkt.Pdu.VbList)
                        //{
                        //    metroTextBox1.AppendText(string.Format("**** {0} {1}: {2}", v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                        //}
                        //metroTextBox1.AppendText("** End of SNMP Version 1 TRAP data.");
                    }
                    else
                    {
                        // Parse SNMP Version 2 TRAP packet
                        SnmpV2Packet pkt = new SnmpV2Packet();
                        pkt.decode(indata, inlen);
                        if (pkt.Pdu.Type != PduType.V2Trap)
                        {
                            MessageBox.Show("*** NOT an SNMPv2 trap ****" + "\r\n");
                        }
                        else
                        {

                            foreach (Vb v in pkt.Pdu.VbList)
                            {
                                if (metroCheckfilter.Checked == false)
                                {
                                    ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                                    item.SubItems.Add(inep.ToString());
                                    item.SubItems.Add(pkt.Community.ToString());
                                    item.SubItems.Add(pkt.Pdu.VbList.Count.ToString());
                                    item.SubItems.Add(v.Oid.ToString());
                                    item.SubItems.Add(SnmpConstants.GetTypeName(v.Value.Type));
                                    item.SubItems.Add(v.Value.ToString());

                                    string[] hex = Regex.Split(v.Value.ToString(), "\\s+", RegexOptions.IgnoreCase);
                                    if ((hex.Length >= 8) && (hex[0] == "07") || (hex[0] == "08"))
                                    {
                                        string a = hex[0];
                                        string b = hex[1];
                                        string year = int.Parse(a + b, NumberStyles.HexNumber).ToString();
                                        string month = int.Parse(hex[2], NumberStyles.HexNumber).ToString();
                                        string day = int.Parse(hex[3], NumberStyles.HexNumber).ToString();
                                        string hour = int.Parse(hex[4], NumberStyles.HexNumber).ToString();
                                        string min = int.Parse(hex[5], NumberStyles.HexNumber).ToString();
                                        string sed = int.Parse(hex[6], NumberStyles.HexNumber).ToString();
                                        string mil = int.Parse(hex[7], NumberStyles.HexNumber).ToString();
                                        item.SubItems.Add(year + "-" + month + "-" + day + "," + hour + ":" + min + ":" + sed + ":" + mil);

                                    }

                                    item.EnsureVisible();
                                }
                                else
                                {
                                    if (inep.ToString().Contains(metroTextfilterip.Text))
                                    {
                                        ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                                        item.SubItems.Add(inep.ToString());
                                        item.SubItems.Add(pkt.Community.ToString());
                                        item.SubItems.Add(pkt.Pdu.VbList.Count.ToString());
                                        item.SubItems.Add(v.Oid.ToString());
                                        item.SubItems.Add(SnmpConstants.GetTypeName(v.Value.Type));
                                        item.SubItems.Add(v.Value.ToString());
                                        string[] hex = Regex.Split(v.Value.ToString(), "\\s+", RegexOptions.IgnoreCase);
                                        if ((hex.Length >= 8) && (hex[0] == "07") || (hex[0] == "08"))
                                        {
                                            string a = hex[0];
                                            string b = hex[1];
                                            string year = int.Parse(a + b, NumberStyles.HexNumber).ToString();
                                            string month = int.Parse(hex[2], NumberStyles.HexNumber).ToString();
                                            string day = int.Parse(hex[3], NumberStyles.HexNumber).ToString();
                                            string hour = int.Parse(hex[4], NumberStyles.HexNumber).ToString();
                                            string min = int.Parse(hex[5], NumberStyles.HexNumber).ToString();
                                            string sed = int.Parse(hex[6], NumberStyles.HexNumber).ToString();
                                            string mil = int.Parse(hex[7], NumberStyles.HexNumber).ToString();
                                            item.SubItems.Add(year + "-" + month + "-" + day + "," + hour + ":" + min + ":" + sed + ":" + mil);

                                        }
                                        item.EnsureVisible();
                                    }
                                }






                            }
                            //metroTextBox1.AppendText("** End of SNMP Version 2 TRAP data." + "\r\n");
                        }
                    }
                }
                else
                {
                    if (inlen == 0)
                        MessageBox.Show("Zero length packet received.");
                }
            }



        }

        private void 关于软件OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About About = new About();//实例化窗体
            About.ShowDialog();// 将窗体显示出来
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help Help = new Help();//实例化窗体
            Help.ShowDialog();// 将窗体显示出来
        }

        private void metroButConSql_Click(object sender, EventArgs e)
        {
            String connetStr = "server=60.205.155.127;port=3306;user=root;password=Hunan7420716.; database=mib;charset=utf8;";
            // server=127.0.0.1/localhost 代表本机，端口号port默认是3306可以不写
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                            // MessageBox.Show("已经建立连接");
                            //在这里使用代码对数据库进行增删查改
                            //设置查询命令
                string sql = "select distinct table_class from mib;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                //查询结果读取器
                MySqlDataReader reader = cmd.ExecuteReader();
                metroComTableClass.Items.Clear();
                while (reader.Read())
                {

                    metroComTableClass.Items.Add(reader[0].ToString());



                }
                metroComTableClass.SelectedIndex = metroComTableClass.Items.Count - 1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void metroComTableClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            String connetStr = "server=60.205.155.127;port=3306;user=root;password=Hunan7420716.; database=mib;charset=utf8;";
            // server=127.0.0.1/localhost 代表本机，端口号port默认是3306可以不写
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                            // MessageBox.Show("已经建立连接");
                            //在这里使用代码对数据库进行增删查改
                            //设置查询命令
                string sql = "SELECT mib.table_name from mib WHERE table_class = '" + metroComTableClass.Text + "' GROUP BY table_name";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                //查询结果读取器
                MySqlDataReader reader = cmd.ExecuteReader();
                // MessageBox.Show(reader[0].ToString());
                metroComTableName.Items.Clear();
                while (reader.Read())
                {

                    metroComTableName.Items.Add(reader[0].ToString());



                }
                metroComTableName.SelectedIndex = metroComTableName.Items.Count - 1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void metroComTableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            String connetStr = "server=60.205.155.127;port=3306;user=root;password=Hunan7420716.; database=mib;charset=utf8;";
            // server=127.0.0.1/localhost 代表本机，端口号port默认是3306可以不写
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                //MessageBox.Show("已经建立连接");
                //在这里使用代码对数据库进行增删查改
                //设置查询命令
                string sql = "SELECT mib.name from mib WHERE table_name = '" + metroComTableName.Text + "' GROUP BY name";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                //查询结果读取器
                MySqlDataReader reader = cmd.ExecuteReader();
                metroComOidName.Items.Clear();
                // MessageBox.Show(reader[0].ToString());
                while (reader.Read())
                {

                    metroComOidName.Items.Add(reader[0].ToString());



                }
                metroComOidName.SelectedIndex = metroComOidName.Items.Count - 1;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void metroComOidName_SelectedIndexChanged(object sender, EventArgs e)
        {
            String connetStr = "server=60.205.155.127;port=3306;user=root;password=Hunan7420716.; database=mib;charset=utf8;";
            // server=127.0.0.1/localhost 代表本机，端口号port默认是3306可以不写
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                //MessageBox.Show("已经建立连接");
                //在这里使用代码对数据库进行增删查改
                //设置查询命令
                string SQLoid = "SELECT mib.oid, mib.type, mib.permission, mib.value, mib.note  from mib WHERE name = '" + metroComOidName.Text + "'";
                MySqlCommand oid = new MySqlCommand(SQLoid, conn);
                //查询结果读取器
                MySqlDataReader readeroid = oid.ExecuteReader();
                metroTextoid.Text = "";
                metroTextOidType.Text = "";
                metroTextOidPermission.Text = "";
                metroTextOidValue.Text = "";
                metroTextOidNote.Text = "";
                // MessageBox.Show(reader[0].ToString());
                while (readeroid.Read())
                {

                    metroTextoid.AppendText(readeroid[0].ToString());
                    metroTextOidType.AppendText(readeroid[1].ToString());
                    metroTextOidPermission.AppendText(readeroid[2].ToString());
                    metroTextOidValue.AppendText(readeroid[3].ToString());
                    metroTextOidNote.AppendText(readeroid[4].ToString());


                }

            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void metroButselect_Click(object sender, EventArgs e)
        {
            //        MessageBox.Show("支持了三方FTP工具，请先启动第三方FTP工具,然后点击批量升级。否则会出现卡死的情况，得重新关闭软件在打开！"
            //+ "\r\n" + "注意事项：FTP用户名：admin密码：admin必须一样，APP文件必须和升级的文件名一致");
            Batch batchfrm = new Batch
            {
                FTPIP = comftpip.Text,
                FTPUSR = textftpusr.Text,
                FTPPSD = textftppsd.Text,
                GPNUSR = textusr.Text,
                GPNPSD = textpsd.Text,
                GPNPSDEN = textpsden.Text,
                yanshi = ts,
                app = comapp.Text
            };//实例化窗体
            batchfrm.ShowDialog();// 将窗体显示出来
        }

        private void textDOS_TextChanged(object sender, EventArgs e)
        {

        }
        private void datagridviewcreat()
        {
            if (checkapp.Checked == true)
            {
                if (DGVSTATUS.Columns["APP"] == null)
                {

                    this.DGVSTATUS.Columns.Add("APP", "APP");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("APP");
                    this.DGVSTATUS.Columns.Add("APP", "APP");
                }
            }
            if (checkcode.Checked == true)
            {
                if (DGVSTATUS.Columns["CODE"] == null)
                {

                    this.DGVSTATUS.Columns.Add("CODE", "CODE");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("CODE");
                    this.DGVSTATUS.Columns.Add("CODE", "CODE");
                }
            }
            if (checknms.Checked == true)
            {
                if (DGVSTATUS.Columns["NMS"] == null)
                {

                    this.DGVSTATUS.Columns.Add("NMS", "NMS");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("NMS");
                    this.DGVSTATUS.Columns.Add("NMS", "NMS");
                }
            }
            if (checksw.Checked == true)
            {
                if (DGVSTATUS.Columns["SW"] == null)
                {

                    this.DGVSTATUS.Columns.Add("SW", "SW");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("SW");
                    this.DGVSTATUS.Columns.Add("SW", "SW");
                }
            }
            if (check760s.Checked == true)
            {
                if (DGVSTATUS.Columns["760S"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760S", "760S");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760S");
                    this.DGVSTATUS.Columns.Add("760S", "760S");
                }
            }
            if (check760b.Checked == true)
            {
                if (DGVSTATUS.Columns["760B"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760B", "760B");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760B");
                    this.DGVSTATUS.Columns.Add("760B", "760B");
                }
            }
            if (check760c.Checked == true)
            {
                if (DGVSTATUS.Columns["760C"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760C", "760C");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760C");
                    this.DGVSTATUS.Columns.Add("760C", "760C");
                }
            }
            if (check760d.Checked == true)
            {
                if (DGVSTATUS.Columns["760D"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760D", "760D");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760D");
                    this.DGVSTATUS.Columns.Add("760D", "760D");
                }
            }
            if (check760e.Checked == true)
            {
                if (DGVSTATUS.Columns["760E"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760E", "760E");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760E");
                    this.DGVSTATUS.Columns.Add("760E", "760E");
                }
            }
            if (check760f.Checked == true)
            {
                if (DGVSTATUS.Columns["760F"] == null)
                {

                    this.DGVSTATUS.Columns.Add("760F", "760F");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("760F");
                    this.DGVSTATUS.Columns.Add("760F", "760F");
                }
            }
            if (checksysfile.Checked == true)
            {
                if (DGVSTATUS.Columns["SysFile"] == null)
                {

                    this.DGVSTATUS.Columns.Add("SysFile", "SysFile");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("SysFile");
                    this.DGVSTATUS.Columns.Add("SysFile", "SysFile");
                }
            }
            if (checkflash.Checked == true)
            {
                if (DGVSTATUS.Columns["FLASH"] == null)
                {

                    this.DGVSTATUS.Columns.Add("FLASH", "FLASH");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("FLASH");
                    this.DGVSTATUS.Columns.Add("FLASH", "FLASH");
                }
            }
            if (checkyaffs.Checked == true)
            {
                if (DGVSTATUS.Columns["YAFFS"] == null)
                {

                    this.DGVSTATUS.Columns.Add("YAFFS", "YAFFS");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("YAFFS");
                    this.DGVSTATUS.Columns.Add("YAFFS", "YAFFS");
                }
            }
            if (checkconfig.Checked == true)
            {
                if (DGVSTATUS.Columns["Config"] == null)
                {

                    this.DGVSTATUS.Columns.Add("Config", "Config");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("Config");
                    this.DGVSTATUS.Columns.Add("Config", "Config");
                }
            }
            if (checkdb.Checked == true)
            {
                if (DGVSTATUS.Columns["Db"] == null)
                {

                    this.DGVSTATUS.Columns.Add("Db", "Db");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("Db");
                    this.DGVSTATUS.Columns.Add("Db", "Db");
                }
            }
            if (checkslotconfig.Checked == true)
            {
                if (DGVSTATUS.Columns["SlotConfig"] == null)
                {

                    this.DGVSTATUS.Columns.Add("SlotConfig", "SlotConfig");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("SlotConfig");
                    this.DGVSTATUS.Columns.Add("SlotConfig", "SlotConfig");
                }
            }



        }
        private void DataGridViewCreeatGuding()
        {

            if (DGVSTATUS.Columns["升级优先级"] == null)
            {

                this.DGVSTATUS.Columns.Add("升级优先级", "升级优先级");
            }
            else
            {
                this.DGVSTATUS.Columns.Remove("升级优先级");
                this.DGVSTATUS.Columns.Add("升级优先级", "升级优先级");
            }
            if (DGVSTATUS.Columns["ip地址"] == null)
            {

                this.DGVSTATUS.Columns.Add("ip地址", "ip地址");
            }
            else
            {
                this.DGVSTATUS.Columns.Remove("ip地址");
                this.DGVSTATUS.Columns.Add("ip地址", "ip地址");
            }
            if (DGVSTATUS.Columns["FTP操作命令"] == null)
            {

                this.DGVSTATUS.Columns.Add("FTP操作命令", "FTP操作命令");
            }
            else
            {
                this.DGVSTATUS.Columns.Remove("FTP操作命令");
                this.DGVSTATUS.Columns.Add("FTP操作命令", "FTP操作命令");
            }
            if (DGVSTATUS.Columns["重启设备"] == null)
            {
                DataGridViewCheckBoxColumn newColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "重启设备",
                    HeaderText = "重启设备"
                };
                DGVSTATUS.Columns.Add(newColumn);
            }
        }
        private void buttbatchdownload_Click(object sender, EventArgs e)
        {
            LoadCountsum = 0;
            LoadCountany = 0;


            if (buttbatchdownload.Text == "批量下载")
            {



                if (FtpPortEnable == false || FtpStatusEnable == false)
                {
                    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                    return;
                }
                string shengjiip = "";

                for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
                {
                    if ((bool)DGVSTATUS.Rows[i].Cells["执行"].EditedFormattedValue == true)
                    {
                        int c = 1;
                        LoadCountsum = LoadCountsum + c;
                        string asd = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();
                        shengjiip = shengjiip + asd + "\r\n";     //设备IP地址
                    }


                }
                DialogResult dr = MessageBox.Show("是否确认 升级如下设备？\r\n" + shengjiip + "\r\n一共：" + LoadCountsum.ToString() + "台设备", "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    // DGVSTATUS.DataSource = null;
                    //DGVSTATUS.Rows.Clear();
                    ftpCtrlFlagID = "2";
                    int i = DGVSTATUS.ColumnCount;
                    //MessageBox.Show(i.ToString());
                    for (int a = 10; a < i; a++)
                    {
                        // MessageBox.Show(a.ToString());
                        DGVSTATUS.Columns.RemoveAt(10);


                    }
                    DGVSTATUS.Refresh();
                    // DGVSTATUS.Columns.Clear();
                    //DataGridViewCreeatGuding();
                    datagridviewcreat();
                    string task = "BatchDownload";
                    ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
                    Thread t = new Thread(p);
                    t.Start(task);
                    buttbatchdownload.Text = "批量重启";
                }
                if (dr == DialogResult.No)
                {
                    buttbatchdownload.Text = "批量重启";
                }


            }
            else
            {
                string chognqiip = "";

                for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
                {
                    if ((bool)DGVSTATUS.Rows[i].Cells["重启选择"].EditedFormattedValue == true)
                    {
                        string asd = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();
                        chognqiip = chognqiip + asd + "\r\n";     //设备IP地址
                    }


                }
                DialogResult dr = MessageBox.Show("是否重启 勾选的设备？\r\n" + chognqiip, "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    int a = 0;
                    for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
                    {
                        a = int.Parse(DGVSTATUS.Rows[i].Cells["优先级"].Value.ToString());
                        DGVSTATUS.Rows[i].Cells["优先级"].Value = a;

                    }
                    DGVSTATUS.Sort(DGVSTATUS.Columns[2], ListSortDirection.Ascending);
                    //对重启选择优先级进行排序
                    if (DGVSTATUS.Columns["重启设备"] == null)
                    {

                        this.DGVSTATUS.Columns.Add("重启设备", "重启设备");
                    }
                    else
                    {
                        this.DGVSTATUS.Columns.Remove("重启设备");
                        this.DGVSTATUS.Columns.Add("重启设备", "重启设备");
                    }
                    string task = "reboot";
                    ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
                    Thread t = new Thread(p);
                    t.Start(task);
                    buttbatchdownload.Text = "批量下载";
                }
                if (dr == DialogResult.No)
                {
                    buttbatchdownload.Text = "批量下载";
                }
            }


        }
        private void Xianchengchi(object obj)
        {
            //int i = 1;
            ThreadPool.SetMinThreads(5, 5);
            ThreadPool.SetMaxThreads(20, 20);
            for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
            {
                if (obj.ToString() == "BatchDownload" && (bool)DGVSTATUS.Rows[i].Cells["执行"].EditedFormattedValue == true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(MibFtpTransentFile), i.ToString());
                }
                if (obj.ToString() == "reboot" && (bool)DGVSTATUS.Rows[i].Cells["重启选择"].EditedFormattedValue == true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(MibReboot), i.ToString());
                }
            }
        }
        private void MibReboot(object obj)
        {
            int i = int.Parse(obj.ToString());
            string gpnip = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();
            string Rebootoid = "1.3.6.1.4.1.10072.6.2.1.1.1.14.1";
            string readcommunity = textReadCommunity.Text;      //读团体
            string writecommunity = textWriteCommunity.Text;    //写团体
            int pingcunt = 5;
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(gpnip, 120);
            bool link = false;
            //判断请求是否超时
            for (int but = 0; but < pingcunt; but++)
            {

                pingReply = ping.Send(gpnip, 120);
                if (pingReply.Status == IPStatus.Success)
                {
                    link = true;
                    DGVSTATUS.Rows[i].Cells["ping测试"].Value = "OK";
                    DGVSTATUS.Rows[i].Cells["ping测试"].Style.BackColor = Color.GreenYellow;

                    break;
                }
                Thread.Sleep(XHTime);
            }
            if (link == false)
            {
                DGVSTATUS.Rows[i].Cells["ping测试"].Value = "NOK";
                DGVSTATUS.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString("hh:mm:ss");
                DGVSTATUS.Rows[i].Cells["ping测试"].Style.BackColor = Color.Yellow;
                return;
            }

            // Prepare target
            UdpTarget target = new UdpTarget((IPAddress)new IpAddress(gpnip), 161, 4000, 2);
            // Create a SET PDU
            Pdu pdu = new Pdu(PduType.Set);
            // Set sysLocation.0 to a new string
            pdu.VbList.Add(new Oid(Rebootoid), new Integer32(2));
            AgentParameters aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(writecommunity), true);
            SnmpV2Packet response = null;

            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch
            {
                DGVSTATUS.Rows[i].Cells["重启设备"].Value = "OK";
                DGVSTATUS.Rows[i].Cells["重启设备"].Style.BackColor = Color.GreenYellow;
            }
            DGVSTATUS.Rows[i].Cells["重启设备"].Value = "OK";
            DGVSTATUS.Rows[i].Cells["重启设备"].Style.BackColor = Color.GreenYellow;



        }
        private void MibFtpTransentFile(object obj)
        {



            int i = int.Parse(obj.ToString());
            string ftpServerIP = comftpip.Text;     //FTP服务器IP
            string ftpUserName = textftpusr.Text;   //FTP用户名
            string ftpPassWord = textftppsd.Text;   //FTP密码
            string ftpFileName = comcode.Text;                //FTP服务器文件名称
            string ftpLoadStatus = "";              //执行操作状态
            string ftpLoadFile = "";                //文件类型
            string ftpDeviceFileName = "";          //自定义文件名称
            string gpnip = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();       //设备IP地址
            string readcommunity = textReadCommunity.Text;      //读团体
            string writecommunity = textWriteCommunity.Text;    //写团体
            int pingcunt = 5;
            string dcnautooid = "1.3.6.1.4.1.10072.6.2.6.8.0";
            string saveoid = "1.3.6.1.4.1.10072.6.62.1.1.3.0";
            string ftpipoid = "1.3.6.1.4.1.10072.2.12.2.1.0";
            string ftpiusernameoid = "1.3.6.1.4.1.10072.2.12.2.2.0";
            string ftppassdwordoid = "1.3.6.1.4.1.10072.2.12.2.3.0";
            string ftpfilenameoid = "1.3.6.1.4.1.10072.2.12.2.5.0";
            string ftpctrlflagoid = "1.3.6.1.4.1.10072.2.12.2.6.0";
            string ftploadstatusoid = "1.3.6.1.4.1.10072.2.12.2.7.0";
            string ftploadfileoid = "1.3.6.1.4.1.10072.2.12.2.11.0";
            string ftpdevicefilenameoid = "1.3.6.1.4.1.10072.2.12.2.12.0";
            string appversionoid = "1.3.6.1.4.1.10072.6.2.1.1.1.8.1";
            string fpgaoid = "1.3.6.1.4.1.10072.6.2.1.1.1.7.1";
            // pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.11");   //11槽位主备状态
            //  pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.12");   //12槽位主备状态
            //  pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.17");   //17槽位主备状态
            //  pdu.VbList.Add("1.3.6.1.4.1.10072.6.2.2.1.1.6.1.18");   //18槽位主备状态
            string slot11oid = "1.3.6.1.4.1.10072.6.2.2.1.1.8.1.11";
            string slot12oid = "1.3.6.1.4.1.10072.6.2.2.1.1.8.1.12";
            string slot17oid = "1.3.6.1.4.1.10072.6.2.2.1.1.8.1.17";
            string slot18oid = "1.3.6.1.4.1.10072.6.2.2.1.1.8.1.18";
            string runningcountstr = "";
            int runningcount = 0;
            DGVSTATUS.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString("hh:mm:ss");

            Ping ping = new Ping();
            PingReply pingReply = ping.Send(gpnip, 120);
            bool link = false;
            //判断请求是否超时
            for (int but = 0; but < pingcunt; but++)
            {

                pingReply = ping.Send(gpnip, 120);
                if (pingReply.Status == IPStatus.Success)
                {
                    link = true;
                    DGVSTATUS.Rows[i].Cells["ping测试"].Value = "OK";
                    DGVSTATUS.Rows[i].Cells["ping测试"].Style.BackColor = Color.GreenYellow;

                    break;
                }
                Thread.Sleep(XHTime);
            }
            if (link == false)
            {
                DGVSTATUS.Rows[i].Cells["ping测试"].Value = "NOK";
                DGVSTATUS.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString("hh:mm:ss");
                DGVSTATUS.Rows[i].Cells["ping测试"].Style.BackColor = Color.Yellow;
                lock (PiLiangShengJi)
                {
                    LoadCountany++;
                }
                return;
            }



            // Prepare target
            UdpTarget target = new UdpTarget((IPAddress)new IpAddress(gpnip), 161, 4000, 2);
            // Create a SET PDU
            Pdu pdu = new Pdu(PduType.Set);
            // Set sysLocation.0 to a new string
            pdu.VbList.Add(new Oid(saveoid), new Integer32(2));
            AgentParameters aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(writecommunity), true);
            SnmpV2Packet response = null;

            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch
            {
                // If exception happens, it will be returned here
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " save " + "请求后未收到回复" + "\r\n");
                lock (PiLiangShengJi)
                {
                    LoadCountany++;
                }
                return;

            }
            // Make sure we received a response
            if (response == null)
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " save " + "发送错误的SNMP请求" + "\r\n");
                lock (PiLiangShengJi)
                {
                    LoadCountany++;
                }
                return;

            }
            else
            {
                // Check if we received an SNMP error from the agent
                if (response.Pdu.ErrorStatus != 0)
                {
                    textDOS.AppendText(String.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " save " + "SNMP回复错误！错误码 {0} 错误索引：第 {1} 行\r\n",
                    FindDevType.FindErrorCode(response.Pdu.ErrorStatus), response.Pdu.ErrorIndex));

                    DGVSTATUS.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString("hh:mm:ss");
                    lock (PiLiangShengJi)
                    {
                        LoadCountany++;
                    }
                    return;
                }
                else
                {
                    for (int a = 0; a <= XHCount; a++)
                    {
                        pdu.Reset();
                        pdu = new Pdu(PduType.Get);
                        pdu.VbList.Add(saveoid);
                        pdu.VbList.Add(appversionoid);
                        pdu.VbList.Add(fpgaoid);
                        pdu.VbList.Add(slot11oid);
                        pdu.VbList.Add(slot12oid);
                        pdu.VbList.Add(slot17oid);
                        pdu.VbList.Add(slot18oid);
                        aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(readcommunity), true);
                        SnmpPacket result = null;
                        try
                        {
                            result = target.Request(pdu, aparam);
                        }
                        catch (SnmpException ex)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " save " + ex.Message + "\r\n");
                        }
                        if (result != null)
                        {
                            if (result.Pdu.ErrorStatus != 0)
                            {
                                textDOS.AppendText(String.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " save " + "SNMP回复错误！错误代码：{0} 错误索引：第 {1} 行\r\n", FindDevType.FindErrorCode(result.Pdu.ErrorStatus), result.Pdu.ErrorIndex));
                                DGVSTATUS.Rows[i].Cells["保存"].Value = "失败";
                                DGVSTATUS.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString("hh:mm:ss");
                                break;
                            }
                            else
                            {
                                if (result.Pdu.VbList[0].Value.ToString() == "1")
                                {

                                    DGVSTATUS.Rows[i].Cells["保存"].Value = "成功";
                                    DGVSTATUS.Rows[i].Cells["保存"].Style.BackColor = Color.GreenYellow;
                                    DGVSTATUS.Rows[i].Cells["当前版本"].Value = "APP:" + result.Pdu.VbList[1].Value.ToString() + " FPGA:" + result.Pdu.VbList[2].Value.ToString();
                                    runningcountstr = result.Pdu.VbList[3].Value.ToString() + result.Pdu.VbList[4].Value.ToString() + result.Pdu.VbList[5].Value.ToString() + result.Pdu.VbList[6].Value.ToString();
                                    textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 板卡运行状态11/12/17/18槽位 4-running 1-null ：" + runningcountstr + "\r\n"));
                                    runningcount = runningcountstr.Split(new char[] { '4' }).Length - 1;


                                    break;
                                }
                                if (result.Pdu.VbList[0].Value.ToString() == "3")
                                {
                                    DGVSTATUS.Rows[i].Cells["保存"].Value = "清空";
                                }
                                if (result.Pdu.VbList[0].Value.ToString() == "4")
                                {
                                    DGVSTATUS.Rows[i].Cells["保存"].Value = "执行中";
                                }
                                if (result.Pdu.VbList[0].Value.ToString() == "5")
                                {
                                    DGVSTATUS.Rows[i].Cells["保存"].Value = "禁止";
                                    DGVSTATUS.Rows[i].Cells["保存"].Style.BackColor = Color.Yellow;
                                }
                                if (result.Pdu.VbList[0].Value.ToString() == "6")
                                {
                                    DGVSTATUS.Rows[i].Cells["保存"].Value = "失败";

                                    DGVSTATUS.Rows[i].Cells["保存"].Style.BackColor = Color.Yellow;

                                }

                            }
                        }
                        Thread.Sleep(3 * XHTime);
                    }
                }
            }


            int colunms = DGVSTATUS.ColumnCount;
            string[,] array = new string[,] {
                { "APP","2",comapp.Text,""},
                { "CODE","7",comcode.Text,""},
                { "NMS","6",comnms.Text,""},
                { "SW","5",comsw.Text,""},
                { "Config","11",comconfig.Text,""},
                { "Db","12",comdb.Text,""},
                { "SlotConfig","18",comslotconfig.Text,""},
                { "FLASH","15",comflash.Text,""},
                { "SysFile","13",comsysfile.Text,""},
               // { "OTNPACK","20",com760f.Text,""},
                { "YAFFS","",comyaffs.Text,""},
                { "760S","14",com760s.Text,"/yaffs/sys/760s.fpga"},
                { "760B","14",com760b.Text,"/yaffs/sys/760b.fpga"},
                { "760C","14",com760c.Text,"/yaffs/sys/760c.fpga"},
                { "760D","14",com760d.Text,"/yaffs/sys/760d.fpga"},
                { "760E","14",com760e.Text,"/yaffs/sys/760e.fpga"},
                { "760F","14",com760f.Text,"/yaffs/sys/760f.fpga"},
            };
            int row = array.GetLength(0);
            for (int c = 8; c < colunms; c++)
            {
                string header = DGVSTATUS.Columns[c].HeaderText;

                for (int d = 0; d < row; d++)
                {
                    if (array[d, 0].ToString() == header)
                    {

                        if (ftpCtrlFlagID == "2")
                        {
                            ftpFileName = array[d, 2].ToString();
                            textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 开始下载 文件类型：{0} 文件加载码：{1} 文件名称：{2}\r\n", array[d, 0].ToString(), array[d, 1].ToString(), array[d, 2].ToString()));


                        }
                        else
                        {

                            ftpFileName = gpnip + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + header + ".bin";
                            textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 开始上传 文件类型：{0} 文件加载码：{1} 文件名称：{2}\r\n", array[d, 0].ToString(), array[d, 1].ToString(), ftpFileName));

                        }
                        ftpLoadFile = array[d, 1].ToString();
                        ftpDeviceFileName = array[d, 3].ToString();

                        pdu.Reset();
                        pdu = new Pdu(PduType.Set);
                        pdu.VbList.Add(new Oid(ftpipoid), new IpAddress(ftpServerIP));
                        pdu.VbList.Add(new Oid(ftpiusernameoid), new OctetString(ftpUserName));
                        pdu.VbList.Add(new Oid(ftppassdwordoid), new OctetString(ftpPassWord));
                        pdu.VbList.Add(new Oid(ftpfilenameoid), new OctetString(ftpFileName));
                        pdu.VbList.Add(new Oid(ftploadfileoid), new Integer32(ftpLoadFile));
                        pdu.VbList.Add(new Oid(ftpdevicefilenameoid), new OctetString(ftpDeviceFileName));
                        aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(writecommunity), true);
                        response = null;

                        try
                        {
                            // Send request and wait for response
                            response = target.Request(pdu, aparam) as SnmpV2Packet;
                        }

                        catch
                        {
                            // If exception happens, it will be returned here
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + " 请求后未收到回复" + "\r\n");
                            break;
                        }

                        // Make sure we received a response
                        if (response == null)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + " 发送错误的SNMP请求" + "\r\n");
                            break;
                        }
                        else
                        {
                            // Check if we received an SNMP error from the agent
                            if (response.Pdu.ErrorStatus != 0)
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + String.Format(" SNMP回复错误！错误代码{0} 错误索引：第{1}行\r\n", FindDevType.FindErrorCode(response.Pdu.ErrorStatus), response.Pdu.ErrorIndex));
                                break;
                            }
                            else
                            {
                                Thread.Sleep(2 * XHTime);
                                pdu.Reset();
                                pdu.VbList.Add(new Oid(ftpctrlflagoid), new Integer32(ftpCtrlFlagID));
                                aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(writecommunity), true);
                                try
                                {
                                    response = target.Request(pdu, aparam) as SnmpV2Packet;
                                }

                                catch
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + " 请求后未收到回复" + "\r\n");
                                }
                                if (response == null)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + " 发送错误的SNMP请求" + "\r\n");
                                }
                                else
                                {
                                    // Check if we received an SNMP error from the agent
                                    if (response.Pdu.ErrorStatus != 0)
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + String.Format(" SNMP回复错误！错误码 {0} 错误索引：第 {1} 行\r\n", FindDevType.FindErrorCode(response.Pdu.ErrorStatus), response.Pdu.ErrorIndex));
                                        break;
                                    }
                                    else
                                    {
                                        if (ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells["操作命令"].Value = "下载";
                                        }
                                        if (ftpCtrlFlagID == "3")
                                        {
                                            DGVSTATUS.Rows[i].Cells["操作命令"].Value = "上传";
                                        }
                                        if (ftpCtrlFlagID == "1")
                                        {
                                            DGVSTATUS.Rows[i].Cells["操作命令"].Value = "未操作";
                                        }
                                    }
                                }

                            }
                        }
                        Thread.Sleep(3 * XHTime);
                        for (int a = 0; a <= XHCount; a++)
                        {

                            pdu.Reset();
                            pdu = new Pdu(PduType.Get);
                            pdu.VbList.Add(ftploadstatusoid);
                            aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(readcommunity), true);
                            SnmpPacket result = null;
                            try
                            {
                                result = target.Request(pdu, aparam);
                            }
                            catch
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + " 请求后未收到回复" + "\r\n");

                                break;

                            }
                            if (result != null)
                            {
                                if (result.Pdu.ErrorStatus != 0)
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " " + header + String.Format(" SNMP回复错误！错误码 {0} 错误索引：第 {1} 行\r\n", FindDevType.FindErrorCode(result.Pdu.ErrorStatus), result.Pdu.ErrorIndex));
                                    break;
                                }
                                else
                                {
                                    if (result.Pdu.VbList[0].Value.ToString() == "1")
                                    {




                                        if (header == "APP" && runningcount == 2 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";

                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响APP同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待4分钟后继续执行其他操作\r\n"));
                                                Thread.Sleep(240 * XHTime);
                                            }
                                        }
                                        if (header == "APP" && runningcount >= 3 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";

                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响APP同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待8分钟后继续执行其他操作\r\n"));
                                                Thread.Sleep(480 * XHTime);
                                            }
                                        }
                                        if (header == "CODE" && runningcount >= 2 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";
                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响CODE同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待1分钟后继续执行其他操作\r\n"));
                                                Thread.Sleep(60 * XHTime);
                                            }
                                        }
                                        if (header == "NMS" && runningcount >= 2 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";
                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响NMS同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待40秒后继续执行其他操作\r\n"));
                                                Thread.Sleep(40 * XHTime);
                                            }
                                        }

                                        if (header == "SW" && runningcount >= 2 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";
                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响SW同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待20秒后继续执行其他操作\r\n"));
                                                Thread.Sleep(20 * XHTime);
                                            }
                                        }
                                        if (header == "SysFile" && runningcount >= 2 && ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells[header].Value = "同步中";

                                            if (DGVSTATUS.ColumnCount > 10)
                                            {
                                                textDOS.AppendText(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 主要影响Sysfile同步升级的板卡有：" + runningcount.ToString() + "块" +
"需要等待20秒后继续执行其他操作\r\n"));
                                                Thread.Sleep(20 * XHTime);
                                            }
                                        }
                                        if (ftpCtrlFlagID == "2")
                                        {
                                            DGVSTATUS.Rows[i].Cells["重启选择"].Value = true;

                                        }
                                        DGVSTATUS.Rows[i].Cells[header].Value = "成功";
                                        DGVSTATUS.Rows[i].Cells[header].Style.BackColor = Color.GreenYellow;
                                        DGVSTATUS.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString("hh:mm:ss");
                                        break;
                                    }
                                    if (result.Pdu.VbList[0].Value.ToString() == "2")
                                    {
                                        DGVSTATUS.Rows[i].Cells[header].Value = "下载中";
                                    }
                                    if (result.Pdu.VbList[0].Value.ToString() == "3")
                                    {
                                        DGVSTATUS.Rows[i].Cells[header].Value = "上传中";
                                    }
                                    if (result.Pdu.VbList[0].Value.ToString() == "4")
                                    {
                                        DGVSTATUS.Rows[i].Cells[header].Value = "FTP错误";
                                        DGVSTATUS.Rows[i].Cells[header].Style.BackColor = Color.Yellow;
                                        break;
                                    }
                                    if (result.Pdu.VbList[0].Value.ToString() == "5")
                                    {
                                        DGVSTATUS.Rows[i].Cells[header].Value = "文件名错误";
                                        DGVSTATUS.Rows[i].Cells[header].Style.BackColor = Color.Yellow;
                                        break;
                                    }

                                    if (result.Pdu.VbList[0].Value.ToString() == "6")
                                    {
                                        DGVSTATUS.Rows[i].Cells[header].Value = "其他错误";
                                        DGVSTATUS.Rows[i].Cells[header].Style.BackColor = Color.Yellow;
                                        break;
                                    }

                                }
                            }
                            Thread.Sleep(3 * XHTime);
                        }

                    }

                }


                Thread.Sleep(3 * XHTime);

            }
            lock(PiLiangShengJi){
                LoadCountany++;
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + gpnip + " 累计完成设备数量：" + LoadCountany.ToString() + "\r\n");
                if (LoadCountsum == LoadCountany)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "执行完成！，一共执行：" + LoadCountany.ToString() + "台设备" + "\r\n");
                    MessageBox.Show("执行完成！，一共执行：" + LoadCountany.ToString() + "台设备");
                    LoadCountsum = 0;
                    LoadCountany = 0;

                }
            }







        }

        private void buttAddIp_Click(object sender, EventArgs e)
        {
            int index = DGVSTATUS.Rows.Add();
            DGVSTATUS.Rows[index].Cells["ip地址"].Value = comip.Text;
            DGVSTATUS.Rows[index].Cells["优先级"].Value = 1;
            DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void buttbatchupload_Click(object sender, EventArgs e)
        {
            if (FtpPortEnable == false || FtpStatusEnable == false)
            {
                MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                return;
            }

            LoadCountsum = 0;
            LoadCountany = 0;
            string shengjiip = "";

            for (int i = 0; i < DGVSTATUS.Rows.Count - 1; i++)
            {
                if ((bool)DGVSTATUS.Rows[i].Cells["执行"].EditedFormattedValue == true)
                {
                    int c = 1;
                    LoadCountsum = LoadCountsum + c;
                    string asd = DGVSTATUS.Rows[i].Cells["ip地址"].Value.ToString();
                    shengjiip = shengjiip + asd + "\r\n";     //设备IP地址
                }


            }
            DialogResult dr = MessageBox.Show("是否确认 上载如下设备？\r\n" + shengjiip + "\r\n一共：" + LoadCountsum.ToString() + "台设备", "提示", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                // DGVSTATUS.DataSource = null;
                //DGVSTATUS.Rows.Clear();
                ftpCtrlFlagID = "3";
                int i = DGVSTATUS.ColumnCount;
                //MessageBox.Show(i.ToString());
                for (int a = 10; a < i; a++)
                {
                    // MessageBox.Show(a.ToString());
                    DGVSTATUS.Columns.RemoveAt(10);


                }
                DGVSTATUS.Refresh();
                // DGVSTATUS.Columns.Clear();
                //DataGridViewCreeatGuding();
                datagridviewcreat();
                string task = "BatchDownload";
                ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
                Thread t = new Thread(p);
                t.Start(task);
            }
            if (dr == DialogResult.No)
            {
                //  buttbatchdownload.Text = "批量重启";
            }




        }


        private void del_Click(object sender, EventArgs e)
        {


            try
            {
                Gpnip user = new Gpnip();
                // 登录时 如果没有Data.bin文件就创建、有就打开
                FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
                BinaryFormatter bf = new BinaryFormatter();
                // 保存在实体类属性中
                string gpnip = DGVSTATUS.Rows[DGVSTATUS.CurrentCell.RowIndex].Cells["ip地址"].Value.ToString();       //设备IP地址
                user.GpnIP = gpnip;
                //保存密码选中状态

                if (MessageBox.Show("正在删除当前ip:" + gpnip + "，是否删除？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    userss.Remove(user.GpnIP);
                    try
                    {
                        if (DGVSTATUS != null && DGVSTATUS.CurrentCell != null && DGVSTATUS.CurrentCell.RowIndex != -1)
                        {
                            DGVSTATUS.Rows.RemoveAt(DGVSTATUS.CurrentCell.RowIndex);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("空白行无法删除");
                    }
                }
                //写入文件
                bf.Serialize(fs, userss);
                //关闭
                fs.Close();
            }
            catch
            {
                MessageBox.Show("空白行无法删除");
            }




        }

        private void add_Click(object sender, EventArgs e)
        {
            int index = DGVSTATUS.Rows.Add();
            DGVSTATUS.Rows[index].Cells["ip地址"].Value = comip.Text;
            DGVSTATUS.Rows[index].Cells["优先级"].Value = index + 1;
            DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;





            Gpnip user = new Gpnip();
            // 登录时 如果没有Data.bin文件就创建、有就打开
            FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            // 保存在实体类属性中
            user.GpnIP = comip.Text;
            if (userss.ContainsKey(user.GpnIP))
            {
                //如果有清掉
                userss.Remove(user.GpnIP);
                // MessageBox.Show("ip已经存在，替换完成");
            }
            //添加用户信息到集合
            userss.Add(user.GpnIP, user);
            //写入文件
            bf.Serialize(fs, userss);

            fs.Close();






        }


        private void 导出前俩列ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strPath = @"C:\gpn\batchip.xls";//完整的路径名

            int lie = 2;
            HSSFWorkbook wb = new HSSFWorkbook();
            HSSFSheet sheet = (HSSFSheet)wb.CreateSheet("sheet1");
            HSSFRow headRow = (HSSFRow)sheet.CreateRow(0);
            for (int i = 0; i < lie; i++)
            {
                HSSFCell headCell = (HSSFCell)headRow.CreateCell(i, CellType.String);
                headCell.SetCellValue(DGVSTATUS.Columns[i].HeaderText);
            }
            for (int i = 0; i < DGVSTATUS.Rows.Count; i++)
            {
                HSSFRow row = (HSSFRow)sheet.CreateRow(i + 1);
                for (int j = 0; j < lie; j++)
                {
                    HSSFCell cell = (HSSFCell)row.CreateCell(j);
                    if (DGVSTATUS.Rows[i].Cells[j].Value == null)
                    {
                        cell.SetCellType(CellType.Blank);
                    }
                    else
                    {

                        cell.SetCellValue(DGVSTATUS.Rows[i].Cells[j].Value.ToString());
                    }

                }

            }
            for (int i = 0; i < lie; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            using (FileStream fs = new FileStream(strPath, FileMode.Create))
            {
                wb.Write(fs);
            }
            wb.Close();




            //NPOIExcel ET = new NPOIExcel();
            //ET.ExportExcel("sheet1", DGVSTATUS, 2);
        }

        private void 导入前俩列ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DGVSTATUS.DataSource = null;
            DGVSTATUS.Columns.Clear();
            //  DGVSTATUS.Rows.Clear();
            //OpenFileDialog ofd = new OpenFileDialog();
            string strPath = @"C:\gpn\batchip.xls";//完整的路径名

            try
            {

                //strPath = ofd.FileName;
                DataTable dataTable = null;
                dataTable = ExcelUtility.ExcelToDataTable(strPath, true);
                //DataView dv = ds.Tables[0].DefaultView;
                // dataTable.DefaultView.RowFilter = "ip地址 = '" + comtype.Text + "'";

                DGVSTATUS.DataSource = dataTable;
                //dataTable.Clear();

                if (DGVSTATUS.Columns["ping测试"] == null)
                {

                    this.DGVSTATUS.Columns.Add("ping测试", "ping测试");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("ping测试");
                    this.DGVSTATUS.Columns.Add("ping测试", "ping测试");
                }
                if (DGVSTATUS.Columns["操作命令"] == null)
                {

                    this.DGVSTATUS.Columns.Add("操作命令", "操作命令");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("操作命令");
                    this.DGVSTATUS.Columns.Add("操作命令", "操作命令");
                }
                if (DGVSTATUS.Columns["重启选择"] == null)
                {
                    DataGridViewCheckBoxColumn newColumn = new DataGridViewCheckBoxColumn
                    {
                        Name = "重启选择",
                        HeaderText = "重启选择"
                    };
                    DGVSTATUS.Columns.Add(newColumn);
                }
                if (DGVSTATUS.Columns["开始时间"] == null)
                {

                    this.DGVSTATUS.Columns.Add("开始时间", "开始时间");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("开始时间");
                    this.DGVSTATUS.Columns.Add("开始时间", "开始时间");
                }
                if (DGVSTATUS.Columns["结束时间"] == null)
                {

                    this.DGVSTATUS.Columns.Add("结束时间", "结束时间");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("结束时间");
                    this.DGVSTATUS.Columns.Add("结束时间", "结束时间");
                }
                if (DGVSTATUS.Columns["保存"] == null)
                {

                    this.DGVSTATUS.Columns.Add("保存", "保存");
                }
                else
                {
                    this.DGVSTATUS.Columns.Remove("保存");
                    this.DGVSTATUS.Columns.Add("保存", "保存");
                }


                DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);//捕捉异常
            }

        }

        private void butbatchip_Click(object sender, EventArgs e)
        {



            int index = DGVSTATUS.Rows.Add();
            DGVSTATUS.Rows[index].Cells["ip地址"].Value = comip.Text;
            DGVSTATUS.Rows[index].Cells["执行"].Value = true;
            DGVSTATUS.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            int[] array = new int[DGVSTATUS.Rows.Count-1];
            int aa = 0;
            for (int i = 0; i < DGVSTATUS.Rows.Count - 2; i++, aa++)
            {
                array[i] = int.Parse(DGVSTATUS.Rows[aa].Cells[2].Value.ToString());

            }
            int temp = 0;
            int index1 = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (temp < array[i])    //如果用<= 则找bai到的是最大du值(多个中zhi的最后一个) <则是多个中的第一dao个
                {
                    temp = array[i];
                    index1 = i;
                }
            }
            DGVSTATUS.Rows[index].Cells["优先级"].Value = temp + 1;


            Gpnip user = new Gpnip();
            // 登录时 如果没有Data.bin文件就创建、有就打开
            FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
            BinaryFormatter bf = new BinaryFormatter();
            // 保存在实体类属性中
            user.GpnIP = comip.Text;
            user.GpnPRY = temp + 1;
            user.GpnZX = true;
            if (userss.ContainsKey(user.GpnIP))
            {
                //如果有清掉
                userss.Remove(user.GpnIP);
                // MessageBox.Show("ip已经存在，替换完成");
            }
            //添加用户信息到集合
            userss.Add(user.GpnIP, user);
            //写入文件
            bf.Serialize(fs, userss);

            fs.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //  ToStringArray(DGVSTATUS, false);
            //  textDOS.AppendText(ToStringArray(DGVSTATUS, false) + "\r\n");
            object[,] abc = FindDevType.Orderby(ToStringArray(DGVSTATUS, false), new int[]{2}, 0);
            for (int i = 0; i < DGVSTATUS.Rows.Count-1; i++ )
            {
                textDOS.AppendText(string.Format(abc[i,2] + "\r\n"));
            }


            }
        public object[,] ToStringArray(DataGridView dataGridView, bool includeColumnText)
        {
            #region 实现...
            object[,] arrReturn = null;
            int rowsCount = dataGridView.Rows.Count;
            int colsCount = 3;
            if (rowsCount > 0)
            {
                //最后一行是供输入的行时，不用读数据。
                if (dataGridView.Rows[rowsCount - 1].IsNewRow)
                {
                    rowsCount--;
                }
            }
            int i = 0;
            //包括列标题
            if (includeColumnText)
            {
                rowsCount++;
                arrReturn = new object[rowsCount, colsCount];
                for (i = 0; i < colsCount; i++)
                {
                    arrReturn[0, i] = dataGridView.Columns[i].HeaderText;
                }
                i = 1;
            }
            else
            {
                arrReturn = new object[rowsCount, colsCount];
            }
            //读取单元格数据
            int rowIndex = 0;
            for (; i < rowsCount; i++, rowIndex++)
            {
                for (int j = 0; j < colsCount; j++)
                {
                    if (j == 2)
                    {
                        arrReturn[i, j] = int.Parse(dataGridView.Rows[rowIndex].Cells[j].Value.ToString());
                    }
                    else {
                        arrReturn[i, j] = dataGridView.Rows[rowIndex].Cells[j].Value.ToString();
                    }

                    // textDOS.AppendText(string.Format(arrReturn[i, j] + "\r\n"));
                }
            }
            return arrReturn;
            #endregion 实现
        }

        private void DGVSTATUS_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

            if (this.DGVSTATUS.CurrentCell.ColumnIndex == 2)
            {
                e.Control.KeyPress += new KeyPressEventHandler(TextBoxDec_KeyPress);

            }
        }
        private void TextBoxDec_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && !Char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void tabPageGpn_Click(object sender, EventArgs e)
        {

        }
    }
}