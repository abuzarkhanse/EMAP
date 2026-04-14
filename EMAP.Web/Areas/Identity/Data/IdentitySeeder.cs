using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EMAP.Infrastructure.Data;

namespace EMAP.Web.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<EMAP.Infrastructure.Data.EmapDbContext>();

            // Seed roles
            string[] roles =
            {
                "Admin",
                "Student",
                "Supervisor",
                "FYPCoordinator",
                "Reviewer",
                "HEDOfficial",
                "DOSTOfficial"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed departments
            if (!await db.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "School of Computing Sciences", Code = "SCS", IsActive = true },
                    new Department { Name = "Electrical Engineering", Code = "EE", IsActive = true },
                    new Department { Name = "Mechanical Engineering", Code = "ME", IsActive = true },
                    new Department { Name = "English", Code = "ENG", IsActive = true },
                    new Department { Name = "Pharmacy", Code = "PHARM", IsActive = true },
                    new Department { Name = "Bio-Medical Sciences", Code = "BMS", IsActive = true },
                    new Department { Name = "Chemical Engineering", Code = "CHE", IsActive = true },
                    new Department { Name = "Civil Engineering", Code = "CE", IsActive = true },
                    new Department { Name = "Management Sciences", Code = "MS", IsActive = true },
                    new Department { Name = "Mathematics", Code = "MATH", IsActive = true },
                    new Department { Name = "Physics", Code = "PHY", IsActive = true }
                };

                await db.Departments.AddRangeAsync(departments);
                await db.SaveChangesAsync();
            }

            // Load departments
            var scsDepartment = await db.Departments.FirstAsync(d => d.Code == "SCS");
            var eeDepartment = await db.Departments.FirstAsync(d => d.Code == "EE");
            var pharmDepartment = await db.Departments.FirstAsync(d => d.Code == "PHARM");

            // Admin user
            var adminEmail = "admin@emap.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin"
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                admin.FullName = "System Admin";
                await userManager.UpdateAsync(admin);
            }

            // Supervisors
            var supervisors = new List<(string Email, string FullName, int DepartmentId)>
            {
                ("sup1@emap.local", "Dr. Supervisor One", scsDepartment.Id),
                ("sup2@emap.local", "Dr. Supervisor Two", eeDepartment.Id)
            };

            foreach (var item in supervisors)
            {
                var supervisor = await userManager.FindByEmailAsync(item.Email);

                if (supervisor == null)
                {
                    supervisor = new ApplicationUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        FullName = item.FullName,
                        DepartmentId = item.DepartmentId
                    };

                    var result = await userManager.CreateAsync(supervisor, "Supervisor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(supervisor, "Supervisor");
                    }
                }
                else
                {
                    supervisor.FullName = item.FullName;
                    supervisor.DepartmentId = item.DepartmentId;
                    await userManager.UpdateAsync(supervisor);

                    if (!await userManager.IsInRoleAsync(supervisor, "Supervisor"))
                    {
                        await userManager.AddToRoleAsync(supervisor, "Supervisor");
                    }
                }
            }

            // FYP Coordinator
            var coordEmail = "coordinator@emap.local";
            var coord = await userManager.FindByEmailAsync(coordEmail);

            if (coord == null)
            {
                coord = new ApplicationUser
                {
                    UserName = coordEmail,
                    Email = coordEmail,
                    FullName = "FYP Coordinator",
                    DepartmentId = scsDepartment.Id
                };

                var result = await userManager.CreateAsync(coord, "Coordinator@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coord, "FYPCoordinator");
                }
            }
            else
            {
                coord.FullName = "FYP Coordinator";
                coord.DepartmentId = scsDepartment.Id;
                await userManager.UpdateAsync(coord);

                if (!await userManager.IsInRoleAsync(coord, "FYPCoordinator"))
                {
                    await userManager.AddToRoleAsync(coord, "FYPCoordinator");
                }
            }

            // Students
            var students = new List<(string Email, string FullName, string RegNo, int DepartmentId)>
            {
                ("student0@emap.local", "Student Zero", "F20-1000", scsDepartment.Id),
                ("student2@emap.local", "Student Two", "F20-1002", scsDepartment.Id),
                ("student3@emap.local", "Student Three", "F20-1003", eeDepartment.Id),
                ("student22@emap.local", "Student Twenty Two", "F20-1022", pharmDepartment.Id),
                ("student33@emap.local", "Student Thirty Three", "F20-1033", scsDepartment.Id)
            };

            foreach (var item in students)
            {
                var student = await userManager.FindByEmailAsync(item.Email);

                if (student == null)
                {
                    student = new ApplicationUser
                    {
                        UserName = item.Email,
                        Email = item.Email,
                        FullName = item.FullName,
                        RegistrationNumber = item.RegNo,
                        DepartmentId = item.DepartmentId
                    };

                    var result = await userManager.CreateAsync(student, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(student, "Student");
                    }
                }
                else
                {
                    student.FullName = item.FullName;
                    student.RegistrationNumber = item.RegNo;
                    student.DepartmentId = item.DepartmentId;
                    await userManager.UpdateAsync(student);

                    if (!await userManager.IsInRoleAsync(student, "Student"))
                    {
                        await userManager.AddToRoleAsync(student, "Student");
                    }
                }
            }


            // Seed default evaluation criteria
            if (!await db.FypEvaluationCriteria.AnyAsync())
            {
                db.FypEvaluationCriteria.AddRange(
                    new FypEvaluationCriterion
                    {
                        EvaluationType = FypMilestoneType.MidEvaluation,
                        Title = "Problem Understanding",
                        Description = "Understanding of the selected problem and context.",
                        MaxMarks = 10,
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new FypEvaluationCriterion
                    {
                        EvaluationType = FypMilestoneType.MidEvaluation,
                        Title = "Methodology / Approach",
                        Description = "Clarity and suitability of proposed methodology.",
                        MaxMarks = 10,
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new FypEvaluationCriterion
                    {
                        EvaluationType = FypMilestoneType.MidEvaluation,
                        Title = "Progress / Implementation",
                        Description = "Actual progress made in the project so far.",
                        MaxMarks = 10,
                        DisplayOrder = 3,
                        IsActive = true
                    },
                    new FypEvaluationCriterion
                    {
                        EvaluationType = FypMilestoneType.MidEvaluation,
                        Title = "Presentation / Communication",
                        Description = "Communication quality and presentation confidence.",
                        MaxMarks = 10,
                        DisplayOrder = 4,
                        IsActive = true
                    },
                    new FypEvaluationCriterion
                    {
                        EvaluationType = FypMilestoneType.MidEvaluation,
                        Title = "Viva / Questions",
                        Description = "Responses to questions and conceptual command.",
                        MaxMarks = 10,
                        DisplayOrder = 5,
                        IsActive = true
                    }
                );

                await db.SaveChangesAsync();
            }
        }
    }
}
