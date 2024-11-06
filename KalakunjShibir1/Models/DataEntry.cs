
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalakunjShibir.Models
{
    public class DataEntry
    {
        public DataEntry()
        {
            RoomBookings = new HashSet<RoomBooking>();
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
        public string? Village { get; set; }

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

        // Navigation Properties
        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }

        public virtual ICollection<RoomBooking> RoomBookings { get; set; }

        // Computed Properties
        [NotMapped]
        public string BookedRooms => string.Join(", ", RoomBookings.OrderBy(rb => rb.RoomNumber).Select(rb => $"Room {rb.RoomNumber}"));

        [NotMapped]
        public int TotalRoomsBooked => RoomBookings.Count;

        [NotMapped]
        public string DateRange => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [NotMapped]
        public int StayDuration => (EndDate - StartDate).Days + 1;

        [NotMapped]
        public string Status
        {
            get
            {
                var today = DateTime.Today;
                return today switch
                {
                    var date when date < StartDate => "Upcoming",
                    var date when date > EndDate => "Completed",
                    _ => "Active"
                };
            }
        }

        [NotMapped]
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Upcoming" => "warning",
                    "Completed" => "secondary",
                    "Active" => "success",
                    _ => "primary"
                };
            }
        }
    }
}