namespace DCCProxy
{
    using System;
    using System.Net;
    using System.Threading;
    using DCCCommon;
    using DCCCommon.Agents;

    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new Proxy();

            proxy.StartServingTcpPort();
        }


    }

    class Proxy
    {
        public void StartServingTcpPort()
        {
            new Thread(() =>
            {
                while (true)
                {
                    // somehow accept socket

                    // then 
                    new Thread(() =>
                    {
                        int max = NodeIdRangList.Max(nodeInfo => nodeInfo.AdjacentNodesNo);

                        int mavenNodeId = NodeIdRangList
                            .FirstOrDefault(nodeInfo => nodeInfo.AdjacentNodesNo == max).Id;

                        IPEndPoint mavenEndPoint = StartupConfigManager.Default
                            .GetNodeIPEndPointById(mavenNodeId);

                        // Retrieve data from the maven node
                        var dataAgent = new DataAgent();

                        // Retrieve data from the maven node
                        string data = dataAgent.MakeRequest(requestMessage, mavenEndPoint, mavenNodeId.ToString());
                    }).Start();
                }
              
            }).Start();
        }
    }
}