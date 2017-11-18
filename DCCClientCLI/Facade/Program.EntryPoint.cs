namespace DCCClientCLI.Facade
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
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
            Console.Title = "DIS Client";

            //args = new[] { "get-json", "-d", "Employee", "-f", "Id > 10 and Id <= 20", "-o", "Id descending", "-t", "10" };
            //args = new[] { "get-json", "-d", "Employee", "-t", "1" };
            //args = new[] { "get-json", "-d", "Employee", "-f", "Salary > 3000 and Gender == \"Male\"", "-o", "Id", "-t", "1", "-s", "Schemas\\EmployeesSchema.json" };
            //args = new[] { "get-xml", "-d", "Employee", "-f", "Salary > 3000 and Gender == \"Male\"", "-o", "Id", "-t", "1", "-s", "Schemas\\Employees.xsd" };

            Parser.Default.ParseArguments<GetXmlVerb, GetJsonVerb>(args)
                .WithParsed<GetXmlVerb>(ProcessGetXmlCommand)
                .WithParsed<GetJsonVerb>(ProcessGetJsonCommand);

            //Console.ReadKey();

            //Process.GetCurrentProcess().Kill();
            //Environment.Exit(0);
        }
    }
}