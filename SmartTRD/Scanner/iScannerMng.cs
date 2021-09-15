using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartTRD.Scanner.ScannerMngDB;

namespace SmartTRD.Scanner
{
    interface iScannerMng
    {
        void ReportOnReqId(int reqId_A, INTERVAL_STATUS_e reqStatus_A);
        string GetStkNameByReqId(int reqId_A);
    }
}
