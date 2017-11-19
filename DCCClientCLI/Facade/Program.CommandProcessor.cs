namespace DCCClientCLI.Facade
{
    using System;
    using DCCClientLib.Interfaces;
    using Infrastructure;
    using Verbs;

    partial class Program
    {
        private static void ProcessGetXmlCommand(GetXmlVerb options)
        {
#if DEBUG
            Console.Out.WriteLine("options.FilterCondition = {0}", options.FilterCondition);
            Console.Out.WriteLine("options.OrderingCondition = {0}", options.OrderingCondition);
#endif
            IDCCClientWorker clientWorker = IoC.Resolve<IDCCXmlClientWorker>();

            ProcessOutgoingRequest(options, clientWorker);
        }

        private static void ProcessGetJsonCommand(GetJsonVerb options)
        {
#if DEBUG
            Console.Out.WriteLine("options.FilterCondition = {0}", options.FilterCondition);
            Console.Out.WriteLine("options.OrderingCondition = {0}", options.OrderingCondition);
#endif
            IDCCClientWorker clientWorker = IoC.Resolve<IDCCJsonClientWorker>();

            ProcessOutgoingRequest(options, clientWorker);
        }
    }
}