using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalakunjShibir.Models
{
    public class RoomBooking
    {
        public int Id { get; set; }

        [Required]
        public int DataEntryId { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public int RoomNumber { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Additional booking details if needed
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
        public string Status
        {
            get
            {
                var today = DateTime.Today;
                if (today < StartDate) return "Upcoming";
                if (today > EndDate) return "Completed";
                return "Active";
            }
        }
    }
}
