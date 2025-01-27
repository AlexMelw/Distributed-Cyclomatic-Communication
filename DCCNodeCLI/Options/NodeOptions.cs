﻿namespace DCCNodeCLI.Options
{
    using CommandLine;

    class NodeOptions
    {
        [Option('i', "id", HelpText =
            "Sets node's id in order to allow it to retrieve initial configuration information from StartupConfig.xml")]
        public int Id { get; set; }
    }
}