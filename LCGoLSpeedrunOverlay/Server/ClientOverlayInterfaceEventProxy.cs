using System;

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
