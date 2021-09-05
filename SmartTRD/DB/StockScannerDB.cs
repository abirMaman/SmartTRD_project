using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    class StockScannerDB
    {

        private List<ContractDescription> m_conDescScannerP;

        StockScannerDB()
        {
            m_conDescScannerP = new List<ContractDescription>();
        }

        public void InsertNewContrastDescToList(ContractDescription[] contractDescriptions_A, bool first_A = false)
        {
            if (first_A)
                m_conDescScannerP.Clear();

            foreach(ContractDescription con in contractDescriptions_A)
            {
                m_conDescScannerP.Add(con);
            }
        }

        public List<ContractDescription> GetConScanDB()
        {
            return m_conDescScannerP;
        }
    }
}
