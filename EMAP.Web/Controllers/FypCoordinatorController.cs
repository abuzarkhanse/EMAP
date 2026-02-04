using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using EMAP.Web.Services.Email;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers
{
    [Authorize(Roles = "FYPCoordinator")]
    public class FypCoordinatorController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _email;

        public FypCoordinatorController(
            EmapDbContext db,
            UserManager<ApplicationUser> userManager,
            IEmailService email)
        {
            _db = db;
            _userManager = userManager;
            _email = email;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ================= DASHBOARD =================
        public async Task<IActionResult> Dashboard()
        {


            var approvedCount = await _db.FypChapterSubmissions
                .CountAsync(x =>
                    x.Status == ChapterSubmissionStatus.SupervisorApproved);

            return View();
        }

        // ✅ IMPORTANT: Some of your UI links are calling /FypCoordinator/SlotAllocation
        // but your actual listing action is Index().
        // So we provide an alias action to prevent 404.
        public IActionResult SlotAllocation()
        {
            return RedirectToAction(nameof(Index));
        }

        // ================= LIST ELIGIBLE PROPOSALS =================
        public async Task<IActionResult> Index()
        {
            var eligibleStatuses = new[]
            {
                ProposalStatus.ApprovedForDefense,
                ProposalStatus.ProposalAccepted
            };

            var proposals = await _db.ProposalSubmissions
                .Include(p => p.Group).ThenInclude(g => g.FypCall)
                .Include(p => p.Group.Supervisor)
                .Include(p => p.DefenseSchedule)
                .Where(p =>
                    p.DefenseSchedule == null &&
                    (p.Status == ProposalStatus.ApprovedForDefense
                    || p.Status == ProposalStatus.ProposalAccepted))

                .ToListAsync();

            return View(proposals);
        }

        // ================= ASSIGN SLOT (GET) =================
        public async Task<IActionResult> AssignSlot(int id)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group).ThenInclude(g => g.FypCall)
                .Include(p => p.Group.Supervisor)
                .Include(p => p.DefenseSchedule)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null)
                return NotFound();

            // ✅ Allow both statuses
            if (proposal.Status != ProposalStatus.ApprovedForDefense &&
                proposal.Status != ProposalStatus.ProposalAccepted)
            {
                TempData["Error"] = "Proposal is not approved for defense.";
                return RedirectToAction(nameof(Index));
            }

            if (proposal.DefenseSchedule != null)
            {
                TempData["Error"] = "This proposal already has a scheduled slot.";
                return RedirectToAction(nameof(Index));
            }

            var date = DateTime.Today;

            var takenSlots = await _db.ProposalDefenseSchedules
                .Where(s => s.DefenseDate == date.Date)
                .Select(s => s.DefenseTime)
                .ToListAsync();

            var slots = BuildSlots(
                start: new TimeSpan(9, 0, 0),
                end: new TimeSpan(17, 0, 0),
                durationMinutes: 10,
                taken: takenSlots
            );

            var vm = new AssignSlotViewModel
            {
                ProposalId = proposal.Id,
                Title = proposal.Title,
                Batch = proposal.Group.FypCall.Batch,
                SupervisorName = proposal.Group.Supervisor?.Name ?? "-",
                DefenseDate = date,
                AvailableSlots = slots,
                DurationMinutes = 10
            };

            return View(vm);
        }

        // ================= ASSIGN SLOT (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSlot(AssignSlotViewModel vm)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group).ThenInclude(g => g.FypCall)
                .Include(p => p.DefenseSchedule)
                .FirstOrDefaultAsync(p => p.Id == vm.ProposalId);

            if (proposal == null)
                return NotFound();

            // ✅ Allow both statuses (same as GET)
            if (proposal.DefenseSchedule != null ||
                (proposal.Status != ProposalStatus.ApprovedForDefense &&
                 proposal.Status != ProposalStatus.ProposalAccepted))
            {
                TempData["Error"] = "This proposal is not eligible for scheduling.";
                return RedirectToAction(nameof(Index));
            }

            if (!TimeSpan.TryParse(vm.SelectedSlot, out var slotTime))
            {
                TempData["Error"] = "Please select a valid slot.";
                return RedirectToAction(nameof(AssignSlot), new { id = vm.ProposalId });
            }

            bool taken = await _db.ProposalDefenseSchedules.AnyAsync(s =>
                s.DefenseDate == vm.DefenseDate.Date &&
                s.DefenseTime == slotTime);

            if (taken)
            {
                TempData["Error"] = "That slot was just taken. Please choose another.";
                return RedirectToAction(nameof(AssignSlot), new { id = vm.ProposalId });
            }

            var schedule = new ProposalDefenseSchedule
            {
                ProposalSubmissionId = proposal.Id,
                DefenseDate = vm.DefenseDate.Date,
                DefenseTime = slotTime,
                DurationMinutes = vm.DurationMinutes <= 0 ? 10 : vm.DurationMinutes,
                Venue = vm.Venue,
                Instructions = vm.Instructions,
                AssignedById = CurrentUserId
            }; 

            _db.ProposalDefenseSchedules.Add(schedule);

            //  mark status as DefenseScheduled
            proposal.Status = ProposalStatus.DefenseScheduled;

            //
            await _db.SaveChangesAsync();

            // ✅ Email notification (NEVER crash the request)
            try
            {
                await SendDefenseScheduledEmailToGroupAsync(proposal.Id, schedule.Id);
                TempData["Success"] = "Defense slot assigned and email notification sent to the group.";
            }
            catch
            {
                TempData["Success"] = "Defense slot assigned successfully.";
                TempData["Error"] = "Slot assigned but email sending failed. Check SMTP settings in appsettings.json.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task SendDefenseScheduledEmailToGroupAsync(int proposalId, int scheduleId)
        {
            // Reload fresh from DB to avoid lazy/null issues
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group).ThenInclude(g => g.FypCall)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            var schedule = await _db.ProposalDefenseSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (proposal?.Group == null || schedule == null)
                return;

            var group = proposal.Group;

            var ids = new List<string?> { group.LeaderId, group.Member2Id, group.Member3Id }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var emails = new List<string>();

            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id!);
                if (!string.IsNullOrWhiteSpace(user?.Email))
                    emails.Add(user!.Email!);
            }

            emails = emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (emails.Count == 0) return;

            // Auto-open modal
            var portalUrl = Url.Action("Index", "Fyp", new { showDefense = 1 }, Request.Scheme) ?? "";

            var venue = string.IsNullOrWhiteSpace(schedule.Venue) ? "To be announced" : schedule.Venue;
            var instructions = string.IsNullOrWhiteSpace(schedule.Instructions)
                ? "No additional instructions."
                : schedule.Instructions;

            // ✅ THIS IS THE BUG FIX: TimeSpan formatting must use ToString(@"hh\:mm")
            var timeText = schedule.DefenseTime.ToString(@"hh\:mm");

            var enc = HtmlEncoder.Default;
            var safeInstructions = enc.Encode(instructions).Replace("\n", "<br/>");

            var subject = "FYP Proposal Defense Slot Scheduled";

            var body =
                $@"
                <div style='font-family:Arial,sans-serif'>
                    <h2>Proposal Defense Scheduled</h2>
                    <p>Your proposal defense slot has been assigned by the FYP Coordinator.</p>

                    <ul>
                        <li><b>Project Title:</b> {enc.Encode(proposal.Title ?? "FYP Proposal")}</li>
                        <li><b>Batch:</b> {enc.Encode(group.FypCall?.Batch ?? "-")}</li>
                        <li><b>Date:</b> {schedule.DefenseDate:dddd, dd MMM yyyy}</li>
                        <li><b>Time:</b> {enc.Encode(timeText)}</li>
                        <li><b>Duration:</b> {schedule.DurationMinutes} minutes</li>
                        <li><b>Venue:</b> {enc.Encode(venue)}</li>
                    </ul>

                    <h3>Instructions</h3>
                    <div style='padding:12px;border:1px solid #ddd;background:#f8f9fa;border-radius:6px'>
                        {safeInstructions}
                    </div>

                    <p style='margin-top:16px'>
                        Open your portal to view details:
                        <a href='{portalUrl}'>{portalUrl}</a>
                    </p>
                </div>";

            await _email.SendAsync(emails, subject, body);
        }

        // ================= SLOT BUILDER =================
        private static List<SelectListItem> BuildSlots(
            TimeSpan start,
            TimeSpan end,
            int durationMinutes,
            List<TimeSpan> taken)
        {
            var list = new List<SelectListItem>();
            var current = start;

            while (current.Add(TimeSpan.FromMinutes(durationMinutes)) <= end)
            {
                list.Add(new SelectListItem
                {
                    Text = current.ToString(@"hh\:mm"),
                    Value = current.ToString(@"hh\:mm"),
                    Disabled = taken.Contains(current)
                });

                current = current.Add(TimeSpan.FromMinutes(durationMinutes));
            }

            return list;
        }

        // ===================== CHAPTER MANAGEMENT (COORDINATOR) =====================

        public async Task<IActionResult> Chapters()
        {
            var activeCall = await _db.FypCalls
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.AnnouncementDate)
                .FirstOrDefaultAsync();

            if (activeCall == null)
            {
                TempData["Error"] = "No active FYP call found. Please activate a call first.";
                return RedirectToAction(nameof(Dashboard));
            }

            await EnsureChapterRowsAsync(activeCall.Id);

            var list = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == activeCall.Id)
                .OrderBy(x => x.ChapterType)
                .ToListAsync();

            var vm = new CoordinatorChaptersVm
            {
                ActiveCallId = activeCall.Id,
                ActiveCallTitle = activeCall.Title,
                ActiveCallBatch = activeCall.Batch,
                Items = list
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditChapter(int id)
        {
            var item = await _db.FypChapterAnnouncements.FindAsync(id);
            if (item == null) return NotFound();

            var vm = new EditChapterVm
            {
                Id = item.Id,
                ChapterType = item.ChapterType,
                IsOpen = item.IsOpen,
                Deadline = item.Deadline,
                Instructions = item.Instructions
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(EditChapterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var item = await _db.FypChapterAnnouncements.FindAsync(vm.Id);
            if (item == null) return NotFound();

            item.Deadline = vm.Deadline;
            item.Instructions = vm.Instructions;

            if (vm.IsOpen)
            {
                var others = await _db.FypChapterAnnouncements
                    .Where(x => x.FypCallId == item.FypCallId && x.Id != item.Id && x.IsOpen)
                    .ToListAsync();

                foreach (var o in others)
                    o.IsOpen = false;

                item.IsOpen = true;
            }
            else
            {
                item.IsOpen = false;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Chapter updated successfully.";
            return RedirectToAction(nameof(Chapters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenChapter(int id)
        {
            var item = await _db.FypChapterAnnouncements.FindAsync(id);
            if (item == null) return NotFound();

            var others = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == item.FypCallId && x.Id != item.Id && x.IsOpen)
                .ToListAsync();

            foreach (var o in others)
                o.IsOpen = false;

            item.IsOpen = true;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Chapter opened. Students can now submit.";
            return RedirectToAction(nameof(Chapters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseChapter(int id)
        {
            var item = await _db.FypChapterAnnouncements.FindAsync(id);
            if (item == null) return NotFound();

            item.IsOpen = false;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Chapter closed.";
            return RedirectToAction(nameof(Chapters));
        }

        private async Task EnsureChapterRowsAsync(int fypCallId)
        {
            var existing = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == fypCallId)
                .ToListAsync();

            if (existing.Any()) return;

            _db.FypChapterAnnouncements.AddRange(
                new FypChapterAnnouncement { FypCallId = fypCallId, ChapterType = FypChapterType.VisionAndScope },
                new FypChapterAnnouncement { FypCallId = fypCallId, ChapterType = FypChapterType.Srs },
                new FypChapterAnnouncement { FypCallId = fypCallId, ChapterType = FypChapterType.SystemOverview }
            );

            await _db.SaveChangesAsync();
        }

        // ===================== CHAPTER APPROVALS (FINAL) =====================

        public async Task<IActionResult> ChapterApprovals()
        {
            var chapters = await _db.FypChapterSubmissions
                .Include(x => x.Group)
                .Include(x => x.ChapterAnnouncement)
                .Where(x => x.Status == ChapterSubmissionStatus.SupervisorApproved)
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            return View(chapters);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeChapter(
            int submissionId,
            bool accept,
            string? feedback)
        {
            var submission = await _db.FypChapterSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                TempData["Error"] = "Chapter submission not found.";
                return RedirectToAction(nameof(ChapterApprovals));
            }

            // ================= FINAL APPROVAL =================
            if (accept)
            {
                submission.Status = ChapterSubmissionStatus.CoordinatorApproved;
                submission.Feedback = null;

                TempData["Success"] =
                    "Chapter finalized successfully. Students may proceed to the next chapter when opened.";
            }
            // ================= REQUEST CHANGES =================
            else
            {
                if (string.IsNullOrWhiteSpace(feedback))
                {
                    TempData["Error"] =
                        "Please provide feedback before requesting changes.";
                    return RedirectToAction(nameof(ChapterApprovals));
                }

                submission.Status = ChapterSubmissionStatus.ChangesRequested;
                submission.Feedback = feedback;

                TempData["Success"] =
                    "Feedback sent to student. Chapter returned for revision.";
            }

            submission.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ChapterApprovals));
        }
    }

    public class CoordinatorChaptersVm
    {
        public int ActiveCallId { get; set; }
        public string? ActiveCallTitle { get; set; }
        public string? ActiveCallBatch { get; set; }
        public List<FypChapterAnnouncement> Items { get; set; } = new();
    }

    public class EditChapterVm
    {
        public int Id { get; set; }
        public FypChapterType ChapterType { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Instructions { get; set; }
    }
}
