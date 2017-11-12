namespace DCCClientCLI.Facade
{
    using System;
    using System.Threading.Tasks;
    using DCCClientLib.Interfaces;
    using EasySharp.NHelpers.CustomExMethods;
    using Infrastructure;
    using Verbs;

    partial class Program
    {
        private static void ProcessGetXmlCommand(GetXmlVerb options)
        {
            Task.Run((Action) (async () =>
            {
                IDCCClientWorker clientWorker = IoC.Resolve<IDCCXmlClientWorker>();

                await ProcessOutgoingRequestAsync(options, clientWorker).ConfigureAwait(false);
            })).Wait();
        }

        private static void ProcessGetJsonCommand(GetJsonVerb options)
        {
            Task.Run((Action) (async () =>
            {
                IDCCClientWorker clientWorker = IoC.Resolve<IDCCJsonClientWorker>();

                await ProcessOutgoingRequestAsync(options, clientWorker).ConfigureAwait(false);
            })).Wait();
        }
    }
}