// Models/ViewModels/BuildingOccupancyViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace KalakunjShibir.Models.ViewModels
{
    public class BuildingOccupancyViewModel
    {
        public int BuildingId { get; set; }

        [Display(Name = "Building Name")]
        public string BuildingName { get; set; }

        [Display(Name = "Total Rooms")]
        public int TotalRooms { get; set; }

        [Display(Name = "Occupied Rooms")]
        public int OccupiedRooms { get; set; }

        [Display(Name = "Available Rooms")]
        public int AvailableRooms { get; set; }

        [Display(Name = "Occupancy Rate (%)")]
        public decimal OccupancyRate => TotalRooms > 0
            ? Math.Round(((decimal)OccupiedRooms / TotalRooms * 100), 2)
            : 0;

        [Display(Name = "Vacancy Rate (%)")]
        public decimal VacancyRate => 100 - OccupancyRate;

        // List of occupied room numbers
        public List<int> OccupiedRoomNumbers { get; set; } = new List<int>();

        // List of available room numbers
        public List<int> AvailableRoomNumbers { get; set; } = new List<int>();

        // Date for which occupancy is calculated
        public DateTime OccupancyDate { get; set; }

        // Additional helper properties
        public string StatusColor => OccupancyRate switch
        {
            decimal rate when rate >= 90 => "danger",    // Red for high occupancy
            decimal rate when rate >= 70 => "warning",   // Yellow for moderate occupancy
            _ => "success"                               // Green for low occupancy
        };

        public string Status => OccupancyRate switch
        {
            decimal rate when rate >= 90 => "Nearly Full",
            decimal rate when rate >= 70 => "Moderately Occupied",
            decimal rate when rate >= 30 => "Available",
            _ => "Mostly Empty"
        };

        // Constructor
        public BuildingOccupancyViewModel()
        {
            OccupancyDate = DateTime.Today;
        }

        // Helper method to get room status
        public string GetRoomStatus(int roomNumber)
        {
            return OccupiedRoomNumbers.Contains(roomNumber) ? "Occupied" : "Available";
        }

        // Helper method to check if a room is available
        public bool IsRoomAvailable(int roomNumber)
        {
            return !OccupiedRoomNumbers.Contains(roomNumber);
        }
    }
}