// KalakunjShibir.Core/Entities/Building.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using global::KalakunjShibir.Models.Enums;

namespace KalakunjShibir.Models.Entities
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

        [Display(Name = "Contact Person")]
        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(20)]
        public string? ContactNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Property
        public virtual ICollection<DataEntry> DataEntries { get; set; }

        // Computed Properties
        [NotMapped]
        public Dictionary<RoomType, int> RoomCounts => new()
        {
            { RoomType.AC, RoomTypeConfig.RoomCount[RoomType.AC] },
            { RoomType.NonAC, RoomTypeConfig.RoomCount[RoomType.NonAC] }
        };

        [NotMapped]
        public int AvailableRooms { get; set; }

        [NotMapped]
        public int OccupiedRooms => TotalRooms - AvailableRooms;

        [NotMapped]
        public decimal OccupancyRate => TotalRooms > 0 ?
            (decimal)OccupiedRooms / TotalRooms * 100 : 0;
    }

    // KalakunjShibir.Core/Entities/DataEntry.cs
    public class DataEntry
    {
        public DataEntry()
        {
            RoomBookings = new HashSet<RoomBooking>();
            CreatedAt = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "S.No is required")]
        [Display(Name = "S.No")]
        public int SNo { get; set; }

        [Required]
        [Display(Name = "Building")]
        public int BuildingId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Village is required")]
        [StringLength(100)]
        public string Village { get; set; }

        [Display(Name = "NRI")]
        public bool IsNRI { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }
        public virtual ICollection<RoomBooking> RoomBookings { get; set; }

        // Computed Properties
        [NotMapped]
        public string BookedRooms => string.Join(", ", RoomBookings
            .OrderBy(rb => rb.RoomNumber)
            .Select(rb => $"{rb.RoomNumber} ({rb.RoomType})"));

        [NotMapped]
        public int TotalRoomsBooked => RoomBookings.Count;

        [NotMapped]
        public string DateRange => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [NotMapped]
        public int StayDuration => (EndDate - StartDate).Days + 1;

        [NotMapped]
        public string Status => GetBookingStatus();

        [NotMapped]
        public string StatusColor => Status switch
        {
            "Active" => "success",
            "Completed" => "secondary",
            "Cancelled" => "danger",
            "Expired" => "danger",
            "Upcoming" => "primary",
            "Reserved" => "warning",
            _ => "primary"
        };

        private string GetBookingStatus()
        {
            var now = DateTime.Now.Date;

            if (RoomBookings.Any(rb => rb.Status == BookingStatus.CheckedIn))
                return "Active";
            if (RoomBookings.All(rb => rb.Status == BookingStatus.CheckedOut))
                return "Completed";
            if (RoomBookings.All(rb => rb.Status == BookingStatus.Cancelled))
                return "Cancelled";
            if (EndDate < now)
                return "Expired";
            if (StartDate > now)
                return "Upcoming";

            return "Reserved";
        }
    }

    // KalakunjShibir.Core/Entities/RoomBooking.cs
    public class RoomBooking
    {
        public int Id { get; set; }

        [Required]
        public int DataEntryId { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public int RoomNumber { get; set; }

        [Required]
        [Display(Name = "Room Type")]
        public RoomType RoomType { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Check-in Date")]
        public DateTime? CheckInDate { get; set; }

        [Display(Name = "Check-out Date")]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [Display(Name = "Booking Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Reserved;

        [Range(1, 4)]
        public int Occupants { get; set; } = 1;

        public string? Notes { get; set; }

        [Display(Name = "Room Condition")]
        public string? RoomCondition { get; set; }

        [Display(Name = "Special Requirements")]
        public string? SpecialRequirements { get; set; }

        // Navigation Property
        [ForeignKey("DataEntryId")]
        public virtual DataEntry DataEntry { get; set; }

        // Computed Properties
        [NotMapped]
        public int StayDuration => (EndDate - StartDate).Days + 1;

        [NotMapped]
        public string StatusBadge =>
            $"<span class='badge bg-{Status.GetStatusColor()}'>{Status}</span>";
    }


    public class BookingHistory
    {
        public int Id { get; set; }
        public int DataEntryId { get; set; }
        public string Action { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? PerformedBy { get; set; }
        public virtual DataEntry DataEntry { get; set; }
    }
}
