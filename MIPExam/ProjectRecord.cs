using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public class ProjectRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal EstimatedBudget { get; set; }
        public decimal CalculatedCost { get; set; }
        public int PmEmployeeId { get; set; }
        public decimal PmHours { get; set; }
        public List<ProjectAssignment> Assignments { get; private set; }

        public ProjectRecord()
        {
            Assignments = new List<ProjectAssignment>();
        }
    }
}
