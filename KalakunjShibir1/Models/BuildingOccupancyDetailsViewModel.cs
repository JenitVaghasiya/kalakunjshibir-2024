using System.ComponentModel.DataAnnotations;
using KalakunjShibir.Models;

namespace KalakunjShibir.Models.ViewModels
{
    public class BuildingOccupancyDetailsViewModel
    {
        public int BuildingId { get; set; }

        [Display(Name = "Building Name")]
        public string BuildingName { get; set; }

        [Display(Name = "Total Rooms")]
        public int TotalRooms { get; set; }

        [Display(Name = "Occupied Rooms")]
        public int OccupiedRooms { get; set; }

        [Display(Name = "Available Rooms")]
        public int AvailableRooms => TotalRooms - OccupiedRooms;

        [Display(Name = "Occupancy Date")]
        public DateTime SelectedDate { get; set; }

        // List of all occupied room details
        public List<DataEntry> OccupiedRoomDetails { get; set; } = new List<DataEntry>();

        // Room numbers that are occupied
        public List<int> OccupiedRoomNumbers => OccupiedRoomDetails.SelectMany(x => x.RoomBookings.Select(rb => rb.RoomNumber)).ToList();

        // Available room numbers
        public List<int> AvailableRoomNumbers => Enumerable.Range(1, TotalRooms)
            .Except(OccupiedRoomNumbers)
            .ToList();

        [Display(Name = "Occupancy Rate")]
        public decimal OccupancyRate => TotalRooms > 0
            ? Math.Round(((decimal)OccupiedRooms / TotalRooms * 100), 2)
            : 0;

        [Display(Name = "Vacancy Rate")]
        public decimal VacancyRate => 100 - OccupancyRate;

        // Get status color based on occupancy rate
        public string StatusColor => OccupancyRate switch
        {
            var rate when rate >= 90 => "danger",    // Red for high occupancy
            var rate when rate >= 70 => "warning",   // Yellow for moderate occupancy
            var rate when rate >= 30 => "success",   // Green for normal occupancy
            _ => "info"                              // Blue for low occupancy
        };

        // Get status text
        public string Status => OccupancyRate switch
        {
            var rate when rate >= 90 => "Nearly Full",
            var rate when rate >= 70 => "Moderately Occupied",
            var rate when rate >= 30 => "Normal Occupancy",
            _ => "Low Occupancy"
        };

        // Get floor-wise occupancy
        public Dictionary<int, int> FloorwiseOccupancy
        {
            get
            {
                return OccupiedRoomDetails.SelectMany(x => x.RoomBookings)
                    .GroupBy(rb => (rb.RoomNumber - 1) / 10 + 1) // Assuming 10 rooms per floor
                    .ToDictionary(
                        g => g.Key,                            // Floor number
                        g => g.Count()                         // Number of occupied rooms
                    );
            }
        }

        // Helper methods
        public bool IsRoomOccupied(int roomNumber)
        {
            return OccupiedRoomNumbers.Contains(roomNumber);
        }

        public DataEntry GetRoomDetails(int roomNumber)
        {
            return OccupiedRoomDetails.FirstOrDefault(x => x.RoomBookings.Any(rb => rb.RoomNumber == roomNumber));
        }

        public string GetRoomStatus(int roomNumber)
        {
            return IsRoomOccupied(roomNumber) ? "Occupied" : "Available";
        }

        // Statistics
        public int TotalBookings => OccupiedRoomDetails.Count;

        public int NRIBookings => OccupiedRoomDetails.Count(x => x.IsNRI);

        public Dictionary<string, int> BookingsByVillage =>
            OccupiedRoomDetails
                .GroupBy(x => x.Village)
                .ToDictionary(g => g.Key, g => g.Count());

        // Date range methods
        public bool IsDateInRange(DateTime date)
        {
            return OccupiedRoomDetails.Any(x =>
                x.RoomBookings.Any(rb => rb.StartDate <= date && rb.EndDate >= date));
        }

        public List<DataEntry> GetBookingsForDate(DateTime date)
        {
            return OccupiedRoomDetails
                .Where(x => x.RoomBookings.Any(rb => rb.StartDate <= date && rb.EndDate >= date))
                .ToList();
        }

        // Summary properties
        public string Summary =>
            $"{BuildingName}: {OccupiedRooms}/{TotalRooms} rooms occupied ({OccupancyRate}% occupancy)";

        public string DetailedSummary =>
            $"Building: {BuildingName}\n" +
            $"Total Rooms: {TotalRooms}\n" +
            $"Occupied: {OccupiedRooms}\n" +
            $"Available: {AvailableRooms}\n" +
            $"Occupancy Rate: {OccupancyRate}%\n" +
            $"NRI Bookings: {NRIBookings}\n" +
            $"Status: {Status}";
    }
}