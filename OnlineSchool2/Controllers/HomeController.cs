using Microsoft.AspNetCore.Mvc;
using OnlineSchool2.Models;
using System.Diagnostics;

namespace OnlineSchool2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private SchoolContext db;

        public HomeController(ILogger<HomeController> logger, SchoolContext ctx)
        {
            _logger = logger;
            db = ctx;
        }

        public IActionResult Index()
        {
            return View(db.Courses.OrderByDescending(c => c.CreatedDate).Take(8).ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
