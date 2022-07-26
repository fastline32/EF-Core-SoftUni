using Newtonsoft.Json;

namespace BookShop.Data.Models
{
    public class AuthorBook
    {
        public int AuthorId { get; set; }
        [JsonIgnore]
        public Author Author { get; set; }
        public int BookId { get; set; }
        [JsonIgnore]
        public Book Book { get; set; }
    }
}