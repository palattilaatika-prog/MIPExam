using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace MIPExam
{
    namespace MIPEx
    {
        public sealed class Database : IDisposable
        {
            private readonly SQLiteConnection connection;
            private readonly string tracePath;
            private bool traceInitialized;

            public Database()
            {
                string databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agency.db");
                tracePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trace.txt");
                connection = new SQLiteConnection("Data Source=" + databasePath + ";Version=3;");
                connection.Open();
                CreateSchema();
                InitializeTrace();
            }

            private void InitializeTrace()
            {
                if (traceInitialized) return;
                Trace.Listeners.Add(new TextWriterTraceListener(tracePath));
                Trace.AutoFlush = true;
                traceInitialized = true;
            }

            private void CreateSchema()
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
CREATE TABLE IF NOT EXISTS Employees (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Role TEXT NOT NULL,
    Seniority TEXT NULL,
    MonthlySalary NUMERIC NOT NULL
);
CREATE TABLE IF NOT EXISTS Projects (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    EstimatedBudget NUMERIC NOT NULL,
    CalculatedCost NUMERIC NOT NULL,
    PmEmployeeId INTEGER NOT NULL,
    PmHours NUMERIC NOT NULL
);
CREATE TABLE IF NOT EXISTS ProjectAssignments (
    ProjectId INTEGER NOT NULL,
    EmployeeId INTEGER NOT NULL,
    AllocatedHours NUMERIC NOT NULL,
    PRIMARY KEY (ProjectId, EmployeeId),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);";
                    command.ExecuteNonQuery();
                }
            }

            public List<Employee> GetEmployees()
            {
                List<Employee> employees = new List<Employee>();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Name, Role, Seniority, MonthlySalary FROM Employees ORDER BY Id";
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string role = Convert.ToString(reader["Role"]);
                            int id = Convert.ToInt32(reader["Id"]);
                            string name = Convert.ToString(reader["Name"]);
                            decimal salary = Convert.ToDecimal(reader["MonthlySalary"], CultureInfo.InvariantCulture);
                            string seniority = reader["Seniority"] == DBNull.Value ? null : Convert.ToString(reader["Seniority"]);
                            employees.Add(CreateEmployee(id, name, role, seniority, salary));
                        }
                    }
                }
                return employees;
            }

            private static Employee CreateEmployee(int id, string name, string role, string seniority, decimal salary)
            {
                if (role == "Junior programmer" || role == "Mid programmer" || role == "Senior programmer")
                {
                    SeniorityLevel level = (SeniorityLevel)Enum.Parse(typeof(SeniorityLevel), seniority);
                    return new Programmer(id, name, salary, level);
                }
                if (role == "Project manager") return new ProjectManager(id, name, salary);
                if (role == "HR director") return new HrDirector(id, name, salary);
                if (role == "Director") return new Director(id, name, salary);
                return new Employee(id, name, salary);
            }

            public void GenerateRequiredEmployees()
            {
                string[] requiredRoles = { "Junior programmer", "Mid programmer", "Senior programmer", "Project manager", "Director", "HR director" };
                int[] requiredCounts = { 30, 20, 10, 10, 2, 2 };
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    for (int roleIndex = 0; roleIndex < requiredRoles.Length; roleIndex++)
                    {
                        int existingCount = CountRole(requiredRoles[roleIndex]);
                        for (int number = existingCount; number < requiredCounts[roleIndex]; number++)
                        {
                            InsertEmployee(CreateGeneratedEmployee(requiredRoles[roleIndex], number + 1), transaction);
                        }
                    }
                    int total = CountEmployees();
                    for (int number = total; number < 100; number++)
                    {
                        InsertEmployee(new Employee(0, "Support employee " + (number + 1), 7000m + (number % 5) * 300m), transaction);
                    }
                    transaction.Commit();
                }
            }

            private int CountRole(string role)
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Employees WHERE Role = @role";
                    command.Parameters.AddWithValue("@role", role);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }

            private int CountEmployees()
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Employees";
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }

            private static Employee CreateGeneratedEmployee(string role, int number)
            {
                string name = role + " " + number.ToString("D2", CultureInfo.InvariantCulture);
                if (role == "Junior programmer") return new Programmer(0, name, 8000m, SeniorityLevel.Junior);
                if (role == "Mid programmer") return new Programmer(0, name, 12000m, SeniorityLevel.Mid);
                if (role == "Senior programmer") return new Programmer(0, name, 18000m, SeniorityLevel.Senior);
                if (role == "Project manager") return new ProjectManager(0, name, 16000m);
                if (role == "Director") return new Director(0, name, 25000m);
                return new HrDirector(0, name, 23000m);
            }

            private void InsertEmployee(Employee employee, SQLiteTransaction transaction)
            {
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    Programmer programmer = employee as Programmer;
                    command.Transaction = transaction;
                    command.CommandText = "INSERT INTO Employees (Name, Role, Seniority, MonthlySalary) VALUES (@name, @role, @seniority, @salary)";
                    command.Parameters.AddWithValue("@name", employee.Name);
                    command.Parameters.AddWithValue("@role", employee.GetRoleName());
                    command.Parameters.AddWithValue("@seniority", programmer == null ? (object)DBNull.Value : programmer.Seniority.ToString());
                    command.Parameters.AddWithValue("@salary", employee.MonthlySalary);
                    command.ExecuteNonQuery();
                    long id = connection.LastInsertRowId;
                    Trace.WriteLine("DB_INSERT: table=Employees; id=" + id + "; timestamp=" + DateTime.UtcNow.ToString("o"));
                }
            }

            public int SaveProject(ProjectRecord project)
            {
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO Projects (Name, EstimatedBudget, CalculatedCost, PmEmployeeId, PmHours) VALUES (@name, @budget, @cost, @pm, @hours)";
                        command.Parameters.AddWithValue("@name", project.Name);
                        command.Parameters.AddWithValue("@budget", project.EstimatedBudget);
                        command.Parameters.AddWithValue("@cost", project.CalculatedCost);
                        command.Parameters.AddWithValue("@pm", project.PmEmployeeId);
                        command.Parameters.AddWithValue("@hours", project.PmHours);
                        command.ExecuteNonQuery();
                        project.Id = (int)connection.LastInsertRowId;
                    }
                    foreach (ProjectAssignment assignment in project.Assignments)
                    {
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "INSERT INTO ProjectAssignments (ProjectId, EmployeeId, AllocatedHours) VALUES (@project, @employee, @hours)";
                            command.Parameters.AddWithValue("@project", project.Id);
                            command.Parameters.AddWithValue("@employee", assignment.Programmer.Id);
                            command.Parameters.AddWithValue("@hours", assignment.AllocatedHours);
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                Trace.WriteLine("DB_INSERT: table=Projects; id=" + project.Id + "; timestamp=" + DateTime.UtcNow.ToString("o"));
                string assignments = string.Join(", ", project.Assignments.Select(a => a.Programmer.Id + ":" + a.AllocatedHours.ToString(CultureInfo.InvariantCulture)));
                Trace.WriteLine("PROJECT_ACCEPTED: timestamp=" + DateTime.UtcNow.ToString("o") + "; ProjectId=" + project.Id + "; Name=" + project.Name + "; EstimatedBudget=" + project.EstimatedBudget.ToString(CultureInfo.InvariantCulture) + "; CalculatedCost=" + project.CalculatedCost.ToString(CultureInfo.InvariantCulture) + "; PmEmployeeId=" + project.PmEmployeeId + "; PmHours=" + project.PmHours.ToString(CultureInfo.InvariantCulture) + "; Developers=" + assignments);
                return project.Id;
            }

            public List<ProjectRecord> GetProjects(List<Employee> employees)
            {
                List<ProjectRecord> projects = new List<ProjectRecord>();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Name, EstimatedBudget, CalculatedCost, PmEmployeeId, PmHours FROM Projects ORDER BY Id";
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProjectRecord project = new ProjectRecord
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = Convert.ToString(reader["Name"]),
                                EstimatedBudget = Convert.ToDecimal(reader["EstimatedBudget"]),
                                CalculatedCost = Convert.ToDecimal(reader["CalculatedCost"]),
                                PmEmployeeId = Convert.ToInt32(reader["PmEmployeeId"]),
                                PmHours = Convert.ToDecimal(reader["PmHours"])
                            };
                            projects.Add(project);
                        }
                    }
                }
                return projects;
            }

            public void Dispose()
            {
                connection.Dispose();
            }
        }
    }
}
