using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace OperationsTool
{
    class Program
    {
        /// <summary>
        /// adm:添加金额
        /// rmm:减金额
        /// ci：检测接口并修复
        /// ri: 重装接口
        /// </summary>
        public static string strSystemOrder = "admy,admj,ci,ri,asign,exit"; 
        private static Operation init = new Operation();

        static void Main(string[] args)
        {
            start();
        }

        private static void start()
        {
            int igo = 1;
            do
            {
                Console.WriteLine("请输入密码：");
                string password = Console.ReadLine();
                if (string.Equals(password, "ddws"))
                {
                    igo = 0;
                }
            } while (igo == 1);
           
            Console.WriteLine("请输入操作命令：" + strSystemOrder);
            while(igo == 0)
            {
                string strUOrder = Console.ReadLine();
                switch (strUOrder)
                {
                    case "admy":
                        addYSum();
                        GC.Collect();
                        break;
                    case "admj":
                        addJSum();
                        GC.Collect();
                        break;
                    case "ci":

                        break;
                    case "ri":

                        break;
                    case "asign":

                        break;
                    case "exit":
                        igo = 1;
                        break;
                    default:
                        Console.WriteLine("天才，请输入正确命令！");
                        break;
                }
            }
        }

        private static void addYSum()
        {
            try
            {
                Console.WriteLine("请输入需要申请的金额、如需其他条件请输入：金额/YYYYMMDD/YYYYMMDD/0000 —— 开始日期/结束日期/清算分中心");
                string strArgument = Console.ReadLine();
                string strReturn = init.logicAddSum(strArgument);
                Console.WriteLine(strReturn);
                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
            catch (Exception e)
            {
                Console.WriteLine("出现未知异常:"+e.ToString()+";程序中止");
                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
            
        }

        private static void addJSum()
        {
            try
            {
                Console.WriteLine("请输入需要申请的金额、如需其他条件请输入：金额/YYYYMMDD/YYYYMMDD/0000 —— 开始日期/结束日期/清算分中心");
                string strArgument = Console.ReadLine();
                string strReturn = init.logicAddJSum(strArgument);
                Console.WriteLine(strReturn);
                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
            catch (Exception e)
            {
                Console.WriteLine("出现未知异常:" + e.ToString() + ";程序中止");
                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
        }

        private static void checkInterface()
        {
            try
            {
                Console.WriteLine("检查接口文件.............");
                

                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
            catch(Exception e)
            {
                Console.WriteLine("出现未知异常:" + e.ToString() + ";程序中止");
                Console.WriteLine("请输入操作命令：" + strSystemOrder);
            }
        }
    }
}
