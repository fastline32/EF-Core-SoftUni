using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Theatre.Data.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public sbyte RowNumber { get; set; }

        [ForeignKey("Play")]
        public int PlayId { get; set; }

        public Play Play { get; set; }

        [ForeignKey("Theatre")]
        public int TheatreId { get; set; }

        public Theatre Theatre { get; set; }
    }
}

