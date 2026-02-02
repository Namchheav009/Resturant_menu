using Microsoft.AspNetCore.Mvc;
using Resturant_Menu.Data;
using Resturant_Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Resturant_Menu.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminDashboardController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            // Get recent notifications (show up to 10 on dashboard)
            var unreadNotifications = _db.AdminNotifications
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToList();

            var stats = new
            {
                TotalBookings = _db.Bookings.Count(),
                TotalMenuItems = _db.MenuItems.Count(),
                TotalCategories = _db.Categories.Count(),
                TotalAdmins = _db.Admins.Count(),
                UnreadNotifications = unreadNotifications.Count,
                Notifications = unreadNotifications
            };

            return View(stats);
        }

        public IActionResult Notifications()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var notifications = _db.AdminNotifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        [HttpPost]
        public IActionResult MarkNotificationAsRead(int id)
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var notification = _db.AdminNotifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.IsRead = true;
                _db.SaveChanges();
            }

            return RedirectToAction("Notifications");
        }

        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var unreadNotifications = _db.AdminNotifications.Where(n => !n.IsRead).ToList();
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }
            _db.SaveChanges();

            return RedirectToAction("Notifications");
        }

        // Categories CRUD
        public IActionResult Categories()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var categories = _db.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _db.Categories.Find(id);
            if (category != null)
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        // Menu Items CRUD
        public IActionResult MenuItems()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var menuItems = _db.MenuItems.Include(m => m.Category).ToList();
            return View(menuItems);
        }

        [HttpGet]
        public IActionResult CreateMenuItem()
        {
            ViewBag.Categories = _db.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(MenuItem menuItem, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    menuItem.ImageUrl = await SaveImageAsync(ImageFile);
                }

                _db.MenuItems.Add(menuItem);
                _db.SaveChanges();
                return RedirectToAction("MenuItems");
            }
            ViewBag.Categories = _db.Categories.ToList();
            return View(menuItem);
        }

        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            var menuItem = _db.MenuItems.Find(id);
            if (menuItem == null) return NotFound();
            ViewBag.Categories = _db.Categories.ToList();
            return View(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditMenuItem(MenuItem menuItem, IFormFile ImageFile, string ExistingImageUrl)
        {
            // Always allow update even if some validation fails (like image validation)
            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(menuItem.ImageUrl))
                    {
                        DeleteImage(menuItem.ImageUrl);
                    }
                    
                    menuItem.ImageUrl = await SaveImageAsync(ImageFile);
                }
                else if (!string.IsNullOrEmpty(ExistingImageUrl))
                {
                    // Keep existing image if no new file uploaded
                    menuItem.ImageUrl = ExistingImageUrl;
                }

                // Update the menu item
                var existingItem = _db.MenuItems.Find(menuItem.Id);
                if (existingItem != null)
                {
                    existingItem.Name = menuItem.Name;
                    existingItem.Description = menuItem.Description;
                    existingItem.Price = menuItem.Price;
                    existingItem.CategoryId = menuItem.CategoryId;
                    existingItem.IsAvailable = menuItem.IsAvailable;
                    existingItem.ImageUrl = menuItem.ImageUrl ?? existingItem.ImageUrl;
                    
                    _db.MenuItems.Update(existingItem);
                    _db.SaveChanges();
                    return RedirectToAction("MenuItems", new { success = true });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating menu item: " + ex.Message);
            }

            ViewBag.Categories = _db.Categories.ToList();
            return View(menuItem);
        }

        public IActionResult DeleteMenuItem(int id)
        {
            var menuItem = _db.MenuItems.Find(id);
            if (menuItem != null)
            {
                _db.MenuItems.Remove(menuItem);
                _db.SaveChanges();
            }
            return RedirectToAction("MenuItems");
        }

        // Tables Management
        public IActionResult Tables()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var tables = _db.Tables.OrderBy(t => t.Number).ToList();
            return View(tables);
        }

        [HttpPost]
        public IActionResult ToggleTableAvailability(int id)
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var table = _db.Tables.Find(id);
            if (table != null)
            {
                table.IsAvailable = !table.IsAvailable;
                _db.SaveChanges();
            }

            return RedirectToAction("Tables");
        }

        // Admins CRUD
        public IActionResult Admins()
        {
            if (HttpContext.Session.GetString("AdminUsername") == null)
                return RedirectToAction("Login", "AdminAccount");

            var admins = _db.Admins.ToList();
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateAdmin() => View();

        [HttpPost]
        public IActionResult CreateAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                _db.Admins.Add(admin);
                _db.SaveChanges();
                return RedirectToAction("Admins");
            }
            return View(admin);
        }

        [HttpGet]
        public IActionResult EditAdmin(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin == null) return NotFound();
            return View(admin);
        }

        [HttpPost]
        public IActionResult EditAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                _db.Admins.Update(admin);
                _db.SaveChanges();
                return RedirectToAction("Admins");
            }
            return View(admin);
        }

        public IActionResult DeleteAdmin(int id)
        {
            var admin = _db.Admins.Find(id);
            if (admin != null)
            {
                _db.Admins.Remove(admin);
                _db.SaveChanges();
            }
            return RedirectToAction("Admins");
        }

        // Helper method to save image
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Create unique filename
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            
            // Set upload path
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "menu");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Full file path
            string filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return relative path for database
            return "/images/menu/" + fileName;
        }

        // Helper method to delete image
        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // API endpoint to get unread notification count
        [HttpGet]
        public IActionResult GetUnreadNotificationCount()
        {
            var unreadCount = _db.AdminNotifications
                .Where(n => !n.IsRead)
                .Count();
            
            return Json(new { unreadCount });
        }
    }
}