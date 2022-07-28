
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Artillery.DataProcessor.ExportDto;
using Newtonsoft.Json;

namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using System;

    public class Serializer
    {
        public static string ExportShells(ArtilleryContext context, double shellWeight)
        {
            var shellsDto = context.Shells.Where(x => x.ShellWeight > shellWeight)
                .ToList()
                .Select(x => new ShellExportModel
                {
                    ShellWeight = x.ShellWeight,
                    Caliber = x.Caliber,
                    Guns = x.Guns
                        .Where(x => x.GunType.ToString() == "AntiAircraftGun")
                        .ToList()
                        .Select(g => new GunsExportModel
                        {
                            GunType = g.GunType.ToString(),
                            GunWeight = g.GunWeight,
                            BarrelLength = g.BarrelLength,
                            Range = g.Range > 3000 ? "Long-range" : "Regular range"
                        })
                    .OrderByDescending(g => g.GunWeight)
                        .ToArray()
                })
                .OrderBy(x => x.ShellWeight)
                .ToList();
            var json = JsonConvert.SerializeObject(shellsDto, Formatting.Indented);
            return json;
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(List<ExportGunModelXml>), new XmlRootAttribute("Guns"));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("","");
            var textWriter = new StringWriter();
            var gunsDto = context.Guns.Where(x => x.Manufacturer.ManufacturerName == manufacturer)
                .ToList()
                .OrderBy(x => x.BarrelLength)
                .Select(x => new ExportGunModelXml
                {
                    Manufacturer = x.Manufacturer.ManufacturerName,
                    GunType = x.GunType.ToString(),
                    GunWeight = x.GunWeight.ToString(),
                    BarrelLength = x.BarrelLength.ToString(),
                    Range = x.Range.ToString(),
                    Country = x.CountriesGuns
                        .Where(c => c.Country.ArmySize > 4500000)
                        .Select(c => new CountryExportModel
                    {
                        Country = c.Country.CountryName,
                        ArmySize = c.Country.ArmySize.ToString(),
                    })
                        .OrderBy(c => c.ArmySize)
                        .ToArray()
                })
                .ToList();
            xmlSerializer.Serialize(textWriter,gunsDto,ns);
            return textWriter.ToString();
        }
    }
}
