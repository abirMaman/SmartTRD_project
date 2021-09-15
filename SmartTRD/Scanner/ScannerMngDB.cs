using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.Scanner
{
    public class ScannerMngDB
    {

        //=== DATA STRUCTUERS ====
        public enum INTERVAL_STATUS_e
        {
            IN_PROGRESS,
            FINSHED_WITH_ERROR,
            FINSHED_WITHOUT_ERROR
        }
        public struct SCAN_INTERVAL_INFO_s
        {
            public int req_id;
            public string stkName;
            public INTERVAL_STATUS_e reqStatus;
        }
        //=== DATA STRUCTUERS ====
    }
}
