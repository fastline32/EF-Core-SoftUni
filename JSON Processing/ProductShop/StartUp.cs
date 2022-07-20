using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DTOs;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private static IMapper _mapper;
        public static void Main()
        {
            var context = new ProductShopContext();

            var result = GetUsersWithProducts(context);

            Console.WriteLine(result);
        }

        //Task 1
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            InitializeAutoMapper();
            var dtoUsers = JsonConvert.DeserializeObject<IEnumerable<UserInputModel>>(inputJson);

            var users = _mapper.Map<IEnumerable<User>>(dtoUsers);
            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }

        //Task 2
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);
            
            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Task 3
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson).Where(c => c.Name != null);

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }

        //Task 4
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var catProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(catProducts);
            context.SaveChanges();

            return $"Successfully imported {catProducts.Length}";
        }

        //Task 5
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(prod => new
                {
                    name = prod.Name,
                    price = prod.Price,
                    seller = prod.Seller.FirstName + " " + prod.Seller.LastName
                })
                .OrderBy(x => x.price)
                .ToArray();

            var result = JsonConvert.SerializeObject(products, Formatting.Indented);

            return result;
        }

        //Task 6
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any(p => p.BuyerId != null))
                .Select(x => new
                {
                    firstName = x.FirstName,
                    lastName = x.LastName,
                    soldProducts = x.ProductsSold.Where(p => p.BuyerId != null).Select(pro => new
                    {
                        name = pro.Name,
                        price = pro.Price,
                        buyerFirstName = pro.Buyer.FirstName,
                        buyerLastName = pro.Buyer.LastName
                    }).ToList()
                })
                .OrderBy(x => x.lastName)
                .ThenBy(x => x.firstName)
                .ToArray();
            var result = JsonConvert.SerializeObject(users, Formatting.Indented);

            return result;
        }

        //Task 7
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(x => new
                {
                    category = x.Name,
                    productsCount = x.CategoryProducts.Count,
                    averagePrice = x.CategoryProducts.Average(p => p.Product.Price).ToString("F2"),
                    totalRevenue = x.CategoryProducts.Sum(p => p.Product.Price).ToString("F2")
                })
                .OrderByDescending(x => x.productsCount)
                .ToArray();

            var result = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return result;
        }

        //Task 8
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .ToList()
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Where(p => p.BuyerId != null).Count(),
                        products = u.ProductsSold.Where(p => p.BuyerId != null)
                            .Select(p => new
                            {
                                name = p.Name,
                                price = p.Price
                            })
                    }
                })
                .OrderByDescending(u => u.soldProducts.count)
                .ToList();

            var newObjectOfUsers = new
            {
                usersCount = users.Count,
                users = users

            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return JsonConvert.SerializeObject(newObjectOfUsers, settings);
        }
        private static void InitializeAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });
            _mapper = config.CreateMapper();
        }
    }
}