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
    [Authorize(Roles = "FYPCoordinator")]
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

        private async Task<string> GetCurrentUserDisplayNameAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return User.Identity?.Name ?? "Coordinator";
            return !string.IsNullOrWhiteSpace(user.FullName)
                ? user.FullName
                : (user.Email ?? user.UserName ?? "Coordinator");
        }

        public async Task<IActionResult> Index()
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
                return View(new List<FypEvaluation>());
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var data = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Milestone)
                .Include(e => e.Scores)
                .Where(e => e.Milestone.Type == FypMilestoneType.MidEvaluation)
                .Where(e =>
                    e.StudentGroup != null &&
                    !string.IsNullOrWhiteSpace(e.StudentGroup.ProgramCode) &&
                    programCodes.Contains(e.StudentGroup.ProgramCode.ToUpper()))
                .OrderByDescending(e => e.ScheduledAt)
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
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
                return RedirectToAction(nameof(Index));
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var midMilestones = await _db.FypMilestones
                .Where(m => m.Type == FypMilestoneType.MidEvaluation && m.IsActive)
                .OrderBy(m => m.Title)
                .ToListAsync();

            var groups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.Status == GroupStatus.Approved)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                            programCodes.Contains(g.ProgramCode.ToUpper()))
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
        public async Task<IActionResult> Create(int studentGroupId, int milestoneId, DateTime? scheduledAt, string? venue, string? instructions, string? committeeMembers, decimal weightagePercent)
        {
            var currentUserId = GetCurrentUserId();
            var currentEmail = GetCurrentUserEmail();

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x =>
                    x.CoordinatorEmail.ToLower() == currentEmail &&
                    x.IsActive);

            if (committee == null)
            {
                TempData["Error"] = "No active committee assigned to you.";
                return RedirectToAction(nameof(Index));
            }

            var programCodes = committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .ToList();

            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g =>
                    g.Id == studentGroupId &&
                    !string.IsNullOrWhiteSpace(g.ProgramCode) &&
                    programCodes.Contains(g.ProgramCode.ToUpper()));

            if (group == null || milestoneId <= 0)
            {
                TempData["Error"] = "Invalid group or milestone selection.";
                return RedirectToAction(nameof(Create));
            }

            if (weightagePercent < 0 || weightagePercent > 100)
            {
                TempData["Error"] = "Weightage must be between 0 and 100.";
                return RedirectToAction(nameof(Create));
            }

            var exists = await _db.FypEvaluations.AnyAsync(e =>
                e.StudentGroupId == studentGroupId &&
                e.MilestoneId == milestoneId &&
                e.EvaluatorUserId == currentUserId);

            if (exists)
            {
                TempData["Error"] = "Mid evaluation already exists for this group.";
                return RedirectToAction(nameof(Index));
            }

            var evaluatorName = await GetCurrentUserDisplayNameAsync();

            var evaluation = new FypEvaluation
            {
                StudentGroupId = studentGroupId,
                MilestoneId = milestoneId,
                EvaluatorUserId = currentUserId,
                EvaluatorName = evaluatorName,
                ScheduledAt = scheduledAt,
                Venue = venue,
                Instructions = instructions,
                CommitteeMembers = string.IsNullOrWhiteSpace(committeeMembers) ? evaluatorName : committeeMembers,
                WeightagePercent = weightagePercent,
                WeightedMarks = 0,
                Status = FypEvaluationStatus.Draft,
                IsSubmitted = false,
                TotalMarks = 0
            };

            _db.FypEvaluations.Add(evaluation);
            await _db.SaveChangesAsync();

            // Create per-member evaluation rows for all students in the group
            var memberUserIds = new List<string?> { group.LeaderId, group.Member2Id, group.Member3Id }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            foreach (var userId in memberUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) continue;

                _db.FypEvaluationMembers.Add(new FypEvaluationMember
                {
                    EvaluationId = evaluation.Id,
                    StudentUserId = user.Id,
                    StudentName = !string.IsNullOrWhiteSpace(user.FullName)
                        ? user.FullName
                        : (user.UserName ?? user.Email ?? "Student"),
                    RegistrationNo = user.UserName,
                    TotalMarks = 0,
                    WeightedMarks = 0
                });
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Mid evaluation created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyAssigned()
        {
            var programCodes = await GetCurrentCoordinatorProgramCodesAsync();

            if (!programCodes.Any())
            {
                TempData["Error"] = "No active committee assigned to you.";
                return View(new List<FypEvaluation>());
            }

            var data = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Milestone)
                .Include(e => e.Scores)
                .Where(e => e.Milestone.Type == FypMilestoneType.MidEvaluation)
                .Where(e =>
                    e.StudentGroup != null &&
                    !string.IsNullOrWhiteSpace(e.StudentGroup.ProgramCode) &&
                    programCodes.Contains(e.StudentGroup.ProgramCode.ToUpper()))
                .OrderByDescending(e => e.ScheduledAt)
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Evaluate(int id)
        {
            var evaluation = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Members)
                    .ThenInclude(m => m.Scores)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evaluation == null)
                return NotFound();

            if (!await CanCurrentCoordinatorAccessEvaluationAsync(evaluation))
            {
                TempData["Error"] = "You are not allowed to access this evaluation.";
                return RedirectToAction(nameof(Index));
            }

            await EnsureEvaluationMembersExistAsync(evaluation);

            // Reload after creating members
            evaluation = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.FypCall)
                .Include(e => e.StudentGroup)
                    .ThenInclude(g => g.Supervisor)
                .Include(e => e.Members)
                    .ThenInclude(m => m.Scores)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evaluation == null)
                return NotFound();

            var criteria = await _db.FypEvaluationCriteria
                .Where(c => c.EvaluationType == FypMilestoneType.MidEvaluation && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var vm = new FypMidEvaluationFormViewModel
            {
                EvaluationId = evaluation.Id,
                GroupId = evaluation.StudentGroupId,
                ProjectTitle = evaluation.StudentGroup?.TentativeProjectTitle ?? "-",
                Batch = evaluation.StudentGroup?.FypCall?.Batch ?? "-",
                ProgramCode = evaluation.StudentGroup?.ProgramCode ?? "-",
                SupervisorName = evaluation.StudentGroup?.Supervisor?.Name ?? "-",
                Venue = evaluation.Venue,
                ScheduledAt = evaluation.ScheduledAt,
                WeightagePercent = evaluation.WeightagePercent,
                OverallRemarks = evaluation.Remarks,
                EvaluatorName = evaluation.EvaluatorName ?? string.Empty,
                Criteria = criteria.Select(c => new CriterionHeaderViewModel
                {
                    CriterionId = c.Id,
                    Title = c.Title,
                    MaxMarks = c.MaxMarks
                }).ToList()
            };

            foreach (var member in evaluation.Members.OrderBy(m => m.StudentName))
            {
                var row = new MemberEvaluationRowViewModel
                {
                    EvaluationMemberId = member.Id,
                    StudentUserId = member.StudentUserId,
                    StudentName = member.StudentName,
                    RegistrationNo = member.RegistrationNo,
                    Remarks = member.Remarks,
                    TotalMarks = member.TotalMarks,
                    WeightedMarks = member.WeightedMarks
                };

                foreach (var criterion in criteria)
                {
                    var existingScore = member.Scores.FirstOrDefault(s => s.CriterionId == criterion.Id);

                    row.Scores.Add(new MemberCriterionScoreViewModel
                    {
                        CriterionId = criterion.Id,
                        CriterionTitle = criterion.Title,
                        MaxMarks = criterion.MaxMarks,
                        AwardedMarks = existingScore?.AwardedMarks ?? 0,
                        Comment = existingScore?.Comment
                    });
                }

                vm.Members.Add(row);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(FypMidEvaluationFormViewModel vm)
        {
            var evaluation = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                .Include(e => e.Members)
                    .ThenInclude(m => m.Scores)
                .FirstOrDefaultAsync(e => e.Id == vm.EvaluationId);

            if (evaluation == null)
                return NotFound();

            if (!await CanCurrentCoordinatorAccessEvaluationAsync(evaluation))
            {
                TempData["Error"] = "You are not allowed to update this evaluation.";
                return RedirectToAction(nameof(Index));
            }

            var criteria = await _db.FypEvaluationCriteria
                .Where(c => c.EvaluationType == FypMilestoneType.MidEvaluation && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            foreach (var memberVm in vm.Members)
            {
                var member = evaluation.Members.FirstOrDefault(m => m.Id == memberVm.EvaluationMemberId);
                if (member == null) continue;

                foreach (var scoreVm in memberVm.Scores)
                {
                    var criterion = criteria.FirstOrDefault(c => c.Id == scoreVm.CriterionId);
                    if (criterion == null) continue;

                    var marks = scoreVm.AwardedMarks;
                    if (marks < 0) marks = 0;
                    if (marks > criterion.MaxMarks) marks = criterion.MaxMarks;

                    var existingScore = member.Scores.FirstOrDefault(s => s.CriterionId == scoreVm.CriterionId);

                    if (existingScore == null)
                    {
                        member.Scores.Add(new FypEvaluationMemberScore
                        {
                            CriterionId = scoreVm.CriterionId,
                            AwardedMarks = marks,
                            Comment = scoreVm.Comment
                        });
                    }
                    else
                    {
                        existingScore.AwardedMarks = marks;
                        existingScore.Comment = scoreVm.Comment;
                    }
                }

                member.TotalMarks = member.Scores.Sum(x => x.AwardedMarks);

                var maxTotal = criteria.Sum(x => x.MaxMarks);

                member.WeightedMarks = maxTotal <= 0 || evaluation.WeightagePercent <= 0
                    ? 0
                    : Math.Round((member.TotalMarks / maxTotal) * evaluation.WeightagePercent, 2);

                member.Remarks = memberVm.Remarks;
            }

            evaluation.TotalMarks = evaluation.Members.Any()
                ? Math.Round(evaluation.Members.Average(x => x.TotalMarks), 2)
                : 0;

            evaluation.WeightedMarks = evaluation.Members.Any()
                ? Math.Round(evaluation.Members.Average(x => x.WeightedMarks), 2)
                : 0;

            evaluation.EvaluatorName = vm.EvaluatorName;
            evaluation.Remarks = vm.OverallRemarks;
            evaluation.IsSubmitted = true;
            evaluation.SubmittedAt = DateTime.Now;
            evaluation.Status = FypEvaluationStatus.Completed;
            evaluation.IsPublishedToStudent = false;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Per-member evaluation saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var evaluation = await _db.FypEvaluations
                .Include(e => e.StudentGroup)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evaluation == null)
                return NotFound();

            if (!await CanCurrentCoordinatorAccessEvaluationAsync(evaluation))
            {
                TempData["Error"] = "You are not allowed to publish this evaluation.";
                return RedirectToAction(nameof(Index));
            }

            if (evaluation.Status != FypEvaluationStatus.Completed)
            {
                TempData["Error"] = "Only completed evaluations can be published.";
                return RedirectToAction(nameof(Index));
            }

            evaluation.IsPublishedToStudent = true;
            evaluation.Status = FypEvaluationStatus.Published;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Evaluation result published successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<string>> GetCurrentCoordinatorProgramCodesAsync()
        {
            var currentEmail = GetCurrentUserEmail();

            var committee = await _db.FypCommittees
                .Include(x => x.CommitteePrograms)
                .FirstOrDefaultAsync(x =>
                    x.CoordinatorEmail.ToLower() == currentEmail &&
                    x.IsActive);

            if (committee == null)
                return new List<string>();

            return committee.CommitteePrograms
                .Select(x => x.ProgramCode.Trim().ToUpper())
                .Distinct()
                .ToList();
        }

        private async Task<bool> CanCurrentCoordinatorAccessEvaluationAsync(FypEvaluation evaluation)
        {
            var programCodes = await GetCurrentCoordinatorProgramCodesAsync();

            if (!programCodes.Any())
                return false;

            var groupProgram = evaluation.StudentGroup?.ProgramCode?.Trim().ToUpper();

            return !string.IsNullOrWhiteSpace(groupProgram) &&
                   programCodes.Contains(groupProgram);
        }



        private async Task EnsureEvaluationMembersExistAsync(FypEvaluation evaluation)
        {
            if (evaluation.Members != null && evaluation.Members.Any())
                return;

            var group = await _db.StudentGroups
                .FirstOrDefaultAsync(g => g.Id == evaluation.StudentGroupId);

            if (group == null)
                return;

            var memberUserIds = new List<string?> { group.LeaderId, group.Member2Id, group.Member3Id }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            foreach (var userId in memberUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId!);
                if (user == null) continue;

                _db.FypEvaluationMembers.Add(new FypEvaluationMember
                {
                    EvaluationId = evaluation.Id,
                    StudentUserId = user.Id,
                    StudentName = !string.IsNullOrWhiteSpace(user.FullName)
                        ? user.FullName
                        : (user.UserName ?? user.Email ?? "Student"),
                    RegistrationNo = user.UserName,
                    TotalMarks = 0,
                    WeightedMarks = 0
                });
            }

            await _db.SaveChangesAsync();
        }

    }
}
