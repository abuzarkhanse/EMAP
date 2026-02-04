using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EmapDbContext _db;

        public AdminController(EmapDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalCalls = await _db.FypCalls.CountAsync();
            var activeCalls = await _db.FypCalls.CountAsync(c => c.IsActive);

            var supervisors = await _db.FypSupervisors
                .Select(s => new { s.MaxSlots, s.CurrentSlots, s.IsActive })
                .ToListAsync();

            var totalSupervisors = supervisors.Count;
            var activeSupervisors = supervisors.Count(s => s.IsActive);
            var totalSlots = supervisors.Sum(s => s.MaxSlots);
            var usedSlots = supervisors.Sum(s => s.CurrentSlots);

            var totalGroups = await _db.StudentGroups.CountAsync();
            var pendingRequests = await _db.StudentGroups.CountAsync(
                g => g.Status == GroupStatus.PendingSupervisorApproval);

            var proposals = await _db.ProposalSubmissions
                .Select(p => p.Status)
                .ToListAsync();

            var pendingProposals = proposals.Count(p => p == ProposalStatus.PendingReview);
            var changesRequested = proposals.Count(p => p == ProposalStatus.ChangesRequested);
            var approvedForDefense = proposals.Count(p => p == ProposalStatus.ApprovedForDefense);

            var vm = new AdminDashboardViewModel
            {
                TotalCalls = totalCalls,
                ActiveCalls = activeCalls,
                TotalSupervisors = totalSupervisors,
                ActiveSupervisors = activeSupervisors,
                TotalSupervisorSlots = totalSlots,
                UsedSupervisorSlots = usedSlots,
                TotalGroups = totalGroups,
                PendingSupervisorRequests = pendingRequests,
                PendingProposals = pendingProposals,
                ChangesRequestedProposals = changesRequested,
                ApprovedProposals = approvedForDefense
            };

            return View(vm);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalCalls { get; set; }
        public int ActiveCalls { get; set; }

        public int TotalSupervisors { get; set; }
        public int ActiveSupervisors { get; set; }
        public int TotalSupervisorSlots { get; set; }
        public int UsedSupervisorSlots { get; set; }

        public int TotalGroups { get; set; }
        public int PendingSupervisorRequests { get; set; }

        public int PendingProposals { get; set; }
        public int ChangesRequestedProposals { get; set; }
        public int ApprovedProposals { get; set; }
    }
}
