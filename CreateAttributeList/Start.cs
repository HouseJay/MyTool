using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateAttributeList
{
    class Start
    {
       public void start()
        {
            string strPatch = "E:\\BaiduNetdiskDownload\\windows\\system32";

            string YFT50 = string.Empty;

//FileInfo fileinfo = new FileInfo(@"");

            try
            {

                YFT50 = getFoldersFileAttribute(strPatch);
                StreamWriter sw = File.CreateText(strPatch+"\\YFT50.sj");
                sw.Write(YFT50);
                sw.Flush();
                sw.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private String getFoldersFileAttribute(string patch)
        {
            string YFT50 = string.Empty;
            string[] folders = Directory.GetDirectories(patch);
            string[] files = Directory.GetFiles(patch);

            StringBuilder YFT50Builder = new StringBuilder();

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string name = fileInfo.Name;
                long lentch = fileInfo.Length;
                bool readOnly = fileInfo.IsReadOnly;
                DateTime lastWriteTime = fileInfo.LastWriteTime;

                YFT50Builder.Append("<" + name + "/" + lentch + "/" + lastWriteTime.ToString("yyyyMMdd") + "/" + readOnly.ToString() + ">");
            }
            foreach(string folder in folders)
            {
               YFT50Builder.Append(getFoldersFileAttribute(folder));
            }
            YFT50 = YFT50Builder.ToString();
            return YFT50;
        }

        //string[] folders = Directory.GetDirectories(strPatch);
        //string[] files = Directory.GetFiles(strPatch);

        //StringBuilder YFT50Builder = new StringBuilder();

        //foreach (string file in files)
        //{
        //    FileInfo fileInfo = new FileInfo(file);
        //    string name = fileInfo.Name;
        //    long lentch = fileInfo.Length;
        //    bool readOnly = fileInfo.IsReadOnly;
        //    DateTime lastWriteTime = fileInfo.LastWriteTime;

        //    YFT50Builder.Append("<"+name+"/"+lentch+"/"+lastWriteTime.ToString("yyyyMMdd")+"/"+readOnly.ToString()+">");
        //}

        //YFT50 = YFT50Builder.ToString();
    }
}
