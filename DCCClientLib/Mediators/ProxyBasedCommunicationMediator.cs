namespace DCCClientLib.Mediators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Agents;
    using DCCCommon.Messages;
    using DCCCommon.Models;
    using Interfaces;

    public class ProxyBasedCommunicationMediator : ICommunicationMediator
    {
        private IPAddress _clientLocalIpAddress;
        public IPEndPoint ProxyEndPoint { get; set; }
        public int ClientReceiveResponseTcpPort { get; set; }
        public IEnumerable<NodeInfo> NodeIdRangList { get; set; }


        public string MakeRequest(RequestDataMessage requestMessage, int discoveryTimeout)
        {
            //_clientLocalIpAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();

            //IPEndPoint mavenEndPoint = GetMavenEndPoint();

            //// Retrieve data from the maven node
            //var dataAgent = new DataAgent();

            //// Retrieve data from the maven node
            //string data = dataAgent.MakeRequest(requestMessage, mavenEndPoint, "SECRET");

            //return data;

            return default; // $C$ to be reviewed
        }


    }
}