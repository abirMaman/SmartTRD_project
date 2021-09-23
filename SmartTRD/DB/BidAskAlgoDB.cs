using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    

    class BidAskAlgoDB:iBidAskAlgoDB
    {

        struct CONTRACT_FIRST_ASKBID_INFO_s
        {
            public double first_bid;
            public double first_ask;
        }

        static BidAskAlgoDB m_instase;
        CONTRACT_FIRST_ASKBID_INFO_s m_firAskBid;
        List<Bar> m_lastHisData;
        int m_currVol;
        Contract m_currContract;

        public BidAskAlgoDB()
        {
            m_currContract = null;
            m_lastHisData = null;
            m_firAskBid.first_ask = 0.0;
            m_firAskBid.first_bid = 0.0;
            m_currVol = 0;
            m_instase = this;
        }

        public static BidAskAlgoDB GetInstanse()
        {
            return m_instase;
        }

        public void StartNewSession()
        {
            m_currContract = null;
            m_lastHisData = null;
            m_firAskBid.first_ask = 0.0;
            m_firAskBid.first_bid = 0.0;
            m_currVol = 0;
        }

        public void SetContract(Contract con_A)
        {
            m_currContract = con_A;
        }

        public Contract GetCurrContract()
        {
            return m_currContract;
        }

        public void SetFirstAsk(double ask_A)
        {
            m_firAskBid.first_ask = ask_A;
        }
        public void SetFirstBid(double bid_A)
        {
            m_firAskBid.first_bid = bid_A;
        }

        public void SetCurrVol(int vol_A)
        {
            m_currVol = vol_A;
        }
        public void InsertNewHistoryData(Bar currBar_A)
        {
            if (m_lastHisData == null)
                m_lastHisData = new List<Bar>();

            m_lastHisData.Add(currBar_A);
        }
        public int GetCurrVol()
        {
            int tempVol = m_currVol;
            m_currVol = 0;
            return tempVol;
        }

        public List<Bar> GetHistoryData()
        {
            return m_lastHisData;
        }

        public bool HistoryDataAsReceived()
        {
            return (m_lastHisData != null && m_lastHisData.Count > 0) ;
        }

        public bool VolAsReceivd()
        {
            return m_currVol != 0;
        }

        public bool AskAndBidAsReceived()
        {
            return (m_firAskBid.first_ask != 0 && m_firAskBid.first_bid != 0);
        }

        public bool ContractAsReceived()
        {
            return m_currContract != null;
        }
    }
}
