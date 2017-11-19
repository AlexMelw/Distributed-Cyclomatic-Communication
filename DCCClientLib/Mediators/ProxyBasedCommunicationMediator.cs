namespace DCCClientLib.Mediators
{
    using System.Net;
    using DCCCommon.Agents;
    using DCCCommon.Messages;
    using Interfaces;

    public class ProxyBasedCommunicationMediator : ICommunicationMediator
    {
        public IPEndPoint ProxyEndPoint { get; set; }

        public string MakeRequest(RequestDataMessage requestMessage, int discoveryTimeout)
        {
            var dataAgent = new DataAgent();

            string data = dataAgent.MakeRequest(requestMessage, ProxyEndPoint, "SECRET");

            return data;
        }
    }
}