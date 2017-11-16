namespace DCCNodeCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using DCCNodeLib.Interfaces;
    using Infrastructure;
    using Options;

    partial class Program
    {
        private static void RunNodeWithSpecifiedId(NodeOptions options)
        {
            Console.Title = $"DIS NODE :: ID {options.Id}";

            Task.Run((Action) (async () =>
            {
                var nodeWorker = IoC.Resolve<IDCCNodeWorker>();

                nodeWorker.Init(options.Id);

                nodeWorker.Start();
            })).Wait();
        }
    }
}