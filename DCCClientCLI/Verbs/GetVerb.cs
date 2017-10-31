﻿namespace DCCClientCLI.Verbs
{
    using CommandLine;

    abstract class GetVerb
    {
        [Option('t', "data-type", Required = true,
            HelpText = "Specify the type to be retrieved.")]
        public string DataType { get; set; }

        [Option('f', "filter", Required = false,
            HelpText = "Indicate filter condition for data selection.")]
        public string FilterCondition { get; set; }

        [Option('o', "order-by", Required = false,
            HelpText = "Indicate key/keys to apply ordering by.")]
        public string OrderingCondition { get; set; }

        [Option('s', "schema", Required = false,
            HelpText = "Schema file to validate data against.")]
        public string SchemaPath { get; set; }
    }
}