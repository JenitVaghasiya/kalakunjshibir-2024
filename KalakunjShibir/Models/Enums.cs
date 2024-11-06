namespace KalakunjShibir.Models.Enums
{
    public enum RoomType
    {
        AC = 1,
        NonAC = 2
    }

    public enum BookingStatus
    {
        Reserved = 1,
        CheckedIn = 2,
        CheckedOut = 3,
        Cancelled = 4
    }

    public enum RoomStatus
    {
        Available = 1,
        Occupied = 2,
        UnderMaintenance = 3,
        OutOfService = 4
    }

    // Extension methods for enums
    public static class EnumExtensions
    {
        public static string GetDisplayName(this RoomType roomType)
        {
            return roomType switch
            {
                RoomType.AC => "AC Room",
                RoomType.NonAC => "Non-AC Room",
                _ => roomType.ToString()
            };
        }

        public static string GetStatusColor(this BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Reserved => "warning",
                BookingStatus.CheckedIn => "success",
                BookingStatus.CheckedOut => "secondary",
                BookingStatus.Cancelled => "danger",
                _ => "primary"
            };
        }

        public static string GetRoomStatusColor(this RoomStatus status)
        {
            return status switch
            {
                RoomStatus.Available => "success",
                RoomStatus.Occupied => "danger",
                RoomStatus.UnderMaintenance => "warning",
                RoomStatus.OutOfService => "secondary",
                _ => "primary"
            };
        }
    }

    public static class RoomTypeConfig
    {
        public static class AC
        {
            public const decimal BaseRate = 1000.00M;
            public const int MaxOccupancy = 3;
            public const bool HasTV = true;
            public const bool HasWifi = true;
        }

        public static class NonAC
        {
            public const decimal BaseRate = 500.00M;
            public const int MaxOccupancy = 3;
            public const bool HasTV = false;
            public const bool HasWifi = true;
        }

        public static Dictionary<RoomType, string> Amenities = new()
        {
            { RoomType.AC, "Air Conditioning, TV, WiFi, Hot Water" },
            { RoomType.NonAC, "Fan, WiFi, Hot Water" }
        };

        public static Dictionary<RoomType, int> RoomCount = new()
        {
            { RoomType.AC, 25 },
            { RoomType.NonAC, 25 }
        };

        public static bool IsValidRoomNumber(RoomType roomType, int roomNumber)
        {
            return roomType switch
            {
                RoomType.AC => roomNumber >= 1 && roomNumber <= 25,
                RoomType.NonAC => roomNumber >= 26 && roomNumber <= 50,
                _ => false
            };
        }

        public static RoomType GetRoomTypeFromNumber(int roomNumber)
        {
            return roomNumber switch
            {
                <= 25 => RoomType.AC,
                <= 50 => RoomType.NonAC,
                _ => throw new ArgumentException("Invalid room number")
            };
        }
    }
}