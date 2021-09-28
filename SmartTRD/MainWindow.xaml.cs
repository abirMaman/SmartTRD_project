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

                m_bClientP.connectToIbClientTWS(g_ip_tb.Text, port, 1);

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
                DateTime dt = new DateTime();
                try
                {
                    dt = g_bidAskDateLst_dpc.SelectedDate.Value;
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Please insert last trade date and try again", "Error Date", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                g_bisAskAlgoStAnz_bt.Content = "STOP ANALYZE";

                m_bidAskAlgoP.StartAskBidAlgoOnline(g_bidAskAlgoSymName_cmb.Text, dt.ToString("yyyyMMdd"), excMax);
            }
        }
    }
}
