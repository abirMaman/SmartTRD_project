using IBApi;
using SmartTRD.DB;
using SmartTRD.IBclient;
using SmartTRD.ReqId;
using System;
using System.Collections.Generic;
using System.Threading;
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
        private iReqIdMng m_reqIdMngP;
        private static ScannerMng m_instasne;
        private static bool saveToXml = false;
        private static bool readFromXml = true;
        public ScannerMng()
        {
            m_clientP = null;
            m_stockScnDbP = null;
            m_scanMngInfoL = null;
            m_mutex = null;
            m_reqIdMngP = null;
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
            m_reqIdMngP = ReqIdMng.GetInstanse();
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

            //=========================== Phase 2 - Filter Full DB - by OTC marget values ===================
            FilterStkByOtcMarkertData();
            //=========================== Phase 2 - Filter Full DB - by OTC marget values ===================

            //=========================== Phase 3 - Build Full DB - History and Contrart Details ===================
            //Build history of 6 month backward for future algoritem
            Thread hisDataT = new Thread(() => GetHistoryDataForScanStk());
            hisDataT.Start();
            hisDataT.Join();
            //=========================== Phase 3 - Build Full DB - History and Contrart Details ===================
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
            try
            {
                if (m_scanMngInfoL.TryGetValue(reqId_A, out reqScanInfo))
                {
                    stkName = reqScanInfo.stkName;
                }
            }
            catch (Exception e){ }

            return stkName;
        }

        private void FilterStkByOtcMarkertData()
        {
            List<Contract> stkResFromScan = m_stockScnDbP.GetStkContractFromScanRes();
            //Parmter for filter 
            int unrestricredLowLimit = 500000000;//Config
            int unrestricredUpeerLimit = 2000000000;//Config
            double offsetBetAsAndOsIsPrecent = 0.2;//20%//Config

            foreach (Contract stk in stkResFromScan)
            {
                bool deleteStk = false;
                string vpDate = "";
                string jsonFullInString = Http.HttpOtcMarket.GetStrFromFullProfileOtcMarket(stk.Symbol);
                string jsonSymbolInString = Http.HttpOtcMarket.GetStrFromSymbolProfileOtcMarket(stk.Symbol);
                if (jsonFullInString == "" ||
                    jsonSymbolInString == "")//There is no json
                {
                    deleteStk = true;
                }
                else
                {
                    JsonAnalyzer.JsonStkInfo jsonStk = new JsonAnalyzer.JsonStkInfo(jsonFullInString);
                    JsonAnalyzer.JsonStkStmbolInfo jsonSymbolStk = new JsonAnalyzer.JsonStkStmbolInfo(jsonSymbolInString);

                    int unrShare = jsonStk.GetStkUnrestricted();
                    int autShare = jsonStk.GetStkAutorizedShare();
                    int osShare = jsonStk.GetStkOutstandingShare();
                    int diffBetASandOS = autShare - osShare;

                    if(jsonSymbolStk.StkAsPennyStkExempt() == false)
                    {
                        deleteStk = true;
                    }
                    else if(jsonStk.StkAsTransferAgent() == false)
                    {
                        deleteStk = true;
                    }
                    else if(jsonStk.StkAsVerfiedProfile(out vpDate) == false)
                    {
                        deleteStk = true;
                    }
                    else if(jsonStk.StkisOTCQC() == false)
                    {
                        deleteStk = true;
                    }
                    else if (jsonStk.StkIsPink() == false)
                    {
                        deleteStk = true;
                    }
                    else if (unrestricredLowLimit > unrShare)
                    {
                        deleteStk = true;
                    }
                    else if(unrestricredUpeerLimit < unrShare)
                    {
                        deleteStk = true;
                    }
                    else if (autShare * offsetBetAsAndOsIsPrecent > diffBetASandOS)
                    {
                        deleteStk = true;
                    }
                }

                if(deleteStk)
                {
                    m_stockScnDbP.DeleteStkFromDB(stk.Symbol);
                }
            }
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

            double belowPrice = 0.02;//Config
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
                    m_clientP.GetHistorySymbolData(con, "6 M", "30 min", "TRADES");
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
            m_reqIdMngP.InsertReqToDic(reqId, ReqIdMng.ACTION_REQ_e.ACTION_REQ_SCANNER);
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
