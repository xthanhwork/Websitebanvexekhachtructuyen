using DOANCOSO26.Models;
using DOANCOSO26.Repository;
using DOANCOSO26.Services;
using DOANCOSO26.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;
using System.Globalization;

namespace DOANCOSO26.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ReservationController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IBusTripRepository _busTripRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusRepository _busRepository;
        private readonly InvoiceCodeGenerator _invoiceCodeGenerator;
        private readonly IBusRouteRepository _busRouteRepository;
        private readonly IStopRepository _stopRepository;
        private readonly IVnPaySevices _vnPaySevices;
        private readonly SmtpEmailSender _emailSender;

        public ReservationController(
            IBookingRepository bookingRepository,
            SmtpEmailSender emailSender,
            ISeatRepository seatRepository,
            IBusTripRepository busTripRepository,
            UserManager<ApplicationUser> userManager,
            IBusRepository busRepository,
            InvoiceCodeGenerator invoiceCodeGenerator,
            IBusRouteRepository busRouteRepository,
            IStopRepository stopRepository,
            IVnPaySevices vnPaySevices)
        {
            _bookingRepository = bookingRepository;
            _emailSender = emailSender;
            _seatRepository = seatRepository;
            _busTripRepository = busTripRepository;
            _userManager = userManager;
            _busRepository = busRepository;
            _invoiceCodeGenerator = invoiceCodeGenerator;
            _busRouteRepository = busRouteRepository;
            _stopRepository = stopRepository;
            _vnPaySevices = vnPaySevices;
        }

        private static readonly Dictionary<string, double> VoucherDiscounts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["HUIT10"] = 10,
            ["HUIT20"] = 20,
            ["LE5"] = 5
        };

        private static List<int> ParseSeatIds(string? seatIds, int? fallbackSeatId = null)
        {
            var ids = new List<int>();
            if (!string.IsNullOrWhiteSpace(seatIds))
            {
                ids.AddRange(seatIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => int.TryParse(x, out var id) ? id : 0)
                    .Where(id => id > 0));
            }

            if (fallbackSeatId.HasValue && fallbackSeatId.Value > 0)
            {
                ids.Add(fallbackSeatId.Value);
            }

            return ids.Distinct().ToList();
        }

        private static string BuildPromoNote(string? oldNote, string? promoCode, double discountPercent)
        {
            var note = oldNote ?? string.Empty;
            if (string.IsNullOrWhiteSpace(promoCode) || discountPercent <= 0)
            {
                return note;
            }

            var promoText = $"[PROMO:{promoCode.Trim().ToUpperInvariant()};DISCOUNT:{discountPercent:0.##}%]";
            return string.IsNullOrWhiteSpace(note) ? promoText : note + " " + promoText;
        }

        private async Task<int> CountVoucherOrdersUsedByUserAsync(string userId)
        {
            var allBookings = await _bookingRepository.GetAllByUserIdAsync(userId);
            return allBookings
                .Where(b => !string.IsNullOrWhiteSpace(b.Note) && b.Note.Contains("[PROMO:", StringComparison.OrdinalIgnoreCase))
                .Select(b => b.InvoiceCode ?? b.Id.ToString(CultureInfo.InvariantCulture))
                .Distinct()
                .Count();
        }

        private async Task<List<Booking>> GetBookingGroupAsync(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.InvoiceCode) || string.IsNullOrWhiteSpace(booking.UserId))
            {
                return new List<Booking> { booking };
            }

            var userBookings = await _bookingRepository.GetAllByUserIdAsync(booking.UserId);
            return userBookings
                .Where(b => b.InvoiceCode == booking.InvoiceCode)
                .OrderBy(b => b.Seat?.SeatNumber)
                .ToList();
        }

        public async Task<IActionResult> Add(int? tripId, int? seatId, string? seatIds)
        {
            if (!tripId.HasValue)
            {
                return BadRequest("TripId is required");
            }

            var selectedSeatIds = ParseSeatIds(seatIds, seatId);
            if (!selectedSeatIds.Any())
            {
                return BadRequest("Vui lòng chọn ít nhất một ghế.");
            }

            var allSeats = await _seatRepository.GetAllAsync();
            var selectedSeats = allSeats
                .Where(s => selectedSeatIds.Contains(s.Id) && s.BusTripId == tripId.Value)
                .OrderBy(s => s.SeatNumber)
                .ToList();

            if (selectedSeats.Count != selectedSeatIds.Count)
            {
                return BadRequest("Có ghế không thuộc chuyến xe này hoặc không tồn tại.");
            }

            if (selectedSeats.Any(s => s.SeatStatus != Status.Available))
            {
                return BadRequest("Một hoặc nhiều ghế đã được đặt. Vui lòng chọn lại.");
            }

            var stops = await _stopRepository.GetStopsByBusTripIdAsync(tripId.Value);
            stops = stops.OrderBy(s => s.Stt).ToList();

            ViewBag.SelectedSeats = selectedSeats;
            ViewBag.SelectedSeat = selectedSeats.FirstOrDefault();
            ViewBag.SeatIds = string.Join(",", selectedSeats.Select(s => s.Id));
            ViewBag.TripId = tripId.Value;
            ViewBag.TotalPrice = selectedSeats.Sum(s => s.Price);
            ViewBag.Stops = new SelectList(stops, "Id", "DisplayName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Booking booking, string payment = "COD", string? seatIds = null, string? promoCode = null)
        {
            ModelState.Remove(nameof(Booking.Seat));
            ModelState.Remove(nameof(Booking.BusTrip));
            ModelState.Remove(nameof(Booking.User));
            ModelState.Remove(nameof(Booking.Goods));

            var selectedSeatIds = ParseSeatIds(seatIds, booking.SeatId);
            if (!selectedSeatIds.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một ghế.");
                return await ReloadAddView(booking.TripId ?? 0, selectedSeatIds);
            }

            if (!ModelState.IsValid)
            {
                return await ReloadAddView(booking.TripId ?? 0, selectedSeatIds);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var allSeats = await _seatRepository.GetAllAsync();
            var seats = allSeats
                .Where(s => selectedSeatIds.Contains(s.Id))
                .OrderBy(s => s.SeatNumber)
                .ToList();

            if (seats.Count != selectedSeatIds.Count)
            {
                ModelState.AddModelError("", "Có ghế không tồn tại. Vui lòng chọn lại.");
                return await ReloadAddView(booking.TripId ?? seats.FirstOrDefault()?.BusTripId ?? 0, selectedSeatIds);
            }

            var tripId = booking.TripId ?? seats.First().BusTripId;
            if (seats.Any(s => s.BusTripId != tripId))
            {
                ModelState.AddModelError("", "Chỉ được đặt nhiều ghế trong cùng một chuyến xe.");
                return await ReloadAddView(tripId, selectedSeatIds);
            }

            if (seats.Any(s => s.SeatStatus != Status.Available))
            {
                ModelState.AddModelError("", "Một hoặc nhiều ghế đã được đặt. Vui lòng chọn lại.");
                return await ReloadAddView(tripId, selectedSeatIds);
            }

            var discountPercent = 0d;
            var normalizedPromoCode = promoCode?.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(normalizedPromoCode))
            {
                if (!VoucherDiscounts.TryGetValue(normalizedPromoCode, out discountPercent))
                {
                    ModelState.AddModelError("PromoCode", "Mã khuyến mãi không hợp lệ. Mã dùng thử: HUIT10, HUIT20, LE5.");
                    return await ReloadAddView(tripId, selectedSeatIds);
                }

                var usedVoucherOrders = await CountVoucherOrdersUsedByUserAsync(user.Id);
                if (usedVoucherOrders >= 2)
                {
                    ModelState.AddModelError("PromoCode", "Tài khoản này đã sử dụng đủ 2 voucher.");
                    return await ReloadAddView(tripId, selectedSeatIds);
                }
            }

            var invoiceCode = _invoiceCodeGenerator.GenerateInvoiceCode();
            var createdBookings = new List<Booking>();
            var originalTotal = seats.Sum(s => s.Price);
            var finalTotal = Math.Round(originalTotal * (100 - discountPercent) / 100, 0);
            var perTicketFactor = originalTotal > 0 ? finalTotal / originalTotal : 1;

            try
            {
                foreach (var seat in seats)
                {
                    var ticketPrice = Math.Round(seat.Price * perTicketFactor, 0);
                    var newBooking = new Booking
                    {
                        UserId = user.Id,
                        InvoiceCode = invoiceCode,
                        UserName = booking.UserName,
                        SDT = booking.SDT,
                        Email = booking.Email,
                        Timebooking = DateTime.Now,
                        TotalPrice = ticketPrice,
                        Note = BuildPromoNote(booking.Note, normalizedPromoCode, discountPercent),
                        SeatId = seat.Id,
                        TripId = tripId,
                        PickupStopId = booking.PickupStopId,
                        DropOffStopId = booking.DropOffStopId,
                        StatusBooking = StatusBooking.UnPaid,
                        StatusOnBus = StatusOnBus.NotintheBus
                    };

                    seat.SeatStatus = Status.Booked;
                    await _seatRepository.UpdateAsync(seat);

                    var created = await _bookingRepository.AddAsync(newBooking);
                    createdBookings.Add(created);
                }
            }
            catch
            {
                foreach (var seat in seats)
                {
                    seat.SeatStatus = Status.Available;
                    await _seatRepository.UpdateAsync(seat);
                }
                throw;
            }

            if (payment == "Thanh Toán VNPay")
            {
                var firstBooking = createdBookings.First();
                var vnPayModel = new VnPayRequestModel
                {
                    BookingId = firstBooking.Id,
                    TotalPrice = createdBookings.Sum(b => b.TotalPrice),
                    FullName = firstBooking.UserName ?? user.UserName ?? string.Empty,
                    PhoneNum = firstBooking.SDT ?? string.Empty,
                    Timebooking = firstBooking.Timebooking
                };

                return Redirect(_vnPaySevices.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            await SendBookingConfirmationEmailAsync(
                createdBookings,
                "Xác nhận đặt vé HUIT BUS",
                "Chưa thanh toán"
            );

            TempData["Message"] = $"Đặt thành công {createdBookings.Count} vé. Mã đơn hàng: {invoiceCode}.";
            return RedirectToAction("Display", new { id = createdBookings.First().Id });
        }

        private async Task<IActionResult> ReloadAddView(int tripId, List<int>? selectedSeatIds = null)
        {
            var availableSeats = await _seatRepository.GetAllAsync();
            var seatsOfTrip = availableSeats.Where(seat => seat.BusTripId == tripId).ToList();
            var selectedSeats = seatsOfTrip
                .Where(seat => selectedSeatIds != null && selectedSeatIds.Contains(seat.Id))
                .OrderBy(seat => seat.SeatNumber)
                .ToList();

            var stops = await _stopRepository.GetStopsByBusTripIdAsync(tripId);
            stops = stops.OrderBy(s => s.Stt).ToList();

            ViewBag.SelectedSeats = selectedSeats;
            ViewBag.SelectedSeat = selectedSeats.FirstOrDefault();
            ViewBag.SeatIds = string.Join(",", selectedSeats.Select(s => s.Id));
            ViewBag.TotalPrice = selectedSeats.Sum(s => s.Price);
            ViewBag.Seats = new SelectList(seatsOfTrip.Where(s => s.SeatStatus == Status.Available), "Id", "SeatNumber");
            ViewBag.TripId = tripId;
            ViewBag.Stops = new SelectList(stops, "Id", "DisplayName");

            return View("Add");
        }

        [Authorize]
        public async Task<IActionResult> VnPayment()
        {
            var response = _vnPaySevices.PaymentExecute(Request.Query);

            if (response == null || string.IsNullOrEmpty(response.OrderId))
            {
                TempData["Message"] = "Không xác định được mã đơn thanh toán VNPAY.";
                return RedirectToAction("PaymentFail", "Reservation");
            }

            if (!int.TryParse(response.OrderId, out var bookingId))
            {
                TempData["Message"] = $"Mã đơn VNPay không hợp lệ: {response.OrderId}";
                return RedirectToAction("PaymentFail", "Reservation");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                TempData["Message"] = $"Không tìm thấy vé thanh toán. Mã VNPay trả về: {bookingId}";
                return RedirectToAction("PaymentFail", "Reservation");
            }

            var group = await GetBookingGroupAsync(booking);

            if (!response.Success || response.VnPayResponseCode != "00")
            {
                foreach (var item in group)
                {
                    item.StatusBooking = StatusBooking.Cancelled;
                    var itemSeat = item.Seat ?? await _seatRepository.GetByIdAsync(item.SeatId);
                    if (itemSeat != null)
                    {
                        itemSeat.SeatStatus = Status.Available;
                        await _seatRepository.UpdateAsync(itemSeat);
                    }
                    await _bookingRepository.UpdateAsync(item);
                }

                TempData["Message"] = $"Thanh toán VNPAY thất bại. Mã phản hồi: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail", "Reservation");
            }

            foreach (var item in group)
            {
                item.StatusBooking = StatusBooking.Paid;
                await _bookingRepository.UpdateAsync(item);
            }

            await SendBookingConfirmationEmailAsync(
                group,
                "Xác nhận thanh toán vé xe HUIT BUS",
                "Đã thanh toán qua VNPAY"
            );

            TempData["Message"] = $"Thanh toán VNPAY thành công cho {group.Count} vé.";
            return RedirectToAction("ViewMyBookings", "Reservation");
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        public async Task<IActionResult> Display(int id)
        {
            var buses = await _busRepository.GetAllAsync();
            ViewBag.Buses = new SelectList(buses);

            var seats = await _seatRepository.GetAllAsync();
            ViewBag.Seats = new SelectList(seats);

            var busTrips = await _busTripRepository.GetAllAsync();
            ViewBag.Bustrips = new SelectList(busTrips);

            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> Update(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            var existingBooking = await _bookingRepository.GetByIdAsync(id);

            if (existingBooking == null)
            {
                return NotFound();
            }

            existingBooking.SDT = booking.SDT;
            existingBooking.Timebooking = booking.Timebooking;
            existingBooking.TotalPrice = booking.TotalPrice;
            existingBooking.Note = booking.Note;
            existingBooking.SeatId = booking.SeatId;

            await _bookingRepository.UpdateAsync(existingBooking);

            return RedirectToAction(nameof(ViewMyBookings));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.BookingGroup = await GetBookingGroupAsync(booking);
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id, List<int>? cancelBookingIds)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            var group = await GetBookingGroupAsync(booking);
            var selectedIds = cancelBookingIds != null && cancelBookingIds.Any()
                ? cancelBookingIds.Distinct().ToList()
                : new List<int> { id };

            var cancelList = group.Where(b => selectedIds.Contains(b.Id)).ToList();
            if (!cancelList.Any())
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một vé cần hủy.";
                return RedirectToAction(nameof(ViewMyBookings));
            }

            foreach (var item in cancelList)
            {
                if (item.StatusBooking == StatusBooking.Cancelled)
                {
                    continue;
                }

                var seat = item.Seat ?? await _seatRepository.GetByIdAsync(item.SeatId);
                if (seat == null)
                {
                    continue;
                }

                var busTrip = item.BusTrip ?? await _busTripRepository.GetByIdAsync(seat.BusTripId);
                if (busTrip == null)
                {
                    continue;
                }

                var departureDateTime = busTrip.DepartureDate.Date + busTrip.DepartureTime.TimeOfDay;
                if (departureDateTime <= DateTime.Now.AddHours(24))
                {
                    TempData["Message"] = "Không thể hủy vé trong vòng 24 giờ trước giờ xe chạy.";
                    return RedirectToAction(nameof(ViewMyBookings));
                }

                item.StatusBooking = StatusBooking.Cancelled;
                seat.SeatStatus = Status.Available;
                await _seatRepository.UpdateAsync(seat);
                await _bookingRepository.UpdateAsync(item);
            }

            TempData["Message"] = $"Đã hủy {cancelList.Count} vé trong đơn hàng {booking.InvoiceCode}.";
            return RedirectToAction(nameof(ViewMyBookings));
        }

        [HttpGet]
        public IActionResult SearchBooking()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchBooking(string invoiceCode)
        {
            var booking = await _bookingRepository.GetByInvoiceCodeAsync(invoiceCode);

            if (booking != null)
            {
                return RedirectToAction("Display", new { id = booking.Id });
            }

            ModelState.AddModelError("", $"Không tìm thấy vé với mã {invoiceCode}");
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetBusTripAndBusInfo(int seatId)
        {
            var seat = await _seatRepository.GetByIdAsync(seatId);

            if (seat == null)
            {
                return Json(new
                {
                    start = "",
                    end = "",
                    busnumber = "",
                    company = "",
                    departureTime = "",
                    departtureDate = ""
                });
            }

            var busTrip = await _busTripRepository.GetByIdAsync(seat.BusTripId);

            if (busTrip == null)
            {
                return Json(new
                {
                    start = "",
                    end = "",
                    busnumber = "",
                    company = "",
                    departureTime = "",
                    departtureDate = ""
                });
            }

            var bus = await _busRepository.GetByIdAsync(busTrip.BusId);
            var route = await _busRouteRepository.GetByIdAsync(busTrip.BusRouteId);

            string seatnumber = seat.SeatNumber.ToString();
            string price = seat.Price.ToString();
            string start = route?.StartStop?.Name ?? "";
            string end = route?.EndStop?.Name ?? "";
            string busnumber = bus?.BusNumber ?? "";
            string company = bus?.Company ?? "";
            string departureTime = busTrip.DepartureTime.ToString("HH:mm");
            string departtureDate = busTrip.DepartureDate.ToString("dd/MM/yyyy");
            string pickuptime = busTrip.DepartureTime.AddMinutes(-30).ToString("HH:mm");

            return Json(new
            {
                start,
                end,
                busnumber,
                company,
                departureTime,
                departtureDate,
                seatnumber,
                price,
                pickuptime
            });
        }

        [HttpGet]
        public async Task<IActionResult> ViewMyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            var userBookings = await _bookingRepository.GetAllByUserIdAsync(user.Id);

            var sortedUserBookings = userBookings
                .Where(x =>
                    x.StatusBooking == StatusBooking.UnPaid ||
                    x.StatusBooking == StatusBooking.Paid ||
                    x.StatusBooking == StatusBooking.Cancelled)
                .OrderByDescending(x => x.Timebooking)
                .ToList();

            return View(sortedUserBookings);
        }

        private async Task SendBookingConfirmationEmailAsync(
            Booking booking,
            string subject,
            string paymentStatus)
        {
            var group = await GetBookingGroupAsync(booking);
            await SendBookingConfirmationEmailAsync(group, subject, paymentStatus);
        }

        private async Task SendBookingConfirmationEmailAsync(
            List<Booking> bookings,
            string subject,
            string paymentStatus)
        {
            try
            {
                var bookingList = bookings?
                    .Where(b => b != null)
                    .ToList() ?? new List<Booking>();

                if (!bookingList.Any())
                {
                    return;
                }

                var primaryBooking = bookingList.First();

                var user = await _userManager.FindByIdAsync(primaryBooking.UserId);
                var recipientEmail = !string.IsNullOrWhiteSpace(primaryBooking.Email) ? primaryBooking.Email : user?.Email;

                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    return;
                }

                var ticketDetails = new List<(Booking Booking, Seat? Seat)>();
                foreach (var item in bookingList)
                {
                    var itemSeat = item.Seat ?? await _seatRepository.GetByIdAsync(item.SeatId);
                    item.Seat = itemSeat;
                    ticketDetails.Add((item, itemSeat));
                }

                ticketDetails = ticketDetails
                    .OrderBy(x => x.Seat?.SeatNumber)
                    .ToList();

                var firstSeat = ticketDetails.Select(x => x.Seat).FirstOrDefault(x => x != null);

                BusTrip? busTrip = primaryBooking.BusTrip;
                Bus? bus = null;
                BusRoute? route = null;
                Stop? pickupStop = primaryBooking.PickupStop;
                Stop? dropOffStop = primaryBooking.DropOffStop;

                if (firstSeat != null)
                {
                    busTrip ??= await _busTripRepository.GetByIdAsync(firstSeat.BusTripId);
                }

                if (busTrip == null && primaryBooking.TripId.HasValue)
                {
                    busTrip = await _busTripRepository.GetByIdAsync(primaryBooking.TripId.Value);
                }

                if (busTrip != null)
                {
                    bus = await _busRepository.GetByIdAsync(busTrip.BusId);
                    route = await _busRouteRepository.GetByIdAsync(busTrip.BusRouteId);
                }

                if (pickupStop == null && primaryBooking.PickupStopId.HasValue)
                {
                    pickupStop = await _stopRepository.GetByIdAsync(primaryBooking.PickupStopId.Value);
                }

                if (dropOffStop == null && primaryBooking.DropOffStopId.HasValue)
                {
                    dropOffStop = await _stopRepository.GetByIdAsync(primaryBooking.DropOffStopId.Value);
                }

                var passengerName = user?.FullName;
                if (string.IsNullOrWhiteSpace(passengerName)) passengerName = primaryBooking.UserName;
                if (string.IsNullOrWhiteSpace(passengerName)) passengerName = user?.UserName;
                if (string.IsNullOrWhiteSpace(passengerName)) passengerName = "Quý khách";

                var startStop = route?.StartStop?.Name ?? pickupStop?.Name ?? "N/A";
                var endStop = route?.EndStop?.Name ?? dropOffStop?.Name ?? "N/A";
                var pickupName = startStop;
                var dropOffName = endStop;

                var seatNumbers = ticketDetails
                    .Select(x => x.Seat?.SeatNumber?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                var seatNumberText = seatNumbers.Any()
                    ? string.Join(", ", seatNumbers)
                    : "N/A";

                var busNumber = bus?.BusNumber ?? "N/A";
                var company = bus?.Company ?? "N/A";
                var busType = bus?.BusType?.ToString() ?? "Bus Seat";
                var bookingDateText = primaryBooking.Timebooking.ToString("dd/MM/yyyy HH:mm");
                var departureTimeText = busTrip?.DepartureTime.ToString("HH:mm") ?? "N/A";
                var departureDateText = busTrip?.DepartureDate.ToString("dd/MM/yyyy") ?? "N/A";
                var totalPriceText = $"{ticketDetails.Sum(x => x.Booking.TotalPrice):N0} VND";

                string H(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

                var ticketRowsBuilder = new StringBuilder();
                foreach (var item in ticketDetails)
                {
                    var itemSeatNumber = item.Seat?.SeatNumber?.ToString() ?? "N/A";
                    var itemPriceText = $"{item.Booking.TotalPrice:N0} VND";

                    ticketRowsBuilder.Append($@"
            <tr>
              <td style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">HUIT BUS Busline - {H(busType)}<br/><span style=""color:#6b7280;"">Ghế {H(itemSeatNumber)} · {H(departureDateText)} {H(departureTimeText)}</span></td>
              <td align=""center"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">1</td>
              <td align=""right"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">{H(itemPriceText)}</td>
              <td align=""right"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">{H(itemPriceText)}</td>
            </tr>");
                }

                var body = BuildTicketEmailHtml(
                    primaryBooking.InvoiceCode ?? "N/A",
                    bookingDateText,
                    passengerName,
                    primaryBooking.SDT ?? "N/A",
                    recipientEmail,
                    startStop,
                    endStop,
                    pickupName,
                    dropOffName,
                    departureTimeText,
                    departureDateText,
                    seatNumberText,
                    busNumber,
                    company,
                    busType,
                    totalPriceText,
                    paymentStatus,
                    ticketDetails.Count,
                    ticketRowsBuilder.ToString());

                await _emailSender.SendEmailAsync(recipientEmail, subject, body);
            }
            catch
            {
                // Email errors must not break booking or payment flow.
            }
        }

        private static string BuildTicketEmailHtml(
            string invoiceCode,
            string bookingDateText,
            string passengerName,
            string phoneNumber,
            string email,
            string fromStop,
            string toStop,
            string pickupStop,
            string dropOffStop,
            string departureTimeText,
            string departureDateText,
            string seatNumber,
            string busNumber,
            string company,
            string busType,
            string totalPriceText,
            string paymentStatus,
            int ticketCount,
            string ticketRowsHtml)
        {
            string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

            if (string.IsNullOrWhiteSpace(ticketRowsHtml))
            {
                ticketRowsHtml = $@"
            <tr>
              <td style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">HUIT BUS Busline - {E(busType)}<br/><span style=""color:#6b7280;"">Ghế {E(seatNumber)} · {E(departureDateText)} {E(departureTimeText)}</span></td>
              <td align=""center"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">1</td>
              <td align=""right"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">{E(totalPriceText)}</td>
              <td align=""right"" style=""padding:14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">{E(totalPriceText)}</td>
            </tr>";
            }

            var sb = new StringBuilder();
            sb.Append($@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Vé điện tử HUIT BUS</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f5fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;"">
  <div style=""max-width:760px;margin:0 auto;padding:24px 12px;"">
    <div style=""background:#ffffff;border:1px solid #d7def0;border-radius:14px;overflow:hidden;"">
      <div style=""background:#1016a8;padding:18px 18px 14px 18px;color:#ffffff;"">
        <div style=""font-size:26px;font-weight:700;line-height:1.2;margin-bottom:8px;"">Xác nhận đặt vé HUIT BUS</div>
        <div style=""font-size:20px;font-weight:700;margin-bottom:6px;"">Mã đơn hàng: {E(invoiceCode)}</div>
        <div style=""font-size:16px;font-weight:700;margin-bottom:6px;"">Số lượng vé trong đơn: {ticketCount}</div>
        <div style=""font-size:14px;opacity:0.92;"">Ngày đặt: {E(bookingDateText)}</div>
      </div>

      <div style=""padding:18px;"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;"">
          <tr>
            <td valign=""top"" style=""width:58%;padding-right:10px;"">
              <div style=""font-size:18px;font-weight:700;color:#111827;margin-bottom:10px;"">Thông tin khách hàng</div>
              <div style=""font-size:15px;line-height:1.8;"">
                <div><strong>Họ và tên:</strong> {E(passengerName)}</div>
                <div><strong>Số điện thoại:</strong> {E(phoneNumber)}</div>
                <div><strong>Email:</strong> {E(email)}</div>
              </div>
            </td>
            <td valign=""top"" style=""width:42%;padding-left:10px;text-align:right;"">
              <div style=""font-size:18px;font-weight:700;color:#111827;margin-bottom:10px;"">HUIT BUS Busline</div>
              <div style=""font-size:14px;line-height:1.8;color:#4b5563;"">
                <div>1900 6084</div>
                <div>HUIT sale.huitbus@Contact</div>
                <div>HUIT BUS.com</div>
              </div>
            </td>
          </tr>
        </table>

        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;margin-top:18px;"">
          <tr>
            <td valign=""top"" style=""width:50%;padding:10px 10px 10px 0;border-top:1px solid #e5e7eb;border-bottom:1px solid #e5e7eb;"">
              <div style=""font-size:14px;line-height:1.9;"">
                <div><strong>Điểm đi / From:</strong> {E(fromStop)}</div>
                <div><strong>Điểm đón:</strong> {E(pickupStop)}</div>
                <div><strong>Thông tin / Passenger Name:</strong> <strong>{E(passengerName)}</strong></div>
                <div><strong>Danh sách ghế / Seat no(s):</strong> <strong>{E(seatNumber)}</strong></div>
              </div>
            </td>
            <td valign=""top"" style=""width:50%;padding:10px 0 10px 10px;border-top:1px solid #e5e7eb;border-bottom:1px solid #e5e7eb;"">
              <div style=""font-size:14px;line-height:1.9;"">
                <div><strong>Điểm đến / To:</strong> {E(toStop)}</div>
                <div><strong>Điểm xuống:</strong> {E(dropOffStop)}</div>
                <div><strong>Giờ khởi hành / Department Time:</strong> {E(departureTimeText)}</div>
                <div><strong>Ngày / Department Date:</strong> {E(departureDateText)}</div>
                <div><strong>Biển số xe / License plates:</strong> {E(busNumber)}</div>
                <div><strong>Hãng / Company:</strong> {E(company)}</div>
              </div>
            </td>
          </tr>
        </table>

        <div style=""margin-top:18px;border:1px solid #e5e7eb;border-radius:10px;overflow:hidden;"">
          <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;"">
            <tr style=""background:#f7f8fc;color:#111827;"">
              <th align=""left"" style=""padding:12px 14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">Chi tiết</th>
              <th align=""center"" style=""padding:12px 14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">Số lượng</th>
              <th align=""right"" style=""padding:12px 14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">Giá</th>
              <th align=""right"" style=""padding:12px 14px;font-size:14px;border-bottom:1px solid #e5e7eb;"">Tổng</th>
            </tr>
{ticketRowsHtml}
          </table>

          <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border-collapse:collapse;"">
            <tr>
              <td style=""width:50%;padding:20px 14px;"">
                <div style=""display:inline-block;border:1px solid #d1d5db;border-radius:10px;padding:18px 28px;text-align:center;min-width:140px;"">
                  <div style=""font-size:14px;color:#6b7280;margin-bottom:6px;"">Seat No(s)</div>
                  <div style=""font-size:34px;font-weight:700;color:#10b981;letter-spacing:1px;"">{E(seatNumber)}</div>
                </div>
              </td>
              <td style=""width:50%;padding:20px 14px;text-align:right;"">
                <div style=""font-size:16px;color:#4b5563;margin-bottom:6px;"">Thành tiền / Total Amount</div>
                <div style=""font-size:14px;color:#6b7280;margin-bottom:6px;"">Số lượng vé: {ticketCount}</div>
                <div style=""font-size:28px;font-weight:800;color:#111827;"">{E(totalPriceText)}</div>
                <div style=""font-size:14px;color:#0f766e;margin-top:8px;"">{E(paymentStatus)}</div>
              </td>
            </tr>
          </table>
        </div>

        <div style=""margin-top:18px;font-size:14px;line-height:1.8;color:#1f2937;"">
          <div style=""font-weight:700;margin-bottom:6px;"">Lưu ý:</div>
          <div>Quý khách vui lòng có mặt tại bến xuất phát của xe trước ít nhất 30 phút giờ xe khởi hành, mang theo thông báo đặt vé thành công chứa mã vé được gửi từ hệ thống HUIT BUS.</div>
        </div>
      </div>
    </div>
  </div>
</body>
</html>");

            return sb.ToString();
        }
    }
}
