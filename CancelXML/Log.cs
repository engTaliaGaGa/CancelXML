using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancelXML
{
    public class Log
    {
        public static void WriteLog(string strValue)
        {
            string path = System.Configuration.ConfigurationManager.AppSettings["LogPath"];
            StreamWriter sw;
            if (!File.Exists(path))
            { sw = File.CreateText(path); }
            else
            { sw = File.AppendText(path); }

            LogWrite(strValue, sw);

            sw.Flush();
            sw.Close();
        }
        private static void LogWrite(string logMessage, StreamWriter w)
        {
            w.WriteLine("{0}{1}", DateTime.Now.ToString(), logMessage);
            w.WriteLine("----------------------------------------");
        }

    }
}
