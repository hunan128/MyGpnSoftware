using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MyGpnSoftware
{
    class FindDevType
    {

        public static string Finddevtype(string str) {
            string[] devtypeall = new string[] {"INVALID(0)","GPN7600(98)",
"GPN7500(103)",
"GPN710A(104)",
"GPN7600M(106)",
"GPN7700(107)",
"GPN7500-V1(108)",
"GPN7500-V2(109)",
"OTP5500-W-I(110)",
"OTP5500-W-II(111)",
"OTP5500-W-IV(112)",
"UNKNOWN(1000)",
"OTSA120P(1001)",
"GFT1501C(1002)",
"OTSA240(1003)",
"OTSA480(1004)",
"RESERVE1(1005)",
"OTSE120M(1006)",
"OTSE60(1007)",
"OTSE1500(1008)",
"ETRMFE(1009)",
"EOPFE(1010)",
"RESERVE2(1011)",
"RESERVE3(1012)",
"EC8011(1013)",
"EC8041(1014)",
"EC8081(1015)",
"RESERVE4(1016)",
"GPN601(1017)",
"E6080P(1018)",
"E6080(1019)",
"E6080M(1020)",
"E6080-16(1021)",
"E6080PE(1022)",
"OTSA120J(1023)",
"OTSA120SV(1024)",
"REM_802_3_AH(1025)",
"GPN605(1026)",
"GPN710(1027)",
"GPN603_FAKE(1028)",
"GFT1501S(1029)",
"GFT1501SE(1030)",
"OTS_E120S(1031)",
"OTS_E120V(1032)",
"OTS_E120E(1033)",
"OTS_E60VS(1034)",
"OTS_E120MD(1035)",
"GPN710_B_4FE(1036)",
"GPN710_B_4FE4E1(1037)",
"GPN710_A_4FE(1038)",
"GPN710_A_4FE4E1(1039)",
"GPN701_B_4FE(1040)",
"GPN701_B_4FE4E1(1041)",
"GPN701_A_4FE(1042)",
"GPN701_A_4FE4E1(1043)",
"EC8011_FX(1044)",
"EC8041_FX(1045)",
"EC8081_FX(1046)",
"GPN605_2GE(1047)",
"GPN603_2FX(1048)",
"E6100(1049)",
"E6200_NMU(1050)",
"E6200_OMU(1051)",
"GPN7600S_8FX(1052)",
"GPN7600S_16FX(1053)",
"GPN7600S_24FX(1054)",
"GPN605P(1055)",
"GPN605FAKE_FROM_GPN710A(1056)",
"GPN601G(1057)",
"GPN710B_4GE(1058)",
"GPN710A_2GE(1059)",
"GPN710A_4GE(1060)",
"GPN710-B-4FE4E1(1061)",
"GPN710-B-4FE(1062)",
"GPN710-A-4FE(1063)",
"VIRTUAL_DEVICE(1064)",
"GPN7500-P1(1065)",
"GPN7500-P2(1066)",
"MX700-IPTN(1067)",
"GPN605_2FX(2816)",
"GPN710_B_4FE4E1_V1(2817)",
"GPN710_B_4FE4E1_V2(2818)",
"GPN710B(2819)",
"GPN701-B-CQ(2820)",
"GPN710-B-CQ(2821)",
"GPN710C(2822)",
"GPN710-U2-1(2823)",
"GPN710-U1-1(2824)",
"GPN710U(2825)",
"GPN710_B_4FE4E1_V1_GD(2826)",
"GPN710_B_4FE4E1_V2_GD(2827)",
"GPN605-2SFP-4GE(2828)",
"GPN810(2829)",
"GPN910(2830)",
"GPN720-U2(2831)",
"GPN710-B-4GE(2832)",
"GPN710-B-4GE4E1(2833)",
"GPN720-U1-2-AC220S-B(2834)",
"GPN730-2SFP-4GE(2835)",
"GPN730-M1(2836)",
"GPN730-M2(2837)",
"GPN710D_TANZHEN(2838)",
"GPN710D_PTN(2839)",
"GPN7500-V3(2840)",
"GPN730-M1-4GE(2841)",
"STN6800-II/D(2842)",
"GPN710-M2-2GC2GE4E1(2843)",
"GPN710-M1-4GE(2844)",
"GPN710-4GE-V1(2845)",
"GPN710-4GE-V2(2846)",
"GPN710-U1(2847)",
"GPN710-2SFP(2848)",
"GPN710-2SFP-4GE4E1(2849)",
"GPN710-2SFP-4GE4E1-YN(2850)",
"GPN710-E120(2851)",
"GPN720-U1-2(2852)",
"GPN603-2SFP-4GE-AC220S(2853)",
"GPN603-2SFP-4GE-DC48S(2854)",
"GPN603-PTN-CPE(2855)",
"GPN605-PTN-HUB(2856)",
"MX700-TMEVFV65(2857)",
"MX700-TMEVFV63(2858)",
"GPN800-8AT2(2859)",
"GPN800-4XT2(2860)",
"GPN800-2XT2(2861)",
"GPN800-4GT1(2862)",
"GPN800-8AST2(2863)",
"GPN800-2GT1(2864)",
"GPN800-OTU10G(2865)",
"GPN603-2SFP-4FE-AC220S(2866)",
"GPN7500-P5-AC220S(2867)",
"GPN710-2SFP-4GC(2868)",
"GPN710-2SFP-4GC-AC220D(2869)",
"GPN800-OTU25G(2870)",
"GPN800-OTU25G-AT91(2871)",
"GPN800-OTU25G-E(2872)",
"GPN720-U1-3-AC220S(2873)",
"GPN720-U1-3-AC220D(2874)",
"GPN720-U1-3-DC48S(2875)",
"GPN720-U1-3-DC48D(2876)",
"MX100-FES2(3000)",
"TME16FV61(3001)",
"GPN710-B-4GE4E1-V3(3002)",
"GPN710-B-4GE-V3(3003)",
"GPN605-2SFP-4GE-C(3004)",
"GPN7600S-FR7600(4001)",
"MX700-FR7600(4002)",
"M6000-FR7600(4003)",
"XCOM600-FR7600(4004)"};
            string strr = "";
            foreach (string s in devtypeall)
            {
                if (s.Contains(str))
                {
                    string strRegex = @"([\-\d\w]+)([\(])*";

                    Regex r = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string  m = r.Match(s).Groups[1].Value;
                    strr = m;
                }
            }
            return strr;

        }

        public static string FindErrorCode(int i)
        {
            string[] devtypeall = new string[] {
                "没有错误(0)",
                "请求或回复太大(1)",
                 "要求的名称不存在(2)",
                  "不正确的值(3)",
                   "Oid是只读的(4)",
                    "一般错误(5)",
                     "拒绝访问(6)",
                     "类型错误(7)",
                     "长度错误(8)",
                     "编码错误(9)",
                     "值错误(10)",
                     "不能创建(11)",
                     "值不一致(12)",
                     "资源不可用(13)",
                     "提交失败(14)",
                     "撤销失败(15)",
                     "读写团体认证错误(16)",
                     "不可写(17)",
                     "名称不一致(18)",
                        };
            string CodeName = "";
            foreach (string s in devtypeall)
            {
                if (s.Contains(i.ToString()))
                {
                    string strRegex = @"([\S]+)([\(])*";

                    Regex r = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string m = r.Match(s).Groups[1].Value;
                    CodeName = m;
                }
            }
            return CodeName;


        }

        /// <summary>
        /// 对二维数组排序
        /// </summary>
        /// <param name="values">排序的二维数组</param>
        /// <param name="orderColumnsIndexs">排序根据的列的索引号数组</param>
        /// <param name="type">排序的类型，1代表降序，0代表升序</param>
        /// <returns>返回排序后的二维数组</returns>
        public static object[,] Orderby(object[,] values, int[] orderColumnsIndexs, int type)
        {
            object[] temp = new object[values.GetLength(1)];
            int k;
            int compareResult;
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (k = i + 1; k < values.GetLength(0); k++)
                {
                    if (type.Equals(1))
                    {
                        for (int h = 0; h < orderColumnsIndexs.Length; h++)
                        {
                            compareResult = Comparer.Default.Compare(GetRowByID(values, k).GetValue(orderColumnsIndexs[h]), GetRowByID(values, i).GetValue(orderColumnsIndexs[h]));
                            if (compareResult.Equals(1))
                            {
                                temp = GetRowByID(values, i);
                                Array.Copy(values, k * values.GetLength(1), values, i * values.GetLength(1), values.GetLength(1));
                                CopyToRow(values, k, temp);
                            }
                            if (compareResult != 0)
                                break;
                        }
                    }
                    else
                    {
                        for (int h = 0; h < orderColumnsIndexs.Length; h++)
                        {
                            compareResult = Comparer.Default.Compare(GetRowByID(values, k).GetValue(orderColumnsIndexs[h]), GetRowByID(values, i).GetValue(orderColumnsIndexs[h]));
                            if (compareResult.Equals(-1))
                            {
                                temp = GetRowByID(values, i);
                                Array.Copy(values, k * values.GetLength(1), values, i * values.GetLength(1), values.GetLength(1));
                                CopyToRow(values, k, temp);
                            }
                            if (compareResult != 0)
                                break;
                        }
                    }
                }
            }
            return values;

        }
        /// <summary>
        /// 获取二维数组中一行的数据
        /// </summary>
        /// <param name="values">二维数据</param>
        /// <param name="rowID">行ID</param>
        /// <returns>返回一行的数据</returns>
        static object[] GetRowByID(object[,] values, int rowID)
        {
            if (rowID > (values.GetLength(0) - 1))
                throw new Exception("rowID超出最大的行索引号!");

            object[] row = new object[values.GetLength(1)];
            for (int i = 0; i < values.GetLength(1); i++)
            {
                row[i] = values[rowID, i];

            }
            return row;

        }
        /// <summary>
        /// 复制一行数据到二维数组指定的行上
        /// </summary>
        /// <param name="values"></param>
        /// <param name="rowID"></param>
        /// <param name="row"></param>
        static void CopyToRow(object[,] values, int rowID, object[] row)
        {
            if (rowID > (values.GetLength(0) - 1))
                throw new Exception("rowID超出最大的行索引号!");
            if (row.Length > (values.GetLength(1)))
                throw new Exception("row行数据列数超过二维数组的列数!");
            for (int i = 0; i < row.Length; i++)
            {
                values[rowID, i] = row[i];
            }
        }


        /// <summary>
        /// 将字符串转换成二维数组
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string[,] StringToArray(string original)
        {
            if (original.Length == 0)
            {
                throw new IndexOutOfRangeException("二维数组导入为空");
            }
            //将字符串转换成数组（字符串拼接格式：***,***#***,***#***,***，例如apple,banana#cat,dog#red,black）
            string[] originalRow = original.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] originalColstart = null;
            int[] originalColstartcount = new int [originalRow.Length];

            for (int m = 0; m < originalRow.Length; m++) {
                originalColstart = Regex.Split(originalRow[m], "\\s+", RegexOptions.IgnoreCase);
              //  MessageBox.Show(originalColstart.Length.ToString());
                if (originalColstart != null) {
                    originalColstartcount[m] = originalColstart.Length;

                }
            }
            ArrayList list = new ArrayList(originalColstartcount);
            list.Sort();
            int min = Convert.ToInt32(list[0]);
            int max = Convert.ToInt32(list[list.Count - 1]);
          //  MessageBox.Show(max.ToString());

            string[] originalCol = new string[max]; //string[,]是等长数组，列维度一样，只要取任意一行的列维度即可确定整个二维数组的列维度
            int x = originalRow.Length;
            int y = max;
            string[,] twoArray = new string[x, y];
            for (int i = 0; i < x; i++)
            {
                originalCol = Regex.Split(originalRow[i], "\\s+", RegexOptions.IgnoreCase);
                for (int j = 0; j < originalCol.Length; j++)
                {
                    twoArray[i, j] = originalCol[j];
                }
            }
            return twoArray;
        }


    }
}
