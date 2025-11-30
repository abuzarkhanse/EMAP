using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EMAP.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EMAP.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StudentDashboard()
        {
            return View();   // this will look for Views/Home/StudentDashboard.cshtml
        }
    }
}

