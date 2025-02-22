using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class VideoGamesController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VideoGamesController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
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
            var video_games = await GetFilteredAndSortedVideoGamesAsync(searchString, genreId, sortOrder);

            return View(video_games);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<VideoGame> ApplySearchFilter(IQueryable<VideoGame> video_games, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                video_games = video_games.Where(m => m.Title.Contains(searchString));
            }
            return video_games;
        }
        private IQueryable<VideoGame> ApplyGenreFilter(IQueryable<VideoGame> video_games, int? genreId)
        {
            if (genreId.HasValue)
            {
                video_games = video_games.Where(m => m.GenderId == genreId.Value);
            }
            return video_games;
        }
        private IQueryable<VideoGame> ApplySortOrder(IQueryable<VideoGame> video_games, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => video_games.OrderBy(m => m.Title),
                "z_to_a" => video_games.OrderByDescending(m => m.Title),
                "rating_desc" => video_games.OrderByDescending(m => m.Rating),
                "rating_asc" => video_games.OrderBy(m => m.Rating),
                "date_desc" => video_games.OrderByDescending(m => m.Created),
                "date_asc" => video_games.OrderBy(m => m.Created),
                _ => video_games.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<VideoGame>> GetFilteredAndSortedVideoGamesAsync(string searchString, int? genreId, string sortOrder)
        {
            var video_games = _Context.VideoGames.Include(m => m.Gender).AsQueryable();

            video_games = ApplySearchFilter(video_games, searchString);
            video_games = ApplyGenreFilter(video_games, genreId);
            video_games = ApplySortOrder(video_games, sortOrder);

            return await video_games.ToListAsync();
        }
        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Developers, Platform, Rating, Created, ImageFile, GenderId")] VideoGame video_game)
        {
            if (ModelState.IsValid)
            {
                if (video_game.ImageFile != null && video_game.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(video_game.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await video_game.ImageFile.CopyToAsync(fileStream);
                    }

                    video_game.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    video_game.ImageUrl = "/images/generic.jpg";
                }

                _Context.VideoGames.Add(video_game);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(video_game);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var video_game = await _Context.VideoGames.FirstOrDefaultAsync(x => x.Id == id);

            if (video_game == null)
            {
                return NotFound();
            }

            return View(video_game);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Developers, Platform, Rating, Created, ImageFile, ImageUrl, GenderId")] VideoGame video_game)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingVideoGame = await _Context.VideoGames.FirstOrDefaultAsync(x => x.Id == video_game.Id);

                if (existingVideoGame == null)
                {
                    return NotFound();
                }

                existingVideoGame.Title = video_game.Title;
                existingVideoGame.Developers = video_game.Developers;
                existingVideoGame.Platform = video_game.Platform;
                existingVideoGame.Rating = video_game.Rating;
                existingVideoGame.Created = video_game.Created;
                existingVideoGame.GenderId = video_game.GenderId;

                if (video_game.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(video_game.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await video_game.ImageFile.CopyToAsync(fileStream);
                    }

                    existingVideoGame.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingVideoGame);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View(video_game);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var video_game = await _Context.VideoGames.FirstOrDefaultAsync(x =>x.Id == id);
            return View(video_game);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _Context.VideoGames.FindAsync(id);
            if(item != null)
            {
                _Context.VideoGames.Remove(item);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

    }
}

