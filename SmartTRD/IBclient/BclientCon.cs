using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IBApi;
using Newtonsoft.Json.Linq;

namespace SmartTRD.IBclient
{
    class BclientCon : iBclient
    {
        static BclientCon m_instase;
        EClientSocket m_clientSocket;
        EReaderSignal m_readerSignal;
        EWrapperImpl m_testImpl = null;
        Int32 m_reqId;

        //! [socket_init]
        public BclientCon()
        {
            m_reqId = 0;
            m_clientSocket = null;
            m_readerSignal = null;
            m_testImpl = null;
            m_instase = this;
        }
        public static BclientCon GetInstase()
        {
            return m_instase;

        }

        public void Init(EWrapperImpl warpper_A)
        {
            m_testImpl = warpper_A;
        }

        public int GetNextReqId()
        {
            return (m_reqId + 1);
        }

        public bool TwsIsConnectedToApp()
        {
           return m_clientSocket.IsConnected();
        }

        public void DisconnectFromClientTws()
        {
            try
            {
                m_clientSocket.eDisconnect();
            }
            catch (Exception e1)
            {

            }
        }

        public bool connectToIbClientTWS(string ip_A, int port_A, int clientId_A)
        {
             m_clientSocket = m_testImpl.ClientSocket;
             m_readerSignal = m_testImpl.m_signal;

            try
            {
                m_clientSocket.eConnect(ip_A, port_A, clientId_A);

                //! [connect]
                //! [ereader]
                //Create a reader to consume messages from the TWS. The EReader will consume the incoming messages and put them in a queue
                var reader = new EReader(m_clientSocket, m_readerSignal);
                reader.Start();

                //Once the messages are in the queue, an additional thread can be created to fetch them
                new Thread(() =>
                {
                    while (m_clientSocket.IsConnected())
                    {
                        m_readerSignal.waitForSignal(); 
                        reader.processMsgs();
                    }
                })
                { IsBackground = true }.Start();
                //! [ereader]
            }
            catch(Exception e2)
            {
                LogHandle.LogHandler.WriteToFile("Error to connect ,exception info : " + e2.Message.ToString());
                MessageBox.Show("Error to connect ,exception info : " + e2.Message.ToString(), "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

         

            ////! [cashcfd]
            Contract contract = new Contract();
            contract.Symbol = "PBYA";
            contract.SecType = "STK";
            contract.Currency = "USD";
            contract.PrimaryExch = "PINK";
            contract.Exchange = "SMART";

            //m_clientSocket.reqContractDetails(123, contract);


            //Specify the Primary Exchange attribute to avoid contract ambiguity
            // (there is an ambiguity because there is also a MSFT contract with primary exchange = "AEB")
            //! [stkcontractwithprimary]
            //! [cashcfd]

            //m_clientSocket.reqMarketDataType(1);
             //m_clientSocket.reqMktData(111, contract, string.Empty, true, false, null);
            //m_clientSocket.reqHistogramData(1111, contract, true, "2 day");
             String queryTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
            string time = "20210922 16:30:00";
            string timec = "20211006 15:00:00";
            DateTime tt = new DateTime(2021, 09, 22, 16, 30, 00);
            DateTime tt1 = new DateTime(2021, 09, 22, 16, 30, 10);
            double ttee =  tt1.Subtract(tt).TotalSeconds;
            string time1 = "20210927 23:00:00";
            string timee = "20210923-15:00:00 +03:00";
            string []e = timee.Split();
            //DateTime ddd =  DateTime.ParseExact(timee, "yyyyMMdd-HH:mm:ss",CultureInfo.InvariantCulture);

            //m_clientSocket.reqRealTimeBars(3001, contract, 5, "MIDPOINT", true, null);
            //String queryTimeEnd = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
          // m_clientSocket.reqHistoricalData(4005, contract, time1, "1 D", "1 day", "TRADES", 1, 1, false, null);//Save for algo!!!!!
            //m_clientSocket.reqHistoricalData(4005, contract, time, "60 S", "1 secs", "ASK_BID", 1, 1, false, null);//Save for algo!!!!!
            //m_clientSocket.reqHistoricalData(4004, contract, time1, "1 D", "1 min", "BID", 1, 1, false, null);//Save for algo!!!!!
             //m_clientSocket.reqHistoricalData(4006, contract, time1, "1 D", "1 min", "ASK", 1, 1, false, null);//Save for algo!!!!!
            //m_clientSocket.reqHistoricalData(4005, contract, time, "60 S", "1 secs", "ASK", 1, 1, false, null);//Save for algo!!!!!
             //m_clientSocket.reqHistoricalTicks(4005, contract, timec, "", 1000, "BID_ASK", 1, false, null);
           // m_clientSocket.reqHistoricalTicks(4006, contract, timec, "", 1000, "TRADES", 1, false, null);
            //m_clientSocket.reqHistoricalTicks(4006, contract, timec, "", 1000, "MIDPOINT", 1, false, null);

            //for (int i = 0; i < 300; i++)
            //{
            //    //int req = 1;
            //    String RRRR = DateTime.Now.AddSeconds(10).AddHours(-1).ToString("yyyyMMdd HH:mm:ss");
            //    m_clientSocket.reqHistoricalTicks(i + 1, contract, RRRR, null, 1000, "TRADES", 1, false, null);
            //    Thread.Sleep(10000);
            //}
            ////Scan


            //HttpWebRequest req = WebRequest.Create("https://backend.otcmarkets.com/otcapi/company/profile/PBYA/badges?symbol=PBYA") as HttpWebRequest;
            //req.Method = WebRequestMethods.Http.Get;
            //req.Accept = "application/json, text/plain, */*";
            //req.Headers["Accept-Language"] = "en-US,en;q=0.9";
            //req.Host = "backend.otcmarkets.com";
            //req.Headers["Origin"] = "https://www.otcmarkets.com";
            //req.Referer = "https://www.otcmarkets.com/";
            //req.Headers["sec-ch-ua"] = "Chromium;v='93',' Not A;Brand';v='99',Google Chrome;v='93'";
            //req.Headers["sec-ch-ua-mobile"] = "?0";
            //req.Headers["Sec-Fetch-Dest"] = "empty";
            //req.Headers["Sec-Fetch-Mode"] = "cors";
            //req.Headers["Sec-Fetch-Site"] = "same-site";
            //req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
            //HttpWebResponse response = req.GetResponse() as HttpWebResponse;

            //using (Stream responseStream = response.GetResponseStream())
            //{
            //    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            //    string returnInfo = reader.ReadToEnd();

            //}



            // ScannerSubscription scanSub = new ScannerSubscription();
            // scanSub.BelowPrice = 0.0150;
            // scanSub.AbovePrice = 0.0050;
            // scanSub.Instrument = "STK";
            // scanSub.NumberOfRows = 50;
            // scanSub.LocationCode = "STK.PINK";
            // scanSub.ScanCode = "HALTED";//MOST_ACTIVE , HOT_BY_VOLUME,TOP_PERC_GAIN
            //// ! [highoptvolume]
            // m_clientSocket.reqScannerSubscription(7001, scanSub, "", null);
            //Thread.Sleep(1000);
            //m_clientSocket.reqScannerSubscription(7002, scanSub, "", null);

            return true;
      
        }

        public bool isConnected()
        {
           return  m_clientSocket.IsConnected();
        }

        public void GetSymbolDetails(string symbol_A)
        {
            if(m_clientSocket != null)
            {
                //EWrapperImpl::symbolSamples
                m_clientSocket.reqMatchingSymbols(++m_reqId, symbol_A);
            }
        }

        public void GetMktData(int mktType_A,Contract contrart_A)
        {
            if (m_clientSocket != null)
            {
                 m_clientSocket.reqMarketDataType(mktType_A);
                m_clientSocket.reqMktData(++m_reqId, contrart_A, string.Empty, true, false, null);
            }
        }

        public void CancelReq(int reqId_A)
        {
         
        }

        public void GetContransDetails(Contract contranst_A)
        {

                if (m_clientSocket != null)
                {
                    //EWrapperImpl::contractDetails
                    m_clientSocket.reqContractDetails(++m_reqId, contranst_A);
                }
        }
        public void GetHistorySymbolData(Contract contrast_A,string dayToHis_A, string resBar_A, string infoType_A,string time_A = "")
        {
            if (m_clientSocket != null)
            {
                String queryTime = "";
                if (time_A == "")
                    queryTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
                else
                    queryTime = time_A;

                m_clientSocket.reqHistoricalData(++m_reqId, contrast_A, queryTime, dayToHis_A, resBar_A, infoType_A, 1, 1, false, null);
            }
        }

        public void GetHistorySymbolDataTickByTick(Contract contrast_A, string st_time_A, string end_time_A, int maxTicks_A,string whatToShow_A)
        {
            
            if(m_clientSocket != null)
            {
                m_clientSocket.reqHistoricalTicks(++m_reqId, contrast_A, st_time_A, end_time_A, maxTicks_A, whatToShow_A, 1, false, null);
            }
        }

        public void StartToScanStocksByType(double belowPrice_A,double abovePrice_A,int aboveVol_A,string scanCode_A)
        {
            ScannerSubscription scanSub = new ScannerSubscription();
            scanSub.BelowPrice = belowPrice_A;
            scanSub.AbovePrice = abovePrice_A;
            scanSub.AboveVolume = aboveVol_A;//5K
            scanSub.NumberOfRows = 50;
            scanSub.Instrument = "STK";
            scanSub.LocationCode = "STK.PINK";
            scanSub.ScanCode = scanCode_A;//MOST_ACTIVE,HOT_BY_VOLUME,TOP_PERC_GAIN,HIGH_OPT_VOLUME_PUT_CALL_RATIO
            m_clientSocket.reqScannerSubscription(++m_reqId, scanSub, "", null);

        }

        public void StartPnl()
        {
            Thread pnlT = new Thread(PnlThread);
            pnlT.Start();
        }
   

        private void PnlThread()
        {
                DateTime endOfMarket = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 09, 00, 02).AddDays(1);
                object dt = null;
                do
                {
                    dt = DateTime.Now;
                    m_clientSocket.reqPnLSingle(++m_reqId, "DUD00029", "", 265502217);
                    Thread.Sleep(1000);

                } while ((DateTime)dt < endOfMarket);
           
        }

    }
}
