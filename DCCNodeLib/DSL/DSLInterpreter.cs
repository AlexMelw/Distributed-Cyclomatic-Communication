namespace DCCNodeLib.DSL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Threading.Tasks;
    using DCCCommon.Entities;
    using DCCCommon.Messages;

    public class DSLInterpreter
    {
        private readonly string _filterCondition;
        private readonly string _orderingCondition;

        #region CONSTRUCTORS

        public DSLInterpreter(RequestDataMessage requestDataMessage)
        {
            _filterCondition = requestDataMessage.FilterCondition;
            _orderingCondition = requestDataMessage.OrderingCondition;
        }

        #endregion

        public IEnumerable<Employee> GetDataFromDataSource(string dataSourceFilePath)
        {
            IEnumerable<Employee> data = LocalStorageManager.Default
                                             .GetEmployeesFrom(dataSourceFilePath)
                                         ?? Enumerable.Empty<Employee>();

            return data;
        }

        public IEnumerable<Employee> ProcessData(IEnumerable<Employee> employees)
        {
            IEnumerable<Employee> processedEmployees = employees;

            if (!string.IsNullOrWhiteSpace(_filterCondition))
            {
                processedEmployees = employees?.Where(_filterCondition);
            }

            if (!string.IsNullOrWhiteSpace(_orderingCondition))
            {
                processedEmployees = processedEmployees?.OrderBy(_orderingCondition);
            }

            return processedEmployees;
        }
    }
}