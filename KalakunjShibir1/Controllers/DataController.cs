using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalakunjShibir.Data;
using KalakunjShibir.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace KalakunjShibir.Controllers
{
    [Authorize]
    public class DataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataController> _logger;

        public DataController(ApplicationDbContext context, ILogger<DataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Data/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var entries = await _context.DataEntries
                    .Include(d => d.Building)
                    .Include(d => d.RoomBookings)
                    .OrderByDescending(d => d.StartDate)
                    .ThenBy(d => d.Building.Name)
                    .ToListAsync();

                return View(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data entries");
                TempData["Error"] = "An error occurred while loading the data.";
                return View(new List<DataEntry>());
            }
        }

        // GET: Data/Create
        public async Task<IActionResult> Create(int? buildingId = null)
        {
            try
            {
                var model = new DataEntry
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1)
                };

                var buildings = await _context.Buildings.OrderBy(b => b.Name).ToListAsync();
                ViewBag.Buildings = new SelectList(buildings, "Id", "Name", buildingId);

                if (buildingId.HasValue)
                {
                    model.BuildingId = buildingId.Value;
                    var building = await _context.Buildings.FindAsync(buildingId.Value);

                    if (building != null)
                    {
                        ViewBag.SelectedBuilding = building;
                        var availableRooms = await GetAvailableRoomsForBuilding(
                            buildingId.Value,
                            model.StartDate,
                            model.EndDate);

                        ViewBag.AvailableRooms = availableRooms
                            .Select(r => new SelectListItem
                            {
                                Value = r.ToString(),
                                Text = $"Room {r}"
                            })
                            .ToList();
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create GET with buildingId: {BuildingId}", buildingId);
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Data/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DataEntry dataEntry, int[] selectedRooms)
        {
            try
            {
                if (!selectedRooms.Any())
                {
                    ModelState.AddModelError("", "Please select at least one room");
                    await PrepareViewBagForCreate(dataEntry.BuildingId);
                    return View(dataEntry);
                }

                if (ModelState.IsValid)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Verify all rooms are available
                        foreach (var roomNumber in selectedRooms)
                        {
                            var isAvailable = await IsRoomAvailable(
                                dataEntry.BuildingId,
                                roomNumber,
                                dataEntry.StartDate,
                                dataEntry.EndDate);

                            if (!isAvailable)
                            {
                                ModelState.AddModelError("", $"Room {roomNumber} is not available for selected dates");
                                await PrepareViewBagForCreate(dataEntry.BuildingId);
                                return View(dataEntry);
                            }
                        }

                        // Set serial number
                        dataEntry.SNo = await GetNextSerialNumber();

                        // Create main booking
                        _context.DataEntries.Add(dataEntry);
                        await _context.SaveChangesAsync();

                        // Create room bookings
                        foreach (var roomNumber in selectedRooms)
                        {
                            var roomBooking = new RoomBooking
                            {
                                DataEntryId = dataEntry.Id,
                                RoomNumber = roomNumber,
                                StartDate = dataEntry.StartDate,
                                EndDate = dataEntry.EndDate
                            };
                            _context.RoomBookings.Add(roomBooking);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = $"Successfully booked {selectedRooms.Length} room(s)";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                await PrepareViewBagForCreate(dataEntry.BuildingId);
                return View(dataEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                TempData["Error"] = "Error creating the booking.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Data/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var dataEntry = await _context.DataEntries
                    .Include(d => d.Building)
                    .Include(d => d.RoomBookings)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dataEntry == null) return NotFound();

                var buildings = await _context.Buildings.OrderBy(b => b.Name).ToListAsync();
                ViewBag.Buildings = new SelectList(buildings, "Id", "Name", dataEntry.BuildingId);
                ViewBag.CurrentBuilding = dataEntry.Building?.Name;

                // Get all currently booked rooms
                var currentRooms = dataEntry.RoomBookings.Select(rb => rb.RoomNumber).ToList();
                ViewBag.CurrentRooms = currentRooms;

                var availableRooms = await GetAvailableRoomsForBuilding(
                    dataEntry.BuildingId,
                    dataEntry.StartDate,
                    dataEntry.EndDate,
                    currentRooms);

                ViewBag.AvailableRooms = availableRooms
                    .Concat(currentRooms)
                    .OrderBy(r => r)
                    .Select(r => new
                    {
                        Value = r.ToString(),
                        Text = currentRooms.Contains(r) ? $"Room {r} (Current)" : $"Room {r}",
                        Selected = currentRooms.Contains(r)
                    })
                    .Distinct()
                    .Select(r => new SelectListItem
                    {
                        Value = r.Value,
                        Text = r.Text,
                        Selected = r.Selected
                    })
                    .ToList();

                return View(dataEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form");
                TempData["Error"] = "Error loading the edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Data/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DataEntry dataEntry, int[] selectedRooms)
        {
            if (id != dataEntry.Id) return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var existingEntry = await _context.DataEntries
                            .Include(d => d.RoomBookings)
                            .FirstOrDefaultAsync(d => d.Id == id);

                        if (existingEntry == null) return NotFound();

                        // Update main booking details
                        existingEntry.BuildingId = dataEntry.BuildingId;
                        existingEntry.Name = dataEntry.Name;
                        existingEntry.Village = dataEntry.Village;
                        existingEntry.IsNRI = dataEntry.IsNRI;
                        existingEntry.Location = dataEntry.Location;
                        existingEntry.StartDate = dataEntry.StartDate;
                        existingEntry.EndDate = dataEntry.EndDate;

                        // Handle room changes
                        var existingRooms = existingEntry.RoomBookings.Select(rb => rb.RoomNumber).ToList();

                        // Remove rooms
                        foreach (var roomNumber in existingRooms)
                        {
                            var roomBooking = existingEntry.RoomBookings
                                .First(rb => rb.RoomNumber == roomNumber);
                            _context.RoomBookings.Remove(roomBooking);
                        }
                        await _context.SaveChangesAsync();

                        existingRooms = existingEntry.RoomBookings.Select(rb => rb.RoomNumber).ToList();
                        var roomsToAdd = selectedRooms.Except(existingRooms);
                        var roomsToRemove = existingRooms.Except(selectedRooms);

                        // Remove unselected rooms
                        foreach (var roomNumber in roomsToRemove)
                        {
                            var roomBooking = existingEntry.RoomBookings
                                .First(rb => rb.RoomNumber == roomNumber);
                            _context.RoomBookings.Remove(roomBooking);
                        }

                        // Add new rooms
                        foreach (var roomNumber in roomsToAdd)
                        {
                            var isAvailable = await IsRoomAvailable(
                                dataEntry.BuildingId,
                                roomNumber,
                                dataEntry.StartDate,
                                dataEntry.EndDate,
                                dataEntry.Id);

                            if (!isAvailable)
                            {
                                ModelState.AddModelError("", $"Room {roomNumber} is not available for selected dates");
                                await PrepareViewBagForEdit(dataEntry);
                                return View(dataEntry);
                            }

                            var newRoomBooking = new RoomBooking
                            {
                                DataEntryId = dataEntry.Id,
                                RoomNumber = roomNumber,
                                StartDate = dataEntry.StartDate,
                                EndDate = dataEntry.EndDate
                            };
                            _context.RoomBookings.Add(newRoomBooking);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        TempData["Success"] = "Booking updated successfully";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                await PrepareViewBagForEdit(dataEntry);
                return View(dataEntry);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DataEntryExists(dataEntry.Id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error updating booking");
                ModelState.AddModelError("", "The booking was modified by another user. Please try again.");
                await PrepareViewBagForEdit(dataEntry);
                return View(dataEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking");
                TempData["Error"] = "Error updating the booking.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Updated helper methods for room availability
        private async Task<List<int>> GetAvailableRoomsForBuilding(int buildingId, DateTime startDate,
            DateTime endDate, List<int>? excludeRoomNumbers = null)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null)
                return new List<int>();

            // Start with the initial query, but do not call ToList() here
            var query = _context.RoomBookings
                .Where(rb => rb.DataEntry.BuildingId == buildingId &&
                             rb.StartDate <= endDate &&
                             rb.EndDate >= startDate);

            if (excludeRoomNumbers != null && excludeRoomNumbers.Any())
            {
                // Apply the exclusion filter without materializing the query
                query = query.Where(rb => !excludeRoomNumbers.Contains(rb.RoomNumber));
            }

            // Now, materialize the final filtered result and select occupied rooms
            var occupiedRooms = await query.Select(rb => rb.RoomNumber).Distinct().ToListAsync();

            // Generate a list of all rooms in the building and find available rooms
            var allRooms = Enumerable.Range(1, building.TotalRooms);
            var availableRooms = allRooms.Except(occupiedRooms).OrderBy(r => r).ToList();

            return availableRooms;
        }

        public async Task<IActionResult> GetAvailableRooms(int buildingId, DateTime startDate,
             DateTime endDate, List<int>? excludeRoomNumbers = null)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            if (building == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Building not found"
                });
            }

            var query = _context.RoomBookings
                .Where(rb => rb.DataEntry.BuildingId == buildingId &&
                            rb.StartDate <= endDate &&
                            rb.EndDate >= startDate);

            if (excludeRoomNumbers != null && excludeRoomNumbers.Any())
            {
                query = query.Where(rb => !excludeRoomNumbers.Contains(rb.RoomNumber));
            }

            var occupiedRooms = await query.Select(rb => rb.RoomNumber).Distinct().ToListAsync();

            return Json(new
            {
                success = true,
                buildingName = building.Name,
                rooms = Enumerable.Range(1, building.TotalRooms)
                .Except(occupiedRooms)
                .OrderBy(r => r)
                .ToList()
            });
        }

        private async Task<bool> IsRoomAvailable(int buildingId, int roomNumber,
            DateTime startDate, DateTime endDate, int? excludeBookingId = null)
        {
            var query = _context.RoomBookings
                .Where(rb => rb.DataEntry.BuildingId == buildingId &&
                            rb.RoomNumber == roomNumber &&
                            rb.StartDate <= endDate &&
                            rb.EndDate >= startDate);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(rb => rb.DataEntryId != excludeBookingId.Value);
            }

            return !await query.AnyAsync();
        }

        // Add these helper methods to your DataController class

        private async Task PrepareViewBagForEdit(DataEntry dataEntry)
        {
            try
            {
                // Get buildings for dropdown
                var buildings = await _context.Buildings
                    .OrderBy(b => b.Name)
                    .ToListAsync();
                ViewBag.Buildings = new SelectList(buildings, "Id", "Name", dataEntry.BuildingId);

                // Get current building
                var building = await _context.Buildings.FindAsync(dataEntry.BuildingId);
                ViewBag.CurrentBuilding = building?.Name;
                ViewBag.TotalRooms = building?.TotalRooms;

                // Get room bookings
                var roomBookings = await _context.RoomBookings
                    .Where(rb => rb.DataEntryId == dataEntry.Id)
                    .OrderBy(rb => rb.RoomNumber)
                    .ToListAsync();
                ViewBag.CurrentRooms = roomBookings.Select(rb => rb.RoomNumber).ToList();

                // Get available rooms
                var availableRooms = await GetAvailableRoomsForBuilding(
                    dataEntry.BuildingId,
                    dataEntry.StartDate,
                    dataEntry.EndDate,
                    roomBookings.Select(rb => rb.RoomNumber).ToList());

                // Prepare room selection list including current and available rooms
                ViewBag.AvailableRooms = availableRooms
                    .Concat(roomBookings.Select(rb => rb.RoomNumber))
                    .Distinct()
                    .OrderBy(r => r)
                    .Select(r => new SelectListItem
                    {
                        Value = r.ToString(),
                        Text = roomBookings.Any(rb => rb.RoomNumber == r)
                            ? $"Room {r} (Current)"
                            : $"Room {r}",
                        Selected = roomBookings.Any(rb => rb.RoomNumber == r)
                    })
                    .ToList();

                // Additional status information
                ViewBag.BookingStatus = dataEntry.Status;
                ViewBag.StayDuration = dataEntry.StayDuration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing ViewBag for edit");
                throw;
            }
        }

        private async Task PrepareViewBagForCreate(int? buildingId)
        {
            try
            {
                // Get buildings for dropdown
                var buildings = await _context.Buildings
                    .OrderBy(b => b.Name)
                    .ToListAsync();
                ViewBag.Buildings = new SelectList(buildings, "Id", "Name", buildingId);

                if (buildingId.HasValue)
                {
                    // Get selected building details
                    var building = await _context.Buildings.FindAsync(buildingId.Value);
                    if (building != null)
                    {
                        ViewBag.SelectedBuilding = building;
                        ViewBag.TotalRooms = building.TotalRooms;

                        // Get available rooms for the selected dates
                        var today = DateTime.Today;
                        var availableRooms = await GetAvailableRoomsForBuilding(
                            buildingId.Value,
                            today,
                            today.AddDays(1));

                        ViewBag.AvailableRooms = availableRooms
                            .Select(r => new SelectListItem
                            {
                                Value = r.ToString(),
                                Text = $"Room {r}"
                            })
                            .ToList();

                        // Additional building information
                        ViewBag.BuildingLocation = building.Location;
                        ViewBag.BuildingDescription = building.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing ViewBag for create");
                throw;
            }
        }

        private bool DataEntryExists(int id)
        {
            return _context.DataEntries.Any(e => e.Id == id);
        }

        private async Task<bool> HasOverlappingBookings(int buildingId, int roomNumber, DateTime startDate,
            DateTime endDate, int? excludeBookingId = null)
        {
            var query = _context.RoomBookings
                .Where(rb => rb.DataEntry.BuildingId == buildingId &&
                            rb.RoomNumber == roomNumber &&
                            rb.StartDate <= endDate &&
                            rb.EndDate >= startDate);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(rb => rb.DataEntryId != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }

        private async Task<Dictionary<int, int>> GetBuildingOccupancy()
        {
            return await _context.Buildings
                .Include(b => b.DataEntries)
                .ThenInclude(d => d.RoomBookings)
                .Where(b => b.DataEntries.Any(d =>
                    d.RoomBookings.Any(rb =>
                        rb.StartDate <= DateTime.Today &&
                        rb.EndDate >= DateTime.Today)))
                .ToDictionaryAsync(
                    b => b.Id,
                    b => b.DataEntries
                        .SelectMany(d => d.RoomBookings)
                        .Count(rb =>
                            rb.StartDate <= DateTime.Today &&
                            rb.EndDate >= DateTime.Today)
                );
        }

        private async Task<List<DataEntry>> GetUpcomingCheckIns(int days = 7)
        {
            var endDate = DateTime.Today.AddDays(days);
            return await _context.DataEntries
                .Include(d => d.Building)
                .Include(d => d.RoomBookings)
                .Where(d => d.StartDate >= DateTime.Today && d.StartDate <= endDate)
                .OrderBy(d => d.StartDate)
                .ToListAsync();
        }

        private async Task<List<DataEntry>> GetUpcomingCheckOuts(int days = 7)
        {
            var endDate = DateTime.Today.AddDays(days);
            return await _context.DataEntries
                .Include(d => d.Building)
                .Include(d => d.RoomBookings)
                .Where(d => d.EndDate >= DateTime.Today && d.EndDate <= endDate)
                .OrderBy(d => d.EndDate)
                .ToListAsync();
        }

        // Validation helpers
        private bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return startDate <= endDate &&
                   startDate >= DateTime.Today &&
                   (endDate - startDate).TotalDays <= 365; // Max one year stay
        }

        private async Task<bool> IsValidRoomNumbers(int buildingId, List<int> roomNumbers)
        {
            var building = await _context.Buildings.FindAsync(buildingId);
            return building != null &&
                   roomNumbers.All(r => r > 0 && r <= building.TotalRooms);
        }

        // Status helpers
        private string GetBookingStatus(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            if (today < startDate) return "Upcoming";
            if (today > endDate) return "Completed";
            return "Active";
        }

        private async Task<int> GetTotalActiveBookings()
        {
            return await _context.DataEntries
                .CountAsync(d => d.StartDate <= DateTime.Today && d.EndDate >= DateTime.Today);
        }

        private async Task<int> GetNextSerialNumber()
        {
            // Get the maximum existing serial number
            var maxSNo = await _context.DataEntries.MaxAsync(d => (int?)d.SNo) ?? 0;

            // Generate the next serial number
            return maxSNo + 1;
        }
    }
}