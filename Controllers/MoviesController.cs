﻿using Microsoft.AspNetCore.Hosting;
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
            ViewBag.Genres = new SelectList(await GetGenresAsync(), "Id", "Name");

            // Pasar el orden actual a la vista para resaltarlo en el UI
            ViewBag.CurrentSort = sortOrder;

            // Obtener las películas filtradas y ordenadas
            var movies = await GetFilteredAndSortedMoviesAsync(searchString, genreId, sortOrder);

            return View(movies);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<Movie> ApplySearchFilter(IQueryable<Movie> movies, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(m => m.Title.Contains(searchString));
            }
            return movies;
        }
        private IQueryable<Movie> ApplyGenreFilter(IQueryable<Movie> movies, int? genreId)
        {
            if (genreId.HasValue)
            {
                movies = movies.Where(m => m.GenderId == genreId.Value);
            }
            return movies;
        }
        private IQueryable<Movie> ApplySortOrder(IQueryable<Movie> movies, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => movies.OrderBy(m => m.Title),
                "z_to_a" => movies.OrderByDescending(m => m.Title),
                "rating_desc" => movies.OrderByDescending(m => m.Rating),
                "rating_asc" => movies.OrderBy(m => m.Rating),
                "date_desc" => movies.OrderByDescending(m => m.Created),
                "date_asc" => movies.OrderBy(m => m.Created),
                _ => movies.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<Movie>> GetFilteredAndSortedMoviesAsync(string searchString, int? genreId, string sortOrder)
        {
            var movies = _Context.Movies.Include(m => m.Gender).AsQueryable();

            movies = ApplySearchFilter(movies, searchString);
            movies = ApplyGenreFilter(movies, genreId);
            movies = ApplySortOrder(movies, sortOrder);

            return await movies.ToListAsync();
        }
        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Director, Rating, Created, ImageFile, GenderId")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                if (movie.ImageFile != null && movie.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(movie.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await movie.ImageFile.CopyToAsync(fileStream);
                    }

                    movie.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    movie.ImageUrl = "/images/generic.jpg";
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

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Director, Rating, Created, ImageFile, ImageUrl, GenderId")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingMovie = await _Context.Movies.FirstOrDefaultAsync(x => x.Id == movie.Id);

                if (existingMovie == null)
                {
                    return NotFound();
                }

                existingMovie.Title = movie.Title;
                existingMovie.Director = movie.Director;
                existingMovie.Rating = movie.Rating;
                existingMovie.Created = movie.Created;
                existingMovie.GenderId = movie.GenderId;

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

                    existingMovie.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingMovie);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
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

