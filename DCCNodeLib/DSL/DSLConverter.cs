namespace DCCNodeLib.DSL
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DCCCommon.Conventions;
    using DCCCommon.Entities;
    using DCCCommon.Messages;
    using EasySharp.NHelpers.CustomExMethods;
    using Newtonsoft.Json;

    public class DSLConverter
    {
        private readonly string _dataFormat;

        #region CONSTRUCTORS

        public DSLConverter(RequestDataMessage requestDataMessage)
        {
            _dataFormat = requestDataMessage.DataFormat;
        }

        #endregion

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
            dynamic dynamicWrapper = new { Employees = employees };

            string serializedData = JsonConvert.SerializeObject(dynamicWrapper, Formatting.Indented);

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