namespace XmlPlayground
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DCCCommon.Conventions;
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using DCCNodeLib;
    using DCCNodeLib.DSL;
    using EasySharp.NHelpers.CustomExMethods;

    class Program
    {
        static async Task Main(string[] args)
        {
            IEnumerable<Employee> employees = LocalStorageManager.Default.GetEmployees();//.Where(e => e.Id.In(new []{ 10, 11, 12, 13, 14, 15 }));

            //IEnumerable enumerable = employees.Where("Id > 10 and (Id <= 20)")
            //    .OrderBy("Id descending")
            //    .GroupBy("Gender", "it")
            //    .Select("new (it.Key as Gender, it as Employees)");

            //foreach (dynamic g in enumerable)
            //{
            //    Console.Out.WriteLine(g.Gender);

            //    foreach (dynamic employee in g.Employees)
            //    {
            //        Console.Out.WriteLine("employee = {0}", employee);
            //    }
            //}

            var interpreter = new DSLInterpreter(new RequestDataMessage
            {
                DataFormat = Common.Json
            });


            string json = await interpreter.TransfromDataToRequiredFromatAsync(employees).ConfigureAwait(false);

            await Console.Out.WriteLineAsync(json).ConfigureAwait(false);
        }
    }
}