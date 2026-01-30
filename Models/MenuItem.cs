using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Resturant_Menu.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Name { get; set; } = "";

        [MaxLength(600)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999)]
        public decimal Price { get; set; }

        [MaxLength(300)]
        public string? ImageUrl { get; set; } // "/images/food1.jpg"

        public bool IsAvailable { get; set; } = true;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }


}
