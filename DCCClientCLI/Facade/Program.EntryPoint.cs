namespace DCCClientCLI.Facade
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using CommandLine;
    using Infrastructure;
    using Verbs;

    partial class Program
    {
        #region CONSTRUCTORS

        static Program() => IoC.RegisterAll();

        #endregion

        static void Main(string[] args)
        {
            Console.Title = "DIS Client";

            args = new[] { "get-json", "-t", "Employee", "-f", "Id > 10 and Id <= 20", "-o", "Id descending" };

            Parser.Default.ParseArguments<GetXmlVerb, GetJsonVerb>(args)
                .WithParsed<GetXmlVerb>(ProcessGetXmlCommand)
                .WithParsed<GetJsonVerb>(ProcessGetJsonCommand);

            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }
    }
}