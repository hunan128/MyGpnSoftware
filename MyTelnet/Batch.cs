using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    public partial class Batch : Form
    {
        int doneCount = 0;
        public Batch()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
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

        private void butshow_Click(object sender, EventArgs e)
        {
            MessageBox.Show("导出「网管资源列表的Excel表格」尝试导入！");
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            OpenFileDialog ofd = new OpenFileDialog();//首先根据打开对话框，选择excel表格
            ofd.Filter = "Excel office2003(*.xls)|*.xls|Excel office2010(*.xlsx)|*.xlsx";//打开对话框筛选器
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
                    dv.RowFilter = "类型 = '"+comtype.Text+"'";
                    dataGridView1.DataSource = dv;
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
                    else {
                        this.dataGridView1.Columns.Remove("ping测试");
                        this.dataGridView1.Columns.Add("ping测试", "ping测试");
                    }
                    if (dataGridView1.Columns["最终结果"] == null)
                    {
                        this.dataGridView1.Columns.Add("最终结果", "最终结果");
                    }
                    else {
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
                    MessageBox.Show("请使用Office 2003版本格式内容，如.xls格式");
                }
            }
        }

        private void butout_Click(object sender, EventArgs e)
        {
            NPOIExcel ET = new NPOIExcel();
            ET.ExportExcel("sheet1",dataGridView1);
        }

        public void save(object obj)
        {

            int i  = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            Ping ping = new Ping();
            int timeout = 500;
            MySocket mysocket1 = new MySocket();
            string ip = "";
            ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();

            // MessageBox.Show(ip);
            PingReply pingReply = ping.Send(ip, timeout);
            for (int j = 0; j <= 5; j++)
            {


                pingReply = ping.Send(ip, timeout);
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(10);
            }//判断请求是否超时
            if  (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");
                if (bo)
                {

                    if (mysocket1.SendData(GPNUSR))
                    {
                        mysocket1.SendData(GPNPSD);
                        mysocket1.SendData("enable");
                        Thread.Sleep(200);
                        string pass = mysocket1.ReceiveData(int.Parse("10"));
                        if (pass.Contains("Pas"))
                        {
                            mysocket1.SendData(GPNPSD);
                            Thread.Sleep(500);
                            string locked = mysocket1.ReceiveData(int.Parse("10"));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket1.SendData("grosadvdebug");
                                Thread.Sleep(200);
                                mysocket1.SendData("vty user limit no");
                                Thread.Sleep(200);
                                mysocket1.SendData("exit");
                                Thread.Sleep(200);
                                mysocket1.SendData("enable");
                                Thread.Sleep(200);
                                mysocket1.SendData(GPNPSD);
                                Thread.Sleep(500);

                            }

                        }
                        mysocket1.SendData("save");
                            for (int a = 1; a <= 5000; a++)
                            {
                                string box = mysocket1.ReceiveData(int.Parse("10")); ;
                                if (box.Contains("successfully"))
                                {
                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "成功";
                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
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
                                    this.dataGridView1.Rows[i].Cells["最终结果"].Value = "失败";
                                    this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                                    this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
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
                            mysocket1.SendData("logout");
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
                d = d-1;
                toolStripStatusLabelshengyu.Text = d.ToString();

                doneCount++;
               
            }
            //MessageBox.Show("一键保存结束");

        }
        public void upload(object obj)
        {
            int i = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            Ping ping = new Ping();
            int timeout = 500;
            MySocket mysocket1 = new MySocket();
            string ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();

            // MessageBox.Show(ip);
            PingReply pingReply = ping.Send(ip, timeout);
            for (int j = 0; j <= 5; j++)
            {
                pingReply = ping.Send(ip, timeout);
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(10);
            }//判断请求是否超时
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");

                if (bo)
                {

                    if (mysocket1.SendData(GPNUSR))
                    {
                        mysocket1.SendData(GPNPSD);
                        mysocket1.SendData("enable");
                        Thread.Sleep(200);
                        string pass = mysocket1.ReceiveData(int.Parse("10"));
                        if (pass.Contains("Pas"))
                        {
                            mysocket1.SendData(GPNPSD);
                            Thread.Sleep(500);
                            string locked = mysocket1.ReceiveData(int.Parse("10"));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket1.SendData("grosadvdebug");
                                Thread.Sleep(200);
                                mysocket1.SendData("vty user limit no");
                                Thread.Sleep(200);
                                mysocket1.SendData("exit");
                                Thread.Sleep(200);
                                mysocket1.SendData("enable");
                                Thread.Sleep(200);
                                mysocket1.SendData(GPNPSD);
                                Thread.Sleep(500);

                            }
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
                            else {
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
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null) {
               

                MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
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

            string task = "save";
            ParameterizedThreadStart p = new ParameterizedThreadStart(xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread  bar = new Thread(bartest);
            bar.IsBackground = true;
            bar.Start();
        }


        private void xianchengchi(object obj)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(int.Parse(compl.Text), int.Parse(compl.Text));
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                if (obj.ToString() == "link")
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(link), i.ToString());

                }
                if (obj.ToString() == "save")
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(save), i.ToString());

                }
                if (obj.ToString() == "upload")
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(upload), i.ToString());

                }
                if (obj.ToString() == "download")
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(download), i.ToString());

                }
                if (obj.ToString() == "reboot" && (bool)dataGridView1.Rows[i].Cells["重启选择"].EditedFormattedValue == true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(reboot), i.ToString());
                }

            }




        }
        private void bartest()
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
                    MessageBox.Show("批量操作结束");
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

        }
        #endregion

        private void butupload_Click(object sender, EventArgs e)
        {

            if (dataGridView1.DataSource == null)
            {


                MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
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
            ParameterizedThreadStart p = new ParameterizedThreadStart(xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(bartest);
            bar.IsBackground = true;
            bar.Start();

        }
        System.DateTime TimeNow = new DateTime();
        TimeSpan time = new TimeSpan();
        private void timer1_Tick(object sender, EventArgs e)
        {
            time = DateTime.Now - TimeNow;
            toolStripStatusLabeltime.Text = string.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        }

        private void butdown_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null)
            {


                MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
                return;

            }
            if (app.Contains(".bin"))
            {
                DialogResult dr = MessageBox.Show("即将下载APP版本：" + app +" 是否确认升级？" , "提示", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    TimeNow = DateTime.Now;
                    //TimeCount = 0;
                    //Mytimer.Change(0, 1000);
                    this.timer1.Enabled = true;
                    this.timer1.Start();
                    if (dataGridView1.Columns["当前版本"] == null)
                    {

                        this.dataGridView1.Columns.Add("当前版本", "当前版本");

                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("当前版本");
                        this.dataGridView1.Columns.Add("当前版本", "当前版本");
                    }
                    if (dataGridView1.Columns["save"] == null)
                    {

                        this.dataGridView1.Columns.Add("save", "save");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("save");
                        this.dataGridView1.Columns.Add("save", "save");
                    }
                    if (dataGridView1.Columns["备份config"] == null)
                    {

                        this.dataGridView1.Columns.Add("备份config", "备份config");
                    }
                    else
                    {
                        this.dataGridView1.Columns.Remove("备份config");
                        this.dataGridView1.Columns.Add("备份config", "备份config");
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
                    if (dataGridView1.Columns["重启选择"] == null)
                    {
                        DataGridViewCheckBoxColumn newColumn = new DataGridViewCheckBoxColumn();
                        newColumn.Name = "重启选择";
                        newColumn.HeaderText = "重启选择";
                        dataGridView1.Columns.Add(newColumn);
                    }
                   

                    doneCount = 0;
                    toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
                    toolStripStatusLabelchenggong.Text = "0";
                    toolStripStatusLabelshibai.Text = "0";
                    toolStripStatusLabelyichang.Text = "0";

                    string task = "download";
                    ParameterizedThreadStart p = new ParameterizedThreadStart(xianchengchi);
                    Thread t = new Thread(p);
                    t.Start(task);
                    Thread bar = new Thread(bartest);
                    bar.IsBackground = true;
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
        public void download(object obj)
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
            for (int j = 0; j <= 5; j++)
            {


                pingReply = ping.Send(ip, timeout);
                if(pingReply.Status == IPStatus.Success)
                    {
                    break;      
                    }
                Thread.Sleep(10);
            }//判断请求是否超时
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;

                bool bo = mysocket1.Connect(ip, "23");

                if (bo)
                {

                    if (mysocket1.SendData(GPNUSR))
                    {
                        mysocket1.SendData(GPNPSD);
                        mysocket1.SendData("enable");
                        Thread.Sleep(200);
                        string pass = mysocket1.ReceiveData(int.Parse("10"));
                        if (pass.Contains("Pas"))
                        {
                            mysocket1.SendData(GPNPSD);
                            Thread.Sleep(500);
                            string locked = mysocket1.ReceiveData(int.Parse("10"));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket1.SendData("grosadvdebug");
                                Thread.Sleep(200);
                                mysocket1.SendData("vty user limit no");
                                Thread.Sleep(200);
                                mysocket1.SendData("exit");
                                Thread.Sleep(200);
                                mysocket1.SendData("enable");
                                Thread.Sleep(200);
                                mysocket1.SendData(GPNPSD);
                                Thread.Sleep(500);

                            }
                        }
                            this.dataGridView1.Rows[i].Cells["当前版本"].Value = "初始化";
                            this.dataGridView1.Rows[i].Cells["save"].Value = "初始化";
                            this.dataGridView1.Rows[i].Cells["备份config"].Value = "初始化";
                            this.dataGridView1.Rows[i].Cells["下载APP"].Value = "初始化";
                            this.dataGridView1.Rows[i].Cells["写入APP"].Value = "初始化";

                        mysocket1.SendData("show version");
                        string ver = "";
                        for (int b = 0; b <= 100; b++)
                        {

                            ver = ver + mysocket1.ReceiveData(int.Parse(yanshi));

                            Thread.Sleep(1);
                        }
                        Regex r = new Regex("ProductOS Version (V\\d+)*R\\d+(C\\d+)*B\\d+(SP\\d+)*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string banben = r.Match(ver).Groups[0].Value;
                        banben = banben.Substring("ProductOS Version ".Length);
                        this.dataGridView1.Rows[i].Cells["当前版本"].Value = banben.ToString();


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
                                    this.dataGridView1.Rows[i].Cells["备份config"].Value = "OK";
                                    break;
                                }
                                if (box.Contains("fail"))
                                {
                                    this.dataGridView1.Rows[i].Cells["备份config"].Value = "检查FTP服务器IP地址";
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
                                    this.dataGridView1.Rows[i].Cells["备份config"].Value = "检查FTP用户名密码";
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
                            mysocket1.SendData("download ftp app " + FTPIP + " " + FTPUSR + " " + FTPPSD + " " + app +" gpn");
                            for (int a = 1; a <= 20000; a++)
                            {

                                string command = mysocket1.ReceiveData(int.Parse(yanshi));
                                if (command.Contains("Download file ...ok"))
                                {
                                    this.dataGridView1.Rows[i].Cells["下载APP"].Value = "OK";
                                    
                                    for (int b = 1; b <= 20000; b++)
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
                                        Thread.Sleep(10);
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
                                Thread.Sleep(10);
                            }

                            string save = dataGridView1.Rows[i].Cells["save"].Value.ToString();
                            string config = dataGridView1.Rows[i].Cells["备份config"].Value.ToString();
                            string downloadapp = dataGridView1.Rows[i].Cells["下载APP"].Value.ToString();
                            string writeloadapp = dataGridView1.Rows[i].Cells["写入APP"].Value.ToString();
                            
                            if (save == "OK" && config == "OK" && downloadapp == "OK" && writeloadapp == "OK")
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


        private void butreboot_Click(object sender, EventArgs e)
        {

                    if (dataGridView1.DataSource == null)
                    {


                        MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
                        return;

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
                    ParameterizedThreadStart p = new ParameterizedThreadStart(xianchengchi);
                    Thread t = new Thread(p);
                    t.Start(task);
                    Thread bar = new Thread(rebootbar);
                    bar.IsBackground = true;
                    bar.Start();

        }

        private void rebootbar()
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
                    MessageBox.Show("重启完成");
                    butreboot.Enabled = false;
                    this.timer1.Enabled = false;
                    this.timer1.Stop();
                    break;
                }
                Thread.Sleep(1000);
            }

        }
        public void reboot(object obj)
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
            for (int j = 0; j <= 5; j++)
            {
                
                
                pingReply = ping.Send(ip, timeout);
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(10);
            }
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["重启结果"].Value = "初始化";

                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.White;


                if (mysocket1.Connect(ip, "23"))
                {

                    if (mysocket1.SendData(GPNUSR))
                    {
                        mysocket1.SendData(GPNPSD);
                        mysocket1.SendData("enable");
                        Thread.Sleep(200);
                        string pass = mysocket1.ReceiveData(int.Parse("10"));
                        if (pass.Contains("Pas"))
                        {
                            mysocket1.SendData(GPNPSD);
                            Thread.Sleep(500);
                            string locked = mysocket1.ReceiveData(int.Parse("10"));
                            if (locked.Contains("configuration is locked by other user"))
                            {
                                mysocket1.SendData("grosadvdebug");
                                Thread.Sleep(200);
                                mysocket1.SendData("vty user limit no");
                                Thread.Sleep(200);
                                mysocket1.SendData("exit");
                                Thread.Sleep(200);
                                mysocket1.SendData("enable");
                                Thread.Sleep(200);
                                mysocket1.SendData(GPNPSD);
                                Thread.Sleep(500);

                            }
                        }
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


                }
                else
                {

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

        private void batch_Paint(object sender, PaintEventArgs e)
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

        private void batch_Resize(object sender, EventArgs e)
        {
            this.Invalidate();//重绘窗体
        }

        private void comtype_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void butlink_Click(object sender, EventArgs e)
        {
            //if (dataGridView1.DataSource == null)
            //{


            //    MessageBox.Show("请先导入「网管导出的表格」，然后再次尝试");
            //    return;

            //}
            TimeNow = DateTime.Now;
            this.timer1.Enabled = true;
            this.timer1.Start();
            doneCount = 0;
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            toolStripStatusLabelchenggong.Text = "0";
            toolStripStatusLabelshibai.Text = "0";
            toolStripStatusLabelyichang.Text = "0";
            string task = "link";
            ParameterizedThreadStart p = new ParameterizedThreadStart(xianchengchi);
            Thread t = new Thread(p);
            t.Start(task);
            Thread bar = new Thread(bartest);
            bar.IsBackground = true;
            bar.Start();
        }
        public void link(object obj)
        {

            int i = int.Parse(obj.ToString());
            this.dataGridView1.Rows[i].Cells["开始时间"].Value = DateTime.Now.ToString();
            Ping ping = new Ping();
            int timeout = 500;
            MySocket mysocket1 = new MySocket();
            string ip = "";
            ip = dataGridView1.Rows[i].Cells["地址"].Value.ToString();

            // MessageBox.Show(ip);
            PingReply pingReply = ping.Send(ip, timeout);
            for (int j = 0; j <= 1; j++)
            {


                pingReply = ping.Send(ip, timeout);
                if (pingReply.Status == IPStatus.Success)
                {
                    break;
                }
                Thread.Sleep(10);
            }//判断请求是否超时
            if (pingReply.Status == IPStatus.Success)
            {
                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "OK";
                this.dataGridView1.Rows[i].Cells["最终结果"].Value = "在线";
                this.dataGridView1.Rows[i].Cells["结束时间"].Value = DateTime.Now.ToString();
                this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                lock (cg)
                {
                    int c = int.Parse(toolStripStatusLabelchenggong.Text);
                    c = c + 1;
                    toolStripStatusLabelchenggong.Text = c.ToString();

                }


            }
            else
            {

                this.dataGridView1.Rows[i].Cells["ping测试"].Value = "NOK";
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
        public void newip()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            int a = int.Parse(textAstop.Text) - int.Parse(textAstart.Text);
            int b = int.Parse(textBstop.Text) - int.Parse(textBstart.Text);
            int c = int.Parse(textCstop.Text) - int.Parse(textCstart.Text);
            int d = int.Parse(textDstop.Text) - int.Parse(textDstart.Text);
            int astart = int.Parse(textAstart.Text);
            int bstart = int.Parse(textBstart.Text);
            int cstart = int.Parse(textCstart.Text);
            int dstart = int.Parse(textDstart.Text);


            for (int i = 0; i <= c; i++)
            {
                if (c == 0)
                {
                    for (int j = 1; j <= d; j++)
                    {
                        int index = this.dataGridView1.Rows.Add();
                        this.dataGridView1.Rows[index].Cells["地址"].Value = textAstart.Text + "." + textBstart.Text + "." + textCstart.Text + "." + j.ToString();
                       // dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
                        //textBox1.AppendText(textAstart.Text + "." + textBstart.Text + "." + textCstart.Text + "." + j.ToString() + "\r\n");

                    }

                }
                if( c!=0 )
                {
                    for (int q = 1; q <= 254; q++)
                    {
                        int cip = cstart + i;
                        int index = this.dataGridView1.Rows.Add();
                        this.dataGridView1.Rows[index].Cells["地址"].Value = textAstart.Text + "." + textBstart.Text + "." + cip.ToString() + "." + q.ToString();
                       // dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
                        //textBox1.AppendText(textAstart.Text + "." + textBstart.Text + "." + cip.ToString() + "." + q.ToString() + "\r\n");
                    }
                }

            }


            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            toolStripStatusLabelzonggong.Text = (dataGridView1.Rows.Count - 1).ToString();
            toolStripStatusLabelshengyu.Text = toolStripStatusLabelzonggong.Text;
            MessageBox.Show("已完成，共生成了"+ toolStripStatusLabelzonggong.Text+"个ip地址");
            

        }
        private void butping_Click(object sender, EventArgs e)
        {


            // MessageBox.Show(ipping + "\r\n");
            //IPAddress ipAddress1 = new IPAddress(new byte[] { 151, 33, 86, 50 });
        }

        private void butip_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            if (dataGridView1.Columns["地址"] == null)
            {

                this.dataGridView1.Columns.Add("地址", "地址");
            }
            else
            {
                this.dataGridView1.Columns.Remove("地址");
                this.dataGridView1.Columns.Add("地址", "地址");
            }
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
            Thread t = new Thread(newip);
            t.IsBackground = true;
            t.Start();

        }
    }

}
