namespace DCCClientCLI.Facade
{
    using System;
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
                        string nL = Environment.NewLine;
                        Console.Out.WriteLine($"Received {dataType} is VALID!{nL}");
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
                // If do not terminate App manually, then end user would have to do it by pressing ^C (CTRL+C).
                Environment.Exit(0);
            }
        }
    }
}