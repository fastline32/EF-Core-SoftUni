using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VaporStore.Data.Models;
using VaporStore.Data.Models.Enums;
using VaporStore.DataProcessor.Dto.Import;

namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Data;

	public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            var gamesDto = JsonConvert.DeserializeObject<List<GameImportDto>>(jsonString);
            var sb = new StringBuilder();
            List<Developer> developers = new List<Developer>();
            List<Tag> tags = new List<Tag>();
            List<Genre> genres = new List<Genre>();
            List<Game> games = new List<Game>();
            foreach (var gameDto in gamesDto)
            {
                if (!IsValid(gameDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                DateTime releaseDate;
                bool isValidDate = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);
                if (!isValidDate)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                if (gameDto.Tags.Length == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var game = new Game
                {
                    Name = gameDto.Name,
                    Price = gameDto.Price,
                    ReleaseDate = releaseDate
                };

                var developer = developers.FirstOrDefault(d => d.Name == gameDto.Developer);
                if (developer == null)
                {
                    var newDev = new Developer
                    {
                        Name = gameDto.Developer
                    };
                    developers.Add(newDev);
                    game.Developer = newDev;
                }
                else
                {
                    game.Developer = developer;
                }
                Genre gameGenre = genres
                    .FirstOrDefault(g => g.Name == gameDto.Genre);

                if (gameGenre == null)
                {
                    Genre newGenre = new Genre()
                    {
                        Name = gameDto.Genre
                    };

                    genres.Add(newGenre);
                    game.Genre = newGenre;
                }
                else
                {
                    game.Genre = gameGenre;
                }

                foreach (var tagName in gameDto.Tags)
                {
                    if (String.IsNullOrEmpty(tagName))
                    {
                        continue;
                    }

                    Tag gameTag = tags
                        .FirstOrDefault(t => t.Name == tagName);

                    if (gameTag == null)
                    {
                        Tag newGameTag = new Tag()
                        {
                            Name = tagName
                        };

                        tags.Add(newGameTag);
                        game.GameTags.Add(new GameTag()
                        {
                            Game = game,
                            Tag = newGameTag
                        });
                    }
                    else
                    {
                        game.GameTags.Add(new GameTag()
                        {
                            Game = game,
                            Tag = gameTag
                        });
                    }
                }

                if (game.GameTags.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                games.Add(game);
                sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
            }

            context.Games.AddRange(games);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            var usersDto = JsonConvert.DeserializeObject<List<UserImportDto>>(jsonString);
            var users = new List<User>();
            var sb = new StringBuilder();
            foreach (var userDto in usersDto)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                bool areAllCardValid = true;
                List<Card> cards = new List<Card>();
                foreach (var dtoCard in userDto.Cards)
                {
                    if (!IsValid(dtoCard))
                    {
                        sb.AppendLine("Invalid Data");
                        break;
                    }

                    
                    Object cardResult;
                    var cardType = Enum.TryParse(typeof(CardType),dtoCard.Type,out cardResult);
                    if (!cardType)
                    {
                        areAllCardValid = false;
                        break;
                    }

                    var cardsTypes = (CardType) cardResult;
                    cards.Add(new Card
                    {
                        Cvc = dtoCard.CVC,
                        Type = cardsTypes,
                        Number = dtoCard.Number
                    });
                }

                if (!areAllCardValid)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                if (cards.Count==0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var user = new User
                {
                    Age = userDto.Age,
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    Username = userDto.Username,
                    Cards = cards
                };
                users.Add(user);
                sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }
            context.Users.AddRange(users);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<PurchaseInputModel>), new XmlRootAttribute("Purchases"));
            var textReader = new StringReader(xmlString);
            var purchasesDto = xmlSerializer.Deserialize(textReader) as List<PurchaseInputModel>;
            List<Purchase> purchases = new List<Purchase>();
            var sb = new StringBuilder();
            foreach (var model in purchasesDto)
            {
                DateTime purchaseDate;
                var isValidDate = DateTime
                    .TryParseExact(model.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture,DateTimeStyles.None,out purchaseDate);

                Object purchaseType;
                var isValidPurchaseType = Enum.TryParse(typeof(PurchaseType), model.Type, out purchaseType);
            
                var type = (PurchaseType) purchaseType;
                var card = context.Cards.FirstOrDefault(x => x.Number == model.Card);
              
                var game = context.Games.FirstOrDefault(x => x.Name == model.Title);
                var purchase = new Purchase
                {
                    Type = type,
                    Card = card,
                    Date = purchaseDate,
                    ProductKey = model.Key,
                    Game = game
                };
                var user = context.Users.Where(x => x.Cards.Any(x=>x.Number==card.Number)).Select(x => x.Username).Single();
                purchases.Add(purchase);
                sb.AppendLine($"Imported {game.Name} for {user}");
            }
            context.AddRange(purchases);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}