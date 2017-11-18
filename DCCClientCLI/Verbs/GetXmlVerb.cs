namespace DCCClientCLI.Verbs
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    [Verb("get-xml", HelpText = "Retrieves xml from ...")]
    class GetXmlVerb : GetVerb
    {
        public GetXmlVerb() => DataFormat = DataFormat.Xml;

        [Usage(ApplicationAlias = "DCCC")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(
                    "Minimal scenario",
                    new GetXmlVerb
                    {
                        DataType = "Employee"
                    });

                yield return new Example(
                    "Common scenario",
                    UnParserSettings.WithUseEqualTokenOnly(),
                    new GetXmlVerb
                    {
                        Timeout = 1,
                        DataType = "Employee",
                        FilterCondition = @"Gender == ""Female"" and Salary > 3000 or (Gender == ""Male"" and Salary < 1000)",
                        OrderingCondition = "Id descending"
                    });

                yield return new Example(
                    "Common scenario",
                    new GetXmlVerb
                    {
                        Timeout = 5,
                        DataType = "Employee",
                        FilterCondition = "Salary > 1500 and Salary <= 2000",
                        OrderingCondition = "Id ascending"
                    });
            }
        }

    }
}