using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    public abstract class Person
    {
        private int Id { get;set; }
        private string Name { get; set; }

        protected Person(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public abstract string GetRoleName();
    }
}
