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
         void SetCurrVol(int vol_A);
         void SetClosePrice(double clPrice_A);
         int GetCurrVol();
         HistoricalTickLast[] GetHistoryData(int reqId_A);
         void InsertNewHistoryData(int reqId_A, HistoricalTickLast currBar_A);
         void SetNewReqAndPrepare(int reqId_A);
         bool VolAsReceivd();
         bool ContractAsReceived();
        bool ClosePriceAsReceived();
        HistoricalTickBidAsk[] GetHisBidAskDb(int reqId_A);
        bool BidAskDbAsRecevied(int reqId_A);
        double GetClosePrice();
        bool BidAsRecevied();
        bool AskAsRecevied();
        double GetAskPrice();
        double GetBidPrice();
        void SetBidPrice(double bidPrice_A);
        void SetAskPrice(double askPrice_A);
        void SetBidAskPriceToDb(int reqId_A, HistoricalTickBidAsk[] bidAsDb_A);
        bool OpenPriceAsReceived();
        void SetOpenPrice(double openPrice_A);
        double GetOpenPrice();
    }
}
