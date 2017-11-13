namespace DCCNodeLib.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using Interfaces;

    public class DCCNodeWorker : IDCCNodeWorker
    {
        public IPAddress LocalIpAddress { get; set; }
        public IPEndPoint MulticastIPEndPoint { get; set; }
        public int TcpServingPort { get; set; }
        public IEnumerable<IPEndPoint> AdjacentNodesEndPoints { get; set; }

        public Task StartAsync()
        {
            //StartListeningToMulticastPortAsync();
            //StartListeningToTcpServingPortAsync();

            throw new NotImplementedException();
        }

        public async Task InitAsync(int nodeId)
        {
            IPAddress localIpAddress = StartupConfigManager.Default
                .GetNodeLocalIpAddress(nodeId);

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetNodeMulticastIPEndPoint(nodeId);

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            int tcpServingPort = StartupConfigManager.Default
                .GetNodeTcpServingPort(nodeId);

            if (tcpServingPort == -1)
            {
                await Console.Out
                    .WriteLineAsync("TCP serving port is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IEnumerable<IPEndPoint> adjacentNodesEndPoints = StartupConfigManager.Default
                .GetAdjacentNodesEndPoints(nodeId);


            LocalIpAddress = localIpAddress;
            MulticastIPEndPoint = multicastIpEndPoint;
            TcpServingPort = tcpServingPort;
            AdjacentNodesEndPoints = adjacentNodesEndPoints;
        }

        private Task StartListeningToMulticastPortAsync()
        {
            //while (true)
            //{

            //}

            throw new NotImplementedException();
        }

        private Task StartListeningToTcpServingPortAsync()
        {
            //while (true)
            //{

            //}

            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}