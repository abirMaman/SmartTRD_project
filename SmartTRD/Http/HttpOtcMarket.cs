using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.Http
{
    class HttpOtcMarket
    {
        private static string m_otcMarket = "https://backend.otcmarkets.com/otcapi/company/profile/full/";
        public static string GetStrFromOtcMarket(string symbol_A)
        {
            string returnInfo = "";
            string urlOfSymbol = m_otcMarket + symbol_A;

            HttpWebRequest req = WebRequest.Create(urlOfSymbol) as HttpWebRequest;
            req.Method = WebRequestMethods.Http.Get;
            req.Accept = "application/json, text/plain, */*";
            req.Headers["Accept-Language"] = "en-US,en;q=0.9";
            req.Host = "backend.otcmarkets.com";
            req.Headers["Origin"] = "https://www.otcmarkets.com";
            req.Referer = "https://www.otcmarkets.com/";
            req.Headers["sec-ch-ua"] = "Chromium;v='93',' Not A;Brand';v='99',Google Chrome;v='93'";
            req.Headers["sec-ch-ua-mobile"] = "?0";
            req.Headers["Sec-Fetch-Dest"] = "empty";
            req.Headers["Sec-Fetch-Mode"] = "cors";
            req.Headers["Sec-Fetch-Site"] = "same-site";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";

            try
            {
                HttpWebResponse response = req.GetResponse() as HttpWebResponse;

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    returnInfo = reader.ReadToEnd();
                 
                }
            }
            catch(Exception e)
            {

            }

            return returnInfo;

        }
    }
}
