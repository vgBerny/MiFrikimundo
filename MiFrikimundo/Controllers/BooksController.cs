using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;

namespace MiFrikimundo.Controllers
{
    public class BooksController : Controller
    {
        private readonly MiFrikimundoContext _Context;

        public BooksController(MiFrikimundoContext context)
        {
            _Context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var books = _Context.Books.Include(g => g.Gender).AsQueryable();
            if(!string.IsNullOrEmpty(searchString) )
            {
                books = books.Where(b => b.Title.Contains(searchString)); 
            }
            return View(await books.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Author, Saga, Rating, GenderId")]Book book)
        {
            if(ModelState.IsValid)
            {
                _Context.Books.Add(book);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var book = await _Context.Books.FirstOrDefaultAsync(x => x.Id == id);
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Author, Saga, Rating, GenderId")] Book book)
        {
            if(ModelState.IsValid)
            {
                _Context.Update(book);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
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

