using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EMAP.Web.ViewModels;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize] // authenticated users only (roles handled per action)
    public class SupervisorController : Controller
    {
        private readonly EmapDbContext _db;

        // NOTE:
        // You are using Email linking in many places (CurrentEmail).
        // Keep it, but also support linking by UserId when available.
        private string? CurrentEmail => User.Identity?.Name;
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public SupervisorController(EmapDbContext db)
        {
            _db = db;
        }

        // ===================== HELPERS =====================

        /// <summary>
        /// Finds supervisor record for the logged-in supervisor.
        /// Prefer UserId link; fallback to Email match (legacy).
        /// </summary>
        private async Task<FypSupervisor?> GetCurrentSupervisorAsync()
        {
            var userId = CurrentUserId;
            var email = CurrentEmail;

            // Prefer UserId link (BEST)
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var byUserId = await _db.FypSupervisors.FirstOrDefaultAsync(s => s.UserId == userId);
                if (byUserId != null) return byUserId;
            }

            // Fallback to Email link (LEGACY)
            if (!string.IsNullOrWhiteSpace(email))
            {
                var byEmail = await _db.FypSupervisors.FirstOrDefaultAsync(s => s.Email == email);
                if (byEmail != null) return byEmail;
            }

            return null;
        }

        private void SetNotLinkedWarning(string message)
        {
            ViewBag.Warning = message;
        }

        // ===================== ADMIN: SUPERVISOR MASTER LIST =====================

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var supervisors = _db.FypSupervisors
                .Select(s => new FypSupervisor
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email ?? "",
                    Department = s.Department ?? "",
                    FieldOfExpertise = s.FieldOfExpertise ?? "",
                    MaxSlots = s.MaxSlots,
                    CurrentSlots = s.CurrentSlots,
                    IsActive = s.IsActive,
                    UserId = s.UserId
                })
                .OrderBy(s => s.Department)
                .ThenBy(s => s.Name)
                .ToList();

            return View(supervisors);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new FypSupervisor
            {
                MaxSlots = 3,
                IsActive = true,
                CurrentSlots = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(FypSupervisor model)
        {
            ModelState.Remove(nameof(FypSupervisor.User));
            ModelState.Remove(nameof(FypSupervisor.UserId));


            if (!ModelState.IsValid)
                return View(model);

            // Link supervisor to ASP.NET Identity user by Email
            model.UserId = _db.Users
                .Where(u => u.Email == model.Email)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(model.UserId))
            {
                ModelState.AddModelError("", "No ASP.NET user exists with this email.");
                return View(model);
            }

            model.CurrentSlots = 0;

            _db.FypSupervisors.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "Supervisor created successfully.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var supervisor = await _db.FypSupervisors.FindAsync(id);
            if (supervisor == null) return NotFound();

            return View(supervisor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(FypSupervisor model)
        {
            // If you have navigation props in model, ignore them
            ModelState.Remove(nameof(FypSupervisor.UserId));

            if (!ModelState.IsValid)
                return View(model);

            var supervisor = await _db.FypSupervisors.FindAsync(model.Id);
            if (supervisor == null) return NotFound();

            supervisor.Name = model.Name;
            supervisor.Email = model.Email;
            supervisor.Department = model.Department;
            supervisor.FieldOfExpertise = model.FieldOfExpertise;
            supervisor.MaxSlots = model.MaxSlots;
            supervisor.IsActive = model.IsActive;

            // Re-link UserId if email changed (important after DB reset)
            supervisor.UserId = await _db.Users
                .Where(u => u.Email == supervisor.Email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(supervisor.UserId))
            {
                ModelState.AddModelError("", "No ASP.NET user exists with this email.");
                return View(model);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== SUPERVISOR PORTAL =====================

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Dashboard()
        {
            var supervisor = await GetCurrentSupervisorAsync();
            if (supervisor == null)
            {
                SetNotLinkedWarning(
                    "Your login is not linked to any FYP supervisor record. " +
                    "Ask the admin to set your Email and/or link your UserId in the FYP Supervisors list."
                );
                return View(new SupervisorDashboardViewModel());
            }

            var myGroups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Where(g => g.SupervisorId == supervisor.Id)
                .ToListAsync();

            var pendingRequests = myGroups.Count(g => g.Status == GroupStatus.PendingSupervisorApproval);
            var activeGroups = myGroups.Count(g => g.Status == GroupStatus.Approved);

            var myProposals = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor)
                .Where(p => p.Group.SupervisorId == supervisor.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();

            var pendingProposals = myProposals.Count(p => p.Status == ProposalStatus.PendingReview);
            var changesRequested = myProposals.Count(p => p.Status == ProposalStatus.ChangesRequested);
            var approvedForDefense = myProposals.Count(p => p.Status == ProposalStatus.ApprovedForDefense);

            // ✅ IMPORTANT:
            // Your FypChapterSubmission.SupervisorId is stored as ASP.NET UserId (string)
            // So count by supervisor.UserId (string). Fallback to CurrentUserId if needed.
            var supervisorUserId = !string.IsNullOrWhiteSpace(supervisor.UserId) ? supervisor.UserId : CurrentUserId;

            var pendingChapterReviews = 0;
            if (!string.IsNullOrWhiteSpace(supervisorUserId))
            {
                pendingChapterReviews = await _db.FypChapterSubmissions.CountAsync(s =>
                    s.SupervisorId == supervisorUserId &&
                    (s.Status == ChapterSubmissionStatus.Submitted || s.Status == ChapterSubmissionStatus.Resubmitted)
                );
            }

            ViewBag.PendingChapterReviews = pendingChapterReviews;

            var vm = new SupervisorDashboardViewModel
            {
                PendingRequests = pendingRequests,
                ActiveGroups = activeGroups,
                PendingProposals = pendingProposals,
                ChangesRequestedProposals = changesRequested,
                ApprovedForDefenseProposals = approvedForDefense,
                Groups = myGroups,
                RecentProposals = myProposals.Take(5).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> PendingRequests()
        {
            var supervisor = await GetCurrentSupervisorAsync();

            var baseQuery = _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.Status == GroupStatus.PendingSupervisorApproval);

            List<StudentGroup> pending;

            if (supervisor != null)
            {
                pending = await baseQuery
                    .Where(g => g.SupervisorId == supervisor.Id)
                    .OrderBy(g => g.FypCall.Batch)
                    .ToListAsync();

                if (!pending.Any())
                {
                    SetNotLinkedWarning(
                        "No pending requests found for your supervisor profile. Showing ALL pending groups (debug mode). " +
                        "Check that the FYP Supervisor record used by students has your email/UserId and that groups selected you."
                    );

                    pending = await baseQuery
                        .OrderBy(g => g.FypCall.Batch)
                        .ToListAsync();
                }
            }
            else
            {
                SetNotLinkedWarning(
                    "Your login is NOT linked to any record in FYP Supervisors. Admin must set your Email/UserId there. " +
                    "Currently showing ALL pending groups."
                );

                pending = await baseQuery
                    .OrderBy(g => g.FypCall.Batch)
                    .ToListAsync();
            }

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(int groupId)
        {
            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null || group.Supervisor == null)
            {
                TempData["Error"] = "Group or supervisor not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var sup = group.Supervisor;

            if (sup.CurrentSlots >= sup.MaxSlots)
            {
                TempData["Error"] = $"Supervisor {sup.Name} has no remaining slots.";
                return RedirectToAction(nameof(PendingRequests));
            }

            group.Status = GroupStatus.Approved;
            sup.CurrentSlots += 1;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Group approved successfully.";
            return RedirectToAction(nameof(PendingRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Reject(int groupId)
        {
            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                TempData["Error"] = "Group not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            group.SupervisorId = null;
            group.Status = GroupStatus.PendingSupervisorSelection;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Request rejected. Students can choose another supervisor.";
            return RedirectToAction(nameof(PendingRequests));
        }

        // ===================== PROPOSALS =====================

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Proposals()
        {
            var supervisor = await GetCurrentSupervisorAsync();

            var baseQuery = _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor);

            List<ProposalSubmission> proposals;

            if (supervisor != null)
            {
                proposals = await baseQuery
                    .Where(p => p.Group.SupervisorId == supervisor.Id)
                    .OrderByDescending(p => p.SubmittedAt)
                    .ToListAsync();

                if (!proposals.Any())
                {
                    SetNotLinkedWarning(
                        "No proposals found for your groups. Showing ALL proposals (debug) so you can verify data linking."
                    );

                    proposals = await baseQuery
                        .OrderByDescending(p => p.SubmittedAt)
                        .ToListAsync();
                }
            }
            else
            {
                SetNotLinkedWarning(
                    "Your login is NOT linked to any FYP Supervisor record. Admin must set your Email/UserId in the FYP Supervisors list. Currently showing ALL proposals."
                );

                proposals = await baseQuery
                    .OrderByDescending(p => p.SubmittedAt)
                    .ToListAsync();
            }

            return View(proposals);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Review(int id)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();

            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveForDefense(int id)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();

            if (proposal.Status == ProposalStatus.ApprovedForDefense ||
                proposal.Status == ProposalStatus.DefenseScheduled)
            {
                TempData["Error"] = "This proposal is already approved/scheduled. No further action is allowed.";
                return RedirectToAction(nameof(Review), new { id });
            }

            proposal.Status = ProposalStatus.ApprovedForDefense;
            proposal.SupervisorFeedback = null;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Proposal approved for defense scheduling.";
            return RedirectToAction(nameof(Proposals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> RequestChanges(int id, string feedback)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();

            if (proposal.Status == ProposalStatus.ApprovedForDefense ||
                proposal.Status == ProposalStatus.DefenseScheduled)
            {
                TempData["Error"] = "You cannot request changes after approval/scheduling.";
                return RedirectToAction(nameof(Review), new { id });
            }

            proposal.Status = ProposalStatus.ChangesRequested;
            proposal.SupervisorFeedback = feedback;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Feedback saved. Student must revise and resubmit.";
            return RedirectToAction(nameof(Proposals));
        }

        // ===================== CHAPTER REVIEWS (SUPERVISOR) =====================
        //
        // IMPORTANT:
        // Your student submission stores SupervisorId as ASP.NET UserId (string).
        // So here we must compare SupervisorId == supervisor.UserId (string), NOT supervisor.Id (int).
        //
        // Also: include Resubmitted to show resubmissions.
        //

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ChapterReviews()
        {
            var supervisor = await GetCurrentSupervisorAsync();
            if (supervisor == null)
            {
                SetNotLinkedWarning("Supervisor profile not found / not linked.");
                return View(new List<FypChapterSubmission>());
            }

            var supervisorUserId = !string.IsNullOrWhiteSpace(supervisor.UserId)
                ? supervisor.UserId
                : CurrentUserId;

            if (string.IsNullOrWhiteSpace(supervisorUserId))
                return View(new List<FypChapterSubmission>());

            var submissions = await _db.FypChapterSubmissions
                .Include(s => s.Group)
                .Include(s => s.ChapterAnnouncement)
                .Where(s =>
                    s.SupervisorId == supervisorUserId &&
                    (s.Status == ChapterSubmissionStatus.Submitted ||
                     s.Status == ChapterSubmissionStatus.Resubmitted))
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ReviewChapter(
            int submissionId,
            bool approve,
            string? feedback)
        {
            var submission = await _db.FypChapterSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                TempData["Error"] = "Chapter submission not found.";
                return RedirectToAction(nameof(ChapterReviews));
            }

            // ================= SUPERVISOR APPROVES =================
            if (approve)
            {
                submission.Status = ChapterSubmissionStatus.SupervisorApproved;
                submission.Feedback = null;

                TempData["Success"] =
                    "Chapter approved successfully and forwarded to the coordinator for final review.";
            }
            // ================= REQUEST CHANGES =================
            else
            {
                if (string.IsNullOrWhiteSpace(feedback))
                {
                    TempData["Error"] =
                        "Please provide feedback before requesting changes.";
                    return RedirectToAction(nameof(ChapterReviews));
                }

                submission.Status = ChapterSubmissionStatus.ChangesRequested;
                submission.Feedback = feedback;

                TempData["Success"] =
                    "Feedback sent to student. Waiting for resubmission.";
            }

            submission.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ChapterReviews));
        }

    }
}