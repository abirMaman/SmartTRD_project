using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTRD.ReqId
{
    class ReqIdMng:iReqIdMng
    {
        public enum ACTION_REQ_e
        {
            ACTION_REQ_NONE = 0,
            ACTION_REQ_SCANNER = 1,
            ACTION_REQ_ASK_BID_ALGO = 2
        }

        public struct ACTION_REC_INFO_s
        {
            public ACTION_REQ_e action;
            public string codeAction;

        }

        private Dictionary<int, ACTION_REC_INFO_s> m_reqMngDic;
        private static ReqIdMng m_instanse;

        private Mutex m_mutex;

        public ReqIdMng()
        {
            m_reqMngDic = null;
            m_mutex = null;
            m_instanse = this;
        }

        public static ReqIdMng GetInstanse()
        {
            return m_instanse;
        }

        public void Init()
        {
            m_reqMngDic = new Dictionary<int, ACTION_REC_INFO_s>();
            m_mutex = new Mutex();
        }

        public void InsertReqToDic(int reqId_A,ACTION_REQ_e action_A,string codeAction_A ="")
        {
            m_mutex.WaitOne();
            ACTION_REC_INFO_s actionS;
            actionS.action = action_A;
            actionS.codeAction = codeAction_A;
            m_reqMngDic[reqId_A] = actionS;
            m_mutex.ReleaseMutex();
        }

        public ACTION_REQ_e GetActionReqFronDic(int req_A)
        {
            ACTION_REQ_e retVal = ACTION_REQ_e.ACTION_REQ_NONE;
            ACTION_REC_INFO_s reqInfo;
            if (m_reqMngDic != null)
            {
                if (m_reqMngDic.ContainsKey(req_A) == true)
                {
                    m_reqMngDic.TryGetValue(req_A, out reqInfo);
                    retVal = reqInfo.action;
                }
            }

            return retVal;
        }

        public ACTION_REC_INFO_s GetActionReqInfoFronDic(int req_A)
        {
            ACTION_REC_INFO_s reqInfo = new ACTION_REC_INFO_s();

            if (m_reqMngDic != null)
            {
                if (m_reqMngDic.ContainsKey(req_A) == true)
                {
                    m_reqMngDic.TryGetValue(req_A, out reqInfo);
                }
            }

            return reqInfo;
        }

        public void RemoveActionFromDic(int req_A)
        {
            if (m_reqMngDic != null)
            {
                if (m_reqMngDic.ContainsKey(req_A) == true)
                {
                    m_mutex.WaitOne();
                    m_reqMngDic.Remove(req_A);
                    m_mutex.ReleaseMutex();
                }
            }
        }

         

    }
}
