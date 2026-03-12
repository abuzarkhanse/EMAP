using System.Linq;
using System.Threading.Tasks;
using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "FYPCoordinator,Admin")]
    public class FypMilestoneController : Controller
    {
        private readonly EmapDbContext _db;

        public FypMilestoneController(EmapDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var milestones = await _db.FypMilestones
                .OrderBy(x => x.Stage)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return View(milestones);
        }

        public IActionResult Create()
        {
            var vm = new FypMilestoneFormViewModel
            {
                IsActive = true,
                IsOptional = false,
                Stage = FypStage.Fyp1,
                Type = FypMilestoneType.Chapter,
                DisplayOrder = 1
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FypMilestoneFormViewModel vm)
        {
            NormalizeMilestone(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var entity = new FypMilestone
            {
                Title = vm.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
                Stage = vm.Stage,
                Type = vm.Type,
                ChapterNumber = vm.ChapterNumber,
                IsOptional = vm.IsOptional,
                IsActive = vm.IsActive,
                DueDate = vm.DueDate,
                DisplayOrder = vm.DisplayOrder
            };

            _db.FypMilestones.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Milestone created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.FypMilestones.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = new FypMilestoneFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Stage = entity.Stage,
                Type = entity.Type,
                ChapterNumber = entity.ChapterNumber,
                IsOptional = entity.IsOptional,
                IsActive = entity.IsActive,
                DueDate = entity.DueDate,
                DisplayOrder = entity.DisplayOrder
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FypMilestoneFormViewModel vm)
        {
            if (vm.Id == null) return BadRequest();

            NormalizeMilestone(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var entity = await _db.FypMilestones.FindAsync(vm.Id.Value);
            if (entity == null) return NotFound();

            entity.Title = vm.Title.Trim();
            entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
            entity.Stage = vm.Stage;
            entity.Type = vm.Type;
            entity.ChapterNumber = vm.ChapterNumber;
            entity.IsOptional = vm.IsOptional;
            entity.IsActive = vm.IsActive;
            entity.DueDate = vm.DueDate;
            entity.DisplayOrder = vm.DisplayOrder;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Milestone updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _db.FypMilestones.FindAsync(id);
            if (entity == null) return NotFound();

            entity.IsActive = !entity.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Milestone {(entity.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }

        private void NormalizeMilestone(FypMilestoneFormViewModel vm)
        {
            if (vm.Type != FypMilestoneType.Chapter)
            {
                vm.ChapterNumber = null;
            }

            if (vm.Type == FypMilestoneType.PreFinalEvaluation)
            {
                vm.IsOptional = true;
            }
        }
    }
}