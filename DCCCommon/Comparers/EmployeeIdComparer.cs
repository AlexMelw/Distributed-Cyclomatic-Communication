namespace DCCCommon.Comparers
{
    using System;
    using System.Collections.Generic;
    using Entities;

    public class EmployeeIdComparer : IEqualityComparer<Employee>
    {
        public bool Equals(Employee x, Employee y) => x?.Id == y?.Id;

        public int GetHashCode(Employee obj) => obj.Id.GetHashCode();

        public static EmployeeIdComparer Default => new EmployeeIdComparer();
    }
}