using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("SoldProducts")]
    public class ProductOutputModel
    {
        [XmlElement("count")]
        public int ProductsCount { get; set; }
        [XmlArray("products")]
        public List<SoldProductModel> SoldProducts { get; set; }
    }
}