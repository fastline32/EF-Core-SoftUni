using System.Collections.Generic;
using BookShop.Data.Models.Enums;
using BookShop.DataProcessor.ExportDto;

namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors
                .Select(x => new
                {
                    AuthorName = x.FirstName + " " + x.LastName,
                    Books = x.AuthorsBooks
                        .Select(ab => ab.Book)
                        .OrderByDescending(z => z.Price)
                        .Select(z => new
                            {
                                BookName = z.Name,
                                BookPrice = z.Price.ToString("F2"),
                            })
                        .ToArray()
                })
                .ToArray()
                .OrderByDescending(x => x.Books.Length)
                .ThenBy(x => x.AuthorName)
                .ToArray();
            var json = JsonConvert.SerializeObject(authors,Formatting.Indented);
            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<BookOutputModel>), new XmlRootAttribute("Books"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            var booksDto = context.Books.Where(x => x.PublishedOn < date && x.Genre == Genre.Science)
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.PublishedOn)
                .Select(x => new BookOutputModel
                {
                    Pages = x.Pages,
                    Name = x.Name,
                    Date = x.PublishedOn.ToString("MM/dd/yyyy")
                })
                .Take(10)
                .ToList();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,booksDto,ns);
            return textWriter.ToString();
        }
    }
}