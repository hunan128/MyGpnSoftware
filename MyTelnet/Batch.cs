using MetroFramework.Forms;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class Batch : MetroForm
    {
        int doneCount = 0;
        public Batch()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;

        }
        public string FTPIP;
        public string FTPUSR;
        public string FTPPSD;
        public string yanshi;
        public string app;
        public string GPNUSR;
        public string GPNPSD;
        public string GPNPSDEN;


        //导入Excel表格
        private void Butshow_Click(object sender, EventArgs e)
        {
            MessageBox.Show("导出「网管资源列表的Excel表格」尝试导入！");
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            OpenFileDialog ofd = new OpenFileDialog
            {
                // Filter = "Excel office2003(*.xls)|*.xls|Excel office2010(*.xlsx)|*.xlsx"//打开对话框筛选器
            };//首先根据打开对话框，选择excel表格
            string strPath;//完整的路径名
            string strCon = "";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    strPath = ofd.FileName;
                    if ((System.IO.Path.GetExtension(ofd.FileName)).ToLower() == ".xls")
                    {

                        strCon = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;", strPath);
                    }
                    else
                    {

                        strCon = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=Excel 8.0;", strPath);

                    }

                    //string strCon = "provider=microsoft.jet.oledb.4.0;data source=" + strPath + ";extended properties=excel 8.0";//关键是红色区域
                    OleDbConnection Con = new OleDbConnection(strCon);//建立连接
                    string strSql = "select * from [Sheet1$]";//表名的写法也应注意不同，对应的excel表为sheet1，在这里要在其后加美元符号$，并用中括号
                    OleDbCommand Cmd = new OleDbCommand(strSql, Con);//建立要执行的命令
                    OleDbDataAdapter da = new OleDbDataAdapter(Cmd);//建立数据适配器
                    DataSet ds = new DataSet();//新建数据集
                    da.Fill(ds, "Sheet1");//把数据适配器中的数据读到数据集中的一个表中（此处表名为shyman，可以任取表名）
                                          //指定datagridview1的数据源为数据集ds的第一张表（也就是shyman表），也可以写ds.Table["shyman"]

                    // dataGridView1.DataSource = ds.Tables[0];
                    DataView dv = ds.Tables[0].DefaultView;
                    dv.RowFilter = "类型 = '" + comtype.Text + "'";
                    dataGridView1.DataSource = dv;
                    if (dataGridView1.Columns["开始时间"] == null)
                    {

                        this.dataGridView1.Columns.Add("开始时间", "开始时间");
                        this.dataGridView1.Columns["开始时间"].FillWeight = 150;
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("开始时间");
                        this.dataGridView1.Columns.Add("开始时间", "开始时间");
                        this.dataGridView1.Columns["开始时间"].FillWeight = 150;
                    }
                    if (dataGridView1.Columns["ping测试"] == null)
                    {

                        this.dataGridView1.Columns.Add("ping测试", "ping测试");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("ping测试");
                        this.dataGridView1.Columns.Add("ping测试", "ping测试");
                    }
                    if (dataGridView1.Columns["最终结果"] == null)
                    {
                        this.dataGridView1.Columns.Add("最终结果", "最终结果");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("最终结果");
                        this.dataGridView1.Columns.Add("最终结果", "最终结果");
                    }

                    if (dataGridView1.Columns["结束时间"] == null)
                    {

                        this.dataGridView1.Columns.Add("结束时间", "结束时间");
                        this.dataGridView1.Columns["结束时间"].FillWeight = 150;
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("结束时间");
                        this.dataGridView1.Columns.Add("结束时间", "结束时间");
                        this.dataGridView1.Columns["结束时间"].FillWeight = 150;
                    }





                    this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    toolStripStatusLabelzonggong.Text = (dataGridView1.Rows.Count - 1).ToString();
                    toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);//捕捉异常
                    MessageBox.Show("请使用Office2003或者更新版本格式内容，如.xls或者.xlsx格式");
                }
            }
        }
        //导出Excel表格
        private void Butout_Click(object sender, EventArgs e)
        {
            NPOIExcel ET = new NPOIExcel();
            ET.ExportExcel("sheet1", dataGridView1,0);
        }

        //保存业务函数
        public void Save(object obj)
        {

            int i = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(ip);

            for (int a = 0; a <= 1; a++)
            {
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(1000);
                pingReply = ping.Send(ip);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["OMU序列号"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["NMS序列号"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["NMS硬件版本"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["FPGA版本"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["主控保护"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";


                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");
                if (bo)
                {

                    mysocket1.SendData(GPNUSR);
                    for (int a = 0; a <= 1000; a++)
                    {
                        string login = mysocket1.ReceiveData(int.Parse(yanshi));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    for (int c = 0; c <= 1000; c++)
                    {
                        string passd = mysocket1.ReceiveData(int.Parse(yanshi));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Kerberos") || passd.Contains("Bad passwords"))
                        {

                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请重新输入");
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误";
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                            }
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                        }
                        if (passd.Contains(">"))
                        {
                            //textDOS.AppendText("\r\n" + "用户名密码正确==========================================OK");
                            mysocket1.SendData("enable");
                            for (int b = 0; b <= 1000; b++)
                            {
                                string pass = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket1.SendData(GPNPSDEN);
                                    //Thread.Sleep(500);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket1.ReceiveData(int.Parse(yanshi));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录========================OK");
                                            mysocket1.SendData("grosadvdebug");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("vty user limit no");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("exit");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("enable");
                                            Thread.Sleep(200);
                                            if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                            {
                                                mysocket1.SendData(GPNPSDEN);
                                                Thread.Sleep(200);
                                                if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                                {
                                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                                    lock (sb)
                                                    {
                                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                        s = s + 1;
                                                        toolStripStatusLabelshibai.Text = s.ToString();
                                                    }
                                                    lock (o1)
                                                    {
                                                        int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                        e = e - 1;
                                                        toolStripStatusLabelshengyu.Text = e.ToString();

                                                        doneCount++;

                                                    }
                                                    return;
                                                }

                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
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
                                    //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录=============================OK");
                                    mysocket1.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("enable");
                                    Thread.Sleep(200);
                                    if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                    {
                                        mysocket1.SendData(GPNPSDEN);
                                        Thread.Sleep(200);
                                        if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                        {
                                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                            lock (sb)
                                            {
                                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                s = s + 1;
                                                toolStripStatusLabelshibai.Text = s.ToString();
                                            }
                                            lock (o1)
                                            {
                                                int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                e = e - 1;
                                                toolStripStatusLabelshengyu.Text = e.ToString();

                                                doneCount++;

                                            }
                                            return;
                                        }

                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(1);
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }

                    mysocket1.SendData("show ver");
                    string ver = "";
                    string ver2 = "";
                    for (int a = 0; a <= 1000; a++)
                    {
                        ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                        ver = ver + ver2;
                        if (ver2.Contains("Ctrl+c"))
                        {
                            mysocket1.SendDate("\r\n");
                            //MessageBox.Show(ver);
                        }
                        if (ver2.Contains("(config)#"))
                        {
                            ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                            ver = ver + ver2;
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    Regex r = new Regex(@"ProductOS\s*Version\s*([\w\d]+)[\s*\(]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string banben = r.Match(ver).Groups[1].Value;
                    if (banben.ToString() == "")
                    {
                        this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = "获取失败";


                    }
                    else
                    {
                        this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = banben.ToString();


                    }
                    //MessageBox.Show(ver);
                    Regex omusn5 = new Regex(@"SLOT  5 : GPN7600-[\s\S]*(OMU[\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string OMUSN5 = omusn5.Match(ver).Groups[1].Value;

                    if (OMUSN5 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["OMU序列号"].Value = OMUSN5;
                    }
                    Regex omusn6 = new Regex(@"SLOT  6 : GPN7600-[\s\S]*(OMU[\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string OMUSN6 = omusn6.Match(ver).Groups[1].Value;
                    if (OMUSN6 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["OMU序列号"].Value = OMUSN6;
                    }

                    Regex nmssn17 = new Regex(@"SLOT 17 : GPN7600[\s\S]*(G76NMS[\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string NMSSN17 = nmssn17.Match(ver).Groups[1].Value;
                    if (NMSSN17 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["NMS序列号"].Value = NMSSN17;
                    }
                    Regex nmssn18 = new Regex(@"SLOT 18 : GPN7600[\s\S]*(G76NMS[\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string NMSSN18 = nmssn18.Match(ver).Groups[1].Value;
                    //MessageBox.Show(ver);
                    //textDOS.AppendText(ver);

                    //MessageBox.Show(NMSSN18);
                    if (NMSSN18 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["NMS序列号"].Value = NMSSN18;
                    }
                    Regex nmshard17 = new Regex(@"SLOT 17 : GPN7600[\-\d\w\s]*(V[\w\d\.]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string NMSHard17 = nmshard17.Match(ver).Groups[1].Value;
                    if (NMSHard17 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["NMS硬件版本"].Value = NMSHard17;
                    }
                    Regex nmshard18 = new Regex(@"SLOT 18 : GPN7600[\-\d\w\s]*(V[\w\d\.]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string NMSHard18 = nmshard18.Match(ver).Groups[1].Value;
                    if (NMSHard18 != "")
                    {
                        this.dataGridView1.Rows[i].Cells["NMS硬件版本"].Value = NMSHard18;
                    }
                    if (ver.Contains("SLOT 17 : GPN7600") && ver.Contains("SLOT 18 : GPN7600"))
                    {
                        this.dataGridView1.Rows[i].Cells["主控保护"].Value = "双主控";
                    }
                    if (!ver.Contains("SLOT 17 : GPN7600") && ver.Contains("SLOT 18 : GPN7600"))
                    {
                        this.dataGridView1.Rows[i].Cells["主控保护"].Value = "18主用单主控";
                    }
                    if (ver.Contains("SLOT 17 : GPN7600") && !ver.Contains("SLOT 18 : GPN7600"))
                    {
                        this.dataGridView1.Rows[i].Cells["主控保护"].Value = "17主用单主控";
                    }
                    ver = "";
                    mysocket1.SendData("show fpga-version");
                    for (int a = 0; a <= 1000; a++)
                    {
                        ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                        ver = ver + ver2;
                        if (ver2.Contains("Ctrl+c"))
                        {
                            mysocket1.SendDate("\r\n");
                        }
                        if (ver2.Contains("#"))
                        {
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Regex nms0 = new Regex(@"nms.fpga:\s*([\d\.\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    Regex code0 = new Regex(@"fpga_code.bin:\s*([\d\.\w\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string nmsfpga = nms0.Match(ver).Groups[1].Value;
                    string code = code0.Match(ver).Groups[1].Value;
                    if (nmsfpga.ToString() == "" || code.ToString() == "")
                    {
                        this.dataGridView1.Rows[i].Cells["FPGA版本"].Value = "获取失败";
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        lock (sb)
                        {
                            int s = int.Parse(toolStripStatusLabelshibai.Text);
                            s = s + 1;
                            toolStripStatusLabelshibai.Text = s.ToString();
                        }
                    }
                    else
                    {

                        this.dataGridView1.Rows[i].Cells["FPGA版本"].Value = "NMS:" + nmsfpga + "  CODE:" + code;
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                        if (nmsfpga.ToString() != null)
                        {
                            Regex wuma0 = null;
                            string wuma = "";
                            string wuma2 = "";
                            mysocket1.SendData("config msap");
                            string dd = mysocket1.ReceiveData(int.Parse(yanshi));
                            Thread.Sleep(200);
                            mysocket1.SendData("show vc4");
                            Thread.Sleep(200);
                            ver = "";
                            ver2 = "";
                            for (int a = 0; a <= 500; a++)
                            {
                                ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                                ver = ver + ver2;
                                if (ver2.Contains("Ctrl+c"))
                                {
                                    mysocket1.SendDate("\r\n");
                                }
                                if (ver2.Contains("17") || ver2.Contains("18"))
                                {
                                    break;
                                }
                                Thread.Sleep(10);
                            }
                            if (!ver.Contains("17") || !ver.Contains("18"))
                            {
                                this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "17或18槽不存在板卡";
                            }
                            if (ver.Contains("18") && !ver.Contains("17"))
                            {
                                wuma0 = new Regex(@"18\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma = wuma0.Match(ver).Groups[1].Value;
                                //dataGridView1.Rows[i].Cells["主控保护"].Value = "18主";

                            }
                            if (ver.Contains("18") && ver.Contains("17"))
                            {
                                wuma0 = new Regex(@"17\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma = wuma0.Match(ver).Groups[1].Value;
                                // dataGridView1.Rows[i].Cells["主控保护"].Value = "双主控";
                            }
                            if (!ver.Contains("18") || ver.Contains("17"))
                            {
                                wuma0 = new Regex(@"17\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma = wuma0.Match(ver).Groups[1].Value;
                                // dataGridView1.Rows[i].Cells["主控保护"].Value = "17主";
                            }

                            Thread.Sleep(int.Parse(comtime.Text));
                            mysocket1.SendData("show vc4");
                            Thread.Sleep(200);
                            ver = "";
                            ver2 = "";
                            for (int a = 0; a <= 500; a++)
                            {
                                ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                                ver = ver + ver2;
                                if (ver2.Contains("Ctrl+c"))
                                {
                                    mysocket1.SendDate("\r\n");
                                }
                                if (ver2.Contains("17") || ver2.Contains("18"))
                                {
                                    break;
                                }
                                Thread.Sleep(10);
                            }
                            if (ver.Contains("17") || !ver.Contains("18"))
                            {
                                wuma0 = new Regex(@"17\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma2 = wuma0.Match(ver).Groups[1].Value;
                                if (wuma.ToString() != "" && wuma2.ToString() != "")
                                {
                                    if (int.Parse(wuma.ToString()) == int.Parse(wuma2.ToString()))
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "没有误码";
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                                    }
                                    else
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "存在误码" + wuma2;
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                                    }
                                }

                            }
                            if (ver.Contains("17") && ver.Contains("18"))
                            {
                                wuma0 = new Regex(@"17\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma2 = wuma0.Match(ver).Groups[1].Value;
                                if (wuma.ToString() != "" && wuma2.ToString() != "")
                                {
                                    if (int.Parse(wuma.ToString()) == int.Parse(wuma2.ToString()))
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "没有误码";
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                                    }
                                    else
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "存在误码" + wuma2;
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                                    }
                                }

                            }
                            if (ver.Contains("18") && !ver.Contains("17"))
                            {
                                wuma0 = new Regex(@"18\s*\w*\s*([\d]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                wuma2 = wuma0.Match(ver).Groups[1].Value;
                                if (wuma.ToString() != "" && wuma2.ToString() != "")
                                {
                                    if (int.Parse(wuma.ToString()) == int.Parse(wuma2.ToString()))
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "没有误码";
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                                    }
                                    else
                                    {
                                        this.dataGridView1.Rows[i].Cells["主控背板误码"].Value = "存在误码" + wuma2;
                                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                                    }
                                }

                            }

                            mysocket1.SendData("exit");

                        }

                        lock (cg)
                        {
                            //MessageBox.Show("fff");
                            int cg = int.Parse(toolStripStatusLabelchenggong.Text);
                            cg = cg + 1;
                            toolStripStatusLabelchenggong.Text = cg.ToString();
                        }
                    }

                    //mysocket1.SendData("save");
                    //    for (int a = 1; a <= 5000; a++)
                    //    {
                    //        string box = mysocket1.ReceiveData(int.Parse("10")); ;
                    //        if (box.Contains("successfully"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                    //            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    //            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                    //            lock (cg)
                    //            {
                    //                int c = int.Parse(toolStripStatusLabelchenggong.Text);
                    //                c = c + 1;
                    //                toolStripStatusLabelchenggong.Text = c.ToString();
                    //                break;
                    //            }
                    //        }
                    //        if (box.Contains("erro"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                    //            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    //            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    //            lock (sb)
                    //            {
                    //                int s = int.Parse(toolStripStatusLabelshibai.Text);
                    //                s = s + 1;
                    //                toolStripStatusLabelshibai.Text = s.ToString();
                    //                break;
                    //            }
                    //        }

                    //        Thread.Sleep(1);
                    //    }
                    //    mysocket1.SendData("logout");

                }

                else
                {
                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    lock (sb)
                    {
                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                        s = s + 1;
                        toolStripStatusLabelshibai.Text = s.ToString();
                    }
                }


            }
            else
            {

                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }
            if (dataGridView1.Rows[i].Cells["最终结果"].Value.ToString() == "初始化")
            {
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;

                }
            }

            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
            //MessageBox.Show("一键保存结束");

        }
        //备份数据库函数
        public void Upload(object obj)
        {
            int i = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            Ping ping = new Ping();
            int timeout = 500;
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();

            // MessageBox.Show(ip);
            PingReply pingReply = ping.Send(ip, timeout);
            for (int j = 0; j <= 1; j++)
            {

                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(1000);
                pingReply = ping.Send(ip, timeout);
            }//判断请求是否超时
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");

                if (bo)
                {
                    mysocket1.SendData(GPNUSR);
                    for (int a = 0; a <= 1000; a++)
                    {
                        string login = mysocket1.ReceiveData(int.Parse(yanshi));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    for (int c = 0; c <= 1000; c++)
                    {
                        string passd = mysocket1.ReceiveData(int.Parse(yanshi));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Kerberos") || passd.Contains("Bad passwords"))
                        {

                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请重新输入");
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误";
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                            }
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                        }
                        if (passd.Contains(">"))
                        {
                            //textDOS.AppendText("\r\n" + "用户名密码正确==========================================OK");
                            mysocket1.SendData("enable");
                            for (int b = 0; b <= 1000; b++)
                            {
                                string pass = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket1.SendData(GPNPSDEN);
                                    //Thread.Sleep(500);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket1.ReceiveData(int.Parse(yanshi));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录========================OK");
                                            mysocket1.SendData("grosadvdebug");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("vty user limit no");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("exit");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("enable");
                                            Thread.Sleep(200);
                                            if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                            {
                                                mysocket1.SendData(GPNPSDEN);
                                                Thread.Sleep(200);
                                                if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                                {
                                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                                    lock (sb)
                                                    {
                                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                        s = s + 1;
                                                        toolStripStatusLabelshibai.Text = s.ToString();
                                                    }
                                                    lock (o1)
                                                    {
                                                        int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                        e = e - 1;
                                                        toolStripStatusLabelshengyu.Text = e.ToString();

                                                        doneCount++;

                                                    }
                                                    return;
                                                }

                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
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
                                    //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录=============================OK");
                                    mysocket1.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("enable");
                                    Thread.Sleep(200);
                                    if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                    {
                                        mysocket1.SendData(GPNPSDEN);
                                        Thread.Sleep(200);
                                        if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                        {
                                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                            lock (sb)
                                            {
                                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                s = s + 1;
                                                toolStripStatusLabelshibai.Text = s.ToString();
                                            }
                                            lock (o1)
                                            {
                                                int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                e = e - 1;
                                                toolStripStatusLabelshengyu.Text = e.ToString();

                                                doneCount++;

                                            }
                                            return;
                                        }

                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(1);
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }

                    this.dataGridView1.Rows[i].Cells["save"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["config"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["slotconfig"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["db"].Value = "初始化";
                    mysocket1.SendData("save");
                    for (int a = 1; a <= 1000; a++)
                    {

                        string box = mysocket1.ReceiveData(int.Parse(yanshi));
                        if (box.Contains("successfully"))
                        {
                            this.dataGridView1.Rows[i].Cells["save"].Value = "OK";
                            break;
                        }
                        if (box.Contains("erro"))
                        {
                            this.dataGridView1.Rows[i].Cells["save"].Value = "NOK";
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;


                        }

                        Thread.Sleep(10);
                    }
                    Thread.Sleep(1500);

                    mysocket1.SendData("upload ftp file /flash/sys/conf_data.txt " + FTPIP + " " + FTPUSR + " " + FTPPSD + " " + ip + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_config.txt");
                    for (int a = 1; a <= 3000; a++)
                    {


                        string box = mysocket1.ReceiveData(int.Parse(yanshi));

                        if (box.Contains("ok"))
                        {
                            this.dataGridView1.Rows[i].Cells["config"].Value = "OK";
                            break;
                        }
                        if (box.Contains("fail"))
                        {
                            this.dataGridView1.Rows[i].Cells["config"].Value = "检查FTP服务器IP地址";
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (box.Contains("User need password"))
                        {
                            this.dataGridView1.Rows[i].Cells["config"].Value = "检查FTP用户名密码";
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        Thread.Sleep(10);

                    }

                    //Thread.Sleep(1000);
                    mysocket1.SendData("upload ftp file /flash/sys/slotconfig.bin " + FTPIP + " " + FTPUSR + " " + FTPPSD + " " + ip + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_slotconfig.bin");
                    for (int a = 1; a <= 3000; a++)
                    {

                        string box = mysocket1.ReceiveData(int.Parse(yanshi));

                        if (box.Contains("ok"))
                        {
                            this.dataGridView1.Rows[i].Cells["slotconfig"].Value = "OK";
                            break;
                        }
                        if (box.Contains("fail"))
                        {
                            this.dataGridView1.Rows[i].Cells["slotconfig"].Value = "NOK";
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }

                        Thread.Sleep(10);

                    }

                    //Thread.Sleep(1000);

                    mysocket1.SendData("upload ftp file /flash/sys/db.bin " + FTPIP + " " + FTPUSR + " " + FTPPSD + " " + ip + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_db.bin");
                    for (int a = 1; a <= 3000; a++)
                    {

                        string box = mysocket1.ReceiveData(int.Parse(yanshi));

                        if (box.Contains("ok"))
                        {

                            this.dataGridView1.Rows[i].Cells["db"].Value = "OK";
                            break;
                        }
                        if (box.Contains("fail"))
                        {
                            this.dataGridView1.Rows[i].Cells["db"].Value = "NOK";
                            break;
                        }

                        Thread.Sleep(10);

                    }
                    string save = dataGridView1.Rows[i].Cells["save"].Value.ToString();
                    string config = dataGridView1.Rows[i].Cells["config"].Value.ToString();
                    string slotconfig = dataGridView1.Rows[i].Cells["slotconfig"].Value.ToString();
                    string db = dataGridView1.Rows[i].Cells["db"].Value.ToString();
                    if (save == "OK" && config == "OK" && slotconfig == "OK" && db == "OK")
                    {
                        lock (cg)
                        {
                            int c = int.Parse(toolStripStatusLabelchenggong.Text);
                            c = c + 1;
                            toolStripStatusLabelchenggong.Text = c.ToString();

                        }
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                    }
                    else
                    {
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                        lock (sb)
                        {
                            int s = int.Parse(toolStripStatusLabelshibai.Text);
                            s = s + 1;
                            toolStripStatusLabelshibai.Text = s.ToString();

                        }
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                    }


                }
                else
                {
                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    lock (sb)
                    {
                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                        s = s + 1;
                        toolStripStatusLabelshibai.Text = s.ToString();
                    }
                }
            }

            else
            {

                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }
            if (dataGridView1.Rows[i].Cells["最终结果"].Value.ToString() == "初始化")
            {
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;

                }
            }
            if (dataGridView1.Rows[i].Cells["最终结果"].Value.ToString() == "初始化")
            {
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;

                }
            }
            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
            //MessageBox.Show("一键保存结束");

        }
        public static object o1 = new object();
        public static object sb = new object();
        public static object cg = new object();

        //保存按钮
        private void Butsave_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            if (dataGridView1.Columns["升级后当前版本"] == null)
            {

                this.dataGridView1.Columns.Add("升级后当前版本", "升级后当前版本");
            }
            else
            {
                this.dataGridView1.Columns.Remove("升级后当前版本");
                this.dataGridView1.Columns.Add("升级后当前版本", "升级后当前版本");
            }
            if (dataGridView1.Columns["OMU序列号"] == null)
            {

                this.dataGridView1.Columns.Add("OMU序列号", "OMU序列号");
            }
            else
            {
                this.dataGridView1.Columns.Remove("OMU序列号");
                this.dataGridView1.Columns.Add("OMU序列号", "OMU序列号");
            }
            if (dataGridView1.Columns["NMS序列号"] == null)
            {

                this.dataGridView1.Columns.Add("NMS序列号", "NMS序列号");
            }
            else
            {
                this.dataGridView1.Columns.Remove("NMS序列号");
                this.dataGridView1.Columns.Add("NMS序列号", "NMS序列号");
            }
            if (dataGridView1.Columns["NMS硬件版本"] == null)
            {

                this.dataGridView1.Columns.Add("NMS硬件版本", "NMS硬件版本");
            }
            else
            {
                this.dataGridView1.Columns.Remove("NMS硬件版本");
                this.dataGridView1.Columns.Add("NMS硬件版本", "NMS硬件版本");
            }
            if (dataGridView1.Columns["FPGA版本"] == null)
            {

                this.dataGridView1.Columns.Add("FPGA版本", "FPGA版本");
            }
            else
            {
                this.dataGridView1.Columns.Remove("FPGA版本");
                this.dataGridView1.Columns.Add("FPGA版本", "FPGA版本");
            }
            if (dataGridView1.Columns["主控背板误码"] == null)
            {

                this.dataGridView1.Columns.Add("主控背板误码", "主控背板误码");
            }
            else
            {
                this.dataGridView1.Columns.Remove("主控背板误码");
                this.dataGridView1.Columns.Add("主控背板误码", "主控背板误码");
            }
            if (dataGridView1.Columns["主控保护"] == null)
            {

                this.dataGridView1.Columns.Add("主控保护", "主控保护");
            }
            else
            {
                this.dataGridView1.Columns.Remove("主控保护");
                this.dataGridView1.Columns.Add("主控保护", "主控保护");
            }
            TimeNow = DateTime.Now;

            this.timer1.Enabled = true;
            this.timer1.Start();
            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";

            string task = "save";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(Bartest)
            {
                IsBackground = true
            };
            bar.Start();
        }

        //线程池
        private void Xianchengchi(object obj)
        {
            ThreadPool.SetMinThreads(int.Parse(comMinThreads.Text), 1);
            ThreadPool.SetMaxThreads(int.Parse(comMaxThreads.Text), 2);
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                if (obj.ToString() == "link")
                {
                    Thread.Sleep(500);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Link), i.ToString());

                }
                if (obj.ToString() == "save")
                {
                    Thread.Sleep(500);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Save), i.ToString());

                }
                if (obj.ToString() == "upload")
                {
                    Thread.Sleep(500);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Upload), i.ToString());

                }
                if (obj.ToString() == "download")
                {
                    Thread.Sleep(500);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Download), i.ToString());

                }
                if (obj.ToString() == "reboot" && (bool)dataGridView1.Rows[i].Cells["重启选择"].EditedFormattedValue == true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Reboot), i.ToString());
                }
                if (obj.ToString() == "checkver")
                {
                    Thread.Sleep(500);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Checkver), i.ToString());

                }

            }




        }
        //进度条
        private void Bartest()
        {
            for (int a = 0; a <= 1000000000; a++)
            {
                int all = int.Parse(toolStripStatusLabelzonggong.Text);
                int cg = int.Parse(toolStripStatusLabelchenggong.Text);
                int sb = int.Parse(toolStripStatusLabelshibai.Text);
                int dzx = all - cg - sb;

                toolStripStatusLabelyichang.Text = dzx.ToString();
                dataGridView1.CurrentCell = dataGridView1.Rows[doneCount].Cells[0];
                this.toolStripProgressBar1.Maximum = dataGridView1.Rows.Count - 1;
                this.toolStripProgressBar1.Value = doneCount;
                int n = doneCount * 100 / (dataGridView1.Rows.Count - 1);
                toolStripStatusLabeljindu.Text = n.ToString() + "%";
                if (doneCount == dataGridView1.Rows.Count - 1)
                {
                    //dataGridView1.CurrentCell = dataGridView1.Rows[doneCount].Cells[0];

                    this.toolStripProgressBar1.Value = doneCount;
                    toolStripStatusLabeljindu.Text = "100%";
                    //Mytimer.Change(Timeout.Infinite, 1000);
                    MessageBox.Show("批量操作结束！" + "\n" +
                        "\n" +
                        "一共：" + all + "台！" + "\n" +
                        "\n" +
                        "成功：" + cg + "台！" + "\n" +
                        "\n" +
                        "失败：" + sb + "台！");
                    if (dataGridView1.Columns["重启选择"] != null)
                    {
                        butreboot.Enabled = true;

                    }

                    this.timer1.Enabled = false;
                    this.timer1.Stop();
                    break;
                }
                Thread.Sleep(1000);
            }

        }
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
            metroCom603gsysfile.Text = "否";
        }
        #endregion

        //备份数据按钮
        private void Butupload_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            TimeNow = DateTime.Now;
            //TimeCount = 0;
            //Mytimer.Change(0, 1000);
            this.timer1.Enabled = true;
            this.timer1.Start();
            if (dataGridView1.Columns["save"] == null)
            {

                this.dataGridView1.Columns.Add("save", "save");
            }
            else
            {
                this.dataGridView1.Columns.Remove("save");
                this.dataGridView1.Columns.Add("save", "save");
            }
            if (dataGridView1.Columns["config"] == null)
            {

                this.dataGridView1.Columns.Add("config", "config");
            }
            else
            {
                this.dataGridView1.Columns.Remove("config");
                this.dataGridView1.Columns.Add("config", "config");
            }
            if (dataGridView1.Columns["slotconfig"] == null)
            {

                this.dataGridView1.Columns.Add("slotconfig", "slotconfig");

            }
            else
            {
                this.dataGridView1.Columns.Remove("slotconfig");
                this.dataGridView1.Columns.Add("slotconfig", "slotconfig");
            }
            if (dataGridView1.Columns["db"] == null)
            {
                this.dataGridView1.Columns.Add("db", "db");
            }
            else
            {
                this.dataGridView1.Columns.Remove("db");
                this.dataGridView1.Columns.Add("db", "db");
            }


            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";

            string task = "upload";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(Bartest)
            {
                IsBackground = true
            };
            bar.Start();

        }
        System.DateTime TimeNow = new DateTime();
        TimeSpan time = new TimeSpan();
        //计时器1
        private void Timer1_Tick(object sender, EventArgs e)
        {
            time = DateTime.Now - TimeNow;
            toolStripStatusLabeltime.Text = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        }
        //下载升级按钮
        private void Butdown_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            if (app.Contains(".bin"))
            {
                DialogResult dr = MessageBox.Show("即将下载APP版本：" + app + " 是否确认升级？", "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    TimeNow = DateTime.Now;
                    //TimeCount = 0;
                    //Mytimer.Change(0, 1000);
                    this.timer1.Enabled = true;
                    this.timer1.Start();
                    if (dataGridView1.Columns["升级前当前版本"] == null)
                    {

                        this.dataGridView1.Columns.Add("升级前当前版本", "升级前当前版本");

                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("升级前当前版本");
                        this.dataGridView1.Columns.Add("升级前当前版本", "升级前当前版本");
                    }
                    //if (dataGridView1.Columns["save"] == null)
                    //{

                    //    this.dataGridView1.Columns.Add("save", "save");
                    //}
                    //else
                    //{
                    //    this.dataGridView1.Columns.Remove("save");
                    //    this.dataGridView1.Columns.Add("save", "save");
                    //}
                    //if (dataGridView1.Columns["备份config"] == null)
                    //{

                    //    this.dataGridView1.Columns.Add("备份config", "备份config");
                    //}
                    //else
                    //{
                    //    this.dataGridView1.Columns.Remove("备份config");
                    //    this.dataGridView1.Columns.Add("备份config", "备份config");
                    //}
                    if (dataGridView1.Columns["删除sysfile"] == null)
                    {

                        this.dataGridView1.Columns.Add("删除sysfile", "删除sysfile");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("删除sysfile");
                        this.dataGridView1.Columns.Add("删除sysfile", "删除sysfile");
                    }
                    if (dataGridView1.Columns["二次检查sysfile"] == null)
                    {

                        this.dataGridView1.Columns.Add("二次检查sysfile", "二次检查sysfile");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("二次检查sysfile");
                        this.dataGridView1.Columns.Add("二次检查sysfile", "二次检查sysfile");
                    }

                    if (dataGridView1.Columns["下载APP"] == null)
                    {
                        this.dataGridView1.Columns.Add("下载APP", "下载APP");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("下载APP");
                        this.dataGridView1.Columns.Add("下载APP", "下载APP");
                    }
                    if (dataGridView1.Columns["写入APP"] == null)
                    {
                        this.dataGridView1.Columns.Add("写入APP", "写入APP");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("写入APP");
                        this.dataGridView1.Columns.Add("写入APP", "写入APP");
                    }
                    if (dataGridView1.Columns["清空配置"] == null)
                    {

                        this.dataGridView1.Columns.Add("清空配置", "清空配置");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("清空配置");
                        this.dataGridView1.Columns.Add("清空配置", "清空配置");
                    }
                    if (dataGridView1.Columns["重启选择"] == null)
                    {
                        DataGridViewCheckBoxColumn newColumn = new DataGridViewCheckBoxColumn
                        {
                            Name = "重启选择",
                            HeaderText = "重启选择"
                        };
                        dataGridView1.Columns.Add(newColumn);
                    }


                    doneCount = 0;
                    toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
                    toolStripStatusLabelchenggong.Text = "0";
                    toolStripStatusLabelshibai.Text = "0";
                    toolStripStatusLabelyichang.Text = "0";

                    string task = "download";
                    ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
                    Thread t = new Thread(p);
                    t.Start(task);
                    Thread bar = new Thread(Bartest)
                    {
                        IsBackground = true
                    };
                    bar.Start();


                    //户选择确认的操作
                }
                else if (dr == DialogResult.No)
                {
                    //户选择取消的操作

                    return;
                }
            }
            else
            {
                MessageBox.Show("请选择APP、FPGA等文件然后升级");
            }

        }
        //下载升级函数
        public void Download(object obj)
        {
            int i = int.Parse(obj.ToString());
            Thread.Sleep(500);
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            this.dataGridView1.Rows[i].Cells["重启选择"].Value = false;
            Ping ping = new Ping();
            int timeout = 500;
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();

            // MessageBox.Show(ip);
            PingReply pingReply = ping.Send(ip, timeout);
            for (int j = 0; j <= 1; j++)
            {
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }


                Thread.Sleep(1000);
                pingReply = ping.Send(ip, timeout);
            }//判断请求是否超时
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");

                if (bo)
                {
                    mysocket1.SendData(GPNUSR);
                    for (int a = 0; a <= 1000; a++)
                    {
                        string login = mysocket1.ReceiveData(int.Parse(yanshi));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    for (int c = 0; c <= 1000; c++)
                    {
                        string passd = mysocket1.ReceiveData(int.Parse(yanshi));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Kerberos") || passd.Contains("Bad passwords"))
                        {

                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请重新输入");
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误";
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                            }
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                        }
                        if (passd.Contains(">"))
                        {
                            //textDOS.AppendText("\r\n" + "用户名密码正确==========================================OK");
                            mysocket1.SendData("enable");
                            for (int b = 0; b <= 1000; b++)
                            {
                                string pass = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket1.SendData(GPNPSDEN);
                                    //Thread.Sleep(500);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket1.ReceiveData(int.Parse(yanshi));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录========================OK");
                                            mysocket1.SendData("grosadvdebug");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("vty user limit no");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("exit");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("enable");
                                            Thread.Sleep(200);
                                            if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                            {
                                                mysocket1.SendData(GPNPSDEN);
                                                Thread.Sleep(200);
                                                if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                                {
                                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                                    lock (sb)
                                                    {
                                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                        s = s + 1;
                                                        toolStripStatusLabelshibai.Text = s.ToString();
                                                    }
                                                    lock (o1)
                                                    {
                                                        int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                        e = e - 1;
                                                        toolStripStatusLabelshengyu.Text = e.ToString();

                                                        doneCount++;

                                                    }
                                                    return;
                                                }

                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
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
                                    //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录=============================OK");
                                    mysocket1.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("enable");
                                    Thread.Sleep(200);
                                    if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                    {
                                        mysocket1.SendData(GPNPSDEN);
                                        Thread.Sleep(200);
                                        if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                        {
                                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                            lock (sb)
                                            {
                                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                s = s + 1;
                                                toolStripStatusLabelshibai.Text = s.ToString();
                                            }
                                            lock (o1)
                                            {
                                                int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                e = e - 1;
                                                toolStripStatusLabelshengyu.Text = e.ToString();

                                                doneCount++;

                                            }
                                            return;
                                        }

                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(1);
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }

                    this.dataGridView1.Rows[i].Cells["升级前当前版本"].Value = "初始化";
                    //this.dataGridView1.Rows[i].Cells["save"].Value = "初始化";
                    //this.dataGridView1.Rows[i].Cells["备份config"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["删除sysfile"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["二次检查sysfile"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["下载APP"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["写入APP"].Value = "初始化";
                    this.dataGridView1.Rows[i].Cells["清空配置"].Value = "初始化";

                    mysocket1.SendData("show ver");
                    string ver = "";
                    string ver2 = "";
                    for (int a = 0; a <= 1000; a++)
                    {
                        ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                        ver = ver + ver2;
                        if (ver2.Contains("Ctrl+c"))
                        {
                            mysocket1.SendDate("\r\n");
                        }
                        if (ver2.Contains("#"))
                        {
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Regex r = new Regex(@"ProductOS\s*Version\s*([\w\d]+)[\s*\(]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string banben = r.Match(ver).Groups[1].Value;
                    this.dataGridView1.Rows[i].Cells["升级前当前版本"].Value = banben.ToString();


                    //mysocket1.SendData("save");
                    //    for (int a = 1; a <= 1000; a++)
                    //    {

                    //        string box = mysocket1.ReceiveData(int.Parse(yanshi));
                    //        if (box.Contains("successfully"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["save"].Value = "OK";
                    //            break;
                    //        }
                    //        if (box.Contains("erro"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["save"].Value = "NOK";
                    //            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                    //            lock (sb)
                    //            {
                    //                int s = int.Parse(toolStripStatusLabelshibai.Text);
                    //                s = s + 1;
                    //                toolStripStatusLabelshibai.Text = s.ToString();

                    //            }
                    //            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    //            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    //            lock (o1)
                    //            {
                    //                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                    //                d = d - 1;
                    //                toolStripStatusLabelshengyu.Text = d.ToString();

                    //                doneCount++;

                    //            }
                    //            return;


                    //        }

                    //        Thread.Sleep(10);
                    //    }
                    //Thread.Sleep(1500);

                    //mysocket1.senddata("upload ftp file /flash/sys/conf_data.txt " + ftpip + " " + ftpusr + " " + ftppsd + " " + ip + "_" + datetime.now.tostring("yyyy-mm-dd") + "_config.txt");
                    //for (int a = 1; a <= 3000; a++)
                    //{


                    //    string box = mysocket1.receivedata(int.parse(yanshi));

                    //    if (box.contains("ok"))
                    //    {
                    //        this.datagridview1.rows[i].cells["备份config"].value = "ok";
                    //        break;
                    //    }
                    //    if (box.contains("fail"))
                    //    {
                    //        this.datagridview1.rows[i].cells["备份config"].value = "检查ftp服务器ip地址";
                    //        this.datagridview1.rows[i].cells["最终结果"].value = "失败";

                    //        lock (sb)
                    //        {
                    //            int s = int.parse(toolstripstatuslabelshibai.text);
                    //            s = s + 1;
                    //            toolstripstatuslabelshibai.text = s.tostring();

                    //        }
                    //        this.datagridview1.rows[i].cells["结束时间"].value = datetime.now.tostring();
                    //        this.datagridview1.rows[i].defaultcellstyle.backcolor = color.yellow;
                    //        lock (o1)
                    //        {
                    //            int d = int.parse(toolstripstatuslabelshengyu.text);
                    //            d = d - 1;
                    //            toolstripstatuslabelshengyu.text = d.tostring();

                    //            donecount++;

                    //        }
                    //        return;
                    //    }
                    //    if (box.contains("user need password"))
                    //    {
                    //        this.datagridview1.rows[i].cells["备份config"].value = "检查ftp用户名密码";
                    //        this.datagridview1.rows[i].cells["最终结果"].value = "失败";

                    //        lock (sb)
                    //        {
                    //            int s = int.parse(toolstripstatuslabelshibai.text);
                    //            s = s + 1;
                    //            toolstripstatuslabelshibai.text = s.tostring();

                    //        }
                    //        this.datagridview1.rows[i].cells["结束时间"].value = datetime.now.tostring();
                    //        this.datagridview1.rows[i].defaultcellstyle.backcolor = color.yellow;
                    //        lock (o1)
                    //        {
                    //            int d = int.parse(toolstripstatuslabelshengyu.text);
                    //            d = d - 1;
                    //            toolstripstatuslabelshengyu.text = d.tostring();

                    //            donecount++;

                    //        }
                    //        return;
                    //    }
                    //    thread.sleep(10);

                    //}
                    mysocket1.SendData("grosadvdebug");
                    Thread.Sleep(500);
                    mysocket1.SendData("shell");
                    Thread.Sleep(500);
                    if (metroCom603gsysfile.Text == "是")
                    {
                        mysocket1.SendData("system2 \"rm /mnt/flash/gwd/sysfile_ini.bin\"");
                        Thread.Sleep(5000);
                        string SHANCHUSYSFILE = mysocket1.ReceiveData(int.Parse(yanshi));
                        this.dataGridView1.Rows[i].Cells["删除sysfile"].Value = "已执行删除";


                    }
                    else {
                        this.dataGridView1.Rows[i].Cells["删除sysfile"].Value = "不执行删除";
                    }

                    mysocket1.SendData("system2 \"ls -all /mnt/flash/gwd\"");
                    Thread.Sleep(5000);
                    string chaxun = mysocket1.ReceiveData(int.Parse(yanshi));
                    if (chaxun.Contains("sysfile_ini.bin"))
                    {
                        this.dataGridView1.Rows[i].Cells["二次检查sysfile"].Value = "存在Sysfile";
                        if (metroCom603gsysfile.Text == "是")
                        {
                            mysocket1.SendData("system2 \"rm /mnt/flash/gwd/sysfile_ini.bin\"");
                            this.dataGridView1.Rows[i].Cells["二次检查sysfile"].Value = "不存在Sysfile";


                        }

                        Thread.Sleep(500);

                    }
                    else
                    {
                        this.dataGridView1.Rows[i].Cells["二次检查sysfile"].Value = "不存在Sysfile";
                    }
                    mysocket1.SendData("exit");
                    Thread.Sleep(500);
                    mysocket1.SendData("exit");
                    Thread.Sleep(500);

                    mysocket1.SendData("download ftp app " + FTPIP + " " + FTPUSR + " " + FTPPSD + " " + app + " gpn");
                    for (int a = 1; a <= 300; a++)
                    {

                        string command = mysocket1.ReceiveData(int.Parse(yanshi));
                        if (command.Contains("Download file ...ok"))
                        {
                            this.dataGridView1.Rows[i].Cells["下载APP"].Value = "OK";

                            for (int b = 1; b <= 300; b++)
                            {
                                string download = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (download.Contains("ok"))
                                {
                                    this.dataGridView1.Rows[i].Cells["写入APP"].Value = "OK";
                                    break;
                                }
                                if (command.Contains("failed"))
                                {
                                    this.dataGridView1.Rows[i].Cells["写入APP"].Value = "NOK";
                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                                    lock (sb)
                                    {
                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                        s = s + 1;
                                        toolStripStatusLabelshibai.Text = s.ToString();

                                    }
                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                    lock (o1)
                                    {
                                        int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                        d = d - 1;
                                        toolStripStatusLabelshengyu.Text = d.ToString();

                                        doneCount++;

                                    }
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            break;
                        }
                        if (command.Contains("failed"))
                        {
                            this.dataGridView1.Rows[i].Cells["下载APP"].Value = "NOK";
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                    mysocket1.SendData("erase config-file");
                    Thread.Sleep(500);
                    mysocket1.SendData("Y");
                    Thread.Sleep(500);
                    this.dataGridView1.Rows[i].Cells["清空配置"].Value = "OK";

                    string save = dataGridView1.Rows[i].Cells["删除sysfile"].Value.ToString();
                    string config = dataGridView1.Rows[i].Cells["二次检查sysfile"].Value.ToString();
                    string downloadapp = dataGridView1.Rows[i].Cells["下载APP"].Value.ToString();
                    string writeloadapp = dataGridView1.Rows[i].Cells["写入APP"].Value.ToString();
                    string erase = dataGridView1.Rows[i].Cells["清空配置"].Value.ToString();
                    if (metroCom603gsysfile.Text == "否") {
                        if (save == "不执行删除" && config == "存在Sysfile" && downloadapp == "OK" && writeloadapp == "OK" && erase == "OK")
                        {
                            lock (cg)
                            {
                                int c = int.Parse(toolStripStatusLabelchenggong.Text);
                                c = c + 1;
                                toolStripStatusLabelchenggong.Text = c.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                            this.dataGridView1.Rows[i].Cells["重启选择"].Value = true;
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                        }
                        else
                        {

                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                        }
                    }
                    if (metroCom603gsysfile.Text == "是") {
                        if (save == "已执行删除" && config == "不存在Sysfile" && downloadapp == "OK" && writeloadapp == "OK" && erase == "OK")
                        {
                            lock (cg)
                            {
                                int c = int.Parse(toolStripStatusLabelchenggong.Text);
                                c = c + 1;
                                toolStripStatusLabelchenggong.Text = c.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                            this.dataGridView1.Rows[i].Cells["重启选择"].Value = true;
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                        }
                        else
                        {

                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();

                            }
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                        }
                    }





                }
                else
                {
                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    lock (sb)
                    {
                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                        s = s + 1;
                        toolStripStatusLabelshibai.Text = s.ToString();
                    }
                }
            }
            else
            {

                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }
            if (dataGridView1.Rows[i].Cells["最终结果"].Value.ToString() == "初始化")
            {
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;

                }
            }

            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
        }

        //重启按钮
        private void Butreboot_Click(object sender, EventArgs e)
        {

            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            if (dataGridView1.Columns["重启前清空配置"] == null)
            {
                this.dataGridView1.Columns.Add("重启前清空配置", "重启前清空配置");
            }
            else
            {
                this.dataGridView1.Columns.Remove("重启前清空配置");
                this.dataGridView1.Columns.Add("重启前清空配置", "重启前清空配置");
            }
            if (dataGridView1.Columns["重启结果"] == null)
            {
                this.dataGridView1.Columns.Add("重启结果", "重启结果");
            }
            else
            {
                this.dataGridView1.Columns.Remove("重启结果");
                this.dataGridView1.Columns.Add("重启结果", "重启结果");
            }

            TimeNow = DateTime.Now;
            //TimeCount = 0;
            //Mytimer.Change(0, 1000);
            this.timer1.Enabled = true;
            this.timer1.Start();
            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";
            string task = "reboot";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(Rebootbar)
            {
                IsBackground = true
            };
            bar.Start();

        }
        //重启进度条
        private void Rebootbar()
        {
            for (int a = 0; a <= 1000000000; a++)
            {
                int all = int.Parse(toolStripStatusLabelzonggong.Text);
                int cg = int.Parse(toolStripStatusLabelchenggong.Text);
                int sb = int.Parse(toolStripStatusLabelshibai.Text);
                int dzx = all - cg - sb;

                toolStripStatusLabelyichang.Text = dzx.ToString();
                int b = 0;
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    if ((bool)dataGridView1.Rows[i].Cells["重启选择"].EditedFormattedValue == true)
                    {
                        b++;
                    }
                }
                //dataGridView1.CurrentCell = dataGridView1.Rows[doneCount].Cells[0];
                this.toolStripProgressBar1.Maximum = b;
                this.toolStripProgressBar1.Value = doneCount;
                int n = doneCount * 100 / (b);
                toolStripStatusLabeljindu.Text = n.ToString() + "%";
                if (doneCount == b)
                {
                    // dataGridView1.CurrentCell = dataGridView1.Rows[doneCount].Cells[0];

                    this.toolStripProgressBar1.Value = doneCount;
                    toolStripStatusLabeljindu.Text = "100%";
                    //Mytimer.Change(Timeout.Infinite, 1000);
                    MessageBox.Show("重启完成，如果是603G，请等待1分30分后，点击 【检查版本】进行最终确认！");
                    butreboot.Enabled = false;
                    this.timer1.Enabled = false;
                    this.timer1.Stop();
                    break;
                }
                Thread.Sleep(1000);
            }

        }
        //重启函数
        public void Reboot(object obj)
        {

            int i = int.Parse(obj.ToString());

            MySocket mysocket1 = new MySocket();
            string ip = "";
            ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();
            Ping ping = new Ping();
            int timeout = 500;
            PingReply pingReply = ping.Send(ip, timeout);
            // MessageBox.Show(ip);
            //判断请求是否超时
            for (int j = 0; j <= 1; j++)
            {
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(1000);
                pingReply = ping.Send(ip, timeout);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["重启结果"].Value = "初始化";
                this.dataGridView1.Rows[i].Cells["重启前清空配置"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;


                if (mysocket1.Connect(ip, "23"))
                {

                    mysocket1.SendData(GPNUSR);
                    for (int a = 0; a <= 1000; a++)
                    {
                        string login = mysocket1.ReceiveData(int.Parse(yanshi));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    for (int c = 0; c <= 1000; c++)
                    {
                        string passd = mysocket1.ReceiveData(int.Parse(yanshi));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Kerberos") || passd.Contains("Bad passwords"))
                        {

                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请重新输入");
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误";
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                            }
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                        }
                        if (passd.Contains(">"))
                        {
                            //textDOS.AppendText("\r\n" + "用户名密码正确==========================================OK");
                            mysocket1.SendData("enable");
                            for (int b = 0; b <= 1000; b++)
                            {
                                string pass = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket1.SendData(GPNPSDEN);
                                    //Thread.Sleep(500);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket1.ReceiveData(int.Parse(yanshi));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录========================OK");
                                            mysocket1.SendData("grosadvdebug");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("vty user limit no");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("exit");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("enable");
                                            Thread.Sleep(200);
                                            if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                            {
                                                mysocket1.SendData(GPNPSDEN);
                                                Thread.Sleep(200);
                                                if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                                {
                                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                                    lock (sb)
                                                    {
                                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                        s = s + 1;
                                                        toolStripStatusLabelshibai.Text = s.ToString();
                                                    }
                                                    lock (o1)
                                                    {
                                                        int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                        e = e - 1;
                                                        toolStripStatusLabelshengyu.Text = e.ToString();

                                                        doneCount++;

                                                    }
                                                    return;
                                                }

                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
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
                                    //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录=============================OK");
                                    mysocket1.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("enable");
                                    Thread.Sleep(200);
                                    if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                    {
                                        mysocket1.SendData(GPNPSDEN);
                                        Thread.Sleep(200);
                                        if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                        {
                                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                            lock (sb)
                                            {
                                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                s = s + 1;
                                                toolStripStatusLabelshibai.Text = s.ToString();
                                            }
                                            lock (o1)
                                            {
                                                int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                e = e - 1;
                                                toolStripStatusLabelshengyu.Text = e.ToString();

                                                doneCount++;

                                            }
                                            return;
                                        }

                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(1);
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }

                    mysocket1.SendData("erase config-file");
                    Thread.Sleep(500);
                    mysocket1.SendData("Y");
                    Thread.Sleep(500);
                    this.dataGridView1.Rows[i].Cells["重启前清空配置"].Value = "OK";

                    mysocket1.SendData("reboot");
                    for (int a = 1; a <= 5000; a++)
                    {
                        string box = mysocket1.ReceiveData(int.Parse("10")); ;
                        if (box.Contains("Are you sure want to reboot switch system? [Y/N]"))
                        {
                            mysocket1.SendData("Y");

                            this.dataGridView1.Rows[i].Cells["重启结果"].Value = "OK";
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;

                            lock (cg)
                            {
                                int c = int.Parse(toolStripStatusLabelchenggong.Text);
                                c = c + 1;
                                toolStripStatusLabelchenggong.Text = c.ToString();
                                break;
                            }
                        }
                        if (box.Contains("erro"))
                        {
                            this.dataGridView1.Rows[i].Cells["重启结果"].Value = "NOK";

                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                                break;
                            }
                        }

                        Thread.Sleep(1);
                    }


                }
                else
                {
                    this.dataGridView1.Rows[i].Cells["重启前清空配置"].Value = "NOK";
                    this.dataGridView1.Rows[i].Cells["重启结果"].Value = "NOK";

                    lock (sb)
                    {
                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                        s = s + 1;
                        toolStripStatusLabelshibai.Text = s.ToString();
                    }
                }
            }
            else
            {
                this.dataGridView1.Rows[i].Cells["重启前清空配置"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["重启结果"].Value = "NOK";

                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }


            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
            //MessageBox.Show("一键保存结束");

        }

        private void Batch_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color FColor = Color.White;
            Color TColor = Color.YellowGreen;

            if (this.ClientRectangle.Height != 0)
            {

                Brush b = new LinearGradientBrush(this.ClientRectangle, FColor, TColor, LinearGradientMode.Vertical);


                g.FillRectangle(b, this.ClientRectangle);
            }
        }
        //重绘窗体函数
        private void Batch_Resize(object sender, EventArgs e)
        {
            this.Invalidate();//重绘窗体
        }
        //连接测试按钮
        private void Butlink_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            TimeNow = DateTime.Now;
            this.timer1.Enabled = true;
            this.timer1.Start();
            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";
            string task = "link";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(Bartest)
            {
                IsBackground = true
            };
            bar.Start();
        }
        //连接测试函数
        public void Link(object obj)
        {

            int i = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();
            //Ping p1 = new Ping();
            //p1.SendAsync(ip, 10000, obj);
            //p1.PingCompleted += new PingCompletedEventHandler(PingCompletedCallBack);//设置PingCompleted事件处理程序 

            Ping ping = new Ping();
            //ing.SendAsync(ip, timeout);
            PingReply pingReply = ping.Send(ip, 400);
            for (int a = 0; a <= 1; a++)
            {
                if (pingReply.Status == IPStatus.Success)
                {
                    this.dataGridView1.Rows[i].Cells["ping测试"].Value = pingReply.RoundtripTime;
                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "在线";
                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                    lock (cg)
                    {
                        int c = int.Parse(toolStripStatusLabelchenggong.Text);
                        c = c + 1;
                        toolStripStatusLabelchenggong.Text = c.ToString();

                    }

                    break;
                }

                Thread.Sleep(1000);
                pingReply = ping.Send(ip);
            }
            if (pingReply.Status != IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = pingReply.Status;
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "离线";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }

            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
            //MessageBox.Show("一键保存结束");

        }
        //生成IP表格函数

        private void PingCompletedCallBack(object sender, PingCompletedEventArgs e)
        {

            //Thread.Sleep(1000);
            string a = e.UserState as string;
            // MessageBox.Show(a);
            int i = int.Parse(a.ToString());
            if (e.Cancelled)
            {
                MessageBox.Show("Ping Canncel");
                return;
            }
            if (e.Error != null)
            {
                //listBox1.Items.Add(e.Error.Message);
                return;

            }
            StringBuilder sbuilder;
            PingReply reply = e.Reply;
            if (reply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = reply.RoundtripTime;
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "在线";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                lock (cg)
                {
                    int c = int.Parse(toolStripStatusLabelchenggong.Text);
                    c = c + 1;
                    toolStripStatusLabelchenggong.Text = c.ToString();

                }
                sbuilder = new StringBuilder();
                sbuilder.Append(string.Format("Address: {0} ", reply.Address.ToString()));
                sbuilder.Append(string.Format("RoundTrip time: {0} ", reply.RoundtripTime));
                sbuilder.Append(string.Format("Time to live: {0} ", reply.Options.Ttl));
                sbuilder.Append(string.Format("Don't fragment: {0} ", reply.Options.DontFragment));
                sbuilder.Append(string.Format("Buffer size: {0} ", reply.Buffer.Length));
                //listBox1.Items.Add(sbuilder.ToString());

            }
            if (reply.Status != IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = reply.Status;
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "离线";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }
            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
        }
        public void Newip()
        {

            Control.CheckForIllegalCrossThreadCalls = false;
            string StartIp = TextStartIp.Text;
            string EndIp = TextStopIp.Text;
            uint iStartip = IpTint(StartIp);
            uint iEndIp = IpTint(EndIp);
            //string ip_result="";  
            StringBuilder ip_result = new StringBuilder();
            if (iEndIp >= iStartip)
            {
                for (uint ip = iStartip; ip <= iEndIp; ip++)
                {
                    String[] values = { ip_result + IntTip(ip) };
                    dataGridView1.Rows.Add(values);
                    //ip_result = ip_result + intTip(ip)+"\r\n";  
                }
                //String[] values = { ip_result.ToString() }; dataGridView1.Rows.Add(values);

                // resultTextBox.Text = ip_result.ToString();   //RichTextBox  
            }
            else
            {
                MessageBox.Show("错误！起始ip不能比终止ip大");
            }



            //int a = int.Parse(textAstop.Text) - int.Parse(textAstart.Text);
            //int b = int.Parse(textBstop.Text) - int.Parse(textBstart.Text);
            //int c = int.Parse(textCstop.Text) - int.Parse(textCstart.Text);
            //int d = int.Parse(textDstop.Text) - int.Parse(textDstart.Text);
            //int astart = int.Parse(textAstart.Text);
            //int bstart = int.Parse(textBstart.Text);
            //int cstart = int.Parse(textCstart.Text);
            //int dstart = int.Parse(textDstart.Text);


            //for (int i = 0; i <= c; i++)
            //{
            //    if (c == 0)
            //    {
            //        for (int j = int.Parse(textDstart.Text); j <= int.Parse(textDstop.Text); j++)
            //        {
            //            //int index = this.dataGridView1.Rows.Add();
            //            //this.dataGridView1.Rows[index].Cells["地址"].Value = textAstart.Text + "." + textBstart.Text + "." + textCstart.Text + "." + j.ToString();
            //            // dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
            //            //textBox1.AppendText(textAstart.Text + "." + textBstart.Text + "." + textCstart.Text + "." + j.ToString() + "\r\n");
            //            //MessageBox.Show(index.ToString());
            //            String[] values = { textAstart.Text + "." + textBstart.Text + "." + textCstart.Text + "." + j.ToString() }; dataGridView1.Rows.Add(values);
            //            //MessageBox.Show(j.ToString());
            //        }

            //    }
            //    if( c!=0 )
            //    {
            //        for (int q = 1; q <= 254; q++)
            //        {
            //            int cip = cstart + i;
            //            int index = this.dataGridView1.Rows.Add();
            //            this.dataGridView1.Rows[index].Cells["地址"].Value = textAstart.Text + "." + textBstart.Text + "." + cip.ToString() + "." + q.ToString();
            //           // dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
            //            //textBox1.AppendText(textAstart.Text + "." + textBstart.Text + "." + cip.ToString() + "." + q.ToString() + "\r\n");
            //        }
            //    }

            //}


            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            toolStripStatusLabelzonggong.Text = (dataGridView1.Rows.Count - 1).ToString();
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            MessageBox.Show("已完成，共生成了" + toolStripStatusLabelzonggong.Text + "个ip地址");


        }
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
        public static uint IpTint(string ipStr)
        {
            string[] ip = ipStr.Split('.');
            uint ipcode = 0xFFFFFF00 | byte.Parse(ip[3]);
            ipcode = ipcode & 0xFFFF00FF | (uint.Parse(ip[2]) << 0x8);
            ipcode = ipcode & 0xFF00FFFF | (uint.Parse(ip[1]) << 0xF);
            ipcode = ipcode & 0x00FFFFFF | (uint.Parse(ip[0]) << 0x18);
            return ipcode;
        }
        public static string IntTip(uint ipcode)
        {
            byte a = (byte)((ipcode & 0xFF000000) >> 0x18);
            byte b = (byte)((ipcode & 0x00FF0000) >> 0xF);
            byte c = (byte)((ipcode & 0x0000FF00) >> 0x8);
            byte d = (byte)(ipcode & 0x000000FF);
            string ipStr = string.Format("{0}.{1}.{2}.{3}", a, b, c, d);
            return ipStr;
        }
        //生成IP表格按钮
        private void Butip_Click(object sender, EventArgs e)
        {
            if (!IsIP(TextStartIp.Text.Trim()) || !IsIP(TextStopIp.Text.Trim()))
            {
                MessageBox.Show("您输入了非法IP地址，请修改后再次尝试！");
                return;
            }
            uint iStartip = IpTint(TextStartIp.Text.Trim());
            uint iEndIp = IpTint(TextStopIp.Text.Trim());
            if (iEndIp <= iStartip)
            {
                MessageBox.Show("错误！起始ip不能比终止ip大,请重新输入");
                return;

            }
            //dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            if (dataGridView1.Columns["地址"] == null)
            {

                this.dataGridView1.Columns.Add("地址", "地址");
            }
            //else
            //{
            //    this.dataGridView1.Columns.Remove("地址");
            //    this.dataGridView1.Columns.Add("地址", "地址");
            //}
            if (dataGridView1.Columns["开始时间"] == null)
            {

                this.dataGridView1.Columns.Add("开始时间", "开始时间");
                this.dataGridView1.Columns["开始时间"].FillWeight = 200;
            }
            else
            {
                this.dataGridView1.Columns.Remove("开始时间");
                this.dataGridView1.Columns.Add("开始时间", "开始时间");
                this.dataGridView1.Columns["开始时间"].FillWeight = 200;
            }
            if (dataGridView1.Columns["ping测试"] == null)
            {

                this.dataGridView1.Columns.Add("ping测试", "ping测试");
            }
            else
            {
                this.dataGridView1.Columns.Remove("ping测试");
                this.dataGridView1.Columns.Add("ping测试", "ping测试");
            }
            if (dataGridView1.Columns["最终结果"] == null)
            {
                this.dataGridView1.Columns.Add("最终结果", "最终结果");
            }
            else
            {
                this.dataGridView1.Columns.Remove("最终结果");
                this.dataGridView1.Columns.Add("最终结果", "最终结果");
            }

            if (dataGridView1.Columns["结束时间"] == null)
            {

                this.dataGridView1.Columns.Add("结束时间", "结束时间");
                this.dataGridView1.Columns["结束时间"].FillWeight = 200;
            }
            else
            {
                this.dataGridView1.Columns.Remove("结束时间");
                this.dataGridView1.Columns.Add("结束时间", "结束时间");
                this.dataGridView1.Columns["结束时间"].FillWeight = 200;
            }


            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            Thread t = new Thread(Newip)
            {
                IsBackground = true
            };
            t.Start();

        }

        private void butcheckver_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null)
            {
                MessageBox.Show("请先导入「网管导出的表格」或「制作IP表」，然后再次尝试");
                return;
            }
            if (dataGridView1.Columns["升级后当前版本"] == null)
            {

                this.dataGridView1.Columns.Add("升级后当前版本", "升级后当前版本");
            }
            else
            {
                this.dataGridView1.Columns.Remove("升级后当前版本");
                this.dataGridView1.Columns.Add("升级后当前版本", "升级后当前版本");
            }

            TimeNow = DateTime.Now;

            this.timer1.Enabled = true;
            this.timer1.Start();
            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";

            string task = "checkver";
            ParameterizedThreadStart p = new ParameterizedThreadStart(Xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(Bartest)
            {
                IsBackground = true
            };
            bar.Start();












        }
        public void Checkver(object obj)
        {

            int i = int.Parse(obj.ToString());
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(ip);

            for (int a = 0; a <= 1; a++)
            {
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(1000);
                pingReply = ping.Send(ip);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = "初始化";


                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");
                if (bo)
                {

                    mysocket1.SendData(GPNUSR);
                    for (int a = 0; a <= 1000; a++)
                    {
                        string login = mysocket1.ReceiveData(int.Parse(yanshi));
                        // MessageBox.Show(login);
                        if (login.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    for (int c = 0; c <= 1000; c++)
                    {
                        string passd = mysocket1.ReceiveData(int.Parse(yanshi));
                        //MessageBox.Show(passd);
                        if (passd.Contains("Error") || passd.Contains("failed") || passd.Contains("Kerberos") || passd.Contains("Bad passwords"))
                        {

                            //textDOS.AppendText("\r\n" + "用户名或密码错误，请重新输入");
                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误";
                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            lock (sb)
                            {
                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                s = s + 1;
                                toolStripStatusLabelshibai.Text = s.ToString();
                            }
                            lock (o1)
                            {
                                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                                d = d - 1;
                                toolStripStatusLabelshengyu.Text = d.ToString();

                                doneCount++;

                            }
                            return;
                        }
                        if (passd.Contains("Password:"))
                        {
                            mysocket1.SendData(GPNPSD);
                        }
                        if (passd.Contains(">"))
                        {
                            //textDOS.AppendText("\r\n" + "用户名密码正确==========================================OK");
                            mysocket1.SendData("enable");
                            for (int b = 0; b <= 1000; b++)
                            {
                                string pass = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (pass.Contains("Pas"))
                                {
                                    mysocket1.SendData(GPNPSDEN);
                                    //Thread.Sleep(500);
                                    for (int d = 0; d <= 1000; d++)
                                    {
                                        string locked = mysocket1.ReceiveData(int.Parse(yanshi));
                                        if (locked.Contains("configuration is locked by other user"))
                                        //configuration is locked by other user
                                        {
                                            //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录========================OK");
                                            mysocket1.SendData("grosadvdebug");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("vty user limit no");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("exit");
                                            Thread.Sleep(200);
                                            mysocket1.SendData("enable");
                                            Thread.Sleep(200);
                                            if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                            {
                                                mysocket1.SendData(GPNPSDEN);
                                                Thread.Sleep(200);
                                                if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                                {
                                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                                    lock (sb)
                                                    {
                                                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                        s = s + 1;
                                                        toolStripStatusLabelshibai.Text = s.ToString();
                                                    }
                                                    lock (o1)
                                                    {
                                                        int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                        e = e - 1;
                                                        toolStripStatusLabelshengyu.Text = e.ToString();

                                                        doneCount++;

                                                    }
                                                    return;
                                                }

                                                break;
                                            }
                                        }
                                        if (locked.Contains("#"))
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
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
                                    //textDOS.AppendText("\r\n" + "已经有用户登录，正在重新登录=============================OK");
                                    mysocket1.SendData("grosadvdebug");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("vty user limit no");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("exit");
                                    Thread.Sleep(200);
                                    mysocket1.SendData("enable");
                                    Thread.Sleep(200);
                                    if (mysocket1.ReceiveData(int.Parse(yanshi)).Contains("Pas"))
                                    {
                                        mysocket1.SendData(GPNPSDEN);
                                        Thread.Sleep(200);
                                        if (!mysocket1.ReceiveData(int.Parse(yanshi)).Contains("failed"))
                                        {
                                            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "用户名密码错误2";
                                            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                                            lock (sb)
                                            {
                                                int s = int.Parse(toolStripStatusLabelshibai.Text);
                                                s = s + 1;
                                                toolStripStatusLabelshibai.Text = s.ToString();
                                            }
                                            lock (o1)
                                            {
                                                int e = int.Parse(toolStripStatusLabelshengyu.Text);
                                                e = e - 1;
                                                toolStripStatusLabelshengyu.Text = e.ToString();

                                                doneCount++;

                                            }
                                            return;
                                        }

                                        break;
                                    }
                                    break;
                                }
                                Thread.Sleep(1);
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }

                    mysocket1.SendData("show ver");
                    string ver = "";
                    string ver2 = "";
                    for (int a = 0; a <= 1000; a++)
                    {
                        ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                        ver = ver + ver2;
                        if (ver2.Contains("Ctrl+c"))
                        {
                            mysocket1.SendDate("\r\n");
                            //MessageBox.Show(ver);
                        }
                        if (ver2.Contains("(config)#"))
                        {
                            ver2 = mysocket1.ReceiveData(int.Parse(yanshi));
                            ver = ver + ver2;
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    Regex r = new Regex(@"ProductOS\s*Version\s*([\w\d]+)[\s*\(]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string banben = r.Match(ver).Groups[1].Value;
                    if (banben.ToString() == "")
                    {
                        this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = "获取失败";
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        lock (sb)
                        {
                            int s = int.Parse(toolStripStatusLabelshibai.Text);
                            s = s + 1;
                            toolStripStatusLabelshibai.Text = s.ToString();
                        }


                    }
                    else
                    {
                        this.dataGridView1.Rows[i].Cells["升级后当前版本"].Value = banben.ToString();
                        this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                        this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                        this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                        lock (cg)
                        {
                            //MessageBox.Show("fff");
                            int cg = int.Parse(toolStripStatusLabelchenggong.Text);
                            cg = cg + 1;
                            toolStripStatusLabelchenggong.Text = cg.ToString();
                        }

                    }
                   

                    //mysocket1.SendData("save");
                    //    for (int a = 1; a <= 5000; a++)
                    //    {
                    //        string box = mysocket1.ReceiveData(int.Parse("10")); ;
                    //        if (box.Contains("successfully"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                    //            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    //            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                    //            lock (cg)
                    //            {
                    //                int c = int.Parse(toolStripStatusLabelchenggong.Text);
                    //                c = c + 1;
                    //                toolStripStatusLabelchenggong.Text = c.ToString();
                    //                break;
                    //            }
                    //        }
                    //        if (box.Contains("erro"))
                    //        {
                    //            this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                    //            this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    //            this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    //            lock (sb)
                    //            {
                    //                int s = int.Parse(toolStripStatusLabelshibai.Text);
                    //                s = s + 1;
                    //                toolStripStatusLabelshibai.Text = s.ToString();
                    //                break;
                    //            }
                    //        }

                    //        Thread.Sleep(1);
                    //    }
                    //    mysocket1.SendData("logout");

                }

                else
                {
                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    lock (sb)
                    {
                        int s = int.Parse(toolStripStatusLabelshibai.Text);
                        s = s + 1;
                        toolStripStatusLabelshibai.Text = s.ToString();
                    }
                }


            }
            else
            {

                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;
                    toolStripStatusLabelshibai.Text = s.ToString();
                }
            }
            if (dataGridView1.Rows[i].Cells["最终结果"].Value.ToString() == "初始化")
            {
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "telnet失败";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                lock (sb)
                {
                    int s = int.Parse(toolStripStatusLabelshibai.Text);
                    s = s + 1;

                }
            }

            lock (o1)
            {
                int d = int.Parse(toolStripStatusLabelshengyu.Text);
                d = d - 1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;

            }
            //MessageBox.Show("一键保存结束");

        }


        private void butUtility_Click(object sender, EventArgs e)
        {
            MessageBox.Show("导出「网管资源列表的Excel表格」尝试导入！");
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            OpenFileDialog ofd = new OpenFileDialog();
            string strPath;//完整的路径名
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    strPath = ofd.FileName;
                    DataTable dataTable = null;
                    dataTable = ExcelUtility.ExcelToDataTable(strPath, true);
                    //DataView dv = ds.Tables[0].DefaultView;
                    dataTable.DefaultView.RowFilter = "类型 = '" + comtype.Text + "'";
                    dataGridView1.DataSource = dataTable;
                    if (dataGridView1.Columns["开始时间"] == null)
                    {

                        this.dataGridView1.Columns.Add("开始时间", "开始时间");
                        this.dataGridView1.Columns["开始时间"].FillWeight = 150;
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("开始时间");
                        this.dataGridView1.Columns.Add("开始时间", "开始时间");
                        this.dataGridView1.Columns["开始时间"].FillWeight = 150;
                    }
                    if (dataGridView1.Columns["ping测试"] == null)
                    {

                        this.dataGridView1.Columns.Add("ping测试", "ping测试");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("ping测试");
                        this.dataGridView1.Columns.Add("ping测试", "ping测试");
                    }
                    if (dataGridView1.Columns["最终结果"] == null)
                    {
                        this.dataGridView1.Columns.Add("最终结果", "最终结果");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("最终结果");
                        this.dataGridView1.Columns.Add("最终结果", "最终结果");
                    }

                    if (dataGridView1.Columns["结束时间"] == null)
                    {

                        this.dataGridView1.Columns.Add("结束时间", "结束时间");
                        this.dataGridView1.Columns["结束时间"].FillWeight = 150;
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("结束时间");
                        this.dataGridView1.Columns.Add("结束时间", "结束时间");
                        this.dataGridView1.Columns["结束时间"].FillWeight = 150;
                    }





                    this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    toolStripStatusLabelzonggong.Text = (dataGridView1.Rows.Count - 1).ToString();
                    toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);//捕捉异常
                    MessageBox.Show("请使用Office2003或者更新版本格式内容，如.xls或者.xlsx格式");
                }
            }
        }

        private void butoid_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            if (dataGridView1.Columns["序号"] == null)
            {

                this.dataGridView1.Columns.Add("序号", "序号");
                this.dataGridView1.Columns["序号"].Width = 50;
            }
            else
            {
                this.dataGridView1.Columns.Remove("序号");
                this.dataGridView1.Columns.Add("序号", "序号");
                this.dataGridView1.Columns["序号"].Width = 50;
            }
            if (dataGridView1.Columns["Mib类"] == null)
            {

                this.dataGridView1.Columns.Add("Mib类", "Mib类");
                this.dataGridView1.Columns["Mib类"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("Mib类");
                this.dataGridView1.Columns.Add("Mib类", "Mib类");
                this.dataGridView1.Columns["Mib类"].Width = 100;
            }
            if (dataGridView1.Columns["Mib表"] == null)
            {

                this.dataGridView1.Columns.Add("Mib表", "Mib表");
                this.dataGridView1.Columns["Mib表"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("Mib表");
                this.dataGridView1.Columns.Add("Mib表", "Mib表");
                this.dataGridView1.Columns["Mib表"].Width = 100;
            }
            if (dataGridView1.Columns["节点名称"] == null)
            {

                this.dataGridView1.Columns.Add("节点名称", "节点名称");
                this.dataGridView1.Columns["节点名称"].Width = 150;
            }
            else
            {
                this.dataGridView1.Columns.Remove("节点名称");
                this.dataGridView1.Columns.Add("节点名称", "节点名称");
                this.dataGridView1.Columns["节点名称"].Width = 150;
            }
            if (dataGridView1.Columns["Mib节点"] == null)
            {

                this.dataGridView1.Columns.Add("Mib节点", "Mib节点");
                this.dataGridView1.Columns["Mib节点"].Width = 200;
            }
            else
            {
                this.dataGridView1.Columns.Remove("Mib节点");
                this.dataGridView1.Columns.Add("Mib节点", "Mib节点");
                this.dataGridView1.Columns["Mib节点"].Width = 200;
            }
            if (dataGridView1.Columns["节点类型"] == null)
            {

                this.dataGridView1.Columns.Add("节点类型", "节点类型");
                this.dataGridView1.Columns["节点类型"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("节点类型");
                this.dataGridView1.Columns.Add("节点类型", "节点类型");
                this.dataGridView1.Columns["节点类型"].Width = 100;
            }
            if (dataGridView1.Columns["访问权限"] == null)
            {

                this.dataGridView1.Columns.Add("访问权限", "访问权限");
                this.dataGridView1.Columns["访问权限"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("访问权限");
                this.dataGridView1.Columns.Add("访问权限", "访问权限");
                this.dataGridView1.Columns["访问权限"].Width = 100;
            }
            if (dataGridView1.Columns["取值"] == null)
            {

                this.dataGridView1.Columns.Add("取值", "取值");
                this.dataGridView1.Columns["取值"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("取值");
                this.dataGridView1.Columns.Add("取值", "取值");
                this.dataGridView1.Columns["取值"].Width = 100;
            }
            if (dataGridView1.Columns["说明"] == null)
            {

                this.dataGridView1.Columns.Add("说明", "说明");
                this.dataGridView1.Columns["说明"].Width = 100;
            }
            else
            {
                this.dataGridView1.Columns.Remove("说明");
                this.dataGridView1.Columns.Add("说明", "说明");
                this.dataGridView1.Columns["说明"].Width = 100;
            }
            String connetStr = "server=60.205.155.127;port=3306;user=root;password=Hunan7420716.; database=mib;charset=utf8;";
            MySqlConnection conn = new MySqlConnection(connetStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                Console.WriteLine("已经建立连接");
                //在这里使用代码对数据库进行增删查改
                string sql = "select* from mib where concat(table_class, table_name,name,oid,value,note) like '%" + textselect.Text + "%'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();//执行ExecuteReader()返回一个MySqlDataReader对象
                 //设置自动换行

                this.dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                //设置自动调整高度

                //this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                while (reader.Read())
                {
                    int index = this.dataGridView1.Rows.Add();

                    this.dataGridView1.Rows[index].Cells[0].Value = reader.GetString("index");
                    this.dataGridView1.Rows[index].Cells[1].Value = reader.GetString("table_class");
                    this.dataGridView1.Rows[index].Cells[2].Value = reader.GetString("table_name");
                    this.dataGridView1.Rows[index].Cells[3].Value = reader.GetString("name");
                    this.dataGridView1.Rows[index].Cells[4].Value = reader.GetString("oid");
                    this.dataGridView1.Rows[index].Cells[5].Value = reader.GetString("type");
                    this.dataGridView1.Rows[index].Cells[6].Value = reader.GetString("permission");
                    this.dataGridView1.Rows[index].Cells[7].Value = reader.GetString("value");
                    this.dataGridView1.Rows[index].Cells[8].Value = reader.GetString("note");

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
    }

}
