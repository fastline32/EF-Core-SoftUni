using System.Xml.Serialization;

namespace CarDealer.DataTransferObjects.Output
{
    [XmlType("suplier")]
    public class SupplierOutModel
    {
        [XmlAttribute("id")]
        public int SupplierId { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("parts-count")]
        public int PartsCount { get; set; }
    }
}