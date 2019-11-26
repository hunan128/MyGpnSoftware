using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class GPN : Form
    {
        #region 全局变量的声明
        private MySocket mysocket;

        public static string com = "";                  //
        public static string ts = "10";
        public static string slot18 = "";               //18槽位状态
        public static string slot11 = "";               //11槽位状态
        public static string slotsw = "";               //SW型号状态
        public static string slot17 = "";               //17槽位状态
        public static string slot12 = "";               //12槽位状态
        public static string sw = "";                   //SW型号状态
        string defaultfilePath = "";                    //打开文件夹默认路径
        public static string version = "";              //设备版本号
        TcpListener myTcpListener = null;
        private Thread listenThread;

        // 保存户名和密码
        Dictionary<string, string> users;
        #endregion
        public GPN()
        {
            InitializeComponent();
            mysocket = new MySocket();

            #region 一键升级的线程
            /////一键升级的线程
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            #endregion
            #region 一键备份配置的线程

            /////一键备份配置的线程
            backgroundWorker2.WorkerReportsProgress = true;
            backgroundWorker2.WorkerSupportsCancellation = true;
            backgroundWorker2.DoWork += new DoWorkEventHandler(backgroundWorker2_DoWork);
            backgroundWorker2.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker2_ProgressChanged);
            backgroundWorker2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);
            #endregion
            #region 一键下载配置的线程
            /////一键下载配置的线程
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.WorkerSupportsCancellation = true;
            backgroundWorker3.DoWork += new DoWorkEventHandler(backgroundWorker3_DoWork);
            backgroundWorker3.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker3_ProgressChanged);
            backgroundWorker3.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker3_RunWorkerCompleted);
            #endregion
            #region 打印网卡的ip地址
            ////打印网卡的ip地址
            NetworkInterface[] NetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface NetworkIntf in NetworkInterfaces)
            {
                IPInterfaceProperties IPInterfaceProperties = NetworkIntf.GetIPProperties();
                UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = IPInterfaceProperties.UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
                {
                    if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        string ftpipall = UnicastIPAddressInformation.Address.ToString();
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
            if (File.Exists(strFilePath))//读取时先要判读INI文件是否存在
            {

                strSec = Path.GetFileNameWithoutExtension(strFilePath);
                comftpip.Text = ContentValue(strSec, "FTPip");
                tbxFtpServerPort.Text = ContentValue(strSec, "FTPport");
                textftpusr.Text = ContentValue(strSec, "FTPuser");
                textftppsd.Text = ContentValue(strSec, "FTPpsd");
                tbxFtpRoot.Text = ContentValue(strSec, "FTPpath");
                if (ContentValue(strSec, "GPNip") != "")
                {
                    textip.Text = ContentValue(strSec, "GPNip");

                }

                textusr.Text = ContentValue(strSec, "GPNuser");
                textpsd.Text = ContentValue(strSec, "GPNpsd");
                if (Directory.Exists(tbxFtpRoot.Text))
                {
                    DirectoryInfo dir = new DirectoryInfo(tbxFtpRoot.Text);
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
                        if (s.Contains("sysfile") || s.Contains("Sysfile") || s.Contains("SYSFILE"))
                        {
                            comsysfile.Items.Add(s);
                            if (comsysfile.Items.Count > 0)
                            {
                                comsysfile.SelectedIndex = comsysfile.Items.Count - 1;
                            }
                        }
                    }
                }
                else
                {

                }

            }
            else
            {

            }
            #endregion
        }
        #region ③启动FTP服务器
        // 启动服务器
        private void btnFtpServerStartStop_Click(object sender, EventArgs e)
        {
            if (textftpusr.Text == "" || textftppsd.Text == "")
            {
                MessageBox.Show("请填写用户名密码后，点击③启动FTP服务器！");
                return;
            }
            if (tbxFtpRoot.Text == "")
            {
                MessageBox.Show("请选择FTP目录后，点击③启动FTP服务器！");
            }
            else
            {
                if (myTcpListener == null)
                {
#pragma warning disable CS0219 // 变量“inUse”已被赋值，但从未使用过它的值
                    bool inUse = false;
#pragma warning restore CS0219 // 变量“inUse”已被赋值，但从未使用过它的值

                    IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                    IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();


                    foreach (IPEndPoint endPoint in ipEndPoints)
                    {
                        int A = int.Parse(tbxFtpServerPort.Text);
                        if (endPoint.Port == A)
                        {
                            inUse = true;
                            MessageBox.Show(A + "端口已占，主机已经开启FTP服务器，请检查！");
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
                    btnFtpServerStartStop.Text = "③停止FTP服务器";

                }
                else
                {
                    myTcpListener.Stop();
                    myTcpListener = null;
                    listenThread.Abort();
                    lstboxStatus.Items.Add("FTP服务器已停止!--------------FTP服务器已停止!");
                    //lstboxStatus.TopIndex = lstboxStatus.Items.Count - 1;
                    btnFtpServerStartStop.Text = "③启动FTP服务器";
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
                Thread.Sleep(10);
            }
            if (pingReply.Status == IPStatus.Success)
            {

                myTcpListener = new TcpListener(IPAddress.Parse(comftpip.Text), int.Parse(tbxFtpServerPort.Text));
                // 开始监听传入的请求
                myTcpListener.Start();
                AddInfo("③启动FTP服务器成功!--------------③启动FTP服务器成功!");
                //AddInfo("开始监听用户端请求....");
                //          AddInfo("Ftp服务器运行中...[点击”停止“按钮停止FTP服务]");
                while (true)
                {
                    try
                    {
                        // 接收连接请求
                        TcpClient tcpClient = myTcpListener.AcceptTcpClient();

                        AddInfo(string.Format("客户端（{0}）与本机（{1}）建立FTP连接", tcpClient.Client.RemoteEndPoint, myTcpListener.LocalEndpoint));
                        User user = new User();
                        user.commandSession = new UserSeesion(tcpClient);
                        user.workDir = tbxFtpRoot.Text;
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
                lstboxStatus.Items.Add("FTP服务启动失败!--------------FTP服务启动失败!");
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
                    receiveString = user.commandSession.streamReader.ReadLine();
                }
                catch (Exception ex)
                {
                    if (user.commandSession.tcpClient.Connected == false)
                    {
                        AddInfo(string.Format("客户端({0}断开连接！)", user.commandSession.tcpClient.Client.RemoteEndPoint));
                    }
                    else
                    {
                        AddInfo("接收命令失败！" + ex.Message);
                    }

                    break;
                }

                if (receiveString == null)
                {
                    AddInfo("接收字符串为null,结束线程！");
                    break;
                }

                AddInfo(string.Format("来自{0}：[{1}]", user.commandSession.tcpClient.Client.RemoteEndPoint, receiveString));

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
                    user.commandSession.Close();
                    return;
                }
                else
                {
                    switch (user.loginOK)
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
                                    user.commandSession.Close();
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
                user.commandSession.streamWriter.WriteLine(str);
                AddInfo(string.Format("向客户端（{0}）发送[{1}]", user.commandSession.tcpClient.Client.RemoteEndPoint, str));
            }
            catch
            {
                AddInfo(string.Format("向客户端（{0}）发送信息失败", user.commandSession.tcpClient.Client.RemoteEndPoint));
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
        #region 处理各个命令

        #region 登录过程，即户身份验证过程
        // 处理USER命令，接收户名但不进行验证
        private void CommandUser(User user, string command, string param)
        {
            string sendString = string.Empty;
            if (command == "USER")
            {
                sendString = "331 USER command OK, password required.";
                user.userName = param;
                // 设置loginOk=1为了确保后面紧接的要求输入密码
                // 1表示已接收到户名，等到接收密码
                user.loginOK = 1;
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
                if (users.TryGetValue(user.userName, out password))
                {
                    if (password == param)
                    {
                        sendString = "230 User logged in success";
                        // 2表示登录成功
                        user.loginOK = 2;
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
            user.currentDir = user.workDir;
        }

        #endregion

        #region 文件管理命令
        private void CommandQUIT(User user, string temp)
        {
            user.commandSession.Close();
            return;

        }
        // 处理CWD命令，改变工作目录
        private void CommandCWD(User user, string temp)
        {
            string sendString = string.Empty;
            try
            {
                string dir = user.workDir.TrimEnd('/') + temp;

                // 是否为当前目录的子目录，且不包含父目录名称
                if (Directory.Exists(dir))
                {
                    user.currentDir = dir;
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
            sendString = "257 '" + user.currentDir + "' is the current directory";
            RepleyCommandToUser(user, sendString);
        }

        // 处理LIST/NLIST命令，想客户端发送当前或指定目录下的所有文件名和子目录名
        private void CommandLIST(User user, string parameter)
        {
            string sendString = string.Empty;
            DateTimeFormatInfo dateTimeFormat = new CultureInfo("en-US", true).DateTimeFormat;

            // 得到目录列表
            string[] dir = Directory.GetDirectories(user.currentDir);
            if (string.IsNullOrEmpty(parameter) == false)
            {
                if (Directory.Exists(user.currentDir + parameter))
                {
                    dir = Directory.GetDirectories(user.currentDir + parameter);
                }
                else
                {
                    string s = user.currentDir.TrimEnd('/');
                    user.currentDir = s.Substring(0, s.LastIndexOf("/") + 1);
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
            string[] files = Directory.GetFiles(user.currentDir);
            if (string.IsNullOrEmpty(parameter) == false)
            {
                if (Directory.Exists(user.currentDir + parameter + "/"))
                {
                    files = Directory.GetFiles(user.currentDir + parameter + "/");
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
            bool isBinary = user.isBinary;
            user.isBinary = false;
            RepleyCommandToUser(user, "150 Opening ASCII data connection");
            InitDataSession(user);
            SendByUserSession(user, sendString);
            RepleyCommandToUser(user, "226 Transfer complete");
            user.isBinary = isBinary;
        }

        // 处理RETR命令，提供下载功能，将户请求的文件发送给户
        private void CommandRETR(User user, string filename)
        {
            string sendString = "";

            // 下载的文件全名
            string path = user.currentDir + filename;
            FileStream filestream = new FileStream(path, FileMode.Open, FileAccess.Read);

            // 发送150到户，表示服务器文件状态良好，将要打开数据连接传输文件
            if (user.isBinary)
            {
                sendString = "150 Opening BINARY mode data connection for download";
            }
            else
            {
                sendString = "150 Opening ASCII mode data connection for download";
            }

            RepleyCommandToUser(user, sendString);
            InitDataSession(user);
            SendFileByUserSession(user, filestream);
            RepleyCommandToUser(user, "226 Transfer complete");
        }

        // 处理STOR命令，提供上传功能，接收客户端上传的文件
        private void CommandSTOR(User user, string filename)
        {
            string sendString = "";
            // 上传的文件全名
            string path = user.currentDir + filename;
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);

            // 发送150到户，表示服务器状态良好
            if (user.isBinary)
            {
                sendString = "150 Opening BINARY mode data connection for upload";
            }
            else
            {
                sendString = "150 Opeing ASCII mode data connection for upload";
            }

            RepleyCommandToUser(user, sendString);
            InitDataSession(user);
            ReadFileByUserSession(user, fs);
            RepleyCommandToUser(user, "226 Transfer complete");
        }

        // 处理DELE命令，提供删除功能，删除服务器上的文件
        private void CommandDELE(User user, string filename)
        {
            string sendString = "";

            // 删除的文件全名
            string path = user.currentDir + filename;
            AddInfo("正在删除文件" + filename + "...");
            File.Delete(path);
            AddInfo("删除成功");
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
                    user.dataListener = new TcpListener(localip, port);
                    AddInfo("TCP 数据连接已打开（被动模式）--" + localip.ToString() + "：" + port);
                }
                catch
                {
                    continue;
                }

                user.isPassive = true;
                string temp = localip.ToString().Replace('.', ',');

                // 必须把端口号IP地址告诉客户端，客户端接收到响应命令后，
                // 再通过新的端口连接服务器的端口P，然后进行文件数据传输
                sendString = "227 Entering Passive Mode(" + temp + "," + random1 + "," + random2 + ")";
                RepleyCommandToUser(user, sendString);
                user.dataListener.Start();
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
            user.remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipString), portNum);
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
                user.isBinary = true;
                sendstring = "220 Type set to I(Binary)";
            }
            else
            {
                // ASCII方式
                user.isBinary = false;
                sendstring = "330 Type set to A(ASCII)";
            }

            RepleyCommandToUser(user, sendstring);
        }

        #endregion

        #endregion
        #region 初始化数据连接
        // 初始化数据连接
        private void InitDataSession(User user)
        {
            TcpClient client = null;
            if (user.isPassive)
            {
                AddInfo("采被动模式返回LIST目录和文件列表");
                client = user.dataListener.AcceptTcpClient();
            }
            else
            {
                AddInfo("采主动模式向户发送LIST目录和文件列表");
                client = new TcpClient();
                client.Connect(user.remoteEndPoint);
            }

            user.dataSession = new UserSeesion(client);
        }
        #endregion
        #region 使数据连接发送字符串
        // 使数据连接发送字符串
        private void SendByUserSession(User user, string sendString)
        {
            AddInfo("向户发送(字符串信息)：[" + sendString + "]");
            try
            {
                user.dataSession.streamWriter.WriteLine(sendString);
                AddInfo("发送完毕");
            }
            finally
            {
                user.dataSession.Close();
            }
        }
        #endregion
        #region 使数据连接发送文件流
        // 使数据连接发送文件流（客户端发送下载文件命令）
        private void SendFileByUserSession(User user, FileStream fs)
        {
            AddInfo("向户发送(文件流)：[..........................");
            try
            {
                if (user.isBinary)
                {
                    byte[] bytes = new byte[1024];
                    BinaryReader binaryReader = new BinaryReader(fs);
                    int count = binaryReader.Read(bytes, 0, bytes.Length);
                    while (count > 0)
                    {
                        user.dataSession.binaryWriter.Write(bytes, 0, count);
                        user.dataSession.binaryWriter.Flush();
                        count = binaryReader.Read(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    StreamReader streamReader = new StreamReader(fs);
                    while (streamReader.Peek() > -1)
                    {
                        user.dataSession.streamWriter.WriteLine(streamReader.ReadLine());
                    }
                }

                AddInfo("......................................]发送完毕！");
            }
            finally
            {
                user.dataSession.Close();
                fs.Close();
            }
        }
        #endregion
        #region 使数据连接接收文件流
        // 使数据连接接收文件流(客户端发送上传文件功能)
        private void ReadFileByUserSession(User user, FileStream fs)
        {
            AddInfo("接收户上传数据（文件流）：[.........................");
            try
            {
                if (user.isBinary)
                {
                    byte[] bytes = new byte[1024];
                    BinaryWriter binaryWriter = new BinaryWriter(fs);
                    int count = user.dataSession.binaryReader.Read(bytes, 0, bytes.Length);
                    while (count > 0)
                    {
                        binaryWriter.Write(bytes, 0, count);
                        binaryWriter.Flush();
                        count = user.dataSession.binaryReader.Read(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    StreamWriter streamWriter = new StreamWriter(fs);
                    while (user.dataSession.streamReader.Peek() > -1)
                    {
                        streamWriter.Write(user.dataSession.streamReader.ReadLine());
                        streamWriter.Flush();
                    }
                }

                AddInfo(".............................................]接收完毕");
            }
            finally
            {
                user.dataSession.Close();
                fs.Close();
            }
        }
        #endregion

        #region 备份数据库
        //这里就是通过响应消息，来处理界面的显示工作
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //string state = (string)e.UserState;//接收ReportProgress方法传递过来的userState
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
            this.toolStripStatusLabelbar.Text = Convert.ToString(e.ProgressPercentage) + "%";
            tabPage1.Text = textip.Text + " " + toolStripStatusLabelbar.Text;
        }
        //这里是后台工作完成后的消息处理，可以在这里进行后续的处理工作。
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
            {   //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "备份成功";
                MessageBox.Show("备份数据库成功!");
            }
            else
            {
                //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "备份失败!";
                MessageBox.Show("备份失败请检查!");
            }
        }
        //这里，就是后台进程开始工作时，调工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            //立即开始计时，时间间隔1000毫秒
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            Save(e);
            for (int p = 0; p <= 40; p++)
            {
                if (backgroundWorker2.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker2.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(10);
                }
            }

            Backup(e);

            for (int p = 40; p <= 100; p++)
            {
                if (backgroundWorker2.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker2.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(10);
                }
            }
            this.butsend.PerformClick();
            

        }
        #endregion

        #region 下载数据库

        //这里就是通过响应消息，来处理界面的显示工作
        private void backgroundWorker3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //string state = (string)e.UserState;//接收ReportProgress方法传递过来的userState
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
            this.toolStripStatusLabelbar.Text = Convert.ToString(e.ProgressPercentage) + "%";
            tabPage1.Text = textip.Text + " " + toolStripStatusLabelbar.Text;
        }
        //这里是后台工作完成后的消息处理，可以在这里进行后续的处理工作。
        private void backgroundWorker3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
            {   //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "下载成功";
            }
            else
            {
                //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "下载失败!";
                MessageBox.Show("恢复失败请检查!");
            }
        }
        //这里，就是后台进程开始工作时，调工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //立即开始计时，时间间隔1000毫秒
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            for (int p = 10; p <= 30; p++)
            {
                if (backgroundWorker3.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker3.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(100);
                }
            }
            toolStripStatusLabelzt.Text = "正在下载config文件";
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("download ftp file /flash/sys/conf_data.txt " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comconfig.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + "下载config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + "下载config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                    
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + "下载config========================请检查FTP户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                    
                }
                Thread.Sleep(10);

            }
            for (int p = 30; p <= 60; p++)
            {
                if (backgroundWorker3.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker3.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(100);
                }
            }
            //Thread.Sleep(1000);
            toolStripStatusLabelzt.Text = "正在下载slotconfig文件";
            mysocket.SendData("download ftp file /flash/sys/slotconfig.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comslotconfig.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载lsotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);

            }
            for (int p = 60; p <= 80; p++)
            {
                if (backgroundWorker3.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker3.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(100);
                }
            }
            //Thread.Sleep(1000);
            toolStripStatusLabelzt.Text = "正在下载db文件";
            mysocket.SendData("download ftp file /flash/sys/db.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comdb.Text);
            for (int i = 1; i <= 1000; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string db = "db.bin";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    for (int a = 1; a <= 1000; a++)
                    {
                        string box2 = mysocket.ReceiveData(int.Parse(ts));

                        if (box2.Contains(db))
                        {
                            textDOS.AppendText("下载db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(10);

                    }

                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);

            }
            for (int p = 80; p <= 100; p++)
            {
                if (backgroundWorker3.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    backgroundWorker3.ReportProgress(p, "Working");
                    System.Threading.Thread.Sleep(100);
                }
            }
            Reboot(e);
        }

        #endregion

        #region 一键升级开始按钮
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        private void butupgrade_Click(object sender, EventArgs e)
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
                    checksysfile.Checked == false &&
                    check760e.Checked == false)
                {
                    MessageBox.Show("请勾选升级文件后继续！");
                    return;
                }




                //DialogResult dr = MessageBox.Show("升级过程中不要点击停止按钮，只支持GPN系列设备，确认升级设备吗？", "提示", MessageBoxButtons.YesNo);
                //if (dr == DialogResult.Yes)
                //{
                    mysocket.SendData("Y");
                    this.butupgrade.Text = "④停止升级";
                    this.toolStripProgressBar1.Maximum = 100;
                    this.backgroundWorker1.RunWorkerAsync();
                    //户选择确认的操作
               // }
                //else if (dr == DialogResult.No)
                //{
                    //户选择取消的操作
                   // mysocket.SendData("N");
                   // return;
                //}
            }
            else {
                Mytimer.Change(Timeout.Infinite, 1000);
                this.butupgrade.Enabled = true;
                backgroundWorker1.CancelAsync();
                backgroundWorker2.CancelAsync();
                backgroundWorker3.CancelAsync();
                this.butupgrade.Text = "④下载升级";
                textDOS.AppendText("\r\n" + "下载升级已停止！");

            }
        }
        #endregion

        #region 定时建立telnet连接
        private void timer2_Tick(object sender, EventArgs e)
        {
            textDOS.AppendText(mysocket.ReceiveData(int.Parse(ts)));
        }
        #endregion


        #region 一键升级内容
        #region 升级过程中显示工作
        //这里就是通过响应消息，来处理界面的显示工作

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //string state = (string)e.UserState;//接收ReportProgress方法传递过来的userState
            this.toolStripProgressBar1.Value = e.ProgressPercentage;
            this.toolStripStatusLabelbar.Text = Convert.ToString(e.ProgressPercentage) + "%";
            tabPage1.Text = textip.Text + " " + toolStripStatusLabelbar.Text;
        }
        #endregion

        #region 升级后操作
        //这里是后台工作完成后的消息处理，可以在这里进行后续的处理工作。
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
            {   //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "升级成功";
            }
            else
            {
                //停止计时
                Mytimer.Change(Timeout.Infinite, 1000);
                this.toolStripStatusLabelzt.Text = "升级失败!";
            }
        }
        #endregion


        #region 正式升级
        //这里，就是后台进程开始工作时，调工作函数的地方。你可以把你现有的处理函数写在这儿。
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //立即开始计时，时间间隔1000毫秒
            TimeCount = 0;
            Mytimer.Change(0, 1000);
            Control.CheckForIllegalCrossThreadCalls = false;
            Testftpser(e);
            if (e.Cancel == true)
            {
                e.Cancel = true;
                return;

            }
            Thread.Sleep(2000);
            Save(e);

            int a = 0;
            int p = 0;
            backgroundWorker1.ReportProgress(p, "Working");

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
            if (checksysfile.Checked == true)
            {
                a++;
            }

            int s = (int)Math.Floor((double)100 / a);
            p = (int)Math.Floor((double)100 / a);

            if (checkapp.Checked == true)
            {
                Rm(e);
                App(e);
                
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (checkcode.Checked == true)
            {
                Fpgacode(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (checknms.Checked == true)
            {
                Nms(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (checksw.Checked == true)
            {
                Swfpga(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (check760a.Checked == true)
            {
                Fpga760a(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (check760b.Checked == true)
            {
                Fpga760b(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (check760c.Checked == true)
            {
                Fpga760c(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (check760d.Checked == true)
            {
                Fpga760d(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (check760e.Checked == true)
            {
                Fpga760e(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }
            if (checksysfile.Checked == true)
            {
                Sysfile(e);
                if (s == p)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        backgroundWorker1.ReportProgress(p, "Working");
                        System.Threading.Thread.Sleep(100);
                        p = s + p;
                    }
                }
                else
                {

                    if (p > 95 && p <= 100)
                    {
                        p = 100;
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        if (backgroundWorker1.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        else
                        {
                            backgroundWorker1.ReportProgress(p, "Working");
                            System.Threading.Thread.Sleep(100);
                            p = s + p;
                        }
                    }

                }

            }


            Thread.Sleep(100);
            string canyu = mysocket.ReceiveData(int.Parse(ts));
            toolStripStatusLabelzt.Text = "已完成";

            Reboot(e);
            this.butupgrade.Enabled = true;
            backgroundWorker1.CancelAsync();

        }

        #endregion
        #endregion

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
        #region 连接断开按钮
        private void butlogin_Click(object sender, EventArgs e)
        {


            if (string.Compare(butlogin.Text, "①断开设备") == 0)
            {
                butlogin.Text = "①连接设备";
                textip.Enabled = true;
                textcom.Enabled = false;
                butsend.Enabled = false;
                butupgrade.Text = "④下载升级";
                butupgrade.Enabled = false;
                butbackup.Enabled = false;
                butdownload.Enabled = false;
                toolStripStatusLabelnms.Text = "17槽：无";
                toolStripStatusLabelnms18.Text = "18槽：无";
                toolStripStatusLabelswa11.Text = "11槽：无";
                toolStripStatusLabelswa12.Text = "12槽：无";
                toolStripStatusLabelver.Text = "版本：无";
                slot18 = "";               //18槽位状态
                slot11 = "";                //11槽位状态
                sw = "";                    //SW型号状态
                slot17 = "";               //17槽位状态
                slot12 = "";               //12槽位状态
                version = "";              //设备版本号
                textDOS.AppendText("\r\n" + "已断开=================================================OK");
                this.AcceptButton = butlogin;
                mysocket.Close();
                toolStripStatusLabellinkstat.Text = "未连接";
                return;
            }
            if (string.Compare(butlogin.Text, "①连接设备") == 0)
            {
                if (!IsIP(textip.Text.Trim()))
                {
                    MessageBox.Show("您输入了非法IP地址，请修改后再次尝试！");
                    return;

                }
                Ping ping = new Ping();
                int timeout = 120;
                PingReply pingReply = ping.Send(textip.Text, timeout);

                if (pingReply.Status == IPStatus.Success)
                {
                    textDOS.AppendText("\r\n" + "设备可以ping通，正在尝试Telnet登录，请稍等...");
                    if (mysocket.Connect(textip.Text.Trim(), "23"))
                    {
                        butlogin.Text = "①断开设备";
                        textip.Enabled = false;
                        textcom.Enabled = true;
                        butsend.Enabled = true;
                        butupgrade.Enabled = true;
                        butbackup.Enabled = true;
                        butdownload.Enabled = true;

                        // textDOS.AppendText(mysocket.ReceiveData(int.Parse(ts)));
                        this.AcceptButton = butsend;
                        textcom.Focus();
                        tabPage1.Text = textip.Text;

                        if (mysocket.SendData(textusr.Text))
                        {
                            Thread.Sleep(200);
                            mysocket.SendData(textpsd.Text);
                            string pass0 = mysocket.ReceiveData(int.Parse(ts)); //清掉上次残留的password数据
                            mysocket.SendData("enable");
                            Thread.Sleep(500);
                            string pass = mysocket.ReceiveData(int.Parse(ts));
                            if (pass.Contains("Pas"))
                            {
                                mysocket.SendData(textpsd.Text);
                                Thread.Sleep(500);
                                string locked = mysocket.ReceiveData(int.Parse(ts));
                                if (locked.Contains("configuration is locked by other user"))
                                {
                                    mysocket.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket.SendData("enable");
                                    Thread.Sleep(200);
                                    mysocket.SendData(textpsd.Text);
                                }
                            }
                            toolStripStatusLabellinkstat.Text = "已连接";


                            mysocket.SendData("show slot" + "\r\n");
                            //mysocket.SendData("\r\n");
                            //mysocket.SendData("\r\n");
                            string nms17A = "17  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   ACTIVE";
                            string nms17S = "17  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   STANDBY";
                            string nms18A = "18  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   ACTIVE";
                            string nms18S = "18  GPN7600-NMS-V1           GPN7600-NMS-V1           RUNNING        MASTER   STANDBY";
                            string nms17AV2 = "17  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   ACTIVE";
                            string nms17SV2 = "17  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   STANDBY";
                            string nms18AV2 = "18  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   ACTIVE";
                            string nms18SV2 = "18  GPN7600-V2-NMS           GPN7600-V2-NMS           RUNNING        MASTER   STANDBY";
                            string nms17A2 = "17  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   ACTIVE";
                            string nms17S2 = "17  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   STANDBY";
                            string nms18A2 = "18  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   ACTIVE";
                            string nms18S2 = "18  GPN7600-NMS-V2           GPN7600-NMS-V2           RUNNING        MASTER   STANDBY";
                            string swa11AA = "11  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    ACTIVE";
                            string swa12AS = "12  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    STANDBY";
                            string swa11AS = "11  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    STANDBY";
                            string swa12AA = "12  GPN7600-SW-A             GPN7600-SW-A             RUNNING        SLAVE    ACTIVE";
                            string swa11AAV2 = "11  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    ACTIVE";
                            string swa12ASV2 = "12  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    STANDBY";
                            string swa11ASV2 = "11  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    STANDBY";
                            string swa12AAV2 = "12  GPN7600-V2-SW            GPN7600-V2-SW            RUNNING        SLAVE    ACTIVE";
                            string swb11AA = "11  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    ACTIVE";
                            string swb12AS = "12  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    STANDBY";
                            string swb11AS = "11  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    STANDBY";
                            string swb12AA = "12  GPN7600-SW-B             GPN7600-SW-B             RUNNING        SLAVE    ACTIVE";
                            string GPN800 = "1  GPN800-NMS-V1            GPN800-NMS-V1            RUNNING        MASTER   ACTIVE";


                            for (int a = 0; a <= 100; a++)
                            {

                                string slot = mysocket.ReceiveData(int.Parse(ts));
                                if (slot.Contains(GPN800))
                                {
                                    slot17 = "ACTIVE";
                                    toolStripStatusLabelnms.Text = "01槽：主";
                                    toolStripStatusLabelnms.ForeColor = Color.DarkGreen;
                                    // textDOS.AppendText("\r\n" + "17槽主在位==============================================OK");
                                }

                                if ((slot.Contains(nms17A)) || (slot.Contains(nms17AV2)) || slot.Contains(nms17A2))
                                {
                                    slot17 = "ACTIVE";
                                    toolStripStatusLabelnms.Text = "17槽：主";
                                    toolStripStatusLabelnms.ForeColor = Color.DarkGreen;

                                    // textDOS.AppendText("\r\n" + "17槽主在位==============================================OK");
                                }
                                if ((slot.Contains(nms17S)) || (slot.Contains(nms17SV2)) || (slot.Contains(nms17S2)))
                                {
                                    slot17 = "STANDBY";
                                    toolStripStatusLabelnms.Text = "17槽：备";
                                    toolStripStatusLabelnms.ForeColor = Color.Red;
                                    // textDOS.AppendText("\r\n" + "17槽备在位==============================================OK");
                                }
                                if ((slot.Contains(nms18A)) || (slot.Contains(nms18AV2)) || (slot.Contains(nms18A2)))
                                {
                                    slot18 = "ACTIVE";
                                    toolStripStatusLabelnms18.Text = "18槽：主";
                                    toolStripStatusLabelnms18.ForeColor = Color.DarkGreen;
                                    //textDOS.AppendText("\r\n" + "18槽主在位==============================================OK");
                                }
                                if ((slot.Contains(nms18S)) || (slot.Contains(nms18SV2)) || (slot.Contains(nms18S2)))
                                {
                                    toolStripStatusLabelnms18.Text = "18槽：备";
                                    toolStripStatusLabelnms18.ForeColor = Color.Red;
                                    slot18 = "STANDBY";
                                    // textDOS.AppendText("\r\n" + "18槽备在位=============================================OK");
                                }

                                if ((slot.Contains(swa11AA)) || (slot.Contains(swa11AAV2)))
                                {
                                    slot11 = "在位";
                                    toolStripStatusLabelswa11.Text = "11槽SW-A：主";
                                    toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                                    //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                                }
                                if ((slot.Contains(swa11AS)) || (slot.Contains(swa11ASV2)))
                                {
                                    slot11 = "在位";
                                    toolStripStatusLabelswa11.Text = "11槽SW-A：备";
                                    toolStripStatusLabelswa11.ForeColor = Color.Red;
                                    //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                                }
                                if ((slot.Contains(swa12AA)) || (slot.Contains(swa12AAV2)))
                                {
                                    slot12 = "在位";
                                    toolStripStatusLabelswa12.Text = "12槽SW-A：主";
                                    toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                                    //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                                }
                                if ((slot.Contains(swa12AS) || slot.Contains(swb12AS)) || slot.Contains(swa12ASV2))
                                {
                                    slot12 = "在位";
                                    toolStripStatusLabelswa12.Text = "12槽SW-A：备";
                                    toolStripStatusLabelswa12.ForeColor = Color.Red;
                                    //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                                }
                                if (slot.Contains(swb11AA))
                                {
                                    slot11 = "在位";
                                    sw = "swb";
                                    toolStripStatusLabelswa11.Text = "11槽SW-B：主";
                                    toolStripStatusLabelswa11.ForeColor = Color.DarkGreen;
                                    //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                                }
                                if (slot.Contains(swb11AS))
                                {
                                    slot11 = "在位";
                                    sw = "swb";
                                    toolStripStatusLabelswa11.Text = "11槽SW-B：备";
                                    toolStripStatusLabelswa11.ForeColor = Color.Red;
                                    //textDOS.AppendText("\r\n" + "11槽在位=============================================OK");
                                }
                                if (slot.Contains(swb12AA))
                                {
                                    slot12 = "在位";
                                    sw = "swb";
                                    toolStripStatusLabelswa12.Text = "12槽SW-B：主";
                                    toolStripStatusLabelswa12.ForeColor = Color.DarkGreen;
                                    //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                                }
                                if (slot.Contains(swb12AS))
                                {
                                    slot12 = "在位";
                                    sw = "swb";
                                    toolStripStatusLabelswa12.Text = "12槽SW-B：备";
                                    toolStripStatusLabelswa12.ForeColor = Color.Red;
                                    //textDOS.AppendText("\r\n" + "12槽在位=============================================OK");
                                }

                                Thread.Sleep(1);
                            }
                            mysocket.SendDate("\r\n");
                            // mysocket.SendDate("\r\n");
                            // mysocket.SendDate("\x03");
                            Thread.Sleep(500);
                            string meiyong = textDOS.Text + "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                            mysocket.SendData("show ver");
                            string ver = "";
                            for (int b = 0; b <= 100; b++)
                            {

                                ver = ver + mysocket.ReceiveData(int.Parse(ts));

                                Thread.Sleep(1);
                            }
                            Regex r = new Regex("ProductOS Version (V\\d+)*R\\d+(C\\d+)*B\\d+(SP\\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            string banben = r.Match(ver).Groups[0].Value;
                            if (banben.ToString() == "")
                            {
                                MessageBox.Show("请检查设备是否合法，再次尝试");
                                this.butlogin.PerformClick();
                                return;
                            }
                            banben = banben.Substring("ProductOS Version ".Length);
                            toolStripStatusLabelver.Text = "版本：" + banben.ToString();
                            toolStripStatusLabelver.ForeColor = Color.Red;
                            version = banben.ToString();
                            mysocket.SendDate("\x03");
                            Thread.Sleep(500);
                            string meiryong = textDOS.Text + "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                            textDOS.AppendText("\r\n" + "登录成功可以使用========================================OK");
                            this.butsend.PerformClick();
                        }
                        else
                        {
                            textDOS.AppendText("\r\n" + textcom.Text + "发送失败!");
                            textcom.Text = "";
                            textDOS.Focus();
                            textDOS.ScrollToCaret();
                            textcom.Focus();
                        }

                    }
                    else {
                        textDOS.AppendText("\r\n" + "无法Telnet登录，请检查设备是否正常！");
                    }
                }
                else
                {
                    textDOS.AppendText("\r\n" + "设备无法ping通，请检查IP地址或者设备是否正常！");
                }

            }


        }
        #endregion

        #region 发送按钮

        private void butsend_Click(object sender, EventArgs e)
        {


            if (mysocket.SendData(textcom.Text))
            {
                if (textcom.Text == "")
                {
                    Thread.Sleep(100);
                    string ctrlc = "Press any key to continue Ctrl+c to stop";
                    string DOS = textDOS.Text;
                    if (DOS.Contains(ctrlc))
                    {
                        textDOS.Text = DOS.Replace(ctrlc, "");
                        //MessageBox.Show("检测到了");
                    }
                    string str = "\r\n" + mysocket.ReceiveData(int.Parse(ts));
                    string luanma = "[7m --Press any key to continue Ctrl+c to stop-- [m";
                    string newSS = str.Replace(luanma, "Press any key to continue Ctrl+c to stop");
                    string luama2 = "\n" + "                                              ";
                    string newSD = newSS.Replace(luama2, "");
                    string vcg = "[0m[0;0m";
                    string newvcg = newSD.Replace(vcg, "");
                    string vcg2 = "\n";
                    string newvcg2 = newvcg.Replace(vcg2, "\r\n");
                    string kou = "";
                    string kou2 = "";
                    string newkou = newvcg2.Replace(kou, "");
                    string newkou2 = newkou.Replace(kou2, "");
                    string msapeth = "[0;32m";
                    string msapeth2 = newkou2.Replace(msapeth,"");

                    string msapeth1 = "[0m";
                    string msapeth3 = msapeth2.Replace(msapeth1, "");

                    string msapeth4 = "[0;0m";
                    string msapeth5 = msapeth3.Replace(msapeth4, "");


                    //textBox3.Text = newkou2;
                    textDOS.AppendText(msapeth5);
                    //this.textBox3.Text = str;
                }
                else
                {
                    com = textcom.Text;
                    Thread.Sleep(200);
                    string ss = mysocket.ReceiveData(int.Parse(ts));
                    string luanma = "[7m --Press any key to continue Ctrl+c to stop-- [m";
                    string newSS = ss.Replace(luanma, "Press any key to continue Ctrl+c to stop");
                    string luama2 = "\n" + "                                              ";
                    string newSD = newSS.Replace(luama2, "");
                    string vcg = "[0m[0;0m";
                    string newvcg = newSD.Replace(vcg, "");
                    string vcg2 = "\n";
                    string newvcg2 = newvcg.Replace(vcg2, "\r\n");
                    string kou = "";
                    string kou2 = "";
                    string newkou = newvcg2.Replace(kou, "");
                    string newkou2 = newkou.Replace(kou2, "");
                    string msapeth = "[0;32m";
                    string msapeth2 = newkou2.Replace(msapeth,"");
                    string msapeth1 = "[0m";
                    string msapeth3 = msapeth2.Replace(msapeth1, "");
                    string msapeth4 = "[0;0m";
                    string msapeth5 = msapeth3.Replace(msapeth4, "");


                    //textBox3.Text = newkou2;
                    textDOS.AppendText(msapeth5);
                    //this.textDOS.Text = ss;




                }
            }
            else
            {
                textDOS.AppendText("\r\n" + "连接通信故障，请断开后，重新尝试！");
                //this.butlogin.PerformClick();

            }
            textcom.Text = "";
            textDOS.Focus();
            textDOS.ScrollToCaret();
            textcom.Focus();
        }
        #endregion

        #region 保存配置 Save
        private void Save(DoWorkEventArgs e)
        {

            toolStripStatusLabelzt.Text = "正在保存配置";
            mysocket.SendData("save");
            for (int i = 1; i <= 20; i++)
            {
                string save = "successfully";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(save))
                {
                    textDOS.AppendText("\r\n" + "save===================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains("erro"))
                {
                    textDOS.AppendText("\r\n" + "save===================================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }

                Thread.Sleep(100);
            }
            string box1 = mysocket.ReceiveData(int.Parse(ts));
            Thread.Sleep(1000);

        }
        #endregion

        #region 测试FTP服务器IP是否正常 Testftpser
        private void Testftpser(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "检查FTP服务器中";
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp file /flash/sys/slotconfig.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + "FTP服务器检测文件.bin");

            for (int i = 1; i <= 1000; i++)
            {

                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("\r\n" + "FTP服务器测试==========================================OK" + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("\r\n" + "FTP服务器IP地址故障，请检查！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    e.Cancel = true;
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("\r\n" + "FTP服务器用户名密码错误，请检查！" + "\r\n");
                    toolStripStatusLabelzt.Text = "FTP故障，请检查！";
                    e.Cancel = true;
                    return;

                }
                Thread.Sleep(10);

            }

        }
        #endregion

        #region 备份文件 Backup
        private void Backup(DoWorkEventArgs e)
        {

            toolStripStatusLabelzt.Text = "正在备份config文件";
            //textDOS.AppendText("已执行保存");
            mysocket.SendData("upload ftp file /flash/sys/conf_data.txt " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + textip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_config.txt");

            for (int i = 1; i <= 1000; i++)
            {

                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("备份config=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("备份config=================请检查FTP服务器IP或是否开启==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                if (box.Contains("User need password"))
                {
                    textDOS.AppendText("备份config=========================请检查FTP用户名密码==NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);

            }
            //Thread.Sleep(1000);
            toolStripStatusLabelzt.Text = "正在备份slotconfig文件";
            mysocket.SendData("upload ftp file /flash/sys/slotconfig.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + textip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_slotconfig.bin");
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "ok";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("备份slotconfig=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("备份lsotconfig=========================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);

            }
            //Thread.Sleep(1000);
            toolStripStatusLabelzt.Text = "正在备份db文件";
            mysocket.SendData("upload ftp file /flash/sys/db.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + textip.Text + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_db.bin");
            for (int i = 1; i <= 1000; i++)
            {
                string ok = "ok";
                string fail = "fail";
                string db = "db.bin";
                string box = mysocket.ReceiveData(int.Parse(ts));
                if (box.Contains(ok))
                {
                    for (int a = 1; a <= 1000; a++)
                    {
                        string box2 = mysocket.ReceiveData(int.Parse(ts));

                        if (box2.Contains(db))
                        {
                            textDOS.AppendText("备份db=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            break;
                        }
                        Thread.Sleep(10);

                    }

                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("备份db=================================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);

            }


        }
        #endregion

        #region 删除残余文件 Rm
        private void Rm(DoWorkEventArgs e)
        {

            toolStripStatusLabelzt.Text = "清空主槽残余文件";
            mysocket.SendData("dosfs");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("dosfs"))
                {
                    break;
                }
                Thread.Sleep(10);
            }
            mysocket.SendData("cd /flash/sys");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("cd /flash/sys"))
                {
                    break;
                }
                Thread.Sleep(10);
            }

            mysocket.SendData("rm app_code_backup.bin");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("rm app_code_backup.bin"))
                {
                    textDOS.AppendText("主槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(10);
            }

            mysocket.SendData("rm record.txt");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("rm record.txt"))
                {
                    textDOS.AppendText("主槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(10);
            }

            mysocket.SendData("rm fpga_code.bin");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("rm fpga_code.bin"))
                {
                    textDOS.AppendText("主槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                Thread.Sleep(10);
            }
            mysocket.SendData("exit");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("exit"))
                {
                    break;
                }
                Thread.Sleep(10);
            }

            string nms = mysocket.ReceiveData(int.Parse(ts));

            if (slot18 == "STANDBY" || slot17 == "STANDBY")
            {

                toolStripStatusLabelzt.Text = "清空备槽残余文件";
                textDOS.AppendText("备槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("grosadvdebug");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("grosadvdebug"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                if (slot18 == "STANDBY")
                {
                    mysocket.SendData("switch 18");
                }
                if (slot17 == "STANDBY")
                {
                    mysocket.SendData("switch 17");
                }
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Slot1"))
                    {
                        break;
                    }

                    if (command.Contains("Please try later"))
                    {
                        textDOS.AppendText("备槽有其他户登录，已退出终止升级");
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
                            Thread.Sleep(10);
                        }
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("grosadvdebug");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("grosadvdebug"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("slave-config enable");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("slave-config enable"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("enable");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("enable"))
                    {
                        mysocket.SendData(textpsd.Text);
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("dosfs");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("dosfs"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("cd /flash/sys");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("cd /flash/sys"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("rm app_code_backup.bin");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("rm app_code_backup.bin"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                textDOS.AppendText("备槽删除app_code_backup.bin============================OK" + toolStripStatusLabeltime.Text + "\r\n");

                mysocket.SendData("rm record.txt");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("rm record.txt"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                textDOS.AppendText("备槽删除record.txt=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("rm fpga_code.bin");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("rm fpga_code.bin"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                textDOS.AppendText("备槽删除/flash/sys/fpga_code.bin=======================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("exit");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                mysocket.SendData("exit");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("xit"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(500);
                mysocket.SendData("exit");
                for (int a = 1; a <= 3000; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("(config)#"))
                    {
                        break;
                    }
                    if (command.Contains(">"))
                    {
                        mysocket.SendData("enable");
                        Thread.Sleep(1000);
                        string pass = mysocket.ReceiveData(int.Parse(ts));
                        if (pass.Contains("Pas"))
                        {
                            mysocket.SendData(textpsd.Text);
                            Thread.Sleep(1000);
                            string locked = mysocket.ReceiveData(int.Parse(ts));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket.SendData("grosadvdebug");
                                Thread.Sleep(200);
                                mysocket.SendData("vty user limit no");
                                Thread.Sleep(200);
                                mysocket.SendData("exit");
                                Thread.Sleep(200);
                                mysocket.SendData("enable");
                                Thread.Sleep(200);
                                mysocket.SendData(textpsd.Text);
                            }
                        }
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
            ///OK是判断==0 备槽不在位 == 1 备槽在位
            if (slot18 == "")
            {
                for (int a = 1; a <= 100; a++)
                {
                    string command = mysocket.ReceiveData(int.Parse(ts));
                    if (command.Contains("Press"))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                textDOS.AppendText("备槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
            }

        }
        #endregion

        #region 下载 App  ///////////////////////////////////////////下载软件APP开始////////////////////////////////
        private void App(DoWorkEventArgs e)
        {

            ///////////////////////////////////////////下载软件APP开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载APP中";


            mysocket.SendData("download ftp app " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comapp.Text + " gpn");
            for (int a = 1; a <= 20000; a++)
            {

                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    textDOS.AppendText("主槽APP下载成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    toolStripStatusLabelzt.Text = "写入APP中";
                    for (int b = 1; b <= 50000; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText("17槽APP写入成功==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText("18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")

                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("备槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("备槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")

                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("备槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("备槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S12 = "爱好";
                                    string S18 = "号码";
                                    textDOS.AppendText("11槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {

                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("18槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";

                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("18槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {

                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText("全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText("11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }

                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText("18槽APP写入成功========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";
                                            textDOS.AppendText("17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText("17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")

                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText("11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")

                                {
                                    string S11 = "其他";
                                    string S18 = "未知";
                                    textDOS.AppendText("12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up1118slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up1118slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up1118slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up1118slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up1118slot.Contains("upgraded all files successfully") || (S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "其他";
                                    string S12 = "爱好";
                                    string S18 = "号码";
                                    textDOS.AppendText("11槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {

                                        string allslot = mysocket.ReceiveData(int.Parse(ts));
                                        if (allslot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到备槽中";

                                            textDOS.AppendText("17槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";

                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (allslot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText("17槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (allslot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("备槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (allslot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "未知";
                                    string S12 = "其他";
                                    textDOS.AppendText("11槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位=============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 100000; c++)
                                    {

                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up112slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText("全部槽同步APP==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText("12槽在位==================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步APP========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步APP===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步APP============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("备槽不在位================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;
                                }


                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText("主槽APP写入===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText("主槽APP下载==============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }



        }
        #endregion

        #region 下载 Fpgacode  //////////////////////////////////////下载FPGA_code开始//////////////////////////////
        private void Fpgacode(DoWorkEventArgs e)
        {
            ///////////////////////////////////////////下载FPGA_code开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载FPGA_CODE中";

            if (comapp.Text.Contains("R13"))
            {
                textDOS.AppendText("升级为R13版本执行特殊升级方式===========================OK" + toolStripStatusLabeltime.Text + "\r\n");
                mysocket.SendData("download ftp file /flash/sys/fpga_code.bin " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comcode.Text);

            }
            else
            {
                mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comcode.Text + " other");

            }
            for (int a = 1; a <= 50000; a++)
            {

                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入FPGA_CODE中";
                    textDOS.AppendText("主槽FPGA_CODE下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= 50000; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));

                        if (comapp.Text.Contains("R13"))
                        {
                            if (download.Contains("ok"))
                            {
                                textDOS.AppendText("主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                break;
                            }
                        }
                        else
                        {
                            if (download.Contains("ok"))
                            {

                                textDOS.AppendText("主槽FPGA_CODE写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY")

                                {
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText("备槽准备同步FPGA_CODE================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("18 fail") || up18slot.Contains("Failed upgraded slot 18"))
                                        {

                                            textDOS.AppendText("18槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY")
                                {
                                    textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步FPGA_CODE到备槽中";
                                            textDOS.AppendText("17槽准备同步FPGA_CODE==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("17 fail") || up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText("17槽同步FPGA_CODE=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步FPGA_CODE======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                break;

                            }

                        }

                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText("主槽FPGA_CODE写入=======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText("主槽FPGA_CODE下载==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }

        }
        #endregion

        #region 下载 Nms  ///////////////////////////////////////////下载FPGA_NMS开始///////////////////////////////
        private void Nms(DoWorkEventArgs e)
        {

            ///////////////////////////////////////////下载FPGA_NMS开始////////////////////////////////
            toolStripStatusLabelzt.Text = "下载FPGA_NMS中";
            mysocket.SendData("download ftp fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comnms.Text + " master");
            for (int a = 1; a <= 50000; a++)
            {

                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入FPGA_NMS中";
                    textDOS.AppendText("主槽FPGA_NMS下载成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= 50000; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {

                            textDOS.AppendText("主槽FPGA_NMS写入成功=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                            if (slot18 == "STANDBY")
                            {
                                textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= 50000; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 18"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText("18槽准备同步FPGA_NMS===================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                    }
                                    if (up18slot.Contains("18 fail"))
                                    {
                                        textDOS.AppendText("18槽同步FPGA_NMS======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }

                                    if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                    {
                                        textDOS.AppendText("18槽同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(1);
                                }

                            }
                            if (slot17 == "STANDBY")
                            {
                                textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                for (int c = 1; c <= 50000; c++)
                                {
                                    string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                    if (up18slot.Contains("auto-upgrade to slot 17"))
                                    {
                                        toolStripStatusLabelzt.Text = "同步FPGA_NMS到备槽中";
                                        textDOS.AppendText("17槽准备同步FPGA_NMS=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                    }
                                    if (up18slot.Contains("17 fail"))
                                    {
                                        textDOS.AppendText("17槽同步FPGA_NMS==========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                        return;
                                    }

                                    if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                    {
                                        textDOS.AppendText("17槽同步FPGA_NMS===========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                        break;
                                    }
                                    Thread.Sleep(1);
                                }

                            }
                            break;

                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText("主槽FPGA_NMS写入========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText("主槽FPGA_NMS下载===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }

        }
        #endregion

        #region 下载 Swfpga /////////////////////////////////////////下载SW-Afpga开始///////////////////////////////
        private void Swfpga(DoWorkEventArgs e)
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
            for (int a = 1; a <= 5000; a++)
            {

                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("Download file ...ok"))
                {
                    toolStripStatusLabelzt.Text = "写入SW_FPGA中";
                    textDOS.AppendText("主槽SW_FPGA下载成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    for (int b = 1; b <= 50000; b++)
                    {
                        string download = mysocket.ReceiveData(int.Parse(ts));
                        if (download.Contains("ok"))
                        {
                            if (slot17 == "ACTIVE")
                            {
                                textDOS.AppendText("17槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "")

                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA===========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "" && slot12 == "在位")

                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("18槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 18"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA===============================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    string S18 = "3";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 18"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("18槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText("11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";

                                            textDOS.AppendText("12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("18 fail"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 18 successful"))
                                        {
                                            textDOS.AppendText("18槽同步SW_FPGA==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot18 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("18槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;

                                }
                                break;

                            }
                            if (slot18 == "ACTIVE")
                            {
                                textDOS.AppendText("18槽SW_FPGA写入成功====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA======================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }

                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "")

                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "" && slot12 == "在位")

                                {
                                    String S11 = "1";
                                    String S18 = "3";
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("17槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 17"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA=========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || S11 == S18)
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA=======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "STANDBY" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    string S18 = "3";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 17"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到备槽中";
                                            textDOS.AppendText("17槽准备同步SW_FPGA===================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            textDOS.AppendText("11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";

                                            textDOS.AppendText("12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("17 fail"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 17 successful"))
                                        {
                                            textDOS.AppendText("17槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S18 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12 && S11 == S18))
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "在位")
                                {
                                    string S11 = "1";
                                    string S12 = "2";
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {

                                        string up18slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up18slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步SW_FPGA到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA==================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up18slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S11 = "OK";
                                        }
                                        if (up18slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            S12 = "OK";
                                        }
                                        if (up18slot.Contains("upgraded all files successfully") || (S11 == S12))
                                        {
                                            textDOS.AppendText("全部槽同步SW_FPGA====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "在位" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 11"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到11槽中";
                                            textDOS.AppendText("11槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 11"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 11 successful"))
                                        {
                                            textDOS.AppendText("11槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "在位")
                                {
                                    textDOS.AppendText("12槽在位=================================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    for (int c = 1; c <= 50000; c++)
                                    {
                                        string up112slot = mysocket.ReceiveData(int.Parse(ts));
                                        if (up112slot.Contains("auto-upgrade to slot 12"))
                                        {
                                            toolStripStatusLabelzt.Text = "同步APP到12槽中";
                                            textDOS.AppendText("12槽准备同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");

                                        }
                                        if (up112slot.Contains("Failed upgraded slot 12"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                                            e.Cancel = true;
                                            return;
                                        }
                                        if (up112slot.Contains("Auto-upgrade to slot 12 successful"))
                                        {
                                            textDOS.AppendText("12槽同步SW_FPGA=====================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                            break;
                                        }

                                        Thread.Sleep(1);
                                    }

                                }
                                if (slot17 == "" && slot11 == "" && slot12 == "")
                                {
                                    textDOS.AppendText("11槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("12槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    textDOS.AppendText("17槽不在位===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                                    break;

                                }
                                break;

                            }
                            break;
                        }
                        if (download.Contains("failed"))
                        {
                            textDOS.AppendText("主用槽SW_FPGA写入=====================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                            return;
                        }
                        Thread.Sleep(1);
                    }
                    break;
                }
                if (command.Contains("failed"))
                {
                    textDOS.AppendText("主用槽SW_FPGA下载========================================NOK" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }

        }



        #endregion

        #region 下载 Fpga760a

        private void Fpga760a(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载760A";
            mysocket.SendData("download ftp file /yaffs/sys/760a.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760a.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载760a===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载760a===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 下载 Fpga760b

        private void Fpga760b(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载760B";
            mysocket.SendData("download ftp file /yaffs/sys/760b.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760b.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载760b===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载760b===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 下载 Fpga760c
        private void Fpga760c(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载760C";
            mysocket.SendData("download ftp file /yaffs/sys/760c.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760c.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载760c===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载760c===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 下载 Fpga760d
        private void Fpga760d(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载760C";
            mysocket.SendData("download ftp file /yaffs/sys/760d.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760d.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载760d===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载760d===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 下载 Fpga760e
        private void Fpga760e(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载760C";
            mysocket.SendData("download ftp file /yaffs/sys/760e.fpga " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + com760e.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载760e===============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载760e===============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 下载 Sysfile
        private void Sysfile(DoWorkEventArgs e)
        {
            toolStripStatusLabelzt.Text = "正在下载sysfile";
            mysocket.SendData("download ftp sysfile " + comftpip.Text + " " + textftpusr.Text + " " + textftppsd.Text + " " + comsysfile.Text);
            for (int i = 1; i <= 1000; i++)
            {

                string ok = "Write to flash...";
                string fail = "fail";
                string box = mysocket.ReceiveData(int.Parse(ts)); ;
                if (box.Contains(ok))
                {
                    textDOS.AppendText("下载sysfile==============================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                    break;
                }
                if (box.Contains(fail))
                {
                    textDOS.AppendText("下载sysfile=============================================fail" + toolStripStatusLabeltime.Text + "\r\n");
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region 升级后重启 Reboot
        private void Reboot(DoWorkEventArgs e)
        {
            mysocket.SendData("reboot");
            for (int a = 1; a <= 100; a++)
            {
                string command = mysocket.ReceiveData(int.Parse(ts));
                if (command.Contains("reboot"))
                {
                    break;
                }
                Thread.Sleep(100);
            }
            DialogResult dr = MessageBox.Show("已完成，是否重启设备？", "提示", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                mysocket.SendData("Y");
                textDOS.AppendText("您选择重启设备==========================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                this.butlogin.PerformClick();
                //户选择确认的操作

            }
            if (dr == DialogResult.No)
            {
                //户选择取消的操作
                mysocket.SendData("N");
                textDOS.AppendText("您没有选择重启设备======================================OK" + toolStripStatusLabeltime.Text + "\r\n");
                this.butsend.PerformClick();
            }


        }
        #endregion

        #region ctrl+c 按钮
        private void butctrlc_Click(object sender, EventArgs e)
        {
            mysocket.SendDate("\x03");
            // Thread.Sleep(1000);
            //textDOS.AppendText("\r\n"+"CTRL+C已发送");
            // textDOS.AppendText(mysocket.ReceiveData(int.Parse(ts)));
            this.butsend.PerformClick();
        }
        #endregion

        #region ctrl+q按钮
        private void butctrlq_Click(object sender, EventArgs e)
        {
            mysocket.SendDate("\x011");

            // textDOS.AppendText("\r\n"+"CTRL+Q已发送");
            this.butsend.PerformClick();
        }
        #endregion

        #region 上下键显示上次记忆字符串
        private void textcom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                this.butsend.Focus();
                textcom.Text = com.ToString();
            }
            if (e.KeyCode == Keys.Down)
            {
                this.butsend.Focus();
                textcom.Text = "";
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

                DirectoryInfo dir = new DirectoryInfo(@path.SelectedPath);
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
                    if (s.Contains("code")||s.Contains("CODE"))
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
                    if (s.Contains("sysfile") || s.Contains("Sysfile") || s.Contains("SYSFILE"))
                    {
                        comsysfile.Items.Add(s);
                        if (comsysfile.Items.Count > 0)
                        {
                            comsysfile.SelectedIndex = comsysfile.Items.Count - 1;
                        }
                    }
                }

            }
            //comapp.SelectedIndex = 0;
            //this.comapp.Text = file.SafeFileName;
        }
        #endregion

        #region 保存记录内容
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "保存打印记录";
            //sfd.InitialDirectory = @"C:\";
            sfd.Filter = "文本文件| *.txt";
            sfd.FileName = textip.Text + "-" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + "Telnet+FTP打印日志";

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
                string jilu = toolStripStatusLabelver.Text
                    + "\r\n" + toolStripStatusLabelnms.Text 
                    + "\r\n" + toolStripStatusLabelnms18.Text
                    + "\r\n" + toolStripStatusLabelswa11.Text 
                    + "\r\n" + toolStripStatusLabelswa12.Text 
                    + "\r\n" + comapp.Text + checkapp.Checked
                    + "\r\n" + comcode.Text + checkcode.Checked
                    + "\r\n" + comnms.Text + checknms.Checked
                    + "\r\n" + comsw.Text + checksw.Checked
                    + "\r\n" + com760a.Text + check760a.Checked
                    + "\r\n" + com760b.Text + check760b.Checked
                    + "\r\n" + com760c.Text + check760c.Checked
                    + "\r\n" + com760d.Text + check760d.Checked
                    + "\r\n" + com760e.Text + check760e.Checked
                    + "\r\n" + comsysfile.Text + checksysfile.Checked
                    + "\r\n ===============================" 
                    + "\r\n" + textDOS.Text
                    + "\r\n ====================="
                    + "\r\n" + saveString;
                byte[] buffer = Encoding.Default.GetBytes(jilu);
                fsWrite.Write(buffer, 0, buffer.Length);
                MessageBox.Show("保存成功!");

            }
        }
        #endregion

        #region ctrl+s快捷键保存
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.S)
            {
                {
                    this.保存ToolStripMenuItem.PerformClick();
                }
            }
            if (e.KeyCode == Keys.Down)
            {
                this.butsend.Focus();
                textcom.Text = "";
            }
            if (e.Control == true && e.KeyCode == Keys.C)
            {
                mysocket.SendDate("\x03");
                // Thread.Sleep(1000);
                //textDOS.AppendText("\r\n"+"CTRL+C已发送");
                this.butsend.PerformClick();

            }
            if (e.Control == true && e.KeyCode == Keys.Q)
            {
                mysocket.SendDate("\x011");
                // textDOS.AppendText("\r\n"+"CTRL+Q已发送");
                this.butsend.PerformClick();

            }
            if (e.KeyCode == Keys.Back)
            {
                //string msg = textDOS.Text;
                //textDOS.Text = msg.Substring(0, msg.Length - 1);
                //mysocket.SendDate("\b");
                ////MessageBox.Show("按下了back键");

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
                Thread.Sleep(100);
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
                        textDOS.AppendText(newSw);
                    }
                    else
                    {
                        string luanma = "\b";
                        string newSS = stra.Replace(luanma, "");
                        textDOS.AppendText(newSS);
                    }


                }
                else
                {
                    string luanma = "\b";
                    string newSS = stra.Replace(luanma, "");
                    textDOS.AppendText(newSS);
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

        #region 软件版本信息
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {

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


        private void Form1_Load(object sender, EventArgs e)
        {
            Mytimer = new System.Threading.Timer(new TimerCallback(TimerUp), null, Timeout.Infinite, 1000);
            this.WindowState = FormWindowState.Maximized;
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
        private string strFilePath = @"C:\Program Files\Config.ini";
        private string strSec = ""; //INI文件名

        #endregion

        #region 设置ini文件内容
        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
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
                WritePrivateProfileString(strSec, "GPNip", textip.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNuser", textusr.Text.Trim(), strFilePath);
                WritePrivateProfileString(strSec, "GPNpsd", textpsd.Text.Trim(), strFilePath);
                MessageBox.Show("写入成功");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());

            }
        }

        private string ContentValue(string Section, string key)
        {

            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, strFilePath);
            return temp.ToString();
        }
        #endregion

        #region 退出窗口
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            DialogResult dr = MessageBox.Show("是否退出并保存？", "提示", MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                mysocket.SendData("Y");
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
                    WritePrivateProfileString(strSec, "GPNip", textip.Text.Trim(), strFilePath);
                    WritePrivateProfileString(strSec, "GPNuser", textusr.Text.Trim(), strFilePath);
                    WritePrivateProfileString(strSec, "GPNpsd", textpsd.Text.Trim(), strFilePath);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());

                }
                ///////保存telnet 记录////////
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "保存打印记录";
                //sfd.InitialDirectory = @"C:\";
                sfd.Filter = "文本文件| *.txt";
                sfd.FileName = textip.Text + "-" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + "Telnet打印记录";

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
                    string jilu = toolStripStatusLabelver.Text + "\r\n" + toolStripStatusLabelnms.Text + "\r\n" + toolStripStatusLabelnms18.Text + "\r\n" + toolStripStatusLabelswa11.Text + "\r\n" + toolStripStatusLabelswa12.Text + "\r\n" + comapp.Text + "\r\n" + comcode.Text + "\r\n" + comnms.Text + "\r\n" + comsw.Text + "\r\n ===============================" + "\r\n" + textDOS.Text + "\r\n =====================" + "\r\n" + saveString;
                    byte[] buffer = Encoding.Default.GetBytes(jilu);
                    fsWrite.Write(buffer, 0, buffer.Length);
                    MessageBox.Show("保存成功!");

                }
                //户选择确认的操作

            }
            else if (dr == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            if (dr == DialogResult.No)
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
                    WritePrivateProfileString(strSec, "GPNip", textip.Text.Trim(), strFilePath);
                    WritePrivateProfileString(strSec, "GPNuser", textusr.Text.Trim(), strFilePath);
                    WritePrivateProfileString(strSec, "GPNpsd", textpsd.Text.Trim(), strFilePath);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());

                }
            }





        }
        #endregion

        #region 显示密码
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textpsd.PasswordChar = (char)0;
                textftppsd.PasswordChar = (char)0;
            }
            else
            {
                textftppsd.PasswordChar = '*';
                textpsd.PasswordChar = '*';
            }
        }
        #endregion

        #region 备份按钮
        private void butbackup_Click(object sender, EventArgs e)
        {

            this.toolStripProgressBar1.Maximum = 100;
            this.backgroundWorker2.RunWorkerAsync();
            //户选择确认的操作

        }
        #endregion

        #region 下载配置按钮
        private void butdownload_Click(object sender, EventArgs e)
        {
            if (comconfig.Text.Contains(".txt") && comslotconfig.Text.Contains("slotconfig") && comdb.Text.Contains("db"))
            {
                this.toolStripProgressBar1.Maximum = 100;
                this.backgroundWorker3.RunWorkerAsync();
                //户选择确认的操作


            }
            else
            {
                MessageBox.Show("三个配置文件命名必须各自包含，config.txt、slotconfig.bin、db.bin字符串方可行进下载");
            }

        }
        #endregion


        private void butbatch_Click(object sender, EventArgs e)
        {
            //if (string.Compare(btnFtpServerStartStop.Text, "启动FTP") == 0)
            //{
            //    MessageBox.Show("请先③启动FTP服务器,进行后续操作！");
            //    return;
            //}
            MessageBox.Show("支持了三方FTP工具，请先启动第三方FTP工具,然后点击批量升级。否则会出现卡死的情况，得重新关闭软件在打开！"
                + "\r\n" + "注意事项：FTP用户名：admin密码：admin必须一样，APP文件必须和升级的文件名一致");
            Batch batchfrm = new Batch();//实例化窗体
            batchfrm.FTPIP = comftpip.Text;
            batchfrm.FTPUSR = textftpusr.Text;
            batchfrm.FTPPSD = textftppsd.Text;
            batchfrm.GPNUSR = textusr.Text;
            batchfrm.GPNPSD = textpsd.Text;
            batchfrm.yanshi = ts;
            batchfrm.app = comapp.Text;

            batchfrm.ShowDialog();// 将窗体显示出来
            //this.Hide();//当前窗体隐藏
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
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

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate();//重绘窗体
        }

        private void 关于ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About About = new About();//实例化窗体

            About.ShowDialog();// 将窗体显示出来
        }

        private void 帮助ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Help Help = new Help();//实例化窗体

            Help.ShowDialog();// 将窗体显示出来
        }

        private void butgpnall_Click(object sender, EventArgs e)
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
                checksysfile.Checked = true;
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
                checksysfile.Checked = false;
                butgpnall.Text = "全部勾选";
            }

        }

        private void butgpn7600_Click(object sender, EventArgs e)
        {
            if (butgpn7600.Text == "GPN76-OTN勾选")
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



                checkapp.Checked = true;
                checkcode.Checked = true;
                checknms.Checked = true;
                checksw.Checked = true;
                check760a.Checked = true;
                check760b.Checked = true;
                check760c.Checked = true;
                check760e.Checked = true;
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
                checksysfile.Checked = false;
                butgpn7600.Text = "GPN76-OTN勾选";
            }
        }

        private void butgpn800_Click(object sender, EventArgs e)
        {
            if (butgpn800.Text == "GPN800勾选")
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


                checkapp.Checked = true;
                checknms.Checked = true;
                check760c.Checked = true;
                check760d.Checked = true;
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
                checksysfile.Checked = false;
                butgpn800.Text = "GPN800勾选";
            }
        }

        private void butgpn7600old_Click(object sender, EventArgs e)
        {
            if (butgpn7600old.Text == "GPN76-PTN勾选")
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

        private void butsyslog_Click(object sender, EventArgs e)
        {

            textcom.Text = "screen lines 40";
            this.butsend.PerformClick();

            textDOS.AppendText("\r\n"+ "////////////////////////////////////////////////////////////////////版本信息//////////////////////////////" + "\r\n");
            textcom.Text = "show ver";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////FPGA的SP版本/////////////////////////////" + "\r\n");

            textcom.Text = "grosadvdebug";
            this.butsend.PerformClick();
            textcom.Text = "show fpga";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    //MessageBox.Show("跳出循环");
                    textcom.Text = "exit";
                    this.butsend.PerformClick();
                    break;
                }

            }
            textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////槽位信息//////////////////////////////" + "\r\n");
            textcom.Text = "show slot";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                Thread.Sleep(100);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////配置文件信息//////////////////////////////" + "\r\n");
            textcom.Text = "show run";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////CPU内存利用率//////////////////////////////" + "\r\n");
            textcom.Text = "show system resource usage";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            if (toolStripStatusLabelver.Text.Contains("R13"))
            {
                textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////进程CPU利用率//////////////////////////////" + "\r\n");
                textcom.Text = "show task";
                this.butsend.PerformClick();
                for (int g = 0; g <= 1000; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }

                    //Thread.Sleep(10);
                }

            }
            else {
                textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////进程CPU利用率//////////////////////////////" + "\r\n");
                textcom.Text = "show task cpu";
                this.butsend.PerformClick();
                for (int g = 0; g <= 1000; g++)
                {
                    //MessageBox.Show("进入循环");
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        break;
                    }

                    //Thread.Sleep(10);
                }

            }

            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////温度信息///////////////////////////////" + "\r\n");
            textcom.Text = "show temperature";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////存储器系统日志//////////////////////////////" + "\r\n");
            textcom.Text = "show nvram syslog";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }

            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////当前告警//////////////////////////////" + "\r\n");
            textcom.Text = "show current alarm";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////ARP地址表//////////////////////////////" + "\r\n");
            textcom.Text = "show arp all";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////MAC地址表//////////////////////////////" + "\r\n");
            textcom.Text = "show forward-entry";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////接口状态信息//////////////////////////////" + "\r\n");
            textcom.Text = "show port-link";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////当前系统时间//////////////////////////////" + "\r\n");
            textcom.Text = "show time";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }
            textDOS.AppendText("\r\n" + "////////////////////////////////////////////////////////////////////设备启动时间//////////////////////////////" + "\r\n");
            textcom.Text = "show start time";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                //MessageBox.Show("进入循环");
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    break;
                }

                //Thread.Sleep(10);
            }

            textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////系统告警日志/////////////////////////////" + "\r\n");

            textcom.Text = "show alarm log";
            this.butsend.PerformClick();
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    //MessageBox.Show("跳出循环");
                    break;
                }

            }
            textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////内存队列信息/////////////////////////////" + "\r\n");

            textcom.Text = "grosadvdebug";
            this.butsend.PerformClick();
            textcom.Text = "show que";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    //MessageBox.Show("跳出循环");
                    textcom.Text = "exit";
                    this.butsend.PerformClick();
                    break;
                }

            }
            textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////APP内部版本信息/////////////////////////////" + "\r\n");

            textcom.Text = "grosadvdebug";
            this.butsend.PerformClick();
            textcom.Text = "show debug-version";
            this.butsend.PerformClick();
            for (int g = 0; g <= 1000; g++)
            {
                if (textDOS.Text.Contains("Ctrl+c"))
                {
                    this.butsend.PerformClick();
                }
                else
                {
                    //MessageBox.Show("跳出循环");
                    textcom.Text = "exit";
                    this.butsend.PerformClick();
                    break;
                }

            }



            this.保存ToolStripMenuItem.PerformClick();
        }

        private void butpaigu_Click(object sender, EventArgs e)
        {
            textDOS.Text = "";
            textcom.Text = "screen lines 40";
            this.butsend.PerformClick();
            if (comboard.Text == "EOS-8FX" || comboard.Text == "EOS-8FE" || comboard.Text == "DMD-8GE"||comboard.Text == "EOS/P-126")
            {
                textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////VCG接口信息/////////////////////////////" + "\r\n");
                textcom.Text = "interface vcg "+ comslot.Text + "/" + cometh.Text;
                this.butsend.PerformClick();
                textcom.Text = "show";
                this.butsend.PerformClick();
                for (int g = 0; g <= 1000; g++)
                {
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        break;
                    }

                }
                textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////VCG成员信息/////////////////////////////" + "\r\n");
                textcom.Text = "show current-state";
                this.butsend.PerformClick();
                for (int g = 0; g <= 1000; g++)
                {
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textcom.Text = "exit";
                        this.butsend.PerformClick();
                        break;
                    }

                }
                if ((comboard.Text == "EOS-8FX") || (comboard.Text == "EOS-8FE"))
                {
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////MSAP-ETH接口配置信息/////////////////////////////" + "\r\n");
                    textcom.Text = "interface msap-eth " + comslot.Text + "/" + cometh.Text;
                    this.butsend.PerformClick();
                    textcom.Text = "show";
                    this.butsend.PerformClick();
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////MSAP-ETH接口状态信息/////////////////////////////" + "\r\n");
                    textcom.Text = "show current";
                    this.butsend.PerformClick();
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////MSAP-SFP光模块信息/////////////////////////////" + "\r\n");
                    textcom.Text = "show sfp";
                    this.butsend.PerformClick();
                    for (int g = 0; g <= 1000; g++)
                    {
                        if (textDOS.Text.Contains("Ctrl+c"))
                        {
                            this.butsend.PerformClick();
                        }
                        else
                        {
                            //MessageBox.Show("跳出循环");
                            break;
                        }

                    }
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////MSAP-ETH流量信息/////////////////////////////" + "\r\n");
                    textcom.Text = "show portpfm current";
                    this.butsend.PerformClick();
                    for (int g = 0; g <= 1000; g++)
                    {
                        if (textDOS.Text.Contains("Current Perform Counters End"))
                        {
                            //MessageBox.Show("跳出循环");
                            textcom.Text = "exit";
                            this.butsend.PerformClick();
                            break;
                        }
                        else
                        {

                            this.butsend.PerformClick();
                        }
                        Thread.Sleep(1000);

                    }
                }
                
                textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////VCG流量查询/////////////////////////////" + "\r\n");
                textcom.Text = "config msap";
                this.butsend.PerformClick();
                textcom.Text = "rmon " + comslot.Text + " " + cometh.Text;
                this.butsend.PerformClick();
               
                for (int g = 0; g <= 1000; g++)
                {
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        textcom.Text = "exit";
                        this.butsend.PerformClick();
                        break;
                    }

                }
                if (comboard.Text == "EOS-8FX" || comboard.Text == "EOS-8FE" )
                {
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////VCG错包查询/////////////////////////////" + "\r\n");
                    textcom.Text = "grosadvdebug";
                    this.butsend.PerformClick();
                    textcom.Text = "debug command enable";
                    this.butsend.PerformClick();
                    textcom.Text = "exit";
                    this.butsend.PerformClick();
                    textcom.Text = "config msap";
                    this.butsend.PerformClick();
                    if (comboard.Text == "EOS-8FX")
                    {

                        textcom.Text = "eos8 vcg autofiforeset show " + comslot.Text;
                        this.butsend.PerformClick();
                    }
                    if (comboard.Text == "EOS-8FE")
                    {

                        textcom.Text = "eos_8fe vcg autofiforeset show " + comslot.Text;
                        this.butsend.PerformClick();
                    }

                    for (int g = 0; g <= 1000; g++)
                    {
                        if (textDOS.Text.Contains("Ctrl+c"))
                        {
                            this.butsend.PerformClick();
                        }
                        else
                        {
                            //MessageBox.Show("跳出循环");
                            textcom.Text = "exit";
                            this.butsend.PerformClick();
                            break;
                        }

                    }

                }
                
                textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////SDH上联口告警查询/////////////////////////////" + "\r\n");
                textcom.Text = "show current alarm";
                this.butsend.PerformClick();
                for (int g = 0; g <= 1000; g++)
                {
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        break;
                    }

                }
                textcom.Text = "config msap";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 5/1";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 5/2";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 5/3";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 5/4";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 6/1";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 6/2";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 6/3";
                this.butsend.PerformClick();
                textcom.Text = "ioctl soh show 6/4";
                this.butsend.PerformClick();
                textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////VCG寄存器信息查询/////////////////////////////" + "\r\n");
                textcom.Text = "ioctl vcg info " + comslot.Text + " " + cometh.Text;
                this.butsend.PerformClick();
                //MessageBox.Show("跳出循环");
                for (int g = 0; g <= 1000; g++)
                {
                    if (textDOS.Text.Contains("Ctrl+c"))
                    {
                        this.butsend.PerformClick();
                    }
                    else
                    {
                        //MessageBox.Show("跳出循环");
                        Thread.Sleep(100);
                        textcom.Text = "exit";
                        this.butsend.PerformClick();
                        break;
                    }

                }
                if (comboard.Text == "DMD-8GE")
                {
                    textDOS.AppendText("\r\n" + "/////////////////////////////////////////////////////////////////////以太口流量信息查询/////////////////////////////" + "\r\n");
                    textcom.Text = "interface eth ";
                    this.butsend.PerformClick();
                    for (int g = 0; g <= 1000; g++)
                    {
                        if (textDOS.Text.Contains("Ctrl+c"))
                        {
                            this.butsend.PerformClick();
                        }
                        else
                        {
                            //MessageBox.Show("跳出循环");
                            break;
                        }

                    }


                }
            }
        }

    }
}