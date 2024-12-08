using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Models;

namespace MiFrikimundo.Data
{
    public class MiFrikimundoContext : DbContext
    {
        public MiFrikimundoContext(DbContextOptions<MiFrikimundoContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
    }
}
