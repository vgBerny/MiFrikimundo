using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class AnimesController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AnimesController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
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
            var animes = await GetFilteredAndSortedAnimesAsync(searchString, genreId, sortOrder);

            return View(animes);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<Anime> ApplySearchFilter(IQueryable<Anime> animes, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                animes = animes.Where(m => m.Title.Contains(searchString));
            }
            return animes;
        }
        private IQueryable<Anime> ApplyGenreFilter(IQueryable<Anime> animes, int? genreId)
        {
            if (genreId.HasValue)
            {
                animes = animes.Where(m => m.GenderId == genreId.Value);
            }
            return animes;
        }
        private IQueryable<Anime> ApplySortOrder(IQueryable<Anime> animes, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => animes.OrderBy(m => m.Title),
                "z_to_a" => animes.OrderByDescending(m => m.Title),
                "rating_desc" => animes.OrderByDescending(m => m.Rating),
                "rating_asc" => animes.OrderBy(m => m.Rating),
                "date_desc" => animes.OrderByDescending(m => m.Created),
                "date_asc" => animes.OrderBy(m => m.Created),
                _ => animes.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<Anime>> GetFilteredAndSortedAnimesAsync(string searchString, int? genreId, string sortOrder)
        {
            var animes = _Context.Animes.Include(m => m.Gender).AsQueryable();

            animes = ApplySearchFilter(animes, searchString);
            animes = ApplyGenreFilter(animes, genreId);
            animes = ApplySortOrder(animes, sortOrder);

            return await animes.ToListAsync();
        }
        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Authot, Chapters, Seasons, Rating, Created, ImageFile, GenderId")] Anime anime)
        {
            if (ModelState.IsValid)
            {
                if (anime.ImageFile != null && anime.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(anime.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await anime.ImageFile.CopyToAsync(fileStream);
                    }

                    anime.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    anime.ImageUrl = "/images/generic.jpg";
                }

                _Context.Animes.Add(anime);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(anime);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var anime = await _Context.Animes.FirstOrDefaultAsync(x => x.Id == id);

            if (anime == null)
            {
                return NotFound();
            }

            return View(anime);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Author, Chapters, Seasons, Rating, Created, ImageFile, ImageUrl, GenderId")] Anime anime)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingAnime = await _Context.Animes.FirstOrDefaultAsync(x => x.Id == anime.Id);

                if (existingAnime == null)
                {
                    return NotFound();
                }

                existingAnime.Title = anime.Title;
                existingAnime.Author = anime.Author;
                existingAnime.Chapters = anime.Chapters;
                existingAnime.Seasons = anime.Seasons;
                existingAnime.Rating = anime.Rating;
                existingAnime.Created = anime.Created;
                existingAnime.GenderId = anime.GenderId;

                if (anime.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(anime.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await anime.ImageFile.CopyToAsync(fileStream);
                    }

                    existingAnime.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingAnime);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View(anime);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var anime = await _Context.Animes.FirstOrDefaultAsync(x =>x.Id == id);
            return View(anime);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _Context.Animes.FindAsync(id);
            if(item != null)
            {
                _Context.Animes.Remove(item);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}

