namespace MiFrikimundo.Models
{
    public class Gender
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Movie>? Movies { get; set; }
    }
}
