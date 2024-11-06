using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalakunjShibir.Models
{
    public class Building
    {
        public Building()
        {
            DataEntries = new HashSet<DataEntry>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Total Rooms")]
        public int TotalRooms { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Property
        public virtual ICollection<DataEntry> DataEntries { get; set; }

        // Computed Properties
        [NotMapped]
        public int AvailableRooms { get; set; }

        [NotMapped]
        public int OccupiedRooms { get; set; }

        [NotMapped]
        public decimal OccupancyRate => TotalRooms > 0 ? (decimal)OccupiedRooms / TotalRooms * 100 : 0;
    }
}