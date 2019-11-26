using System;
using System.Collections.Generic;
using System.Text;

namespace MyGpnSoftware
{
    class MySocket
    {
        private System.Net.Sockets.Socket socket;
        private bool closed;
        public MySocket()

        {

            socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            closed = true;
        }
        public bool Connect(string address, string port)
        {
            try
            {

                System.Net.IPAddress ipaddr = System.Net.IPAddress.Parse(address);
                System.Net.IPEndPoint ipep = new System.Net.IPEndPoint(ipaddr, int.Parse(port));
                socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                socket.Connect(ipep);
                int datalong = socket.Available;

                System.Threading.Thread.Sleep(100);
                while (datalong < socket.Available)
                {
                    datalong = socket.Available;
                    System.Threading.Thread.Sleep(100);
                }
                if (datalong > 0)
                {
                    byte []recvdata = new byte[datalong];
                    byte[] senddat = new byte[datalong];
                    int p=0;
                    socket.Receive(recvdata, 0, datalong, System.Net.Sockets.SocketFlags.None);
                    for (int i = 0; i < datalong; i++)
                    {
                        if(recvdata[i]==255)
                        {

                            if (recvdata[i + 1] == 250)
                            {
                                senddat[p] = 255;
                                senddat[p+ 1] = 240;
                                senddat[p + 2] = recvdata[i + 2];
                                i = i + 2;
                                p = p + 3;
                            }
                            if (recvdata[i + 1] == 251)
                            {
                                senddat[p] = 255;
                                senddat[p + 1] = 253;
                                senddat[p + 2] = recvdata[i + 2];
                                i = i + 2;
                                p = p + 3;
                            }
                            if (recvdata[i + 1] == 253)
                            {
                                senddat[p] = 255;
                                senddat[p + 1] = 251;
                                senddat[p + 2] = recvdata[i + 2];
                                i = i + 2;
                                p = p + 3;
                            }
                            if (recvdata[i + 1] == 252||recvdata[i + 1] == 254)
                            {
                                senddat[p] = 255;
                                senddat[p + 1] = 254;
                                senddat[p + 2] = recvdata[i + 2];
                                i = i + 2;
                                p = p + 3;
                            }

                        }
                    }
                    socket.Send(senddat, 0, p, System.Net.Sockets.SocketFlags.None);
                }

            }
            catch 
            {
                
                return false;
            }
            closed = false;
            return true;
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="ts">不同数据块到达的最大间隔（毫秒）</param>
        /// <returns></returns>
        public string ReceiveData(int ts)
        {
            string str = "";
            int datalong;
            byte[] recvdata;
            try
            {
                datalong = socket.Available;
                System.Threading.Thread.Sleep(ts);
                while (datalong < socket.Available)
                {
                    datalong = socket.Available;
                    System.Threading.Thread.Sleep(ts);
                }
                recvdata = new byte[datalong];
                if (recvdata.Length > 0)
                {
                    socket.Receive(recvdata, 0, datalong, System.Net.Sockets.SocketFlags.None);
                    str = Encoding.ASCII.GetString(recvdata).Trim();
                }
            }
            catch (Exception eee)
            {
                str = eee.ToString();
            }
            return str;

        }
        /// <summary>
        /// 发送带命令结束符\r\n的数据
        /// </summary>
        /// <param name="dataStr"></param>
        /// <returns></returns>
        public bool SendData(string dataStr)
        {
            bool r = true;
            if (dataStr == null || dataStr.Length < 0)
                return false;
            byte[] cmd = Encoding.ASCII.GetBytes(dataStr + "\r\n");
            try
            {
                int n = socket.Send(cmd, 0, cmd.Length, System.Net.Sockets.SocketFlags.None);
                if (n < 1)
                    r = false;
            }
            catch 
            {

                r = false;
            }
            return r;
        }
        public bool SendDate(string dataStr)
        {
            bool r = true;
            if (dataStr == null || dataStr.Length < 0)
                return false;
            byte[] cmd = Encoding.ASCII.GetBytes(dataStr);
            try
            {
                int n = socket.Send(cmd, 0, cmd.Length, System.Net.Sockets.SocketFlags.None);
                if (n < 1)
                    r = false;
            }
            catch
            {

                r = false;
            }
            return r;
        }
        public bool SendData(byte[] dataByte)
        {
            bool r = true;
            if (dataByte == null || dataByte.Length < 0)
                return false;
            try
            {
                int n = socket.Send(dataByte, 0, dataByte.Length, System.Net.Sockets.SocketFlags.None);
                if (n < 1)
                    r = false;
            }
            catch 
            {
                r = false;
            }
            return r;
        }
        public void Close()
        {
            try
            {
                socket.Close();
            }
            catch { }
            closed = true;
        }
        public bool IsClosed()
        {
            return closed;
        }

    }

}
