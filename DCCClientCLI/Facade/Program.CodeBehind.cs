namespace DCCClientCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using DCCClientLib.Interfaces;
    using Verbs;

    partial class Program
    {
        private static void ProcessOutgoingRequest(GetVerb options, IDCCClientWorker clientWorker)
        {
            try
            {
                clientWorker.Initialize();

                clientWorker.MakeRequest(
                    options.DataType,
                    options.DataFormat.ToString(),
                    options.FilterCondition,
                    options.OrderingCondition);

                bool valid = true;

                if (!string.IsNullOrWhiteSpace(options.SchemaPath))
                {
                    valid = clientWorker.ValidateResponseAgainstSchema(options.SchemaPath);
                }

                if (valid)
                {
                    string response = clientWorker.GetResponse();
                    Console.Out.WriteLine(response);
                }
                else
                {
                    string dataType = options.DataFormat.ToString();
                    Console.Out.WriteLine($"Received {dataType} is NOT valid!");
                }
            }
            finally
            {
                //clientWorker.Dispose();
            }
        }
    }
}