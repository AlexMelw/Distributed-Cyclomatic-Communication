namespace DCCCommon.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [Serializable]
    [XmlRoot(Namespace = "", ElementName = "Employees", IsNullable = false)]
    public class EmployeesRoot
    {
        [XmlElement("Employee")]
        public List<Employee> EmployeeArray { get; set; }
    }

    [Serializable]
    public class Employee
    {
        [XmlElement("id")]
        public byte Id { get; set; }

        [XmlElement("firstname")]
        public string FirstName { get; set; }

        [XmlElement("lastname")]
        public string LastName { get; set; }

        [XmlElement("gender")]
        public string Gender { get; set; }

        [XmlElement("salary")]
        public ushort Salary { get; set; }

        public override string ToString() => $"{{ {nameof(Id)}: {Id}, " +
                                             $"{nameof(FirstName)}: {FirstName}, " +
                                             $"{nameof(LastName)}: {LastName}, " +
                                             $"{nameof(Gender)}: {Gender}, " +
                                             $"{nameof(Salary)}: {Salary} }}";
    }
}