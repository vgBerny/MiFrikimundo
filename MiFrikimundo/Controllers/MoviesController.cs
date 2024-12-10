using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiFrikimundo.Data;
using MiFrikimundo.Models;

namespace MiFrikimundo.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MiFrikimundoContext _Context;

        public MoviesController(MiFrikimundoContext context)
        {
            _Context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movie = await _Context.Movies.Include(g => g.Gender).ToListAsync();

            return View(movie);
        }

        public IActionResult Create()
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Title, Director, GenderId")]Movie movie)
        {
            if(ModelState.IsValid)
            {
                _Context.Movies.Add(movie);
                await _Context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Genders"] = new SelectList(_Context.Genders, "Id", "Name");
            var movie = await _Context.Movies.FirstOrDefaultAsync(x => x.Id == id);
            return View(movie);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int Id, [Bind("Id, Title, Director, GenderId")] Movie movie)
        {
            if(ModelState.IsValid)
            {
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

