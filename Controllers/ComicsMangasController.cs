using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class ComicsMangasController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ComicsMangasController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
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
            var comics_mangas = await GetFilteredAndSortedComicsMangasAsync(searchString, genreId, sortOrder);

            return View(comics_mangas);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<ComicManga> ApplySearchFilter(IQueryable<ComicManga> comics_mangas, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                comics_mangas = comics_mangas.Where(m => m.Title.Contains(searchString));
            }
            return comics_mangas;
        }
        private IQueryable<ComicManga> ApplyGenreFilter(IQueryable<ComicManga> comics_mangas, int? genreId)
        {
            if (genreId.HasValue)
            {
                comics_mangas = comics_mangas.Where(m => m.GenderId == genreId.Value);
            }
            return comics_mangas;
        }
        private IQueryable<ComicManga> ApplySortOrder(IQueryable<ComicManga> comics_mangas, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => comics_mangas.OrderBy(m => m.Title),
                "z_to_a" => comics_mangas.OrderByDescending(m => m.Title),
                "rating_desc" => comics_mangas.OrderByDescending(m => m.Rating),
                "rating_asc" => comics_mangas.OrderBy(m => m.Rating),
                "date_desc" => comics_mangas.OrderByDescending(m => m.Created),
                "date_asc" => comics_mangas.OrderBy(m => m.Created),
                _ => comics_mangas.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<ComicManga>> GetFilteredAndSortedComicsMangasAsync(string searchString, int? genreId, string sortOrder)
        {
            var comics_mangas = _Context.ComicsMangas.Include(m => m.Gender).AsQueryable();

            comics_mangas = ApplySearchFilter(comics_mangas, searchString);
            comics_mangas = ApplyGenreFilter(comics_mangas, genreId);
            comics_mangas = ApplySortOrder(comics_mangas, sortOrder);

            return await comics_mangas.ToListAsync();
        }
        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Author, Chapters, Volumes, Rating, Created, ImageFile, GenderId")] ComicManga comic_manga)
        {
            if (ModelState.IsValid)
            {
                if (comic_manga.ImageFile != null && comic_manga.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(comic_manga.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await comic_manga.ImageFile.CopyToAsync(fileStream);
                    }

                    comic_manga.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    comic_manga.ImageUrl = "/images/generic.jpg";
                }

                _Context.ComicsMangas.Add(comic_manga);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(comic_manga);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var comic_manga = await _Context.ComicsMangas.FirstOrDefaultAsync(x => x.Id == id);

            if (comic_manga == null)
            {
                return NotFound();
            }

            return View(comic_manga);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Author, Chapters, Volumes, Rating, Created, ImageFile, ImageUrl, GenderId")] ComicManga comic_manga)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingComicManga = await _Context.ComicsMangas.FirstOrDefaultAsync(x => x.Id == comic_manga.Id);

                if (existingComicManga == null)
                {
                    return NotFound();
                }

                existingComicManga.Title = comic_manga.Title;
                existingComicManga.Author = comic_manga.Author;
                existingComicManga.Chapters = comic_manga.Chapters;
                existingComicManga.Volumes = comic_manga.Volumes;
                existingComicManga.Rating = comic_manga.Rating;
                existingComicManga.Created = comic_manga.Created;
                existingComicManga.GenderId = comic_manga.GenderId;

                if (comic_manga.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(comic_manga.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await comic_manga.ImageFile.CopyToAsync(fileStream);
                    }

                    existingComicManga.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingComicManga);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View(comic_manga);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var comic_manga = await _Context.ComicsMangas.FirstOrDefaultAsync(x =>x.Id == id);
            return View(comic_manga);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _Context.ComicsMangas.FindAsync(id);
            if(item != null)
            {
                _Context.ComicsMangas.Remove(item);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}

