using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Theatre.Data.Models;
using Theatre.Data.Models.Enums;
using Theatre.DataProcessor.ImportDto;

namespace Theatre.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Theatre.Data;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";

        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<PlayImportModel>), new XmlRootAttribute("Plays"));
            var textReader = new StringReader(xmlString);
            var playsDto = xmlSerializer.Deserialize(textReader) as List<PlayImportModel>;
            var sb = new StringBuilder();
            List<Play> plays = new List<Play>();
            foreach (var playDto in playsDto)
            {
                if (!IsValid(playDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var play = new Play
                {
                    Title = playDto.Title,
                    Description = playDto.Description,
                    Rating = playDto.Rating,
                    Screenwriter = playDto.Screenwriter
                };

                var isValidGenre = Enum.TryParse(playDto.Genre,out Genre genre);

                if (!isValidGenre)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                play.Genre = genre;
                TimeSpan trySpan;
                var isValidTimeSpan = TimeSpan.TryParse(playDto.Duration, CultureInfo.InvariantCulture, out trySpan);
                if (!isValidTimeSpan)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (trySpan.TotalMinutes < 60)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                play.Duration = (TimeSpan)trySpan;
                var playGenre = play.Genre;
                sb.AppendLine(String.Format(SuccessfulImportPlay, play.Title, play.Genre.ToString(), play.Rating));
                plays.Add(play);
            }
            context.Plays.AddRange(plays);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CastImportModel>), new XmlRootAttribute("Casts"));
            var textReader = new StringReader(xmlString);
            var castsDto = xmlSerializer.Deserialize(textReader) as List<CastImportModel>;
            var sb = new StringBuilder();
            List<Cast> casts = new List<Cast>();
            foreach (var castDto in castsDto)
            {
                if (!IsValid(castDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                var cast = new Cast
                {
                    FullName = castDto.FullName,
                    PhoneNumber = castDto.PhoneNumber,
                    PlayId = castDto.PlayId,
                    IsMainCharacter = bool.Parse(castDto.IsMainCharacter)};
                casts.Add(cast);
                if (cast.IsMainCharacter)
                {
                    sb.AppendLine($"Successfully imported actor {cast.FullName} as a main character!");
                }
                else
                {
                    sb.AppendLine($"Successfully imported actor {cast.FullName} as a lesser character!");
                }
            }
            context.Casts.AddRange(casts);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            var theathresDto = JsonConvert.DeserializeObject<List<TheatreAndTicketsImportModel>>(jsonString);
            var theathres = new List<Data.Models.Theatre>();
            var sb = new StringBuilder();
            foreach (var theathreDto  in theathresDto)
            {
                if (!IsValid(theathreDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var theathre = new Data.Models.Theatre
                {
                    Name = theathreDto.Name,
                    NumberOfHalls = theathreDto.NumberOfHalls,
                    Director = theathreDto.Director
                };
                var tickets = new List<Ticket>();
                foreach (var ticketDto in theathreDto.Tickets)
                {
                    if (!IsValid(ticketDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var ticket = new Ticket
                    {
                        Price = ticketDto.Price,
                        RowNumber = ticketDto.RowNumber,
                        PlayId =  ticketDto.PlayId
                    };
                    tickets.Add(ticket);
                }

                theathre.Tickets = tickets;
                theathres.Add(theathre);
                sb.AppendLine(String.Format(SuccessfulImportTheatre, theathre.Name, theathre.Tickets.Count));
            }
            context.Theatres.AddRange(theathres);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
