using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EMAP.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmapDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(EmapDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var user = await _userManager.GetUserAsync(User);

            var vm = new StudentDashboardViewModel
            {
                StudentName = !string.IsNullOrWhiteSpace(user?.FullName)
                    ? user.FullName
                    : (user?.Email ?? User.Identity?.Name ?? "Student")
            };

            var activeCall = await _db.FypCalls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AnnouncementDate)
                .FirstOrDefaultAsync();

            if (activeCall == null)
            {
                vm.HasActiveCall = false;
                vm.CurrentStatusLabel = "No Active FYP Call";
                vm.NextActionTitle = "Wait for FYP Announcement";
                vm.NextActionDescription = "There is no active FYP call right now.";
                vm.NextActionButtonText = "Home";
                vm.NextActionController = "Home";
                vm.NextActionAction = "Index";
                return View(vm);
            }

            vm.HasActiveCall = true;
            vm.ActiveCallTitle = activeCall.Title;
            vm.ActiveBatch = activeCall.Batch;

            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .Include(g => g.FypCall)
                .FirstOrDefaultAsync(g =>
                    g.FypCallId == activeCall.Id &&
                    (g.LeaderId == userId ||
                     g.Member2Id == userId ||
                     g.Member3Id == userId));

            vm.Group = group;

            if (group != null && group.IsFypCompleted)
            {
                vm.CurrentStageLabel = "FYP Completed";
                vm.CurrentStatusLabel = "Final Year Project Completed";
                vm.NextActionTitle = "View Final FYP Record";
                vm.NextActionDescription = "Your Final Year Project has been completed successfully and finalized by the coordinator.";
                vm.NextActionButtonText = "Open FYP Portal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
            }

            if (group == null)
            {
                vm.CurrentStatusLabel = "Group Not Created";
                vm.NextActionTitle = "Create Your FYP Group";
                vm.NextActionDescription = "Start your FYP journey by creating a group in the portal.";
                vm.NextActionButtonText = "Create Group";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "CreateGroup";
                return View(vm);
            }

            vm.CurrentStageLabel = group.CurrentStage == FypStage.Fyp2 ? "FYP-2" : "FYP-1";

            var proposal = await _db.ProposalSubmissions
                .Where(p => p.GroupId == group.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .FirstOrDefaultAsync();

            vm.Proposal = proposal;

            if (proposal != null)
            {
                vm.DefenseSchedule = await _db.ProposalDefenseSchedules
                    .FirstOrDefaultAsync(x => x.ProposalSubmissionId == proposal.Id);

                vm.DefenseEvaluation = await _db.ProposalDefenseEvaluations
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync(x => x.ProposalSubmissionId == proposal.Id);
            }

            vm.OpenChapter = await _db.FypChapterAnnouncements
                .Where(x => x.FypCallId == activeCall.Id &&
                            x.Stage == group.CurrentStage &&
                            x.IsOpen)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            vm.CompletedChaptersCount = await _db.FypChapterSubmissions
                .Include(x => x.ChapterAnnouncement)
                .Where(x => x.GroupId == group.Id)
                .Where(x => x.ChapterAnnouncement.Stage == group.CurrentStage)
                .CountAsync(x => x.Status == ChapterSubmissionStatus.CoordinatorApproved);

            vm.TotalStageChapters = await _db.FypChapterAnnouncements
                .CountAsync(x => x.FypCallId == activeCall.Id && x.Stage == group.CurrentStage);

            vm.PublishedEvaluationsCount = await _db.FypEvaluations
                .Include(x => x.Milestone)
                .CountAsync(x =>
                    x.StudentGroupId == group.Id &&
                    x.IsPublishedToStudent &&
                    x.Milestone.Stage == group.CurrentStage);

            // ===== Status + Next Action =====
            if (group.IsFypCompleted)
            {
                vm.CurrentStatusLabel = "Final Year Project Completed";
                vm.NextActionTitle = "View Final FYP Record";
                vm.NextActionDescription = "All academic requirements have been completed successfully.";
                vm.NextActionButtonText = "Open FYP Portal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
            }

            else if (group.Status == GroupStatus.PendingSupervisorSelection)
            {
                vm.CurrentStatusLabel = "Waiting for Supervisor Approval";
                vm.NextActionTitle = "Wait for Supervisor Approval";
                vm.NextActionDescription = "Your supervisor request has been sent and is awaiting approval.";
                vm.NextActionButtonText = "Open Portal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
            }
            else if (proposal == null)
            {
                vm.CurrentStatusLabel = "Proposal Submission Pending";
                vm.NextActionTitle = "Submit Proposal";
                vm.NextActionDescription = "Your group is approved. Submit your proposal to continue.";
                vm.NextActionButtonText = "Submit Proposal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "SubmitProposal";
                vm.UpcomingTitle = "Proposal Deadline";
                vm.UpcomingDateText = activeCall.ProposalDeadline.ToString("dd MMM yyyy");
            }
            else if (proposal.Status == ProposalStatus.ChangesRequested)
            {
                vm.CurrentStatusLabel = "Proposal Revision Required";
                vm.NextActionTitle = "Resubmit Proposal";
                vm.NextActionDescription = "Your supervisor requested changes in the proposal.";
                vm.NextActionButtonText = "Resubmit";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "SubmitProposal";
            }
            else if (vm.DefenseSchedule == null &&
                     (proposal.Status == ProposalStatus.ApprovedForDefense ||
                      proposal.Status == ProposalStatus.ProposalAccepted))
            {
                vm.CurrentStatusLabel = "Waiting for Defense Slot";
                vm.NextActionTitle = "Wait for Defense Scheduling";
                vm.NextActionDescription = "Your proposal has been accepted. The coordinator will assign your defense slot.";
                vm.NextActionButtonText = "Open Portal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
            }
            else if (vm.DefenseSchedule != null && vm.DefenseEvaluation == null)
            {
                vm.CurrentStatusLabel = "Defense Scheduled";
                vm.NextActionTitle = "Prepare for Defense";
                vm.NextActionDescription = "Your proposal defense has been scheduled. Check slot details and prepare.";
                vm.NextActionButtonText = "View Defense Slot";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
                vm.UpcomingTitle = "Proposal Defense";
                vm.UpcomingDateText = vm.DefenseSchedule.DefenseDate.ToString("dd MMM yyyy");
                vm.UpcomingSecondaryTitle = "Time";
                vm.UpcomingSecondaryDateText = vm.DefenseSchedule.DefenseTime.ToString(@"hh\:mm");
            }
            else if (vm.OpenChapter != null)
            {
                vm.CurrentStatusLabel = $"{vm.CurrentStageLabel} Chapter Work Active";
                vm.NextActionTitle = $"Submit {vm.OpenChapter.ChapterType}";
                vm.NextActionDescription = "A chapter is currently open for your stage. Complete and submit it from the portal.";
                vm.NextActionButtonText = "Open Chapter Work";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
                if (vm.OpenChapter.Deadline.HasValue)
                {
                    vm.UpcomingTitle = "Current Chapter Deadline";
                    vm.UpcomingDateText = vm.OpenChapter.Deadline.Value.ToString("dd MMM yyyy");
                }
            }
            else
            {
                vm.CurrentStatusLabel = $"{vm.CurrentStageLabel} In Progress";
                vm.NextActionTitle = "Open FYP Portal";
                vm.NextActionDescription = "Track your current status, submissions, evaluations, and academic updates.";
                vm.NextActionButtonText = "Open FYP Portal";
                vm.NextActionController = "Fyp";
                vm.NextActionAction = "Index";
            }

            return View(vm);
        }
    }
}
