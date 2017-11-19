namespace DCCNodeLib.DSL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Threading.Tasks;
    using DCCCommon.Entities;
    using DCCCommon.Messages;

    public class DSLProcessor
    {
        private readonly string _filterCondition;
        private readonly string _orderingCondition;
        private int _propagation;

        #region CONSTRUCTORS

        public DSLProcessor(RequestDataMessage requestDataMessage)
        {
            _filterCondition = requestDataMessage.FilterCondition;
            _orderingCondition = requestDataMessage.OrderingCondition;
            _propagation = requestDataMessage.Propagation;
        }

        #endregion

        //public IEnumerable<Employee> GetDataFromDataSource(string dataSourceFilePath)
        //{
        //    IEnumerable<Employee> data = LocalStorageManager.Default
        //                                     .GetEmployeesFrom(dataSourceFilePath)
        //                                 ?? Enumerable.Empty<Employee>();

        //    return data;
        //}

        public IEnumerable<Employee> ProcessData(IEnumerable<Employee> employees)
        {
            IEnumerable<Employee> processedEmployees = employees;

            try
            {
                if (!string.IsNullOrWhiteSpace(_filterCondition))
                {
                    processedEmployees = employees?.Where(_filterCondition);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Error: Couldn't filter employees.");
            }

            try
            {
                if (_propagation == 0 && !string.IsNullOrWhiteSpace(_orderingCondition))
                {
                    processedEmployees = processedEmployees?.OrderBy(_orderingCondition);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Error: Couldn't sort employees.");
            }
            return processedEmployees;
        }
    }
}