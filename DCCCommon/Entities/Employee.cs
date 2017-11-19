namespace DCCCommon.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Newtonsoft.Json;

    [Serializable]
    [XmlRoot(Namespace = "", ElementName = "Employees", IsNullable = false)]
    [JsonObject(MemberSerialization.OptIn, Title = "Employees")]
    public class EmployeesRoot
    {
        [XmlElement("Employee")]
        [JsonProperty("Employeey")]
        public List<Employee> EmployeeArray { get; set; }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Employee
    {
        [JsonProperty("id")]
        [XmlElement("id")]
        public int Id { get; set; }

        [JsonProperty("firstname")]
        [XmlElement("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        [XmlElement("lastname")]
        public string LastName { get; set; }

        [JsonProperty("gender")]
        [XmlElement("gender")]
        public string Gender { get; set; }

        [JsonProperty("salary")]
        [XmlElement("salary")]
        public int Salary { get; set; }

        public override string ToString() => $"{{ {nameof(Id)}: {Id}, " +
                                             $"{nameof(FirstName)}: {FirstName}, " +
                                             $"{nameof(LastName)}: {LastName}, " +
                                             $"{nameof(Gender)}: {Gender}, " +
                                             $"{nameof(Salary)}: {Salary} }}";
    }
}