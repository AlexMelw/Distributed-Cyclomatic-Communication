namespace DCCNodeCLI.Facade
{
    using CommandLine;
    using Infrastructure;
    using Options;

    partial class Program
    {
        #region CONSTRUCTORS

        static Program() => IoC.RegisterAll();

        #endregion

        static void Main(string[] args)
        {
            //args = new[] { "-i", "6" }; // The 6th node is going to be run

            Parser.Default.ParseArguments<NodeOptions>(args).WithParsed(RunNodeWithSpecifiedId);
        }
    }
}