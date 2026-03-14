using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "FYPCoordinator,Admin")]
    public class FypEvaluationController : Controller
    {
        private readonly EmapDbContext _db;

        public FypEvaluationController(EmapDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var evaluations = await _db.FypEvaluations
                .Include(x => x.StudentGroup)
                .Include(x => x.Milestone)
                .OrderByDescending(x => x.ScheduledAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return View(evaluations);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new FypEvaluationFormViewModel
            {
                IsPublishedToStudent = false,
                ShowCommitteeToStudent = false
            };

            await LoadOptions(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FypEvaluationFormViewModel vm)
        {
            ValidatePublishingRules(vm);

            if (!ModelState.IsValid)
            {
                await LoadOptions(vm);
                return View(vm);
            }

            var entity = new FypEvaluation
            {
                StudentGroupId = vm.StudentGroupId,
                MilestoneId = vm.MilestoneId,
                ScheduledAt = vm.ScheduledAt,
                Venue = string.IsNullOrWhiteSpace(vm.Venue) ? null : vm.Venue.Trim(),
                Instructions = string.IsNullOrWhiteSpace(vm.Instructions) ? null : vm.Instructions.Trim(),
                CommitteeMembers = string.IsNullOrWhiteSpace(vm.CommitteeMembers) ? null : vm.CommitteeMembers.Trim(),
                IsPublishedToStudent = vm.IsPublishedToStudent,
                ShowCommitteeToStudent = vm.ShowCommitteeToStudent,
                Status = FypEvaluationStatus.Scheduled,
                IsSubmitted = false
            };

            _db.FypEvaluations.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Evaluation scheduled successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.FypEvaluations.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = new FypEvaluationFormViewModel
            {
                Id = entity.Id,
                StudentGroupId = entity.StudentGroupId,
                MilestoneId = entity.MilestoneId,
                ScheduledAt = entity.ScheduledAt,
                Venue = entity.Venue,
                Instructions = entity.Instructions,
                CommitteeMembers = entity.CommitteeMembers,
                IsPublishedToStudent = entity.IsPublishedToStudent,
                ShowCommitteeToStudent = entity.ShowCommitteeToStudent
            };

            await LoadOptions(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FypEvaluationFormViewModel vm)
        {
            if (vm.Id == null) return BadRequest();

            ValidatePublishingRules(vm);

            if (!ModelState.IsValid)
            {
                await LoadOptions(vm);
                return View(vm);
            }

            var entity = await _db.FypEvaluations.FindAsync(vm.Id.Value);
            if (entity == null) return NotFound();

            entity.StudentGroupId = vm.StudentGroupId;
            entity.MilestoneId = vm.MilestoneId;
            entity.ScheduledAt = vm.ScheduledAt;
            entity.Venue = string.IsNullOrWhiteSpace(vm.Venue) ? null : vm.Venue.Trim();
            entity.Instructions = string.IsNullOrWhiteSpace(vm.Instructions) ? null : vm.Instructions.Trim();
            entity.CommitteeMembers = string.IsNullOrWhiteSpace(vm.CommitteeMembers) ? null : vm.CommitteeMembers.Trim();
            entity.IsPublishedToStudent = vm.IsPublishedToStudent;
            entity.ShowCommitteeToStudent = vm.ShowCommitteeToStudent;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Evaluation updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadOptions(FypEvaluationFormViewModel vm)
        {
            vm.GroupOptions = await _db.StudentGroups
                .Where(g =>
                    g.Status == GroupStatus.Approved &&
                    _db.FypChapterSubmissions.Any(s =>
                        s.GroupId == g.Id &&
                        s.Status == ChapterSubmissionStatus.CoordinatorApproved))
                .OrderBy(g => g.Id)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = $"Group #{g.Id} - {g.TentativeProjectTitle}"
                })
                .ToListAsync();

            vm.MilestoneOptions = await _db.FypMilestones
                .Where(x => x.Type != FypMilestoneType.Chapter)
                .OrderBy(x => x.Stage)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = $"{x.Stage} - {x.Title}"
                })
                .ToListAsync();
        }

        private void ValidatePublishingRules(FypEvaluationFormViewModel vm)
        {
            if (!vm.IsPublishedToStudent)
            {
                vm.ShowCommitteeToStudent = false;
            }

            if (vm.ShowCommitteeToStudent && string.IsNullOrWhiteSpace(vm.CommitteeMembers))
            {
                ModelState.AddModelError(nameof(vm.CommitteeMembers), "Enter committee members if you want to show them to students.");
            }
        }
    }
}