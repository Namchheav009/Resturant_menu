using Microsoft.AspNetCore.Mvc;
using Resturant_Menu.Data;
using Resturant_Menu.Models;
using System.Text.Json;

namespace Resturant_Menu.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public IActionResult AddToCart(int menuItemId, int quantity = 1)
        {
            // Get cart from session
            var cartJson = HttpContext.Session.GetString("cart");
            var cartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // Check if item already in cart
            var existingItem = cartItems.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem { MenuItemId = menuItemId, Quantity = quantity });
            }

            // Save cart back to session
            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cartItems));

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cartJson = HttpContext.Session.GetString("cart");
            var cartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // Get menu items for cart
            var menuItemIds = cartItems.Select(c => c.MenuItemId).ToList();
            var menuItems = _db.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToList();

            var cartWithDetails = new List<CartItemDetail>();
            foreach (var cartItem in cartItems)
            {
                var menuItem = menuItems.FirstOrDefault(m => m.Id == cartItem.MenuItemId);
                if (menuItem != null)
                {
                    cartWithDetails.Add(new CartItemDetail
                    {
                        MenuItemId = cartItem.MenuItemId,
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        ImageUrl = menuItem.ImageUrl,
                        Quantity = cartItem.Quantity
                    });
                }
            }

            return View(cartWithDetails);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            var cartJson = HttpContext.Session.GetString("cart");
            var cartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            cartItems.RemoveAll(c => c.MenuItemId == menuItemId);

            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cartItems));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int menuItemId, int quantity)
        {
            var cartJson = HttpContext.Session.GetString("cart");
            var cartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItem>() 
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            var cartItem = cartItems.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    cartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
            }

            HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cartItems));

            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("cart");
            return RedirectToAction("Index", "Menu");
        }
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
}
