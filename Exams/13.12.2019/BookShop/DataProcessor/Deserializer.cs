using BookShop.Data.Models;
using BookShop.Data.Models.Enums;
using BookShop.DataProcessor.ImportDto;

namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<BookImportModel>), new XmlRootAttribute("Books"));
            var textReader = new StringReader(xmlString);
            var booksDto = xmlSerializer.Deserialize(textReader) as List<BookImportModel>;
            var books = new List<Book>();
            var sb = new StringBuilder();
            foreach (var bookImportModel in booksDto)
            {
                if (!IsValid(bookImportModel))
                {
                    sb.AppendLine("Invalid data!");
                    continue;
                }
                var book = new Book
                {
                    Name = bookImportModel.Name,
                    Pages = bookImportModel.Pages,
                    Price = bookImportModel.Price,
                    Genre = Enum.Parse<Genre>(bookImportModel.Genre),
                    PublishedOn = DateTime.ParseExact(bookImportModel.PublishedOn,"MM/dd/yyyy",CultureInfo.InvariantCulture)
                };
                sb.AppendLine($"Successfully imported book {book.Name} for {book.Price:F2}.");
                books.Add(book);
            }
            context.Books.AddRange(books);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorDtos = JsonConvert.DeserializeObject<List<AuthorInputModel>>(jsonString);
            var authors = new List<Author>();
            var sb = new StringBuilder();

            foreach (var authorDto in authorDtos)
            {
                if (!IsValid(authorDto) || authors.Any(x => x.Email == authorDto.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var author = new Author
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Phone = authorDto.Phone,
                    Email = authorDto.Email
                };

                foreach (var books in authorDto.Books)
                {
                    var book = context.Books.Find(books.Id);

                    if (book == null)
                    {
                        continue;
                    }
                    author.AuthorsBooks.Add(new AuthorBook
                    {
                        Author = author,
                        Book = book
                    });
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                authors.Add(author);
                sb.AppendLine($"Successfully imported author - {author.FirstName} {author.LastName} with {author.AuthorsBooks.Count} books.");
            }
            context.Authors.AddRange(authors);
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