using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication.ExtendedProtection;
using System.Xml.Serialization;
using TeisterMask.Data.Models;

namespace TeisterMask.DataProcessor.ImportDto
{
    [XmlType("Project")]
    public class ProjectInputModel
    {
        [XmlElement("Name")]
        [Required]
        [StringLength(40,MinimumLength = 2)]
        public string Name { get; set; }

        [XmlElement("OpenDate")]
        [Required]
        public string OpenDate { get; set; }

        public string DueDate { get; set; }
        [XmlArray("Tasks")]
        public TaskInputModel[] Tasks { get; set; }
        
    }

    [XmlType("Task")]
    public class TaskInputModel
    {
        [XmlElement("Name")]
        [Required]
        [StringLength(40,MinimumLength = 2)]
        public string Name { get; set; }

        [XmlElement("OpenDate")]
        [Required]
        public string OpenDate { get; set; }

        [XmlElement("DueDate")]
        [Required]
        public string DueDate { get; set; }

        [XmlElement("ExecutionType")]
        [Required]
        public string ExecutionType { get; set; }

        [XmlElement("LabelType")]
        [Required]
        public string LabelType { get; set; }
    }
}

//< Project >
//    < Name > S </ Name >
//    < OpenDate > 25 / 01 / 2018 </ OpenDate >
//    < DueDate > 16 / 08 / 2019 </ DueDate >
//    < Tasks >
//    < Task >
//    < Name > Australian </ Name >
//    < OpenDate > 19 / 08 / 2018 </ OpenDate >
//    < DueDate > 13 / 07 / 2019 </ DueDate >
//    < ExecutionType > 2 </ ExecutionType >
//    < LabelType > 0 </ LabelType >
//    </ Task >
//    < Task >
//    < Name > Upland Boneset </ Name >
//    < OpenDate > 24 / 10 / 2018 </ OpenDate >
//    < DueDate > 11 / 06 / 2019 </ DueDate >
//    < ExecutionType > 2 </ ExecutionType >
//    < LabelType > 3 </ LabelType >
//    </ Task >
//    </ Tasks >
//    </ Project >
