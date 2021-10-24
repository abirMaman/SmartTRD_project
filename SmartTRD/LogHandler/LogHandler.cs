using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SmartTRD.LogHandle
{
    abstract class LogHandler
    {
        private static Mutex m_mutex = new Mutex();
        private static Mutex m_mutex2 = new Mutex();
        private static MainWindow m_mainWindowP;
        private static StreamWriter m_fdP;
        private static List<string> m_logs;
        private static List<string> m_fileLogs;
        private static bool m_closed;

        public static void Init(int CID_A = 1) 
        {
            m_logs = new List<string>();
            m_fileLogs = new List<string>();
            m_closed = false;
            string name = "Logs\\" +"LogSmarTrd_CIDN_" + CID_A.ToString() + "_" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".txt";
            if (Directory.Exists("Logs") == false)
                Directory.CreateDirectory("Logs");

            m_fdP = new StreamWriter(name);
            m_mainWindowP = MainWindow.GetInstanse();

            Thread rtbT = new Thread(RitcTextBoxThr);
            rtbT.Start();

            Thread fileT = new Thread(WriteToFileT);
            fileT.Start();
        }

        public static void WriteToFile(string msg_A)
        {
                m_mutex.WaitOne();

                if (m_fdP != null)
                { 
                    m_fileLogs.Add(DateTime.Now.ToString("HH:mm:ss") + " - " + msg_A);

                    // m_mainWindowP.g_logBox_rtb.AppendText(DateTime.Now.ToString("HH:mm:ss") + " - " + msg_A + Environment.NewLine);
                    // m_mainWindowP.g_logBox_rtb.ScrollToEnd();
                }

                m_mutex.ReleaseMutex();

                m_mutex2.WaitOne();
                m_logs.Add(DateTime.Now.ToString("HH:mm:ss") + " - " + msg_A + Environment.NewLine);
                m_mutex2.ReleaseMutex();
        }
        public static void WriteToFile(string msg_A,params object [] args_A)
        {
                m_mutex.WaitOne();

                if (m_fdP != null)
                {
                    string formatFile = String.Format(DateTime.Now.ToString("HH:mm:ss") + " - " + msg_A, args_A);
                    m_fileLogs.Add(formatFile);
                }

                m_mutex.ReleaseMutex();

                m_mutex2.WaitOne();
                string format = String.Format(DateTime.Now.ToString("HH:mm:ss") + " - " + msg_A, args_A);
                m_logs.Add(format);
                m_mutex2.ReleaseMutex();
        }
        public static void CloseFile()
        {
            m_closed = true;

            if (m_fdP != null)
            {
              DateTime timeAndData = DateTime.Now;
              string msg = "Date :" + timeAndData.ToShortDateString() + ", Time: " + timeAndData.ToShortTimeString() + ", Msg:" + "File:" + " was closed by remote" + Environment.NewLine;
              m_fdP.Write(msg);
              m_fdP.Close();
            }
        }

        private static void WriteToFileT()
        {

            while (m_closed == false)
            {
                bool clear = false;
                m_mutex.WaitOne();
                try
                {
                    if(m_fileLogs.Count !=0)
                    {
                        clear = true;
                        foreach (string line in m_fileLogs)
                        {
                            m_fdP.WriteLine(line);
                        }
                    }
                }
                catch(Exception e2) { }


                if (clear)
                {
                    m_fileLogs.Clear();           
                }

                m_mutex.ReleaseMutex();

                if(clear)
                    Thread.Sleep(1500);

            }
        }

        private static void RitcTextBoxThr()
        {
            
            while(m_closed == false)
            {
                bool update = false;
                if(m_logs.Count !=0 )
                {                
                    m_mutex2.WaitOne();
                    update = true;
                    string[] tempList = new string[m_logs.Count];
                    m_logs.CopyTo(tempList);
                    m_logs.Clear();
                    m_mutex2.ReleaseMutex();

                    foreach (string log in tempList)
                    {
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            m_mainWindowP.g_logBox_lg.Items.Add(log);
                      
                        });
                        //Thread.Sleep(5);
                    }
                }
                if (update)
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        m_mainWindowP.g_logBox_lg.Items.MoveCurrentToLast();
                        m_mainWindowP.g_logBox_lg.ScrollIntoView(m_mainWindowP.g_logBox_lg.Items.CurrentItem);
                    });

                    //Thread.Sleep(20);
                }
                else
                {
                    //Thread.Sleep(100);
                }

            }
        }
    }
}
