using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                Title = "FYP Call",
                AnnouncementDate = DateTime.Today,
                ProposalDeadline = DateTime.Today.AddDays(7),
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FypCall model)
        {
            model.Title = "FYP Call";

            model.AnnouncementDate = model.AnnouncementDate.Date;
            model.ProposalDeadline = model.ProposalDeadline.Date;
            var today = DateTime.Today;

            if (string.IsNullOrWhiteSpace(model.Batch))
            {
                ModelState.AddModelError("Batch", "Batch is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Session))
            {
                ModelState.AddModelError("Session", "Session is required.");
            }

            if (model.AnnouncementDate < today)
            {
                ModelState.AddModelError("AnnouncementDate", "Announcement date cannot be in the past.");
            }

            if (model.ProposalDeadline <= model.AnnouncementDate)
            {
                ModelState.AddModelError("ProposalDeadline", "Proposal deadline must be after announcement date.");
            }

            bool exists = _db.FypCalls.Any(x =>
                x.Batch == model.Batch &&
                x.Session == model.Session &&
                x.IsActive);

            if (exists)
            {
                ModelState.AddModelError("", "An active FYP call already exists for this batch and session.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _db.FypCalls.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "FYP Call created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var call = await _db.FypCalls.FindAsync(id);
            if (call == null)
                return NotFound();

            return View(call);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FypCall model)
        {
            if (id != model.Id)
                return NotFound();

            var existingCall = await _db.FypCalls.FindAsync(id);
            if (existingCall == null)
                return NotFound();

            model.Title = "FYP Call";
            model.AnnouncementDate = model.AnnouncementDate.Date;
            model.ProposalDeadline = model.ProposalDeadline.Date;

            var today = DateTime.Today;

            if (string.IsNullOrWhiteSpace(model.Batch))
            {
                ModelState.AddModelError("Batch", "Batch is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Session))
            {
                ModelState.AddModelError("Session", "Session is required.");
            }

            if (model.AnnouncementDate < today)
            {
                ModelState.AddModelError("AnnouncementDate", "Announcement date cannot be in the past.");
            }

            if (model.ProposalDeadline <= model.AnnouncementDate)
            {
                ModelState.AddModelError("ProposalDeadline", "Proposal deadline must be after announcement date.");
            }

            bool exists = _db.FypCalls.Any(x =>
                x.Id != model.Id &&
                x.Batch == model.Batch &&
                x.Session == model.Session &&
                x.IsActive);

            if (exists)
            {
                ModelState.AddModelError("", "Another active FYP call already exists for this batch and session.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            existingCall.Batch = model.Batch;
            existingCall.Session = model.Session;
            existingCall.Title = "FYP Call";
            existingCall.AnnouncementDate = model.AnnouncementDate;
            existingCall.ProposalDeadline = model.ProposalDeadline;
            existingCall.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["Success"] = "FYP Call updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var call = await _db.FypCalls.FirstOrDefaultAsync(x => x.Id == id);
            if (call == null)
                return NotFound();

            return View(call);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var call = await _db.FypCalls.FindAsync(id);
            if (call == null)
                return NotFound();

            _db.FypCalls.Remove(call);
            await _db.SaveChangesAsync();

            TempData["Success"] = "FYP Call deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}