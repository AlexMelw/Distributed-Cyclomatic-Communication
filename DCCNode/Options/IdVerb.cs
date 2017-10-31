namespace DCCNodeCLI.Options
{
    using CommandLine;

    [Verb("id", HelpText =
        "Sets node's id in order to allow it retrieve initial configuration information from StartupConfig.xml")]
    class IdVerb
    {
        [Value(0)]
        public int Id { get; set; }
    }
}