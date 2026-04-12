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
                    .ThenInclude(g => g.FypCall)
                .Include(x => x.Group)
                    .ThenInclude(g => g.Supervisor)
                .Include(x => x.ChapterAnnouncement)
                .Where(x =>
                    x.SupervisorId == userId &&
                    (x.Status == ChapterSubmissionStatus.Submitted ||
                     x.Status == ChapterSubmissionStatus.Resubmitted))
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return View("~/Views/Supervisor/ChapterReviews.cshtml", items);
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
                sub.Feedback = null;
            }
            else if (status == ChapterSubmissionStatus.ChangesRequested)
            {
                if (string.IsNullOrWhiteSpace(feedback))
                {
                    TempData["Error"] = "Please provide feedback before requesting changes.";
                    return RedirectToAction(nameof(Index));
                }

                sub.Status = ChapterSubmissionStatus.ChangesRequested;
                sub.Feedback = feedback.Trim();
            }

            sub.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = status == ChapterSubmissionStatus.SupervisorApproved
                ? "Chapter approved successfully."
                : "Feedback sent and changes requested successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
