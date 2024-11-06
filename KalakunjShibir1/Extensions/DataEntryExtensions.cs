using KalakunjShibir.Models;

namespace KalakunjShibir.Extensions
{
    public static class DataEntryExtensions
    {
        public static bool IsRoomAvailable(this Building building, int roomNumber, DateTime startDate, DateTime endDate,
            IEnumerable<RoomBooking> existingBookings)
        {
            return !existingBookings.Any(b =>
                b.RoomNumber == roomNumber &&
                b.StartDate <= endDate &&
                b.EndDate >= startDate);
        }

        public static List<int> GetAvailableRooms(this Building building, DateTime startDate, DateTime endDate,
            IEnumerable<RoomBooking> existingBookings)
        {
            var occupiedRooms = existingBookings
                .Where(b => b.StartDate <= endDate && b.EndDate >= startDate)
                .Select(b => b.RoomNumber)
                .Distinct();

            return Enumerable.Range(1, building.TotalRooms)
                .Except(occupiedRooms)
                .OrderBy(r => r)
                .ToList();
        }
    }
}
