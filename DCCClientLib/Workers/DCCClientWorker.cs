namespace DCCClientLib.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Messages;
    using DCCCommon.Models;
    using Interfaces;
    using Mediators;
    using static DCCCommon.Conventions.Common;

    public abstract class DCCClientWorker : IDCCClientWorker
    {
        protected string ReceivedData;

        public ICommunicationMediator CommunicationMediator { get; set; }

        public abstract bool ValidateResponseAgainstSchema(string schemaPath);

        public void MakeRequest(string dataType, string dataFormat, string filterCondition, string orderingCondition, int discoveryTimeout)
        {
            var requestMessage = new RequestDataMessage
            {
                DataFormat = dataFormat,
                Propagation = 1,
                DataType = dataType,
                FilterCondition = filterCondition,
                OrderingCondition = orderingCondition
            };

            ReceivedData = CommunicationMediator.MakeRequest(requestMessage, discoveryTimeout);
        }

        public string GetResponse() => ReceivedData;

        //public void Dispose() => CommunicationMediator.Dispose();

        #region Client Worker Initialization

        public void Initialize()
        {
            bool discoveryIsUsed = StartupConfigManager.Default.ExistsKey(Client);
            bool proxyIsUsed = StartupConfigManager.Default.ExistsKey(Proxy);

            if (discoveryIsUsed)
            {
                ConfigureWithDiscoveryServiceSettings();
                return;
            }

            if (proxyIsUsed)
            {
                ConfigureWithProxyNodeSettings();
                return;
            }

            Console.Out.WriteLine("Cannot operate without a proper startup configuration. " +
                                  "Neither discovery service was configured, nor proxy.");
            

            Environment.Exit(1);
        }

        #region Client Worker Configuration

        private void ConfigureWithDiscoveryServiceSettings()
        {
            //IPAddress localIpAddress = StartupConfigManager.Default.GetClientLocalIpAddress();

            //if (localIpAddress == null)
            //{
            //    Console.Out.WriteLine("Local IP Address is not found in the configuration file.");
            //    Environment.Exit(1);
            //}

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default.GetDiscoveryClientMulticastIPEndPoint();

            if (multicastIpEndPoint == null)
            {
                Console.Out.WriteLine("Multicast IP Address and port are not found in the configuration file.");
                Environment.Exit(1);
            }

            int responseTcpPort = StartupConfigManager.Default.GetDiscoveryClientResponseTcpPort();

            if (responseTcpPort == -1)
            {
                Console.Out.WriteLine("Discovery response TCP port is not indicated within configuration file.");
                Environment.Exit(1);
            }

            CommunicationMediator = new DiscoveryBasedCommunicationMediator
            {
                MulticastIPEndPoint = multicastIpEndPoint
                //ClientReceiveResponseTcpPort = responseTcpPort
            };
        }

        private void ConfigureWithProxyNodeSettings()
        {
            //IPAddress localIpAddress = StartupConfigManager.Default.GetClientLocalIpAddress();

            //if (localIpAddress == null)
            //{
            //    Console.Out.WriteLine("Local IP Address is not found in the configuration file.");
            //    Environment.Exit(1);
            //}

            int responseTcpPort = StartupConfigManager.Default.GetDiscoveryClientResponseTcpPort();

            if (responseTcpPort == -1)
            {
                Console.Out.WriteLine("Discovery response TCP port is not indicated within configuration file.");
                Environment.Exit(1);
            }

            IPEndPoint proxyEndPoint = StartupConfigManager.Default.GetProxyEndPoint();

            if (proxyEndPoint == null)
            {
                Console.Out.WriteLine("Proxy IP Address and/or port are not found in the configuration file.");
                Environment.Exit(1);
            }

            IEnumerable<NodeInfo> nodesIDsWithAdjacentNodesNo = StartupConfigManager.Default.GetNodesIDsWithAdjacentNodesNo();

            CommunicationMediator = new ProxyBasedCommunicationMediator
            {
                ClientReceiveResponseTcpPort = responseTcpPort,
                ProxyEndPoint = proxyEndPoint,
                NodeIdRangList = nodesIDsWithAdjacentNodesNo
            };
        }

        #endregion

        #endregion

        #region Not Used Properties

        //public IPAddress LocalIpAddress { get; set; }
        //public int ResponseTcpPort { get; set; }
        //public IPEndPoint MulticastIPEndPoint { get; set; }

        #endregion
    }
}