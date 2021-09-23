using SmartTRD.DB;
using SmartTRD.IBclient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SmartTRD.BidAsk_Algo
{
    class BidAskAlgo
    {
        struct BID_ASK_REQ_ID_s
        {
            public int bid_req_id;
            public int ask_req_id;
        }

        private BID_ASK_REQ_ID_s m_bidAskReqIds;
        private iBidAskAlgoDB m_bidAskDb;
        private iBclient m_clientP;
        private string m_symbol;
        private string m_dateForAskBid;

        public  BidAskAlgo()
        {
            m_clientP = null;
            m_bidAskDb = null;
            m_bidAskReqIds.ask_req_id = 0;
            m_bidAskReqIds.bid_req_id = 0;
            m_dateForAskBid = "";
            m_symbol = "";
        }

        public void Init()
        {
            m_bidAskDb = BidAskAlgoDB.GetInstanse();     
            m_clientP = BclientCon.GetInstase();
        }

        public void StartAskBidAlgo(string symbol_A,string dateForAskBid_A)
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

            if (StepGetSymbol() == false)//Step 2
                return;
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

            return askAndBidReceived;
        }

        private void StartReceiveHisAndAnalyze()
        {
            bool marketTimeUp = false;
            double bidPrice = m_bidAskDb.GetFirstBid();
            double askPrice = m_bidAskDb.GetFirstAsk();
            DateTime firstTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 00, 00);
            object currTime = null;

            do
            {
                if (currTime == null)//First time
                {
                    currTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                       DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                }

                double diff = ((DateTime)currTime).Subtract(firstTime).TotalSeconds;

                if(diff >= 10)
                {
                    int reqId = m_clientP.GetNextReqId();
                    m_clientP.GetHistorySymbolDataTickByTick(m_bidAskDb.GetCurrContract(), firstTime.ToString("yyyyMMdd HH:mm:ss"), 1000, "TRADES");
                }

            }
            while (marketTimeUp == false);
                
                



            }
            


        }
    }

