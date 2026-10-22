using MIPExam.MIPEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MIPExam
{
    public partial class Form1 : Form
    {
        private readonly Database database;
        public Form1()
        {
            InitializeComponent();
            database = new Database();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            database.GenerateRequiredEmployees();
            dataGridView1.DataSource = database.GetEmployees()
                .Select(employee => new
                {
                    employee.Id,
                    employee.Name,
                    Role = employee.GetRoleName(),
                    employee.MonthlySalary
                })
                .ToList();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
