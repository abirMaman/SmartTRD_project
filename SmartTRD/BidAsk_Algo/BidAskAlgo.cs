using IBApi;
using SmartTRD.DB;
using SmartTRD.IBclient;
using SmartTRD.LogHandle;
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
            public double openPrice;
            public double currPrice;
            public double curr_ask;
            public double curr_bid;
            public long askSize;
            public long bidSize;
            public int tradeVol;
            public long unreported;
            public int countMaxTrdStkBid;
            public long countSizeOfTrdStkBid;
            public int countMaxTrdStkAsk;
            public long countSizeOfTrdStkAsk;
        }


        struct HISTORY_VOL_REQ_INFO_s
        {
            public bool wakeUpFromTimeOut;
        }

        private static int m_diffBetweenREQ = 20;//[SEC]
        private static BidAskAlgo m_instase;
        private ASK_BID_VOLS_INFO_s m_bidAskVolRes;
        private HISTORY_VOL_REQ_INFO_s m_hisInfo;
        private iBidAskAlgoDB m_bidAskDb;
        private iBclient m_clientP;
        private iReqIdMng m_reqIdMngP;
        private MainWindow m_mainWindowP;
        private string m_symbol;
        private string m_lstTrdData;
        private string m_firstTrdData;//For offline
        private int m_maxTrdSizeForMark;
        private bool m_stopAnalyze;
        private DateTime m_firstTime;
        private DateTime m_bidAskLstTime;
        private bool m_firstTimeWasSet;
        private bool m_bidAskTimeWasSet;
        private bool m_stkisRaised;
        private Thread m_stTrdP;

        public BidAskAlgo()
        {
            m_clientP = null;
            m_bidAskDb = null;
            m_reqIdMngP = null;
            m_mainWindowP = null;
            m_stkisRaised = false;
            m_firstTimeWasSet = false;
            m_bidAskTimeWasSet = false;
            m_symbol = "";
            m_lstTrdData = "";
            m_firstTrdData = "";
            m_maxTrdSizeForMark = 0;
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

        private void PrepareToNewRound()
        {
        
            m_stopAnalyze = false;
            m_stkisRaised = false;
            m_firstTimeWasSet = false;
            m_bidAskTimeWasSet = false;
            m_bidAskVolRes = new ASK_BID_VOLS_INFO_s();
            m_bidAskDb.StartNewSession();
            InitGUI();
        }
        public void StartAskBidAlgoOnline(string symbol_A, string lstTrdDate_A, int maxTrdSizeForMark_A,int reRate_A)
        {
            PrepareToNewRound();

            m_symbol = symbol_A;
            m_maxTrdSizeForMark = maxTrdSizeForMark_A;
            m_lstTrdData = lstTrdDate_A;
            m_diffBetweenREQ = reRate_A;

            m_stTrdP = new Thread(StartAlgoThread);
            m_stTrdP.Start();
        }
        public void StartAskBidAlgoOffline(string symbol_A, string firTrdDate_A,string lstTrdDate_A, int maxTrdSizeForMark_A, int reRate_A)
        {
            PrepareToNewRound();

            m_symbol = symbol_A;
            m_maxTrdSizeForMark = maxTrdSizeForMark_A;
            m_lstTrdData = lstTrdDate_A;
            m_firstTrdData = firTrdDate_A;
            m_diffBetweenREQ = reRate_A;

            m_stTrdP = new Thread(StartAlgoThreadOffline);
            m_stTrdP.Start();
        }

        public void StopBidAskAlgo()
        {
            m_stopAnalyze = true;
            //m_stTrdP.Abort();
        }

        private void StartAlgoThread()
        {
            if (StepGetSymbol() == false)//Step 1
                return;

            if (StepClosePriceFirst() == false)//Step 2
                return;

            StartReceiveHisAndAnalyze();
        }
        private void StartAlgoThreadOffline()
        {
            if (StepGetSymbol() == false)//Step 1
                return;

            if (StepClosePriceFirst() == false)//Step 2
                return;

            StartReceiveHisAndAnalyzeOffline();
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
                m_bidAskVolRes.openPrice = m_bidAskVolRes.currPrice;
                Console.WriteLine("Close Price = " + m_bidAskVolRes.currPrice);
            }

            m_reqIdMngP.RemoveActionFromDic(reqId);

            return closePriceAsReceived;
        }

        private bool StepBidAskPriceFirst()
        {
            bool bidAskPriceAsReceived = false;
            int countSleep = 0;
            DateTime bidAskStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 30, 00).AddDays(-2);//33333

            int reqIdBidAsk = m_clientP.GetNextReqId();
            m_reqIdMngP.InsertReqToDic(reqIdBidAsk, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO, "BID");
            m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), bidAskStart.ToString("yyyyMMdd HH:mm:ss"), "", 1, "BID_ASK");//Save for algo!!!!!

            Thread.Sleep(5000);

            while (bidAskPriceAsReceived == false && countSleep < 5000)
            {
                bidAskPriceAsReceived = m_bidAskDb.BidAsRecevied() && m_bidAskDb.AskAsRecevied();

                if (bidAskPriceAsReceived == false)
                {
                    Thread.Sleep(50);
                    countSleep += 50;
                }
            }
            if (bidAskPriceAsReceived == false)
            {
                MessageBox.Show(m_symbol + " not recevied bid ask price of last trade day", "Error Close Price", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                m_bidAskVolRes.curr_bid = m_bidAskDb.GetBidPrice();
                m_bidAskVolRes.curr_ask = m_bidAskDb.GetAskPrice();
                Console.WriteLine("first bid Price = " + m_bidAskVolRes.curr_bid);
                Console.WriteLine("first ask Price = " + m_bidAskVolRes.curr_ask);
            }

            m_reqIdMngP.RemoveActionFromDic(reqIdBidAsk);

            return bidAskPriceAsReceived;
        }
        private void StartReceiveHisAndAnalyzeOffline()
        {
            int trdReqId = 0, volReqId = 0, bidAskReqId = 0;
            string startInDate = m_firstTrdData + " " + "16:30:00";
            string endInDate =  m_firstTrdData + " " + "23:00:15";
            m_firstTime = DateTime.ParseExact(startInDate, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endOfMarket = DateTime.ParseExact(endInDate, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            int countTradesByCycle = 0;
            m_bidAskLstTime = m_firstTime;

            do
            {
                    m_bidAskTimeWasSet = false;
                    m_firstTimeWasSet = false;

                    Console.WriteLine("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time end = " + (endOfMarket).ToString("yyyyMMdd HH:mm:ss")));
                    LogHandler.WriteToFile("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time end = " + (endOfMarket).ToString("yyyyMMdd HH:mm:ss")));
                    m_hisInfo.wakeUpFromTimeOut = false;

                    bidAskReqId = m_clientP.GetNextReqId();//Bid Ask
                    m_reqIdMngP.InsertReqToDic(bidAskReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), m_bidAskLstTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "BID_ASK");


                    trdReqId = m_clientP.GetNextReqId();//Bid Ask
                    m_reqIdMngP.InsertReqToDic(trdReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_bidAskDb.SetNewReqAndPrepare(trdReqId);
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), m_firstTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "TRADES");
                    

                    Thread anlThr = new Thread(() => countTradesByCycle = WaitAndGetHistoryData(trdReqId, volReqId, bidAskReqId,true));
                    anlThr.Start();

                     SleepAndWaitT(m_diffBetweenREQ * 1000);

                    m_hisInfo.wakeUpFromTimeOut = true;

                    anlThr.Join();

                    if (m_bidAskTimeWasSet == false)
                    {
                        m_bidAskLstTime = m_firstTime.AddSeconds(-10);
                    }

                if (countTradesByCycle == 0)
                    break;


            }
            while (m_firstTime < endOfMarket && m_stopAnalyze == false && m_clientP.isConnected());

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                m_mainWindowP.g_bisAskAlgoStAnz_bt.Content = "START ANALYZE";
            });

            m_hisInfo.wakeUpFromTimeOut = true;

            MessageBox.Show("Bid Ask Algorithm end!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartReceiveHisAndAnalyze()
        {
            int trdReqId = 0, volReqId = 0, bidAskReqId = 0;
            m_firstTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 30, 00);
            DateTime endOfMarket = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 00, 15);
            m_bidAskLstTime = m_firstTime;
            object currTime = null;

            do
            {
                currTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                      DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                //currTime = new DateTime(2021, 10, 01,
                //       23, 00, 00);

                double diff = ((DateTime)currTime).Subtract(m_firstTime).TotalSeconds;

                if (diff >= m_diffBetweenREQ)
                {
                    m_bidAskTimeWasSet = false;
                    m_firstTimeWasSet = false;

                    Console.WriteLine("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time curr = " + ((DateTime)currTime).ToString("yyyyMMdd HH:mm:ss")));
                    LogHandler.WriteToFile("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time curr = " + ((DateTime)currTime).ToString("yyyyMMdd HH:mm:ss")));
                    m_hisInfo.wakeUpFromTimeOut = false;

                    bidAskReqId = m_clientP.GetNextReqId();//Bid Ask
                    m_reqIdMngP.InsertReqToDic(bidAskReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), m_bidAskLstTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "BID_ASK");


                    trdReqId = m_clientP.GetNextReqId();//Bid Ask
                    m_reqIdMngP.InsertReqToDic(trdReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_bidAskDb.SetNewReqAndPrepare(trdReqId);
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), m_firstTime.ToString("yyyyMMdd HH:mm:ss"), "", 1000, "TRADES");

                    volReqId = m_clientP.GetNextReqId();//Vol
                    m_reqIdMngP.InsertReqToDic(volReqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_ASK_BID_ALGO);
                    m_clientP.GetMktData(1, m_bidAskDb.GetCurrContract());

                    Thread anlThr = new Thread(() => WaitAndGetHistoryData(trdReqId, volReqId, bidAskReqId));
                    anlThr.Start();

                    SleepAndWaitT(m_diffBetweenREQ * 1000);

                    m_hisInfo.wakeUpFromTimeOut = true;

                    anlThr.Join();

                    if (m_firstTimeWasSet == false)
                        m_firstTime = (DateTime)currTime;
                    else
                    {
                        Console.WriteLine("first time to set by previos cycle to time = ", m_firstTime.ToString("yyyyMMdd HH:mm:ss"));
                        LogHandler.WriteToFile("first time to set by previos cycle to time = ", m_firstTime.ToString("yyyyMMdd HH:mm:ss"));
                        m_firstTime.AddSeconds(1);
                    }
                    if (m_bidAskTimeWasSet == false)
                    {
                        m_bidAskLstTime = m_firstTime;
                    }

                   
                }
                else
                {
                    Thread.Sleep(5);
                }

            }
            while (DateTime.Now < endOfMarket && m_stopAnalyze == false && m_clientP.isConnected());

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                m_mainWindowP.g_bisAskAlgoStAnz_bt.Content = "START ANALYZE";
            });

            m_hisInfo.wakeUpFromTimeOut = true;

            MessageBox.Show("Bid Ask Algorithm end!!!!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SleepAndWaitT(int timeInMilSec_A)
        {
            int countSleep = 0;
            while ((timeInMilSec_A != countSleep)  && (m_stopAnalyze == false) && m_clientP.isConnected())
            {
                Thread.Sleep(500);
                countSleep += 500;
            }
        }
        private int WaitAndGetHistoryData(int trdReqId_A, int volReqId_A, int bidAskReqId_A,bool offline_A = false)
        {
            Thread.Sleep(2000);//Wait for update

            HistoricalTickBidAsk[] bidAskDB = null;
            bool volAsReceived = false;
            bool bidAskDbReceived = false;
            bool breakForBewBidAskReq = false;
            int prevCount = 0;
            int gloabalCnt = 0;
            object lstTrdTime = null;

            while (m_hisInfo.wakeUpFromTimeOut == false)
            {
                bool hisAsUpdate = false;

                if (!bidAskDbReceived)
                {
                    if (bidAskDbReceived = m_bidAskDb.BidAskDbAsRecevied(bidAskReqId_A) == true)
                    {
                        HandleWithSpin(true);
                        bidAskDB = m_bidAskDb.GetHisBidAskDb(bidAskReqId_A);
                        Console.WriteLine("History bid Ask as received , req id = " + bidAskReqId_A + " , num of size = " + (bidAskDB.Length));
                        LogHandler.WriteToFile("History bid Ask as received , req id = " + bidAskReqId_A + " , num of size = " + (bidAskDB.Length));
                    }
                }

                if (!volAsReceived)
                {
                    volAsReceived = m_bidAskDb.VolAsReceivd();

                    m_bidAskVolRes.tradeVol = m_bidAskDb.GetCurrVol();

                    if (!offline_A)
                    {
                        Console.WriteLine("Vol as received , req id = " + volReqId_A + " , vol size = " + m_bidAskVolRes.tradeVol);
                        LogHandler.WriteToFile("Vol as received , req id = " + volReqId_A + " , vol size = " + m_bidAskVolRes.tradeVol);
                    }
                }

                HistoricalTickLast[] hisDataList = m_bidAskDb.GetHistoryData(trdReqId_A);

                if (bidAskDbReceived &&
                    prevCount != hisDataList.Length)//Update
                {
                    Console.WriteLine("History as received , req id = " + trdReqId_A + " , num of size = " + (hisDataList.Length - prevCount));
                    LogHandler.WriteToFile("History as received , req id = " + trdReqId_A + " , num of size = " + (hisDataList.Length - prevCount));

                    for (int i = prevCount; i < hisDataList.Length; i++)
                    {
                        HistoricalTickLast his = hisDataList[i];

                        string[] date = Util.UnixSecondsToString(his.Time, "yyyyMMdd-HH:mm:ss zzz").Split();

                        DateTime trdTime = DateTime.ParseExact(date[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);

                        if (trdTime >= m_firstTime)
                        {
                            lstTrdTime = trdTime;
                            gloabalCnt++;

                            if (his.TickAttribLast.Unreported)
                            {
                                m_bidAskVolRes.unreported += his.Size;
                            }
                            else
                            {
                                switch (CheckIfTradeIsBidOrAsk(bidAskDB, his))
                                {
                                    case "ASK":
                                        m_bidAskVolRes.askSize += his.Size;

                                        if (his.Size >= m_maxTrdSizeForMark)
                                        {
                                            m_bidAskVolRes.countMaxTrdStkAsk++;
                                            m_bidAskVolRes.countSizeOfTrdStkAsk += his.Size;
                                        }
                                        break;
                                    case "BID":
                                        m_bidAskVolRes.bidSize += his.Size;

                                        if (his.Size >= m_maxTrdSizeForMark)
                                        {
                                            m_bidAskVolRes.countMaxTrdStkBid++;
                                            m_bidAskVolRes.countSizeOfTrdStkBid += his.Size;
                                        }
                                        break;
                                    case "NONE":
                                        break;
                                    case "BREAK":
                                        m_hisInfo.wakeUpFromTimeOut = true;
                                        breakForBewBidAskReq = true;                               
                                        break;
                                }

                                if (breakForBewBidAskReq)
                                    break;

                                if (m_bidAskVolRes.currPrice > his.Price ||
                                    (his.Price == m_bidAskVolRes.currPrice && m_stkisRaised == false))//Sum Ask
                                {
                                    m_stkisRaised = false;
                                    m_bidAskVolRes.currPrice = his.Price;
                                    //m_bidAskVolRes.askSize += his.Size;

                                }
                                else
                                {
                                    m_stkisRaised = true;
                                    m_bidAskVolRes.currPrice = his.Price;
                                    // m_bidAskVolRes.bidSize += his.Size;                             
                                }
                            }
                        }

                        prevCount = hisDataList.Length;
                        hisAsUpdate = true;
                    }
                }

                if (hisAsUpdate)
                {
                    HandleWithGUI((DateTime)lstTrdTime,offline_A);           
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(200);
                }

            }

            if ((lstTrdTime != null) && 
                ((gloabalCnt >= 1000) ||
                (offline_A)))
            {
                m_firstTime = (DateTime)lstTrdTime;
                if (!breakForBewBidAskReq)
                    m_firstTime = m_firstTime.AddSeconds(1);
                else
                    m_bidAskLstTime = m_firstTime.AddSeconds(-10);

                m_firstTimeWasSet = true;
            }

            //Save last BID_ASK time
            if(!breakForBewBidAskReq && 
                bidAskDB != null && 
                bidAskDB.Length > 0)
            {
                string[] date = Util.UnixSecondsToString(bidAskDB[bidAskDB.Length-1].Time, "yyyyMMdd-HH:mm:ss zzz").Split();
                m_bidAskLstTime = DateTime.ParseExact(date[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);
                m_bidAskTimeWasSet = true;
            }

            
            Console.WriteLine("curr price = " + m_bidAskVolRes.currPrice);
            LogHandler.WriteToFile("curr price = " + m_bidAskVolRes.currPrice);
            Console.WriteLine("ask size = " + m_bidAskVolRes.askSize + " Exc ask cnt = " + m_bidAskVolRes.countMaxTrdStkAsk + " Exc ask size = " + m_bidAskVolRes.countSizeOfTrdStkAsk);
            LogHandler.WriteToFile("ask size = " + m_bidAskVolRes.askSize + " Exc ask cnt = " + m_bidAskVolRes.countMaxTrdStkAsk + " Exc ask size = " + m_bidAskVolRes.countSizeOfTrdStkAsk);
            Console.WriteLine("bid size = " + m_bidAskVolRes.bidSize + " Exc bid cnt = " + m_bidAskVolRes.countMaxTrdStkBid + " Exc bid size = " + m_bidAskVolRes.countSizeOfTrdStkBid);
            LogHandler.WriteToFile("bid size = " + m_bidAskVolRes.bidSize + " Exc bid cnt = " + m_bidAskVolRes.countMaxTrdStkBid + " Exc bid size = " + m_bidAskVolRes.countSizeOfTrdStkBid);
            if (!offline_A)
            {
                Console.WriteLine("curr vol = " + m_bidAskVolRes.tradeVol);
                LogHandler.WriteToFile("curr vol = " + m_bidAskVolRes.tradeVol);
            }

            m_reqIdMngP.RemoveActionFromDic(trdReqId_A);
            m_reqIdMngP.RemoveActionFromDic(volReqId_A);
            m_reqIdMngP.RemoveActionFromDic(bidAskReqId_A);

            HandleWithSpin(false);

            return gloabalCnt;
        }

        private string CheckIfTradeIsBidOrAsk(HistoricalTickBidAsk[] bidAskDb_A, HistoricalTickLast trdInfo_A)
        {
            string ret = "NONE";
            int i = 0;

            string[] dateTrd = Util.UnixSecondsToString(trdInfo_A.Time, "yyyyMMdd-HH:mm:ss zzz").Split();

            DateTime trdTime = DateTime.ParseExact(dateTrd[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);
            string[] dateBidAsk = null;
            DateTime bidAskTime;
            bool found = false;

            for (i = 0; i < bidAskDb_A.Length; i++)
            {
                dateBidAsk = Util.UnixSecondsToString(bidAskDb_A[i].Time, "yyyyMMdd-HH:mm:ss zzz").Split();

                bidAskTime = DateTime.ParseExact(dateBidAsk[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);

                if (bidAskTime >= trdTime)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                DateTime closeToEnd = new DateTime(trdTime.Year, trdTime.Month, trdTime.Day, 22, 58, 00);

                if (trdTime > closeToEnd)
                    bidAskTime = trdTime.AddSeconds(1);
                else
                 return "BREAK";
            }

            for (int j = i - 1; j >= 0; j--)
            {
                if (bidAskDb_A[j].PriceAsk <= trdInfo_A.Price ||
                    trdInfo_A.Price < bidAskDb_A[j].PriceAsk && trdInfo_A.Price > bidAskDb_A[j].PriceBid)
                {
                    ret = "ASK";
                    break;
                }
                else if (bidAskDb_A[j].PriceBid >= trdInfo_A.Price)
                {
                    ret = "BID";
                    break;
                }
            }

            return ret;
        }

        private void HandleWithSpin(bool spin_A)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                m_mainWindowP.g_bidAskAlgoSpin_sp.Spin = spin_A;
            });
        }

        private void InitGUI()
        {
            HandleWithSpin(false);

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                CultureInfo cul = new CultureInfo("en-US");
                m_mainWindowP.g_bidAskAlgoCurrPrice_tb.Text = "0.0";
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_bidAskAlgoVolTrade_tb.Text = "0";
                m_mainWindowP.g_bidAskAlgoVolDol_tb.Text = "0" + "$";
                m_mainWindowP.g_bidCntMaxBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_askCntMaxBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_bidMaxBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_askMaxBidAskAlgo_tb.Text = "0";
                m_mainWindowP.g_bidAslAlgoUnreporetd_tb.Text = "0";
                m_mainWindowP.g_bidAslAlgolstTrdTime_tb.Text = "";
                m_mainWindowP.g_bidAslAlgoUnreporetd_tb.Text = "0";
                m_mainWindowP.g_bidAslAlgoDiff_tb.Text = "0";
                m_mainWindowP.g_bidAslAlgoOpenPrice_tb.Text = "0.0";


                //Color GUI
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                m_mainWindowP.g_bidAskAlgoVolTrade_tb.Background = Brushes.White;
            });
        }
        private void HandleWithGUI(DateTime lstTrdTime_A,bool offlineMode_A)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                double dollarVol = 0;
                CultureInfo cul = new CultureInfo("en-US");
                m_mainWindowP.g_bidAskAlgoCurrPrice_tb.Text = m_bidAskVolRes.currPrice.ToString();
                m_mainWindowP.g_bidAslAlgoOpenPrice_tb.Text = m_bidAskVolRes.openPrice.ToString();
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Text = m_bidAskVolRes.askSize.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Text = m_bidAskVolRes.bidSize.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_bidAslAlgoDiff_tb.Text = Math.Abs(m_bidAskVolRes.askSize - m_bidAskVolRes.bidSize).ToString("N", cul);
                m_mainWindowP.g_bidAslAlgoDiff_tb.Text += "/" + GetPrecentBetweenBidAsk((Math.Abs(m_bidAskVolRes.askSize - m_bidAskVolRes.bidSize)), Math.Max((long)m_bidAskVolRes.askSize, (long)m_bidAskVolRes.bidSize)).ToString() + "%";
                if (offlineMode_A == false)
                     m_mainWindowP.g_bidAskAlgoVolTrade_tb.Text = (m_bidAskVolRes.tradeVol - (int)m_bidAskVolRes.unreported).ToString("N", cul).Replace(".00", "");
                else
                    m_mainWindowP.g_bidAskAlgoVolTrade_tb.Text = ((m_bidAskVolRes.askSize + m_bidAskVolRes.bidSize) - (int)m_bidAskVolRes.unreported).ToString("N", cul).Replace(".00", "");
                if (offlineMode_A == false)
                {
                    dollarVol = m_bidAskVolRes.currPrice * m_bidAskVolRes.tradeVol;
                    m_mainWindowP.g_bidAskAlgoVolDol_tb.Text = dollarVol.ToString("N", cul) + "$";
                }
                else
                {
                    dollarVol = (m_bidAskVolRes.currPrice * ((m_bidAskVolRes.askSize + m_bidAskVolRes.bidSize) - (int)m_bidAskVolRes.unreported));
                    m_mainWindowP.g_bidAskAlgoVolDol_tb.Text = dollarVol.ToString("N", cul) + "$";
                }
                m_mainWindowP.g_bidCntMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countMaxTrdStkBid.ToString();
                m_mainWindowP.g_askCntMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countMaxTrdStkAsk.ToString();
                m_mainWindowP.g_bidMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countSizeOfTrdStkBid.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_askMaxBidAskAlgo_tb.Text = m_bidAskVolRes.countSizeOfTrdStkAsk.ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_bidAslAlgoUnreporetd_tb.Text = (m_bidAskVolRes.unreported).ToString("N", cul).Replace(".00", "");
                m_mainWindowP.g_bidAslAlgolstTrdTime_tb.Text = lstTrdTime_A.ToString("ddMMyyyy HH:mm:ss");

                //Color GUI
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_askSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Background = Brushes.White;
                m_mainWindowP.g_bidSizeBidAskAlgo_tb.Foreground = Brushes.Black;
                if (m_bidAskVolRes.askSize < m_bidAskVolRes.bidSize)
                {
                    m_mainWindowP.g_bidSizeBidAskAlgo_tb.Background = Brushes.Red;
                    m_mainWindowP.g_bidSizeBidAskAlgo_tb.Foreground = Brushes.White;
                }
                else if (m_bidAskVolRes.askSize > m_bidAskVolRes.bidSize)
                {
                    if (m_bidAskVolRes.currPrice < m_bidAskVolRes.openPrice)
                    {
                        m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.Red;
                        m_mainWindowP.g_askSizeBidAskAlgo_tb.Foreground = Brushes.White;
                    }
                    else
                    {
                        m_mainWindowP.g_askSizeBidAskAlgo_tb.Background = Brushes.LightGreen;
                    }
                
                }

                if (dollarVol >= 1000000)
                {
                    m_mainWindowP.g_bidAskAlgoVolDol_tb.Background = Brushes.LightGreen;
                }
            });
        }
        private int GetPrecentBetweenBidAsk(long diff_A,long size_A)
        {
            int div = (int)((((double)diff_A) / ((double)size_A)) * 100);

            return div;
        }
    }
    }

