using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTRD.LogHandle
{
    abstract class LogHandler
    {
        private static Mutex m_mutex = new Mutex();

        private static StreamWriter m_fdP;

        public static void Init() 
        {
            string name = "Logs\\" +"LogSmarTrd_" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".txt";
            if (Directory.Exists("Logs") == false)
                Directory.CreateDirectory("Logs");

            m_fdP = new StreamWriter(name); 
        }

        public static void WriteToFile(string msg_A)
        {
            m_mutex.WaitOne();

            if (m_fdP != null)
            {
                m_fdP.WriteLine(msg_A);
            }

            m_mutex.ReleaseMutex();
        }
        public static void CloseFile()
        {
            if (m_fdP != null)
            {
              DateTime timeAndData = DateTime.Now;
              string msg = "Date :" + timeAndData.ToShortDateString() + ", Time: " + timeAndData.ToShortTimeString() + ", Msg:" + "File:" + " was closed by remote" + Environment.NewLine;
              m_fdP.Write(msg);
              m_fdP.Close();
            }
        }
    }
}
