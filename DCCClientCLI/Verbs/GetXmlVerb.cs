namespace DCCClientCLI.Verbs
{
    using CommandLine;

    [Verb("get-xml", HelpText = "Retrieves xml from ...")]
    class GetXmlVerb : GetVerb
    {
        public GetXmlVerb() => VerbType = VerbType.XmlVerb;
    }
}