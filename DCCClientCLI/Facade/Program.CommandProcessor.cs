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
            Task.Run((Action) (() =>
            {
                var clientWorker = IoC.Resolve<IDCCXmlClientWorker>();

                ProcessOutgoingRequestAsync(options, clientWorker).ConfigureAwait(false);

            })).Wait();
        }

        private static void ProcessGetJsonCommand(GetJsonVerb options)
        {
            Task.Run((Action)(() =>
            {
                var clientWorker = IoC.Resolve<IDCCXmlClientWorker>();

                ProcessOutgoingRequestAsync(options, clientWorker).ConfigureAwait(false);

            })).Wait();

        }
    }
}