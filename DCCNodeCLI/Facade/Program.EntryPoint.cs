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
            //args = new[] { "-i", "6" };

            //var arguments = Parser.Default.FormatCommandLine(new NodeOptions
            //    {
            //        Id = 6
            //    }
            //);
            //Console.Out.WriteLine("arguments = {0}", arguments);

            Parser.Default.ParseArguments<NodeOptions>(args).WithParsed(RunNodeWithSpecifiedId);
        }
    }
}