using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SmartTRD.ReqId.ReqIdMng;

namespace SmartTRD.ReqId
{
    interface iReqIdMng
    {
        void InsertReqToDic(int reqId_A, ACTION_REQ_e action_A);
        ACTION_REQ_e GetActionReqFronDic(int req_A);
        void RemoveActionFromDic(int req_A);
    }
}
