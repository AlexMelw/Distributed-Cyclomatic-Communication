namespace DCCClientLib.Mediators
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Messages;
    using Interfaces;

    public class ProxyBasedCommunicationMediator : ICommunicationMediator
    {
        public IPEndPoint ProxyEndPoint { get; set; }
        public IPAddress ClientLocalIpAddress { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }

        public async Task<string> MakeRequestAsync(RequestDataMessage requestMessage)
        {
            throw new NotImplementedException();

            // Find the maven node
            IPEndPoint mavenEndPoint = default;

            // Retrieve data from the maven node
            var dataAgent = new DataAgent();

            // Retrieve data from the maven node
            string data = await dataAgent.MakeRequestAsync(requestMessage, mavenEndPoint)
                .ConfigureAwait(false);

            return data;

        }
    }
}