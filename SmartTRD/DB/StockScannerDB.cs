using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    class StockScannerDB: iStockScannerDB
    {

        public struct FULL_INFO_ON_STK_s
        {
            public ContractDetails stkDetails;
            public List<Bar> stkHistory;
        }

        Dictionary<string, FULL_INFO_ON_STK_s> m_conDescScanner_P;
        static StockScannerDB m_instanse;

        public StockScannerDB()
        {
            m_instanse = this;
            m_conDescScanner_P = null;
        }

        public void Init()
        {
            m_conDescScanner_P = new Dictionary<string, FULL_INFO_ON_STK_s>();
        }

        public void ReadyToNewScan()
        {
            m_conDescScanner_P.Clear();
        }

        public void InsertNewContrastDestToList(ContractDetails contractDescriptions_A)
        {
            if( m_conDescScanner_P.ContainsKey(contractDescriptions_A.Contract.Symbol) == false)
            {
                FULL_INFO_ON_STK_s fullInfo = new FULL_INFO_ON_STK_s();
                fullInfo.stkHistory = new List<Bar>();
                fullInfo.stkDetails = contractDescriptions_A;

                m_conDescScanner_P[fullInfo.stkDetails.Contract.Symbol] = fullInfo;
            }
        }

        public void InsertNetHistoryBarContractToList(string stkName, Bar barHistory_A)
        {
            if (m_conDescScanner_P.ContainsKey(stkName) == true)
            {
                m_conDescScanner_P[stkName].stkHistory.Add(barHistory_A);
            }
        }

        public List<ContractDetails> GetStkContractDetailsFromDB()
        {
            List<ContractDetails> listOfScanContract = new List<ContractDetails>();
            foreach (FULL_INFO_ON_STK_s fullStk in m_conDescScanner_P.Values)
            {
                listOfScanContract.Add(fullStk.stkDetails);

            }

            return listOfScanContract;
        }

        public List<Contract> GetStkContractFromScanRes()
        {
            List<Contract> listOfScanContract = new List<Contract>();
            foreach (FULL_INFO_ON_STK_s fullStk in m_conDescScanner_P.Values)
            {
                listOfScanContract.Add(fullStk.stkDetails.Contract);

            }

            return listOfScanContract;
        }

        public static iStockScannerDB GetInstanse()
        {
            return m_instanse;
        }
    }
}
