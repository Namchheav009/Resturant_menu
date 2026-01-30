using Microsoft.AspNetCore.Mvc;
using Resturant_Menu.Data;

namespace Resturant_Menu.Controllers
{
    public class AdminAccountController : Controller
    {
        
        private readonly ApplicationDbContext _db;

        public AdminAccountController(ApplicationDbContext db) => _db = db;

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username , string password)
        { 
             var admin = _db.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            
            if (admin == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetInt32("AdminId", admin.Id);
            HttpContext.Session.SetString("AdminUsername", admin.Username);

            return RedirectToAction("Index", "AdminBookings");

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }





    }
}
