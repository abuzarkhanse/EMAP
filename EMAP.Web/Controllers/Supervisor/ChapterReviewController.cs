using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EMAP.Web.Controllers.Supervisor
{
    [Authorize(Roles = "Supervisor")]
    public class ChapterReviewController : Controller
    {
        private readonly EmapDbContext _db;

        public ChapterReviewController(EmapDbContext db)
        {
            _db = db;
        }

        // ================= LIST =================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = await _db.FypChapterSubmissions
                .Include(x => x.Group)
                .Include(x => x.ChapterAnnouncement)
                .Where(x =>
                    x.SupervisorId == userId &&
                    (x.Status == ChapterSubmissionStatus.Submitted ||
                     x.Status == ChapterSubmissionStatus.Resubmitted))
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return View(items);
        }

        // ================= REVIEW =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(
            int id,
            ChapterSubmissionStatus status,
            string? feedback)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sub = await _db.FypChapterSubmissions
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.SupervisorId == userId);

            if (sub == null)
                return NotFound();

            if (status == ChapterSubmissionStatus.SupervisorApproved)
            {
                sub.Status = ChapterSubmissionStatus.SupervisorApproved;
            }
            else if (status == ChapterSubmissionStatus.ChangesRequested)
            {
                sub.Status = ChapterSubmissionStatus.ChangesRequested;
                sub.Feedback = feedback;
            }

            sub.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}