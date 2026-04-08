using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;

namespace EMAP.Web.Controllers
{
    public class FypMidEvaluationController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FypMidEvaluationController(EmapDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLower()
                ?? User.Identity?.Name?.Trim().ToLower()
                ?? string.Empty;
        }

        // ===================== COORDINATOR: LIST =====================

        public async Task<IActionResult> Index()
        {
            var currentEmail = GetCurrentUserEmail();

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x =>
                    x.CoordinatorEmail.ToLower() == currentEmail &&
                    x.IsActive);

            if (committee == null && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new List<FypEvaluation>());
            }

            var programCodes = committee?.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList() ?? new List<string>();

            var evaluationsQuery = _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Milestone)
                .Include(e => e.Scores)
                .Where(e => e.Milestone.Type == FypMilestoneType.MidEvaluation);

            if (!User.IsInRole("Admin"))
            {
                evaluationsQuery = evaluationsQuery.Where(e =>
                    e.StudentGroup != null &&
                    !string.IsNullOrWhiteSpace(e.StudentGroup.ProgramCode) &&
                    programCodes.Contains(e.StudentGroup.ProgramCode.ToUpper()));
            }

            var data = await evaluationsQuery
                .OrderByDescending(e => e.ScheduledAt)
                .ToListAsync();

            return View(data);
        }

        // ===================== COORDINATOR: CREATE =====================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var midMilestones = await _db.FypMilestones
                .Where(m => m.Type == FypMilestoneType.MidEvaluation && m.IsActive)
                .OrderBy(m => m.Title)
                .ToListAsync();

            var groups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.Status == GroupStatus.Approved)
                .OrderBy(g => g.Id)
                .ToListAsync();

            ViewBag.Milestones = new SelectList(midMilestones, "Id", "Title");
            ViewBag.Groups = new SelectList(groups.Select(g => new
            {
                g.Id,
                Text = $"Group #{g.Id} - {g.TentativeProjectTitle} ({g.ProgramCode})"
            }), "Id", "Text");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int studentGroupId, int milestoneId, string evaluatorEmail, DateTime? scheduledAt, string? venue, string? instructions, string? committeeMembers)
        {
            if (studentGroupId <= 0 || milestoneId <= 0 || string.IsNullOrWhiteSpace(evaluatorEmail))
            {
                TempData["Error"] = "Group, milestone and evaluator email are required.";
                return RedirectToAction(nameof(Create));
            }

            evaluatorEmail = evaluatorEmail.Trim().ToLower();

            var evaluator = await _userManager.FindByEmailAsync(evaluatorEmail);
            if (evaluator == null)
            {
                TempData["Error"] = "Evaluator email not found.";
                return RedirectToAction(nameof(Create));
            }

            var exists = await _db.FypEvaluations.AnyAsync(e =>
                e.StudentGroupId == studentGroupId &&
                e.MilestoneId == milestoneId &&
                e.EvaluatorUserId == evaluator.Id);

            if (exists)
            {
                TempData["Error"] = "This evaluator is already assigned for this group's mid evaluation.";
                return RedirectToAction(nameof(Index));
            }

            var evaluation = new FypEvaluation
            {
                StudentGroupId = studentGroupId,
                MilestoneId = milestoneId,
                EvaluatorUserId = evaluator.Id,
                ScheduledAt = scheduledAt,
                Venue = venue,
                Instructions = instructions,
                CommitteeMembers = committeeMembers,
                Status = FypEvaluationStatus.Draft,
                IsSubmitted = false,
                TotalMarks = 0
            };

            _db.FypEvaluations.Add(evaluation);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Mid evaluation assigned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== EVALUATOR: MY ASSIGNED MID EVALUATIONS =====================

        public async Task<IActionResult> MyAssigned()
        {
            var currentUserId = GetCurrentUserId();

            var data = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Milestone)
                .Include(e => e.Scores)
                .Where(e => e.EvaluatorUserId == currentUserId && e.Milestone.Type == FypMilestoneType.MidEvaluation)
                .OrderByDescending(e => e.ScheduledAt)
                .ToListAsync();

            return View(data);
        }

        // ===================== EVALUATOR: FORM =====================

        [HttpGet]
        public async Task<IActionResult> Evaluate(int id)
        {
            var currentUserId = GetCurrentUserId();

            var evaluation = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Scores)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evaluation == null)
                return NotFound();

            if (!User.IsInRole("Admin") && evaluation.EvaluatorUserId != currentUserId)
            {
                TempData["Error"] = "You are not assigned to this evaluation.";
                return RedirectToAction(nameof(MyAssigned));
            }

            var criteria = await _db.FypEvaluationCriteria
                .Where(c => c.EvaluationType == FypMilestoneType.MidEvaluation && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var vm = new FypMidEvaluationFormViewModel
            {
                EvaluationId = evaluation.Id,
                StudentGroupId = evaluation.StudentGroupId,
                GroupTitle = evaluation.StudentGroup?.TentativeProjectTitle ?? "-",
                Batch = evaluation.StudentGroup?.FypCall?.Batch ?? "-",
                ProgramCode = evaluation.StudentGroup?.ProgramCode ?? "-",
                SupervisorName = evaluation.StudentGroup?.Supervisor?.Name ?? "-",
                Venue = evaluation.Venue ?? string.Empty,
                ScheduledAt = evaluation.ScheduledAt,
                Remarks = evaluation.Remarks,
                Criteria = criteria.Select(c =>
                {
                    var existing = evaluation.Scores.FirstOrDefault(s => s.CriterionId == c.Id);
                    return new FypMidEvaluationCriterionItem
                    {
                        CriterionId = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        MaxMarks = c.MaxMarks,
                        AwardedMarks = existing?.AwardedMarks ?? 0,
                        Comment = existing?.Comment
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(FypMidEvaluationFormViewModel vm)
        {
            var currentUserId = GetCurrentUserId();

            var evaluation = await _db.FypEvaluations
                .Include(e => e.Scores)
                .Include(e => e.Milestone)
                .FirstOrDefaultAsync(e => e.Id == vm.EvaluationId);

            if (evaluation == null)
                return NotFound();

            if (!User.IsInRole("FypCoordinator") && evaluation.EvaluatorUserId != currentUserId)
            {
                TempData["Error"] = "You are not assigned to this evaluation.";
                return RedirectToAction(nameof(MyAssigned));
            }

            var criteria = await _db.FypEvaluationCriteria
                .Where(c => c.EvaluationType == FypMilestoneType.MidEvaluation && c.IsActive)
                .ToListAsync();

            foreach (var item in vm.Criteria)
            {
                var criterion = criteria.FirstOrDefault(c => c.Id == item.CriterionId);
                if (criterion == null)
                    continue;

                if (item.AwardedMarks < 0 || item.AwardedMarks > criterion.MaxMarks)
                {
                    ModelState.AddModelError(string.Empty, $"{criterion.Title} marks must be between 0 and {criterion.MaxMarks}.");
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            foreach (var item in vm.Criteria)
            {
                var score = evaluation.Scores.FirstOrDefault(s => s.CriterionId == item.CriterionId);

                if (score == null)
                {
                    score = new FypEvaluationScore
                    {
                        CriterionId = item.CriterionId,
                        AwardedMarks = item.AwardedMarks,
                        Comment = item.Comment
                    };
                    evaluation.Scores.Add(score);
                }
                else
                {
                    score.AwardedMarks = item.AwardedMarks;
                    score.Comment = item.Comment;
                }
            }

            evaluation.TotalMarks = evaluation.Scores.Sum(x => x.AwardedMarks);
            evaluation.Remarks = vm.Remarks;
            evaluation.IsSubmitted = true;
            evaluation.SubmittedAt = DateTime.Now;
            evaluation.Status = FypEvaluationStatus.Submitted;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Mid evaluation submitted successfully.";
            return RedirectToAction(nameof(MyAssigned));
        }
    }
}
