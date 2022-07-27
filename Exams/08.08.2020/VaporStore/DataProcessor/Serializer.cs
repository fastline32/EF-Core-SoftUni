using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VaporStore.Data.Models;
using VaporStore.Data.Models.Enums;
using VaporStore.DataProcessor.Dto.Export;

namespace VaporStore.DataProcessor
{
	using System;
	using Data;

	public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var games = context.Genres
                .ToArray()
                .Where(x => genreNames.Contains(x.Name))
                .Select(x => new
                {
                    Id = x.Id,
                    Genre = x.Name,
                    Games = x.Games
                        .Where(g=>g.Purchases.Any())
                        .Select(g => new
                        {
                            Id = g.Id,
                            Title = g.Name,
                            Developer = g.Developer.Name,
                            Tags = string.Join(", ", g.GameTags.Select(x => x.Tag.Name).ToArray()),
                            Players = g.Purchases.Count
                        })
                        .OrderByDescending(x => x.Players)
                        .ThenBy(x => x.Id).ToArray(),
                    TotalPlayers = x.Games.Sum(p => p.Purchases.Count)
                })
                .OrderByDescending(x => x.TotalPlayers)
                .ThenBy(x => x.Id)
                .ToArray();

            var result = JsonConvert.SerializeObject(games, Formatting.Indented);
            return result;
        }

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(UserExportModel[]), new XmlRootAttribute("Users"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            var purchaseType = Enum.Parse<PurchaseType>(storeType);
            var usersDto = context.Users.ToArray()
                .Where(x => x.Cards.Any(z => z.Purchases.Any()))
                .Select(u => new UserExportModel
                {
                    Username = u.Username,
                    Purchase = context.Purchases
                        .ToArray()
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseType)
                        .OrderBy(p => p.Date)
                        .Select(p => new PurchaseModel
                        {
                            Card = p.Card.Number,
                            CVC = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new GameModel
                            {
                                Genre = p.Game.Genre.Name,
                                Title = p.Game.Name,
                                Price = p.Game.Price
                            }
                        }).ToArray(),
                    TotalSpend = context.Purchases.ToArray()
                        .Where(x => x.Card.User.Username == u.Username && x.Type == purchaseType)
                        .Sum(x => x.Game.Price)
                })
                .Where(u => u.Purchase.Length > 0)
                .OrderByDescending(u => u.TotalSpend)
                .ThenBy(u => u.Username)
                .ToArray();
            xmlSerializer.Serialize(textWriter,usersDto,ns);
            return textWriter.ToString();
        }
	}
}