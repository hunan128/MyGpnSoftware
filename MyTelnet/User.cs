using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MyGpnSoftware
{
    class User
    {
        public UserSeesion CommandSession { get; set; }
        public UserSeesion DataSession { get; set; }
        public TcpListener DataListener { get; set; }

        // 主动模式下使用的客户端监听的IPEndPoint
        public IPEndPoint RemoteEndPoint { get; set; }

        // 用户名
        public string UserName { get; set; }

        // 工作目录
        public string WorkDir { get; set; }

        // 当前工作目录
        public string CurrentDir { get; set; }

        // 初始状态为等待输入用户名
        public int LoginOK { get; set; }

        // 是否使用二进制数据传输方式
        public bool IsBinary { get; set; }

        // 数据连接使用的是否被动连接
        public bool IsPassive { get; set; }
       

    }
}
