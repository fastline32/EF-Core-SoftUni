using System.Xml.Serialization;

namespace CarDealer.DataTransferObjects.Output
{
    [XmlType("sale")]
    public class SaleOutputModel
    {
        [XmlElement("car")]
        public CarOutputModelSale Car { get; set; }
        [XmlElement("discount")]
        public decimal Discount { get; set; }
        [XmlElement("customer-name")]
        public string CustomerName { get; set; }
        [XmlElement("price")]
        public decimal CarPrice { get; set; }
        [XmlElement("price-with-discount")]
        public decimal CarPriceWithDiscount { get; set; }
    }
}