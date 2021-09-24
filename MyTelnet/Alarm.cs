using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGpnSoftware
{
    class Alarm
    {
        public static string VcgAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "GFP-LOF"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + "" + "GFP-LCSS"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "GFP-LCS"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "GFP-PtiErr"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "GFP-ExiErr"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "GFP-UpiMismatch"; }
                if (AlarmState[6] == '1') { Alarm = Alarm + " " + "GFP-CidMismatch"; }
                if (AlarmState[7] == '1') { Alarm = Alarm + " " + "GFP-SpareMismatch"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string LpAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "AIS"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "LOP"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "UNEQ"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "PLM"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "RDI"; }
                if (AlarmState[7] == '1') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[8] == '1') { Alarm = Alarm + " " + "DEG"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string HpAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "AIS"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "LOP"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "UNEQ"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "PLM"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "LOM"; }
                if (AlarmState[6] == '1') { Alarm = Alarm + " " + "RDI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string RsAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "RS-LOF"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "RS-OOF"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "RS-TIM"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string MsAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "MS-AIS"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "MS-DEG"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "MS-EXC"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "MS-RDI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string SdhPortAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "LOS"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }

            return Alarm;
        }
        public static string OtuAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "LOF"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "LOM"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[6] == '1') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[7] == '1') { Alarm = Alarm + " " + "FEC"; }
                if (AlarmState[8] == '1') { Alarm = Alarm + " " + "LOS"; }
                if (AlarmState[9] == '1') { Alarm = Alarm + " " + "IAE"; }
                if (AlarmState[10] == '1') { Alarm = Alarm + " " + "BIAE"; }
                if (AlarmState[11] == '1') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[12] == '1') { Alarm = Alarm + " " + "BEI"; }
                if (AlarmState[13] == '1') { Alarm = Alarm + " " + "SM_BIAE"; }

                if (Alarm == "") { Alarm = alarm; }
            } catch { 
                return Alarm;
            }
            if (Alarm == "00000000") {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OdukAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "OCI"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "LCK"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[6] == '1') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[7] == '1') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[8] == '1') { Alarm = Alarm + " " + "LOM"; }
                if (AlarmState[9] == '1') { Alarm = Alarm + " " + "BEI"; }
                if (AlarmState[10] == '1') { Alarm = Alarm + " " + "LOFLOM"; }

                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }
            if (Alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OdukTcmAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "LTC"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "OCI"; }
                if (AlarmState[2] == '1') { Alarm = Alarm + " " + "LCK"; }
                if (AlarmState[3] == '1') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[4] == '1') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[5] == '1') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[6] == '1') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[7] == '1') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[8] == '1') { Alarm = Alarm + " " + "IAE"; }
                if (AlarmState[9] == '1') { Alarm = Alarm + " " + "BIAE"; }
                if (AlarmState[10] == '1') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[11] == '1') { Alarm = Alarm + " " + "BEI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }
            if (Alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OtuAlarmMask(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '0') { Alarm = "LOF"; }
                if (AlarmState[1] == '0') { Alarm = Alarm + " " + "LOM"; }
                if (AlarmState[2] == '0') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[3] == '0') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[4] == '0') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[5] == '0') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[6] == '0') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[7] == '0') { Alarm = Alarm + " " + "FEC"; }
                if (AlarmState[8] == '0') { Alarm = Alarm + " " + "LOS"; }
                if (AlarmState[9] == '0') { Alarm = Alarm + " " + "IAE"; }
                if (AlarmState[10] == '0') { Alarm = Alarm + " " + "BIAE"; }
                if (AlarmState[11] == '0') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[12] == '0') { Alarm = Alarm + " " + "BEI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch
            {
                return Alarm;
            }
            if (alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OdukAlarmMask(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '0') { Alarm = "OCI"; }
                if (AlarmState[1] == '0') { Alarm = Alarm + " " + "LCK"; }
                if (AlarmState[2] == '0') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[3] == '0') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[4] == '0') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[5] == '0') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[6] == '0') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[7] == '0') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[8] == '0') { Alarm = Alarm + " " + "LOFLOM"; }
                if (AlarmState[9] == '0') { Alarm = Alarm + " " + "BEI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }
            if (alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OdukTcmAlarmMask(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '0') { Alarm = "LTC"; }
                if (AlarmState[1] == '0') { Alarm = Alarm + " " + "OCI"; }
                if (AlarmState[2] == '0') { Alarm = Alarm + " " + "LCK"; }
                if (AlarmState[3] == '0') { Alarm = Alarm + " " + "TIM"; }
                if (AlarmState[4] == '0') { Alarm = Alarm + " " + "DEG"; }
                if (AlarmState[5] == '0') { Alarm = Alarm + " " + "BDI"; }
                if (AlarmState[6] == '0') { Alarm = Alarm + " " + "SSF"; }
                if (AlarmState[7] == '0') { Alarm = Alarm + " " + "AIS"; }
                if (AlarmState[8] == '0') { Alarm = Alarm + " " + "IAE"; }
                if (AlarmState[9] == '0') { Alarm = Alarm + " " + "BIAE"; }
                if (AlarmState[10] == '0') { Alarm = Alarm + " " + "EXC"; }
                if (AlarmState[11] == '0') { Alarm = Alarm + " " + "BEI"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }
            if (alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OpukAlarm(string alarm)
        {
            string Alarm = "";
            try
            {
                char[] AlarmState = System.Convert.ToString(Convert.ToInt32(alarm, 16), 2).PadLeft(32, '0').ToCharArray();
                if (AlarmState[0] == '1') { Alarm = "PLM"; }
                if (AlarmState[1] == '1') { Alarm = Alarm + " " + "MSIM"; }
                if (Alarm == "") { Alarm = alarm; }
            }
            catch { return Alarm; }
            if (Alarm == "00000000")
            {
                Alarm = "OK";
            }
            return Alarm;
        }
        public static string OtnIfOPUkPt(string str)
        {
            string anyPhyServIfTypeValue0 = str;
            switch (str)
            {
                case "0":
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "未知";
                    break;                                             
                case "1":                                              
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "实验映射";
                    break;                                             
                case "2":                                              
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "AMP异步CBR映射";
                    break;                                             
                case "3":                                              
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "BMP比特同步CBR映射";
                    break;                                             
                case "5":                                              
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "GFP映射";
                    break;                                            
                case "7":                                              
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "PCS码字透明以太网映射";
                    break;                                             
                case "10":                                             
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "STM-1到OPU0,GMP映射";
                    break;                                             
                case "11":                                             
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "STM-4到OPU0,GMP映射";
                    break;                                             
                case "32":                                             
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "ODTUjk映射";
                    break;                                             
                case "33":                                             
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "ODTUK.ts映射";
                    break;                                             
                case "254":                                            
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "PRBS测试信号映射";
                    break;                                             
                case "255":                                            
                    anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "无效";
                    break;
                default: anyPhyServIfTypeValue0 = "0x" + int.Parse(str).ToString("X2") + "(" + str + ")" + "暂未定义映射"; break;
            }
            return anyPhyServIfTypeValue0;
        }
    }
}
