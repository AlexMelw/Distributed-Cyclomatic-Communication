namespace DCCClientLib.Mediators
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon.Messages;
    using Interfaces;

    public class DiscoveryBasedCommunicationMediator : ICommunicationMediator
    {
        public IPAddress ClientLocalIpAddress { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }

        public Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            throw new NotImplementedException();
        }

        public Task<string> MakeRequestAsync(RequestDataMessage requestMessage)
        {
            throw new NotImplementedException();
        }
    }
}