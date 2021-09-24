using IBApi;
using SmartTRD.DB;
using SmartTRD.IBclient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SmartTRD.BidAsk_Algo
{
    class BidAskAlgo:iBidAskAlgo
    {
        struct ASK_BID_VOLS_INFO_s
        {
            public long askSize;
            public double askPrice;
            public long bidSize;
            public double bidPrice;
            public int tradeVol;
            public int dollarVol;
        }
        struct BID_ASK_REQ_ID_s
        {
            public int bid_req_id;
            public int ask_req_id;
        }

        struct HISTORY_REQ_INFO_s
        {
            public int actReqId;
            public bool reqIsActive;
            public bool wakeUpFromTimeOut;
            public bool reqReceiveAnw;
        }

        private static BidAskAlgo m_instase;
        private ASK_BID_VOLS_INFO_s m_bidAskVolRes;
        private HISTORY_REQ_INFO_s m_hisInfo;
        private BID_ASK_REQ_ID_s m_bidAskReqIds;
        private iBidAskAlgoDB m_bidAskDb;
        private iBclient m_clientP;
        private string m_symbol;
        private string m_dateForAskBid;
        private DateTime m_firstTime;

        public BidAskAlgo()
        {
            m_clientP = null;
            m_bidAskDb = null;
            m_bidAskReqIds.ask_req_id = 0;
            m_bidAskReqIds.bid_req_id = 0;
            m_dateForAskBid = "";
            m_symbol = "";
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
        }

        public int GetBidReqId()
        {
            return m_bidAskReqIds.bid_req_id;
        }
        public int GetAskReqId()
        {
            return m_bidAskReqIds.ask_req_id;
        }


        public void StartAskBidAlgo(string symbol_A, string dateForAskBid_A)
        {
            m_symbol = symbol_A;
            m_dateForAskBid = dateForAskBid_A;
            m_bidAskReqIds.ask_req_id = 0;
            m_bidAskReqIds.bid_req_id = 0;
            m_bidAskDb.StartNewSession();

            Thread stAlgoT = new Thread(StartAlgoThread);
            stAlgoT.Start();
        }

        private void StartAlgoThread()
        {
            if (StepGetSymbol() == false)//Step 1
                return;

            if (StepPriceBidAskFirst() == false)//Step 2
                return;

            StartReceiveHisAndAnalyze();
        }


        private bool StepGetSymbol()
        {
            //===== Step 1 get contract details ======
            int countSleep = 0;
            bool symbolReceived = false;
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
            //===== Step 1 get contract details ======

            return symbolReceived;
        }
        private bool StepPriceBidAskFirst()
        {
            bool askAndBidReceived = false;
            int countSleep = 0;
            m_dateForAskBid += " " + "23:00:00";//Get last update from requested date

            m_bidAskReqIds.bid_req_id = m_clientP.GetNextReqId();
            m_clientP.GetHistorySymbolData(m_bidAskDb.GetCurrContract(), "1 D", "1 min", "BID", m_dateForAskBid);
            m_bidAskReqIds.ask_req_id = m_clientP.GetNextReqId();
            m_clientP.GetHistorySymbolData(m_bidAskDb.GetCurrContract(), "1 D", "1 min", "ASK", m_dateForAskBid);

            while (askAndBidReceived == false && countSleep < 5000)
            {
                askAndBidReceived = m_bidAskDb.AskAndBidAsReceived();

                if (askAndBidReceived == false)
                {
                    Thread.Sleep(50);
                    countSleep += 50;
                }
            }
            if (askAndBidReceived == false)
            {
                MessageBox.Show(m_symbol + " not recevied bid and ask price from date " + m_dateForAskBid + ", please try again", "Error BID ASK", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                m_bidAskVolRes.askPrice = m_bidAskDb.GetFirstAsk();
                m_bidAskVolRes.bidPrice = m_bidAskDb.GetFirstBid();
            }

            return askAndBidReceived;
        }

        private void StartReceiveHisAndAnalyze()
        {
            
            m_firstTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 00);
            DateTime endOfMarket = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 00, 00);
            object currTime = null;

            do
            {
                if (currTime == null)//First time
                {
                    currTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                       DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }

                double diff = ((DateTime)currTime).Subtract(m_firstTime).TotalSeconds;

                if (diff >= 10)
                {
                    m_hisInfo.reqIsActive = true;
                    m_hisInfo.reqReceiveAnw = false;
                    m_hisInfo.wakeUpFromTimeOut = false;
                    m_hisInfo.actReqId = m_clientP.GetNextReqId();
                    Console.WriteLine("time first = " + m_firstTime.ToString("yyyyMMdd HH:mm:ss" + " time curr = " + ((DateTime)currTime).ToString("yyyyMMdd HH:mm:ss")));
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), "", ((DateTime)currTime).ToString("yyyyMMdd HH:mm:ss"), 1000, "TRADES");
                    Thread anlThr = new Thread(() => WaitAndGetHistoryData(m_firstTime));
                    anlThr.Start();
                    Thread.Sleep(9990);
                    m_hisInfo.wakeUpFromTimeOut = true;
                    m_firstTime = (DateTime)currTime;
                    currTime = null;

                }
                else
                {
                    Thread.Sleep(10);
                }

            }
            while (DateTime.Now < endOfMarket);


            MessageBox.Show("Stop");
        }
   

        private void WaitAndGetHistoryData(DateTime firstTime_A)
        {
            Thread.Sleep(3000);//Wait  for update

            while(m_hisInfo.reqReceiveAnw == false && 
                m_hisInfo.wakeUpFromTimeOut == false)
            {
                m_hisInfo.reqReceiveAnw = m_bidAskDb.HistoryDataAsReceived();

                if(m_hisInfo.reqReceiveAnw)
                {
                    List<HistoricalTickLast> hisDataList = m_bidAskDb.GetHistoryData();
                    foreach(HistoricalTickLast his in hisDataList)
                    {
                        string[] date = Util.UnixSecondsToString(his.Time, "yyyyMMdd-HH:mm:ss zzz").Split();

                        DateTime trdTime = DateTime.ParseExact(date[0], "yyyyMMdd-HH:mm:ss", CultureInfo.InvariantCulture).AddHours(3);
                        if(trdTime >= firstTime_A)
                        {
                            if(his.Price <= m_bidAskVolRes.bidPrice && his.Price < m_bidAskVolRes.askPrice)//bid
                            {
                                m_bidAskVolRes.bidPrice = his.Price;
                                m_bidAskVolRes.bidSize += his.Size;
                            }
                            else
                            {
                                m_bidAskVolRes.askPrice = his.Price;
                                m_bidAskVolRes.askSize += his.Size;
                            }
                        }
                        else
                        {
                            int x = 1;
                        }
                    }
                }
            }
            if(m_hisInfo.reqReceiveAnw)
            {
                Console.WriteLine("ask price = " + m_bidAskVolRes.askPrice + " ask size = " + m_bidAskVolRes.askSize);
                Console.WriteLine("bid price = " + m_bidAskVolRes.bidPrice + " bid size = " + m_bidAskVolRes.bidSize);
                //Update GUI
            }
        }
    }
    }

