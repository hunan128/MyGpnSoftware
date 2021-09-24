using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    class Snmp
    {
        public static string Get(string ip, string community, int timeout, int retry, string oid)
        {
            string ReV = "0";
            //定义代理参数类 
            AgentParameters param = new AgentParameters(SnmpVersion.Ver2,new OctetString(community),true);
            //将SNMP版本设置为1（或2） 
            //构造代理地址对象
            //这里很容易使用IpAddress类，因为
            //如果不
            //解析为IP地址，它将尝试解析构造函数参数
            IpAddress agent = new IpAddress(ip);
            //构建目标 
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, timeout, retry);
            UdpTransport h = new UdpTransport(true);
            //  用于所有请求PDU级 
            Pdu pdu = new Pdu(PduType.Get);
            try
            {
                pdu.VbList.Add(oid);
            }
            catch {
                return ReV;
            }
 
            SnmpPacket result = null;
            try
            {
                result = target.Request(pdu, param);
            }
            catch (SnmpException ex)
            {
                ReV=(ex.Message);
                return ReV;
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
                    ReV=(String.Format("SNMP回复错误！错误代码：{0}，错误索引：第 {1} 行 \r\n",
                            FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                            result.Pdu.ErrorIndex));
                    return ReV;
                }
                else
                {
                    //返回值
                   // ReV = result.Pdu.VbList[0].Value.ToString();
                    ReV = String.Join("", Regex.Split(result.Pdu.VbList[0].Value.ToString(), "\\s+", RegexOptions.IgnoreCase)).ToLower();

                }
            }
            return ReV;         
        }

        public static List<string[]> Arry(string ip, string community, int timeout, int retry, string oid)
        {

            List<string[]> arry = new List<string[]>();

            AgentParameters param = new AgentParameters(SnmpVersion.Ver2, new OctetString(community), true);
            // 将 SNMP 版本设置为 2（GET-BULK 仅适用于 SNMP 版本 2 和 3）
            // 构造代理地址对象
            // IpAddress 类在这里很容易使用，因为
            // 如果没有，它将尝试解析构造函数参数
            //解析为IP地址 
            IpAddress agent = new IpAddress(ip);
            //构造目标
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, timeout, retry);
            // 定义作为 MIB 根的 Oid
            // 要检索的树
            Oid rootOid = new Oid(oid);
            // 如果描述
            // 这个 Oid 代表最后一次返回的 Oid
            // SNMP代理
            Oid lastOid = (Oid)rootOid.Clone();
            // 用于所有请求的 Pdu 类
            Pdu pdu = new Pdu(PduType.GetBulk);
            // 在此示例中，将 NonRepeaters 值设置为 0
            pdu.NonRepeaters = 0;
            // MaxRepetitions 告诉代理要返回多少个 Oid/Value 对
            // 在响应中。
            pdu.MaxRepetitions = 5;
            // 循环结果
            while (lastOid != null)
            {
                // 首次构造Pdu类时，RequestId设置为0
                // 并且在编码期间 id 将被设置为随机值
                // 对于后续请求，id 将被设置为一个值
                // 需要递增以对每个请求具有唯一的请求 ID
                // 数据包
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                // 从 Pdu 类中清除 Oid。
                pdu.VbList.Clear();
                // 使用最后检索到的 Oid 初始化请求 PDU
                pdu.VbList.Add(lastOid);
                // 发出 SNMP 请求
                SnmpPacket result = null;
                try
                {
                    result = target.Request(pdu, param);
                }
                catch (SnmpException ex)
                {

                }
                // 如果在实际应用中使用，您应该在请求中捕获异常。
                // 如果结果为空，则代理没有回复或者我们无法解析回复。
                if (result != null)
                {
                    // ErrorStatus other then 0 是一个错误返回
                    // 代理 - 有关错误定义，请参阅 SnmpConstants
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // 代理报告请求错误
                        lastOid = null;
                      
                        MessageBox.Show(String.Format("SNMP回复错误！错误代码：{0}，错误索引：第 {1} 行 \r\n",
                            FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                            result.Pdu.ErrorIndex));
                        break;
                    }
                    else
                    {
                        // 遍历返回的变量绑定
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // 检查检索到的 Oid 是否是根 OID 的“子级”
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                //Console.WriteLine("{0} ({1}): {2}",
                                //    v.Oid.ToString(),
                                //    SnmpConstants.GetTypeName(v.Value.Type),
                                //    v.Value.ToString());
                                string[] hex  = Regex.Split(v.Value.ToString(), "\\s+", RegexOptions.IgnoreCase);
                                string Time = String.Empty;
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
                                    Time = (year + "-" + month + "-" + day + "," + hour + ":" + min + ":" + sed + ":" + mil);

                                }
                                string vValue = String.Join("", Regex.Split(v.Value.ToString(), "\\s+", RegexOptions.IgnoreCase)).ToLower();
                                string[] resltvalue = { ip ,community, result.Pdu.VbList.Count.ToString() , v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), vValue, Time};
                                arry.Add(resltvalue);


                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                    lastOid = null;
                                else
                                    lastOid = v.Oid;
                            }
                            else
                            {
                                // 我们已经到达请求的末尾
                                // MIB 树。 将 lastOid 设置为 null 并退出循环
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
            return arry;
        }

        public static string Set(string ip, string readecommunity,string writecommunity, int timeout, int retry, string oid,string value,string WriteType) {
            // SNMP团体名称 
            //定义代理参数类 
            string Set = "0";
            AgentParameters param = new AgentParameters(SnmpVersion.Ver2, new OctetString(readecommunity), true);
            //将SNMP版本设置为1（或2） 
            //构造代理地址对象
            //这里很容易使用IpAddress类，因为
            //如果不
            //解析为IP地址，它将尝试解析构造函数参数
            IpAddress agent = new IpAddress(ip);
            //构建目标 
            UdpTarget target = new UdpTarget(new IPAddress(agent), 161, timeout, retry);
            //  用于所有请求PDU级 
            Pdu pdu = new Pdu(PduType.Get);
            try
            {
                pdu.VbList.Add(oid);

            }
            catch {
                target.Close();

                return Set;
            }


            SnmpPacket result = null;
            try
            {
                result = target.Request(pdu, param);
            }
            catch 
            {
                Set = "请求后未收到回复";
                target.Close();

                return Set;
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
                    Set = (String.Format("SNMP回复错误！错误代码：{0}，错误索引：第{1}行\r\n",
                            FindDevType.FindErrorCode(result.Pdu.ErrorStatus),
                            result.Pdu.ErrorIndex));
                    return Set;
                    
                }
                else
                {
                    //返回变量的返回顺序与添加
                    //到VbList
                    WriteType = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);

                }
            }
            else
            {
                WriteType = "Integer32";
            }
            target.Close();

            if (WriteType == "Unknown") {
                WriteType = "Integer32";
            }
            // Prepare target
            target = new UdpTarget((IPAddress)new IpAddress(ip));
            // Create a SET PDU
            pdu = new Pdu(PduType.Set);
            // Set sysLocation.0 to a new string
            if (value == "ffffffff" && WriteType == "OctetString")
            {
                pdu.VbList.Add(new Oid(oid), new OctetString(new byte[] { 255, 255, 255, 255 }));
            }
            if (WriteType == "OctetString" && value != "ffffffff")
            {
                pdu.VbList.Add(new Oid(oid), new OctetString(value));

            }
            if (WriteType == "Integer32")
            {
                pdu.VbList.Add(new Oid(oid), new Integer32(value));
            }
            if (WriteType == "UInteger32")
            {
                pdu.VbList.Add(new Oid(oid), new UInteger32(value));
            }
            if (WriteType == "Gauge32")
            {
                pdu.VbList.Add(new Oid(oid), new Gauge32(value));
            }
            if (WriteType == "TimeTicks")
            {
                pdu.VbList.Add(new Oid(oid), new TimeTicks(value));
            }
            if (WriteType == "IpAddress")
            {
                pdu.VbList.Add(new Oid(oid), new IpAddress(value));
            }
            AgentParameters aparam = aparam = new AgentParameters(SnmpVersion.Ver2, new OctetString(writecommunity), true);


            // Response packet
            SnmpV2Packet response;
            try
            {
                // Send request and wait for response
                response = target.Request(pdu, aparam) as SnmpV2Packet;
            }
            catch 
            {
                // If exception happens, it will be returned here
                Set= "请求后未收到回复";
                target.Close();

                return Set;
            }
            // Make sure we received a response
            if (response == null)
            {
                Set="发送错误的SNMP请求";
                target.Close();
                return Set;
            }
            else
            {
                // Check if we received an SNMP error from the agent
                if (response.Pdu.ErrorStatus != 0)
                {
                    Set = (String.Format("SNMP回复错误！错误代码:{0}，错误索引：第{1}行\r\n",
                        FindDevType.FindErrorCode(response.Pdu.ErrorStatus), response.Pdu.ErrorIndex));
                    return Set;
                }
                else
                {
                    // Everything is ok. Agent will return the new value for the OID we changed
                    try
                    {
                        Set = response.Pdu[0].Value.ToString();

                    }
                    catch {
                        Set = "配置参数不合法";
                    }
                    target.Close();

                }
            }
            return Set;
        }

    }
}
