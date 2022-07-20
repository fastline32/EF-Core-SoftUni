using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        private static IMapper _mapper;
        public static void Main()
        {
            var context = new CarDealerContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();
            
            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }

        //Task 9
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();
            var dtoSupplier = JsonConvert.DeserializeObject<IEnumerable<SupplierInsertModel>>(inputJson);

            var supplier = _mapper.Map<IEnumerable<Supplier>>(dtoSupplier);
            context.Suppliers.AddRange(supplier);
            context.SaveChanges();

            return $"Successfully imported {supplier.Count()}.";
        }

        //Task 10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();
            List<int> suppliersIds = context.Suppliers.Select(x => x.Id).ToList();
            var dtoParts = JsonConvert.DeserializeObject<IEnumerable<PartInputModel>>(inputJson).Where(x => suppliersIds.Contains(x.SupplierId));

            var parts = _mapper.Map<IEnumerable<Part>>(dtoParts);
            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}.";
        }

        //Task 11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();
            var dtoCars = JsonConvert.DeserializeObject<IEnumerable<CarInputModel>>(inputJson);
            var cars = new List<Car>();
            foreach (var carInputModel in dtoCars)
            {
                var carInput = new Car
                {
                    Make = carInputModel.Make,
                    Model = carInputModel.Model,
                    TravelledDistance = carInputModel.TravelledDistance,
                };
                foreach (var i in carInputModel.PartsId.Distinct())
                {
                    carInput.PartCars.Add(new PartCar
                    {
                        PartId = i
                    });
                }

                cars.Add(carInput);
            }
            
            
            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count()}.";
        }
        //Task 12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();
            var dtoCustomer = JsonConvert.DeserializeObject<IEnumerable<CustomerInputModel>>(inputJson);

            var customer = _mapper.Map<IEnumerable<Customer>>(dtoCustomer);
            context.Customers.AddRange(customer);
            context.SaveChanges();

            return $"Successfully imported {customer.Count()}.";
        }

        //Task 13
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            InitializeAutoMapper();
            var dtoSales = JsonConvert.DeserializeObject<IEnumerable<SaleInputModel>>(inputJson);

            var sales = _mapper.Map<IEnumerable<Sale>>(dtoSales);
            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count()}.";
        }

        //Task 14
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(x => new
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToArray();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        //Task 15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars.Where(x => x.Make == "Toyota")
                .Select(x => new
                {
                    Id = x.Id,
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToArray();

            var result = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return result;
        }

        //Task 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var supplier = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                })
                .ToArray();

            var result = JsonConvert.SerializeObject(supplier, Formatting.Indented);
            return result;
        }

        //Task 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new
                {
                    car = new
                    {
                        x.Make,
                        x.Model,
                        x.TravelledDistance
                    },
                    parts = x.PartCars.Select(y => new
                    {
                        y.Part.Name,
                        Price = y.Part.Price.ToString("F2")
                    })
                })
                .ToList();

            return JsonConvert.SerializeObject(cars, Formatting.Indented);
        }

        //Task 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(x => x.Sales.Count > 0)
                .Select(x => new
                {
                    fullName = x.Name,
                    boughtCars = x.Sales.Count,
                    spentMoney = x.Sales.Sum(y => y.Car.PartCars.Sum(z => z.Part.Price))
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToList();

            return JsonConvert.SerializeObject(customers, Formatting.Indented);
        }

        //Task 19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(x => new
                {
                    car = new
                    {
                        x.Car.Make,
                        x.Car.Model,
                        x.Car.TravelledDistance
                    },
                    customerName = x.Customer.Name,
                    Discount = x.Discount.ToString("F2"),
                    price = x.Car.PartCars.Sum(y => y.Part.Price).ToString("F2"),
                    priceWithDiscount = (x.Car.PartCars.Sum(p => p.Part.Price) - x.Car.PartCars.Sum(p => p.Part.Price) * x.Discount / 100).ToString("F2")
                }).Take(10)
                .ToList();
            return JsonConvert.SerializeObject(sales,Formatting.Indented);
        }
        private static void InitializeAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });
            _mapper = config.CreateMapper();
        }
    }
}