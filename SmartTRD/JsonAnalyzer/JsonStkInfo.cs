using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.JsonAnalyzer
{
    public class JsonStkInfo
    {

        private JObject m_jsonObject;

        public JsonStkInfo(string canToJson_A)
        {
            m_jsonObject = JObject.Parse(canToJson_A);
        }

        public bool StkIsPink()
        {
            bool retAnswer = false;

            if(m_jsonObject != null)
            {
                var secFromJson = m_jsonObject["securities"][0];

                retAnswer = bool.Parse(secFromJson["isPinkSheets"].ToString());

                

            }

            return retAnswer;
        }

        public bool StkisOTCQC()
        {      
            bool retAnswer = false;

            if (m_jsonObject != null)
            {
                try
                {
                    string otcQX = m_jsonObject["legalCounsels"][0]["roles"][1].ToString();

                    if (otcQX.Contains("OTCQX"))
                    {
                        retAnswer = true;
                    }
                }
                catch(Exception e)
                {

                }
                  
            }

            return retAnswer;
        }

        public bool StkAsVerfiedProfile(out string vpData_A)
        {
            vpData_A = "";
            bool retAnswer = false;

            if (m_jsonObject != null)
            {
                retAnswer = bool.Parse(m_jsonObject["isProfileVerified"].ToString());

                if (retAnswer)
                {
                    vpData_A = m_jsonObject["profileVerifiedAsOfDate"].ToString();
                }

            }

            return retAnswer;
        }

        public bool StkAsPennyStkExempt()
        {
            return true;
        }
    
        public bool StkAsTransferAgent()
        {
            bool retAnswer = false;

            if (m_jsonObject != null)
            {
                try
                {
                    var tranAgent = m_jsonObject["securities"][0]["transferAgents"][0];

                    int id =  int.Parse(tranAgent["id"].ToString());

                    if (id > 0)
                        retAnswer = true;

                }
                catch (Exception e)
                {

                }

            }
            return retAnswer;
        }

        public int GetStkUnrestricted()
        {
            int retAnswer = 0;

            if (m_jsonObject != null)
            {
                var secFromJson = m_jsonObject["securities"][0];

                try

                {
                    retAnswer = int.Parse(secFromJson["unrestrictedShares"].ToString());
                }
                catch(Exception e) { }
            }

            return retAnswer;
        }

        public int GetStkAutorizedShare()
        {
            int retAnswer = 0;

            if (m_jsonObject != null)
            {
                var secFromJson = m_jsonObject["securities"][0];

                try

                {
                    retAnswer = int.Parse(secFromJson["authorizedShares"].ToString());
                }
                catch (Exception e) { }
            }

            return retAnswer;
        }
        public int GetStkOutstandingShare()
        {
            int retAnswer = 0;

            if (m_jsonObject != null)
            {
                var secFromJson = m_jsonObject["securities"][0];

                try

                {
                    retAnswer = int.Parse(secFromJson["outstandingShares"].ToString());
                }
                catch (Exception e) { }
            }

            return retAnswer;
        }
    }

}


