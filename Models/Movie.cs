﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MiFrikimundo.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Director { get; set; }
        public int Rating { get; set; }
        public DateOnly? Created { get; set; }
        public string? ImageUrl { get; set; }
        public int? GenderId { get; set; }
        [ForeignKey("GenderId")]
        public Gender? Gender { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
