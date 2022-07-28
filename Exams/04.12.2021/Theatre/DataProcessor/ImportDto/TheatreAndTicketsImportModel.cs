using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Theatre.DataProcessor.ImportDto
{
    public class TheatreAndTicketsImportModel
    {
        [Required]
        [StringLength(30,MinimumLength = 4)]
        public string Name { get; set; }

        [Required]
        [Range(1,10)]
        public sbyte NumberOfHalls { get; set; }

        [Required]
        [StringLength(30,MinimumLength = 4)]
        public string Director { get; set; }

        public List<TicketsInputModel> Tickets { get; set; }
    }

    public class TicketsInputModel
    {
        [Range(typeof(decimal),"1.00","100.00")]
        public decimal Price { get; set; }

        [Required]
        [Range(1,10)]
        public sbyte RowNumber { get; set; }

        public int PlayId { get; set; }
    }
}