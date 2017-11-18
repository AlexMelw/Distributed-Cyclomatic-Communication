namespace DCCClientCLI.Facade
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DCCClientLib.Interfaces;
    using EasySharp.NHelpers.CustomExMethods;
    using Infrastructure;
    using Verbs;

    partial class Program
    {
        private static void ProcessGetXmlCommand(GetXmlVerb options)
        {
            Console.Out.WriteLine("options.FilterCondition = {0}", options.FilterCondition);
            Console.Out.WriteLine("options.OrderingCondition = {0}", options.OrderingCondition);


            IDCCClientWorker clientWorker = IoC.Resolve<IDCCXmlClientWorker>();

            ProcessOutgoingRequest(options, clientWorker);
        }

        private static void ProcessGetJsonCommand(GetJsonVerb options)
        {
            Console.Out.WriteLine("options.FilterCondition = {0}", options.FilterCondition);
            Console.Out.WriteLine("options.OrderingCondition = {0}", options.OrderingCondition);

            IDCCClientWorker clientWorker = IoC.Resolve<IDCCJsonClientWorker>();

            ProcessOutgoingRequest(options, clientWorker);

        }
    }
}