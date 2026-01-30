using System.ComponentModel.DataAnnotations;


namespace Resturant_Menu.Models;

public class Admin
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = "";

    [Required, MaxLength(100)]
    public string Password { get; set; } = ""; // You will insert in SQL Server

    [Required, MaxLength(80)]
    public string FullName { get; set; } = "";

    [MaxLength(100)]
    public string? Email { get; set; }
}
