﻿using System;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Prisoner")]
    public class PrisonerOutputModel
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string FullName { get; set; }

        [XmlElement("IncarcerationDate")]
        public string IncarcerationDate { get; set; }

        [XmlArray("EncryptedMessages")]
        public EncriptedMail[] Mail { get; set; }
        
    }

    [XmlType("Message")]
    public class EncriptedMail
    {
        [XmlElement("Description")]
        public string Description { get; set; }
    }
}