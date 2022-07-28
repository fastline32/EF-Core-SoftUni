using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Theatre.DataProcessor.ExportDto;

namespace Theatre.DataProcessor
{
    using System;
    using Theatre.Data;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var theatres = context.Theatres
                .Where(x => x.NumberOfHalls >= numbersOfHalls && x.Tickets.Count >= 20)
                .ToList()
            .Select(t => new
             {
                 Name = t.Name,
                 Halls = t.NumberOfHalls,
                 TotalIncome = t.Tickets.Where(x => x.RowNumber >= 1 && x.RowNumber <= 5).Sum(x => x.Price),
                 Tickets = t.Tickets.Where(x => x.RowNumber >= 1 && x.RowNumber <= 5)
                    .Select(x => new
                    {
                        Price = x.Price,
                        RowNumber = x.RowNumber
                    })
                    .OrderByDescending(x => x.Price)
                    .ToList()
             })
            .OrderByDescending(t => t.Halls)
            .ThenBy(t => t.Name).ToList();

            var json = JsonConvert.SerializeObject(theatres, Formatting.Indented);
            return json;
        }

        public static string ExportPlays(TheatreContext context, double rating)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<PlayExportModel>), new XmlRootAttribute("Plays"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            var playsDto = context.Plays.Where(x => x.Rating <= rating)
                .ToList()
                .Select(x => new PlayExportModel
                {
                    Title = x.Title,
                    Duration = x.Duration.ToString("c", CultureInfo.InvariantCulture),
                    Rating = x.Rating.ToString(),
                    Genre = x.Genre.ToString(),
                    Actors = x.Casts.Where(x => x.IsMainCharacter == true)
                        .Select(a => new ActorExportModel
                        {
                            FullName = a.FullName,
                            MainCharacter = $"Plays main character in '{a.Play.Title}'."
                        })
                        .OrderByDescending(a => a.FullName)
                        .ToList()
                })
                .OrderBy(x => x.Title)
                .ThenByDescending(x => x.Genre).ToList();
            foreach (var playExportModel in playsDto)
            {
                if (playExportModel.Rating == "0")
                {
                    playExportModel.Rating = "Premier";
                }
            }
            xmlSerializer.Serialize(textWriter,playsDto,ns);

            return textWriter.ToString();
        }
    }
}
