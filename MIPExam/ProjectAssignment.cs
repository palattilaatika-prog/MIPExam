using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public class ProjectAssignment
    {
        public Programmer Programmer { get; private set; }
        public decimal AllocatedHours { get; set; }

        public ProjectAssignment(Programmer programmer, decimal allocatedHours)
        {
            Programmer = programmer;
            AllocatedHours = allocatedHours;
        }
    }
}