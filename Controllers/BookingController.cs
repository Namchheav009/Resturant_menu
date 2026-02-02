using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Data;
using Resturant_Menu.Models;
using System.Text.Json;
using Resturant_Menu.Controllers;
using Microsoft.AspNetCore.SignalR;
using Resturant_Menu.Hubs;

namespace Resturant_Menu.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;

        public BookingController (ApplicationDbContext db, IHubContext<NotificationHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Create(int? menuItemID)
        {
            // Prevent admins from making bookings
            if (HttpContext.Session.GetString("AdminUsername") != null)
                return RedirectToAction("Index", "AdminDashboard");

            // Get cart items
            var cartJson = HttpContext.Session.GetString("cart");
            var cartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // Get menu item details for cart display
            var menuItemIds = cartItems.Select(c => c.MenuItemId).ToList();
            var menuItems = _db.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToList();

            var cartDetails = new List<CartItemDetail>();
            decimal totalPrice = 0;
            foreach (var cartItem in cartItems)
            {
                var menuItem = menuItems.FirstOrDefault(m => m.Id == cartItem.MenuItemId);
                if (menuItem != null)
                {
                    var detail = new CartItemDetail
                    {
                        MenuItemId = cartItem.MenuItemId,
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        ImageUrl = menuItem.ImageUrl,
                        Quantity = cartItem.Quantity
                    };
                    cartDetails.Add(detail);
                    totalPrice += detail.Subtotal;
                }
            }

            ViewBag.CartItems = cartDetails;
            ViewBag.TotalPrice = totalPrice;
            ViewBag.MenuItemID = menuItemID;
            // provide tables for selection
            ViewBag.Tables = _db.Tables.OrderBy(t => t.Number).ToList();
            return View();
        }

        public class CartItem
        {
            public int MenuItemId { get; set; }
            public int Quantity { get; set; }
        }

        public class CartItemDetail
        {
            public int MenuItemId { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string ImageUrl { get; set; }
            public int Quantity { get; set; }

            public decimal Subtotal => Price * Quantity;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string customerName, int tableId, int? menuItemID)
        {
            // Prevent admins from making bookings
            if (HttpContext.Session.GetString("AdminUsername") != null)
                return RedirectToAction("Index", "AdminDashboard");

            if (string.IsNullOrWhiteSpace(customerName))
            {
                ViewBag.Error = "Customer name is required.";
                ViewBag.MenuItemID = menuItemID;
                ViewBag.Tables = _db.Tables.OrderBy(t => t.Number).ToList();
                return View();
            }

            var customer = new Customer { Name = customerName.Trim() };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            var booking = new Booking
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TableId = tableId,
                MenuItemId = menuItemID,
                CreatedAt = DateTime.Now
            };
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // mark table as unavailable
            var table = await _db.Tables.FindAsync(tableId);
            if (table != null)
            {
                table.IsAvailable = false;
                _db.Tables.Update(table);
                await _db.SaveChangesAsync();
            }

            // Save cart items to booking
            var cartJson = HttpContext.Session.GetString("cart");
            if (!string.IsNullOrEmpty(cartJson))
            {
                var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
                var menuItemIds = cartItems.Select(c => c.MenuItemId).ToList();
                var menuItems = await _db.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToListAsync();

                foreach (var cartItem in cartItems)
                {
                    var menuItem = menuItems.FirstOrDefault(m => m.Id == cartItem.MenuItemId);
                    if (menuItem != null)
                    {
                        var bookingItem = new BookingItem
                        {
                            BookingId = booking.Id,
                            MenuItemId = cartItem.MenuItemId,
                            Quantity = cartItem.Quantity,
                            Price = menuItem.Price
                        };
                        _db.BookingItems.Add(bookingItem);
                    }
                }
                await _db.SaveChangesAsync();
            }

            // Create notification for admin
            var itemNames = _db.BookingItems
                .Where(bi => bi.BookingId == booking.Id)
                .Include(bi => bi.MenuItem)
                .Select(bi => $"{bi.MenuItem!.Name} (x{bi.Quantity})")
                .ToList();
            
            var itemsDisplay = itemNames.Count > 0 ? string.Join(", ", itemNames) : "Items";
            var tableNumberForMsg = table?.Number ?? tableId;
            var notification = new AdminNotification
            {
                Message = $"New booking from {customerName} for table {tableNumberForMsg}: {itemsDisplay}",
                Type = "booking",
                BookingId = booking.Id,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _db.AdminNotifications.Add(notification);
            await _db.SaveChangesAsync();

            // Send real-time notification via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveBookingNotification", new
            {
                BookingId = booking.Id,
                Message = $"New booking from {customerName} for table {tableNumberForMsg}",
                CustomerName = customerName,
                Items = itemNames,
                TableNumber = tableNumberForMsg,
                CreatedAt = DateTime.Now
            });

            // Clear the cart after successful booking
            HttpContext.Session.Remove("cart");

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
