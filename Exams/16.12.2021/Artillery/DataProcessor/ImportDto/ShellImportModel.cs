using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Shell")]
    public class ShellImportModel
    {
        [XmlElement("ShellWeight")]
        [Required]
        [Range(2,1680)]
        public double ShellWeight { get; set; }

        [XmlElement("Caliber")]
        [Required]
        [StringLength(30,MinimumLength = 4)]
        public string Caliber { get; set; }
    }
}