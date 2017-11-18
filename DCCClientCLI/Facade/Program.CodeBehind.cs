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
                    options.OrderingCondition,
                    options.Timeout);

                bool valid = true;

                if (!string.IsNullOrWhiteSpace(options.SchemaPath))
                {
                    valid = clientWorker.ValidateResponseAgainstSchema(options.SchemaPath);

                    string dataType = options.DataFormat.ToString();

                    if (!valid)
                    {
                        Console.Out.WriteLine($"Received {dataType} is NOT valid!");
                        return;

                    }
                    else
                    {
                        Console.Out.WriteLine($"Received {dataType} is VALID!");
                    }

                }

                string response = clientWorker.GetResponse();
                Console.Out.WriteLine(response);
            }
            catch (Exception)
            {
                Console.Out.WriteLine("An error occurred while retrieving data...");
                Console.Out.WriteLine("Application terminated.");
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}