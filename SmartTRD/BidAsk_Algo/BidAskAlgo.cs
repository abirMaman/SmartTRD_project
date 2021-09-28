using IBApi;
using SmartTRD.DB;
using SmartTRD.IBclient;
using SmartTRD.ReqId;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartTRD.BidAsk_Algo
{
    class BidAskAlgo : iBidAskAlgo
    {

        //struct BID_ASK_REQ_ID_s
        //{
        //    public int bid_req;
        //    public int ask_req;
        //}
        struct ASK_BID_VOLS_INFO_s
        {
            public double prevPrice;
            public double currPrice;
            public long askSize;
            public long bidSize;
            public int tradeVol;
            public int dollarVol;
            public int countMaxTrdStkBid;
            public long countSizeOfTrdStkBid;
            public int countMaxTrdStkAsk;
            public long countSizeOfTrdStkAsk;
        }

        struct REQ_INFO_s
        {
            public int actReqId;
            public bool reqReceiveAnw;
        }

        struct HISTORY_VOL_REQ_INFO_s
        {
            public bool wakeUpFromTimeOut;
            public REQ_INFO_s[] reqInfoArr;

        }

        private static BidAskAlgo m_instase;
        private ASK_BID_VOLS_INFO_s m_bidAskVolRes;
        private HISTORY_VOL_REQ_INFO_s m_hisInfo;
        private iBidAskAlgoDB m_bidAskDb;
        private iBclient m_clientP;
        private iReqIdMng m_reqIdMngP;
        private MainWindow m_mainWindowP;
        private string m_symbol;
        private string m_lstTrdData;
        private int m_maxTrdSizeForMark_A;
        private bool m_stopAnalyze;
        private DateTime m_firstTime;

        public BidAskAlgo()
        {
            m_clientP = null;
            m_bidAskDb = null;
            m_reqIdMngP = null;
            m_mainWindowP = null;
            m_symbol = "";
            m_maxTrdSizeForMark_A = 0;
            m_instase = this;
        }

        public static BidAskAlgo GetInstanse()
        {
            return m_instase;
        }

        public void Init()
        {
            m_bidAskDb = BidAskAlgoDB.GetInstanse();
            m_clientP = BclientCon.GetInstase();
            m_reqIdMngP = ReqIdMng.GetInstanse();
            m_mainWindowP = MainWindow.GetInstanse();
        }
        public void StartAskBidAlgoOnline(string symbol_A, string lstTrdDate_A, int maxTrdSizeForMark_A)
        {
            m_symbol = symbol_A;
            m_maxTrdSizeForMark_A = maxTrdSizeForMark_A;
            m_lstTrdData = lstTrdDate_A;
            m_stopAnalyze = false;
            m_bidAskVolRes = new ASK_BID_VOLS_INFO_s();
            m_bidAskDb.StartNewSession();

            Thread stAlgoT = new Thread(StartAlgoThread);
            stAlgoT.Start();
        }

        public void StopBidAskAlgo()
        {
            m_stopAnalyze = true;
        }

        private void StartAlgoThread()
        {
            if (StepGetSymbol() == false)//Step 1
                return;

            if (StepClosePriceFirst() == false)//Step 2
                return;

            StartReceiveHisAndAnalyze();
        }


        private bool StepGetSymbol()
        {
            //===== Step 1 get contract details ======
            int countSleep = 0;
            bool symbolReceived = false;
            m_reqIdMngP.InsertReqToDic(m_clientP.GetNextReqId(), ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
            m_clientP.GetSymbolDetails(m_symbol);

            while (symbolReceived == false && countSleep < 5000)
            {
                symbolReceived = m_bidAskDb.ContractAsReceived();

                if (symbolReceived == false)
                {
                    Thread.Sleep(50);
                    countSleep += 50;
                }
            }
            if (symbolReceived == false)
            {
                MessageBox.Show(m_symbol + " not found , please try again", "Error Symbol", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                InsertContractToXmlIfNeeded(m_symbol);

            }
            //===== Step 1 get contract details ======

            return symbolReceived;
        }

        private void InsertContractToXmlIfNeeded(string sybmol_A)
        {
            List<string> cntFromXml = null;
            XML.XmlLoad<List<string>> conLoad = new XML.XmlLoad<List<string>>();
            try
            {
                cntFromXml = conLoad.loadData("symbolCmnBidAskAlgo.xml");
                    
            }
            catch (Exception) { }

            if (cntFromXml == null)
                cntFromXml = new List<string>();

            if (cntFromXml.Contains(sybmol_A) == false)
            {
                cntFromXml.Add(sybmol_A);

                XML.XmlHandler.SaveData(cntFromXml, "symbolCmnBidAskAlgo.xml");

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    m_mainWindowP.g_bidAskAlgoSymName_cmb.Items.Add(sybmol_A);
                });
            }
        }
        private bool StepClosePriceFirst()
        {
            bool closePriceAsReceived = false;
            int countSleep = 0;
            m_lstTrdData += " " + "23:00:00";//Get last update from requested date

            int reqId = m_clientP.GetNextReqId();
            m_reqIdMngP.InsertReqToDic(reqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
            m_clientP.GetHistorySymbolData(m_bidAskDb.GetCurrContract(), "1 D", "1 day", "TRADES", m_lstTrdData);//Save for algo!!!!!

            Thread.Sleep(5000);

            while (closePriceAsReceived == false && countSleep < 5000)
            {
                closePriceAsReceived = m_bidAskDb.ClosePriceAsReceived();

                if (closePriceAsReceived == false)
                {
                    Thread.Sleep(50);
                    countSleep += 50;
                }
            }
            if (closePriceAsReceived == false)
            {
                MessageBox.Show(m_symbol + " not recevied closing price of last trade day", "Error Close Price", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                m_bidAskVolRes.currPrice = m_bidAskDb.GetClosePrice();
                //m_bidAskVolRes.askPrice = m_bidAskDb.GetFirstAsk();
                //m_bidAskVolRes.bidPrice = m_bidAskDb.GetFirstBid();
                Console.WriteLine("Close Price = " + m_bidAskVolRes.currPrice);
                //Console.WriteLine("first bid price by date = " + m_lstTrdData + " is : " + m_bidAskVolRes.bidPrice);
            }

            m_reqIdMngP.RemoveActionFromDic(reqId);

            return closePriceAsReceived;
        }

        private void StartReceiveHisAndAnalyze()
        {

            m_firstTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 00);
            DateTime endOfMarket = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 00, 00);
            object currTime = null;

            do
            {
                currTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                       DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                //currTime = new DateTime(2021, 09, 27,
                //       23, 00, 00);

                double diff = ((DateTime)currTime).Subtract(m_firstTime).TotalSeconds;

                if (diff >= 10)
                {
                    m_hisInfo = new HISTORY_VOL_REQ_INFO_s();
                    m_hisInfo.reqInfoArr = new REQ_INFO_s[2];

                    m_hisInfo.reqInfoArr[0].actReqId = m_clientP.GetNextReqId();//Bid Ask
                    m_reqIdMngP.InsertReqToDic(m_hisInfo.reqInfoArr[0].actReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    Console.WriteLine("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time curr = " + ((DateTime)currTime).ToString("yyyyMMdd HH:mm:ss")));
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), m_firstTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "TRADES");

                    m_hisInfo.reqInfoArr[1].actReqId = m_clientP.GetNextReqId();//Vol
                    m_reqIdMngP.InsertReqToDic(m_hisInfo.reqInfoArr[1].actReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_clientP.GetMktData(1, m_bidAskDb.GetCurrContract());

                    Thread anlThr = new Thread(() => WaitAndGetHistoryData(m_firstTime));
                    anlThr.Start();

                    Thread.Sleep(9990);
                    m_hisInfo.wakeUpFromTimeOut = true;
                    m_firstTime = (DateTime)currTime;

                }
                else
                {
                    Thread.Sleep(5);
                }

            }
            while (DateTime.Now < endOfMarket && m_stopAnalyze == false);

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                   m_mainWindowP.g_bisAskAlgoStAnz_bt.Content = "START ANALYZE";
            });

            MessageBox.Show("Bid Ask Algorithm end!!!!","Info",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        private void WaitAndGetHistoryData(DateTime firstTime_A)
        {
            Thread.Sleep(3000);//Wait for update
            bool allDataReceivd = false;
            bool stkisRaised = false;

            while (allDataReceivd == false &&
                m_hisInfo.wakeUpFromTimeOut == false)
            {
                m_hisInfo.reqInfoArr[0].reqReceiveAnw = m_bidAskDb.HistoryDataAsReceived();
                m_hisInfo.reqInfoArr[1].reqReceiveAnw = m_bidAskDb.VolAsReceivd();

                allDataReceivd = m_hisInfo.reqInfoArr[0].reqReceiveAnw && m_hisInfo.reqInfoArr[1].reqReceiveAnw;

                if (m_hisInfo.reqInfoArr[0].reqReceiveAnw)
                {
                    List<HistoricalTickLast> hisDataList = m_bidAskDb.GetHistoryData();
                    foreach (HistoricalTickLast his in hisDataList)
                    {
                        string[] date = Util.UnixSecondsToString(his.Time, "yyyyMMdd-HH:mm:ss zzz").Split();

                        DateTime trdTime = DateTime.ParseExact(date[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);
                        if (trdTime >= firstTime_A)
                        {
                            if (m_bidAskVolRes.currPrice > his.Price ||
                                (his.Price == m_bidAskVolRes.currPrice && stkisRaised == false))//Sum Ask
                            {
                                stkisRaised = false;
                                m_bidAskVolRes.currPrice = his.Price;
                                m_bidAskVolRes.askSize += his.Size;

                                if (his.Size >= m_maxTrdSizeForMark_A)
                                {
                                    m_bidAskVolRes.countMaxTrdStkAsk++;
                                    m_bidAskVolRes.countSizeOfTrdStkAsk += his.Size;
                                }

                            }
                            else
                            {
                                stkisRaised = true;
                                m_bidAskVolRes.currPrice = his.Price;
                                m_bidAskVolRes.bidSize += his.Size;

                                if (his.Size >= m_maxTrdSizeForMark_A)
                                {
                                    m_bidAskVolRes.countMaxTrdStkBid++;
                                    m_bidAskVolRes.countSizeOfTrdStkBid += his.Size;
                                }
                            }

                            if (his.Price != m_bidAskVolRes.currPrice)
                                m_bidAskVolRes.prevPrice = m_bidAskVolRes.currPrice;

                        }
                    }
                }
                if (m_hisInfo.reqInfoArr[1].reqReceiveAnw)
                {
                    m_bidAskVolRes.tradeVol = m_bidAskDb.GetCurrVol();
                }


            }
            Console.WriteLine("prev price = " + m_bidAskVolRes.prevPrice + " , curr price = " + m_bidAskVolRes.currPrice);
            Console.WriteLine("ask size = " + m_bidAskVolRes.askSize + "Exc ask cnt = " + m_bidAskVolRes.countMaxTrdStkAsk + " Exc ask size = " + m_bidAskVolRes.countSizeOfTrdStkAsk);
            Console.WriteLine("bid size = " + m_bidAskVolRes.bidSize + "Exc bid cnt = " + m_bidAskVolRes.countMaxTrdStkBid + " Exc bid size = " + m_bidAskVolRes.countSizeOfTrdStkBid);
            Console.WriteLine("curr vol = " + m_bidAskVolRes.tradeVol);

            if (allDataReceivd)
            {
                HandleWithGUI();
                //Update GUI
            }

            m_reqIdMngP.RemoveActionFromDic(m_hisInfo.reqInfoArr[0].actReqId);
            m_reqIdMngP.RemoveActionFromDic(m_hisInfo.reqInfoArr[1].actReqId);
        }
        private void HandleWithGUI()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                CultureInfo cul = new CultureInfo("en-US");
                m_mainWindowP.g_bidAskAlgoCurrPrice_tb.Text = m_bidAskVolRes.currPrice.ToString();
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Text = m_bidAskVolRes.askSize.ToString("N",cul).Replace(".00", "");
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Text = m_bidAskVolRes.bidSize.ToString("N", cul).Replace(".00","");
                m_mainWindowP.g_bidAskAlgoVolTrade_tb.Text = m_bidAskVolRes.tradeVol.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_bidAskAlgoVolDol_tb.Text = (m_bidAskVolRes.currPrice * m_bidAskVolRes.tradeVol).ToString("N", cul) + "$";
                m_mainWindowP.g_bidCntMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countMaxTrdStkBid.ToString();
                m_mainWindowP.g_askCntMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countMaxTrdStkAsk.ToString();
                m_mainWindowP.g_bidMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countSizeOfTrdStkBid.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_askMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countSizeOfTrdStkAsk.ToString("N", cul).Replace(".00", "");


                //Color GUI
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                if (m_bidAskVolRes.askSize > m_bidAskVolRes.bidSize)
                {
                    m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.Red;
                    m_mainWindowP.g_askSizeBidAskAlgo_tb.Foreground = Brushes.White;
                }
                else if(m_bidAskVolRes.askSize < m_bidAskVolRes.bidSize)
                {
                    m_mainWindowP.g_bidSizeBidAskAlgo_tb.Background = Brushes.LightGreen;
                }
    
            });
        }
    }
    }

