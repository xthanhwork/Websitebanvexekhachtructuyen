
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DOANCOSO26.Areas.Driver.Controllers
{
    [Area("Driver")]
    [Authorize(Roles = "Driver")]
    public class TDCController : Controller
    {
        
        public IActionResult Index()
        {
            return RedirectToAction("ViewMySchedule", "Trip", new { area = "" });
        }
    }
}

