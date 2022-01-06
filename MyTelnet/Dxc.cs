using Org.BouncyCastle.Utilities;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MyGpnSoftware
{
    class Dxc
    {
        public static uint TsCreate(uint ifType, uint subType, uint srcSlot, uint srcPort, uint srcHp, uint srcLp)
        {
            uint CreateHpIfIndex = 0;
            if (srcHp != 0)
            {
                try
                {
                    CreateHpIfIndex = (ifType << 26) + (subType << 23) + (srcSlot << 18) + (srcPort << 11) + (srcHp << 6) + srcLp;
                    return CreateHpIfIndex;

                }
                catch
                {
                    return CreateHpIfIndex;
                }

            }
            else {
                try
                {
                    CreateHpIfIndex = (ifType << 26) + (subType << 23) + (srcSlot << 18) + (srcPort << 11) + (srcHp << 6) + srcLp;
                    return CreateHpIfIndex;

                }
                catch
                {
                    return CreateHpIfIndex;
                }

            }



        }

        public static uint TsDxcSearch(uint srcSlot, uint srcPort, uint srcHp, uint srcLp, string VC)
        {
            uint ifType = 0;
            if (VC.Contains("VC12")) { ifType = 22; }
            if (VC.Contains("VC3")) { ifType = 31; }
            if (VC.Contains("VC4")) { ifType = 23; }
            uint subType = 4;
            uint CreateHpIfIndex = 0;

            try
            {
                CreateHpIfIndex = (ifType << 26) + (subType << 23) + (srcSlot << 18) + (srcPort << 11) + (srcHp << 6) + srcLp;
                return CreateHpIfIndex;

            }
            catch
            {
                return CreateHpIfIndex;
            }


        }
        public static List<string[]> Array;
        public static string ipp;
        public static string TsPathIdSearch(string ip, string community, string pohifindex, string vc) {

            string srcLpOid = "";
            if (!vc.Contains("5"))
            {
                srcLpOid = "1.3.6.1.4.1.10072.6.11.2.1.9";

            }
            else
            {
                //srcLpOid = "1.3.6.1.4.1.10072.6.11.2.1.4";

            }
            List<string[]> array;
            if (Array == null || ipp != ip) {
                array = Snmp.Array(ip, community, 2000, 2, srcLpOid);
                Array = array;
            }
            ipp = ip;
            string pathid = "0";
            foreach (var row in Array)
            {
                //metroTextResultDXC.AppendText(row[5] + "没找到\r\n");

                if (row[5].Contains(pohifindex))
                {
                    string[] hex = Regex.Split(row[3], @"[.]", RegexOptions.IgnoreCase);
                    pathid = hex[12];
                }
            }

            return pathid;
        }
        public static string OtnTsPathIdSearch(string ip, string community, string pohifindex)
        {

            string oduDxcSrcWorkIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.3";
            string oduDxcSinkIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.5";

            string pathid = "0";

            List<string[]> array = Snmp.Array(ip, community, 2000, 2, oduDxcSinkIfList);
          
            foreach (var row in array)
            {
                //metroTextResultDXC.AppendText(row[5] + "没找到\r\n");

                if (row[5]==pohifindex)
                {
                    string[] hex = Regex.Split(row[3], @"[.]", RegexOptions.IgnoreCase);
                    pathid = hex[16];
                }
            }
            if (pathid == "0") {

                array = Snmp.Array(ip, community, 2000, 2, oduDxcSrcWorkIfList);
                foreach (var row in array)
                {
                    //metroTextResultDXC.AppendText(row[5] + "没找到\r\n");

                    if (row[5]==pohifindex)
                    {
                        string[] hex = Regex.Split(row[3], @"[.]", RegexOptions.IgnoreCase);
                        pathid = hex[16];
                    }
                }
            }
            return pathid;
        }
        public static string OdukindexSearch(string ip, string community, string pohifindex)
        {

            string oduklineoid = "1.3.6.1.4.1.10072.6.8.1.1.1.2.1.6";
            string odukcltoid = "1.3.6.1.4.1.10072.6.8.1.1.2.5.1.6";
            string oduktype = "";
            string odukid = "0";
            string odukiftype = "0";

            List<string[]> array= Snmp.Array(ip, community, 2000, 2, oduklineoid);
            foreach (var row in array)
            {
                //metroTextResultDXC.AppendText(row[5] + "没找到\r\n");

                if (ZhuanYou10to10(row[5]).Contains(pohifindex))
                {
                    string[] hex = Regex.Split(row[3], @"[.]", RegexOptions.IgnoreCase);
                    odukid = hex[16];
                    odukiftype = hex[17];
                    oduktype = "OTN";

                }
            }


            array = Snmp.Array(ip, community, 2000, 2, odukcltoid);
                foreach (var row in array)
                {
                    //metroTextResultDXC.AppendText(row[5] + "没找到\r\n");

                    if (ZhuanYou10to10(row[5]).Contains(pohifindex))
                    {
                        string[] hex = Regex.Split(row[3], @"[.]", RegexOptions.IgnoreCase);
                        odukid = hex[16];
                        odukiftype = hex[17];
                    oduktype = "ANY";
                    }
                }
            return oduktype;
        }
        public static (uint ifType, uint subType, uint srcSlot, uint srcPort, uint srcHp, uint srcLp) TsRestore(string srcCreateHpIfIndex)
        {
            uint CreateHpIfIndex = 0;
            uint ifType = 0;
            uint subType = 0;
            uint srcSlot = 0;
            uint srcPort = 0;
            uint srcHp = 0;
            uint srcLp = 0;
            try
            {
                CreateHpIfIndex = uint.Parse(srcCreateHpIfIndex);
            }
            catch
            {
                return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
            }
            ifType = CreateHpIfIndex >> 26;
            subType = (CreateHpIfIndex & 0x3800000) >> 23;
            srcSlot = (CreateHpIfIndex & 0x7C0000) >> 18;
            srcPort = (CreateHpIfIndex & 0x3F800) >> 11;
            srcHp = (CreateHpIfIndex & 0x7C0) >> 6;
            srcLp = CreateHpIfIndex & 0x3f;
            return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);

        }
        public static (uint ifType, uint subType, uint srcSlot, uint srcPort, uint srcHp, uint srcLp) TsVcg(string srcCreateHpIfIndex)
        {
            uint CreateHpIfIndex = 0;
            uint ifType = 0;
            uint subType = 0;
            uint srcSlot = 0;
            uint srcPort = 0;
            uint srcHp = 0;
            uint srcLp = 0;
            try
            {
                CreateHpIfIndex = uint.Parse(srcCreateHpIfIndex);
            } catch {
                return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
            }

            ifType = CreateHpIfIndex >> 26;
            subType = (CreateHpIfIndex & 0x3800000) >> 23;
            srcSlot = (CreateHpIfIndex & 0x7C0000) >> 18;
            srcPort = (CreateHpIfIndex & 0x3FC00) >> 10;
            srcHp = (CreateHpIfIndex & 0x3C0) >> 6;
            srcLp = CreateHpIfIndex & 0x3f;
            return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);

        }

        public static (uint ifType, uint subType, uint srcSlot, uint srcPort, uint srcHp, uint srcLp) TsEth(string srcCreateHpIfIndex)
        {
            uint CreateHpIfIndex = 0;
            uint ifType = 0;
            uint subType = 0;
            uint srcSlot = 0;
            uint srcPort = 0;
            uint srcHp = 0;
            uint srcLp = 0;
            try
            {
                CreateHpIfIndex = uint.Parse(srcCreateHpIfIndex);
            }
            catch
            {
                return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
            }

            ifType = CreateHpIfIndex >> 26;
            subType = (CreateHpIfIndex & 0x3800000) >> 23;
            srcSlot = (CreateHpIfIndex & 0x7C0000) >> 18;
            srcPort = (CreateHpIfIndex & 0x3F000) >> 12;
            srcHp = (CreateHpIfIndex & 0x3C0) >> 6;
            srcLp = CreateHpIfIndex & 0x3f;
            return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);

        }

        public static (uint ifType, uint subType, uint srcSlot, uint srcPort, uint srcHp, uint srcLp) TsOtnDxc(string srcCreateHpIfIndex)
        {
            uint CreateHpIfIndex = 0;
            uint ifType = 0;
            uint subType = 0;
            uint srcSlot = 0;
            uint srcPort = 0;
            uint srcHp = 0;
            uint srcLp = 0;

            if (!srcCreateHpIfIndex.Contains("-"))
            {
                if (IsHexadecimal(srcCreateHpIfIndex) == true)
                {
                    try
                    {
                        CreateHpIfIndex = System.Convert.ToUInt32(srcCreateHpIfIndex, 16);

                    }
                    catch
                    {
                        return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
                    }
                }
                else
                {
                    try
                    {
                        CreateHpIfIndex = uint.Parse(srcCreateHpIfIndex);

                    }
                    catch
                    {
                        return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
                    }

                }
            }
            else {
                try
                {
                    CreateHpIfIndex = (uint)int.Parse(srcCreateHpIfIndex);

                }
                catch
                {
                    return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);
                }
            }
            ifType = CreateHpIfIndex >> 26;
            subType = (CreateHpIfIndex & 0x3C00000) >> 22;
            srcSlot = (CreateHpIfIndex & 0x3E0000) >> 17;
            srcPort = (CreateHpIfIndex & 0x1F800) >> 11;
            srcHp = (CreateHpIfIndex & 0x700) >> 8;
            srcLp = CreateHpIfIndex & 0xff;
            return (ifType, subType, srcSlot, srcPort, srcHp, srcLp);



        }



        public static string TsOtnDxcindex(string srcCreateHpIfIndex)
        {
            uint CreateHpIfIndex = 0;
            uint ifType = 0;
            uint subType = 0;
            uint srcSlot = 0;
            uint srcPort = 0;
            uint srcHp = 0;
            uint srcLp = 0;
            string Ts = "";

            if (!srcCreateHpIfIndex.Contains("-"))
            {
                if (IsHexadecimal(srcCreateHpIfIndex) == true)
                {
                    try
                    {
                        CreateHpIfIndex = System.Convert.ToUInt32(srcCreateHpIfIndex, 16);

                    }
                    catch
                    {
                        return Ts;
                    }
                }
                else
                {
                    try
                    {
                        CreateHpIfIndex = uint.Parse(srcCreateHpIfIndex);

                    }
                    catch
                    {
                        return Ts;
                    }

                }
            }
            else
            {
                try
                {
                    CreateHpIfIndex = (uint)int.Parse(srcCreateHpIfIndex);

                }
                catch
                {
                    return Ts;
                }
            }
            ifType = CreateHpIfIndex >> 26;
            subType = (CreateHpIfIndex & 0x3C00000) >> 22;
            srcSlot = (CreateHpIfIndex & 0x3E0000) >> 17;
            srcPort = (CreateHpIfIndex & 0x1F800) >> 11;
            srcHp = (CreateHpIfIndex & 0x700) >> 8;
            srcLp = CreateHpIfIndex & 0xff;
            Ts = srcSlot.ToString() + "-" + srcPort.ToString() + "-" + srcLp.ToString();
            return Ts;



        }
        /// <summary>
        /// 判断是否十六进制格式字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsHexadecimal(string str)
        {
            const string PATTERN = @"([^A-Fa-f0-9]|\s+?)+";
            return System.Text.RegularExpressions.Regex.IsMatch(str, PATTERN);
        }


        public static string appVer = "1.3.6.1.4.1.10072.6.2.1.1.1.8.1";
        public static string fpgaVer = "1.3.6.1.4.1.10072.6.2.1.1.1.7.1";
        //ETH端口
        public static string EthIfIndex = "1.3.6.1.4.1.10072.2.60.8.1.3.";
        public static string ethIfMediaType = "1.3.6.1.4.1.10072.6.20.1.1.1.2.";
        public static string ethIfOperStatus = "1.3.6.1.4.1.10072.6.20.1.1.1.5.";
        public static string ethIfAutoNegAdminStatus = "1.3.6.1.4.1.10072.6.20.1.1.1.11.";
        public static string ethIfMauType = "1.3.6.1.4.1.10072.6.20.1.1.1.9.";
        public static string ethIfMtu = "1.3.6.1.4.1.10072.6.20.1.1.1.25.";
        public static string ethIfVlanTpId = "1.3.6.1.4.1.10072.6.20.1.1.1.56.";
        public static string rtUpBandWidth = "1.3.6.1.4.1.10072.2.19.2.3.1.1.1.";        //上行带宽 in方向
        public static string rtDownBandWidth = "1.3.6.1.4.1.10072.2.19.2.3.1.2.1.";
        public static string eosServiceMatchType = "1.3.6.1.4.1.10072.2.60.23.1.1.1.";     //EOS端口模式 后面端口不是ifindex,是1.1

        //数据光模块
        public static string optSfpPresent = "1.3.6.1.4.1.10072.6.10.1.1.1.12.";     //光模块在位
        public static string optBitRate = "1.3.6.1.4.1.10072.6.10.1.1.1.5.";      //光模块速率
        public static string optFiberOrCopperType = "1.3.6.1.4.1.10072.6.10.1.1.1.23.";     //光电类型
        public static string optLaserStatus = "1.3.6.1.4.1.10072.6.10.1.1.1.16.";      //激光状态
        public static string optSingleDouble = "1.3.6.1.4.1.10072.6.10.1.1.1.3.";       //单双向
        public static string optDistance = "1.3.6.1.4.1.10072.6.10.1.1.1.6.";           //传输距离
        public static string optWaveLength = "1.3.6.1.4.1.10072.6.10.1.1.1.4.";       //波长
        public static string optTxPower = "1.3.6.1.4.1.10072.6.10.1.1.1.8.";      //发光
        public static string optRxPower = "1.3.6.1.4.1.10072.6.10.1.1.1.7.";          //收光
        //VCG端口

        public static string vcgPortIfIndex = "1.3.6.1.4.1.10072.2.60.7.1.3.";      //后面跟1.1
        public static string vcgPortBandwidthGranularity = "1.3.6.1.4.1.10072.6.10.5.1.1.1.2.";
        public static string vcgPortTransmissionProtocol = "1.3.6.1.4.1.10072.6.10.5.1.1.1.6.";
        public static string vcgPortChannelMemberNum = "1.3.6.1.4.1.10072.6.10.5.1.1.1.19.";
        public static string vcgPortLcasEn = "1.3.6.1.4.1.10072.6.10.5.1.1.1.8.";
        public static string vcgPortVlanId = "1.3.6.1.4.1.10072.6.10.5.1.1.1.22.";
        public static string vcgPortVlanMode = "1.3.6.1.4.1.10072.6.10.5.1.1.1.21.";
        public static string vcgPortTxChannelNum = "1.3.6.1.4.1.10072.6.10.5.1.1.1.16.";
        public static string vcgPortRxChannelNum = "1.3.6.1.4.1.10072.6.10.5.1.1.1.17.";
        public static string vcgTxC2 = "1.3.6.1.4.1.10072.6.10.5.1.1.1.9.";
        public static string vcgTxV5 = "1.3.6.1.4.1.10072.6.10.5.1.1.1.11.";
        public static string vcgPortAlmStatus = "1.3.6.1.4.1.10072.6.10.5.1.1.1.18.";
        public static string vcgTxChange = "1.3.6.1.4.1.10072.6.10.5.7.1.1.1.";
        public static string vcgTxVid = "1.3.6.1.4.1.10072.6.10.5.7.1.1.2.";
        public static string vcgRxChange = "1.3.6.1.4.1.10072.6.10.5.7.1.1.4.";
        public static string vcgRxVid = "1.3.6.1.4.1.10072.6.10.5.7.1.1.5.";
        public static string eosServiceEthPort = "1.3.6.1.4.1.10072.2.60.23.2.1.2.";
        public static string eosServiceVlanID = "1.3.6.1.4.1.10072.2.60.23.2.1.3.";
        public static string gfpCurrentTxBytes = "1.3.6.1.4.1.10072.6.10.5.4.1.1.2.";
        public static string gfpCurrentRxBytes = "1.3.6.1.4.1.10072.6.10.5.4.1.1.8.";
        //SDH交叉表
        public static string tsDxc = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.1";     //这个是用来WALK的 查询源或者宿接口时隙和PathID 的
        public static string tsDxcFpmMonOid = "1.3.6.1.4.1.10072.6.11.2.1.13.";
        public static string tsDxcloopControlOid = "1.3.6.1.4.1.10072.6.11.2.1.14.";
        public static string tsDxcUnLoopTimeDelayOid = "1.3.6.1.4.1.10072.6.11.2.1.15.";
        //HP时隙
        public static string hpIfIndex = "1.3.6.1.4.1.10072.6.10.5.2.1.1.2.";   //+VCG号—+.1-63        
        public static string highOrderPathVCMonitorEn = "1.3.6.1.4.1.10072.6.10.2.3.1.1.11."; //设置为1使能告警监视，0是禁止
        public static string highOrderPathVCAlmStatus = "1.3.6.1.4.1.10072.6.10.2.3.1.1.9.";    //HP告警
        public static string highOrderPathVCOverheadC2Tx = "1.3.6.1.4.1.10072.6.10.2.3.1.1.7.";
        public static string highOrderPathVCOverheadC2Rx = "1.3.6.1.4.1.10072.6.10.2.3.1.1.8.";
        public static string tsDxcSourceHpChannelIfIndex = "1.3.6.1.4.1.10072.6.11.2.1.2.";     //根据pathid查询源高阶
        //LP时隙
        public static string lpIfIndex = "1.3.6.1.4.1.10072.6.10.5.2.1.1.2.";   //+VCG号—+.1-63        
        public static string lowOrderPathVCMonitorEn = "1.3.6.1.4.1.10072.6.10.2.3.5.1.11."; //设置为1使能告警监视，0是禁止
        public static string lowOrderPathVCAlmStatus = "1.3.6.1.4.1.10072.6.10.2.3.5.1.9.";    //LP告警
        public static string lowOrderPathVCOverheadC2Tx = "1.3.6.1.4.1.10072.6.10.2.3.5.1.13.";
        public static string lowOrderPathVCOverheadC2Rx = "1.3.6.1.4.1.10072.6.10.2.3.5.1.14.";
        public static string lowOrderPathVCOverheadV5Tx = "1.3.6.1.4.1.10072.6.10.2.3.5.1.7.";
        public static string lowOrderPathVCOverheadV5Rx = "1.3.6.1.4.1.10072.6.10.2.3.5.1.8.";
        public static string tsDxcSourceLpChannelIfIndex = "1.3.6.1.4.1.10072.6.11.2.1.8.";     //根据pathid查询源高阶


        //SDH接口



        public static string sdhPortIfIndex = "1.3.6.1.4.1.10072.2.60.3.1.3.";      //端口号 5.1 加上这个
        public static string sdhPortType = "1.3.6.1.4.1.10072.6.10.2.1.1.1.2.";
        public static string sdhPortMonitorEn = "1.3.6.1.4.1.10072.6.10.2.1.1.1.3.";    //告警监视  1 使能 2 禁止 0 自动
        public static string sdhPortAlmStatus = "1.3.6.1.4.1.10072.6.10.2.1.1.1.6.";    //告警状态
        public static string sdhPortLoop = "1.3.6.1.4.1.10072.6.10.2.1.1.1.5.";      //0禁止  1 系统  2 线路 环回
        public static string rsAlmStatus = "1.3.6.1.4.1.10072.6.10.2.2.1.1.4.";
        public static string msAlmStatus = "1.3.6.1.4.1.10072.6.10.2.2.5.1.1.";


        //电路光模块


        public static string optModSfpPresent = "1.3.6.1.4.1.10072.6.10.8.1.1.12.";     //光模块在位
        public static string optModBitRate = "1.3.6.1.4.1.10072.6.10.8.1.1.5.";      //光模块速率
        public static string optModFiberOrCopperType = "1.3.6.1.4.1.10072.6.10.8.1.1.23.";     //光电类型
        public static string optModLaserStatus = "1.3.6.1.4.1.10072.6.10.8.1.1.16.";      //激光状态
        public static string optModSingleDouble = "1.3.6.1.4.1.10072.6.10.8.1.1.3.";       //单双向
        public static string optModDistance = "1.3.6.1.4.1.10072.6.10.8.1.1.6.";           //传输距离
        public static string optModWaveLength = "1.3.6.1.4.1.10072.6.10.8.1.1.4.";       //波长
        public static string optModTxPower = "1.3.6.1.4.1.10072.6.10.8.1.1.8.";      //发光
        public static string optModRxPower = "1.3.6.1.4.1.10072.6.10.8.1.1.7.";          //收光



        //any接口
        public static string anyPhyServIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.2.1.1.6.";          //索引
        public static string anyPhyPortLoopState = "1.3.6.1.4.1.10072.6.8.1.1.2.1.1.7.";          //环回
        public static string anyPhyServIfType = "1.3.6.1.4.1.10072.6.8.1.1.2.1.1.5.";          //速率
         //vpsdh
        public static string otnClntSdhServIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.2.2.1.4.";          //索引
        public static string otnClntSdhServIfType = "1.3.6.1.4.1.10072.6.8.1.1.2.2.1.3.";          //速率
        //vpeth
        public static string otnClntEthServIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.2.3.1.4.";          //索引
        public static string otnClntEthServIfType = "1.3.6.1.4.1.10072.6.8.1.1.2.3.1.3.";          //速率
        //OTN接口
        public static string otnPhyOtuIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.1.1.1.4.";          //OTU索引
        public static string otnPhyOduIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.1.1.1.5.";          //odu索引
        public static string otnPhyServIfType = "1.3.6.1.4.1.10072.6.8.1.1.1.1.1.6.";          //速率
        public static string otnPhyPortLoopState = "1.3.6.1.4.1.10072.6.8.1.1.1.1.1.7.";          //环回模式
        //线路侧oduk
        public static string otnLineODUkIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.1.2.1.6.";          //oduk索引
        public static string otnLineODUkScrambling = "1.3.6.1.4.1.10072.6.8.1.1.1.2.1.8.";          //扰码模式
        //客户侧oduk
        public static string otnClntODUkIfIndex = "1.3.6.1.4.1.10072.6.8.1.1.2.5.1.6.";          //oduk索引
        public static string otnClntODUkMapEncMode = "1.3.6.1.4.1.10072.6.8.1.1.2.5.1.7.";          //oduk映射模式
        //oduk交叉表
        public static string oduDxcIndex = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.1.";          //索引
        public static string oduDxcSrcWorkIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.3.";          //源主用oduK索引
        public static string oduDxcSrcProtIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.4.";          //源备用oduk索引
        public static string oduDxcSinkIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.5.";          //宿主用oduk索引
        public static string oduDxcExtraIfList = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.6.";          //宿备用oduK索引
        public static string oduDxcODUkIfType = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.10.";          //oduk速率
        public static string oduDxcServiceType = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.11.";          //业务类型
        public static string oduDxcEncMode = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.12.";          //映射模式
        public static string oduDxcPgIndex = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.14.";          //保护组索引
        public static string oduDxcRowStatus = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.15.";          //激活去激活状态
        public static string oduDxcLoopControl = "1.3.6.1.4.1.10072.6.8.1.2.3.1.2.1.16.";          //环回控制

        //OTUk性能
        public static string otnIfOTUkAlmState = "1.3.6.1.4.1.10072.6.8.1.2.1.2.1.1.15.";          //告警
        public static string otnIfOTUkAlmMask = "1.3.6.1.4.1.10072.6.8.1.2.1.2.1.1.16.";          //告警翻转+使能
        public static string otnIfOTUkType = "1.3.6.1.4.1.10072.6.8.1.2.1.2.1.1.4.";          //OTUK类型
        //Oduk性能
        public static string otnIfODUkAlmState = "1.3.6.1.4.1.10072.6.8.1.2.1.3.1.1.16.";          //告警
        public static string otnIfODUkAlmMask = "1.3.6.1.4.1.10072.6.8.1.2.1.3.1.1.17.";            //告警翻转+使能
        public static string otnIfODUkIfType = "1.3.6.1.4.1.10072.6.8.1.2.1.3.1.1.4.";          //oduk类型
        //OdukT性能
        public static string otnIfODUkTAlmState = "1.3.6.1.4.1.10072.6.8.1.2.1.4.1.1.13.";          //告警
        //OpuK性能
        public static string otnIfOPUkPtTx = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.2.";          //PT发送
        public static string otnIfOPUkPtExp = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.3.";          //PT期望
        public static string otnIfOPUkPtRx = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.4.";          //PT接收
        public static string otnIfOPUkAlmState = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.5.";          //OPU告警
        public static string otnIfOPUkMsiTx = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.7.";          //MSI发送
        public static string otnIfOPUkMsiExp = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.8.";          //MSI期望
        public static string otnIfOPUkMsiRx = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.9.";          //MSI接收
        public static string otnIfOPUkMsiMode = "1.3.6.1.4.1.10072.6.8.1.2.1.5.1.1.10.";          //MSI模式


        public static string ZhuanYou10to16(string str) {
            uint i = 0;
            string j = "0";
            try
            {
                i = (uint)int.Parse(str);
                j = System.Convert.ToString(i, 16);
            }
            catch {

            }
            return j;
        }
        public static string ZhuanYou10to10(string str)
        {
            uint i = 0;
            try
            {
                 i = (uint)int.Parse(str);
            }
            catch {

            }
            return i.ToString();

        }
        public static string MAUMib(string Maumib)
        {
            string[,] array = new string[,] {
                { "1.3.6.1.2.1.26.4.1", "AUI"            },
                { "1.3.6.1.2.1.26.4.2", "105"        },
                { "1.3.6.1.2.1.26.4.3", "Foirl"          },
                { "1.3.6.1.2.1.26.4.4", "102"        },
                { "1.3.6.1.2.1.26.4.5", "10T"        },
                { "1.3.6.1.2.1.26.4.6", "10FP"       },
                { "1.3.6.1.2.1.26.4.7", "10FB"       },
                { "1.3.6.1.2.1.26.4.8", "10FL"       },
                { "1.3.6.1.2.1.26.4.9", "10Broad36"      },
                { "1.3.6.1.2.1.26.4.10","10T半双工"      },
                { "1.3.6.1.2.1.26.4.11","10T全双工"      },
                { "1.3.6.1.2.1.26.4.12","10FL半双工"     },
                { "1.3.6.1.2.1.26.4.13","10FL全双工"     },
                { "1.3.6.1.2.1.26.4.14","100T4"      },
                { "1.3.6.1.2.1.26.4.15","100TX半双工"        },
                { "1.3.6.1.2.1.26.4.16","100TX全双工"        },
                { "1.3.6.1.2.1.26.4.17","100FX半双工"    },
                { "1.3.6.1.2.1.26.4.18","100FX全双工"    },
                { "1.3.6.1.2.1.26.4.19","100T2半双工"    },
                { "1.3.6.1.2.1.26.4.20","100T2全双工"    },
                { "1.3.6.1.2.1.26.4.21","1000光半双工"    },
                { "1.3.6.1.2.1.26.4.22","1000光全双工"    },
                { "1.3.6.1.2.1.26.4.23","1000LX半双工"   },
                { "1.3.6.1.2.1.26.4.24","1000LX全双工"   },
                { "1.3.6.1.2.1.26.4.25","1000SX半双工"   },
                { "1.3.6.1.2.1.26.4.26","1000SX全双工"   },
                { "1.3.6.1.2.1.26.4.27","1000CX半双工"   },
                { "1.3.6.1.2.1.26.4.28","1000CX全双工"   },
                { "1.3.6.1.2.1.26.4.29","1000电半双工"    },
                { "1.3.6.1.2.1.26.4.30","1000电全双工"    },
                { "1.3.6.1.2.1.26.4.31","10GigX"     },
                { "1.3.6.1.2.1.26.4.32","10GigLX4"   },
                { "1.3.6.1.2.1.26.4.33","10GigR"     },
                { "1.3.6.1.2.1.26.4.34","10GigER"    },
                { "1.3.6.1.2.1.26.4.35","10GigLR"    },
                { "1.3.6.1.2.1.26.4.36","10GigSR"    },
                { "1.3.6.1.2.1.26.4.37","10GigW"     },
                { "1.3.6.1.2.1.26.4.38","10GigEW"    },
                { "1.3.6.1.2.1.26.4.39","10GigLW"    },
                { "1.3.6.1.2.1.26.4.40","10GigSW"    },


            };
            string MAU = "";
            for (int i = 0; i < array.GetLength(0); i++) {
                if (array[i, 0] == Maumib) {
                    MAU = array[i, 1];
                }
            }
            return MAU;
        }
        public static string OdukIfType(string str) {
            string oudkiftype = "0";
            switch (str) {
                case "odu0":
                    oudkiftype = "0";
                    break;
                case "odu1":
                    oudkiftype = "1";
                    break;
                case "odu2":
                    oudkiftype = "2";
                    break;
                case "oduflex":
                    oudkiftype = "9";
                    break;
                case "odu2e":
                    oudkiftype = "8";
                    break;
            }
            return oudkiftype;
        }
        public static string OduDxcODUkIfType(string str)
        {
            string oudkiftype = "0";
            switch (str)
            {
                case "0":
                    oudkiftype = "odu0";
                    break;
                case "1":
                    oudkiftype = "odu1";
                    break;
                case "2":
                    oudkiftype = "oud2";
                    break;
                case "9":
                    oudkiftype = "oudflex";
                    break;
                case "8":
                    oudkiftype = "odu2e";
                    break;
            }
            return oudkiftype;
        }
        public static string AnyPhyServIfTypeValue(string str)
        {
            string anyPhyServIfTypeValue0 = "0";
            switch (str)
            {
                case "1":
                    anyPhyServIfTypeValue0 = "OTU1";
                    break;
                case "2":
                    anyPhyServIfTypeValue0 = "OTU2";
                    break;
                case "20":
                    anyPhyServIfTypeValue0 = "OTU0";
                    break;
                case "9":
                    anyPhyServIfTypeValue0 = "GE";
                    break;
                case "10":
                    anyPhyServIfTypeValue0 = "XGE";
                    break;
                case "8":
                    anyPhyServIfTypeValue0 = "FE";
                    break;
                case "16":
                    anyPhyServIfTypeValue0 = "STM-1";
                    break;
                case "17":
                    anyPhyServIfTypeValue0 = "STM-4";
                    break;
                case "18":
                    anyPhyServIfTypeValue0 = "STM-16";
                    break;
                case "19":
                    anyPhyServIfTypeValue0 = "STM-16";
                    break;
            }
            return anyPhyServIfTypeValue0;
        }
        public static string OduDxcLoopControl(string str)
        {
            string oudkiftype = "0";
            switch (str)
            {
                case "0":
                    oudkiftype = "不存在";
                    break;
                case "1":
                    oudkiftype = "不环回";
                    break;
                case "2":
                    oudkiftype = "源端环回";
                    break;
                case "3":
                    oudkiftype = "宿端环回";
                    break;
                case "4":
                    oudkiftype = "双向环回";
                    break;
            }
            return oudkiftype;
        }
        public static string OduDxcEncMode(string str)
        {
            string anyPhyServIfTypeValue0 = "0";
            switch (str)
            {
                case "1":
                    anyPhyServIfTypeValue0 = "other";
                    break;
                case "2":
                    anyPhyServIfTypeValue0 = "AMP";
                    break;
                case "3":
                    anyPhyServIfTypeValue0 = "BMP";
                    break;
                case "4":
                    anyPhyServIfTypeValue0 = "GMP";
                    break;
                case "5":
                    anyPhyServIfTypeValue0 = "TTT-GMP";
                    break;
                case "6":
                    anyPhyServIfTypeValue0 = "GFP-F";
                    break;
                case "7":
                    anyPhyServIfTypeValue0 = "GFP-T";
                    break;
                case "8":
                    anyPhyServIfTypeValue0 = "IMP";
                    break;
                case "9":
                    anyPhyServIfTypeValue0 = "oduk-oduk";
                    break;
            }
            return anyPhyServIfTypeValue0;
        }
        public static string OduDxcServiceType(string str)
        {
            string anyPhyServIfTypeValue0 = "0";
            switch (str)
            {
                case "1":
                    anyPhyServIfTypeValue0 = "OTN";
                    break;
                case "2":
                    anyPhyServIfTypeValue0 = "ETH";
                    break;
                case "3":
                    anyPhyServIfTypeValue0 = "SDH";
                    break;
                case "0":
                    anyPhyServIfTypeValue0 = "未知";
                    break;

            }
            return anyPhyServIfTypeValue0;
        }

    }
}
