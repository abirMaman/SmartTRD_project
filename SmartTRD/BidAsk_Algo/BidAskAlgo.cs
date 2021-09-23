using SmartTRD.IBclient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTRD.BidAsk_Algo
{
    class BidAskAlgo
    {
        struct BID_ASK_REQ_ID_s
        {
            int bid_req_id;
            int ask_req_id;
        }

        private iBidAskAlgo m_bidAskDb;
        private iBclient m_clientP;
        private string m_symbol;

        public  BidAskAlgo()
        {
            m_clientP = null;
            m_bidAskDb = null;
            m_symbol = "";
        }

        public void Init(string Symbol_A)
        {
            m_symbol = Symbol_A;
            m_clientP = BclientCon.GetInstase();
        }

        public void StartAskBidAlgo()
        {
            Thread stAlgoT = new Thread(StartAlgoThread);
            stAlgoT.Start();
        }

        private void StartAlgoThread()
        {
            //===== Step 1 get contract details ======
            m_clientP.GetSymbolDetails(m_symbol);
            //while()
        }

        private void HanleGui()
        {

        }

    }
}
