using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EMAP.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class FypController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public FypController(
            EmapDbContext db,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _env = env;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // ===================== FYP PORTAL =====================

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();

            var activeCalls = await _db.FypCalls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AnnouncementDate)
                .ToListAsync();

            var activeCall = activeCalls.FirstOrDefault();

            StudentGroup? group = null;
            ProposalSubmission? proposal = null;
            ProposalDefenseSchedule? defenseSchedule = null;
            ProposalDefenseEvaluation? defenseEvaluation = null;

            IList<FypChapterAnnouncement> chapters = new List<FypChapterAnnouncement>();
            FypChapterAnnouncement? openChapter = null;

            // NEW: list of boxes/cards (one per chapter)
            var chapterBoxes = new List<EMAP.Web.ViewModels.Fyp.ChapterBoxViewModel>();

            if (activeCall != null)
            {
                group = await _db.StudentGroups
                    .Include(g => g.Supervisor)
                        .ThenInclude(s => s.User)
                    .Include(g => g.Project)
                    .FirstOrDefaultAsync(g =>
                        g.FypCallId == activeCall.Id &&
                        (g.LeaderId == userId ||
                         g.Member2Id == userId ||
                         g.Member3Id == userId));

                if (group != null)
                {
                    proposal = await _db.ProposalSubmissions
                        .Where(p => p.GroupId == group.Id)
                        .OrderByDescending(p => p.SubmittedAt)
                        .FirstOrDefaultAsync();

                    if (proposal != null)
                    {
                        defenseSchedule = await _db.ProposalDefenseSchedules
                            .FirstOrDefaultAsync(x => x.ProposalSubmissionId == proposal.Id);

                        defenseEvaluation = await _db.ProposalDefenseEvaluations
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefaultAsync(x => x.ProposalSubmissionId == proposal.Id);
                    }
                }

                // All chapter announcements for active call
                chapters = await _db.FypChapterAnnouncements
                    .Where(x => x.FypCallId == activeCall.Id)
                    .OrderBy(x => x.ChapterType)
                    .ToListAsync();

                // for old UI / other code if still used
                openChapter = chapters.FirstOrDefault(x => x.IsOpen);

                // NEW: Build boxes if group exists
                if (group != null && chapters.Any())
                {
                    // Load all submissions for this group (for all chapters)
                    var submissions = await _db.FypChapterSubmissions
                        .Where(x => x.GroupId == group.Id)
                        .OrderByDescending(x => x.SubmittedAt)
                        .ToListAsync();

                    bool unlocked = true; // first chapter unlocked by default

                    foreach (var ch in chapters)
                    {
                        // latest submission for this chapter announcement
                        var latest = submissions
                            .Where(s => s.ChapterAnnouncementId == ch.Id)
                            .OrderByDescending(s => s.SubmittedAt)
                            .FirstOrDefault();

                        var isLeader = group.LeaderId == userId;

                        var status = latest?.Status;

                        // allow submit if: unlocked + open + (no submission OR changes requested)
                        bool isChangesRequested = status == ChapterSubmissionStatus.ChangesRequested;

                        // block new submit if already submitted/in review/approved and not changes requested
                        bool alreadySubmittedAndLocked =
                            latest != null &&
                            status != ChapterSubmissionStatus.ChangesRequested &&
                            status != ChapterSubmissionStatus.CoordinatorApproved;

                        var box = new EMAP.Web.ViewModels.Fyp.ChapterBoxViewModel
                        {
                            ChapterAnnouncementId = ch.Id,
                            ChapterType = ch.ChapterType,
                            IsOpen = ch.IsOpen,
                            Deadline = ch.Deadline,

                            SubmissionId = latest?.Id,
                            Status = status,
                            Feedback = latest?.Feedback,
                            SubmittedAt = latest?.SubmittedAt,

                            IsUnlocked = unlocked,

                            CanSubmitNow = unlocked && ch.IsOpen && !alreadySubmittedAndLocked && (latest == null) && isLeader,
                            CanResubmit = unlocked && ch.IsOpen && isChangesRequested && isLeader
                        };

                        chapterBoxes.Add(box);

                        // Unlock next chapter ONLY if current is fully completed (CoordinatorApproved)
                        unlocked = (status == ChapterSubmissionStatus.CoordinatorApproved);
                    }
                }
            }

            var supervisors = await _db.FypSupervisors
                .Where(s => s.IsActive && s.CurrentSlots < s.MaxSlots)
                .OrderBy(s => s.Department ?? "")
                .ThenBy(s => s.Name ?? "")
                .ToListAsync();

            var model = new FypPortalViewModel
            {
                ActiveCalls = activeCalls,
                ActiveCall = activeCall,
                Group = group,
                Proposal = proposal,
                DefenseSchedule = defenseSchedule,
                DefenseEvaluation = defenseEvaluation,
                AvailableSupervisors = supervisors,
                IsGroupLeader = group != null && group.LeaderId == userId,

                // keep existing for backward compatibility
                OpenChapter = openChapter,

                // OLD single-submission property no longer used by new UI,
                // you can keep it null or leave as-is:
                ChapterSubmission = null,

                // NEW list for UI
                ChapterBoxes = chapterBoxes
            };

            return View(model);
        }

        // ===================== CREATE GROUP (GET) =====================

        [HttpGet]
        public async Task<IActionResult> CreateGroup()
        {
            var userId = GetCurrentUserId();

            var activeCall = await _db.FypCalls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AnnouncementDate)
                .FirstOrDefaultAsync();

            if (activeCall == null)
            {
                TempData["Error"] = "No active FYP call is open.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _db.StudentGroups
                .FirstOrDefaultAsync(g =>
                    g.FypCallId == activeCall.Id &&
                    (g.LeaderId == userId ||
                     g.Member2Id == userId ||
                     g.Member3Id == userId));

            if (existing != null)
            {
                TempData["Error"] = "You are already part of an FYP group.";
                return RedirectToAction(nameof(Index));
            }

            return View(new CreateGroupViewModel
            {
                ActiveCallTitle = activeCall.Title,
                Batch = activeCall.Batch
            });
        }

        // ===================== CREATE GROUP (POST) =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(CreateGroupViewModel model)
        {
            var userId = GetCurrentUserId();

            var activeCall = await _db.FypCalls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AnnouncementDate)
                .FirstOrDefaultAsync();

            if (activeCall == null)
            {
                TempData["Error"] = "No active FYP call at the moment.";
                return RedirectToAction(nameof(Index));
            }

            var existingGroup = await _db.StudentGroups
                .FirstOrDefaultAsync(g =>
                    g.FypCallId == activeCall.Id &&
                    (g.LeaderId == userId ||
                     g.Member2Id == userId ||
                     g.Member3Id == userId));

            if (existingGroup != null)
            {
                TempData["Error"] = "You are already part of a group.";
                return RedirectToAction(nameof(Index));
            }

            string? member2Id = null;
            string? member3Id = null;

            if (!string.IsNullOrWhiteSpace(model.Member2Email))
            {
                var m2 = await _userManager.FindByEmailAsync(model.Member2Email.Trim());
                if (m2 == null)
                {
                    ModelState.AddModelError(nameof(model.Member2Email), "No such user exists.");
                }
                else if (m2.Id == userId)
                {
                    ModelState.AddModelError(nameof(model.Member2Email), "You cannot add yourself.");
                }
                else
                {
                    var inOther = await _db.StudentGroups.AnyAsync(g =>
                        g.FypCallId == activeCall.Id &&
                        (g.LeaderId == m2.Id || g.Member2Id == m2.Id || g.Member3Id == m2.Id));

                    if (inOther)
                        ModelState.AddModelError(nameof(model.Member2Email), "User is already in a group.");
                    else
                        member2Id = m2.Id;
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Member3Email))
            {
                var m3 = await _userManager.FindByEmailAsync(model.Member3Email.Trim());
                if (m3 == null)
                {
                    ModelState.AddModelError(nameof(model.Member3Email), "No such user exists.");
                }
                else if (m3.Id == userId || m3.Id == member2Id)
                {
                    ModelState.AddModelError(nameof(model.Member3Email), "Duplicate member.");
                }
                else
                {
                    var inOther = await _db.StudentGroups.AnyAsync(g =>
                        g.FypCallId == activeCall.Id &&
                        (g.LeaderId == m3.Id || g.Member2Id == m3.Id || g.Member3Id == m3.Id));

                    if (inOther)
                        ModelState.AddModelError(nameof(model.Member3Email), "User is already in a group.");
                    else
                        member3Id = m3.Id;
                }
            }

            if (!ModelState.IsValid)
            {
                model.ActiveCallTitle = activeCall.Title;
                model.Batch = activeCall.Batch;
                return View(model);
            }

            _db.StudentGroups.Add(new StudentGroup
            {
                FypCallId = activeCall.Id,
                LeaderId = userId,
                Member2Id = member2Id,
                Member3Id = member3Id,
                Status = GroupStatus.PendingSupervisorSelection
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Group created successfully. Now select a supervisor.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== SELECT SUPERVISOR (POST) =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectSupervisor(int supervisorId)
        {
            var userId = GetCurrentUserId();

            var activeCall = await _db.FypCalls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AnnouncementDate)
                .FirstOrDefaultAsync();

            if (activeCall == null)
            {
                TempData["Error"] = "No active FYP call.";
                return RedirectToAction(nameof(Index));
            }

            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g => g.FypCallId == activeCall.Id && g.LeaderId == userId);

            if (group == null)
            {
                TempData["Error"] = "Only group leader can select supervisor.";
                return RedirectToAction(nameof(Index));
            }

            if (group.SupervisorId != null)
            {
                TempData["Error"] = "Supervisor already selected.";
                return RedirectToAction(nameof(Index));
            }

            var supervisor = await _db.FypSupervisors
                .FirstOrDefaultAsync(s => s.Id == supervisorId && s.IsActive);

            if (supervisor == null)
            {
                TempData["Error"] = "Supervisor not available.";
                return RedirectToAction(nameof(Index));
            }

            if (supervisor.CurrentSlots >= supervisor.MaxSlots)
            {
                TempData["Error"] = "Supervisor has no available slots.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ FK is int? -> store supervisor.Id (int)
            group.SupervisorId = supervisor.Id;
            group.Status = GroupStatus.PendingSupervisorApproval;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor request sent successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== SUBMIT PROPOSAL (GET) =====================

        [HttpGet]
        public async Task<IActionResult> SubmitProposal()
        {
            var userId = GetCurrentUserId();

            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g =>
                    g.LeaderId == userId ||
                    g.Member2Id == userId ||
                    g.Member3Id == userId);

            if (group == null)
            {
                TempData["Error"] = "Create a group first.";
                return RedirectToAction(nameof(Index));
            }

            if (group.LeaderId != userId)
            {
                TempData["Error"] = "Only the group leader can submit or resubmit proposals.";
                return RedirectToAction(nameof(Index));
            }

            if (group.Status != GroupStatus.Approved)
            {
                TempData["Error"] = "Supervisor must approve your group first.";
                return RedirectToAction(nameof(Index));
            }

            var existing = await _db.ProposalSubmissions
                .Where(p => p.GroupId == group.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .FirstOrDefaultAsync();

            var vm = new SubmitProposalViewModel
            {
                GroupId = group.Id,
                SupervisorName = group.Supervisor != null ? group.Supervisor.Name : "Not Assigned",
                ExistingStatus = existing?.Status,
                ExistingFeedback = existing?.SupervisorFeedback,
                Title = existing?.Title ?? ""
            };

            return View(vm);
        }

        // ===================== SUBMIT PROPOSAL (POST) =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitProposal(SubmitProposalViewModel model, IFormFile proposalFile)
        {
            var userId = GetCurrentUserId();

            if (proposalFile == null || proposalFile.Length == 0)
            {
                ModelState.AddModelError("", "Choose a file to upload.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError(nameof(model.Title), "Proposal title required.");
                return View(model);
            }

            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g =>
                    g.LeaderId == userId ||
                    g.Member2Id == userId ||
                    g.Member3Id == userId);

            if (group == null)
            {
                TempData["Error"] = "Only the group leader can submit.";
                return RedirectToAction(nameof(Index));
            }

            if (group.Status != GroupStatus.Approved)
            {
                TempData["Error"] = "Supervisor must approve your group first.";
                return RedirectToAction(nameof(Index));
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "proposals");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(proposalFile.FileName);
            var fileName = $"group_{group.Id}_rev_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            await using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                await proposalFile.CopyToAsync(fs);
            }

            var relativePath = "/uploads/proposals/" + fileName;

            var existing = await _db.ProposalSubmissions
                .Where(p => p.GroupId == group.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                _db.ProposalSubmissions.Add(new ProposalSubmission
                {
                    GroupId = group.Id,
                    Title = model.Title,
                    FilePath = relativePath,
                    SubmittedAt = DateTime.UtcNow,
                    Status = ProposalStatus.PendingReview,
                    SupervisorFeedback = null,
                    RevisionNumber = 1
                });
            }
            else
            {
                existing.Title = model.Title;
                existing.FilePath = relativePath;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = ProposalStatus.PendingReview;
                existing.SupervisorFeedback = null;
                existing.RevisionNumber++;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Proposal submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== SUBMIT CHAPTER (GET) =====================

        [HttpGet]
        public async Task<IActionResult> SubmitChapter()
        {
            var userId = GetCurrentUserId();

            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g =>
                    g.LeaderId == userId ||
                    g.Member2Id == userId ||
                    g.Member3Id == userId);

            if (group == null)
            {
                TempData["Error"] = "Group not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentCall = await _db.FypCalls.FirstOrDefaultAsync(c => c.IsActive);
            if (currentCall == null)
            {
                TempData["Error"] = "No active FYP call.";
                return RedirectToAction(nameof(Index));
            }

            var openChapter = await _db.FypChapterAnnouncements
                .Where(c => c.FypCallId == currentCall.Id && c.IsOpen)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (openChapter == null)
            {
                TempData["Error"] = "Chapter is not open.";
                return RedirectToAction(nameof(Index));
            }

            var myChapters = await _db.FypChapterSubmissions
                .Include(x => x.ChapterAnnouncement)
                .Where(x =>
                    x.GroupId == group.Id &&
                    (x.Status == ChapterSubmissionStatus.ChangesRequested ||
                     x.Status == ChapterSubmissionStatus.SupervisorApproved))
                .OrderByDescending(x => x.SubmittedAt)
                .ToListAsync();

            var completedChapters = new List<FypChapterSubmission>();

            if (group != null)
            {
                completedChapters = await _db.FypChapterSubmissions
                    .Include(x => x.ChapterAnnouncement)
                    .Where(x =>
                        x.GroupId == group.Id &&
                        x.Status == ChapterSubmissionStatus.CoordinatorApproved)
                    .OrderBy(x => x.ChapterAnnouncement.ChapterType)
                    .ToListAsync();
            }

            return View(new SubmitChapterViewModel
            {
                ChapterAnnouncementId = openChapter.Id,
                ChapterType = openChapter.ChapterType,
                Deadline = openChapter.Deadline,
                Instructions = openChapter.Instructions,
                MyChapters = myChapters
            });
        }

        // ===================== SUBMIT CHAPTER (POST) =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChapter(SubmitChapterViewModel vm, IFormFile file)
        {
            var userId = GetCurrentUserId();

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View(vm);
            }

            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g =>
                    g.LeaderId == userId ||
                    g.Member2Id == userId ||
                    g.Member3Id == userId);

            if (group == null)
            {
                TempData["Error"] = "Group not found.";
                return RedirectToAction(nameof(Index));
            }

            if (group.SupervisorId == null)
            {
                TempData["Error"] = "Supervisor not assigned yet.";
                return RedirectToAction(nameof(Index));
            }

            var currentCall = await _db.FypCalls.FirstOrDefaultAsync(c => c.IsActive);
            if (currentCall == null)
            {
                TempData["Error"] = "No active FYP call.";
                return RedirectToAction(nameof(Index));
            }

            var openChapter = await _db.FypChapterAnnouncements
                .FirstOrDefaultAsync(c => c.FypCallId == currentCall.Id && c.IsOpen);

            if (openChapter == null)
            {
                TempData["Error"] = "No chapter is currently open.";
                return RedirectToAction(nameof(Index));
            }

            // 🔑 Load existing submission (if any)
            var existing = await _db.FypChapterSubmissions
                .FirstOrDefaultAsync(s =>
                    s.GroupId == group.Id &&
                    s.ChapterAnnouncementId == openChapter.Id);

            // 🔒 BLOCK FINALIZED CHAPTER (NEW FUNCTIONALITY)
            if (existing != null && existing.Status == ChapterSubmissionStatus.CoordinatorApproved)
            {
                TempData["Error"] = "This chapter has already been finalized and cannot be modified.";
                return RedirectToAction(nameof(Index));
            }

            // Save file
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "chapters");
            Directory.CreateDirectory(uploads);

            var safeFileName =
                $"chapter_{group.Id}_{openChapter.ChapterType}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";

            var path = Path.Combine(uploads, safeFileName);

            await using (var fs = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            if (existing == null)
            {
                // ✅ FIRST SUBMISSION
                var supervisorUserId = await _db.FypSupervisors
                    .Where(s => s.Id == group.SupervisorId.Value)
                    .Select(s => s.UserId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(supervisorUserId))
                {
                    TempData["Error"] = "Supervisor record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _db.FypChapterSubmissions.Add(new FypChapterSubmission
                {
                    GroupId = group.Id,
                    ChapterAnnouncementId = openChapter.Id,
                    Title = vm.Title,
                    FilePath = "/uploads/chapters/" + safeFileName,
                    SubmittedAt = DateTime.UtcNow,
                    Status = ChapterSubmissionStatus.Submitted,
                    SupervisorId = supervisorUserId
                });
            }
            else if (existing.Status == ChapterSubmissionStatus.ChangesRequested)
            {
                // ✅ RESUBMISSION
                existing.Title = vm.Title;
                existing.FilePath = "/uploads/chapters/" + safeFileName;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = ChapterSubmissionStatus.Resubmitted;
            }
            else
            {
                TempData["Error"] = "You have already submitted this chapter.";
                return RedirectToAction(nameof(Index));
            }

            await _db.SaveChangesAsync();

            TempData["Success"] =
                $"Chapter '{openChapter.ChapterType}' submitted successfully. It has been sent to your supervisor for review.";

            return RedirectToAction(nameof(Index));
        }
    }
}
