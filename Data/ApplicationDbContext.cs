using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Models;

namespace Resturant_Menu.Data
{
    // Fix: Change class declaration to inherit only from IdentityDbContext (not both DbContext and IdentityDbContext)
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public virtual DbSet<Customer> Customers => Set<Customer>();
        public virtual DbSet<Booking> Bookings => Set<Booking>();
        public virtual DbSet<Category> Categories => Set<Category>();
        public virtual DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public virtual DbSet<Admin> Admins => Set<Admin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);


        }

    }

}



