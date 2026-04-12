using DocumentFormat.OpenXml.InkML;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

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
            return RedirectToAction(nameof(PendingProposalSlots));
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

            // Allow both statuses
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

            var leader = await _userManager.FindByIdAsync(proposal.Group.LeaderId);

            var member2 = !string.IsNullOrWhiteSpace(proposal.Group.Member2Id)
                ? await _userManager.FindByIdAsync(proposal.Group.Member2Id)
                : null;

            var member3 = !string.IsNullOrWhiteSpace(proposal.Group.Member3Id)
                ? await _userManager.FindByIdAsync(proposal.Group.Member3Id)
                : null;

            var vm = new AssignSlotViewModel
            {
                ProposalId = proposal.Id,
                Title = proposal.Title,
                Batch = proposal.Group.FypCall.Batch,
                SupervisorName = proposal.Group.Supervisor?.Name ?? "-",

                GroupLeaderName = leader?.FullName ?? leader?.UserName ?? leader?.Email ?? "-",
                Member2Name = member2?.FullName ?? member2?.UserName ?? member2?.Email,
                Member3Name = member3?.FullName ?? member3?.UserName ?? member3?.Email,

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
                TempData["Error"] = "Slot assigned but email sending failed.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;
        }

        public async Task<IActionResult> PendingProposalSlots()
        {
            var currentEmail = GetCurrentUserEmail();

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x =>
                    x.CoordinatorEmail.ToLower() == currentEmail &&
                    x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new List<ProposalSubmission>());
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var proposals = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor)
                .Include(p => p.DefenseSchedule)
                .Where(p =>
                    p.Group != null &&
                    !string.IsNullOrWhiteSpace(p.Group.ProgramCode) &&
                    programCodes.Contains(p.Group.ProgramCode.ToUpper()) &&
                    p.DefenseSchedule == null &&
                    (p.Status == ProposalStatus.ApprovedForDefense ||
                     p.Status == ProposalStatus.ProposalAccepted))
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();

            ViewBag.CommitteeName = committee.Name;
            return View(proposals);
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

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // ===================== Bulk Slot Allocation =====================

        [HttpGet]
        public IActionResult BulkAssignSlots(List<int> proposalIds)
        {
            if (proposalIds == null || !proposalIds.Any())
            {
                TempData["Error"] = "Please select at least one proposal.";
                return RedirectToAction(nameof(PendingProposalSlots));
            }

            var vm = new BulkAssignProposalSlotsViewModel
            {
                ProposalIds = proposalIds,
                DefenseDate = DateTime.Today.AddDays(1)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssignSlots(BulkAssignProposalSlotsViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var currentEmail = GetCurrentUserEmail();

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x =>
                    x.CoordinatorEmail.ToLower() == currentEmail &&
                    x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return RedirectToAction(nameof(PendingProposalSlots));
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var proposals = await _db.ProposalSubmissions
                .Include(p => p.Group)
                .Include(p => p.DefenseSchedule)
                .Where(p => vm.ProposalIds.Contains(p.Id))
                .Where(p => p.Group != null &&
                            !string.IsNullOrWhiteSpace(p.Group.ProgramCode) &&
                            programCodes.Contains(p.Group.ProgramCode.ToUpper()))
                .Where(p => p.DefenseSchedule == null)
                .Where(p => p.Status == ProposalStatus.ApprovedForDefense ||
                            p.Status == ProposalStatus.ProposalAccepted)
                .OrderBy(p => p.Id)
                .ToListAsync();

            if (!proposals.Any())
            {
                TempData["Error"] = "No valid proposals found for slot assignment.";
                return RedirectToAction(nameof(PendingProposalSlots));
            }

            var currentTime = vm.StartTime;
            var counter = 0;

            foreach (var proposal in proposals)
            {
                bool taken = await _db.ProposalDefenseSchedules.AnyAsync(s =>
                    s.DefenseDate == vm.DefenseDate.Date &&
                    s.DefenseTime == currentTime);

                if (taken)
                {
                    TempData["Error"] = $"The slot {currentTime:hh\\:mm} on {vm.DefenseDate:dd-MMM-yyyy} is already taken.";
                    return RedirectToAction(nameof(PendingProposalSlots));
                }

                var schedule = new ProposalDefenseSchedule
                {
                    ProposalSubmissionId = proposal.Id,
                    DefenseDate = vm.DefenseDate.Date,
                    DefenseTime = currentTime,
                    DurationMinutes = vm.SlotDurationMinutes <= 0 ? 10 : vm.SlotDurationMinutes,
                    Venue = vm.Venue,
                    Instructions = vm.Instructions,
                    AssignedById = CurrentUserId
                };

                _db.ProposalDefenseSchedules.Add(schedule);
                proposal.Status = ProposalStatus.DefenseScheduled;

                counter++;
                currentTime = currentTime.Add(TimeSpan.FromMinutes(vm.SlotDurationMinutes));

                if (vm.BreakAfterEvery > 0 &&
                    vm.BreakMinutes > 0 &&
                    counter % vm.BreakAfterEvery == 0)
                {
                    currentTime = currentTime.Add(TimeSpan.FromMinutes(vm.BreakMinutes));
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{proposals.Count} proposal defense slots assigned successfully.";
            return RedirectToAction(nameof(PendingProposalSlots));
        }

        // ===================== CHAPTER MANAGEMENT (COORDINATOR) =====================

        public async Task<IActionResult> Chapters(FypStage stage = FypStage.Fyp1)
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
                .Where(x => x.FypCallId == activeCall.Id && x.Stage == stage)
                .OrderBy(x => x.ChapterType)
                .ToListAsync();

            var vm = new CoordinatorChaptersVm
            {
                ActiveCallId = activeCall.Id,
                ActiveCallTitle = activeCall.Title,
                ActiveCallBatch = activeCall.Batch,
                SelectedStage = stage,
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
                Stage = item.Stage,
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
                    .Where(x => x.FypCallId == item.FypCallId &&
                                x.Stage == item.Stage &&
                                x.Id != item.Id &&
                                x.IsOpen)
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
            return RedirectToAction(nameof(Chapters), new { stage = item.Stage });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenChapter(int id)
        {
            var item = await _db.FypChapterAnnouncements.FindAsync(id);
            if (item == null) return NotFound();

            var others = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == item.FypCallId &&
                            x.Stage == item.Stage &&
                            x.Id != item.Id &&
                            x.IsOpen)
                .ToListAsync();

            foreach (var o in others)
                o.IsOpen = false;

            item.IsOpen = true;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Chapter opened. Students can now submit.";
            return RedirectToAction(nameof(Chapters), new { stage = item.Stage });
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
            return RedirectToAction(nameof(Chapters), new { stage = item.Stage });
        }

        private async Task EnsureChapterRowsAsync(int fypCallId)
        {
            var existing = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == fypCallId)
                .ToListAsync();

            var required = new List<(FypStage Stage, FypChapterType ChapterType)>
    {
        // FYP-1
        (FypStage.Fyp1, FypChapterType.VisionAndScope),
        (FypStage.Fyp1, FypChapterType.Srs),
        (FypStage.Fyp1, FypChapterType.SystemOverview),

        // FYP-2
        (FypStage.Fyp2, FypChapterType.SystemImplementation),
        (FypStage.Fyp2, FypChapterType.SystemTestingAndDevelopment),
        (FypStage.Fyp2, FypChapterType.ResultsAndDiscussion)
    };

            foreach (var item in required)
            {
                var exists = existing.Any(x =>
                    x.FypCallId == fypCallId &&
                    x.Stage == item.Stage &&
                    x.ChapterType == item.ChapterType);

                if (!exists)
                {
                    _db.FypChapterAnnouncements.Add(new FypChapterAnnouncement
                    {
                        FypCallId = fypCallId,
                        Stage = item.Stage,
                        ChapterType = item.ChapterType,
                        IsOpen = false
                    });
                }
            }

            await _db.SaveChangesAsync();
        }

        // ===================== CHAPTER APPROVALS (FINAL) =====================

        public async Task<IActionResult> ChapterApprovals()
        {
            var chapters = await _db.FypChapterSubmissions
                .Include(x => x.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(x => x.Group)
                    .ThenInclude(g => g.Supervisor)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeFyp1(int groupId)
        {
            var group = await _db.StudentGroups
                .Include(g => g.FypCall)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                TempData["Error"] = "Student group not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (group.CurrentStage != FypStage.Fyp1)
            {
                TempData["Error"] = "This group is not currently in FYP-1.";
                return RedirectToAction(nameof(Dashboard));
            }

            var finalEvaluation = await _db.FypEvaluations
                .Include(e => e.Milestone)
                .Where(e => e.StudentGroupId == group.Id)
                .Where(e => e.Milestone.Type == FypMilestoneType.FinalEvaluation)
                .OrderByDescending(e => e.SubmittedAt)
                .FirstOrDefaultAsync();

            if (finalEvaluation == null)
            {
                TempData["Error"] = "Final evaluation has not been created for this group.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (finalEvaluation.Status != FypEvaluationStatus.Completed &&
                finalEvaluation.Status != FypEvaluationStatus.Published)
            {
                TempData["Error"] = "Final evaluation must be completed before finalizing FYP-1.";
                return RedirectToAction(nameof(Dashboard));
            }

            group.CurrentStage = FypStage.Fyp2;

            await _db.SaveChangesAsync();

            TempData["Success"] = "FYP-1 finalized successfully. Group has been promoted to FYP-2.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeFyp2(int groupId, string? completionRemarks)
        {
            var group = await _db.StudentGroups
                .Include(g => g.FypCall)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                TempData["Error"] = "Student group not found.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            if (group.CurrentStage != FypStage.Fyp2)
            {
                TempData["Error"] = "This group is not currently in FYP-2.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            if (group.IsFypCompleted)
            {
                TempData["Error"] = "This group has already been finalized.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            var completedFyp2Chapters = await _db.FypChapterSubmissions
                .Include(x => x.ChapterAnnouncement)
                .Where(x => x.GroupId == group.Id)
                .Where(x => x.ChapterAnnouncement.Stage == FypStage.Fyp2)
                .CountAsync(x => x.Status == ChapterSubmissionStatus.CoordinatorApproved);

            if (completedFyp2Chapters < 3)
            {
                TempData["Error"] = "All FYP-2 chapters must be coordinator-approved before finalization.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            var finalEvaluation = await _db.FypEvaluations
                .Include(e => e.Milestone)
                .Where(e => e.StudentGroupId == group.Id)
                .Where(e => e.Milestone.Stage == FypStage.Fyp2 &&
                            e.Milestone.Type == FypMilestoneType.FinalEvaluation)
                .OrderByDescending(e => e.SubmittedAt)
                .FirstOrDefaultAsync();

            if (finalEvaluation == null)
            {
                TempData["Error"] = "Final evaluation has not been created for this FYP-2 group.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            if (finalEvaluation.Status != FypEvaluationStatus.Completed &&
                finalEvaluation.Status != FypEvaluationStatus.Published)
            {
                TempData["Error"] = "Final evaluation must be completed before finalizing FYP-2.";
                return RedirectToAction(nameof(FinalizeFyp2Groups));
            }

            group.IsFypCompleted = true;
            group.CompletedAt = DateTime.UtcNow;
            group.CompletionRemarks = string.IsNullOrWhiteSpace(completionRemarks)
                ? "FYP project completed successfully and finalized by coordinator."
                : completionRemarks.Trim();
            group.ReadyForLmsSync = true;
            group.LastStatusUpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "FYP-2 finalized successfully. Group is now marked as completed and ready for LMS/CMS sync.";
            return RedirectToAction(nameof(FinalizeFyp2Groups));
        }

        private async Task<bool> IsReadyForFyp1FinalizationAsync(StudentGroup group)
        {
            if (group.CurrentStage != FypStage.Fyp1)
                return false;

            var finalEvaluation = await _db.FypEvaluations
                .Include(e => e.Milestone)
                .Where(e => e.StudentGroupId == group.Id)
                .Where(e => e.Milestone.Type == FypMilestoneType.FinalEvaluation)
                .OrderByDescending(e => e.SubmittedAt)
                .FirstOrDefaultAsync();

            return finalEvaluation != null &&
                   (finalEvaluation.Status == FypEvaluationStatus.Completed ||
                    finalEvaluation.Status == FypEvaluationStatus.Published);
        }

        private async Task<bool> IsReadyForFyp2FinalizationAsync(StudentGroup group)
        {
            if (group.CurrentStage != FypStage.Fyp2 || group.IsFypCompleted)
                return false;

            var completedFyp2Chapters = await _db.FypChapterSubmissions
                .Include(x => x.ChapterAnnouncement)
                .Where(x => x.GroupId == group.Id)
                .Where(x => x.ChapterAnnouncement.Stage == FypStage.Fyp2)
                .CountAsync(x => x.Status == ChapterSubmissionStatus.CoordinatorApproved);

            if (completedFyp2Chapters < 3)
                return false;

            var finalEvaluation = await _db.FypEvaluations
                .Include(e => e.Milestone)
                .Where(e => e.StudentGroupId == group.Id)
                .Where(e => e.Milestone.Stage == FypStage.Fyp2 &&
                            e.Milestone.Type == FypMilestoneType.FinalEvaluation)
                .OrderByDescending(e => e.SubmittedAt)
                .FirstOrDefaultAsync();

            return finalEvaluation != null &&
                   (finalEvaluation.Status == FypEvaluationStatus.Completed ||
                    finalEvaluation.Status == FypEvaluationStatus.Published);
        }

        public async Task<IActionResult> FinalizeFyp2Groups()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x => x.CoordinatorEmail.ToLower() == currentEmail && x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new List<StudentGroup>());
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var groups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.CurrentStage == FypStage.Fyp2)
                .Where(g => !g.IsFypCompleted)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
                .OrderBy(g => g.Id)
                .ToListAsync();

            return View(groups);
        }

        public async Task<IActionResult> FinalizeFyp1Groups()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x => x.CoordinatorEmail.ToLower() == currentEmail && x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new List<StudentGroup>());
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var groups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.CurrentStage == FypStage.Fyp1)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
                .OrderBy(g => g.Id)
                .ToListAsync();

            return View(groups);
        }

        public async Task<IActionResult> FinalizationCenter()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x => x.CoordinatorEmail.ToLower() == currentEmail && x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new FypFinalizationCenterVm());
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var allGroups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
                .OrderBy(g => g.Id)
                .ToListAsync();

            var vm = new FypFinalizationCenterVm();

            foreach (var group in allGroups)
            {
                if (await IsReadyForFyp1FinalizationAsync(group))
                    vm.ReadyForFyp1Finalization.Add(group);

                if (await IsReadyForFyp2FinalizationAsync(group))
                    vm.ReadyForFyp2Finalization.Add(group);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeAllReadyFyp1()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x => x.CoordinatorEmail.ToLower() == currentEmail && x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return RedirectToAction(nameof(FinalizationCenter));
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var groups = await _db.StudentGroups
                .Where(g => g.CurrentStage == FypStage.Fyp1)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
                .ToListAsync();

            var finalizedCount = 0;

            foreach (var group in groups)
            {
                if (!await IsReadyForFyp1FinalizationAsync(group))
                    continue;

                group.CurrentStage = FypStage.Fyp2;
                group.LastStatusUpdatedAt = DateTime.UtcNow;
                finalizedCount++;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{finalizedCount} FYP-1 group(s) finalized and promoted to FYP-2.";
            return RedirectToAction(nameof(FinalizationCenter));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeAllReadyFyp2()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x => x.CoordinatorEmail.ToLower() == currentEmail && x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return RedirectToAction(nameof(FinalizationCenter));
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var groups = await _db.StudentGroups
                .Where(g => g.CurrentStage == FypStage.Fyp2)
                .Where(g => !g.IsFypCompleted)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
                .ToListAsync();

            var finalizedCount = 0;

            foreach (var group in groups)
            {
                if (!await IsReadyForFyp2FinalizationAsync(group))
                    continue;

                group.IsFypCompleted = true;
                group.CompletedAt = DateTime.UtcNow;
                group.CompletionRemarks = "FYP completed and finalized by coordinator.";
                group.ReadyForLmsSync = true;
                group.LastStatusUpdatedAt = DateTime.UtcNow;
                finalizedCount++;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{finalizedCount} FYP-2 group(s) finalized and marked completed.";
            return RedirectToAction(nameof(FinalizationCenter));
        }
    }

    public class CoordinatorChaptersVm
    {
        public int ActiveCallId { get; set; }
        public string? ActiveCallTitle { get; set; }
        public string? ActiveCallBatch { get; set; }

        public FypStage SelectedStage { get; set; } = FypStage.Fyp1;

        public List<FypChapterAnnouncement> Items { get; set; } = new();
    }

    public class EditChapterVm
    {
        public int Id { get; set; }
        public FypChapterType ChapterType { get; set; }
        public FypStage Stage { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Instructions { get; set; }
    }

    public class FypFinalizationCenterVm
    {
        public List<StudentGroup> ReadyForFyp1Finalization { get; set; } = new();
        public List<StudentGroup> ReadyForFyp2Finalization { get; set; } = new();
    }
}
