namespace DCCClientLib.Mediators
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Interfaces;

    public class ProxyBasedCommunicationMediator : ICommunicationMediator
    {
        public IPEndPoint ProxyEndPoint { get; set; }
        public IPAddress ClientLocalIpAddress { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }

        public Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            throw new NotImplementedException();
        }
    }
}