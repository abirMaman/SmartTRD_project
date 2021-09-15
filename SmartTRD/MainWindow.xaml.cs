using SmartTRD.DB;
using SmartTRD.IBclient;
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

namespace SmartTRD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BclientCon m_bClientP;
        private ScannerMng m_scnMngP;
        private EWrapperImpl m_testImpl;
        private StockScannerDB m_stkDbP;

        public MainWindow()
        {
            InitializeComponent();

            m_bClientP = null;
            m_scnMngP = null;
            m_stkDbP = null;
            m_testImpl = null;

            CreatePackage();
            InitAll();

            //string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("FZMD");
            string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("PBYA");

            JsonAnalyzer.JsonStkInfo js = new JsonAnalyzer.JsonStkInfo(ret);

            js.StkIsPink();
            js.StkAsTransferAgent();
            js.StkisOTCQC();
            string date = "";
            js.StkAsVerfiedProfile(out date);
            //m_scnMngP.StartScanStkProcess();
        }

        public void CreatePackage()
        {

            m_testImpl = new EWrapperImpl();
            m_bClientP = new BclientCon();
            m_stkDbP = new StockScannerDB();
            m_scnMngP = new ScannerMng();
        }

        public void InitAll()
        {
            m_testImpl.Init();
            m_bClientP.Init(m_testImpl);
            m_bClientP.connectToIbClientTWS("127.0.0.1", 7497, 1);      
            m_scnMngP.Init();
            m_stkDbP.Init();
        }
    }
}
