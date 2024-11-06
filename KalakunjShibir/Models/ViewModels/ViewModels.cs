// KalakunjShibir.Core/ViewModels/DashboardViewModels.cs
using KalakunjShibir.Models.Enums;

namespace KalakunjShibir.Models.ViewModels
{
    public class DashboardStats
    {
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public int NRIGuestsCount { get; set; }
        public Dictionary<string, BuildingOccupancy> BuildingOccupancies { get; set; }
        public RoomTypeStats ACRoomStats { get; set; }
        public RoomTypeStats NonACRoomStats { get; set; }
        public List<MonthlyBookingStat> MonthlyStats { get; set; }
    }

    public class BuildingOccupancy
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int ACRoomsOccupied { get; set; }
        public int NonACRoomsOccupied { get; set; }
        public decimal OccupancyRate => TotalRooms > 0 ?
            (decimal)OccupiedRooms / TotalRooms * 100 : 0;
    }

    public class RoomTypeStats
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public decimal OccupancyRate => TotalRooms > 0 ?
            (decimal)OccupiedRooms / TotalRooms * 100 : 0;
    }

    public class MonthlyBookingStat
    {
        public string Month { get; set; }
        public int TotalBookings { get; set; }
        public int ACRoomsBooked { get; set; }
        public int NonACRoomsBooked { get; set; }
    }

    public class AvailableRoomViewModel
    {
        public int RoomNumber { get; set; }
        public RoomType RoomType { get; set; }
        public string Amenities { get; set; }
        public string DisplayText =>
            $"Room {RoomNumber} - {RoomType.GetDisplayName()}";
    }
}