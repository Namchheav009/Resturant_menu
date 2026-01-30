using Microsoft.AspNetCore.Mvc;
using Resturant_Menu.Data;
using Resturant_Menu.Models;

namespace Resturant_Menu.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BookingController (ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Create(int? menuItemID)
        {
            ViewBag.MenuItemID = menuItemID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string customerName, int tableNumber, int? menuItemID)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ViewBag.Error = "Customer name is required.";
                ViewBag.MenuItemID = menuItemID;
                return View();
            }
            var customer = new Customer { Name = customerName.Trim() };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            var booking = new Booking
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TableNumber = tableNumber,
                MenuItemId = menuItemID,
                CreatedAt = DateTime.Now
            };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return RedirectToAction("Success", new { id = booking.Id });
        }
        public IActionResult Success(int id)
        {
            var b = _db.Bookings.FirstOrDefault(b => b.Id == id);
            if (b == null)
            {
                return NotFound();
            }
            return View(b);
        }
            

    }
}
