using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SoftJail.Data.Models;
using SoftJail.Data.Models.Enums;
using SoftJail.DataProcessor.ImportDto;

namespace SoftJail.DataProcessor
{

    using Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var departmentCells = JsonConvert.DeserializeObject<IEnumerable<DepartmentCellDto>>(jsonString);
            var departments = new List<Department>();
            foreach (var departmentCellDto in departmentCells)
            {
                if (!IsValid(departmentCellDto) || !departmentCellDto.Cells.All(IsValid) ||
                    departmentCellDto.Cells.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = departmentCellDto.Name,
                    Cells = departmentCellDto.Cells.Select(x => new Cell
                    {
                        CellNumber = x.CellNumber,
                        HasWindow = x.HasWindow
                    }).ToList()
                };

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
                departments.Add(department);
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonerMails = JsonConvert.DeserializeObject<List<PrisonerMailInputModel>>(jsonString);
            var sb = new StringBuilder();
            var prisoners = new List<Prisoner>();
            foreach (var prisonerMailInputModel in prisonerMails)
            {
                if (!IsValid(prisonerMailInputModel) || !prisonerMailInputModel.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var releaseDate = DateTime.TryParseExact(
                    prisonerMailInputModel.ReleaseDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime relResult);

                var prisoner = new Prisoner
                {
                    FullName = prisonerMailInputModel.FullName,
                    Nickname = prisonerMailInputModel.Nickname,
                    Age = prisonerMailInputModel.Age,
                    Bail = prisonerMailInputModel.Bail,
                    CellId = prisonerMailInputModel.CellId,
                    IncarcerationDate =
                        DateTime.ParseExact(prisonerMailInputModel.IncarcerationDate, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture),
                    ReleaseDate = releaseDate ? (DateTime?) relResult : null,
                    Mails = prisonerMailInputModel.Mails.Select(x => new Mail
                    {
                        Description = x.Description,
                        Address = x.Address,
                        Sender = x.Sender
                    }).ToList()
                };
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
                prisoners.Add(prisoner);
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();
            return sb.ToString().Trim();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<OfficerPrisonerInputModel>), new XmlRootAttribute("Officers"));
            var textReader = new StringReader(xmlString);
            var officersDto = xmlSerializer.Deserialize(textReader) as List<OfficerPrisonerInputModel>;
            var officerPrisoners = new List<Officer>();
            var sb = new StringBuilder();
            foreach (var model in officersDto)
            {
                if (!IsValid(model))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = model.Name,
                    Salary = model.Salary,
                    DepartmentId = model.DepartmentId,
                    Position = Enum.Parse<Position>(model.Position),
                    Weapon = Enum.Parse<Weapon>(model.Weapon),
                    OfficerPrisoners = model.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    }).ToList()
                };
                officerPrisoners.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }
            context.Officers.AddRange(officerPrisoners);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}