using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KalakunjShibir.Models;
using KalakunjShibir.Models.Entities;

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
        public DbSet<BookingHistory> BookingHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<DataEntry>()
                .HasOne(de => de.Building)
                .WithMany(b => b.DataEntries)
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
                    Description = "Building A in the East Wing",
                    ContactPerson = "Admin 1",
                    ContactNumber = "1234567890",
                    IsActive = true
                },
                new Building
                {
                    Id = 2,
                    Name = "Block B",
                    TotalRooms = 50,
                    Location = "West Wing",
                    Description = "Building B in the West Wing",
                    ContactPerson = "Admin 2",
                    ContactNumber = "9876543210",
                    IsActive = true
                }
            );

            // Indexes for better query performance
            builder.Entity<DataEntry>()
                .HasIndex(de => de.SNo)
                .IsUnique();

            builder.Entity<DataEntry>()
                .HasIndex(de => new { de.StartDate, de.EndDate });

            builder.Entity<RoomBooking>()
                .HasIndex(rb => new { rb.DataEntryId, rb.RoomNumber })
                .IsUnique();

            builder.Entity<RoomBooking>()
                .HasIndex(rb => new { rb.StartDate, rb.EndDate, rb.Status });
        }
    }
}