﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MiFrikimundo.Models
{
    public class VideoGame
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Developers { get; set; }
        public string? Platform { get; set; }
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
