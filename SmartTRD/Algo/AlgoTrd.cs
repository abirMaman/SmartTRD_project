using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.Algo
{
    class AlgoTrd
    {

        bool CheckPatternLoadingStk(DB.StockScannerDB.FULL_INFO_ON_STK_s stkInfo,int dayToCheckLoad_A,int tickOffset_A)
        {
            bool   stockIsLoading = false;
            double priceCloseInLastDayTrd = stkInfo.stkHistory[0].Close;
            double priceLoadLimitUp = priceCloseInLastDayTrd + tickOffset_A;
            double priceLoadLimitDown = priceCloseInLastDayTrd - tickOffset_A;

            for (int i=1; i < dayToCheckLoad_A + 1;i++)
            {
                if((priceLoadLimitDown <= stkInfo.stkHistory[i].Close) && 
                    (stkInfo.stkHistory[i].Close <= priceLoadLimitUp))
                {
                    stockIsLoading = true;
                }
                else
                {
                    stockIsLoading = false;
                    break;
                }
            }
            return stockIsLoading;
        }
    }
}
