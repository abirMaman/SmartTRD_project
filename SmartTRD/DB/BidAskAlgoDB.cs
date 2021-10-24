using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    

    class BidAskAlgoDB:iBidAskAlgoDB
    {
        struct BID_ASK_DB_s
        {
            public List<HistoricalTickBidAsk> bidAskDB;
            public bool askBidRcv;
        }
        static BidAskAlgoDB m_instase;
        //List<HistoricalTickLast> m_lastHisData;
        Dictionary<int, List<HistoricalTickLast>> m_lastHisData;
        Dictionary<int, BID_ASK_DB_s> m_bidAskDb;
        int m_currVol;
        double m_closePrice;
        double m_openPrice;
        double m_bidPrice;
        double m_askPrice;
        Contract m_currContract;
        Mutex m_mutex;

        public BidAskAlgoDB()
        {
            m_currContract = null;
            m_lastHisData = null;
            m_bidAskDb = null;
            m_mutex = null;
            m_closePrice = 0.0;
            m_bidPrice = 0.0;
            m_askPrice = 0.0;
            m_openPrice = 0.0;
            m_currVol = 0;
            m_instase = this;
        }

        public void Init()
        {
            m_mutex = new Mutex();
        }

        public static BidAskAlgoDB GetInstanse()
        {
            return m_instase;
        }

        public void StartNewSession()
        {
            m_currContract = null;
            m_lastHisData = new Dictionary<int, List<HistoricalTickLast>>();
            m_bidAskDb = new Dictionary<int, BID_ASK_DB_s>();
            m_closePrice = 0.0;
            m_openPrice = 0.0;
            m_currVol = 0;
            m_askPrice = 0.0;
            m_bidPrice = 0.0;
        }

        public void SetNewReqAndPrepare(int reqId_A)
        {
            if (m_lastHisData.ContainsKey(reqId_A) == false)
            {
                List<HistoricalTickLast> his = new List<HistoricalTickLast>();
                m_lastHisData[reqId_A] = his;
                Console.WriteLine("Set new enter in dic of BidAlgoDb , req id = " + reqId_A);
            }
            else
            {
                Console.WriteLine("enter alreday exist,error!!! , re id = " + reqId_A);
            }
        }

        public void SetClosePrice(double clPrice_A)
        {
            m_closePrice = clPrice_A;
        }
        public void SetOpenPrice(double openPrice_A)
        {
            m_openPrice = openPrice_A;
        }

        public void SetBidPrice(double bidPrice_A)
        {
            m_bidPrice = bidPrice_A;
        }
        public void SetAskPrice(double askPrice_A)
        {
            m_askPrice = askPrice_A;
        }

        public void SetContract(Contract con_A)
        {
            con_A.Exchange = "SMART";
            m_currContract = con_A;
        }

        public void SetBidAskPriceToDb(int reqId_A,HistoricalTickBidAsk[] bidAsDb_A)
        {
            BID_ASK_DB_s bidAskDb = new BID_ASK_DB_s();
            if (m_bidAskDb.ContainsKey(reqId_A) == false)
            {
                bidAskDb.bidAskDB = bidAsDb_A.ToList();
                bidAskDb.askBidRcv = true;
                m_bidAskDb[reqId_A] = bidAskDb;
            }

        }

        public Contract GetCurrContract()
        {
            return m_currContract;
        }

        public void SetCurrVol(int vol_A)
        {
            m_currVol = vol_A;
        }
        public void InsertNewHistoryData(int reqId_A,HistoricalTickLast currBar_A)
        {

            if (m_lastHisData.ContainsKey(reqId_A) == true)
            {
                m_mutex.WaitOne();
                m_lastHisData[reqId_A].Add(currBar_A);
                m_mutex.ReleaseMutex();
                Console.WriteLine("Insert new item to list , req id " + reqId_A + " , time = " + currBar_A.Time);
            }
            else
            {
                Console.WriteLine("Insert new item to list failed!!!!!!!, req id " + reqId_A + " , time = " + currBar_A.Time);
            }
        }
        public int GetCurrVol()
        {
            int tempVol = m_currVol * 100;
            m_currVol = 0;
            return tempVol;
        }


        public double GetOpenPrice()
        {
            return m_openPrice;
        }

        public double GetBidPrice()
        {
            return m_bidPrice;
        }

        public double GetAskPrice()
        {
            return m_askPrice;
        }
        public double GetClosePrice()
        {
            return m_closePrice;
        }

        public HistoricalTickLast [] GetHistoryData(int reqId_A)
        {
            List<HistoricalTickLast> ret = null;
            HistoricalTickLast[] retArray = null;

            if (m_lastHisData.TryGetValue(reqId_A , out ret) == true)
            {
                m_mutex.WaitOne();
                retArray = new HistoricalTickLast[ret.Count];
                ret.CopyTo(retArray);
                m_mutex.ReleaseMutex();
            }

            return retArray;
        }

        public HistoricalTickBidAsk[] GetHisBidAskDb(int reqId_A)
        {
            List<HistoricalTickBidAsk> ret = null;
            BID_ASK_DB_s db;
            HistoricalTickBidAsk[] retArray = null;

            if (m_bidAskDb.TryGetValue(reqId_A, out db) == true)
            {
                retArray = new HistoricalTickBidAsk[db.bidAskDB.Count];
                db.bidAskDB.CopyTo(retArray);
            }

            return retArray;
        }

        public bool VolAsReceivd()
        {
            return m_currVol != 0;
        }

        public bool BidAsRecevied()
        {
            return m_bidPrice !=0;
        }
        public bool AskAsRecevied()
        {
            return m_askPrice != 0;
        }

        public bool ClosePriceAsReceived()
        {
            return m_closePrice != 0;
        }

        public bool OpenPriceAsReceived()
        {
            return m_openPrice != 0;
        }

        public bool ContractAsReceived()
        {
            return m_currContract != null;
        }

        public bool BidAskDbAsRecevied(int reqId_A)
        {
            bool ret = false;

            if (m_bidAskDb.ContainsKey(reqId_A) == true)
            {
                ret = m_bidAskDb[reqId_A].askBidRcv;
            }

            return ret;
        }
    }
}
