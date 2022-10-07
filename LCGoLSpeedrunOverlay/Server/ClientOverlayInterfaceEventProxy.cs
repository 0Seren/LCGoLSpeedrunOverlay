using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess.Server
{
    internal class ClientOverlayInterfaceEventProxy : MarshalByRefObject
    {
        public event DisconnectedEvent Disconnected;

        public void DisconnectedProxyHandler()
        {
            if (!(Disconnected is null))
            {
                Disconnected();
            }
        }
    }
}
