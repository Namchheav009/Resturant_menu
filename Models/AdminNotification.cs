using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class AdminNotification
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = "";

        [Required]
        public string Type { get; set; } = "booking"; // booking, update, delete, etc.

        public int? BookingId { get; set; }

        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public Booking? Booking { get; set; }
    }
}
