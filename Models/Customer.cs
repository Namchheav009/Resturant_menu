using System.ComponentModel.DataAnnotations;

namespace Resturant_Menu.Models
{
    public class Customer
    {


        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = "";

       public List<Booking> Bookings { get; set; } = new();
    }


}
