namespace DCCNodeLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using DCCCommon;
    using DCCCommon.Entities;

    public class LocalStorageManager
    {
        private static readonly object PadLock = new object();

        private static readonly Lazy<LocalStorageManager> LazyInstance =
            new Lazy<LocalStorageManager>(() => new LocalStorageManager(), true);
        //private const string ConfigFilePath = "StartupConfig.xml";

        //public string DataFilePath { get; set; }

        public static LocalStorageManager Default => LazyInstance.Value;

        #region CONSTRUCTORS

        private LocalStorageManager()
        {
            //DataFilePath = GetStartupConfigPath();
        }

        #endregion

        //private static string GetStartupConfigPath()
        //{
        //    string executingPath = AppDomain.CurrentDomain.BaseDirectory;
        //    string startupConfigPath = Path.Combine(executingPath, "Employees.xml");
        //    return startupConfigPath;
        //}

        public IEnumerable<Employee> GetEmployeesFrom(string dataSourceFilePath)
        {
            Console.Out.WriteLine($"I'm trying to access {dataSourceFilePath}");

            try
            {
                EmployeesRoot result;
                using (FileStream fileStream = new FileStream(dataSourceFilePath, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EmployeesRoot));
                    result = (EmployeesRoot)serializer.Deserialize(fileStream);
                }

                return result.EmployeeArray;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                return Array.Empty<Employee>();
            }
        }
    }
}