namespace DCCClientCLI.Facade
{
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
            Parser.Default.ParseArguments<GetXmlVerb, GetJsonVerb>(args)
                .WithParsed<GetXmlVerb>(ProcessGetXmlCommand)
                .WithParsed<GetJsonVerb>(ProcessGetJsonCommand);
        }
    }
}