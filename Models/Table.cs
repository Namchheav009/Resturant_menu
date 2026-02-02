using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required]
        public int Number { get; set; }

        // true = available
        public bool IsAvailable { get; set; } = true;
    }
}
