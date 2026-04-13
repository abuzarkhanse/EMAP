using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using EMAP.Web.ViewModels;
using EMAP.Web.ViewModels.Fyp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EMAP.Web.Controllers.Admin
{
    [Authorize] // authenticated users only (roles handled per action)
    public class SupervisorController : Controller
    {
        private readonly EmapDbContext _db;

        // NOTE:
        // You are using Email linking in many places (CurrentEmail).
        // Keep it, but also support linking by UserId when available.
        private string? CurrentEmail => User.Identity?.Name;
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        public SupervisorController(EmapDbContext db)
        {
            _db = db;
        }

        // ===================== HELPERS =====================

        private void LoadDepartments()
        {
            ViewBag.Departments = _db.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToList();
        }

        /// <summary>
        /// Finds supervisor record for the logged-in supervisor.
        /// Prefer UserId link; fallback to Email match (legacy).
        /// </summary>
        private async Task<FypSupervisor?> GetCurrentSupervisorAsync()
        {
            var userId = CurrentUserId;
            var email = CurrentEmail;

            // Prefer UserId link (BEST)
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var byUserId = await _db.FypSupervisors.FirstOrDefaultAsync(s => s.UserId == userId);
                if (byUserId != null) return byUserId;
            }

            // Fallback to Email link (LEGACY)
            if (!string.IsNullOrWhiteSpace(email))
            {
                var byEmail = await _db.FypSupervisors.FirstOrDefaultAsync(s => s.Email == email);
                if (byEmail != null) return byEmail;
            }

            return null;
        }

        private void SetNotLinkedWarning(string message)
        {
            ViewBag.Warning = message;
        }

        private void ValidateSupervisorModel(FypSupervisor model, bool isEdit = false, int currentId = 0)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(FypSupervisor.Name), "Name is required.");
            }
            else if (model.Name.Trim().Length < 3)
            {
                ModelState.AddModelError(nameof(FypSupervisor.Name), "Name must be at least 3 characters.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(FypSupervisor.Email), "Email is required.");
            }
            else if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                ModelState.AddModelError(nameof(FypSupervisor.Email), "Please enter a valid email address.");
            }
            else
            {
                bool emailExists = _db.FypSupervisors.Any(s =>
                    s.Email == model.Email &&
                    (!isEdit || s.Id != currentId));

                if (emailExists)
                {
                    ModelState.AddModelError(nameof(FypSupervisor.Email), "This email is already used by another supervisor.");
                }
            }

            if (model.DepartmentId <= 0)
            {
                ModelState.AddModelError(nameof(FypSupervisor.DepartmentId), "Please select a valid department.");
            }

            if (string.IsNullOrWhiteSpace(model.FieldOfExpertise))
            {
                ModelState.AddModelError(nameof(FypSupervisor.FieldOfExpertise), "Field of expertise is required.");
            }
            else if (model.FieldOfExpertise.Trim().Length < 2)
            {
                ModelState.AddModelError(nameof(FypSupervisor.FieldOfExpertise), "Field of expertise must be at least 2 characters.");
            }

            if (model.MaxSlots < 1 || model.MaxSlots > 50)
            {
                ModelState.AddModelError(nameof(FypSupervisor.MaxSlots), "Max FYP slots must be between 1 and 50.");
            }

            if (model.CurrentSlots < 0 || model.CurrentSlots > 50)
            {
                ModelState.AddModelError(nameof(FypSupervisor.CurrentSlots), "Current slots must be between 0 and 50.");
            }

            if (model.CurrentSlots > model.MaxSlots)
            {
                ModelState.AddModelError(nameof(FypSupervisor.CurrentSlots), "Current slots cannot be greater than max slots.");
            }
        }

        // ===================== ADMIN: SUPERVISOR MASTER LIST =====================

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var supervisors = _db.FypSupervisors
                .Select(s => new FypSupervisor
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email ?? "",
                    Department = s.Department ?? "",
                    DepartmentId = s.DepartmentId,
                    FieldOfExpertise = s.FieldOfExpertise ?? "",
                    MaxSlots = s.MaxSlots,
                    CurrentSlots = s.CurrentSlots,
                    IsActive = s.IsActive,
                    UserId = s.UserId
                })
                .OrderBy(s => s.Department)
                .ThenBy(s => s.Name)
                .ToList();

            return View(supervisors);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            LoadDepartments();

            return View(new FypSupervisor
            {
                MaxSlots = 3,
                IsActive = true,
                CurrentSlots = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(FypSupervisor model)
        {
            ModelState.Remove(nameof(FypSupervisor.User));
            ModelState.Remove(nameof(FypSupervisor.UserId));
            ModelState.Remove(nameof(FypSupervisor.Department));
            ModelState.Remove(nameof(FypSupervisor.DepartmentRef));

            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == model.DepartmentId && d.IsActive);

            if (department == null)
            {
                ModelState.AddModelError(nameof(FypSupervisor.DepartmentId), "Please select a valid department.");
            }
            else
            {
                model.Department = department.Name;
            }

            ValidateSupervisorModel(model);

            if (!ModelState.IsValid)
            {
                LoadDepartments();
                return View(model);
            }

            model.UserId = await _db.Users
                .Where(u => u.Email == model.Email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(model.UserId))
            {
                ModelState.AddModelError(nameof(FypSupervisor.Email), "No ASP.NET user exists with this email.");
                LoadDepartments();
                return View(model);
            }

            if (model.CurrentSlots < 0)
                model.CurrentSlots = 0;

            _db.FypSupervisors.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var supervisor = await _db.FypSupervisors.FindAsync(id);
            if (supervisor == null) return NotFound();

            LoadDepartments();
            return View(supervisor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(FypSupervisor model)
        {
            ModelState.Remove(nameof(FypSupervisor.User));
            ModelState.Remove(nameof(FypSupervisor.UserId));
            ModelState.Remove(nameof(FypSupervisor.Department));
            ModelState.Remove(nameof(FypSupervisor.DepartmentRef));

            var department = await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == model.DepartmentId && d.IsActive);

            if (department == null)
            {
                ModelState.AddModelError(nameof(FypSupervisor.DepartmentId), "Please select a valid department.");
            }
            else
            {
                model.Department = department.Name;
            }

            ValidateSupervisorModel(model, true, model.Id);

            if (!ModelState.IsValid)
            {
                LoadDepartments();
                return View(model);
            }

            var supervisor = await _db.FypSupervisors.FindAsync(model.Id);
            if (supervisor == null) return NotFound();

            supervisor.Name = model.Name;
            supervisor.Email = model.Email;
            supervisor.DepartmentId = model.DepartmentId;
            supervisor.Department = model.Department;
            supervisor.FieldOfExpertise = model.FieldOfExpertise;
            supervisor.MaxSlots = model.MaxSlots;
            supervisor.CurrentSlots = model.CurrentSlots;
            supervisor.IsActive = model.IsActive;

            supervisor.UserId = await _db.Users
                .Where(u => u.Email == supervisor.Email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(supervisor.UserId))
            {
                ModelState.AddModelError(nameof(FypSupervisor.Email), "No ASP.NET user exists with this email.");
                LoadDepartments();
                return View(model);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var supervisor = await _db.FypSupervisors.FindAsync(id);
            if (supervisor == null) return NotFound();

            return View(supervisor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supervisor = await _db.FypSupervisors.FindAsync(id);
            if (supervisor == null) return NotFound();

            bool isAssigned = await _db.StudentGroups.AnyAsync(g => g.SupervisorId == id);

            if (isAssigned)
            {
                TempData["Error"] = "This supervisor cannot be deleted because they are assigned to one or more student groups. Please reassign or remove those groups first.";
                return RedirectToAction(nameof(Index));
            }

            _db.FypSupervisors.Remove(supervisor);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== REAL-TIME VALIDATION =====================

        [AcceptVerbs("GET", "POST")]
        [Authorize(Roles = "Admin")]
        public IActionResult VerifyEmail(string email, int? id)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json("Email is required.");

            if (!new EmailAddressAttribute().IsValid(email))
                return Json("Please enter a valid email address.");

            bool exists = _db.FypSupervisors.Any(x =>
                x.Email == email &&
                (!id.HasValue || x.Id != id.Value));

            if (exists)
                return Json("This email is already used by another supervisor.");

            bool userExists = _db.Users.Any(u => u.Email == email);
            if (!userExists)
                return Json("No ASP.NET user exists with this email.");

            return Json(true);
        }

        // ===================== BULK UPLOAD =====================

        [Authorize(Roles = "Admin")]
        public IActionResult BulkUpload()
        {
            return View(new BulkSupervisorUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkUpload(BulkSupervisorUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(nameof(model.File), "Please select an Excel file.");
                return View(model);
            }

            var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
            {
                ModelState.AddModelError(nameof(model.File), "Only .xlsx files are allowed.");
                return View(model);
            }

            var supervisorsToAdd = new List<FypSupervisor>();
            var errors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await model.File.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var usedRange = worksheet.RangeUsed();

                    if (usedRange == null)
                    {
                        ModelState.AddModelError(nameof(model.File), "The uploaded Excel file is empty.");
                        return View(model);
                    }

                    var rows = usedRange.RowsUsed().Skip(1);
                    int rowNumber = 2;

                    foreach (var row in rows)
                    {
                        string name = row.Cell(1).GetString().Trim();
                        string email = row.Cell(2).GetString().Trim();
                        string department = row.Cell(3).GetString().Trim();
                        string fieldOfExpertise = row.Cell(4).GetString().Trim();
                        string maxSlotsText = row.Cell(5).GetString().Trim();
                        string currentSlotsText = row.Cell(6).GetString().Trim();
                        string isActiveText = row.Cell(7).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Row {rowNumber}: Name is required.");
                            rowNumber++;
                            continue;
                        }

                        if (name.Length < 3)
                        {
                            errors.Add($"Row {rowNumber}: Name must be at least 3 characters.");
                            rowNumber++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
                        {
                            errors.Add($"Row {rowNumber}: A valid email is required.");
                            rowNumber++;
                            continue;
                        }

                        bool supervisorEmailExists = _db.FypSupervisors.Any(x => x.Email == email) ||
                                                     supervisorsToAdd.Any(x => x.Email == email);
                        if (supervisorEmailExists)
                        {
                            errors.Add($"Row {rowNumber}: Supervisor email '{email}' already exists.");
                            rowNumber++;
                            continue;
                        }

                        var userId = await _db.Users
                            .Where(u => u.Email == email)
                            .Select(u => u.Id)
                            .FirstOrDefaultAsync();

                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            errors.Add($"Row {rowNumber}: No ASP.NET user exists with email '{email}'.");
                            rowNumber++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(department))
                        {
                            errors.Add($"Row {rowNumber}: Department is required.");
                            rowNumber++;
                            continue;
                        }

                        var departmentEntity = await _db.Departments
                            .FirstOrDefaultAsync(d =>
                                d.Name.ToLower() == department.ToLower() ||
                                d.Code.ToLower() == department.ToLower());

                        if (departmentEntity == null)
                        {
                            errors.Add($"Row {rowNumber}: Department '{department}' was not found.");
                            rowNumber++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(fieldOfExpertise))
                        {
                            errors.Add($"Row {rowNumber}: Field of expertise is required.");
                            rowNumber++;
                            continue;
                        }

                        if (fieldOfExpertise.Length < 2)
                        {
                            errors.Add($"Row {rowNumber}: Field of expertise must be at least 2 characters.");
                            rowNumber++;
                            continue;
                        }

                        if (!int.TryParse(maxSlotsText, out int maxSlots) || maxSlots < 1 || maxSlots > 50)
                        {
                            errors.Add($"Row {rowNumber}: Max slots must be a number between 1 and 50.");
                            rowNumber++;
                            continue;
                        }

                        if (!int.TryParse(currentSlotsText, out int currentSlots) || currentSlots < 0 || currentSlots > 50)
                        {
                            errors.Add($"Row {rowNumber}: Current slots must be a number between 0 and 50.");
                            rowNumber++;
                            continue;
                        }

                        if (currentSlots > maxSlots)
                        {
                            errors.Add($"Row {rowNumber}: Current slots cannot be greater than max slots.");
                            rowNumber++;
                            continue;
                        }

                        bool isActive = true;
                        if (!string.IsNullOrWhiteSpace(isActiveText))
                        {
                            var normalized = isActiveText.Trim().ToLower();
                            isActive = normalized == "true" || normalized == "yes" || normalized == "1" || normalized == "active";
                        }

                        supervisorsToAdd.Add(new FypSupervisor
                        {
                            Name = name,
                            Email = email,
                            Department = departmentEntity.Name,
                            DepartmentId = departmentEntity.Id,
                            FieldOfExpertise = fieldOfExpertise,
                            MaxSlots = maxSlots,
                            CurrentSlots = currentSlots,
                            IsActive = isActive,
                            UserId = userId
                        });

                        rowNumber++;
                    }
                }
            }

            if (supervisorsToAdd.Any())
            {
                _db.FypSupervisors.AddRange(supervisorsToAdd);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"{supervisorsToAdd.Count} supervisor(s) uploaded successfully.";
            }

            if (errors.Any())
            {
                ViewBag.Errors = errors;
            }

            return View(new BulkSupervisorUploadViewModel());
        }

        // ===================== SUPERVISOR PORTAL =====================

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Dashboard()
        {
            var supervisor = await GetCurrentSupervisorAsync();
            if (supervisor == null)
            {
                SetNotLinkedWarning(
                    "Your login is not linked to any FYP supervisor record. " +
                    "Ask the admin to set your Email and/or link your UserId in the FYP Supervisors list."
                );
                return View(new SupervisorDashboardViewModel());
            }

            var myGroups = await _db.StudentGroups
                .Include(g => g.FypCall)
                .Where(g => g.SupervisorId == supervisor.Id)
                .ToListAsync();

            var pendingRequests = myGroups.Count(g => g.Status == GroupStatus.PendingSupervisorApproval);
            var activeGroups = myGroups.Count(g => g.Status == GroupStatus.Approved);

            var myProposals = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor)
                .Where(p => p.Group.SupervisorId == supervisor.Id)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();

            var pendingProposals = myProposals.Count(p => p.Status == ProposalStatus.PendingReview);
            var changesRequested = myProposals.Count(p => p.Status == ProposalStatus.ChangesRequested);
            var approvedForDefense = myProposals.Count(p => p.Status == ProposalStatus.ApprovedForDefense);

            // ✅ IMPORTANT:
            // Your FypChapterSubmission.SupervisorId is stored as ASP.NET UserId (string)
            // So count by supervisor.UserId (string). Fallback to CurrentUserId if needed.
            var supervisorUserId = !string.IsNullOrWhiteSpace(supervisor.UserId) ? supervisor.UserId : CurrentUserId;

            var pendingChapterReviews = 0;
            if (!string.IsNullOrWhiteSpace(supervisorUserId))
            {
                pendingChapterReviews = await _db.FypChapterSubmissions.CountAsync(s =>
                    s.SupervisorId == supervisorUserId &&
                    (s.Status == ChapterSubmissionStatus.Submitted || s.Status == ChapterSubmissionStatus.Resubmitted)
                );
            }

            ViewBag.PendingChapterReviews = pendingChapterReviews;

            var vm = new SupervisorDashboardViewModel
            {
                PendingRequests = pendingRequests,
                ActiveGroups = activeGroups,
                PendingProposals = pendingProposals,
                ChangesRequestedProposals = changesRequested,
                ApprovedForDefenseProposals = approvedForDefense,
                Groups = myGroups,
                RecentProposals = myProposals.Take(5).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> PendingRequests()
        {
            var supervisor = await GetCurrentSupervisorAsync();

            var baseQuery = _db.StudentGroups
                .Include(g => g.FypCall)
                .Include(g => g.Supervisor)
                .Where(g => g.Status == GroupStatus.PendingSupervisorApproval);

            List<StudentGroup> pending;

            if (supervisor != null)
            {
                pending = await baseQuery
                    .Where(g => g.SupervisorId == supervisor.Id)
                    .OrderBy(g => g.FypCall.Batch)
                    .ToListAsync();

                if (!pending.Any())
                {
                    SetNotLinkedWarning(
                        "No pending requests found for your supervisor profile. Showing ALL pending groups (debug mode). " +
                        "Check that the FYP Supervisor record used by students has your email/UserId and that groups selected you."
                    );

                    pending = await baseQuery
                        .OrderBy(g => g.FypCall.Batch)
                        .ToListAsync();
                }
            }
            else
            {
                SetNotLinkedWarning(
                    "Your login is NOT linked to any record in FYP Supervisors. Admin must set your Email/UserId there. " +
                    "Currently showing ALL pending groups."
                );

                pending = await baseQuery
                    .OrderBy(g => g.FypCall.Batch)
                    .ToListAsync();
            }

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(int groupId)
        {
            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null || group.Supervisor == null)
            {
                TempData["Error"] = "Group or supervisor not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var sup = group.Supervisor;

            if (sup.CurrentSlots >= sup.MaxSlots)
            {  
                TempData["Error"] = $"Supervisor {sup.Name} has no remaining slots.";
                return RedirectToAction(nameof(PendingRequests));
            }

            group.Status = GroupStatus.Approved;
            sup.CurrentSlots += 1;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Group approved successfully.";
            return RedirectToAction(nameof(PendingRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Reject(int groupId)
        {
            var group = await _db.StudentGroups
                .Include(g => g.Supervisor)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                TempData["Error"] = "Group not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            group.SupervisorId = null;
            group.Status = GroupStatus.PendingSupervisorSelection;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Request rejected. Students can choose another supervisor.";
            return RedirectToAction(nameof(PendingRequests));
        }

        // ===================== PROPOSALS =====================

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Proposals()
        {
            var supervisor = await GetCurrentSupervisorAsync();

            var baseQuery = _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor);

            List<ProposalSubmission> proposals;

            if (supervisor != null)
            {
                proposals = await baseQuery
                    .Where(p => p.Group.SupervisorId == supervisor.Id)
                    .OrderByDescending(p => p.SubmittedAt)
                    .ToListAsync();

                if (!proposals.Any())
                {
                    SetNotLinkedWarning(
                        "No proposals found for your groups. Showing ALL proposals (debug) so you can verify data linking."
                    );

                    proposals = await baseQuery
                        .OrderByDescending(p => p.SubmittedAt)
                        .ToListAsync();
                }
            }
            else
            {
                SetNotLinkedWarning(
                    "Your login is NOT linked to any FYP Supervisor record. Admin must set your Email/UserId in the FYP Supervisors list. Currently showing ALL proposals."
                );

                proposals = await baseQuery
                    .OrderByDescending(p => p.SubmittedAt)
                    .ToListAsync();
            }

            return View(proposals);
        }

        [Authorize(Roles = "Supervisor")]
        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                    .ThenInclude(g => g.FypCall)
                .Include(p => p.Group)
                    .ThenInclude(g => g.Supervisor)
                .Include(p => p.DefenseSchedule)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.Group != null &&
                    p.Group.Supervisor != null &&
                    p.Group.Supervisor.UserId == userId);

            if (proposal == null)
            {
                TempData["Error"] = "Proposal not found or you are not allowed to review it.";
                return RedirectToAction(nameof(Proposals));
            }

            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveForDefense(int id)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null)
                return NotFound();

            if (proposal.Status == ProposalStatus.ApprovedForDefense ||
                proposal.Status == ProposalStatus.DefenseScheduled)
            {
                TempData["Error"] = "This proposal is already approved/scheduled. No further action is allowed.";
                return RedirectToAction(nameof(Review), new { id });
            }

            proposal.Status = ProposalStatus.ApprovedForDefense;
            proposal.SupervisorFeedback = null;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Proposal approved for defense scheduling.";
            return RedirectToAction(nameof(Proposals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> RequestChanges(int id, string feedback)
        {
            var proposal = await _db.ProposalSubmissions
                .Include(p => p.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proposal == null) return NotFound();

            if (proposal.Status == ProposalStatus.ApprovedForDefense ||
                proposal.Status == ProposalStatus.DefenseScheduled)
            {
                TempData["Error"] = "You cannot request changes after approval/scheduling.";
                return RedirectToAction(nameof(Review), new { id });
            }

            proposal.Status = ProposalStatus.ChangesRequested;
            proposal.SupervisorFeedback = feedback;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Feedback saved. Student must revise and resubmit.";
            return RedirectToAction(nameof(Proposals));
        }


        // ===================== CHAPTER REVIEWS (SUPERVISOR) =====================
        //
        // IMPORTANT:
        // Your student submission stores SupervisorId as ASP.NET UserId (string).
        // So here we must compare SupervisorId == supervisor.UserId (string), NOT supervisor.Id (int).
        //
        // Also: include Resubmitted to show resubmissions.
        //

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ChapterReviews()
        {
            var supervisor = await GetCurrentSupervisorAsync();
            if (supervisor == null)
            {
                SetNotLinkedWarning("Supervisor profile not found / not linked.");
                return View(new List<FypChapterSubmission>());
            }

            var supervisorUserId = !string.IsNullOrWhiteSpace(supervisor.UserId)
                ? supervisor.UserId
                : CurrentUserId;

            if (string.IsNullOrWhiteSpace(supervisorUserId))
                return View(new List<FypChapterSubmission>());

            var submissions = await _db.FypChapterSubmissions
                .Include(s => s.Group)
                .Include(s => s.ChapterAnnouncement)
                .Where(s =>
                    s.SupervisorId == supervisorUserId &&
                    (s.Status == ChapterSubmissionStatus.Submitted ||
                     s.Status == ChapterSubmissionStatus.Resubmitted))
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ReviewChapter(
            int submissionId,
            bool approve,
            string? feedback)
        {
            var submission = await _db.FypChapterSubmissions
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
            {
                TempData["Error"] = "Chapter submission not found.";
                return RedirectToAction(nameof(ChapterReviews));
            }

            // ================= SUPERVISOR APPROVES =================
            if (approve)
            {
                submission.Status = ChapterSubmissionStatus.SupervisorApproved;
                submission.Feedback = null;

                TempData["Success"] =
                    "Chapter approved successfully and forwarded to the coordinator for final review.";
            }
            // ================= REQUEST CHANGES =================
            else
            {
                if (string.IsNullOrWhiteSpace(feedback))
                {
                    TempData["Error"] =
                        "Please provide feedback before requesting changes.";
                    return RedirectToAction(nameof(ChapterReviews));
                }

                submission.Status = ChapterSubmissionStatus.ChangesRequested;
                submission.Feedback = feedback;

                TempData["Success"] =
                    "Feedback sent to student. Waiting for resubmission.";
            }

            submission.ReviewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ChapterReviews));
        }

    }
}