using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string CustomerName { get; set; } = "";

        [Required, Range(1, 500)]
        public int TableNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }



}
