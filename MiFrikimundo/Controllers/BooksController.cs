using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;
using System.IO;

namespace MiFrikimundo.Controllers
{
    public class BooksController : Controller
    {
        private readonly MiFrikimundoContext _Context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(MiFrikimundoContext context, IWebHostEnvironment webHostEnvironment)
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

            // Obtener filtradas y ordenadas
            var books = await GetFilteredAndSortedBooksAsync(searchString, genreId, sortOrder);

            return View(books);
        }
        private async Task<List<Gender>> GetGenresAsync()
        {
            return await _Context.Genders.ToListAsync();
        }
        private IQueryable<Book> ApplySearchFilter(IQueryable<Book> books, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(m => m.Title.Contains(searchString));
            }
            return books;
        }
        private IQueryable<Book> ApplyGenreFilter(IQueryable<Book> books, int? genreId)
        {
            if (genreId.HasValue)
            {
                books = books.Where(m => m.GenderId == genreId.Value);
            }
            return books;
        }
        private IQueryable<Book> ApplySortOrder(IQueryable<Book> books, string sortOrder)
        {
            return sortOrder switch
            {
                "a_to_z" => books.OrderBy(m => m.Title),
                "z_to_a" => books.OrderByDescending(m => m.Title),
                "rating_desc" => books.OrderByDescending(m => m.Rating),
                "rating_asc" => books.OrderBy(m => m.Rating),
                "date_desc" => books.OrderByDescending(m => m.Created),
                "date_asc" => books.OrderBy(m => m.Created),
                _ => books.OrderByDescending(m => m.Created),
            };
        }
        private async Task<List<Book>> GetFilteredAndSortedBooksAsync(string searchString, int? genreId, string sortOrder)
        {
            var books = _Context.Books.Include(m => m.Gender).AsQueryable();

            books = ApplySearchFilter(books, searchString);
            books = ApplyGenreFilter(books, genreId);
            books = ApplySortOrder(books, sortOrder);

            return await books.ToListAsync();
        }


        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Author, Saga, Rating, Created, ImageFile, GenderId")] Book book)
        {
            if (ModelState.IsValid)
            {
                if (book.ImageFile != null && book.ImageFile.Length > 0)
                {
                    // Generar un nombre único para la imagen
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(book.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    // Guardar la imagen en el servidor
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.ImageFile.CopyToAsync(fileStream);
                    }

                    book.ImageUrl = "/images/" + fileName;
                }
                else
                {
                    // Asignar imagen por defecto si no se sube ninguna
                    book.ImageUrl = "/images/generic.jpg";
                }

                _Context.Books.Add(book);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var book = await _Context.Books.FirstOrDefaultAsync(x => x.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Author, Saga, Rating, Created, GenderId, ImageFile")] Book book)
        {
            if (ModelState.IsValid)
            {
                // Recupera la película original de la base de datos
                var existingBook = await _Context.Books.FirstOrDefaultAsync(x => x.Id == book.Id);

                if (existingBook == null)
                {
                    return NotFound();
                }

                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Saga = book.Saga;
                existingBook.Rating = book.Rating;
                existingBook.Created = book.Created;
                existingBook.GenderId = book.GenderId;

                if (book.ImageFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(book.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadPath);
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.ImageFile.CopyToAsync(fileStream);
                    }

                    existingBook.ImageUrl = "/images/" + fileName;
                }

                _Context.Update(existingBook);
                await _Context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View(book);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var book = await _Context.Books.FirstOrDefaultAsync(x =>x.Id == id);
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _Context.Books.FindAsync(id);
            if(book != null)
            {
                _Context.Books.Remove(book);
                await _Context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}

