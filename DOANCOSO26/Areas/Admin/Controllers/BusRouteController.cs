using DOANCOSO26.Models;
using DOANCOSO26.Repository;
using DOANCOSO26.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DOANCOSO26.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BusRouteController : Controller
    {
        private readonly GoogleMapService _mapService;
        private readonly IBusTripRepository _bustripRepository;
        private readonly IBusRouteRepository _busRouteRepository;
        private readonly IStopRepository _stopRepository;
        private readonly IBookingRepository _bookingRepository;
        //public BusRouteController(IBusTripRepository bustripRepository, IStopRepository stopRepository, IBusRouteRepository busRouteRepository, IBookingRepository bookingRepository)
        //{
        //    _bustripRepository = bustripRepository;
        //    _stopRepository = stopRepository;
        //    _busRouteRepository = busRouteRepository;
        //    _bookingRepository = bookingRepository;
        //}
        public BusRouteController(
            IBusTripRepository bustripRepository,
            IStopRepository stopRepository,
            IBusRouteRepository busRouteRepository,
            IBookingRepository bookingRepository,
            GoogleMapService mapService)
        {
            _bustripRepository = bustripRepository;
            _stopRepository = stopRepository;
            _busRouteRepository = busRouteRepository;
            _bookingRepository = bookingRepository;
            _mapService = mapService;
        }
        public async Task<IActionResult> Index()
        {
            var route = await _busRouteRepository.GetAllAsync();
            return View(route);
        }
        // Hiển thị form thêm sản phẩm mới
        public async Task<IActionResult> Add()
        {
            //var route = await _busRouteRepository.GetAllAsync();
            //return View();
            ViewBag.StartStopId = new SelectList(
                  await _stopRepository.GetAllAsync(),
                  "Id",
                  "DisplayName"
              );

            ViewBag.EndStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName"
            );

            return View();
        }

        // Xử lý thêm loại mới

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Add(BusRoute route , IFormFile imageUrl, List<IFormFile> images)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (imageUrl != null)
        //        {
        //            // Lưu hình ảnh đại diện tham khảo bài 02 hàm SaveImage
        //            route.ImageUrl = await SaveImage(imageUrl);
        //        }
        //        if (images != null)
        //        {
        //            route.Images = new List<RouteImage>();
        //            foreach (var item in images)
        //            {
        //                RouteImage image = new RouteImage()
        //                {
        //                    BusRouteId = route.Id,
        //                    Url = await SaveImage(item)
        //                };
        //                route.Images.Add(image);
        //            }
        //        }
        //        await _busRouteRepository.AddAsync(route);
        //        return RedirectToAction(nameof(Index));
        //    }

        //    // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
        //    return View(route);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
        BusRoute route,
        IFormFile imageUrl,
        List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    route.ImageUrl = await SaveImage(imageUrl);
                }

                if (images != null)
                {
                    route.Images = new List<RouteImage>();

                    foreach (var item in images)
                    {
                        RouteImage image = new RouteImage()
                        {
                            BusRouteId = route.Id,
                            Url = await SaveImage(item)
                        };

                        route.Images.Add(image);
                    }
                }

                await _busRouteRepository.AddAsync(route);

                return RedirectToAction(nameof(Index));
            }

            // LOAD LẠI DROPDOWN
            ViewBag.StartStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                route.StartStopId
            );

            ViewBag.EndStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                route.EndStopId
            );

            // Nếu ModelState không hợp lệ
            return View(route);
        }
        private async Task<string> SaveImage(IFormFile imageUrl)
        {
            var savePath = Path.Combine("wwwroot/images", imageUrl.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await imageUrl.CopyToAsync(fileStream);
            }
            return "/images/" + imageUrl.FileName;
        }

        // Hiển thị thông tin chi tiết loại
        public async Task<IActionResult> Display(int id)
        {
            var BusRoute = await _busRouteRepository.GetByIdAsync(id);
            if (BusRoute == null)
            {
                return NotFound();
            }
            return View(BusRoute);
        }

        public async Task<IActionResult> Update(int id)
        {
            //var BusRoute = await _busRouteRepository.GetByIdAsync(id);
            //if (BusRoute == null)
            //{
            //    return NotFound();
            //}
            //return View(BusRoute);
            var BusRoute = await _busRouteRepository.GetByIdAsync(id);

            if (BusRoute == null)
            {
                return NotFound();
            }

            ViewBag.StartStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                BusRoute.StartStopId
            );

            ViewBag.EndStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                BusRoute.EndStopId
            );

            return View(BusRoute);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, BusRoute busroute, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != busroute.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingroute = await _busRouteRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync

                if (imageUrl == null)
                {
                    busroute.ImageUrl = existingroute.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    busroute.ImageUrl = await SaveImage(imageUrl);
                }
                // Giữ nguyên thông tin hình ảnh nếu không có hình mới được tải lên
                existingroute.StartStopId = busroute.StartStopId;
                existingroute.EndStopId = busroute.EndStopId;

                existingroute.Id = busroute.Id;
                existingroute.Distance = busroute.Distance;
                existingroute.Time = busroute.Time;
                existingroute.Price = busroute.Price;
                existingroute.ImageUrl = busroute.ImageUrl;




                await _busRouteRepository.UpdateAsync(existingroute);
                return RedirectToAction(nameof(Index));
            }
                ViewBag.StartStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                busroute.StartStopId
            );

                ViewBag.EndStopId = new SelectList(
                await _stopRepository.GetAllAsync(),
                "Id",
                "DisplayName",
                 busroute.EndStopId
                    );
            return View(busroute);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var BusRoute = await _busRouteRepository.GetByIdAsync(id);
            if (BusRoute == null)
            {
                return NotFound();
            }
            return View(BusRoute);
        }


        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _busRouteRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> GetDistance(int startId, int endId)
        {
            try
            {
                var stops = await _stopRepository.GetAllAsync();

                var start = stops.FirstOrDefault(x => x.Id == startId);
                var end = stops.FirstOrDefault(x => x.Id == endId);

                if (start == null || end == null)
                    return Json(new { distanceKm = 0, durationMin = 0 });

                if (start.Latitude == null || start.Longitude == null ||
                    end.Latitude == null || end.Longitude == null)
                {
                    return Json(new { distanceKm = 0, durationMin = 0 });
                }

                var result = await _mapService.GetDistanceAsync(
                    double.Parse(start.Latitude),
                    double.Parse(start.Longitude),
                    double.Parse(end.Latitude),
                    double.Parse(end.Longitude)
                );

                return Json(new
                {
                    distanceKm = result.distanceKm,
                    durationMin = result.durationMin
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    distanceKm = 0,
                    durationMin = 0,
                    error = ex.Message
                });
            }
        }
    }
}
