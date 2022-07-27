using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using TeisterMask.DataProcessor.ExportDto;

namespace TeisterMask.DataProcessor
{
    using System;

    using Data;

    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employee = context.Employees
                .Where(x => x.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .ToList()
                .Select(x => new
                {
                    Username = x.Username,
                    Tasks = x.EmployeesTasks
                        .Where(et => et.Task.OpenDate >= date)
                        .ToList()
                        .OrderByDescending(et => et.Task.DueDate)
                        .ThenBy(et => et.Task.Name)
                        .Select(t => new
                        {
                            TaskName = t.Task.Name,
                            OpenDate = t.Task.OpenDate.ToString("d",CultureInfo.InvariantCulture),
                            DueDate = t.Task.DueDate.ToString("d",CultureInfo.InvariantCulture),
                            LabelType = t.Task.LabelType.ToString(),
                            ExecutionType = t.Task.ExecutionType.ToString()
                        })
                        .ToList()
                })
                .OrderByDescending(x => x.Tasks.Count)
                .ThenBy(x => x.Username)
                .Take(10)
                .ToList();

            var list = JsonConvert.SerializeObject(employee, Formatting.Indented);

            return list;
        }

        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(ExportProjectModel[]), new XmlRootAttribute("Projects"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            var projectsList = context.Projects.Where(x => x.Tasks.Any())
                .ToList()
                .Select(x => new ExportProjectModel
                {
                    ProjectName = x.Name,
                    HasEndDate = x.DueDate.HasValue ? "Yes" : "No",
                    TaskCount = x.Tasks.Count,
                    Tasks = x.Tasks.Select(t => new TaskExportModel
                        {
                            Name = t.Name,
                            Label = t.LabelType.ToString()
                        })
                        .OrderBy(t => t.Name)
                        .ToArray()
                })
                .OrderByDescending(x => x.TaskCount)
                .ThenBy(x => x.ProjectName)
                .ToArray();
            xmlSerializer.Serialize(textWriter,projectsList,ns);
            return textWriter.ToString();
        }
    }
}