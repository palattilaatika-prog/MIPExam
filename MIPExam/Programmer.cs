using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public enum SeniorityLevel
    {
        Junior,
        Mid,
        Senior
    }

    public class Programmer : Employee
    {
        public SeniorityLevel Seniority { get; private set; }

        public Programmer(int id, string name, decimal monthlySalary, SeniorityLevel seniority)
            : base(id, name, monthlySalary)
        {
            Seniority = seniority;
        }

        public override string GetRoleName() { return Seniority + " programmer"; }
    }
}
