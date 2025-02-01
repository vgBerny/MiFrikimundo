namespace MiFrikimundo.Models
{
    public class Gender
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Movie>? Movies { get; set; }
        public List<Book>? Books { get; set; }
        public List<Serie>? Series { get; set; }
        public List<Anime>? Animes { get; set; }

    }
}
