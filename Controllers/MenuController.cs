using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Data;

namespace Resturant_Menu.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MenuController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? categoryId)
        {
            var items = await _db.MenuItems
                .Where(m => m.IsAvailable)
                .Include(m => m.Category)
                .ToListAsync();

            // Filter by category if provided
            if (categoryId.HasValue)
            {
                items = items.Where(m => m.CategoryId == categoryId.Value).ToList();
            }

            items = items.OrderBy(m => m.Name).ToList();

            // Get all categories for filter
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
