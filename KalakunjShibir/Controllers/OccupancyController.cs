using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KalakunjShibir.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using KalakunjShibir.Data;

namespace KalakunjShibir.Controllers
{
    [Authorize]
    public class OccupancyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OccupancyController> _logger;

        public OccupancyController(ApplicationDbContext context, ILogger<OccupancyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Occupancy Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;
                var occupancyStats = await GetOccupancyStats(today);
                return View(occupancyStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading occupancy dashboard");
                TempData["Error"] = "Error loading occupancy data.";
                return View(new List<BuildingOccupancyDetailsViewModel>());
            }
        }

        // GET: Occupancy/Details/{buildingId}
        public async Task<IActionResult> Details(int buildingId, DateTime? date)
        {
            try
            {
                date ??= DateTime.Today;
                var building = await _context.Buildings.FindAsync(buildingId);

                if (building == null)
                    return NotFound();

                var occupiedRooms = await _context.DataEntries
                    .Where(d => d.Building.Id == buildingId &&
                               d.RoomBookings.Any(rb => rb.StartDate <= date && rb.EndDate >= date))
                    .Include(d => d.Building)
                    .Include(d => d.RoomBookings)
                    .ToListAsync();

                var viewModel = new BuildingOccupancyDetailsViewModel
                {
                    BuildingId = building.Id,
                    BuildingName = building.Name,
                    TotalRooms = building.TotalRooms,
                    OccupiedRooms = occupiedRooms.SelectMany(d => d.RoomBookings).Count(rb => rb.StartDate <= date && rb.EndDate >= date),
                    SelectedDate = date.Value,
                    OccupiedRoomDetails = occupiedRooms
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading building details");
                TempData["Error"] = "Error loading building details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Occupancy/GetDailyStats
        [HttpGet]
        public async Task<JsonResult> GetDailyStats()
        {
            try
            {
                var today = DateTime.Today;
                var stats = await GetOccupancyStats(today);
                return Json(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily stats");
                return Json(new
                {
                    success = false,
                    message = "Error loading statistics"
                });
            }
        }

        // Helper Methods
        private async Task<List<BuildingOccupancyDetailsViewModel>> GetOccupancyStats(DateTime date)
        {
            var occupancyList = new List<BuildingOccupancyDetailsViewModel>();
            var buildings = await _context.Buildings.ToListAsync();

            foreach (var building in buildings)
            {
                var occupiedRooms = await _context.DataEntries
                    .Where(d => d.Building.Id == building.Id && d.RoomBookings.Any(rb => rb.StartDate <= date && rb.EndDate >= date))
                    .Include(d => d.RoomBookings)
                    .ToListAsync();

                var availableRooms = Enumerable.Range(1, building.TotalRooms)
                    .Except(occupiedRooms.SelectMany(d => d.RoomBookings.Select(rb => rb.RoomNumber)))
                    .ToList();

                occupancyList.Add(new BuildingOccupancyDetailsViewModel
                {
                    BuildingId = building.Id,
                    BuildingName = building.Name,
                    TotalRooms = building.TotalRooms,
                    OccupiedRooms = occupiedRooms.SelectMany(d => d.RoomBookings).Count(rb => rb.StartDate <= date && rb.EndDate >= date),
                    SelectedDate = date,
                    OccupiedRoomDetails = occupiedRooms
                });
            }

            return occupancyList;
        }
    }
}