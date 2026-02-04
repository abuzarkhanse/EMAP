using System.Security.Claims;
using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "FYPCoordinator")]
    public class DefenseCommitteeController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DefenseCommitteeController(EmapDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ===================== LIST + SEARCH =====================

        public async Task<IActionResult> Index(string? q, DateTime? date)
        {
            var targetDate = (date ?? DateTime.Today).Date;

            // Get scheduled proposals for the selected date + also allow already evaluated on that date
            var proposals = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.DefenseSchedule)
                .Include(p => p.DefenseEvaluation)
                .Where(p =>
                    p.DefenseSchedule != null &&
                    p.DefenseSchedule.DefenseDate == targetDate)
                .OrderBy(p => p.DefenseSchedule!.DefenseTime)
                .ToListAsync();

            // Build member display for search + UI
            var allIds = proposals
                .SelectMany(p => new[] { p.Group.LeaderId, p.Group.Member2Id, p.Group.Member3Id })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            var userMap = users.ToDictionary(
                x => x.Id,
                x => string.IsNullOrWhiteSpace(x.UserName) ? (x.Email ?? x.Id) : x.UserName!);

            var items = proposals.Select(p =>
            {
                var leader = p.Group.LeaderId != null && userMap.ContainsKey(p.Group.LeaderId) ? userMap[p.Group.LeaderId] : p.Group.LeaderId ?? "-";
                var m2 = p.Group.Member2Id != null && userMap.ContainsKey(p.Group.Member2Id) ? userMap[p.Group.Member2Id] : p.Group.Member2Id;
                var m3 = p.Group.Member3Id != null && userMap.ContainsKey(p.Group.Member3Id) ? userMap[p.Group.Member3Id] : p.Group.Member3Id;

                var members = leader;
                if (!string.IsNullOrWhiteSpace(m2)) members += ", " + m2;
                if (!string.IsNullOrWhiteSpace(m3)) members += ", " + m3;

                return new DefenseListItemVm
                {
                    ProposalId = p.Id,
                    Title = p.Title ?? "Untitled Proposal",
                    Batch = p.Group.FypCall?.Batch ?? "-",
                    GroupId = p.GroupId,
                    Members = members,
                    Time = p.DefenseSchedule!.DefenseTime.ToString(@"hh\:mm"),
                    DurationMinutes = p.DefenseSchedule!.DurationMinutes,
                    Venue = p.DefenseSchedule!.Venue,
                    Status = p.Status.ToString(),
                    HasEvaluation = p.DefenseEvaluation != null,
                    IsPresent = p.DefenseEvaluation?.IsPresent,
                    Decision = p.DefenseEvaluation?.Decision.ToString()
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();

                items = items.Where(x =>
                        (x.Title ?? "").ToLower().Contains(term) ||
                        (x.Members ?? "").ToLower().Contains(term) ||
                        x.GroupId.ToString().Contains(term) ||
                        x.ProposalId.ToString().Contains(term))
                    .ToList();
            }

            var vm = new DefenseIndexVm
            {
                Date = targetDate,
                Query = q,
                Items = items
            };

            return View(vm);
        }

        // ===================== EVALUATE (GET) =====================

        public async Task<IActionResult> Evaluate(int id)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.DefenseSchedule)
                .Include(p => p.DefenseEvaluation)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();
            if (proposal.DefenseSchedule == null) return BadRequest("No defense schedule found for this proposal.");

            // member display
            var ids = new[] { proposal.Group.LeaderId, proposal.Group.Member2Id, proposal.Group.Member3Id }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            string Display(string? id2)
            {
                if (string.IsNullOrWhiteSpace(id2)) return "-";
                var u = users.FirstOrDefault(x => x.Id == id2);
                return u == null ? id2 : (!string.IsNullOrWhiteSpace(u.UserName) ? u.UserName! : (u.Email ?? id2));
            }

            var leader = Display(proposal.Group.LeaderId);
            var m2 = Display(proposal.Group.Member2Id);
            var m3 = Display(proposal.Group.Member3Id);

            var vm = new DefenseEvaluateVm
            {
                ProposalId = proposal.Id,
                Title = proposal.Title ?? "Untitled Proposal",
                Batch = proposal.Group.FypCall?.Batch ?? "-",
                GroupId = proposal.GroupId,

                Leader = leader,
                Member2 = m2,
                Member3 = m3,

                DefenseDate = proposal.DefenseSchedule.DefenseDate,
                DefenseTime = proposal.DefenseSchedule.DefenseTime.ToString(@"hh\:mm"),
                DurationMinutes = proposal.DefenseSchedule.DurationMinutes,
                Venue = proposal.DefenseSchedule.Venue,
                Instructions = proposal.DefenseSchedule.Instructions,

                // existing evaluation (edit support)
                IsPresent = proposal.DefenseEvaluation?.IsPresent ?? true,
                Decision = proposal.DefenseEvaluation?.Decision ?? DefenseDecision.Accepted,
                Feedback = proposal.DefenseEvaluation?.Feedback
            };

            return View(vm);
        }

        // ===================== EVALUATE (POST) =====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate(DefenseEvaluateVm vm)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.DefenseSchedule)
                .Include(p => p.DefenseEvaluation)
                .FirstOrDefaultAsync(p => p.Id == vm.ProposalId);

            if (proposal == null)
                return NotFound();

            if (proposal.DefenseSchedule == null)
                return BadRequest("No defense schedule found.");

            // Keep date BEFORE any possible removal
            var redirectDate = proposal.DefenseSchedule.DefenseDate;

            // Validation: absent requires feedback
            if (!vm.IsPresent && string.IsNullOrWhiteSpace(vm.Feedback))
            {
                ModelState.AddModelError(nameof(vm.Feedback),
                    "Feedback is required when group is absent.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            // ================= SAVE / UPDATE EVALUATION =================
            if (proposal.DefenseEvaluation == null)
            {
                proposal.DefenseEvaluation = new ProposalDefenseEvaluation
                {
                    ProposalSubmissionId = proposal.Id,
                    IsPresent = vm.IsPresent,
                    Decision = vm.Decision,
                    Feedback = vm.Feedback,
                    EvaluatedById = CurrentUserId,
                    EvaluatedAt = DateTime.UtcNow
                };

                _db.ProposalDefenseEvaluations.Add(proposal.DefenseEvaluation);
            }
            else
            {
                proposal.DefenseEvaluation.IsPresent = vm.IsPresent;
                proposal.DefenseEvaluation.Decision = vm.Decision;
                proposal.DefenseEvaluation.Feedback = vm.Feedback;
                proposal.DefenseEvaluation.EvaluatedById = CurrentUserId;
                proposal.DefenseEvaluation.EvaluatedAt = DateTime.UtcNow;
            }

            // ================= UPDATE PROPOSAL STATUS =================
            switch (vm.Decision)
            {
                case DefenseDecision.Accepted:
                    proposal.Status = ProposalStatus.ProposalAccepted;
                    // ✅ defense complete → keep schedule + evaluation
                    break;

                case DefenseDecision.AcceptedWithChanges:
                    proposal.Status = ProposalStatus.DefenseChangesRequired;
                    // ✅ keep schedule so student can view feedback
                    break;

                case DefenseDecision.Rejected:
                    proposal.Status = ProposalStatus.ApprovedForDefense;

                    // 🔥 CRITICAL FIX:
                    // remove old schedule so coordinator can reassign slot
                    if (proposal.DefenseSchedule != null)
                    {
                        _db.ProposalDefenseSchedules.Remove(proposal.DefenseSchedule);
                    }
                    break;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Defense evaluation saved successfully.";

            return RedirectToAction(nameof(Index), new { date = redirectDate });
        }

    }

    // ===================== VIEW MODELS =====================

    public class DefenseIndexVm
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public string? Query { get; set; }
        public List<DefenseListItemVm> Items { get; set; } = new List<DefenseListItemVm>();
    }

    public class DefenseListItemVm
    {
        public int ProposalId { get; set; }
        public int GroupId { get; set; }

        public string? Title { get; set; }
        public string? Batch { get; set; }
        public string? Members { get; set; }

        public string? Time { get; set; }
        public int DurationMinutes { get; set; }
        public string? Venue { get; set; }

        public string? Status { get; set; }

        public bool HasEvaluation { get; set; }
        public bool? IsPresent { get; set; }
        public string? Decision { get; set; }
    }

    public class DefenseEvaluateVm
    {
        public int ProposalId { get; set; }
        public int GroupId { get; set; }

        public string Title { get; set; } = "";
        public string Batch { get; set; } = "";

        public string Leader { get; set; } = "";
        public string Member2 { get; set; } = "";
        public string Member3 { get; set; } = "";

        public DateTime DefenseDate { get; set; }
        public string DefenseTime { get; set; } = "";
        public int DurationMinutes { get; set; }
        public string? Venue { get; set; }
        public string? Instructions { get; set; }

        public bool IsPresent { get; set; } = true;
        public DefenseDecision Decision { get; set; } = DefenseDecision.Accepted;
        public string? Feedback { get; set; }
    }
}

