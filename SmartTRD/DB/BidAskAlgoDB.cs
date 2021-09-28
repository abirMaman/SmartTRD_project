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

        static BidAskAlgoDB m_instase;
        List<HistoricalTickLast> m_lastHisData;
        int m_currVol;
        double m_closePrice;
        Contract m_currContract;

        public BidAskAlgoDB()
        {
            m_currContract = null;
            m_lastHisData = null;
            m_currVol = 0;
            m_instase = this;
        }

        public void Init()
        {

        }

        public static BidAskAlgoDB GetInstanse()
        {
            return m_instase;
        }

   

        public void StartNewSession()
        {
            m_currContract = null;
            m_lastHisData = null;
             m_closePrice = 0.0;
             m_currVol = 0;
        }

        public void SetClosePrice(double clPrice_A)
        {
            m_closePrice = clPrice_A;
        }

        public void SetContract(Contract con_A)
        {
            con_A.Exchange = "SMART";
            m_currContract = con_A;
        }

        public Contract GetCurrContract()
        {
            return m_currContract;
        }

        public void SetCurrVol(int vol_A)
        {
            m_currVol = vol_A;
        }
        public void InsertNewHistoryData(HistoricalTickLast currBar_A)
        {
            if (m_lastHisData == null)
                m_lastHisData = new List<HistoricalTickLast>();

            m_lastHisData.Add(currBar_A);
        }
        public int GetCurrVol()
        {
            int tempVol = m_currVol * 100;
            m_currVol = 0;
            return tempVol;
        }
         public double GetClosePrice()
        {
            return m_closePrice;
        }

        public List<HistoricalTickLast> GetHistoryData()
        {
            List<HistoricalTickLast> returnList = new List<HistoricalTickLast>(m_lastHisData.Count);
            returnList = m_lastHisData;
            m_lastHisData = null;
            return returnList;
        }

        public bool HistoryDataAsReceived()
        {
            return (m_lastHisData != null && m_lastHisData.Count > 0) ;
        }

        public bool VolAsReceivd()
        {
            return m_currVol != 0;
        }

        public bool ClosePriceAsReceived()
        {
            return m_closePrice != 0;
        }

        public bool ContractAsReceived()
        {
            return m_currContract != null;
        }
    }
}
