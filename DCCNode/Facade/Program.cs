namespace DCCNodeCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using CommandLine;
    using DCCNodeLib.Interfaces;
    using Infrastructure;
    using Options;

    partial class Program
    {
        #region CONSTRUCTORS

        static Program() => IoC.RegisterAll();

        #endregion

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<IdVerb>(args)
                .WithParsed(options =>
                {
                    Task.Run((Action) (async () =>
                    {
                        var nodeWorker = IoC.Resolve<IDCCNodeWorker>();

                        await InitializeNodeWorkerAsync(nodeWorker, options.Id).ConfigureAwait(false);

                        await nodeWorker.StartAsync().ConfigureAwait(false);

                    })).Wait();
                });
        }
    }
}