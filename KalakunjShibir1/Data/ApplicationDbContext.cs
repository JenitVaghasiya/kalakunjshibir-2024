using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KalakunjShibir.Models;

namespace KalakunjShibir.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataEntry> DataEntries { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<RoomBooking> RoomBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<DataEntry>()
       .HasOne(de => de.Building)
       .WithMany(b => b.DataEntries)  // Assuming Building has a collection of DataEntries; if not, change to .WithMany()
       .HasForeignKey(de => de.BuildingId)
       .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RoomBooking>()
                .HasOne(rb => rb.DataEntry)
                .WithMany(de => de.RoomBookings)
                .HasForeignKey(rb => rb.DataEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial buildings
            builder.Entity<Building>().HasData(
     new Building
     {
         Id = 1,
         Name = "Block A",
         TotalRooms = 50,
         Location = "East Wing",
         Description = "Building A in the East Wing"
     },
     new Building
     {
         Id = 2,
         Name = "Block B",
         TotalRooms = 50,
         Location = "West Wing",
         Description = "Building B in the West Wing"
     }
 );
        }
    }
}