using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Resturant_Menu.Models;
using System.Linq;

namespace Resturant_Menu.Data
{
    // Fix: Change class declaration to inherit only from IdentityDbContext (not both DbContext and IdentityDbContext)
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public virtual DbSet<Customer> Customers => Set<Customer>();
        public virtual DbSet<Booking> Bookings => Set<Booking>();
        public virtual DbSet<Table> Tables => Set<Table>();
        public virtual DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public virtual DbSet<Category> Categories => Set<Category>();
        public virtual DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public virtual DbSet<Admin> Admins => Set<Admin>();
        public virtual DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            // Seed 21 tables (1..21)
            modelBuilder.Entity<Table>().HasData(
                Enumerable.Range(1, 21)
                    .Select(i => new Table { Id = i, Number = i, IsAvailable = true })
                    .ToArray()
            );

        }

    }

}



