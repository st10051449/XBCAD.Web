using AntonieMotors_XBCAD7319.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CarEngine()
        {
            return View();
        }

        public IActionResult CarPanelbeating()
        {
            return View();
        }

        public IActionResult CarUpholstery()
        {
            return View();
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
