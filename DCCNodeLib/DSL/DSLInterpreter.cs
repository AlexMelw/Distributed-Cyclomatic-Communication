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

        public Task<IEnumerable<Employee>> GetDataAsync()
        {
            IEnumerable<Employee> employees = LocalStorageManager.Default.GetEmployees();

            IEnumerable<Employee> selectedEmployees = employees
                .Where(_filterCondition)
                .OrderBy(_orderingCondition);

            return Task.FromResult(selectedEmployees);
        }
    }
}