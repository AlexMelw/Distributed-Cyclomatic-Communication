namespace DCCNodeLib.DSL
{
    using System.Collections.Generic;
    using System.Linq;
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

        public string TransformDataToRequiredFormat(IEnumerable<Employee> employees)
        {
            string serializedData;

            switch (_dataFormat)
            {
                case Common.Xml:
                    serializedData = SerializeToXml(employees);
                    break;

                case Common.Json:
                    serializedData = SerializeToJson(employees);
                    break;

                default:
                    serializedData = new ErrorMessage { ErrorDescription = "Unsupported data format was requested" }
                        .SerializeToXml();
                    break;
            }

            return serializedData;
        }

        private string SerializeToJson(IEnumerable<Employee> employees)
        {
            dynamic dynamicWrapper = new { Employees = employees };

            string serializedData = JsonConvert.SerializeObject(dynamicWrapper, Formatting.Indented);

            return serializedData;
        }

        private string SerializeToXml(IEnumerable<Employee> employees)
        {
            var employeesRoot = new EmployeesRoot
            {
                EmployeeArray = employees.ToList()
            };

            string serializedData = employeesRoot.SerializeToXml();

            return serializedData;
        }
    }
}