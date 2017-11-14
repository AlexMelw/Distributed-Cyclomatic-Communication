namespace DCCNodeCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using DCCNodeLib.Interfaces;
    using Infrastructure;
    using Options;

    partial class Program
    {
        private static void RunNodeWithSpecifiedId(IdVerb options)
        {
            Task.Run((Action) (async () =>
            {
                var nodeWorker = IoC.Resolve<IDCCNodeWorker>();

                await nodeWorker.InitAsync(options.Id).ConfigureAwait(false);

                await nodeWorker.StartAsync().ConfigureAwait(false);
            })).Wait();
        }
    }
}