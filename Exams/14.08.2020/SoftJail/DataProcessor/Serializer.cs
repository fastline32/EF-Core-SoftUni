using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SoftJail.DataProcessor.ExportDto;

namespace SoftJail.DataProcessor
{

    using Data;
    using System;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var result = context.Prisoners
                .Where(X => ids.Contains(X.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.FullName,
                    CellNumber = x.Cell.CellNumber,
                    Officers = x.PrisonerOfficers.Select(z => new
                    {
                        OfficerName = z.Officer.FullName,
                        Department = z.Officer.Department.Name
                    })
                        .OrderBy(x => x.OfficerName)
                        .ToList(),
                    TotalOfficerSalary = decimal.Parse(x.PrisonerOfficers.Sum(z => z.Officer.Salary).ToString("F2"))
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToList();
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<PrisonerOutputModel>), new XmlRootAttribute("Prisoners"));
            var names = prisonersNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var prisoners = context.Prisoners
                .Where(x => names.Contains(x.FullName))
                .Select(x => new PrisonerOutputModel
                {
                    FullName = x.FullName,
                    Id = x.Id,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd"),
                    Mail = x.Mails.Select(z => new EncriptedMail
                    {
                        Description = string.Join("",z.Description.Reverse())
                    }).ToArray()
                })
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.Id)
                .ToList();
            var textWriter = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            xmlSerializer.Serialize(textWriter, prisoners, ns);

            return textWriter.ToString();
        }

        private static string Reverse(string text)
        {
            char[] chArray = text.ToCharArray();
            string reversed = string.Empty;
            for (int i = chArray.Length - 1; i > -1; i--)
            {
                reversed += chArray[i];
            }
            return reversed;
        }
    }
}