namespace DCCClientCLI.Facade
{
    using System;
    using System.Diagnostics;
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
            args = new[] { "get-xml", "-t", "Employee" };

            Parser.Default.ParseArguments<GetXmlVerb, GetJsonVerb>(args)
                .WithParsed<GetXmlVerb>(ProcessGetXmlCommand)
                .WithParsed<GetJsonVerb>(ProcessGetJsonCommand);

            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }
    }
}