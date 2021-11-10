using SmartTRD.BidAsk_Algo;
using SmartTRD.DB;
using SmartTRD.IBclient;
using SmartTRD.ReqId;
using SmartTRD.Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Net;
using System.Globalization;
using SmartTRD.LogHandle;
using System.Threading;

namespace SmartTRD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
 

        private BclientCon m_bClientP;
        private ScannerMng m_scnMngP;
        private EWrapperImpl m_testImpl;
        private StockScannerDB m_stkDbP;
        private BidAskAlgoDB m_bidAskAlgoDbP;
        private BidAskAlgo m_bidAskAlgoP;
        private ReqIdMng m_reqIdMngP;
        private static MainWindow m_instanse;

        public MainWindow()
        {
            InitializeComponent();

            m_bClientP = null;
            m_scnMngP = null;
            m_stkDbP = null;
            m_testImpl = null;
            m_reqIdMngP = null;
            m_instanse = this;

            

            CreatePackage();
            InitAll();
            ReadDataFromXmlAndLoadToGUI();

            //string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("FZMD");
            //string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("NNRX");

            //JsonAnalyzer.JsonStkInfo js = new JsonAnalyzer.JsonStkInfo(ret);

            //js.StkIsPink();
            //  js.StkAsTransferAgent();
            // js.StkisOTCQC();
            //string date = "";
            //js.StkAsVerfiedProfile(out date);
            //m_scnMngP.StartScanStkProcess();




        }

        public static MainWindow GetInstanse()
        {
            return m_instanse;
        }

        public void CreatePackage()
        {

            m_testImpl = new EWrapperImpl();
            m_bClientP = new BclientCon();
            m_stkDbP = new StockScannerDB();
            m_scnMngP = new ScannerMng();
            m_bidAskAlgoP = new BidAskAlgo();
            m_bidAskAlgoDbP = new BidAskAlgoDB();
            m_reqIdMngP = new ReqIdMng();
                 
        }

        public void InitAll()
        {
            LogHandler.Init();
            m_testImpl.Init();
            m_bClientP.Init(m_testImpl);          
            m_scnMngP.Init();
            m_stkDbP.Init();
            m_bidAskAlgoP.Init();
            m_bidAskAlgoDbP.Init();
            m_reqIdMngP.Init();
        }

        public void ReadDataFromXmlAndLoadToGUI()
        {
            // === AskBid combo box - symbols ===
            XML.XmlLoad<List<string>> conLoad = new XML.XmlLoad<List<string>>();
            try
            {
                List<string> cntFromXml = conLoad.loadData("symbolCmnBidAskAlgo.xml");

                foreach(string sym in cntFromXml)
                {
                    g_bidAskAlgoSymName_cmb.Items.Add(sym);
                }
            }
            catch (Exception) { }
            // === AskBid combo box - symbols ===
        }

        private void g_connect_bt_Click(object sender, RoutedEventArgs e)
        {
            if (g_connect_bt.Content.ToString() == "Disconnect")
            {
                m_bClientP.DisconnectFromClientTws();
                g_mainTab_tbc.IsEnabled = false;
                g_connect_bt.Content = "Connect";
                g_conStatus_br.Background = Brushes.Red;

            }
            else
            {
                IPAddress ip;
                int port;
                if (IPAddress.TryParse(g_ip_tb.Text, out ip) == false)
                {
                    MessageBox.Show("Please insert legall ip and try again", "Error Ip", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.TryParse(g_port_tb.Text, out port) == false)
                {
                    MessageBox.Show("Please insert legall port and try again", "Error Port", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int cid = 1;
                int.TryParse(g_clientId_tb.Text, out cid);

                m_bClientP.connectToIbClientTWS(g_ip_tb.Text, port, cid);

                if (m_bClientP.TwsIsConnectedToApp())
                {
                    g_mainTab_tbc.IsEnabled = true;
                    g_connect_bt.Content = "Disconnect";
                    g_conStatus_br.Background = Brushes.LightGreen;
                }

            }        
        }

        private void g_bisAskAlgoStAnz_bt_Click(object sender, RoutedEventArgs e)
        {
            if (g_bisAskAlgoStAnz_bt.Content.ToString() == "STOP ANALYZE")
            {
                m_bidAskAlgoP.StopBidAskAlgo();
            }
            else
            {
                if (g_bidAskAlgoSymName_cmb.Text == "")
                {
                    MessageBox.Show("Please insert symbol name and try again", "Error Symbol", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                int excMax;
                CultureInfo cul = new CultureInfo("en-US");
                if (int.TryParse(g_bidAskAlgoMaxExc_tb.Text, System.Globalization.NumberStyles.Any, cul, out excMax) == false)
                {
                    MessageBox.Show("Please insert legall number,for example:500,000", "Error Execption", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            
                int refRate = 0;
                if( int.TryParse(g_bidAskRefRate_tb.Text,out refRate) == false)
                {
                    refRate = 10;
                }
                if (g_bidAskUseRTH_chb.IsChecked == true)
                {
                    DateTime currTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                     DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);


                    DateTime strTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    15, 00,00);
                    DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    23, 05, 00);

                    if (currTime >= strTime && currTime < endTime)
                    {
                        m_bidAskAlgoP.StartAskBidAlgoOnline(g_bidAskAlgoSymName_cmb.Text, excMax, refRate);
                    }
                    else
                    {
                        MessageBox.Show("Please wait to 15:00:00 for opening day trade time..", "Error Time", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
           
                }
                else
                {
                    //DateTime firTrdDay = new DateTime();
                    //try
                    //{
                    //    firTrdDay = g_bidAskDateFirst_dpc.SelectedDate.Value;
                    //}
                    //catch (Exception e1)
                    //{
                    //    MessageBox.Show("Please insert trade date and try again", "Error Date", MessageBoxButton.OK, MessageBoxImage.Error);
                    //    return;
                    //}

                    DateTime TrdDay = new DateTime();
                    try
                    {
                        TrdDay = g_bidAskDateFirst_dpc.SelectedDate.Value;
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show("Please insert trade date and try again", "Error Date", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string[] time = g_bidAskTimeToClose_tb.Text.Split(':');
                    int hour, min, sec;
                    if (int.TryParse(time[0], out hour) == false ||
                        int.TryParse(time[0], out min) == false ||
                        int.TryParse(time[0], out sec) == false ||
                            time.Length != 3)
                        {
                        MessageBox.Show("Please inset valid time , for example : 23:00:00...", "Error Time", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                 
                    m_bidAskAlgoP.StartAskBidAlgoOffline(g_bidAskAlgoSymName_cmb.Text, TrdDay.ToString("yyyyMMdd"), g_bidAskTimeToClose_tb.Text, excMax, refRate);
                }

                g_bisAskAlgoStAnz_bt.Content = "STOP ANALYZE";
            }
        }

        private void g_bidAskUseRTH_chb_Click(object sender, RoutedEventArgs e)
        {
            if (g_bidAskUseRTH_chb.IsChecked == true)
            {
                g_bidAskTimeToClose_tb.IsEnabled = false;
                g_bidAskDateFirst_dpc.IsEnabled = false;
            }
            else
            {
                g_bidAskTimeToClose_tb.IsEnabled = true;
                g_bidAskDateFirst_dpc.IsEnabled = true;
            }
        }


        private void MetroWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure??", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                m_bClientP.DisconnectFromClientTws();
                m_bidAskAlgoP.StopBidAskAlgo();
                LogHandler.CloseFile();
                Thread.Sleep(1000);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
