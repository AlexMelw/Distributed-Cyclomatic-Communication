﻿namespace XmlPlayground
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic;
    using DCCCommon.Entities;
    using DCCNodeLib;
    using EasySharp.NHelpers.CustomExMethods;

    class Program
    {
        static void Main(string[] args)
        {
            //IEnumerable<Employee> employees = LocalStorageManager.Default.GetEmployees();

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
        }
    }
}