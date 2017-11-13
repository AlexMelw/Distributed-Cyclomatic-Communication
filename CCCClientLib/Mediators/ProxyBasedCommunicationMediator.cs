namespace DCCClientLib.Mediators
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon.Messages;
    using Interfaces;

    public class ProxyBasedCommunicationMediator : ICommunicationMediator
    {
        public IPEndPoint ProxyEndPoint { get; set; }
        public IPAddress ClientLocalIpAddress { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }

        public Task<string> MakeRequestAsync(RequestDataMessage requestMessage)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}