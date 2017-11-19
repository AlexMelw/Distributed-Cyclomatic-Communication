namespace DCCCommon.Comparers
{
    using System.Collections.Generic;
    using Entities;

    public class EmployeeIdComparer : IEqualityComparer<Employee>
    {
        public static EmployeeIdComparer Default => new EmployeeIdComparer();
        public bool Equals(Employee x, Employee y) => x?.Id == y?.Id;

        public int GetHashCode(Employee obj) => obj.Id.GetHashCode();
    }
}