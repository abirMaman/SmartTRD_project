using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    interface iStockScannerDB
    {
        void ReadyToNewScan();

        void InsertNewContrastDestToList(ContractDetails contractDescriptions_A);
        void InsertNetHistoryBarContractToList(string stkName, Bar barHistory_A);
        void DeleteStkFromDB(string sybmol_A);
        List<Contract> GetStkContractFromScanRes();
        List<ContractDetails> GetStkContractDetailsFromDB();


    }
}
