using Ionic.Zip;
using MetroFramework.Forms;
using Microsoft.Win32;
using SnmpSharpNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static string defaultfilePath = "";                    //打开文件夹默认路径
        public static string version = "";              //设备版本号
        TcpListener myTcpListener = null;
        private Thread listenThread;
        public int XHTime = 1000;                       //循环间隔时间
        public int XHCount = 720;                       //循环次数
        public static string devtype = "";              //设备类型
        public bool backupfile = false;
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
            if (!Directory.Exists(@"C:\gpn"))
            {
                Directory.CreateDirectory(@"C:\gpn");
            }
            gpnurlupdate();

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
                FileStream fs = new FileStream(@"C:\gpn\gpnip.bin", FileMode.OpenOrCreate);
                if (fs.Length > 0)
                {
                    try
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        //读出存在Data.bin 里的用户信息
                        userss = bf.Deserialize(fs) as Dictionary<string, Gpnip>;
                        //循环添加到Combox1
                        foreach (Gpnip user in userss.Values)
                        {
                            comip.Items.Add(user.GpnIP);
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
                fs.Close();
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
                if (com760a.Items.Contains(ContentValue(strSec, "760A")))
                {
                    com760a.Text = ContentValue(strSec, "760A");
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
                if (comotnpack.Items.Contains(ContentValue(strSec, "OtnPack")))
                {
                    comotnpack.Text = ContentValue(strSec, "OtnPack");
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
            #endregion
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
                btnFtpServerStartStop.PerformClick();
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
                            MessageBox.Show(A + "号端口已占用，请关闭其它FTP软件后，再次尝试！");
                            return;
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
                    btnFtpServerStartStop.Text = "③停止FTP服务器";
                }
                else
                {
                    myTcpListener.Stop();
                    myTcpListener = null;
                    listenThread.Abort();
                    lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ③断开FTP服务器成功!--------------IP地址是：" + comftpip.Text);
                    //lstboxStatus.TopIndex = lstboxStatus.Items.Count - 1;
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
                myTcpListener = new TcpListener(IPAddress.Parse(comftpip.Text), int.Parse(tbxFtpServerPort.Text));
                // 开始监听传入的请求
                myTcpListener.Start();
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " ③启动FTP服务器成功!--------------IP地址是：" + comftpip.Text);
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"开始监听用户端请求....");
                //          AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"Ftp服务器运行中...[点击”停止“按钮停止FTP服务]");
                while (true)
                {
                    try
                    {
                        // 接收连接请求
                        TcpClient tcpClient = myTcpListener.AcceptTcpClient();
                        //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+string.Format("客户端（{0}）与本机（{1}）建立FTP连接", tcpClient.Client.RemoteEndPoint, myTcpListener.LocalEndpoint));
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
                lstboxStatus.Items.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " FTP服务启动失败!--------------FTP服务启动失败!");
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
                        AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + string.Format(" 客户端({0}断开连接！)", user.CommandSession.tcpClient.Client.RemoteEndPoint));
                    }
                    else
                    {
                        AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 接收命令失败！" + ex.Message);
                    }
                    break;
                }
                if (receiveString == null)
                {
                    AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 接收字符串为null,结束线程！");
                    break;
                }
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + string.Format(" 来自{0}：[{1}]", user.CommandSession.tcpClient.Client.RemoteEndPoint, receiveString));
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
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+string.Format("向客户端（{0}）发送[{1}]", user.commandSession.tcpClient.Client.RemoteEndPoint, str));
            }
            catch
            {
                // AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+string.Format("向客户端（{0}）发送信息失败", user.commandSession.tcpClient.Client.RemoteEndPoint));
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
            try
            {
                string sendString = "";
                // 下载的文件全名
                string path = user.CurrentDir + filename;
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
            catch(Exception ex)
            {
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " "+ex);
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
            ReadFileByUserSession(user, fs, filename);
            RepleyCommandToUser(user, "226 Transfer complete");
        }
        // 处理DELE命令，提供删除功能，删除服务器上的文件
        private void CommandDELE(User user, string filename)
        {
            string sendString = "";
            // 删除的文件全名
            string path = user.CurrentDir + filename;
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 正在删除文件" + filename + "...");
            File.Delete(path);
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 删除成功");
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
                    AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " TCP 数据连接已打开（被动模式）--" + localip.ToString() + "：" + port);
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
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"采被动模式返回LIST目录和文件列表");
                client = user.DataListener.AcceptTcpClient();
            }
            else
            {
                //AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"采主动模式向户发送LIST目录和文件列表");
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
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 向户发送(字符串信息)：[" + sendString + "]");
            try
            {
                user.DataSession.streamWriter.WriteLine(sendString);
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 发送完毕");
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
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 向用户发送(文件流)：[........................");
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
                        percent = (int)Math.Floor((float)totalDownloadedByte / (float)Filesize * 100);
                        // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+percent.ToString() + "\r\np");
                        //textDOS.AppendText("d:"+totalDownloadedByte.ToString() + "\r\n");
                        toolStripStatusLabjindu.Text =  percent.ToString() + "%";
                        if (percent >= 0 && percent <= 100)
                        {
                            metroProgressBar.Value = percent;
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
                metroProgressBar.Value = 100;
                toolStripStatusLabjindu.Text = 100 + "%";
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "                                              ...................]发送完毕！");
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
        private void ReadFileByUserSession(User user, FileStream fs, string filename)
        {
            AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " 接收用户上传数据（文件流）：[..................");
            try
            {
                if (user.IsBinary)
                {
                    byte[] bytes = new byte[1024];
                    long totalDownloadedByte = 0;
                    int percent = 0;
                    BinaryWriter binaryWriter = new BinaryWriter(fs);
                    //textDOS.AppendText("t:"+ Filesize.ToString() + "\r\n");
                    int count = user.DataSession.binaryReader.Read(bytes, 0, bytes.Length);
                    while (count > 0)
                    {
                        totalDownloadedByte = count + totalDownloadedByte;
                        binaryWriter.Write(bytes, 0, count);
                        binaryWriter.Flush();
                        count = user.DataSession.binaryReader.Read(bytes, 0, bytes.Length);
                        //textDOS.AppendText("t2:"+totalBytes.ToString() + "\r\n");
                        percent = (int)Math.Floor((float)totalDownloadedByte / (float)Filesize * 100);
                        // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+percent.ToString() + "\r\np");
                        //textDOS.AppendText("d:"+totalDownloadedByte.ToString() + "\r\n");
                        toolStripStatusLabjindu.Text = percent.ToString() + "%";
                        if (percent >= 0 && percent <= 100)
                        {
                            metroProgressBar.Value = percent;
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
                metroProgressBar.Value = 100;
                toolStripStatusLabjindu.Text =  100 + "%";
                AddInfo(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "                                              ...................]接收完毕！");
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
            if (butupgrade.Text == "④下载升级")
            {
                if (string.Compare(btnFtpServerStartStop.Text, "③启动FTP服务器") == 0)
                {
                    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                    return;
                }
                if (checkapp.Checked == false &&
                    checkcode.Checked == false &&
                    checknms.Checked == false &&
                    checksw.Checked == false &&
                    check760a.Checked == false &&
                    check760b.Checked == false &&
                    check760c.Checked == false &&
                    check760d.Checked == false &&
                    checkotnpack.Checked == false &&
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
                    butupgrade.Text = "④停止升级";
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
                    butupgrade.Text = "④停止升级";
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
                butupgrade.Text = "④下载升级";
            }
        }
        #endregion
        #region 定时建立telnet连接
        private void timer2_Tick(object sender, EventArgs e)
        {
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + mysocket.ReceiveData(int.Parse(ts)));
        }
        #endregion

        #region 正式升级
        //这里，就是后台进程开始工作时，调工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void DownLoadFile()
        {
            //立即开始计时，时间间隔1000毫秒
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            Testftpser();
            if (DownLoadFile_Stop)
            {
                textDOS.AppendText(DateTime.Now.ToString("\r\n"+"yyyy-MM-dd HH:mm:ss.fff") + " " + "下载升级已停止！");
                return;
            }
            Save();
            if (backupfile) {
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
            if (check760a.Checked == true)
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
            if (checkotnpack.Checked == true)
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            if (check760a.Checked == true)
            {
                Fpga760a();
                if (s == p)
                {
                    if (DownLoadFile_Stop)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            if (checkotnpack.Checked == true)
            {
                OtnPack();
                if (s == p)
                {
                    if (DownLoadFile_Stop)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (DownLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        DownLoadFilePause = new ManualResetEvent(false);
                        DownLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (DownLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (DownLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            DownLoadFilePause = new ManualResetEvent(false);
                            DownLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            Thread.Sleep(XHTime);
            string canyu = mysocket.ReceiveData(int.Parse(ts));
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
            if (check760a.Checked == true)
            {
                Fpga760aSize();
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
            if (checkotnpack.Checked == true)
            {
                OtnPackSize();
            }
            if (checksysfile.Checked == true)
            {
                SysfileSize();
            }
            Thread.Sleep(XHTime);
            string canyu2 = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "已完成";
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载结束" + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");

            DownLoadFile_Stop = true;
            butupgrade.Text = "④下载升级";
            Mytimer.Change(Timeout.Infinite, 1000);
            Reboot();
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
            if (string.Compare(butlogin.Text, "①断开设备") == 0)
            {
                butlogin.Text = "①连接设备";
                comip.Enabled = true;
                textcom.Enabled = false;
                butsend.Enabled = false;
                butguzhangsend.Enabled = false;
                butpaigu.Enabled = false;
                butsyslog.Enabled = false;
                textguzhangmingling.Enabled = false;
                butupgrade.Text = "④下载升级";
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
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已断开==================================================OK");
                this.AcceptButton = butlogin;
                mysocket.Close();
                toolStripStatusLabellinkstat.Text = "未连接";
                return;
            }
            if (string.Compare(butlogin.Text, "①连接设备") == 0)
            {
                LinkGpn();
            }
        }
        #endregion
        private void LinkGpn()
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
                if (butlogin.Text == "①连接设备")
                {
                    pingReply = ping.Send(comip.Text, timeout);
                    if (pingReply.Status == IPStatus.Success)
                    {
                        link = true;
                        break;
                    }
                }
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备无法ping通剩余：" + (int.Parse(compingcount.Text) - but).ToString() + "次，请检查IP地址：" + comip.Text + "  设备是否正常！");
                Thread.Sleep(XHTime);
            }
            if (link == false)
            {
                return;
            }
            textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备可以ping通，正在尝试Telnet登录，请稍等...");
            if (mysocket.Connect(comip.Text.Trim(), "23"))
            {
                butlogin.Text = "①断开设备";
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
                // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+mysocket.ReceiveData(int.Parse(ts)));
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
                        MessageBox.Show("非我司设置，请更换IP重启登录！");
                        butlogin.PerformClick();
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
                        butlogin.PerformClick();
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
                        textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "用户名密码正确==========================================OK");
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
                                        textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已经有用户登录，正在重新登录============================OK");
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
                                                butlogin.PerformClick();
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
                                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已经有用户登录，正在重新登录============================OK");
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
                                        butlogin.PerformClick();
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
                mysocket.SendData("service snmp source-ip auto");
                Thread.Sleep(XHTime);
                string slot = mysocket.ReceiveData(int.Parse(ts));
                // SNMP团体名称 
                OctetString community = new OctetString(textReadCommunity.Text);
                //定义代理参数类 
                AgentParameters param = new AgentParameters(community);
                //将SNMP版本设置为1（或2） 
                param.Version = SnmpVersion.Ver1;
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
                        textDOS.Text += string.Format("\r\n" + "SNMP回复错误！错误代码 {0} 。错误行数：第 {1} 行\r\n",
                                result.Pdu.ErrorStatus,
                                result.Pdu.ErrorIndex);
                        textDOS.Text += "SNMP连接存在问题，请检查读写团体是否设置正确？";
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




                //mysocket.SendData("show slot");
                ////mysocket.SendData("\r\n");
                ////mysocket.SendData("\r\n");
                //string nms17A = "17  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   ACTIVE";
                //string nms17S = "17  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   STANDBY";
                //string nms18A = "18  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   ACTIVE";
                //string nms18S = "18  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   STANDBY";
                //string nms17AV2 = "17  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   ACTIVE";
                //string nms17SV2 = "17  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   STANDBY";
                //string nms18AV2 = "18  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   ACTIVE";
                //string nms18SV2 = "18  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   STANDBY";
                //string nms17A2 = "17  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   ACTIVE";
                //string nms17S2 = "17  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   STANDBY";
                //string nms18A2 = "18  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   ACTIVE";
                //string nms18S2 = "18  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   STANDBY";
                //string swa11AA = "11  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    ACTIVE";
                //string swa12AS = "12  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    STANDBY";
                //string swa11AS = "11  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    STANDBY";
                //string swa12AA = "12  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    ACTIVE";
                //string swa11AAV2 = "11  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    ACTIVE";
                //string swa12ASV2 = "12  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    STANDBY";
                //string swa11ASV2 = "11  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    STANDBY";
                //string swa12AAV2 = "12  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    ACTIVE";
                //string swa11AAV3 = "11  GPN7600-V2-SW-A          GPN7600-V2-SW-A          RUNNING        SLAVE    ACTIVE";
                //string swa12ASV3 = "12  GPN7600-V2-SW-A          GPN7600-V2-SW-A          RUNNING        SLAVE    STANDBY";
                //string swa11ASV3 = "11  GPN7600-V2-SW-A          GPN7600-V2-SW-A          RUNNING        SLAVE    STANDBY";
                //string swa12AAV3 = "12  GPN7600-V2-SW-A          GPN7600-V2-SW-A          RUNNING        SLAVE    ACTIVE";
                //string swb11AA = "11  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    ACTIVE";
                //string swb12AS = "12  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    STANDBY";
                //string swb11AS = "11  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    STANDBY";
                //string swb12AA = "12  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    ACTIVE";
                //string GPN800 = "1  GPN800-NMS-V1            GPN800-NMS-V1            RUNNING        MASTER   ACTIVE";
                //for (int a = 0; a <= XHCount; a++)
                //{
                //     slot = mysocket.ReceiveData(int.Parse(ts));
                //    if (slot.Contains("Ctrl+c"))
                //    {
                //        mysocket.SendDate("\r\n");
                //    }
                //    if (slot.Contains("#"))
                //    {
                //        break;
                //    }
                //    if (slot.Contains(GPN800))
                //    {
                //        slot17 = "ACTIVE";
                //        toolStripStatusLabelnms.Text = "01槽：主";
                //        toolStripStatusLabelnms.ForeColor = Color.DarkGreen;
                //        // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" " + "17槽主在位==============================================OK");
                //    }
                //    if ((slot.Contains(nms17A)) || (slot.Contains(nms17AV2)) || slot.Contains(nms17A2))
                //    {
                //        slot17 = "ACTIVE";
                //        toolStripStatusLabelnms.Text = "17槽：主";
                //        toolStripStatusLabelnms.ForeColor = Color.DarkGreen;
                //        // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" " + "17槽主在位==============================================OK");
                //    }
                //    if ((slot.Contains(nms17S)) || (slot.Contains(nms17SV2)) || (slot.Contains(nms17S2)))
                //    {
                //        slot17 = "STANDBY";
                //        toolStripStatusLabelnms.Text = "17槽：备";
                //        toolStripStatusLabelnms.ForeColor = Color.Red;
                //        // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" " + "17槽备在位==============================================OK");
                //    }
                //    if ((slot.Contains(nms18A)) || (slot.Contains(nms18AV2)) || (slot.Contains(nms18A2)))
                //    {
                //        slot18 = "ACTIVE";
                //        toolStripStatusLabelnms18.Text = "18槽：主";
                //        toolStripStatusLabelnms18.ForeColor = Color.DarkGreen;
                //        //textDOS.AppendText("\r\n" + "18槽主在位==============================================OK");
                //    }
                //    if ((slot.Contains(nms18S)) || (slot.Contains(nms18SV2)) || (slot.Contains(nms18S2)))
                //    {
                //        toolStripStatusLabelnms18.Text = "18槽：备";
                //        toolStripStatusLabelnms18.ForeColor = Color.Red;
                //        slot18 = "STANDBY";
                //        // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" " + "18槽备在位=============================================OK");
                //    }
                //    if ((slot.Contains(swa11AA)) || (slot.Contains(swa11AAV2)) || (slot.Contains(swa11AAV3)))
                //    {
                //        slot11 = "在位";
                //        toolStripStatusLabelswa11.Text = "11槽SW-A：主";
                //        toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                //        //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                //    }
                //    if ((slot.Contains(swa11AS)) || (slot.Contains(swa11ASV2)) || (slot.Contains(swa11ASV3)))
                //    {
                //        slot11 = "在位";
                //        toolStripStatusLabelswa11.Text = "11槽SW-A：备";
                //        toolStripStatusLabelswa11.ForeColor = Color.Red;
                //        //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                //    }
                //    if ((slot.Contains(swa12AA)) || (slot.Contains(swa12AAV2)) || (slot.Contains(swa12AAV3)))
                //    {
                //        slot12 = "在位";
                //        toolStripStatusLabelswa12.Text = "12槽SW-A：主";
                //        toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                //        //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                //    }
                //    if ((slot.Contains(swa12AS) || slot.Contains(swb12AS)) || slot.Contains(swa12ASV2) || slot.Contains(swa12ASV3))
                //    {
                //        slot12 = "在位";
                //        toolStripStatusLabelswa12.Text = "12槽SW-A：备";
                //        toolStripStatusLabelswa12.ForeColor = Color.Red;
                //        //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                //    }
                //    if (slot.Contains(swb11AA))
                //    {
                //        slot11 = "在位";
                //        sw = "swb";
                //        toolStripStatusLabelswa11.Text = "11槽SW-B：主";
                //        toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                //        //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                //    }
                //    if (slot.Contains(swb11AS))
                //    {
                //        slot11 = "在位";
                //        sw = "swb";
                //        toolStripStatusLabelswa11.Text = "11槽SW-B：备";
                //        toolStripStatusLabelswa11.ForeColor = Color.Red;
                //        //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                //    }
                //    if (slot.Contains(swb12AA))
                //    {
                //        slot12 = "在位";
                //        sw = "swb";
                //        toolStripStatusLabelswa12.Text = "12槽SW-B：主";
                //        toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                //        //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                //    }
                //    if (slot.Contains(swb12AS))
                //    {
                //        slot12 = "在位";
                //        sw = "swb";
                //        toolStripStatusLabelswa12.Text = "12槽SW-B：备";
                //        toolStripStatusLabelswa12.ForeColor = Color.Red;
                //        //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                //    }
                //    Thread.Sleep(XHTime / 3);
                //}
                ////mysocket.SendDate("\r\n");
                //// mysocket.SendDate("\r\n");
                //// mysocket.SendDate("\x03");
                ////Thread.Sleep(XHTime);
                ////string meiyong = textDOS.Text + "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                //textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "获取槽位信息============================================OK");
                //mysocket.SendData("show version");
                //string ver = "";
                //string ver2 = "";
                //for (int a = 0; a <= XHCount; a++)
                //{
                //    ver2 = mysocket.ReceiveData(int.Parse(ts));
                //    ver = ver + ver2;
                //    if (ver2.Contains("Ctrl+c"))
                //    {
                //        mysocket.SendDate("\r\n");
                //    }
                //    if (ver2.Contains("#"))
                //    {
                //        break;
                //    }
                //    Thread.Sleep(XHTime / 3);
                //}
                //Regex r = new Regex(@"ProductOS\s*Version\s*([\w\d]+)[\s*\(]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                //string banben = r.Match(ver).Groups[1].Value;
                //if (banben.ToString() == "")
                //{
                //    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "获取版本信息===========================================NOK");
                //}
                //else
                //{
                //    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "获取版本信息============================================OK");
                //}
                //// banben = banben.Substring("ProductOS Version ".Length);
                //toolStripStatusLabelver.Text = "版本:" + banben.ToString();
                //toolStripStatusLabelver.ForeColor = Color.Red;
                //version = banben.ToString();
                ////mysocket.SendDate("\x03");
                ////Thread.Sleep(XHTime);
                ////string meiryong = textDOS.Text + "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                timer1.Start();
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "登录成功可以使用========================================OK" + "\r\n");
                this.butsend.PerformClick();
                //butguzhangsend.PerformClick();
            }
            else
            {
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "无法Telnet登录，请检查设备是否正常！");
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
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "连接通信故障，请断开后，重新尝试！");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "保存配置===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains("erro"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "保存配置==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
            textDOS.AppendText(DateTime.Now.ToString("\r\n"+"yyyy-MM-dd HH:mm:ss.fff") + " " + "FTP服务器连接测试中，请耐心等待,大约需要15秒钟.....");
            mysocket.SendData("ping " + comftpip.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ping = mysocket.ReceiveData(int.Parse(ts));
                if (ping.Contains("ms"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备ping服务器=========================================OK" + "\r\n");
                    mysocket.SendDate("\x03");
                    Thread.Sleep(XHTime);
                    string ctrlc = mysocket.ReceiveData(int.Parse(ts));
                    break;
                }
                if (ping.Contains("0 packets received"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备ping服务器=========================================NOK" + "\r\n");
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "FTP服务器故障，请点击停止升级后，检查FTP服务器IP地址！");
                    toolStripStatusLabelzt.Text = "FTP的IP地址故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "⑤上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "④下载升级";
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "FTP服务器测试==========================================OK" + "\r\n");
                    break;
                }
                if (box.Contains("fail"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "FTP服务器IP地址故障，请点击停止升级后，检查FTP服务器IP地址！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "⑤上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "④下载升级";
                    MessageBox.Show("请检查FTP服务IP地址后，再次尝试！");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "FTP服务器用户名密码错误，请检查！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    UpLoadFile_Stop = true;
                    butupload.Text = "⑤上传备份";
                    DownLoadFile_Stop = true;
                    backupfile = false;
                    butupgrade.Text = "④下载升级";
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽app_code_backup.bin========================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm app_code_backup.bin"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽record.txt=================================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm record.txt"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽/flash/sys/fpga_code.bin===================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (command.Contains("rm fpga_code.bin"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽有其他户登录，已退出终止升级");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽app_code_backup.bin======================文件不能存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm app_code_backup.bin"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽record.txt=================================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm record.txt"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽/flash/sys/fpga_code.bin===================文件不存在" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                    if (command.Contains("rm fpga_code.bin"))
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽APP下载成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    toolStripStatusLabelzt.Text = "写入APP中";
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "APP写入成功============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽APP写入成功==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽APP写入成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽APP写入===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽APP下载==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "升级为R13版本执行特殊升级方式===========================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_CODE下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (comapp.Text.Contains("R13"))
                        {
                            if (download.Contains("ok"))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }
                        }
                        else
                        {
                            if (download.Contains("ok"))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备槽准备同步FPGA_CODE================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail") || up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步FPGA_CODE==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail") || up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_CODE写入=======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_CODE下载==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_NMS下载成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_NMS写入成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            if (slot18 == "STANDBY")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= XHCount; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 18"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步FPGA_NMS===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    }
                                    if (up18slot.Contains("18 fail"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步FPGA_NMS======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }
                                    if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            if (slot17 == "STANDBY")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= XHCount; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 17"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    }
                                    if (up18slot.Contains("17 fail"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步FPGA_NMS==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }
                                    if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                    {
                                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步FPGA_NMS===========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(XHTime);
                                }
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_NMS写入========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽FPGA_NMS下载===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主槽SW_FPGA下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= XHCount; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA===============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                                break;
                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "18槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")
                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= XHCount; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(XHTime);
                                    }
                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主用槽SW_FPGA写入=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "主用槽SW_FPGA下载========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
            string jieshu = mysocket.ReceiveData(int.Parse(ts));
        }
        #endregion
        #region 下载 Fpga760a
        private void Fpga760a()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载760A";
            mysocket.SendData("download ftp file /yaffs/sys/760a.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760a.Text);
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760a===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760A";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {

                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步760a===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }

                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760a===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760b===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760B";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步760b===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760b===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760c===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760C";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步760c===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760c===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760d===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760D";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步760d===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }

                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760d===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760e===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步760E";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步760e===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载760e===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                Thread.Sleep(XHTime);
            }
            Thread.Sleep(XHTime);
        }
        #endregion
        #region 下载 OTN-Pack
        private void OtnPack()
        {
            Thread.Sleep(XHTime);
            string cccc = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "正在下载OtnPack";
            mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comotnpack.Text + " otn");
            for (int i = 1; i <= XHCount; i++)
            {
                string ok = "Write to flash...";
                string fail = "fail";
                string Error = "Error";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(Error))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载OtnPack===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "由于此版本不支持大于30Mb文件写入，请下载R19C07B035版本后的APP重启再次进行尝试下载，我们将在此版本支持。" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                    //mysocket.SendData("download ftp file /yaffs/sys/otn_pack.bin  " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comotnpack.Text);
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载OtnPack============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    Thread.Sleep(8000);
                    if (slot17 != "" && slot18 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步OtnPack";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步OtnPack============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载OtnPack============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载sysfile============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    if (slot17 != "" && slot18 != "" || slot11 != "" || slot12 != "")
                    {
                        toolStripStatusLabelzt.Text = "正在同步sysfile文件";
                        string sync = "successful";
                        for (int p = 1; p <= XHCount; p++)
                        {
                            string syncotn = mysocket.ReceiveData(int.Parse(ts));
                            if (syncotn.Contains(sync))
                            {
                                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "同步sysfile============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                Thread.Sleep(XHTime);
                                break;
                            }
                            Thread.Sleep(XHTime);
                        }
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载sysfile=========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载Flash==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "写入Flash进度===========================================O%");
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
                    metroProgressBar.Value = int.Parse(strjinfu);
                    toolStripStatusLabjindu.Text = jindu;
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "写入Flash==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载Flash=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
            mysocket.SendData("download ftp yaffs " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comflash.Text);
            for (int i = 1; i <= 6 * XHCount; i++)
            {
                string ok = "100%";
                string fail = "fail";
                string downloadok = "Download file ...ok";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(downloadok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "写入Yaffs进度===========================================O%");
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
                    metroProgressBar.Value = int.Parse(strjinfu);
                    toolStripStatusLabjindu.Text = jindu;
                }
                if (box.Contains(ok))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载Yaffs=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载config========================请检查FTP户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载lsotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "下载db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份config=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份slotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(XHTime);
                    }
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "您选择重启设备=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                Thread.Sleep(XHTime);
                string command = mysocket.ReceiveData(int.Parse(ts));
                butlogin.PerformClick();
                //户选择确认的操作
            }
            if (dr == DialogResult.No)
            {
                //户选择取消的操作
                mysocket.SendData("N");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "您没有选择重启设备=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
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
            // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+mysocket.ReceiveData(int.Parse(ts)));
            this.butsend.PerformClick();
        }
        #endregion
        #region ctrl+q按钮
        private void butctrlq_Click(object sender, EventArgs e)
        {
            mysocket.SendDate("\x011");
            // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+"CTRL+Q已发送");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + mysocket.ReceiveData(int.Parse(ts)) + "\n");
                //text = 
                //this.butsend.Focus();
                //textcom.Text = com.ToString();
            }
            if (e.KeyCode == Keys.Down)
            {
                //textcom.Text = "";
                mysocket.SendDate("\x1b\x5b\x42");
                Thread.Sleep(XHTime);
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + mysocket.ReceiveData(int.Parse(ts)) + "\n");
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
            comapp.Items.Clear();//清除之前打开的历史  
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
                if (s.Contains(".bin") && !s.Contains("code") && !s.Contains("sysfile") && !s.Contains("db") && !s.Contains("slot"))
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
                if (s.Contains("config") && !s.Contains("slot"))
                {
                    comconfig.Items.Add(s);
                    if (comconfig.Items.Count > 0)
                    {
                        comconfig.SelectedIndex = comconfig.Items.Count - 1;
                    }
                }
                if (s.Contains("db"))
                {
                    comdb.Items.Add(s);
                    if (comdb.Items.Count > 0)
                    {
                        comdb.SelectedIndex = comdb.Items.Count - 1;
                    }
                }
                if (s.Contains("slotconfig"))
                {
                    comslotconfig.Items.Add(s);
                    if (comslotconfig.Items.Count > 0)
                    {
                        comslotconfig.SelectedIndex = comslotconfig.Items.Count - 1;
                    }
                }
                if (s.Contains("760a") || s.Contains("760A"))
                {
                    com760a.Items.Add(s);
                    if (com760a.Items.Count > 0)
                    {
                        com760a.SelectedIndex = com760a.Items.Count - 1;
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
                if (s.Contains("pack") || s.Contains("PACK") || s.Contains("Pack"))
                {
                    comotnpack.Items.Add(s);
                    if (comotnpack.Items.Count > 0)
                    {
                        comotnpack.SelectedIndex = comotnpack.Items.Count - 1;
                    }
                }
                if (s.Contains("sysfile") || s.Contains("Sysfile") || s.Contains("SYSFILE"))
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
                    + "\r\n" + com760a.Text + ":  " + check760a.Checked
                    + "\r\n" + com760b.Text + ":  " + check760b.Checked
                    + "\r\n" + com760c.Text + ":  " + check760c.Checked
                    + "\r\n" + com760d.Text + ":  " + check760d.Checked
                    + "\r\n" + com760e.Text + ":  " + check760e.Checked
                    + "\r\n" + comotnpack.Text + ":  " + checkotnpack.Checked
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
                // textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+"CTRL+Q已发送");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + newSw);
                    }
                    else
                    {
                        string luanma = "\b";
                        string newSS = stra.Replace(luanma, "");
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + newSS);
                    }
                }
                else
                {
                    string luanma = "\b";
                    string newSS = stra.Replace(luanma, "");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + newSS);
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
        private Dictionary<string, Gpnip> userss = new Dictionary<string, Gpnip>();
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
                WritePrivateProfileString(strSec, "760A", com760a.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760B", com760b.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760C", com760c.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760D", com760d.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "760E", com760e.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "OtnPack", comotnpack.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "sysfile", comsysfile.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "Flash", comflash.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "Yaffs", comyaffs.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPN7600EMS", comgpn76list.Text.Trim(), strFilePath);
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
                //关闭
                fs.Close();
                // textmesg.Text = "用户以保存，重新打开软件后会显示";
                //MessageBox.Show("账号保存成功！！！");
                //MessageBox.Show("写入成功");
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
            DialogResult dr = MessageBox.Show("是否退出并保存？", "提示", MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                Gpnsetini();
                ///////保存telnet 记录////////
                Savecom();
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
            About About = new About();//实例化窗体
            About.ShowDialog();// 将窗体显示出来
        }
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help Help = new Help();//实例化窗体
            Help.ShowDialog();// 将窗体显示出来
        }
        private void Butgpnall_Click(object sender, EventArgs e)
        {
            if (butgpnall.Text == "全部勾选")
            {
                checkapp.Checked = false;
                checkcode.Checked = false;
                checknms.Checked = false;
                checksw.Checked = false;
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                check760a.Checked = true;
                check760b.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                check760e.Checked = true;
                checkotnpack.Checked = true;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                check760a.Checked = true;
                check760b.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                check760e.Checked = true;
                checkotnpack.Checked = true;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
                checksysfile.Checked = false;
                checkapp.Checked = true;
                checknms.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
                checkotnpack.Checked = true;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checkotnpack.Checked = false;
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
                check760a.Checked = false;
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
                check760a.Checked = false;
                check760b.Checked = false;
                check760c.Checked = false;
                check760d.Checked = false;
                check760e.Checked = false;
                checksysfile.Checked = false;
                butgpn7600old.Text = "GPN76-PTN勾选";
            }
        }
        private void Xianchengchi(object obj)
        {
            int i = 1;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(int.Parse("10"), int.Parse("10"));
            if (obj.ToString() == "Syslog")
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Syslog), i.ToString());
            }
            if (obj.ToString() == "GuZhangPaiCha")
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(GuZhangPaiCha), i.ToString());
            }
        }
        /// <summary>
        /// 一键导出入职主程序
        /// </summary>
        /// <param name="obj"></param>
        public void Syslog(object obj)
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
        /// <summary>
        /// 故障排查主程序
        /// </summary>
        /// <param name="obj"></param>
        public void GuZhangPaiCha(object obj)
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
                                Regex Ts = new Regex(@"nul:\s*(\d\/\d\-\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
                                Regex Ts = new Regex(@"sdh:\s*(\d\/\d\-\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
                            string VCGinfo = VCGINFOFengGe[3];
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
            string task = "Syslog";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
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
                    Thread.Sleep(XHTime / 3);
                    string ctrlc = "Press any key to continue Ctrl+c to stop";
                    string DOS = textcurrent.Text;
                    if (DOS.Contains(ctrlc))
                    {
                        textcurrent.Text = DOS.Replace(ctrlc, "");
                        //MessageBox.Show("检测到了");
                    }
                    string str = "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                    //string luanma = "[7m --Press any key to continue Ctrl+c to stop-- [m";
                    //string newSS = str.Replace(luanma, "Press any key to continue Ctrl+c to stop");
                    //string luama2 = "\n" + "                                              ";
                    //string newSD = newSS.Replace(luama2, "");
                    //string vcg = "[0m[0;0m";
                    //string newvcg = newSD.Replace(vcg, "");
                    //string vcg2 = "\n";
                    //string newvcg2 = newvcg.Replace(vcg2, "\r\n");
                    //string kou = "";
                    //string kou2 = "";
                    //string newkou = newvcg2.Replace(kou, "");
                    //string newkou2 = newkou.Replace(kou2, "");
                    //string msapeth = "[0;32m";
                    //string msapeth2 = newkou2.Replace(msapeth, "");
                    //string msapeth1 = "[0m";
                    //string msapeth3 = msapeth2.Replace(msapeth1, "");
                    //string msapeth4 = "[0;0m";
                    //string msapeth5 = msapeth3.Replace(msapeth4, "");
                    //string msapeth6 = "[0;31m";
                    //string msapeth7 = msapeth5.Replace(msapeth6, "");
                    //textBox3.Text = newkou2;
                    textcurrent.AppendText(str);
                    //this.textBox3.Text = str;
                }
                else
                {
                    com = textguzhangmingling.Text;
                    Thread.Sleep(XHTime / 3);
                    string ss = mysocket.ReceiveData(int.Parse(ts));
                    //string luanma = "[7m --Press any key to continue Ctrl+c to stop-- [m";
                    //string newSS = ss.Replace(luanma, "Press any key to continue Ctrl+c to stop");
                    //string luama2 = "\n" + "                                              ";
                    //string newSD = newSS.Replace(luama2, "");
                    //string vcg = "[0m[0;0m";//[0;31m
                    //string newvcg = newSD.Replace(vcg, "");
                    //string vcg2 = "\n";
                    //string newvcg2 = newvcg.Replace(vcg2, "\r\n");
                    //string kou = "";
                    //string kou2 = "";
                    //string newkou = newvcg2.Replace(kou, "");
                    //string newkou2 = newkou.Replace(kou2, "");
                    //string msapeth = "[0;32m";
                    //string msapeth2 = newkou2.Replace(msapeth, "");
                    //string msapeth1 = "[0m";
                    //string msapeth3 = msapeth2.Replace(msapeth1, "");
                    //string msapeth4 = "[0;0m";
                    //string msapeth5 = msapeth3.Replace(msapeth4, "");
                    //string msapeth6 = "[0;31m";
                    //string msapeth7 = msapeth5.Replace(msapeth6, "");
                    //textBox3.Text = newkou2;
                    textcurrent.AppendText(ss);
                    //this.textDOS.Text = ss;
                }
            }
            else
            {
                textcurrent.AppendText("\r\n" + "连接通信故障，请断开后，重新尝试！");
                //this.butlogin.PerformClick();
            }
            textguzhangmingling.Text = "";
            textcurrent.Focus();
            textcurrent.ScrollToCaret();
            textguzhangmingling.Focus();
            // this.richTextEnd.Select(this.richTextEnd.TextLength, 0);
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
                textguzhangmingling.Text = textcyclemingling.Text;
                butguzhangsend.PerformClick();
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
                // butlogin.Text = "①连接设备";
                // mysocket.Close();

                for (int g = 0; g <= 100; g++)
                {
                    if (textcurrent.Text.Contains("Ctrl+c"))
                    {
                        butguzhangsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }
                    //Thread.Sleep(XHTime/10);
                }
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
                textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块已卸载==============================================OK" + "\r\n");

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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块下载中==============================================OK" + "\r\n");
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
                    // textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +" "+percent.ToString() + "\r\n");
                    toolStripStatusLabjindu.Text = percent.ToString() + "%";
                    metroProgressBar.Value = percent;
                    // System.Windows.Forms.Application.DoEvents();
                }
                stream.Close();
                responseStream.Close();
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块下载成功============================================OK" + "\r\n");
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块保存路径：" + strZipPath + "\r\n");

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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块解压成功============================================OK" + "\r\n");
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
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块安装成功============================================OK" + "\r\n");
            // MessageBox.Show("GPN7600 EMS模块已安装成功！");
        }
        #region 获取GPN7600EMS软件目录
        private void gpnurlupdate()
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
            if (pingReply.Status != IPStatus.Success)
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "GPN模块获取失败，请链接格林威尔VPN后，再次尝试！" + "\r\n");
                return;
            }
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
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "获取网管版本" + "===============================================OK " + node.Count.ToString() + "个版本" + "\r\n");
            for (int i = 0; i < node.Count; i++)
            {
                comgpn76list.Items.Add(node[i].InnerText);
            }
            //textDOS.AppendText("从网管服务器获取GPN76模块链接成功==========================OK" + "\r\n");
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
        private void com760a_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + com760a.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                lab760a.Text = lSize.ToString();
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
        private void comotnpack_SelectedIndexChanged(object sender, EventArgs e)
        {
            long lSize = 0;
            string sFullName = @tbxFtpRoot.Text.Trim() + comotnpack.Text.Trim();
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
                labotnpack.Text = lSize.ToString();
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
        #region 下载配置按钮
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查conf_data.txt文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查conf_data.txt文件比对==============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：conf_data.txt文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：conf_data.txt文件大小为： " + labconfig.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查conf_data.txt文件比对=============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：conf_data.txt文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：conf_data.txt文件大小为： " + labconfig.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查slotconfig.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查slotconfig.bin文件比对=============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：slotconfig.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：slotconfig.bin文件大小为： " + labslotconfig.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查slotconfig.bin文件比对============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：slotconfig.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：slotconfig.bin文件大小为： " + labslotconfig.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查db.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查db.bin文件比对=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：db.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：db.bin文件大小为： " + labdb.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查db.bin文件比对====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：db.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：db.bin文件大小为： " + labdb.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查app_code.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查app_code.bin文件比对===============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：app_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：app_code.bin文件大小为： " + labapp.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查app_code.bin文件比对==============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：app_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：app_code.bin文件大小为： " + labapp.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查nms.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查nms.fpga文件比对===================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：nms.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：nms.fpga文件大小为： " + labnms.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查nms.fpga文件比对==================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：nms.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：nms.fpga文件大小为： " + labnms.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sw.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sw.fpga文件比对====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：sw.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：sw.fpga文件大小为： " + labsw.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sw.fpga文件比对===================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：sw.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：sw.fpga文件大小为： " + labsw.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查fpga_code.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查fpga_code.bin文件比对==============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：fpga_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：fpga_code.bin文件大小为： " + labcode.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查fpga_code.bin文件比对=============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：fpga_code.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：fpga_code.bin文件大小为： " + labcode.Text + " 字节" + "\r\n");
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
        private void Fpga760aSize()
        {
            toolStripStatusLabelzt.Text = "检查FPGA760A大小中";
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
            string appRegex = ".*760a.fpga";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760a.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                if (Appsize == lab760a.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760a.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760a.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760a.fpga文件大小为： " + lab760a.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760a.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760a.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760a.fpga文件大小为： " + lab760a.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760b.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760b.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760b.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760b.fpga文件大小为： " + lab760b.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760b.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760b.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760b.fpga文件大小为： " + lab760b.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760c.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760c.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760c.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760c.fpga文件大小为： " + lab760c.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760c.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760c.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760c.fpga文件大小为： " + lab760c.Text + " 字节" + "\r\n");
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
            toolStripStatusLabelzt.Text = "检查FPGA760A大小中";
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760d.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760d.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760d.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760d.fpga文件大小为： " + lab760d.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760d.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760d.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760d.fpga文件大小为： " + lab760d.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760e.fpga文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760e.fpga文件比对==================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760e.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760e.fpga文件大小为： " + lab760e.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查760e.fpga文件比对=================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：760e.fpga文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：760e.fpga文件大小为： " + lab760e.Text + " 字节" + "\r\n");
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
        private void OtnPackSize()
        {
            toolStripStatusLabelzt.Text = "检查OtnPack大小中";
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
            string appRegex = ".*pack.bin";
            Regex r = new Regex(appRegex, RegexOptions.IgnoreCase);
            string appsize1 = r.Match(ver).Groups[0].Value;
            if (appsize1 == "")
            {
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查otn_pack.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                if (Appsize == labotnpack.Text)
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查otn_pack.bin文件比对===============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：otn_pack.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：otn_pack.bin文件大小为： " + labotnpack.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查otn_pack.bin文件比对==============================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：otn_pack.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：otn_pack.bin文件大小为： " + labotnpack.Text + " 字节" + "\r\n");
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
                textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sysfile_ini.bin文件大小超时，请检查文件是否存在，设备是否正常？==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sysfile_ini.bin文件比对============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：sysfile_ini.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：sysfile_ini.bin文件大小为： " + labsysfile.Text + " 字节" + "\r\n");
                }
                else
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "检查sysfile_ini.bin文件比对===========================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "设备：sysfile_ini.bin文件大小为： " + Appsize + " 字节" + "\r\n");
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "电脑：sysfile_ini.bin文件大小为： " + labsysfile.Text + " 字节" + "\r\n");
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
                check760a.Checked == false &&
                check760b.Checked == false &&
                check760c.Checked == false &&
                check760d.Checked == false &&
                check760e.Checked == false &&
                checkotnpack.Checked == false &&
                checksysfile.Checked == false &&
                checkconfig.Checked == false &&
                checkdb.Checked == false &&
                checkslotconfig.Checked == false)
            {
                MessageBox.Show("请勾选文件后进行比较！");
                return;
            }
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
            if (check760a.Checked == true)
            {
                Fpga760aSize();
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
            if (checkotnpack.Checked == true)
            {
                OtnPackSize();
            }
            if (checksysfile.Checked == true)
            {
                SysfileSize();
            }
            butsend.PerformClick();
        }
        bool UpLoadFile_Stop = true;
        bool UpLoadFile_On_Off = false;
        ManualResetEvent UpLoadFilePause;
        Thread UpLoadFileThread;
        private void butupload_Click(object sender, EventArgs e)
        {
            if (butupload.Text == "⑤上传备份")
            {
                if (string.Compare(btnFtpServerStartStop.Text, "③启动FTP服务器") == 0)
                {
                    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
                    return;
                }
                if (checkapp.Checked == false &&
                    checkcode.Checked == false &&
                    checknms.Checked == false &&
                    checksw.Checked == false &&
                    check760a.Checked == false &&
                    check760b.Checked == false &&
                    check760c.Checked == false &&
                    check760d.Checked == false &&
                    checkotnpack.Checked == false &&
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
                butupload.Text = "⑤停止备份";
                //textcurrent.AppendText("\r\n开始运行！");
            }
            else
            {
                UpLoadFile_Stop = true;
                butupload.Text = "⑤上传备份";
            }
        }
        private void UpLoadFile()
        {
            //立即开始计时，时间间隔1000毫秒
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            uploading = true;
            Testftpser();
            if (DownLoadFile_Stop)
            {
                textDOS.AppendText(DateTime.Now.ToString("\r\n" + "yyyy-MM-dd HH:mm:ss.fff") + " " + "下载升级已停止！");
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
            if (check760a.Checked == true)
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
            if (checkotnpack.Checked == true)
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            if (check760a.Checked == true)
            {
                Fpga760aSize();
                Upload760a();
                if (s == p)
                {
                    if (UpLoadFile_Stop)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            if (checkotnpack.Checked == true)
            {
                OtnPackSize();
                UploadOtnPack();
                if (s == p)
                {
                    if (UpLoadFile_Stop)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                        return;
                    }
                    if (UpLoadFile_On_Off)
                    {
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                        UpLoadFilePause = new ManualResetEvent(false);
                        UpLoadFilePause.WaitOne();
                    }
                    metroProgressBar.Value = p;
                    toolStripStatusLabelbar.Text = p + "%";
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
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                    }
                    else
                    {
                        if (UpLoadFile_Stop)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "已停止！\r\n");
                            return;
                        }
                        if (UpLoadFile_On_Off)
                        {
                            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "暂停中！\r\n");
                            UpLoadFilePause = new ManualResetEvent(false);
                            UpLoadFilePause.WaitOne();
                        }
                        metroProgressBar.Value = p;
                        toolStripStatusLabelbar.Text = p + "%";
                        System.Threading.Thread.Sleep(XHTime);
                        p = s + p;
                    }
                }
            }
            Thread.Sleep(XHTime);
            string canyu = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "已完成";
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份结束" + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
            butsend.PerformClick();
            UpLoadFile_Stop = true;
            butupload.Text = "⑤上传备份";
            Mytimer.Change(Timeout.Infinite, 1000);
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(failed))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================上传config文件失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "============================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void Upload760a()
        {
            toolStripStatusLabelzt.Text = "正在备份fpga760a文件";             //上传状态栏显示
            string strname = lab760aname.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/760a.fpga ";      //上传文件名
            string uploadfilenamesave = "760a.fpga";                       //上传后命名
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(xunhuantime);
            }
        }
        private void UploadOtnPack()
        {
            toolStripStatusLabelzt.Text = "正在备份OtnPack文件";             //上传状态栏显示
            string strname = labOtnPackName.Text;                            //上传文件类型
            int xunhuantime = 10;                                           //循环时间
            int xunhuancishu = 10000;                                        //循环次数           
            string uploadfilename = " file /yaffs/sys/otn_pack.bin ";      //上传文件名
            string uploadfilenamesave = "otn_pack.bin";                       //上传后命名
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=====================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "===========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "============================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "==========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(uploadfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "================================文件上传失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                        textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                        break;
                    }
                }
                if (box.Contains(serveron))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=================请检查FTP服务器IP是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(user))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains(foundfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "=========================设备内不存在该文件==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(uploadfile))
                {
                    textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "备份" + strname + "================================文件上传失败==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Flash进度===========================================O%");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Flash ==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Flash=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Flash=======================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Yaffs文件大小约有528MB,大约需要20分钟，请耐心等待！" + toolStripStatusLabeltime.Text + "\r\n");
            textDOS.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Yaffs进度===========================================O%");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Yaffs==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Yaffs=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    butCycleStart.Text = "⑤开始备份";
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "上传Yaffs=======================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "保存配置===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains("erro"))
                {
                    textDOS.AppendText("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "保存配置==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
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
                    + "\r\n" + com760a.Text + ":  " + check760a.Checked
                    + "\r\n" + com760b.Text + ":  " + check760b.Checked
                    + "\r\n" + com760c.Text + ":  " + check760c.Checked
                    + "\r\n" + com760d.Text + ":  " + check760d.Checked
                    + "\r\n" + com760e.Text + ":  " + check760e.Checked
                    + "\r\n" + comotnpack.Text + ":  " + checkotnpack.Checked
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
            string host = metroTextgpnip.Text;
            string community = metroTextReadCommunity.Text;
            SimpleSnmp snmp = new SimpleSnmp(host, community);
            if (!snmp.Valid)
            {
                MessageBox.Show ("SNMP主机IP地址错误或者读写团体错误");
                return;
            }
            Dictionary<Oid, AsnType> result = snmp.Get(SnmpVersion.Ver1, new string[] { metroTextoid.Text });
            if (result == null)
            {
                MessageBox.Show("请求后未收到回复");
                return;
            }
            foreach (KeyValuePair<Oid, AsnType> kvp in result)
            {

                ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1)+"");
                item.SubItems.Add(host);
                item.SubItems.Add(kvp.Key.ToString());
                item.SubItems.Add(SnmpConstants.GetTypeName(kvp.Value.Type));
                item.SubItems.Add(kvp.Value.ToString());
                item.EnsureVisible();




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
            if (devtype == "")
            {
                return;
            }
            // SNMP团体名称 
            OctetString community = new OctetString(textReadCommunity.Text);
            //定义代理参数类 
            AgentParameters param = new AgentParameters(community);
            //将SNMP版本设置为1（或2） 
            param.Version = SnmpVersion.Ver1;
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

                MessageBox.Show("没有收到SNMP请求后的响应！");
            }
            catch
            {
                MessageBox.Show("请检查Oid项配置信息！");
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
                    textDOS.Text += string.Format("\r\n" + "SNMP回复错误！错误 {0} 第 {1} 项\r\n",
                            result.Pdu.ErrorStatus,
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
            AgentParameters param = new AgentParameters(community);
            //将SNMP版本设置为1（或2） 
            param.Version = SnmpVersion.Ver1;
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
                    //代理报告与所述请求的错误 
                    MessageBox.Show(String.Format( "SNMP回复错误！错误代码 {0} 。错误行数：第 {1} 行\r\n",
                            result.Pdu.ErrorStatus,
                            result.Pdu.ErrorIndex));
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
            AgentParameters aparam = aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(metroTextSetCommunity.Text));


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
                    MessageBox.Show(String.Format("SNMP返回错误状态: {0} on 第 {1}行",
                        response.Pdu.ErrorStatus, response.Pdu.ErrorIndex));
                }
                else
                {
                    // Everything is ok. Agent will return the new value for the OID we changed
                    ListViewItem item = lv2.Items.Add((lv2.Items.Count + 1) + "");
                    item.SubItems.Add(metroTextgpnip.Text);
                    item.SubItems.Add(response.Pdu[0].Oid.ToString());
                    item.SubItems.Add(SnmpConstants.GetTypeName(response.Pdu[0].Value.Type));
                    item.SubItems.Add(response.Pdu[0].Value.ToString());
                    item.EnsureVisible();
                    target.Close();
                    
                }
            }
        }
    }
}