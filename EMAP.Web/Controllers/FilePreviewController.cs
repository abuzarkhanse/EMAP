using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers
{
    [Authorize]
    public class FilePreviewController : Controller
    {
        private readonly EmapDbContext _db;

        public FilePreviewController(EmapDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Proposal(int id)
        {
            var item = await _db.ProposalSubmissions
                .Include(x => x.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(x => x.Group)
                    .ThenInclude(g => g.Supervisor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null || string.IsNullOrWhiteSpace(item.FilePath))
                return NotFound();

            var vm = new FilePreviewVm
            {
                Title = item.Title,
                FilePath = item.FilePath,
                FileTypeLabel = "Proposal File",
                Subtitle = $"Group #{item.GroupId} · {item.Group?.TentativeProjectTitle ?? "-"}",
                Meta1Label = "Batch",
                Meta1Value = item.Group?.FypCall?.Batch ?? "-",
                Meta2Label = "Program",
                Meta2Value = item.Group?.ProgramCode ?? "-",
                Meta3Label = "Supervisor",
                Meta3Value = item.Group?.Supervisor?.Name ?? "-"
            };

            return View("Preview", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Chapter(int id)
        {
            var item = await _db.FypChapterSubmissions
                .Include(x => x.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(x => x.Group)
                    .ThenInclude(g => g.Supervisor)
                .Include(x => x.ChapterAnnouncement)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null || string.IsNullOrWhiteSpace(item.FilePath))
                return NotFound();

            var vm = new FilePreviewVm
            {
                Title = item.Title,
                FilePath = item.FilePath,
                FileTypeLabel = "Chapter Submission",
                Subtitle = $"Group #{item.GroupId} · {item.Group?.TentativeProjectTitle ?? "-"}",
                Meta1Label = "Chapter",
                Meta1Value = item.ChapterAnnouncement?.ChapterType.ToString() ?? "-",
                Meta2Label = "Batch",
                Meta2Value = item.Group?.FypCall?.Batch ?? "-",
                Meta3Label = "Supervisor",
                Meta3Value = item.Group?.Supervisor?.Name ?? "-"
            };

            return View("Preview", vm);
        }
    }

    public class FilePreviewVm
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileTypeLabel { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        public string Meta1Label { get; set; } = string.Empty;
        public string Meta1Value { get; set; } = string.Empty;

        public string Meta2Label { get; set; } = string.Empty;
        public string Meta2Value { get; set; } = string.Empty;

        public string Meta3Label { get; set; } = string.Empty;
        public string Meta3Value { get; set; } = string.Empty;

        public bool IsPdf =>
            !string.IsNullOrWhiteSpace(FilePath) &&
            FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

        public bool IsImage =>
            !string.IsNullOrWhiteSpace(FilePath) &&
            (FilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
             FilePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
             FilePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
             FilePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase));
    }
}
