using DOANCOSO26.Data;
using DOANCOSO26.Models;
using DOANCOSO26.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DOANCOSO26.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BusController : Controller
    {
        private readonly IBusTripRepository _bustripRepository;
        private readonly IBusRepository _busRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITripReportRepository _tripReportRepository;

        public BusController(IBusTripRepository bustripRepository, IBusRepository busRepository, ApplicationDbContext context, IBookingRepository bookingRepository, ITripReportRepository tripReportRepository, UserManager<ApplicationUser> userManager)
        {
            _bustripRepository = bustripRepository;
            _busRepository = busRepository;
            _context = context;
            _bookingRepository = bookingRepository;
            _tripReportRepository = tripReportRepository;
            _userManager = userManager;
        }

        private sealed class BusNumberOption
        {
            public string Company { get; set; } = string.Empty;
            public string BusNumber { get; set; } = string.Empty;
            public BusType BusType { get; set; }
        }

        // Danh mục biển số mẫu để admin chọn nhanh khi tạo/sửa xe.
        // Danh sách này cũng được gộp thêm với các xe đã có trong database.
        private static readonly List<BusNumberOption> DefaultBusNumberOptions = new()
        {
            new() { Company = "Phương Trang", BusNumber = "52K B1820", BusType = BusType.Limousine },
            new() { Company = "Phương Trang", BusNumber = "52K B1820-1", BusType = BusType.Limousine },
            new() { Company = "Phương Trang", BusNumber = "52K B1820-2", BusType = BusType.Limousine },
            new() { Company = "Phương Trang", BusNumber = "57K A1590", BusType = BusType.Limousine },
            new() { Company = "Phương Trang", BusNumber = "56H 12567", BusType = BusType.Limousine },
            new() { Company = "Phương Trang", BusNumber = "51B-123.45", BusType = BusType.Standard },
            new() { Company = "Phương Trang", BusNumber = "51B-234.56", BusType = BusType.Standard },

            new() { Company = "Mai Linh", BusNumber = "52D A1201", BusType = BusType.Limousine },
            new() { Company = "Mai Linh", BusNumber = "52D A1201-1", BusType = BusType.Limousine },
            new() { Company = "Mai Linh", BusNumber = "59G 6789", BusType = BusType.Standard },
            new() { Company = "Mai Linh", BusNumber = "51B-345.67", BusType = BusType.Standard },
            new() { Company = "Mai Linh", BusNumber = "51B-456.78", BusType = BusType.Limousine },

            new() { Company = "Thành Bưởi", BusNumber = "51K F1220", BusType = BusType.Limousine },
            new() { Company = "Thành Bưởi", BusNumber = "51K F1220-1", BusType = BusType.Limousine },
            new() { Company = "Thành Bưởi", BusNumber = "51K X1220", BusType = BusType.Standard },
            new() { Company = "Thành Bưởi", BusNumber = "51K X1220-1", BusType = BusType.Standard },
            new() { Company = "Thành Bưởi", BusNumber = "51B-567.89", BusType = BusType.Limousine },

            new() { Company = "Tuấn Nga", BusNumber = "52K B1820-1", BusType = BusType.Limousine },
            new() { Company = "Tuấn Nga", BusNumber = "52K B1820-2", BusType = BusType.Limousine },
            new() { Company = "Tuấn Nga", BusNumber = "77B-123.45", BusType = BusType.Standard },
            new() { Company = "Tuấn Nga", BusNumber = "77B-234.56", BusType = BusType.Limousine },

            new() { Company = "Kumho", BusNumber = "38KD 72191-1", BusType = BusType.Limousine },
            new() { Company = "Kumho", BusNumber = "51B-678.90", BusType = BusType.Standard },
            new() { Company = "Kumho", BusNumber = "51B-789.01", BusType = BusType.Limousine },
            new() { Company = "Kumho", BusNumber = "60B-123.45", BusType = BusType.Limousine },
        };

        private async Task<List<BusNumberOption>> GetAllBusNumberOptionsAsync()
        {
            var busesInDatabase = await _context.Buses
                .AsNoTracking()
                .Where(b => !string.IsNullOrWhiteSpace(b.Company)
                            && !string.IsNullOrWhiteSpace(b.BusNumber)
                            && b.BusType.HasValue)
                .Select(b => new
                {
                    b.Company,
                    b.BusNumber,
                    BusType = b.BusType!.Value
                })
                .ToListAsync();

            return DefaultBusNumberOptions
                .Concat(busesInDatabase.Select(b => new BusNumberOption
                {
                    Company = b.Company,
                    BusNumber = b.BusNumber,
                    BusType = b.BusType
                }))
                .GroupBy(b => new { b.Company, b.BusNumber, b.BusType })
                .Select(g => g.First())
                .OrderBy(b => b.Company)
                .ThenBy(b => b.BusType)
                .ThenBy(b => b.BusNumber)
                .ToList();
        }

        private async Task LoadBusFormSelectListsAsync()
        {
            var options = await GetAllBusNumberOptionsAsync();

            ViewBag.CompanyList = options
                .Select(b => b.Company)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.TypeList = Enum.GetValues(typeof(BusType))
                .Cast<BusType>()
                .Select(t => t.ToString())
                .ToList();
        }

        public async Task<IActionResult> Index(string? companyFilter, string? typeFilter, string? busNumberFilter)
        {
            var allBuses = (await _busRepository.GetAllAsync()).ToList();
            var buses = allBuses.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(companyFilter))
            {
                buses = buses.Where(b => b.Company == companyFilter);
            }

            if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<BusType>(typeFilter, out var busTypeEnum))
            {
                buses = buses.Where(b => b.BusType == busTypeEnum);
            }

            if (!string.IsNullOrWhiteSpace(busNumberFilter))
            {
                buses = buses.Where(b => b.BusNumber == busNumberFilter);
            }

            var allBusNumberOptions = await GetAllBusNumberOptionsAsync();
            var busNumberOptions = allBusNumberOptions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(companyFilter))
            {
                busNumberOptions = busNumberOptions.Where(b => b.Company == companyFilter);
            }

            if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<BusType>(typeFilter, out var selectedType))
            {
                busNumberOptions = busNumberOptions.Where(b => b.BusType == selectedType);
            }

            ViewBag.CompanyList = allBusNumberOptions
                .Select(b => b.Company)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.TypeList = Enum.GetValues(typeof(BusType))
                .Cast<BusType>()
                .Select(t => t.ToString())
                .ToList();

            ViewBag.BusNumberList = busNumberOptions
                .Select(b => b.BusNumber)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            ViewBag.CompanyFilter = companyFilter;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.BusNumberFilter = busNumberFilter;

            var result = buses
                .OrderBy(b => b.Company)
                .ThenBy(b => b.BusNumber)
                .ToList();

            return View(result);
        }

        // API dùng cho dropdown biển số xe theo hãng xe và loại xe.
        [HttpGet]
        public async Task<IActionResult> GetBusNumbers(string? company, string? busType)
        {
            var options = (await GetAllBusNumberOptionsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(company))
            {
                options = options.Where(b => b.Company == company);
            }

            if (!string.IsNullOrWhiteSpace(busType) && Enum.TryParse<BusType>(busType, out var selectedType))
            {
                options = options.Where(b => b.BusType == selectedType);
            }

            var result = options
                .OrderBy(b => b.BusNumber)
                .Select(b => new
                {
                    company = b.Company,
                    busNumber = b.BusNumber,
                    busType = b.BusType.ToString()
                })
                .ToList();

            return Json(result);
        }

        // Hiển thị form thêm xe mới
        public async Task<IActionResult> Add()
        {
            await LoadBusFormSelectListsAsync();
            return View(new Bus());
        }

        // Xử lý thêm xe mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Bus bus)
        {
            if (ModelState.IsValid)
            {
                await _busRepository.AddAsync(bus);
                return RedirectToAction(nameof(Index));
            }

            await LoadBusFormSelectListsAsync();
            return View(bus);
        }

        // Hiển thị thông tin chi tiết xe
        public async Task<IActionResult> Display(int id)
        {
            var bus = await _busRepository.GetByIdAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        public async Task<IActionResult> Update(int id)
        {
            var bus = await _busRepository.GetByIdAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            await LoadBusFormSelectListsAsync();
            return View(bus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Bus bus)
        {
            if (id != bus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingBus = await _busRepository.GetByIdAsync(id);
                if (existingBus == null)
                {
                    return NotFound();
                }

                existingBus.Company = bus.Company;
                existingBus.BusNumber = bus.BusNumber;
                existingBus.BusType = bus.BusType;
                existingBus.OperatingStatus = bus.OperatingStatus;

                await _busRepository.UpdateAsync(existingBus);
                return RedirectToAction(nameof(Index));
            }

            await LoadBusFormSelectListsAsync();
            return View(bus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, BusOperatingStatus status, string? companyFilter, string? typeFilter, string? busNumberFilter)
        {
            var bus = await _busRepository.GetByIdAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            bus.OperatingStatus = status;
            await _busRepository.UpdateAsync(bus);

            return RedirectToAction(nameof(Index), new
            {
                companyFilter,
                typeFilter,
                busNumberFilter
            });
        }

        // Hiển thị form xác nhận xóa xe
        public async Task<IActionResult> Delete(int id)
        {
            var bus = await _busRepository.GetByIdAsync(id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        // Xử lý xóa xe
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _busRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Dash(int id)
        {
            var model = new BookingCountsViewModel
            {
                TodayBookings = await _bookingRepository.CountBookingsTodayAsync(),
                TotalBookings = await _bookingRepository.CountAllBookingsAsync(),
                TotalPaidBookingsPrice = await _bookingRepository.GetTotalPaidBookingsPriceAsync(),
                TotalCost = await _tripReportRepository.GetTotalCostAsync(),
                Users = await _userManager.Users.ToListAsync(),
            };

            return View(model);
        }
    }
}

