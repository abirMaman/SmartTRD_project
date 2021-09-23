using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.JsonAnalyzer
{
   public  class JsonStkStmbolInfo
    {
        private JObject m_jsonObject;

        public JsonStkStmbolInfo(string canToJson_A)
        {
            m_jsonObject = JObject.Parse(canToJson_A);
        }


        public bool StkAsPennyStkExempt()
        {
            bool retAnswer = false;

            if (m_jsonObject != null)
            {
                retAnswer = bool.Parse(m_jsonObject["isPennyStockExempt"].ToString());

            }

            return retAnswer;
        }
    }
}
