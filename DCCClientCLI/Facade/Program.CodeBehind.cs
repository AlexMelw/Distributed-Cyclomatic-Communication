namespace DCCClientCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using DCCClientLib.Interfaces;
    using Verbs;

    partial class Program
    {
        private static async Task ProcessOutgoingRequestAsync(GetVerb options, IDCCClientWorker clientWorker)
        {
            try
            {
                await clientWorker.InitializeAsync().ConfigureAwait(false);

                await clientWorker.MakeRequestAsync(
                        options.DataType,
                        options.DataFormat.ToString(),
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
                    string dataType = options.DataFormat.ToString();
                    await Console.Out.WriteLineAsync($"Received {dataType} is NOT valid!").ConfigureAwait(false);
                }
            }
            finally
            {
                //clientWorker.Dispose();
            }
        }
    }
}