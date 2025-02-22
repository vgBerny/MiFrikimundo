using System.ComponentModel.DataAnnotations.Schema;

namespace MiFrikimundo.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Saga { get; set; }
        public int Rating { get; set; }
        public DateOnly? Created { get; set; }

        public int? GenderId { get; set; }
        [ForeignKey("GenderId")]
        public Gender? Gender { get; set; }
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
