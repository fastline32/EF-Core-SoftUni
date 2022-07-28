using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Theatre.DataProcessor.ExportDto
{
    [XmlType("Play")]
    public class PlayExportModel
    {
        [XmlAttribute("Title")]
        public string Title { get; set; }

        [XmlAttribute("Duration")]
        public string Duration { get; set; }

        [XmlAttribute("Rating")]
        public string Rating { get; set; }
        [XmlAttribute("Genre")]
        public string Genre { get; set; }

        [XmlArray("Actors")]
        public List<ActorExportModel> Actors { get; set; }
    }

    [XmlType("Actor")]
    public class ActorExportModel
    {
        [XmlAttribute("FullName")]
        public string FullName { get; set; }

        [XmlAttribute("MainCharacter")]
        public string MainCharacter { get; set; }
    }
}

//Play Title = "A Raisin in the Sun" Duration="01:40:00" Rating="5.4" Genre="Drama">
//    <Actors>
//    <Actor FullName="Sylvia Felipe" MainCharacter="Plays main character in 'A Raisin in the Sun'." />
//    <Actor FullName="Sella Mains" MainCharacter="Plays main character in 'A Raisin in the Sun'." />
