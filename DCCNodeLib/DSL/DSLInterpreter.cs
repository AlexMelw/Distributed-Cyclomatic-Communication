namespace DCCNodeLib.DSL
{
    using System.Collections.Generic;
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

        public IEnumerable<Employee> GetData(string dataSourceFilePath)
        {
            IEnumerable<Employee> employees = LocalStorageManager.Default.GetEmployeesFrom(dataSourceFilePath);
            
            if (!string.IsNullOrWhiteSpace(_filterCondition))
            {
                employees = employees.Where(_filterCondition);
            }

            if (!string.IsNullOrWhiteSpace(_orderingCondition))
            {
                employees = employees.OrderBy(_orderingCondition);
            }

            return employees;
        }
    }
}