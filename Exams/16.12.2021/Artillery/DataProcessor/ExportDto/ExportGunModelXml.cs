using System.Xml.Serialization;

namespace Artillery.DataProcessor.ExportDto
{
    [XmlType("Gun")]
    public class ExportGunModelXml
    {
        [XmlAttribute("Manufacturer")]
        public string Manufacturer { get; set; }

        [XmlAttribute("GunType")]
        public string GunType { get; set; }

        [XmlAttribute("GunWeight")]
        public string GunWeight { get; set; }

        [XmlAttribute("BarrelLength")]
        public string BarrelLength { get; set; }

        [XmlAttribute("Range")]
        public string Range { get; set; }

        [XmlArray("Countries")]
        public CountryExportModel[] Country { get; set; }
    }

    [XmlType("Country")]
    public class CountryExportModel
    {
        [XmlAttribute("Country")]
        public string Country { get; set; }

        [XmlAttribute("ArmySize")]
        public string ArmySize { get; set; }
    }
}
