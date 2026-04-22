using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using EMAP.Web.Services.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class FypFinalRecordAdminController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly IFypFinalRecordService _finalRecordService;

        public FypFinalRecordAdminController(
            EmapDbContext db,
            IFypFinalRecordService finalRecordService)
        {
            _db = db;
            _finalRecordService = finalRecordService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var items = await _db.FypFinalRecords
                .Where(x => !x.IsArchived)
                .OrderByDescending(x => x.SubmittedToAdminAt)
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.FypFinalRecords
                .Include(x => x.Students)
                .Include(x => x.Chapters)
                .Include(x => x.Evaluations)
                    .ThenInclude(e => e.Members)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkProcessed(int id, string? adminRemarks)
        {
            var result = await _finalRecordService.MarkProcessedAsync(id, CurrentUserId, adminRemarks);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var result = await _finalRecordService.ArchiveAsync(id, CurrentUserId);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ArchiveList(string? q)
        {
            var query = _db.FypFinalRecords
                .Include(x => x.Students)
                .Where(x => x.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();

                query = query.Where(x =>
                    x.ProjectTitle.ToLower().Contains(q) ||
                    x.ProgramCode.ToLower().Contains(q) ||
                    x.Batch.ToLower().Contains(q) ||
                    x.SupervisorName.ToLower().Contains(q) ||
                    x.StudentGroupId.ToString().Contains(q) ||
                    x.Students.Any(s =>
                        s.StudentName.ToLower().Contains(q) ||
                        s.RegistrationNo.ToLower().Contains(q) ||
                        s.Email.ToLower().Contains(q)
                    ));
            }

            var items = await query
                .OrderByDescending(x => x.ArchivedAt)
                .ToListAsync();

            ViewBag.SearchText = q;
            return View(items);
        }
    }
}
