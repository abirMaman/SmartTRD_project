using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.Algo
{
    class AlgoTrd
    {
        List<ContractDescription> BuildDBOfStockFromApi(ContractDescription[] contractDescriptions_A)
        {
            List<ContractDescription> buildList = new List<ContractDescription>();
            foreach (ContractDescription con in contractDescriptions_A)
            {
                if(con.Contract.SecType == "STK" &&
                   con.Contract.Currency == "USD" &&
                   con.Contract.PrimaryExch.Contains("PINK"))
                    {
                      
                    }
            }

            return buildList;
        }
    }
}
