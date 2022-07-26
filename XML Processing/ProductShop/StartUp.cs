using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            Console.WriteLine(GetUsersWithProducts(context));

        }

        //Task 1
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<UserImportModel>), new XmlRootAttribute("Users"));
            var textReader = new StringReader(inputXml);

            var dtoUsers = xmlSerializer.Deserialize(textReader) as List<UserImportModel>;

            var users = dtoUsers.Select(x => new User
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Age = x.Age
            }).ToList();
            
            context.Users.AddRange(users);
            context.SaveChanges();
            return $"Successfully imported {users.Count}";
        }

        //Task 2
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<ProductInputModel>), new XmlRootAttribute("Products"));
            var textReader = new StringReader(inputXml);

            var dtoProducts = xmlSerializer.Deserialize(textReader) as List<ProductInputModel>;

            List<Product> products = dtoProducts.Select( x => new Product
            {
                Name = x.Name,
                Price = x.Price,
                SellerId = x.SellerId,
                BuyerId = x.BuyerId
            }).ToList();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        //Task 3
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<CategoryInputModel>), new XmlRootAttribute("Categories"));

            var textReader = new StringReader(inputXml);
            var dtoCategories = xmlSerializer.Deserialize(textReader) as List<CategoryInputModel>;

            var categories = dtoCategories.Select(x => new Category
            {
                Name = x.Name
            })
                .Where(x => x.Name != null).ToList();

            ;
            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }

        //Task 4
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CategoryProductInputModel>),
                new XmlRootAttribute("CategoryProducts"));

            var textReader = new StringReader(inputXml);

            var dtoCPInput = xmlSerializer.Deserialize(textReader) as List<CategoryProductInputModel>;
            var categoryIds = context.Categories.Select(x => x.Id).ToList();
            var productIds = context.Products.Select(x => x.Id).ToList();
            var categoryProduct = dtoCPInput.Select(x => new CategoryProduct
            {
                CategoryId = x.CategoryId,
                ProductId = x.ProductId
            }).Where(x => categoryIds.Contains(x.CategoryId) && productIds.Contains(x.ProductId)).ToList();
            
            context.CategoryProducts.AddRange(categoryProduct);
            context.SaveChanges();

            return $"Successfully imported {categoryProduct.Count}";
        }

        //Task 5
        public static string GetProductsInRange(ProductShopContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ProductsInRangeOutputModel>),
                new XmlRootAttribute("Products"));
            var dtoProducts = context.Products.Select(x => new ProductsInRangeOutputModel
                {
                    Name = x.Name,
                    BuyerName = x.Buyer.FirstName + " " + x.Buyer.LastName,
                    Price = x.Price
                })
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                .Take(10)
                .ToList();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,dtoProducts,ns);
            
            return textWriter.ToString();
        }

        //Task 6
        public static string GetSoldProducts(ProductShopContext context)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<UserSoldProductModel>), new XmlRootAttribute("Users"));

            var dtoUsers = context.Users.Where(x => x.ProductsSold.Count > 0)
                .Select(x => new UserSoldProductModel
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Type = x.ProductsSold.Select(y => new SoldProductModel
                    {
                        Name = y.Name,
                        Price = y.Price
                    }).ToList()
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .ToList();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,dtoUsers,ns);

            return textWriter.ToString();
        }

        //Task 7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<CategoryOutputModel>), new XmlRootAttribute("Categories"));

            var dtoCategories = context.Categories
                .Select(x => new CategoryOutputModel
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count,
                    AveragePrice = x.CategoryProducts.Average(y => y.Product.Price),
                    TotalRevenue = x.CategoryProducts.Sum(y => y.Product.Price)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToList();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,dtoCategories,ns);
            return textWriter.ToString();
        }
        //Task 8
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var dtoUsers = context.Users
                .Where(x => x.ProductsSold.Count > 0)
                .Select( x => new ProductOutputModel
                {
                    ProductsCount = x.ProductsSold.Count,
                    SoldProducts = x.ProductsSold.Select(x => )
                })
                .OrderByDescending(x => x.ProductsSold.Count).ToList();
            //TODO:
            return null;
        }
    }
}