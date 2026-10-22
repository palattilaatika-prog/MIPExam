using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public class Employee : Person
    {
        private decimal MonthlySalary { get; set; }

        public Employee(int id, string name, decimal monthlySalary) : base(id, name)
        {
            MonthlySalary = monthlySalary;
        }

        public decimal GetHourlyRate()
        {
            return MonthlySalary / 160m;
        }

        public override string GetRoleName() { return "Employee"; }
    }
}
