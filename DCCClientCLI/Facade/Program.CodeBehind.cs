namespace DCCClientCLI.Facade
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCClientLib.Interfaces;
    using DCCCommon;
    using Verbs;
    using static DCCCommon.Conventions.Common;

    partial class Program
    {
        private static async Task ProcessOutgoingRequestAsync(GetVerb options, IDCCClientWorker clientWorker)
        {
            try
            {
                await InitializeClientWorkerAsync(clientWorker).ConfigureAwait(false);

                await clientWorker.MakeRequestAsync(
                        options.DataType,
                        options.FilterCondition,
                        options.OrderingCondition)
                    .ConfigureAwait(false);

                bool valid = true;

                if (!string.IsNullOrWhiteSpace(options.SchemaPath))
                {
                    valid = await clientWorker
                        .ValidateResponseAgainstSchemaAsync(options.SchemaPath)
                        .ConfigureAwait(false);
                }

                if (valid)
                {
                    string response = await clientWorker.GetResponseAsync().ConfigureAwait(false);
                    await Console.Out.WriteLineAsync(response).ConfigureAwait(false);
                }
                else
                {
                    string dataType = options.VerbType == VerbType.JsonVerb ? "Json" : "Xml";
                    await Console.Out.WriteLineAsync($"Received {dataType} is NOT valid!").ConfigureAwait(false);
                }
            }
            finally
            {
                clientWorker.Dispose();
            }
        }

        private static async Task InitializeClientWorkerAsync(IDCCClientWorker clientWorker)
        {
            bool discoveryIsUsed = StartupConfigManager.Default.ExistsKey(Client);
            bool proxyIsUsed = StartupConfigManager.Default.ExistsKey(Proxy);

            if (discoveryIsUsed)
            {
                await ConfigureWithDiscoveryServiceSettingsAsync(clientWorker).ConfigureAwait(false);

                return;
            }

            if (proxyIsUsed)
            {
                await ConfigureWithProxyNodeSettings(clientWorker).ConfigureAwait(false);

                return;
            }

            await Console.Out
                .WriteLineAsync(
                    "Cannot operate without a proper startup configuration. " +
                    "Neither discovery service was configured, nor proxy.")
                .ConfigureAwait(false);

            Environment.Exit(1);
        }

        private static async Task ConfigureWithDiscoveryServiceSettingsAsync(IDCCClientWorker clientWorker)
        {
            IPAddress localIpAddress = StartupConfigManager.Default.GetClientLocalIpAddress();

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default.GetDiscoveryClientMulticastIPEndPoint();

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            int responseTcpPort = StartupConfigManager.Default.GetDiscoveryClientResponseTcpPort();

            if (responseTcpPort == -1)
            {
                await Console.Out
                    .WriteLineAsync("Discovery response TCP port is not indicated within configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            clientWorker.ResponseTcpPort = responseTcpPort;
            clientWorker.LocalIpAddress = localIpAddress;
            clientWorker.MulticastIPEndPoint = multicastIpEndPoint;
        }

        private static Task ConfigureWithProxyNodeSettings(IDCCClientWorker clientWorker)
        {
            throw new NotImplementedException();
        }
    }
}