using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Castle.DynamicProxy.Generators.Emitters;

namespace BookShop.DataProcessor.ImportDto
{
    public class AuthorInputModel
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"[0-9]\d{2}-\d{3}-\d{4}")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public List<BookInputModel> Books { get; set; }
    }

    public class BookInputModel
    {
        public int? Id { get; set; }
    }
}
