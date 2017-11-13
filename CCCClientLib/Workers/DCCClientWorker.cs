namespace DCCClientLib.Workers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DCCCommon;
    using DCCCommon.Messages;
    using Interfaces;
    using Mediators;
    using static DCCCommon.Conventions.Common;

    public abstract class DCCClientWorker : IDCCClientWorker
    {
        protected string ReceivedData;

        public ICommunicationMediator CommunicationMediator { get; set; }

        public abstract Task<bool> ValidateResponseAgainstSchemaAsync(string schemaPath);

        public async Task MakeRequestAsync(string dataType, string filterCondition, string orderingCondition)
        {
            var requestMessage = new RequestDataMessage
            {
                Propagation = 1,
                DataType = dataType,
                FilterCondition = filterCondition,
                OrderingCondition = orderingCondition
            };

            ReceivedData = await CommunicationMediator.MakeRequestAsync(requestMessage).ConfigureAwait(false);
        }

        public Task<string> GetResponseAsync() => Task.FromResult(ReceivedData);
        public void Dispose() => CommunicationMediator.Dispose();

        #region Client Worker Initialization

        public async Task InitializeAsync()
        {
            bool discoveryIsUsed = StartupConfigManager.Default.ExistsKey(Client);
            bool proxyIsUsed = StartupConfigManager.Default.ExistsKey(Proxy);

            if (discoveryIsUsed)
            {
                await ConfigureWithDiscoveryServiceSettingsAsync().ConfigureAwait(false);

                return;
            }

            if (proxyIsUsed)
            {
                await ConfigureWithProxyNodeSettingsAsync().ConfigureAwait(false);

                return;
            }

            await Console.Out
                .WriteLineAsync(
                    "Cannot operate without a proper startup configuration. " +
                    "Neither discovery service was configured, nor proxy.")
                .ConfigureAwait(false);

            Environment.Exit(1);
        }

        #region Client Worker Configuration

        private async Task ConfigureWithDiscoveryServiceSettingsAsync()
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

            CommunicationMediator = new DiscoveryBasedCommunicationMediator
            {
                ClientLocalIpAddress = localIpAddress,
                MulticastIPEndPoint = multicastIpEndPoint,
                ClientReceiveResponseTcpPort = responseTcpPort
            };
        }

        private async Task ConfigureWithProxyNodeSettingsAsync()
        {
            IPAddress localIpAddress = StartupConfigManager.Default.GetClientLocalIpAddress();

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
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

            IPEndPoint proxyEndPoint = StartupConfigManager.Default.GetProxyEndPoint();

            if (proxyEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Proxy IP Address and/or port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }


            CommunicationMediator = new ProxyBasedCommunicationMediator
            {
                ClientLocalIpAddress = localIpAddress,
                ClientReceiveResponseTcpPort = responseTcpPort,
                ProxyEndPoint = proxyEndPoint
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