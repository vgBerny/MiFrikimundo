using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class SeriesController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SeriesController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
        {
            _Context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString, int? genreId, string sortOrder)
        {
            // Cargar la lista de géneros para el filtro
            ViewBag.Genres = new SelectList(await GetGenresAsync(), "Id", "Name");

            // Pasar el orden actual a la vista para resaltarlo en el UI
            ViewBag.CurrentSort = sortOrder;

            // Obtener las películas filtradas y ordenadas
            var series = await GetFilteredAndSortedSeriesAsync(searchString, genreId, sortOrder);

            return View(series);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<Serie> ApplySearchFilter(IQueryable<Serie> series, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                series = series.Where(m => m.Title.Contains(searchString));
            }
            return series;
        }
        private IQueryable<Serie> ApplyGenreFilter(IQueryable<Serie> series, int? genreId)
        {
            if (genreId.HasValue)
            {
                series = series.Where(m => m.GenderId == genreId.Value);
            }
            return series;
        }
        private IQueryable<Serie> ApplySortOrder(IQueryable<Serie> series, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => series.OrderBy(m => m.Title),
                "z_to_a" => series.OrderByDescending(m => m.Title),
                "rating_desc" => series.OrderByDescending(m => m.Rating),
                "rating_asc" => series.OrderBy(m => m.Rating),
                "date_desc" => series.OrderByDescending(m => m.Created),
                "date_asc" => series.OrderBy(m => m.Created),
                _ => series.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<Serie>> GetFilteredAndSortedSeriesAsync(string searchString, int? genreId, string sortOrder)
        {
            var series = _Context.Series.Include(m => m.Gender).AsQueryable();

            series = ApplySearchFilter(series, searchString);
            series = ApplyGenreFilter(series, genreId);
            series = ApplySortOrder(series, sortOrder);

            return await series.ToListAsync();
        }
        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Director, Chapters, Seasons, Rating, Created, ImageFile, GenderId")] Serie serie)
        {
            if (ModelState.IsValid)
            {
                if (serie.ImageFile != null && serie.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(serie.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await serie.ImageFile.CopyToAsync(fileStream);
                    }

                    serie.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    serie.ImageUrl = "/images/generic.jpg";
                }

                _Context.Series.Add(serie);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(serie);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var serie = await _Context.Series.FirstOrDefaultAsync(x => x.Id == id);

            if (serie == null)
            {
                return NotFound();
            }

            return View(serie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Director, Chapters, Seasons, Rating, Created, ImageFile, ImageUrl, GenderId")] Serie serie)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingSerie = await _Context.Series.FirstOrDefaultAsync(x => x.Id == serie.Id);

                if (existingSerie == null)
                {
                    return NotFound();
                }

                existingSerie.Title = serie.Title;
                existingSerie.Director = serie.Director;
                existingSerie.Chapters = serie.Chapters;
                existingSerie.Seasons = serie.Seasons;
                existingSerie.Rating = serie.Rating;
                existingSerie.Created = serie.Created;
                existingSerie.GenderId = serie.GenderId;

                if (serie.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(serie.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await serie.ImageFile.CopyToAsync(fileStream);
                    }

                    existingSerie.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingSerie);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View(serie);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var serie = await _Context.Series.FirstOrDefaultAsync(x =>x.Id == id);
            return View(serie);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _Context.Series.FindAsync(id);
            if(item != null)
            {
                _Context.Series.Remove(item);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}

