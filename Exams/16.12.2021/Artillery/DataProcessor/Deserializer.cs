using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Artillery.Data.Models;
using Artillery.Data.Models.Enums;
using Artillery.DataProcessor.ImportDto;
using Newtonsoft.Json;

namespace Artillery.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Artillery.Data;

    public class Deserializer
    {
        private const string ErrorMessage =
                "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<CountriesImportModel>), new XmlRootAttribute("Countries"));
            var textReader = new StringReader(xmlString);
            var countriesDto = xmlSerializer.Deserialize(textReader) as List<CountriesImportModel>;
            var countries = new List<Country>();
            var sb = new StringBuilder();
            foreach (var countryDto in countriesDto)
            {
                if (!IsValid(countryDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var country = new Country
                {
                    ArmySize = countryDto.ArmySize,
                    CountryName = countryDto.CountryName
                };
                countries.Add(country);
                sb.AppendLine(String.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));
            }
            context.Countries.AddRange(countries);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ManufacturerImportModel>),
                new XmlRootAttribute("Manufacturers"));
            var textReader = new StringReader(xmlString);
            var manufacturersDto = xmlSerializer.Deserialize(textReader) as List<ManufacturerImportModel>;
            var sb = new StringBuilder();
            var manufactorers = new List<Manufacturer>();

            foreach (var manufacturerDto in manufacturersDto)
            {
                if (!IsValid(manufacturerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (manufactorers.Count > 0)
                {
                    var manifacturerNameTest =
                        manufactorers.Any(x => x.ManufacturerName == manufacturerDto.ManufacturerName);
                    if (manifacturerNameTest)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                }
                var manifaturer = new Manufacturer
                {
                    ManufacturerName = manufacturerDto.ManufacturerName,
                    Founded = manufacturerDto.Founded,
                };
                string[] importData = manifaturer.Founded.Split(", ");
                manufactorers.Add(manifaturer);
                sb.AppendLine($"Successfully import manufacturer {manifaturer.ManufacturerName} founded in {importData[importData.Length-2]}, {importData[importData.Length-1]}.");
            }
            context.Manufacturers.AddRange(manufactorers);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<ShellImportModel>), new XmlRootAttribute("Shells"));
            var textReader = new StringReader(xmlString);
            var shellsDto = xmlSerializer.Deserialize(textReader) as List<ShellImportModel>;
            var shells = new List<Shell>();
            var sb = new StringBuilder();
            foreach (var shellDto in shellsDto)
            {
                if (!IsValid(shellDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                shells.Add(new Shell{Caliber = shellDto.Caliber,ShellWeight = shellDto.ShellWeight});
                sb.AppendLine(String.Format(SuccessfulImportShell, shellDto.Caliber, shellDto.ShellWeight));
            }
            context.Shells.AddRange(shells);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            var gunsDto = JsonConvert.DeserializeObject<List<GunImporModel>>(jsonString);
            var guns = new List<Gun>();
            var sb = new StringBuilder();
            foreach (var gunDto in gunsDto)
            {
                if (!IsValid(gunDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var gun = new Gun
                {
                    ManufacturerId = gunDto.ManufacturerId,
                    BarrelLength = gunDto.BarrelLength,
                    GunWeight = gunDto.GunWeight,
                    Range = gunDto.Range,
                    ShellId = gunDto.ShellId,
                    CountriesGuns = gunDto.Countries.Select(x => new CountryGun
                    {
                        CountryId = x.Id
                    }).ToList()
                };
                if (gunDto.NumberBuild.HasValue)
                {
                    gun.NumberBuild = gunDto.NumberBuild;
                }

                var isValidGunType = Enum.TryParse( gunDto.GunType, out GunType gunType);
                if (!isValidGunType)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                gun.GunType = gunType;
                guns.Add(gun);
                sb.AppendLine(String.Format(SuccessfulImportGun, gun.GunType.ToString(), gun.GunWeight,
                    gun.BarrelLength));
            }
            context.Guns.AddRange(guns);
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
