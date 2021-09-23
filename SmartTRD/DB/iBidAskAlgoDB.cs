using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.DB
{
    interface iBidAskAlgoDB
    {
         void StartNewSession();
         void SetContract(Contract con_A);
         Contract GetCurrContract();
         void SetFirstAsk(double ask_A);
    
         void SetFirstBid(double bid_A);

         void SetCurrVol(int vol_A);

         void InsertNewHistoryData(Bar currBar_A);

         int GetCurrVol();

         List<Bar> GetHistoryData();
    
         bool HistoryDataAsReceived();

         bool VolAsReceivd();

         bool AskAndBidAsReceived();

         bool ContractAsReceived();
    }
}
