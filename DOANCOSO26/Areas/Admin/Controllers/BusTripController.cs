using DOANCOSO26.Data;
using DOANCOSO26.Models;
using DOANCOSO26.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace DOANCOSO26.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class BusTripController : Controller
    {

        private readonly IBusTripRepository _bustripRepository;
        private readonly IBusRepository _busRepository;
        private readonly ApplicationDbContext _context;
        private readonly ISeatRepository _seatRepository;
        private readonly IStopRepository _stopRepository;
        private readonly IBusRouteRepository _busRouteRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITripReportRepository _TripReportRepository;
        private readonly IDriverregisRepository _driverregisRepository;
        public BusTripController(IBusTripRepository bustripRepository, IBusRepository busRepository, ApplicationDbContext context, ISeatRepository seatRepository, IStopRepository stopRepository, IBusRouteRepository busRouteRepository, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IBookingRepository bookingRepository, ITripReportRepository tripReportRepository, IDriverregisRepository driverregisRepository)
        {
            _bustripRepository = bustripRepository;
            _busRepository = busRepository;
            _seatRepository = seatRepository;
            _context = context;
            _stopRepository = stopRepository;
            _busRouteRepository = busRouteRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _bookingRepository = bookingRepository;
            _TripReportRepository = tripReportRepository;
            _driverregisRepository = driverregisRepository;
        }


        // Hiển thị danh sách chuyến xe + lọc/sắp xếp cho Admin
        public async Task<IActionResult> Index(string? company, int? busId, DateTime? departureDate, string? sortOrder)
        {
            var selectedCompany = string.IsNullOrWhiteSpace(company) ? null : company.Trim();

            var allBuses = await _context.Buses
                .AsNoTracking()
                .OrderBy(b => b.Company)
                .ThenBy(b => b.BusNumber)
                .ToListAsync();

            // Nếu đã chọn hãng mà busId hiện tại không thuộc hãng đó thì bỏ busId để tránh lọc sai/rỗng dữ liệu.
            if (!string.IsNullOrWhiteSpace(selectedCompany) && busId.HasValue)
            {
                var busBelongsToCompany = allBuses.Any(b =>
                    b.Id == busId.Value &&
                    !string.IsNullOrWhiteSpace(b.Company) &&
                    b.Company.Trim().Equals(selectedCompany, StringComparison.OrdinalIgnoreCase));

                if (!busBelongsToCompany)
                {
                    busId = null;
                }
            }

            var query = _context.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.BusRoute)
                    .ThenInclude(r => r.StartStop)
                .Include(t => t.BusRoute)
                    .ThenInclude(r => r.EndStop)
                .Include(t => t.Seats)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(selectedCompany))
            {
                query = query.Where(t =>
                    t.Bus != null &&
                    t.Bus.Company != null &&
                    t.Bus.Company.Trim() == selectedCompany);
            }

            if (busId.HasValue)
            {
                query = query.Where(t => t.BusId == busId.Value);
            }

            if (departureDate.HasValue)
            {
                var date = departureDate.Value.Date;
                query = query.Where(t => t.DepartureDate.Date == date);
            }

            query = sortOrder switch
            {
                "company" => query.OrderBy(t => t.Bus!.Company).ThenBy(t => t.DepartureDate).ThenBy(t => t.DepartureTime),
                "company_desc" => query.OrderByDescending(t => t.Bus!.Company).ThenBy(t => t.DepartureDate).ThenBy(t => t.DepartureTime),
                "date" => query.OrderBy(t => t.DepartureDate).ThenBy(t => t.DepartureTime),
                "date_desc" => query.OrderByDescending(t => t.DepartureDate).ThenByDescending(t => t.DepartureTime),
                "bus" => query.OrderBy(t => t.Bus!.BusNumber).ThenBy(t => t.DepartureDate).ThenBy(t => t.DepartureTime),
                "bus_desc" => query.OrderByDescending(t => t.Bus!.BusNumber).ThenBy(t => t.DepartureDate).ThenBy(t => t.DepartureTime),
                _ => query.OrderByDescending(t => t.DepartureDate).ThenByDescending(t => t.DepartureTime)
            };

            var busTrips = await query.ToListAsync();

            var companyNames = allBuses
                .Select(b => b.Company?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            var busesForDropdown = allBuses.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(selectedCompany))
            {
                busesForDropdown = busesForDropdown.Where(b =>
                    !string.IsNullOrWhiteSpace(b.Company) &&
                    b.Company.Trim().Equals(selectedCompany, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Companies = companyNames;
            ViewBag.Buses = new SelectList(
                busesForDropdown.Select(b => new { b.Id, Name = $"{b.BusNumber} - {b.Company}" }).ToList(),
                "Id",
                "Name",
                busId);

            ViewBag.CompanyFilter = selectedCompany;
            ViewBag.BusIdFilter = busId;
            ViewBag.DepartureDateFilter = departureDate?.ToString("yyyy-MM-dd");
            ViewBag.SortOrder = sortOrder;
            ViewBag.CompanySort = sortOrder == "company" ? "company_desc" : "company";
            ViewBag.DateSort = sortOrder == "date" ? "date_desc" : "date";
            ViewBag.BusSort = sortOrder == "bus" ? "bus_desc" : "bus";

            return View(busTrips);
        }


        private async Task<SelectList> GetBusSelectListAsync(int? selectedBusId = null)
        {
            var buses = await _context.Buses
                .AsNoTracking()
                .OrderBy(b => b.Company)
                .ThenBy(b => b.BusNumber)
                .Select(b => new
                {
                    b.Id,
                    DisplayName = (b.BusNumber ?? "") + " - " + (b.Company ?? "") + " - " + (b.BusType == BusType.Limousine ? "Giường nằm Limousine" : "Ghế ngồi")
                })
                .ToListAsync();

            return new SelectList(buses, "Id", "DisplayName", selectedBusId);
        }


        private static bool TryParseDepartureTime24h(string? timeText, out DateTime departureTime)
        {
            departureTime = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(timeText))
            {
                return false;
            }

            var formats = new[] { @"h\:mm", @"hh\:mm" };
            if (TimeSpan.TryParseExact(timeText.Trim(), formats, CultureInfo.InvariantCulture, out var timeSpan))
            {
                departureTime = DateTime.Today.Add(timeSpan);
                return true;
            }
            return false;
        }
        private static bool TryParseMoney(string? value, out double amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var cleaned = value.Trim()
                .Replace("VND", "", StringComparison.OrdinalIgnoreCase)
                .Replace("VNĐ", "", StringComparison.OrdinalIgnoreCase)
                .Replace("đ", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" ", "")
                .Replace(",", "");

            return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out amount)
                || double.TryParse(cleaned, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out amount);
        }

        private static double CalculateAdjustedPrice(double basePrice, double? adjustmentPercent)
        {
            // adjustmentPercent > 0: tăng giá, ví dụ 20 = tăng 20%
            // adjustmentPercent < 0: giảm giá, ví dụ -10 = giảm 10%
            var percent = Math.Clamp(adjustmentPercent ?? 0, -100, 300);
            return Math.Max(0, Math.Round(basePrice * (1 + percent / 100), 0));
        }

        private async Task CreateSeatsForTripAsync(int busTripId, int capacity, double price)
        {
            if (capacity <= 0)
            {
                return;
            }

            var existed = await _context.Seats.AnyAsync(s => s.BusTripId == busTripId);
            if (existed)
            {
                return;
            }

            var busTrip = await _context.BusTrips
                .Include(t => t.Bus)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == busTripId);

            var isLimousine = busTrip?.Bus?.BusType == BusType.Limousine;

            var seats = new List<Seat>();

            if (isLimousine)
            {
                // Giường nằm Limousine: chia thành 2 tầng.
                // Tầng 1: A1..A{n}, tầng 2: B1..B{n}
                var firstFloorCapacity = (int)Math.Ceiling(capacity / 2.0);

                for (var i = 1; i <= capacity; i++)
                {
                    var isFirstFloor = i <= firstFloorCapacity;
                    var seatPrefix = isFirstFloor ? "A" : "B";
                    var seatNumber = isFirstFloor ? i : i - firstFloorCapacity;

                    seats.Add(new Seat
                    {
                        BusTripId = busTripId,
                        SeatNumber = $"{seatPrefix}{seatNumber}",
                        Price = price,
                        SeatStatus = Status.Available
                    });
                }
            }
            else
            {
                // Ghế ngồi: sơ đồ 4 ghế mỗi hàng, ví dụ A1 A2 A3 A4, B1 B2 B3 B4...
                for (var i = 1; i <= capacity; i++)
                {
                    var rowIndex = (i - 1) / 4;
                    var colIndex = ((i - 1) % 4) + 1;
                    var rowLetter = ((char)('A' + rowIndex)).ToString();

                    seats.Add(new Seat
                    {
                        BusTripId = busTripId,
                        SeatNumber = $"{rowLetter}{colIndex}",
                        Price = price,
                        SeatStatus = Status.Available
                    });
                }
            }

            await _seatRepository.AddRangeAsync(seats);
        }


        public async Task<IActionResult> RevenueReport(string? company, DateTime? fromDate, DateTime? toDate)
        {
            var selectedCompany = string.IsNullOrWhiteSpace(company) ? null : company.Trim();

            var query = _context.Bookings
                .Include(b => b.Seat)
                    .ThenInclude(s => s.BusTrip)
                        .ThenInclude(t => t.Bus)
                .Include(b => b.Seat)
                    .ThenInclude(s => s.BusTrip)
                        .ThenInclude(t => t.BusRoute)
                            .ThenInclude(r => r.StartStop)
                .Include(b => b.Seat)
                    .ThenInclude(s => s.BusTrip)
                        .ThenInclude(t => t.BusRoute)
                            .ThenInclude(r => r.EndStop)
                .Where(b => b.StatusBooking == StatusBooking.Paid && b.Seat != null && b.Seat.BusTrip != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(selectedCompany))
            {
                query = query.Where(b =>
                    b.Seat!.BusTrip!.Bus != null &&
                    b.Seat.BusTrip.Bus.Company != null &&
                    b.Seat.BusTrip.Bus.Company.Trim() == selectedCompany);
            }

            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.Date;
                query = query.Where(b => b.Seat!.BusTrip!.DepartureDate.Date >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date;
                query = query.Where(b => b.Seat!.BusTrip!.DepartureDate.Date <= endDate);
            }

            var bookings = await query.ToListAsync();

            var rows = bookings
                .GroupBy(b => b.Seat!.BusTrip!)
                .Select(g => new RevenueReportRow
                {
                    BusTripId = g.Key.Id,
                    Company = g.Key.Bus?.Company ?? "Chưa có hãng xe",
                    TripName = g.Key.Name ?? "Chưa đặt tên chuyến",
                    BusNumber = g.Key.Bus?.BusNumber ?? "N/A",
                    RouteName = $"{g.Key.BusRoute?.StartStop?.Name ?? "N/A"} - {g.Key.BusRoute?.EndStop?.Name ?? "N/A"}",
                    DepartureDate = g.Key.DepartureDate,
                    DepartureTime = g.Key.DepartureTime,
                    TicketCount = g.Count(),
                    Revenue = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(r => r.Company)
                .ThenBy(r => r.DepartureDate)
                .ThenBy(r => r.DepartureTime)
                .ToList();

            var companyNames = await _context.Buses
                .AsNoTracking()
                .Select(b => b.Company)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToListAsync();

            ViewBag.Companies = companyNames
                .Select(c => c!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            var model = new RevenueReportViewModel
            {
                Company = selectedCompany,
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows,
                TotalTickets = rows.Sum(r => r.TicketCount),
                TotalRevenue = rows.Sum(r => r.Revenue)
            };

            return View(model);
        }
        // Hiển thị form thêm chuyến xe mới
        public async Task<IActionResult> Add()
        {
            await LoadBusTripFormDataAsync();
            ViewBag.DepartureTimeText = "";
            ViewBag.DepartureEndDate = "";
            return View();
        }

        private async Task LoadBusTripFormDataAsync(int? selectedBusId = null, int? selectedBusRouteId = null, string? selectedDriverId = null)
        {
            ViewBag.Buses = await GetBusSelectListAsync(selectedBusId);

            // Dùng cho form tạo/sửa chuyến: lọc xe theo 2 loại sơ đồ ghế
            // Standard  = Ghế ngồi
            // Limousine = Giường nằm Limousine
            ViewBag.BusEntities = await _context.Buses
                .AsNoTracking()
                .OrderBy(b => b.Company)
                .ThenBy(b => b.BusNumber)
                .ToListAsync();

            var busRoutes = await _context.BusRoutes
                .Include(r => r.StartStop)
                .Include(r => r.EndStop)
                .OrderBy(r => r.StartStop!.Name)
                .ThenBy(r => r.EndStop!.Name)
                .ToListAsync();

            ViewBag.BusRoutes = new SelectList(busRoutes, "Id", "DisplayName", selectedBusRouteId);
            ViewBag.BusRoutesList = busRoutes;

            var drivers = await _userManager.GetUsersInRoleAsync(Roles.Role_Driver);
            ViewBag.Drivers = new SelectList(drivers, "Id", "FullName", selectedDriverId);
        }

        public async Task<IActionResult> IndexDriver()
        {

            var drivers = await _userManager.GetUsersInRoleAsync(Roles.Role_Driver);
            return View(drivers);
        }
        public async Task<IActionResult> IndexAdmin()
        {

            var admin = await _userManager.GetUsersInRoleAsync(Roles.Role_Admin);
            return View(admin);
        }
        public async Task<IActionResult> IndexCustomer()
        {

            var user = await _userManager.GetUsersInRoleAsync(Roles.Role_Customer);
            return View(user);
        }
        // Xử lý thêm chuyến xe mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(BusTrip bustrip, IFormFile? imageUrl, List<IFormFile>? images, string? departureTimeText, DateTime? departureEndDate, double? holidayDiscountPercent)
        {
            // Các navigation property không nhập từ form nên bỏ validate để tránh lỗi ModelState ảo.
            ModelState.Remove(nameof(BusTrip.DepartureTime));
            ModelState.Remove(nameof(BusTrip.ImageUrl));
            ModelState.Remove(nameof(BusTrip.Bus));
            ModelState.Remove(nameof(BusTrip.BusRoute));
            ModelState.Remove(nameof(BusTrip.Driver));
            ModelState.Remove(nameof(BusTrip.Admin));
            ModelState.Remove(nameof(BusTrip.Seats));
            ModelState.Remove(nameof(BusTrip.Images));

            if (bustrip.BusId <= 0 || !await _context.Buses.AnyAsync(b => b.Id == bustrip.BusId))
            {
                ModelState.AddModelError(nameof(BusTrip.BusId), "Vui lòng chọn xe hợp lệ.");
            }

            BusRoute? selectedRoute = null;
            double routeBasePrice = 0;
            double finalSeatPrice = 0;
            if (bustrip.BusRouteId <= 0)
            {
                ModelState.AddModelError(nameof(BusTrip.BusRouteId), "Vui lòng chọn tuyến hợp lệ.");
            }
            else
            {
                selectedRoute = await _context.BusRoutes.FirstOrDefaultAsync(r => r.Id == bustrip.BusRouteId);
                if (selectedRoute == null)
                {
                    ModelState.AddModelError(nameof(BusTrip.BusRouteId), "Vui lòng chọn tuyến hợp lệ.");
                }
                else if (!TryParseMoney(selectedRoute.Price, out routeBasePrice) || routeBasePrice <= 0)
                {
                    ModelState.AddModelError(nameof(BusTrip.BusRouteId), "Tuyến xe chưa có giá vé hợp lệ. Vui lòng cập nhật giá ở mục Quản lý tuyến đường trước.");
                }
                else
                {
                    finalSeatPrice = CalculateAdjustedPrice(routeBasePrice, holidayDiscountPercent);
                }
            }

            if (holidayDiscountPercent.HasValue && (holidayDiscountPercent < -100 || holidayDiscountPercent > 300))
            {
                ModelState.AddModelError("HolidayDiscountPercent", "Phần trăm điều chỉnh giá phải nằm trong khoảng -100 đến 300. Nhập số dương để tăng giá, số âm để giảm giá.");
            }

            if (bustrip.Capacity == null || bustrip.Capacity <= 0)
            {
                ModelState.AddModelError(nameof(BusTrip.Capacity), "Vui lòng nhập số ghế lớn hơn 0.");
            }

            if (!TryParseDepartureTime24h(departureTimeText, out var parsedDepartureTime))
            {
                ModelState.AddModelError("DepartureTimeText", "Vui lòng nhập giờ theo định dạng 24h HH:mm, ví dụ 20:00.");
            }
            else
            {
                bustrip.DepartureTime = parsedDepartureTime;
            }

            var startDate = bustrip.DepartureDate.Date;
            var endDate = departureEndDate?.Date ?? startDate;
            if (departureEndDate.HasValue && endDate < startDate)
            {
                ModelState.AddModelError("DepartureEndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? mainImageUrl = null;
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        mainImageUrl = await SaveImage(imageUrl);
                    }

                    var user = await _userManager.GetUserAsync(User);
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        var newBusTrip = new BusTrip
                        {
                            Name = bustrip.Name,
                            BusId = bustrip.BusId,
                            Capacity = bustrip.Capacity,
                            ImageUrl = mainImageUrl,
                            DepartureDate = date,
                            DepartureTime = parsedDepartureTime,
                            BusRouteId = bustrip.BusRouteId,
                            DriverId = bustrip.DriverId,
                            AdminId = user?.Id,
                            TripStatus = StatusTrip.NotYetDeparted,
                            PriceAdjustmentPercent = holidayDiscountPercent ?? 0
                        };

                        await _bustripRepository.AddAsync(newBusTrip);

                        // Đồng bộ giá tuyến xe sang chuyến xe: tự tạo ghế theo capacity với giá sau điều chỉnh.
                        await CreateSeatsForTripAsync(newBusTrip.Id, bustrip.Capacity!.Value, finalSeatPrice);
                    }

                    var adjustmentText = (holidayDiscountPercent ?? 0) > 0
                        ? $"+{(holidayDiscountPercent ?? 0):0.##}%"
                        : $"{(holidayDiscountPercent ?? 0):0.##}%";
                    TempData["Message"] = $"Tạo chuyến thành công. Giá trước điều chỉnh: {routeBasePrice:N0} VND, điều chỉnh giá: {adjustmentText}, giá sau điều chỉnh: {finalSeatPrice:N0} VND.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var detail = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError(string.Empty, "Không thể tạo chuyến xe. Vui lòng kiểm tra lại xe, tuyến đường, tài xế và dữ liệu liên quan. Chi tiết: " + detail);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tạo chuyến xe. Chi tiết: " + ex.Message);
                }
            }

            await LoadBusTripFormDataAsync(bustrip.BusId, bustrip.BusRouteId, bustrip.DriverId);
            ViewBag.DepartureTimeText = departureTimeText;
            ViewBag.DepartureEndDate = departureEndDate?.ToString("yyyy-MM-dd");
            ViewBag.HolidayDiscountPercent = holidayDiscountPercent?.ToString(CultureInfo.InvariantCulture);
            return View(bustrip);
        }

        // Lưu hình ảnh chuyến xe
        private async Task<string> SaveImage(IFormFile imageUrl)
        {
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(uploadFolder);

            var safeFileName = Path.GetFileName(imageUrl.FileName);
            var savePath = Path.Combine(uploadFolder, safeFileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await imageUrl.CopyToAsync(fileStream);
            }
            return "/images/" + safeFileName;
        }


        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _bustripRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.lsImage = _context.BusTripImages.Where(x => x.BusTripId == id).ToList();
            return View(product);
        }

        public async Task<IActionResult> Exit(int id)
        {
            var bustrip = await _bustripRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (bustrip == null)
            {
                return NotFound();
            }
            var bookings = await _bookingRepository.GetBookingsByTripIdAsync(id);
            var seatInfoByBookingId = new Dictionary<int, Seat>();
            foreach (var booking in bookings)
            {
                var seat = await _seatRepository.GetSeatByBookingIdAsync(booking.Id);
                seatInfoByBookingId.Add(booking.Id, seat);
            }

            var stops = await _stopRepository.GetStopsByBusRouteId(bustrip.BusRouteId);

            var sortedStops = stops.OrderBy(stop => stop.Stt).ToList();

            ViewBag.Stops = sortedStops;
            ViewBag.Bookings = bookings;
            ViewBag.SeatInfoByBookingId = seatInfoByBookingId;
            ViewBag.lsImage = _context.BusTripImages.Where(x => x.BusTripId == id).ToList();
            bustrip.TripStatus = StatusTrip.Running;
            await _bustripRepository.UpdateAsync(bustrip);
            return View(bustrip);
        }
        public async Task<IActionResult> TripReport(int id)
        {
            var tripReport = await _TripReportRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);
            if (tripReport == null)
            {
                return NotFound();
            }

            var busTripId = tripReport.BusTripId;

            var bookings = await _bookingRepository.GetBookingsByTripIdAsync(busTripId);
            var seatInfoByBookingId = new Dictionary<int, Seat>();
            foreach (var booking in bookings)
            {
                var seat = await _seatRepository.GetSeatByBookingIdAsync(booking.Id);
                seatInfoByBookingId.Add(booking.Id, seat);
            }

            ViewBag.Bookings = bookings;
            ViewBag.SeatInfoByBookingId = seatInfoByBookingId;

            return View(tripReport);

        }
        public async Task<IActionResult> TripReportIndex()
        {
            var tripreport = await _TripReportRepository.GetAllAsync();
            return View(tripreport);
        }
        public async Task<IActionResult> Update(int id)
        {
            var bustrip = await _bustripRepository.GetByIdAsync(id);

            if (bustrip == null)
            {
                return NotFound();
            }

            var departureDateTime = bustrip.DepartureDate.Date.Add(bustrip.DepartureTime.TimeOfDay);
            if (departureDateTime < DateTime.Now)
            {
                TempData["Message"] = "Chuyến xe trong quá khứ chỉ được xem chi tiết, không được sửa.";
                return RedirectToAction(nameof(Display), new { id = bustrip.Id });
            }

            await LoadBusTripFormDataAsync(bustrip.BusId, bustrip.BusRouteId, bustrip.DriverId);
            ViewBag.DepartureTimeText = bustrip.DepartureTime.ToString("HH:mm");
            ViewBag.HolidayDiscountPercent = bustrip.PriceAdjustmentPercent.ToString(CultureInfo.InvariantCulture);
            return View(bustrip);
        }
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, BusTrip bustrip, IFormFile? imageUrl, string? departureTimeText, double? holidayDiscountPercent)
        {
            ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho ImageUrl
            ModelState.Remove(nameof(BusTrip.DepartureTime));

            if (id != bustrip.Id)
            {
                return NotFound();
            }

            if (!TryParseDepartureTime24h(departureTimeText, out var parsedDepartureTime))
            {
                ModelState.AddModelError("DepartureTimeText", "Vui lòng nhập giờ theo định dạng 24h HH:mm, ví dụ 20:00");
            }
            else
            {
                bustrip.DepartureTime = parsedDepartureTime;
            }

            if (ModelState.IsValid)
            {
                var existingbustrip = await _bustripRepository.GetByIdAsync(id);
                if (existingbustrip == null)
                {
                    return NotFound();
                }

                var existingDepartureDateTime = existingbustrip.DepartureDate.Date.Add(existingbustrip.DepartureTime.TimeOfDay);
                if (existingDepartureDateTime < DateTime.Now)
                {
                    TempData["Message"] = "Chuyến xe trong quá khứ chỉ được xem chi tiết, không được sửa.";
                    return RedirectToAction(nameof(Display), new { id = existingbustrip.Id });
                }

                if (imageUrl == null)
                {
                    bustrip.ImageUrl = existingbustrip.ImageUrl;
                }
                else
                {
                    bustrip.ImageUrl = await SaveImage(imageUrl);
                }

                existingbustrip.Name = bustrip.Name;
                existingbustrip.Capacity = bustrip.Capacity;
                existingbustrip.ImageUrl = bustrip.ImageUrl;
                existingbustrip.DepartureDate = bustrip.DepartureDate;
                existingbustrip.DepartureTime = parsedDepartureTime;
                existingbustrip.BusId = bustrip.BusId;
                existingbustrip.BusRouteId = bustrip.BusRouteId;
                existingbustrip.DriverId = bustrip.DriverId;
                existingbustrip.TripStatus = bustrip.TripStatus;
                existingbustrip.PriceAdjustmentPercent = holidayDiscountPercent ?? bustrip.PriceAdjustmentPercent;

                // Nếu đổi tuyến hoặc % điều chỉnh, cập nhật lại giá cho các ghế còn trống.
                var selectedRoute = await _context.BusRoutes.FirstOrDefaultAsync(r => r.Id == existingbustrip.BusRouteId);
                if (selectedRoute != null && TryParseMoney(selectedRoute.Price, out var routeBasePrice))
                {
                    var finalSeatPrice = CalculateAdjustedPrice(routeBasePrice, existingbustrip.PriceAdjustmentPercent);
                    var availableSeats = await _context.Seats
                        .Where(s => s.BusTripId == existingbustrip.Id && s.SeatStatus == Status.Available)
                        .ToListAsync();

                    foreach (var seat in availableSeats)
                    {
                        seat.Price = finalSeatPrice;
                    }

                    await _context.SaveChangesAsync();
                }

                await _bustripRepository.UpdateAsync(existingbustrip);
                return RedirectToAction(nameof(Index));
            }
            await LoadBusTripFormDataAsync(bustrip.BusId, bustrip.BusRouteId, bustrip.DriverId);
            ViewBag.DepartureTimeText = departureTimeText;
            ViewBag.HolidayDiscountPercent = holidayDiscountPercent?.ToString(CultureInfo.InvariantCulture);
            return View(bustrip);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var bustrip = await _bustripRepository.GetByIdAsync(id);
            if (bustrip == null)
            {
                return NotFound();
            }
            return View(bustrip);
        }
        public IActionResult SearchAndResult()
        {
            return View();
        }



        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bustripRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DisplayDriverRegistration(int id)
        {
            var driverRegis = await _driverregisRepository.GetByIdAsync(id);
            if (driverRegis == null)
            {
                return NotFound();
            }

            return View(driverRegis);
        }
        public async Task<IActionResult> IndexDriverRegis()
        {

            var driverRegisList = await _driverregisRepository.GetAllAsync();
            return View(driverRegisList);
        }
        public async Task<IActionResult> UpdateRoleDriver(int id)
        {
            // Tìm kiếm phiếu đăng ký tài xế
            var driverRegis = await _driverregisRepository.GetByIdAsync(id);
            if (driverRegis == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái thành "Đã phê duyệt"
            driverRegis.ApproveStatus = ApproveStatus.Approved;
            await _driverregisRepository.UpdateAsync(driverRegis);

            // Tìm kiếm người dùng
            var user = await _userManager.FindByIdAsync(driverRegis.DriverId);
            if (user == null)
            {
                return NotFound();
            }

            // Thay đổi vai trò của người dùng từ "Customer" thành "Driver"
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Customer"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Customer");
            }
            if (!currentRoles.Contains("Driver"))
            {
                await _userManager.AddToRoleAsync(user, "Driver");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
