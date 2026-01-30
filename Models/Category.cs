using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class Category
    {

        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = "";

        public List<MenuItem> MenuItems { get; set; } = new();
    }


}
