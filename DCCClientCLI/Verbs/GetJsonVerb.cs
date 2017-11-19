namespace DCCClientCLI.Verbs
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    [Verb("get-json", HelpText = "Retrieves json from ...")]
    class GetJsonVerb : GetVerb
    {
        [Usage(ApplicationAlias = "DCCC")]
        public static IEnumerable<Example> Examples
        {
            get {
                yield return new Example(
                    "Minimal scenario",
                    new GetJsonVerb
                    {
                        DataType = "Employee"
                    });

                yield return new Example(
                    "Common scenario",
                    UnParserSettings.WithUseEqualTokenOnly(),
                    new GetJsonVerb
                    {
                        Timeout = 1,
                        DataType = "Employee",
                        FilterCondition =
                            @"Gender == ""Female"" and Salary > 3000 or (Gender == ""Male"" and Salary < 1000)",
                        OrderingCondition = "Id descending"
                    });

                yield return new Example(
                    "Common scenario",
                    new GetJsonVerb
                    {
                        Timeout = 5,
                        DataType = "Employee",
                        FilterCondition = "Salary > 1500 and Salary <= 2000",
                        OrderingCondition = "Id ascending"
                    });
            }
        }

        #region CONSTRUCTORS

        public GetJsonVerb() => DataFormat = DataFormat.Json;

        #endregion
    }
}