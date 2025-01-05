using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MoviesController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
        {
            _Context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, int? genreId, string sortOrder)
        {
            // Cargar la lista de géneros para el filtro
            ViewBag.Genres = new SelectList(await _Context.Genders.ToListAsync(), "Id", "Name");

            // Pasar el orden actual a la vista para resaltarlo en el UI
            ViewBag.CurrentSort = sortOrder;

            // Base query para las películas
            var movies = _Context.Movies.Include(g => g.Gender).AsQueryable();

            // Filtrar por búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(b => b.Title.Contains(searchString));
            }

            // Filtrar por género
            if (genreId.HasValue)
            {
                movies = movies.Where(b => b.GenderId == genreId.Value);
            }

            // Ordenar según el criterio seleccionado
            switch (sortOrder)
            {
                case "rating_desc":
                    movies = movies.OrderByDescending(m => m.Rating);
                    break;
                case "rating_asc":
                    movies = movies.OrderBy(m => m.Rating);
                    break;
                case "date_desc":
                    movies = movies.OrderByDescending(m => m.Created);
                    break;
                case "date_asc":
                    movies = movies.OrderBy(m => m.Created);
                    break;
                default:
                    movies = movies.OrderBy(m => m.Title); // Orden predeterminado (por título)
                    break;
            }

            // Retornar la lista filtrada y ordenada a la vista
            return View(await movies.ToListAsync());
        }



        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Director, Rating, Created, ImageFile, ImageUre, GenderId")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                if (movie.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(movie.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await movie.ImageFile.CopyToAsync(fileStream);
                    }

                    movie.ImageUrl = "/images/" + fileName;
                }

                _Context.Movies.Add(movie);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var movie = await _Context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            return View(movie);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Director, Rating, Created, ImageFile, ImageUrl, GenderId")] Movie movie)
        {
            if(ModelState.IsValid)
            {
                if (movie.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(movie.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await movie.ImageFile.CopyToAsync(fileStream);
                    }

                    movie.ImageUrl = "/images/" + fileName;
                }
                _Context.Update(movie);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _Context.Movies.FirstOrDefaultAsync(x =>x.Id == id);
            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _Context.Movies.FindAsync(id);
            if(item != null)
            {
                _Context.Movies.Remove(item);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}

