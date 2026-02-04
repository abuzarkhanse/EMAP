using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMAP.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Keep it simple (not used as landing anymore)
            return View();
        }

        // ✅ This matches Views/Home/StudentDashboard.cshtml
        [Authorize(Roles = "Student")]
        public IActionResult StudentDashboard()
        {
            return View();
        }
    }
}
