using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class BookingItem
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [Range(1, 999)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 99999.99)]
        public decimal Price { get; set; }

        // Navigation properties
        public Booking? Booking { get; set; }
        public MenuItem? MenuItem { get; set; }

        public decimal Subtotal => Price * Quantity;
    }
}
