using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Models;

namespace MiFrikimundo.Data
{
    public class MiFrikimundoContext : DbContext
    {
        public MiFrikimundoContext(DbContextOptions<MiFrikimundoContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gender>().HasData(
                new Gender { Id= 1, Name="Accion"},
                new Gender { Id= 2, Name= "Drama" }
                );

            base.OnModelCreating(modelBuilder);
        }


        public DbSet<Movie> Movies { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Gender> Genders { get; set; }
    }
}
