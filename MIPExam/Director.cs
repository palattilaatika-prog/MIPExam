using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public class Director : Employee
    {
        public Director(int id, string name, decimal monthlySalary) : base(id, name, monthlySalary) { }
        public override string GetRoleName() { return "Director"; }
    
    }
}
