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
                await InitializeClientWorker(clientWorker).ConfigureAwait(false);

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
            }
            finally
            {
                clientWorker.Dispose();
            }
        }

        private static async Task InitializeClientWorker(IDCCClientWorker clientWorker)
        {
            IPAddress localIpAddress = StartupConfigManager.Default
                .GetLocalIpAddress(Client, LocalIpAddress);

            if (localIpAddress == null)
            {
                await Console.Out
                    .WriteLineAsync("Local IP Address is not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            IPEndPoint multicastIpEndPoint = StartupConfigManager.Default
                .GetMulticastIPEndPoint(Client);

            if (multicastIpEndPoint == null)
            {
                await Console.Out
                    .WriteLineAsync("Multicast IP Address and port are not found in the configuration file.")
                    .ConfigureAwait(false);

                Environment.Exit(1);
            }

            clientWorker.LocalIpAddress = localIpAddress;
            clientWorker.MulticastIPEndPoint = multicastIpEndPoint;
        }
    }
}