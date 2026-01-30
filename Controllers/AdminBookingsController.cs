using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Data;

namespace Resturant_Menu.Controllers
{
    public class AdminBookingsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminBookingsController(ApplicationDbContext db) => _db = db;

        private bool IsAdminLoggedIn()
            => HttpContext.Session.GetInt32("AdminId") != null;
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var bookings = await _db.Bookings
                .Include(b => b.Customer)
                .Include(b => b.MenuItem)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);


        }

    }
}
