using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class FypCallController : Controller
    {
        private readonly EmapDbContext _db;

        public FypCallController(EmapDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var calls = _db.FypCalls.ToList();
            return View(calls);
        }

        public IActionResult Create()
        {
            return View(new FypCall
            {
                AnnouncementDate = DateTime.Today,
                ProposalDeadline = DateTime.Today.AddDays(7),
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FypCall model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.FypCalls.Add(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
