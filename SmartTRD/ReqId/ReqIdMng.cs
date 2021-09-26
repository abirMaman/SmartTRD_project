using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private Dictionary<int, ACTION_REQ_e> m_reqMngDic;
        private static ReqIdMng m_instanse;

        public ReqIdMng()
        {
            m_reqMngDic = null;
            m_instanse = this;
        }

        public static ReqIdMng GetInstanse()
        {
            return m_instanse;
        }

        public void Init()
        {
            m_reqMngDic = new Dictionary<int, ACTION_REQ_e>();
        }

        public void InsertReqToDic(int reqId_A,ACTION_REQ_e action_A)
        {
            m_reqMngDic[reqId_A] = action_A;
        }

        public ACTION_REQ_e GetActionReqFronDic(int req_A)
        {
            ACTION_REQ_e retVal = ACTION_REQ_e.ACTION_REQ_NONE;
            if (m_reqMngDic != null)
            {
                if (m_reqMngDic.ContainsKey(req_A) == true)
                {
                    m_reqMngDic.TryGetValue(req_A, out retVal);
                }
            }

            return retVal;
        }

        public void RemoveActionFromDic(int req_A)
        {
            if (m_reqMngDic != null)
            {
                if (m_reqMngDic.ContainsKey(req_A) == true)
                    m_reqMngDic.Remove(req_A);
            }
        }

         

    }
}
