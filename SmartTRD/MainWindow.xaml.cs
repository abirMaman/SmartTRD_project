using SmartTRD.BidAsk_Algo;
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
        public enum MAIN_ACTIVE_ACTION_e
        {
            MAIN_ACTIVE_ACTION_NONE = 0,
            MAIN_ACTIVE_ACTION_SCANNER = 1,
            MAIN_ACTIVE_ACTION_BID_ASK_ALGO = 2
        }
        public static MAIN_ACTIVE_ACTION_e m_actAction;

        private BclientCon m_bClientP;
        private ScannerMng m_scnMngP;
        private EWrapperImpl m_testImpl;
        private StockScannerDB m_stkDbP;
        private BidAskAlgoDB m_bidAskAlgoDbP;
        private BidAskAlgo m_bidAskAlgoP;

        public MainWindow()
        {
            InitializeComponent();

            m_actAction = MAIN_ACTIVE_ACTION_e.MAIN_ACTIVE_ACTION_NONE;
            m_bClientP = null;
            m_scnMngP = null;
            m_stkDbP = null;
            m_testImpl = null;

            CreatePackage();
            InitAll();

            //string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("FZMD");
            //string ret = Http.HttpOtcMarket.GetStrFromOtcMarket("NNRX");

            //JsonAnalyzer.JsonStkInfo js = new JsonAnalyzer.JsonStkInfo(ret);

            //js.StkIsPink();
            //  js.StkAsTransferAgent();
            // js.StkisOTCQC();
            //string date = "";
            //js.StkAsVerfiedProfile(out date);
            //m_scnMngP.StartScanStkProcess();

            m_actAction = MAIN_ACTIVE_ACTION_e.MAIN_ACTIVE_ACTION_BID_ASK_ALGO;
            m_bidAskAlgoP.StartAskBidAlgo("PBYA", "20210923");
        }

        public void CreatePackage()
        {

            m_testImpl = new EWrapperImpl();
            m_bClientP = new BclientCon();
            m_stkDbP = new StockScannerDB();
            m_scnMngP = new ScannerMng();
            m_bidAskAlgoP = new BidAskAlgo();
            m_bidAskAlgoDbP = new BidAskAlgoDB();
            
        }

        public void InitAll()
        {
            m_testImpl.Init();
            m_bClientP.Init(m_testImpl);
            m_bClientP.connectToIbClientTWS("127.0.0.1", 7497, 1);      
            m_scnMngP.Init();
            m_stkDbP.Init();
            m_bidAskAlgoP.Init();
            m_bidAskAlgoDbP.Init();
        }
    }
}
