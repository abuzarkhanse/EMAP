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
    public class FypEvaluationCriteriaController : Controller
    {
        private readonly EmapDbContext _db;

        public FypEvaluationCriteriaController(EmapDbContext db)
        {
            _db = db;
        }

        private List<FypMilestoneType> AllowedTypes()
        {
            return new List<FypMilestoneType>
            {
                FypMilestoneType.MidEvaluation,
                FypMilestoneType.PreFinalEvaluation,
                FypMilestoneType.FinalEvaluation
            };
        }

        public async Task<IActionResult> Index(FypMilestoneType? type)
        {
            var allowedTypes = AllowedTypes();

            var query = _db.FypEvaluationCriteria
                .Where(x => allowedTypes.Contains(x.EvaluationType))
                .AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(x => x.EvaluationType == type.Value);
            }

            var data = await query
                .OrderBy(x => x.EvaluationType)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToListAsync();

            ViewBag.TypeFilter = new SelectList(
                allowedTypes.Select(x => new
                {
                    Id = x,
                    Name = GetTypeLabel(x)
                }),
                "Id",
                "Name",
                type
            );

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new FypEvaluationCriterionFormViewModel
            {
                EvaluationType = FypMilestoneType.MidEvaluation,
                MaxMarks = 10,
                DisplayOrder = 1,
                IsActive = true
            };

            PopulateTypeDropdown(vm.EvaluationType);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FypEvaluationCriterionFormViewModel vm)
        {
            if (!AllowedTypes().Contains(vm.EvaluationType))
            {
                ModelState.AddModelError(nameof(vm.EvaluationType), "Invalid evaluation type selected.");
            }

            if (!ModelState.IsValid)
            {
                PopulateTypeDropdown(vm.EvaluationType);
                return View(vm);
            }

            var entity = new FypEvaluationCriterion
            {
                EvaluationType = vm.EvaluationType,
                Title = vm.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
                MaxMarks = vm.MaxMarks,
                DisplayOrder = vm.DisplayOrder,
                IsActive = vm.IsActive
            };

            _db.FypEvaluationCriteria.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Rubric added successfully.";
            return RedirectToAction(nameof(Index), new { type = vm.EvaluationType });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _db.FypEvaluationCriteria.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound();

            var vm = new FypEvaluationCriterionFormViewModel
            {
                Id = entity.Id,
                EvaluationType = entity.EvaluationType,
                Title = entity.Title,
                Description = entity.Description,
                MaxMarks = entity.MaxMarks,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            };

            PopulateTypeDropdown(vm.EvaluationType);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FypEvaluationCriterionFormViewModel vm)
        {
            if (!AllowedTypes().Contains(vm.EvaluationType))
            {
                ModelState.AddModelError(nameof(vm.EvaluationType), "Invalid evaluation type selected.");
            }

            if (!ModelState.IsValid)
            {
                PopulateTypeDropdown(vm.EvaluationType);
                return View(vm);
            }

            var entity = await _db.FypEvaluationCriteria.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (entity == null)
                return NotFound();

            entity.EvaluationType = vm.EvaluationType;
            entity.Title = vm.Title.Trim();
            entity.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
            entity.MaxMarks = vm.MaxMarks;
            entity.DisplayOrder = vm.DisplayOrder;
            entity.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Rubric updated successfully.";
            return RedirectToAction(nameof(Index), new { type = vm.EvaluationType });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var entity = await _db.FypEvaluationCriteria.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound();

            entity.IsActive = !entity.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = entity.IsActive ? "Rubric activated." : "Rubric deactivated.";
            return RedirectToAction(nameof(Index), new { type = entity.EvaluationType });
        }

        private void PopulateTypeDropdown(FypMilestoneType selectedType)
        {
            ViewBag.EvaluationTypes = new SelectList(
                AllowedTypes().Select(x => new
                {
                    Id = x,
                    Name = GetTypeLabel(x)
                }),
                "Id",
                "Name",
                selectedType
            );
        }

        private static string GetTypeLabel(FypMilestoneType type)
        {
            return type switch
            {
                FypMilestoneType.MidEvaluation => "Mid Evaluation",
                FypMilestoneType.PreFinalEvaluation => "Pre-Final Evaluation",
                FypMilestoneType.FinalEvaluation => "Final Evaluation",
                _ => "Evaluation"
            };
        }
    }
}
