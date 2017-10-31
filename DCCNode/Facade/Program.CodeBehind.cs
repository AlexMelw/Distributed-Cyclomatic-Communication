namespace DCCNodeCLI.Facade
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Conventions;
    using DCCNodeLib.Interfaces;

    partial class Program
    {
        private static async Task InitializeNodeWorkerAsync(IDCCNodeWorker nodeWorker, int id)
        {
            IPAddress localIpAddress = StartupConfigManager.Default
                .GetLocalIpAddress(Common.Node, Common.LocalIpAddress, id);

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetMulticastIPEndPoint(Common.Node, id);

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            nodeWorker.LocalIpAddress = localIpAddress;
            nodeWorker.MulticastIPEndPoint = multicastIpEndPoint;
        }
    }
}