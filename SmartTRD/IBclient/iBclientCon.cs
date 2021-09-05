using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBApi;

namespace SmartTRD.IBclient
{
    class iBclientCon : iBclient
    {
        EClientSocket m_clientSocket;
        EReaderSignal m_readerSignal;
        EWrapperImpl m_testImpl = null;
        Int32 m_reqId;

        //! [socket_init]
        public iBclientCon()
        {
            m_reqId = 0;
            m_clientSocket = null;
            m_readerSignal = null;
            m_testImpl = null;
          
        }

        public void Init()
        {
            m_testImpl = new EWrapperImpl();
        }

        public bool connectToIbClientTWS(string ip_A, int port_A, int clientId_A)
        {
             m_clientSocket = m_testImpl.ClientSocket;
             m_readerSignal = m_testImpl.Signal;

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
            catch(Exception)
            {
                return false;
            }

            //! [cashcfd]
            Contract contract = new Contract();
            contract.Symbol = "PBYA";
            contract.SecType = "STK";
            contract.Currency = "USD";
            contract.PrimaryExch = "PINK";
            contract.Exchange = "SMART";


            //Specify the Primary Exchange attribute to avoid contract ambiguity
            // (there is an ambiguity because there is also a MSFT contract with primary exchange = "AEB")
            //! [stkcontractwithprimary]
            //! [cashcfd]
           
            //m_clientSocket.reqMarketDataType(2);
            //m_clientSocket.reqMktData(111, contract, string.Empty, false, false, null);
            //clientSocket.reqHistogramData(1111, contract, false, "3 day");
            String queryTime = DateTime.Now.AddDays(-1).ToString("yyyyMMdd HH:mm:ss");
            //m_clientSocket.reqHistoricalData(4002, contract, queryTime, "7 D", "1 min", "TRADES", 1, 1, false, null);
            //Scan


            ScannerSubscription scanSub = new ScannerSubscription();
            scanSub.BelowPrice = 1;
            scanSub.Instrument = "STK";
            scanSub.LocationCode = "STK.US";
            scanSub.ScanCode = "HOT_BY_VOLUME";//MOST_ACTIVE
            //! [highoptvolume]
            m_clientSocket.reqScannerSubscription(7001, scanSub, "", null);
            Thread.Sleep(1000);
            m_clientSocket.reqScannerSubscription(7002, scanSub, "", null);


            
           



            return true;
      
        }

        void GetSymbolDetails(string symbol_A)
        {
            if(m_clientSocket != null)
            {
                //EWrapperImpl::symbolSamples
                m_clientSocket.reqMatchingSymbols(++m_reqId, symbol_A);
            }
        }

        void GetContransDetails(Contract contranst_A)
        {

                if (m_clientSocket != null)
                {
                    //EWrapperImpl::contractDetails
                    m_clientSocket.reqContractDetails(++m_reqId, contranst_A);
                }
        }
        void GetHistorySymbolData(Contract contrast_A,int dayBac_A,int dayToHis_A,string infoType_A)
        {
            if (m_clientSocket != null)
            {
                String queryTime = DateTime.Now.AddDays(dayBac_A).ToString("yyyyMMdd HH:mm:ss");
                m_clientSocket.reqHistoricalData(++m_reqId, contrast_A, queryTime, dayToHis_A +" D", "1 min", infoType_A, 1, 1, false, null);
            }
        }

    }
}
