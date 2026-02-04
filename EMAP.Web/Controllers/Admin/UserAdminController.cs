using EMAP.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // List all students
        public async Task<IActionResult> Students()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var vm = new StudentListViewModel
            {
                Students = students.ToList(),
                TotalStudents = students.Count
            };

            return View(vm);
        }

        // Confirm delete page (double-check)
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new ConfirmDeleteUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? ""
            };

            return View(vm);
        }

        // Actual delete after confirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ConfirmDeleteUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // NOTE: in future, you can check if this user is in any FYP group before deleting.

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Could not delete the user.";
                return RedirectToAction(nameof(Students));
            }

            TempData["Success"] = $"User {model.Email} deleted.";
            return RedirectToAction(nameof(Students));
        }
    }

    public class StudentListViewModel
    {
        public int TotalStudents { get; set; }
        public List<ApplicationUser> Students { get; set; } = new();
    }

    public class ConfirmDeleteUserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
    }
}
