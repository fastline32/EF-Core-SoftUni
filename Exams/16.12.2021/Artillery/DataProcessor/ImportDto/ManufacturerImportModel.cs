using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Manufacturer")]
    public class ManufacturerImportModel
    {
        [XmlElement("ManufacturerName")]
        [Required]
        [StringLength(40,MinimumLength = 4)]
        public string ManufacturerName { get; set; }

        [XmlElement("Founded")]
        [Required]
        [StringLength(100,MinimumLength = 10)]
        public string Founded { get; set; }
    }
}

//< Manufacturer >
//    < ManufacturerName > BAE Systems </ ManufacturerName >
//    < Founded > 30 November 1999, London, England </ Founded >
//    </ Manufacturer >