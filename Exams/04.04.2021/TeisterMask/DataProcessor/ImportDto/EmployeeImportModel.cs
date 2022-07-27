using System.ComponentModel.DataAnnotations;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class EmployeeImportModel
    {
        [Required]
        [StringLength(40, MinimumLength = 3)]
        [RegularExpression("[A-z0-9]{3,}")]
        public string Username { get; set; }

        [Required] [EmailAddress] public string Email { get; set; }

        [Required]
        [RegularExpression("[0-9]{3}-[0-9]{3}-[0-9]{4}")]
        public string Phone { get; set; }

        public int[] Tasks { get; set; }
    }
}