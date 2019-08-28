using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OperationsTool.Tool
{
   public class LogWriter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logState"></param>
        public static void  WriteLogInfo(string message,LOGSTATE logState)
        {
            switch (logState)
            {
                case LOGSTATE.Info:
                    writeInfo(message, "Info");
                    break;
                case LOGSTATE.Warning:
                    writeInfo(message, "Warning");
                    break;
                case LOGSTATE.Error:
                    writeInfo(message, "Error");
                    break;
                case LOGSTATE.Fatal:
                    writeInfo(message, "Fatal");
                    break;
                case LOGSTATE.Event:
                    writeInfo(message, "Event");
                    break;
                default:
                    writeInfo("调用日志记录出错，未知LOGSTATE;"+message, "Error");
                    break;
            }
        }
    private static void writeInfo(string message,string catalog)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            path = System.IO.Path.Combine(path
            , "Logs\\"+catalog+"\\");

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string fileFullName = System.IO.Path.Combine(path
            , string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd")));

            using (StreamWriter output = System.IO.File.AppendText(fileFullName))
            {
                output.WriteLine(message);
                output.Close();
            }
        }
    }
}
