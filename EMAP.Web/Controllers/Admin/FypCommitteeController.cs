using EMAP.Domain.Fyp;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class FypCommitteeController : Controller
    {
        private readonly EmapDbContext _context;

        public FypCommitteeController(EmapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var committees = await _context.FypCommittees
                .Include(x => x.CommitteePrograms)
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Name)
                .ToListAsync();

            return View(committees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new FypCommitteeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FypCommitteeFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var committee = new FypCommittee
            {
                Name = vm.Name.Trim(),
                Session = vm.Session?.Trim() ?? string.Empty,
                CoordinatorEmail = vm.CoordinatorEmail.Trim().ToLower(),
                ConvenorEmail = string.IsNullOrWhiteSpace(vm.ConvenorEmail) ? null : vm.ConvenorEmail.Trim().ToLower(),
                IsActive = vm.IsActive
            };

            var programCodes = vm.ProgramCodesCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToUpper())
                .Distinct()
                .ToList();

            foreach (var code in programCodes)
            {
                committee.CommitteePrograms.Add(new FypCommitteeProgram
                {
                    ProgramCode = code
                });
            }

            _context.FypCommittees.Add(committee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Committee created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
