namespace DCCProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using DCCCommon;
    using DCCCommon.Agents;
    using DCCCommon.Messages;
    using DCCCommon.Models;

    class Proxy
    {
        private IPEndPoint _mavenEndPoint;
        private IPAddress _localProxyIpAddress;
        private int _tcpServingPort;

        public void StartServingTcpPort()
        {
            #region Pattern Trash

            //new Thread(() =>
            //{
            //    while (true)
            //    {
            //        // somehow accept socket

            //        // then 
            //        new Thread(() =>
            //        {
            //            // Retrieve data from the maven node
            //            var dataAgent = new DataAgent();

            //            // Retrieve data from the maven node
            //            string data = dataAgent.MakeRequest(requestMessage, mavenEndPoint, mavenNodeId.ToString());
            //        }).Start();
            //    }
            //}).Start();

            #endregion

            var portListener = new ContinuousTcpPortListener();
            portListener.StartListening(_tcpServingPort, HandleRequest);
        }

        public void Init()
        {
            IPEndPoint proxyEndPoint = StartupConfigManager.Default
                .GetProxyEndPoint();

            if (proxyEndPoint == null)
            {
                Console.Out.WriteLine("Proxy configuration parameters couldn't be found in the configuration file.");
                Environment.Exit(1);
            }

            _localProxyIpAddress = proxyEndPoint.Address;
            _tcpServingPort = proxyEndPoint.Port;

            IEnumerable<NodeInfo> nodeIdRangList = StartupConfigManager.Default.GetNodesIDsWithAdjacentNodesNo();

            _mavenEndPoint = GetMavenEndPoint(nodeIdRangList);
        }

        private void HandleRequest(Socket workerSocket)
        {
            #region Get Request Data Message

            RequestDataMessage requestDataMessage = new RequestInterceptor().GetRequest(workerSocket);

            #endregion


            #region Business Logic

            var dataAgent = new DataAgent();

            // Retransmission (delegation) of data request
            string data = dataAgent.MakeRequest(requestDataMessage, _mavenEndPoint, "SECRET");

            #endregion

            #region Send Back Response Data

            dataAgent.SendResponse(workerSocket, data);

            #endregion

            workerSocket.Close();
        }

        private IPEndPoint GetMavenEndPoint(IEnumerable<NodeInfo> nodeIdRangList)
        {
            int max = nodeIdRangList.Max(nodeInfo => nodeInfo.AdjacentNodesNo);

            int mavenNodeId = nodeIdRangList
                .FirstOrDefault(nodeInfo => nodeInfo.AdjacentNodesNo == max).Id;

            IPEndPoint mavenEndPoint = StartupConfigManager.Default
                .GetNodeIPEndPointById(mavenNodeId);

            return mavenEndPoint;
        }
    }
}