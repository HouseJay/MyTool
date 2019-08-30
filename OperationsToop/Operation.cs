using OperationsTool.Tool;
using OperationsTool.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace OperationsTool
{
    class Operation
    {

        //public string strSourcePath = "D:\\Program Files (x86)\\医付通\\db\\CDSI.mdb";
        //public string strExetuceOrder = "SELECT * FROM JY结算主记录 WHERE 经办时间>#2019-4-1#";

        public DataTable connAccessDatabase()
        {
            return null;
        }

        public string logicAddSum(string strArgument)
        {
            double money = 0;
            string strStartTime = getStartDay();
            string strEndTime = getEndDay();
            string strBranchCenter = null;
            string strDataPath = string.Empty;

            try
            {
                string[] strArgumentArray = strArgument.Split('/');

                switch (strArgumentArray.Count())
                {
                    case 1:
                        money = double.Parse(strArgumentArray[0].Trim());
                        break;
                    case 2:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strBranchCenter = strArgumentArray[1].Trim();
                        break;
                    case 3:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strStartTime = strArgumentArray[1].Trim().Remove(4)+"-"+strArgumentArray[1].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[1].Trim().Substring(6);
                        strEndTime = strArgumentArray[2].Trim().Remove(4) + "-" + strArgumentArray[2].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[2].Trim().Substring(6);
                        break;
                    case 4:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strStartTime = strArgumentArray[1].Trim().Remove(4) + "-" + strArgumentArray[1].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[1].Trim().Substring(6);
                        strEndTime = strArgumentArray[2].Trim().Remove(4) + "-" + strArgumentArray[2].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[2].Trim().Substring(6);
                        strBranchCenter = strArgumentArray[3].Trim();
                        break;
                    default:
                        LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument , LOGSTATE.Error);
                        return "传入参数分割出错请重新输入";
                }

                DataBaseHelper dataBase = new DataBaseHelper();
                //判断电脑位数，和文件存放位置
                if (Environment.Is64BitOperatingSystem)
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
                    Microsoft.Win32.RegistryKey subKey = key.OpenSubKey("SOFTWARE\\Wow6432Node\\CDSI");
                    strDataPath = subKey.GetValue("InstallPath").ToString() + "\\db\\CDSI.mdb";
                }
                else
                {
                    strDataPath = "D:\\Program Files\\医付通db\\CDSI.mdb";
                }
               

                if (dataBase.connection(strDataPath))
                {
                    string strSelectSQL = string.Empty;
                    switch (strBranchCenter)
                    {
                        case null:
                           strSelectSQL = "SELECT 记账流水号,个人编码,费用总额," +
                                "全自费部分,挂钩自付部分,符合范围部分," +
                                "个人账户支付总额,社保基金支付总额,经办时间," +
                                "清算分中心 FROM JY结算主记录" +
                                " WHERE 经办时间 > #" + strStartTime + "#" + " AND 经办时间 < #" + strEndTime + "#";
                            break;
                        default:
                            strSelectSQL = "SELECT 记账流水号,个人编码,费用总额," +
                                "全自费部分,挂钩自付部分,符合范围部分," +
                                "个人账户支付总额,社保基金支付总额,经办时间," +
                                "清算分中心 FROM JY结算主记录 WHERE 经办时间 > #" + strStartTime + "#" + " AND 经办时间 < #" + strEndTime + "#" + " AND 清算分中心= \""+strBranchCenter+"\"";
                            break;
                    }
                    DataTable dtData = dataBase.selectData(strSelectSQL);

                    double dSum = 0;
                    foreach(DataRow row in dtData.Rows)
                    {
                        dSum += Double.Parse(row["费用总额"].ToString());
                    }
                    if(money > dSum)
                    {
                        DataRow changeRow = dtData.Rows[0];
                        string strSerialNum = changeRow["记账流水号"].ToString();
                        money = money - dSum + double.Parse(changeRow["费用总额"].ToString());
                        string strUPSQL = "UPDATE JY结算主记录 SET 费用总额 = \"" + 
                            (float)money + " \",符合范围部分= \"" 
                            +(float)money + "\",个人账户支付总额= \"" 
                            +(float)money + "\" WHERE 记账流水号=\"" + strSerialNum + "\"";

                        int InfluenceROW = dataBase.excuteSQL(strUPSQL);
                        //写入log 更改的所有信息记录。
                        LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                            + ":" + changeRow["个人编码"].ToString()
                            + ":" + changeRow["费用总额"].ToString()
                            + ":" + changeRow["全自费部分"].ToString()
                            + ":" + changeRow["挂钩自付部分"].ToString()
                            + ":" + changeRow["符合范围部分"].ToString()
                            + ":" + changeRow["个人账户支付总额"].ToString()
                            + ":" + changeRow["社保基金支付总额"].ToString()
                            + ":" + changeRow["经办时间"].ToString()
                            + ":" + changeRow["清算分中心"].ToString()
                            , LOGSTATE.Event);
                        return "成功，影响行数：" + InfluenceROW + ";影响记账流水号：" + strSerialNum + ";";
                    }
                    else
                    {
                        double poor = dSum - money;
                        int InfluenceRow = 0;
                        DataTable changeDT = new DataTable();
                        DataRow changeRow = null;

                       for(int i = 0;poor > 0;i++)
                        {
                           changeRow  = dtData.Rows[i];
                           double totalAmount = double.Parse(changeRow["费用总额"].ToString());
                            if(poor > totalAmount)
                            {
                                poor -= totalAmount;

                                string strUPSQL = "UPDATE JY结算主记录 SET 费用总额 =0,符合范围部分=0,个人账户支付总额=0 WHERE 记账流水号=\"" + changeRow["记账流水号"].ToString() + "\"";

                                dataBase.excuteSQL(strUPSQL);
                                //写入log 更改的所有信息记录。
                                LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                                    + ":" + changeRow["个人编码"].ToString()
                                    + ":" + changeRow["费用总额"].ToString()
                                    + ":" + changeRow["全自费部分"].ToString()
                                    + ":" + changeRow["挂钩自付部分"].ToString()
                                    + ":" + changeRow["符合范围部分"].ToString()
                                    + ":" + changeRow["个人账户支付总额"].ToString()
                                    + ":" + changeRow["社保基金支付总额"].ToString()
                                    + ":" + changeRow["经办时间"].ToString()
                                    + ":" + changeRow["清算分中心"].ToString()
                                    , LOGSTATE.Event);
                            }
                            else
                            {
                                money = totalAmount - poor;
                                poor = 0;

                                string strUPSQL = "UPDATE JY结算主记录 SET 费用总额 = \"" +
                                    (float)money + " \",符合范围部分= \""
                                    + (float)money + "\",个人账户支付总额= \""
                                    + (float)money + "\" WHERE 记账流水号=\"" + changeRow["记账流水号"].ToString() + "\"";

                                dataBase.excuteSQL(strUPSQL);
                                //写入log 更改的所有信息记录。
                                LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                                    + ":" + changeRow["个人编码"].ToString()
                                    + ":" + changeRow["费用总额"].ToString()
                                    + ":" + changeRow["全自费部分"].ToString()
                                    + ":" + changeRow["挂钩自付部分"].ToString()
                                    + ":" + changeRow["符合范围部分"].ToString()
                                    + ":" + changeRow["个人账户支付总额"].ToString()
                                    + ":" + changeRow["社保基金支付总额"].ToString()
                                    + ":" + changeRow["经办时间"].ToString()
                                    + ":" + changeRow["清算分中心"].ToString()
                                    ,LOGSTATE.Event);
                                InfluenceRow = i+1;
                            }
                        }
                        return "成功，影响行数：" + InfluenceRow + ";影响记账流水号：已经保存在Event Log文件夹中，请查看;";
                    }
                }
                else
                {
                    LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument+ ";连接数据库出错", LOGSTATE.Error);
                    return "连接数据库出错";
                }

            }
            catch (System.Security.SecurityException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "检测到安全时出现异常，是否是杀毒软件拦截";
            }catch(ObjectDisposedException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "查找安装位置出错";
            }
            catch (ArgumentNullException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":"+"传入参数："+strArgument+";"+ e.ToString(), LOGSTATE.Error);
                return "必要参数为空";
            }catch (FormatException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "格式化出错";
            }catch (OverflowException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "内存溢出";
            }catch (Exception e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                throw e;
            }
        }
        public string logicAddJSum(string strArgument)
        {
            double money = 0;
            string strStartTime = getStartDay();
            string strEndTime = getEndDay();
            string strBranchCenter = null;
            string strDataPath = string.Empty;

            try
            {
                string[] strArgumentArray = strArgument.Split('/');

                switch (strArgumentArray.Count())
                {
                    case 1:
                        money = double.Parse(strArgumentArray[0].Trim());
                        break;
                    case 2:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strBranchCenter = strArgumentArray[1].Trim();
                        break;
                    case 3:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strStartTime = strArgumentArray[1].Trim().Remove(4) + "-" + strArgumentArray[1].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[1].Trim().Substring(6);
                        strEndTime = strArgumentArray[2].Trim().Remove(4) + "-" + strArgumentArray[2].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[2].Trim().Substring(6);
                        break;
                    case 4:
                        money = double.Parse(strArgumentArray[0].Trim());
                        strStartTime = strArgumentArray[1].Trim().Remove(4) + "-" + strArgumentArray[1].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[1].Trim().Substring(6);
                        strEndTime = strArgumentArray[2].Trim().Remove(4) + "-" + strArgumentArray[2].Trim().Substring(4).Remove(2) + "-" + strArgumentArray[2].Trim().Substring(6);
                        strBranchCenter = strArgumentArray[3].Trim();
                        break;
                    default:
                        LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument, LOGSTATE.Error);
                        return "传入参数分割出错请重新输入";
                }

                MysqlDataHelper mysqlHelper = new MysqlDataHelper();
                mysqlHelper.ConnectMysql();
                string selectSQL = string.Empty;
                switch (strBranchCenter)
                {
                    case null:
                        selectSQL = "SELECT * FROM `si_local_trade_record` WHERE `OPERATETIME`>\"" + strStartTime + "\" AND `OPERATETIME`<\"" + strEndTime + "\"";
                        break;
                    default:
                        selectSQL = "SELECT * FROM `si_local_trade_record` WHERE `OPERATETIME`>\"" + strStartTime + "\" AND `OPERATETIME`<\"" + strEndTime + "\" AND `LIQUIDATIONCENTER`=\"" + strBranchCenter + "\"";
                        break;
                }
                DataTable dt = new DataTable();
                dt.Columns.Add("记账流水号");
                dt.Columns.Add("个人编码");
                dt.Columns.Add("费用总额");
                dt.Columns.Add("全自费部分");
                dt.Columns.Add("挂钩自付部分");
                dt.Columns.Add("符合范围部分");
                dt.Columns.Add("个人账户支付总额");
                dt.Columns.Add("社保基金支付总额");
                dt.Columns.Add("经办时间");
                dt.Columns.Add("清算分中心");
                dt = mysqlHelper.selectMysql(selectSQL,dt);

                double dSum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    dSum += Double.Parse(row["费用总额"].ToString());
                }

                if (money > dSum)
                {
                    DataRow changeRow = dt.Rows[0];
                    string strSerialNum = changeRow["记账流水号"].ToString();
                    money = money - dSum + double.Parse(changeRow["费用总额"].ToString());
                    string strUPSQL = "UPDATE `si_local_trade_record` SET `COSTTOTAL` = \"" +
                        (float)money + " \",`ACCORDRANGEPART`= \""
                        + (float)money + "\",`PERSONPAYTOTAL`= \""
                        + (float)money + "\" WHERE `ACCOUNTSTREAMCODE`=\"" + strSerialNum + "\"";
                    mysqlHelper.ConnectMysql();
                    int InfluenceROW = mysqlHelper.updateMysql(strUPSQL);
                    //写入log 更改的所有信息记录。
                    LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                        + ":" + changeRow["个人编码"].ToString()
                        + ":" + changeRow["费用总额"].ToString()
                        + ":" + changeRow["全自费部分"].ToString()
                        + ":" + changeRow["挂钩自付部分"].ToString()
                        + ":" + changeRow["符合范围部分"].ToString()
                        + ":" + changeRow["个人账户支付总额"].ToString()
                        + ":" + changeRow["社保基金支付总额"].ToString()
                        + ":" + changeRow["经办时间"].ToString()
                        + ":" + changeRow["清算分中心"].ToString()
                        , LOGSTATE.Event);
                    return "成功，影响行数：" + InfluenceROW + ";影响记账流水号：" + strSerialNum + ";";
                }
                else
                {
                    double poor = dSum - money;
                    int InfluenceRow = 0;
                    DataTable changeDT = new DataTable();
                    DataRow changeRow = null;

                    for (int i = 0; poor > 0; i++)
                    {
                        changeRow = dt.Rows[i];
                        double totalAmount = double.Parse(changeRow["费用总额"].ToString());
                        if (poor > totalAmount)
                        {
                            poor -= totalAmount;

                            string strUPSQL = "UPDATE `si_local_trade_record` SET `COSTTOTAL`=0,`ACCORDRANGEPART`=0,`PERSONPAYTOTAL`=0 WHERE `ACCOUNTSTREAMCODE`=\"" + changeRow["记账流水号"].ToString() + "\"";

                            mysqlHelper.ConnectMysql();
                            mysqlHelper.updateMysql(strUPSQL);
                            //写入log 更改的所有信息记录。
                            LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                                + ":" + changeRow["个人编码"].ToString()
                                + ":" + changeRow["费用总额"].ToString()
                                + ":" + changeRow["全自费部分"].ToString()
                                + ":" + changeRow["挂钩自付部分"].ToString()
                                + ":" + changeRow["符合范围部分"].ToString()
                                + ":" + changeRow["个人账户支付总额"].ToString()
                                + ":" + changeRow["社保基金支付总额"].ToString()
                                + ":" + changeRow["经办时间"].ToString()
                                + ":" + changeRow["清算分中心"].ToString()
                                , LOGSTATE.Event);
                        }
                        else
                        {
                            money = totalAmount - poor;
                            poor = 0;

                            string strUPSQL = "UPDATE `si_local_trade_record` SET `COSTTOTAL` =\"" +
                                (float)money + " \",`ACCORDRANGEPART`= \""
                                + (float)money + "\",`PERSONPAYTOTAL`= \""
                                + (float)money + "\" WHERE `ACCOUNTSTREAMCODE`=\"" + changeRow["记账流水号"].ToString() + "\"";

                            mysqlHelper.ConnectMysql();
                            mysqlHelper.updateMysql(strUPSQL);
                            //写入log 更改的所有信息记录。
                            LogWriter.WriteLogInfo(changeRow["记账流水号"].ToString()
                                + ":" + changeRow["个人编码"].ToString()
                                + ":" + changeRow["费用总额"].ToString()
                                + ":" + changeRow["全自费部分"].ToString()
                                + ":" + changeRow["挂钩自付部分"].ToString()
                                + ":" + changeRow["符合范围部分"].ToString()
                                + ":" + changeRow["个人账户支付总额"].ToString()
                                + ":" + changeRow["社保基金支付总额"].ToString()
                                + ":" + changeRow["经办时间"].ToString()
                                + ":" + changeRow["清算分中心"].ToString()
                                , LOGSTATE.Event);
                            InfluenceRow = i + 1;
                        }
                    }
                    return "成功，影响行数：" + InfluenceRow + ";影响记账流水号：已经保存在Event Log文件夹中，请查看;";
                }
            }
            catch (System.Security.SecurityException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "检测到安全时出现异常，是否是杀毒软件拦截";
            }
            catch (ObjectDisposedException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "查找安装位置出错";
            }
            catch (ArgumentNullException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "必要参数为空";
            }
            catch (FormatException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "格式化出错";
            }
            catch (OverflowException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                return "内存溢出";
            }
            catch (Exception e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + "传入参数：" + strArgument + ";" + e.ToString(), LOGSTATE.Error);
                throw e;
            }
        }
        public bool checkInterface()
        {
            string patch = string.Empty;
            //判断电脑位数，和文件存放位置
            if (Environment.Is64BitOperatingSystem)
            {
                patch = "C:\\Windows\\SysWOW64";
            }
            else
            {
                patch = "C:\\Windows\\System32";
            }
            byte[] b = OperationsToop.Properties.Resources.YFT50;
            string str = Convert.ToBase64String(b);

            Stream fileSm =  System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("YFT50.sj");
            StreamReader fileStream = new StreamReader(fileSm);
            string YFT50 = fileStream.ReadLine();


            return false;
        }




        private string getEndDay()
        {
            int month = -1;
            int year = -1;
            month = DateTime.Now.Month;
            if(month == 1)
            {
                month = 12;
                year = DateTime.Now.Year - 1;
            }
            else
            {
                month -= 1;
                year = DateTime.Now.Year;
            }
            int day = DateTime.DaysInMonth(year,month);
            string endDay = year + "-" + month + "-" + day;
            return endDay;
        }

        private string getStartDay()
        {
            int month = -1;
            int year = -1;
            month = DateTime.Now.Month;
            if (month == 1)
            {
                month = 12;
                year = DateTime.Now.Year - 1;
            }
            else
            {
                month -= 1;
                year = DateTime.Now.Year;
            }
            string startDay = year + "-" + month + "-" + 1;
            return startDay;
        }
    }
}