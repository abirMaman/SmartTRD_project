﻿using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.IBclient
{
    interface iBclient
    {
        bool connectToIbClientTWS(string ip_A, int port_A, int clientId_A);
        void StartToScanStocksByType(double belowPrice_A, double abovePrice_A, int aboveVol_A, string scanCode_A);
        void GetHistorySymbolData(Contract contrast_A, int dayToHis_A, string infoType_A);
        void GetContransDetails(Contract contranst_A);
        void GetSymbolDetails(string symbol_A);
        int GetNextReqId();

    }
}
