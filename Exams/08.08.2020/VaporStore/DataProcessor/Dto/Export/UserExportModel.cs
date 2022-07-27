using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VaporStore.DataProcessor.Dto.Import;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("User")]
    public class UserExportModel
    {
        [XmlAttribute("username")]
        public string Username { get; set; }

        [XmlArray("Purchases")]
        public PurchaseModel[] Purchase { get; set; }
        [XmlElement("TotalSpent")]
        public decimal TotalSpend { get; set; }
    }

    [XmlType("Purchase")]
    public class PurchaseModel
    {
        [XmlElement("Card")]
        public string Card { get; set; }
        [XmlElement("Cvc")]
        public string CVC { get; set; }

        [XmlElement("Date")]
        public string Date { get; set; }

        [XmlElement("Game")]
        public GameModel Game { get; set; }
    }

    public class GameModel
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlElement("Genre")]
        public string Genre { get; set; }
        [XmlElement("Price")]
        public decimal Price { get; set; }
    }
}                                        

