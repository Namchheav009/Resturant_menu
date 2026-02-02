using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string CustomerName { get; set; } = "";

        // Foreign key to Table (1..21)
        [Required]
        public int TableId { get; set; }

        public Table? Table { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Navigation property for cart items
        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

        // Soft delete flag
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
