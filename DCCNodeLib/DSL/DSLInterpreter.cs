namespace DCCNodeLib.DSL
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Threading.Tasks;
    using System.Xml;
    using DCCCommon.Conventions;
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class DSLInterpreter
    {
        private readonly string _dataFormat;
        private readonly string _filterCondition;
        private readonly string _orderingCondition;

        public DSLInterpreter(RequestDataMessage requestDataMessage)
        {
            _dataFormat = requestDataMessage.DataFormat;
            _filterCondition = requestDataMessage.FilterCondition;
            _orderingCondition = requestDataMessage.OrderingCondition;
        }

        public Task<IEnumerable<Employee>> GetDataAsync()
        {
            IEnumerable<Employee> employees = LocalStorageManager.Default.GetEmployees();

            IEnumerable<Employee> selectedEmployees = employees
                .Where(_filterCondition)
                .OrderBy(_orderingCondition);

            return Task.FromResult(selectedEmployees);
        }

        public async Task<string> TransfromDataToRequiredFromatAsync(IEnumerable<Employee> employees)
        {
            string serializedData;

            switch (_dataFormat)
            {
                case Common.Xml:
                    serializedData = await SerializeToXmlAsync(employees).ConfigureAwait(false);
                    break;

                case Common.Json:
                    serializedData = await SerializeToJsonAsync(employees).ConfigureAwait(false);
                    break;

                default:
                    serializedData = new ErrorMessage { ErrorDescription = "Unsupported data format was requested" }
                        .SerializeToXml();
                    break;
            }

            return serializedData;
        }

        private Task<string> SerializeToJsonAsync(IEnumerable<Employee> employees)
        {
            dynamic eeeeeeee = new { Employees = employees };


            string serializedData = JsonConvert.SerializeObject(eeeeeeee, Formatting.Indented);

            return Task.FromResult(serializedData);
        }

        private Task<string> SerializeToXmlAsync(IEnumerable<Employee> employees)
        {
            var employeesRoot = new EmployeesRoot
            {
                EmployeeArray = employees.ToList()
            };

            string serializedData = employeesRoot.SerializeToXml();

            return Task.FromResult(serializedData);
        }
    }
}