using System;
using System.Xml.Serialization;

namespace CarDealer.DataTransferObjects.Input
{
    [XmlType("Customer")]
    public class CustomerInputModel
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("birthDate")]
        public DateTime BirthDay { get; set; }

        [XmlElement("isYoungDriver")]
        public bool IsYoungerDriver { get; set; }
        
    }
}