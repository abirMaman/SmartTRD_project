using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTRD.IBclient
{
    interface iBclient
    {
        bool connectToIbClientTWS(string ip_A, int port_A, int clientId_A);
    }
}
