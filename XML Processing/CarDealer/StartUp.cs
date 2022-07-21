using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CarDealer.Data;
using CarDealer.DataTransferObjects.Input;
using CarDealer.DataTransferObjects.Output;
using CarDealer.Models;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            var result = GetSalesWithAppliedDiscount(context);
            Console.WriteLine(result);

        }

        //Task 9
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SupplierInputModel[])
                , new XmlRootAttribute("Suppliers"));
            var reader = new StringReader(inputXml);

            var suppliersDto = xmlSerializer.Deserialize(reader) as SupplierInputModel[];

            var suppliers = suppliersDto!.Select(x => new Supplier
            {
                Name = x.Name,
                IsImporter = x.IsImporter
            });

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}";
        }

        //Task 10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var suppliersId = context.Suppliers.Select(x => x.Id).ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PartInputModel>)
                , new XmlRootAttribute("Parts"));
            var reader = new StringReader(inputXml);

            var partsDto = xmlSerializer.Deserialize(reader) as List<PartInputModel>;

            var parts = partsDto
                .Select(x => new Part
                {
                    Name = x.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    SupplierId = x.SupplierId
                })
                .Where(x => suppliersId.Contains(x.SupplierId))
                .ToList();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        //Task 11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarInputModel>), new XmlRootAttribute("Cars"));
            var reader = new StringReader(inputXml);
            
            List<CarInputModel> dtoCars = xmlSerializer.Deserialize(reader) as List<CarInputModel>;

            List<int> existingPartIds = context.Parts.Select(x => x.Id).ToList();

            List<Car> cars = dtoCars.Select(x => new Car
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TraveledDistance,
                    PartCars = x.PartsIds.Select(x => x.Id).Intersect(existingPartIds).Distinct()
                        .Select(y => new PartCar
                        {
                            PartId = y
                        })
                        .ToList(),
                })
                .ToList();

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        //Task 12
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CustomerInputModel>), new XmlRootAttribute("Customers"));
            var reader = new StringReader(inputXml);

            var dtoCustomers = xmlSerializer.Deserialize(reader) as List<CustomerInputModel>;

            var customers = dtoCustomers.Select(x => new Customer
            {
                Name = x.Name,
                BirthDate = x.BirthDay,
                IsYoungDriver = x.IsYoungerDriver
            }).ToList();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        //Task 13
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SaleInputModel>), new XmlRootAttribute("Sales"));
            var reader = new StringReader(inputXml);

            var dtoSales = xmlSerializer.Deserialize(reader) as List<SaleInputModel>;

            var carsId = context.Cars.Select(x => x.Id).ToList();

            var sales = dtoSales.Where(x => carsId.Contains(x.CarId))
                .Select(y => new Sale
                {
                    CarId = y.CarId,
                    CustomerId = y.CustomerId,
                    Discount = y.Discount
                }).ToList();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        //Task 14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars.Where(x => x.TravelledDistance > 2000000)
                .Select(x => new CarOutputModel
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarOutputModel>), new XmlRootAttribute("cars"));

            var textWriten = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(textWriten,cars,ns);

            return textWriten.ToString();
        }

        //Task 15
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars.Where(x => x.Make == "BMW")
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .Select(x => new CarBmwModel
                {
                    CarId = x.Id,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                }).ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarBmwModel>), new XmlRootAttribute("cars"));
            var textWriter = new StringWriter();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");

            xmlSerializer.Serialize(textWriter,cars,ns);

            return textWriter.ToString();
        }

        //Task 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new SupplierOutModel
                {
                    SupplierId = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                }).ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SupplierOutModel>), new XmlRootAttribute("suppliers"));

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, suppliers,ns);
            return textWriter.ToString();
        }

        //Task 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var dtoCars = context.Cars
                .Select(x => new CarPartOutputModel
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars.Select(p => new PartCarOutputModel
                        {
                            Name = p.Part.Name,
                            Price = p.Part.Price
                        })
                        .OrderByDescending(x => x.Price)
                        .ToList()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CarPartOutputModel>), new XmlRootAttribute("cars"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,dtoCars,ns);
            return textWriter.ToString();
        }

        //Task 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customer = context.Customers.Where(x => x.Sales.Count != 0)
                .Select(x => new CustomerOutputModel
                {
                    Name = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpendMoney = x.Sales.Select(y => y.Car).SelectMany(z => z.PartCars).Sum(a => a.Part.Price)
                })
                .OrderByDescending(x => x.SpendMoney)
                .ToList();

            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<CustomerOutputModel>), new XmlRootAttribute("customers"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,customer,ns);
            return textWriter.ToString();
        }
        //Task 19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(x => new SaleOutputModel
                {
                    Car = new CarOutputModelSale
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    Discount = x.Discount,
                    CustomerName = x.Customer.Name,
                    CarPrice = x.Car.PartCars.Sum(x => x.Part.Price),
                    CarPriceWithDiscount = x.Car.PartCars.Sum(x => x.Part.Price) -
                                           x.Car.PartCars.Sum(x => x.Part.Price) * x.Discount / 100
                }).ToList();

            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<SaleOutputModel>), new XmlRootAttribute("sales"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter,sales,ns);


            return textWriter.ToString();
        }
    }
}