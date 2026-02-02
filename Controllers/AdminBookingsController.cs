using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Data;
using Resturant_Menu.Models;
using System.Linq;

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
                .Where(b => !b.IsDeleted)
                .Include(b => b.Customer)
                .Include(b => b.Table)
                .Include(b => b.MenuItem)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.MenuItem)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Ensure any missing MenuItem navigation on BookingItems is populated
            var missingMenuItemIds = bookings
                .SelectMany(b => b.BookingItems ?? Enumerable.Empty<BookingItem>())
                .Where(bi => bi.MenuItem == null)
                .Select(bi => bi.MenuItemId)
                .Distinct()
                .ToList();

            if (missingMenuItemIds.Any())
            {
                var menuItems = await _db.MenuItems
                    .Where(m => missingMenuItemIds.Contains(m.Id))
                    .ToDictionaryAsync(m => m.Id);

                foreach (var booking in bookings)
                {
                    foreach (var bi in booking.BookingItems ?? Enumerable.Empty<BookingItem>())
                    {
                        if (bi.MenuItem == null && menuItems.TryGetValue(bi.MenuItemId, out var mi))
                            bi.MenuItem = mi;
                    }
                }
            }

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var booking = await _db.Bookings
                .Include(b => b.Table)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.MenuItem)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (booking == null)
                return NotFound();

            ViewBag.Tables = _db.Tables.Where(t => t.IsAvailable).ToList();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string customerName, int tableId)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null || booking.IsDeleted)
                return NotFound();

            if (string.IsNullOrWhiteSpace(customerName))
            {
                ModelState.AddModelError("customerName", "Customer name is required");
                ViewBag.Tables = _db.Tables.Where(t => t.IsAvailable).ToList();
                return View(booking);
            }

            booking.CustomerName = customerName.Trim();
            booking.TableId = tableId;

            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var booking = await _db.Bookings.Include(b => b.Table).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null || booking.IsDeleted)
                return NotFound();

            // Soft delete - mark as deleted and store deletion time
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.Now;

            _db.Bookings.Update(booking);

            // Make the table available again
            if (booking.Table != null)
            {
                booking.Table.IsAvailable = true;
                _db.Tables.Update(booking.Table);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // View archived/deleted bookings
        public async Task<IActionResult> Archived()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var deletedBookings = await _db.Bookings
                .Where(b => b.IsDeleted)
                .Include(b => b.Customer)
                .Include(b => b.Table)
                .Include(b => b.MenuItem)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.MenuItem)
                .OrderByDescending(b => b.DeletedAt)
                .ToListAsync();

            return View(deletedBookings);
        }

        [HttpGet]
        public async Task<IActionResult> DebugItems(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login", "AdminAccount");

            var items = await _db.BookingItems
                .Where(bi => bi.BookingId == id)
                .Include(bi => bi.MenuItem)
                .Select(bi => new { bi.Id, bi.MenuItemId, MenuItemName = bi.MenuItem != null ? bi.MenuItem.Name : null, bi.Quantity, bi.Price })
                .ToListAsync();

            return Json(new { BookingId = id, Items = items });
        }
    }
}
