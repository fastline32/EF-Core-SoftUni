using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni
{
    public class StartUp
    {
        static void Main()
        {
            var context = new SoftUniContext();
            var result = RemoveTown(context);
            Console.WriteLine(result);
        }

        //Task 3
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new
                {
                    x.EmployeeId,
                    x.FirstName,
                    x.LastName,
                    x.MiddleName,
                    x.JobTitle,
                    x.Salary

                })
                .OrderBy(x => x.EmployeeId)
                .ToList();

            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 4
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new
                {
                    x.FirstName,
                    x.Salary
                }).Where(x => x.Salary > 50000)
                .OrderBy(x => x.FirstName)
                .ToList();

            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 5
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    DepartmentName = x.Department.Name,
                    x.Salary
                })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();
            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.DepartmentName} - ${employee.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 6
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };
            context.Addresses.Add(address);
            context.SaveChanges();

            var user = context.Employees.FirstOrDefault(x => x.LastName == "Nakov");
            user.AddressId = address.AddressId;
            context.SaveChanges();

            var addresses = context.Employees
                .Select(x => new
                {
                    x.Address.AddressText,
                    x.Address.AddressId
                })
                .OrderByDescending(x => x.AddressId)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();
            foreach (var address1 in addresses)
            {
                sb.AppendLine(address1.AddressText);
            }

            return sb.ToString().TrimEnd();
        }

        //Task 7
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Where(x => x.EmployeesProjects.Any(x => x.Project.StartDate.Year >= 2001 
                                                         && x.Project.StartDate.Year <= 2003))
                .Select( x => new
                {
                    EmployeeFirstname = x.FirstName,
                    EmployeeLastName = x.LastName,
                    ManagerFirstName = x.Manager.FirstName,
                    ManagerLastName = x.Manager.LastName,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name,
                        ProjectStartDate = p.Project.StartDate,
                        ProjectEndDate = p.Project.EndDate
                    })
                })
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine(
                    $"{employee.EmployeeFirstname} {employee.EmployeeLastName} - Manager: {employee.ManagerFirstName} {employee.ManagerLastName}");
                foreach (var project in employee.Projects)
                {
                    var endDate = project.ProjectEndDate.HasValue ? project.ProjectEndDate.Value.ToString("M/d/yyyy h:mm:ss tt",CultureInfo.InvariantCulture) : "not finished";
                    sb.AppendLine($"--{project.ProjectName} - {project.ProjectStartDate.ToString("M/d/yyyy h:mm:ss tt",CultureInfo.InvariantCulture)} - {endDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Task 8
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addressess = context.Addresses
                .Include(x => x.Town)
                .Select(x => new
                {
                    x.AddressText,
                    TownName = x.Town.Name,
                    employeeCount = x.Employees.Count

                })
                .OrderByDescending(x => x.employeeCount)
                .ThenBy(x => x.TownName)
                .ThenBy(x => x.AddressText)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();
            foreach (var addressForPrint in addressess)
            {
                sb.AppendLine(
                    $"{addressForPrint.AddressText}, {addressForPrint.TownName} - {addressForPrint.employeeCount} employees");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 9
        public static string GetEmployee147(SoftUniContext context)
        {
            var person = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Select(x => new
                {
                    x.EmployeeId,
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    ProjectsName = x.EmployeesProjects.Select(x => x.Project.Name)
                })
                .FirstOrDefault(x => x.EmployeeId == 147);

            var sb = new StringBuilder();
            sb.AppendLine($"{person.FirstName} {person.LastName} - {person.JobTitle}");
            foreach (var project in person.ProjectsName.OrderBy(x => x))
            {
                sb.AppendLine(project);
            }

            return sb.ToString().TrimEnd();
        }

        //Task 10
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Include(x => x.Manager)
                .Include(x => x.Employees)
                .Select(x => new
                {
                    DepartmentName = x.Name,
                    ManagerFirstName = x.Manager.FirstName,
                    ManagerLastName = x.Manager.LastName,
                    employees = x.Employees
                })
                .Where(x => x.employees.Count > 5)
                .OrderBy(x => x.employees.Count)
                .ThenBy(x => x.DepartmentName)
                .ToList();

            var sb = new StringBuilder();
            foreach (var department in departments)
            {
                sb.AppendLine(
                    $"{department.DepartmentName} - {department.ManagerFirstName} {department.ManagerLastName}");
                foreach (var employee in department.employees)
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Task 11
        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .Select(x => new
                {
                    x.Name,
                    x.Description,
                    x.StartDate
                })
                .OrderByDescending(x => x.StartDate)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();
            foreach (var project in projects.OrderBy(x => x.Name))
            {
                sb.AppendLine($"{project.Name}");
                sb.AppendLine($"{project.Description}");
                sb.AppendLine($"{project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 12
        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Engineering" || x.Department.Name == "Tool Design" ||
                            x.Department.Name == "Marketing"
                            || x.Department.Name == "Information Services")
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;
                context.SaveChanges();
                sb.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 13
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.FirstName.StartsWith("Sa"))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            var sb = new StringBuilder();
            foreach (var employee in employees)
            {
                sb.AppendLine(
                    $"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 14
        public static string DeleteProjectById(SoftUniContext context)
        {
            var project = context.Projects.FirstOrDefault(x => x.ProjectId == 2);

            var emplPro = context.EmployeesProjects
                .Where(x => x.ProjectId == 2)
                .ToList();

            foreach (var employeeProject in emplPro)
            {
                context.EmployeesProjects.Remove(employeeProject);
            }

            context.SaveChanges();
            context.Projects.Remove(project);
            context.SaveChanges();

            var projects = context.Projects.Take(10).ToList();

            var sb = new StringBuilder();
            foreach (var project1 in projects)
            {
                sb.AppendLine($"{project1.Name}");
            }

            return sb.ToString().TrimEnd();
        }

        //Task 15
        public static string RemoveTown(SoftUniContext context)
        {
            Town town = context.Towns.Where(x => x.Name == "Seattle").FirstOrDefault();

            List<int> IdsOfAddressesToDelete = context.Addresses.Where(x => x.Town.Name == "Seattle").Select(x => x.AddressId).ToList();
            List<int> employeesAddressesForNulling = context.Employees
                .Where(x => x.AddressId.HasValue && IdsOfAddressesToDelete.Contains(x.AddressId.Value))
                .Select(x => x.EmployeeId)
                .ToList();

            foreach (Employee emp in context.Employees)
            {
                if (employeesAddressesForNulling.Contains(emp.EmployeeId))
                {
                    emp.AddressId = null;
                }
            }
            context.SaveChanges();

            context.Addresses.RemoveRange(context.Addresses.Where(x => x.Town.Name == "Seattle").ToList());

            context.Towns.Remove(town);

            context.SaveChanges();

            return $"{IdsOfAddressesToDelete.Count} addresses in Seattle were deleted";
        }
    }
}
