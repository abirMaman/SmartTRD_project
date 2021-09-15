using IBApi;
using SmartTRD.DB;
using SmartTRD.IBclient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SmartTRD.Scanner.ScannerMngDB;

namespace SmartTRD.Scanner
{
    class ScannerMng:iScannerMng
    {
        enum SCANNER_PHASE_e
        {
            SCANNER_PHASE_SCAN_STK ,
            SCANNER_PHASE_GET_HISTORY
        }
        private Int32 m_numOfReqToWait = 0;
        private string[] m_scanType = { "MOST_ACTIVE", "HOT_BY_VOLUME", "TOP_PERC_GAIN", "HIGH_OPT_VOLUME_PUT_CALL_RATIO", "TOP_TRADE_COUNT", "HALTED" };
 
        Mutex m_mutex;
        Dictionary<int, SCAN_INTERVAL_INFO_s> m_scanMngInfoL;

        //Interfaces
        private iBclient m_clientP;
        private iStockScannerDB m_stockScnDbP;
        private static ScannerMng m_instasne;
        private static bool saveToXml = false;
        private static bool readFromXml = true;
        public ScannerMng()
        {
            m_clientP = null;
            m_stockScnDbP = null;
            m_scanMngInfoL = null;
            m_mutex = null;
            m_instasne = this;
        }

        public static ScannerMng GetInstanse()
        {
            return m_instasne;
        }
        public void Init()
        {
            m_clientP = BclientCon.GetInstase();
            m_stockScnDbP = StockScannerDB.GetInstanse();
            m_mutex = new Mutex();
            m_scanMngInfoL = new Dictionary<int, SCAN_INTERVAL_INFO_s>();
        }

        public void StartScanStkProcess()
        {
            Thread startProcessT = new Thread(StartProcess);
            startProcessT.Start();
        }

        public void StartProcess()
        {
            if (readFromXml == false)
            {
                //========================== Phase 1 - SCAN ==================================================
                //Build DB for phase 1 - OTC stock with some attributes like price and echange.
                Thread startScanT = new Thread(StartScanInDiffrentThr);
                startScanT.Start();
                startScanT.Join();
                //========================== Phase 1 - SCAN ==================================================
            }
            else
            {
                XML.XmlLoad<List<ContractDetails>> conLoad = new XML.XmlLoad<List<ContractDetails>>();
                try
                {
                    List<ContractDetails> stkResFromScan = conLoad.loadData("stkScannerRes.xml");

                    foreach(ContractDetails stk in stkResFromScan)
                    {
                        m_stockScnDbP.InsertNewContrastDestToList(stk);
                    }
                }
                catch(Exception) { }

                           
            }

            //=========================== Phase 2 - Build Full DB - History and Contrart Details ===================
            //Build history of 6 month backward for future algoritem
            Thread hisDataT = new Thread(() => GetHistoryDataForScanStk());
            hisDataT.Start();
            hisDataT.Join();
            //=========================== Phase 2 - Build Full DB - History and Contrart Details ===================
        }

        public void ReportOnReqId(int reqId_A, INTERVAL_STATUS_e reqStatus_A)
        {
            
                if (reqStatus_A == INTERVAL_STATUS_e.FINSHED_WITHOUT_ERROR)
                {
                    SCAN_INTERVAL_INFO_s reqScanInfo;
                    if (m_scanMngInfoL.TryGetValue(reqId_A, out reqScanInfo))
                    {
                        reqScanInfo.reqStatus = reqStatus_A;
                        m_mutex.WaitOne();
                        m_scanMngInfoL[reqId_A] = reqScanInfo;
                        m_mutex.ReleaseMutex();
                    }
                }   
        }

        public string GetStkNameByReqId(int reqId_A)
        {
            string stkName = "";
            SCAN_INTERVAL_INFO_s reqScanInfo;
            if (m_scanMngInfoL.TryGetValue(reqId_A, out reqScanInfo))
            {
                stkName = reqScanInfo.stkName;
            }

            return stkName;
        }

        private void StartScanInDiffrentThr()
        {

            m_numOfReqToWait = m_scanType.Length;
            m_scanMngInfoL.Clear();
            m_stockScnDbP.ReadyToNewScan();

            ///Create wait to answer Thread ->> From Wrapper ====
            Thread waitToAnswerT = new Thread(WaitToWrapperAnwer);
            waitToAnswerT.Start();
            ///Create wait to answer Thread ->> From Wrapper ====

            double belowPrice = 0.0150;//Config
            double abovePrice = 0.0050;//Config
            int aboveVol = 0;//Config
            for (int i = 0; i < m_scanType.Length; i++)
            {
                if (m_clientP != null)
                {
                    InsertReqToWaitMngArray();
                    m_clientP.StartToScanStocksByType(belowPrice, abovePrice, aboveVol, m_scanType[i]);
                }
            }
       
            waitToAnswerT.Join();

            if(saveToXml)
            {
                List<ContractDetails> stkResFromScan = m_stockScnDbP.GetStkContractDetailsFromDB();
                XML.XmlHandler.SaveData(stkResFromScan, "stkScannerRes.xml");
            }

        }
       

        private void GetHistoryDataForScanStk()
        {
            List<Contract> stkResFromScan = m_stockScnDbP.GetStkContractFromScanRes();
            m_numOfReqToWait = stkResFromScan.Count;
            m_scanMngInfoL.Clear();

            Thread waitToAnswerT = new Thread(WaitToWrapperAnwer);
            waitToAnswerT.Start();

            foreach(Contract con in stkResFromScan)
            {
                if(m_clientP != null)
                {
                    InsertReqToWaitMngArray(con.Symbol);
                    m_clientP.GetHistorySymbolData(con, 6, "TRADES");
                }
            }

            waitToAnswerT.Join();

        }

        private void WaitToWrapperAnwer()
        {
            bool finishedToGetAnswers = false;
           
            while(finishedToGetAnswers == false)
            {
                int countAnswer = 0;

                    m_mutex.WaitOne();
                    foreach(SCAN_INTERVAL_INFO_s inter in m_scanMngInfoL.Values)
                    {
                        if (inter.reqStatus != INTERVAL_STATUS_e.IN_PROGRESS)
                        {
                            countAnswer++;
                        }
                        else
                            break;
                    }
                    m_mutex.ReleaseMutex();

                if (countAnswer == m_numOfReqToWait)
                    finishedToGetAnswers = true;

                Thread.Sleep(50);
            }
        }
        private void InsertReqToWaitMngArray(string stkName= "")
        {
            int reqId = m_clientP.GetNextReqId();
            SCAN_INTERVAL_INFO_s scanInterInfo;
            scanInterInfo.req_id = reqId;
            scanInterInfo.reqStatus = INTERVAL_STATUS_e.IN_PROGRESS;
            scanInterInfo.stkName = stkName;
            m_mutex.WaitOne();
            m_scanMngInfoL[reqId] = scanInterInfo;
            m_mutex.ReleaseMutex();
        }


    }
}
