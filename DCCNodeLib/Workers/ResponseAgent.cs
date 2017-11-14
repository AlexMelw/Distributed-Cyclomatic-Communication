namespace DCCNodeLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCDiscoveryService.Messages;

    public class ResponseAgent
    {
        public Task SendDiscoveryResponseAsync(DiscoveryResponseMessage responseMessage,
            IPAddress clientIpAddress, int clientListeningPort)
        {
            throw new NotImplementedException();
        }
    }
}