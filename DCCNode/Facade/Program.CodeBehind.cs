namespace DCCNodeCLI.Facade
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Conventions;
    using DCCNodeLib.Interfaces;
    using static DCCCommon.Conventions.Common;

    partial class Program
    {
        private static async Task InitializeNodeWorkerAsync(IDCCNodeWorker nodeWorker, int id)
        {
            IPAddress localIpAddress = StartupConfigManager.Default
                .GetLocalIpAddress(Node, LocalIpAddress, id);

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetMulticastIPEndPoint(Node, id);

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            int tcpServingPort = StartupConfigManager.Default
                .GetTcpServingPort(Common.Node, Common.TcpServingPort, id);

            if (tcpServingPort == -1)
            {
                await Console.Out
                    .WriteLineAsync("TCP serving port is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            nodeWorker.TcpServingPort = tcpServingPort;
            nodeWorker.LocalIpAddress = localIpAddress;
            nodeWorker.MulticastIPEndPoint = multicastIpEndPoint;
        }
    }
}