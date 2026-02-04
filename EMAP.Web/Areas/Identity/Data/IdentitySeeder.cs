using EMAP.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace EMAP.Web.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Student", "Supervisor", "FYPCoordinator", "Reviewer", "HEDOfficial", "DOSTOfficial" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

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
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Demo Supervisor users
            string[] supervisorEmails =
            {
                "sup1@emap.local",
                "sup2@emap.local"
            };

            foreach (var email in supervisorEmails)
            {
                var supervisor = await userManager.FindByEmailAsync(email);
                if (supervisor == null)
                {
                    supervisor = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = email.Replace("@emap.local", "").ToUpper(), // SUP1, SUP2
                        Department = "Computer Science"
                    };

                    // Password for all demo supervisors
                    await userManager.CreateAsync(supervisor, "Supervisor@123");
                    await userManager.AddToRoleAsync(supervisor, "Supervisor");
                }
            }

            // FYP Coordinator user
            var coordEmail = "coordinator@emap.local";
            var coord = await userManager.FindByEmailAsync(coordEmail);
            if (coord == null)
            {
                coord = new ApplicationUser
                {
                    UserName = coordEmail,
                    Email = coordEmail,
                    FullName = "FYP Coordinator"
                };
                await userManager.CreateAsync(coord, "Coordinator@123");
                await userManager.AddToRoleAsync(coord, "FYPCoordinator");
            }


            // Multiple demo Student users
            string[] studentEmails =
            {
                "student0@emap.local",
                "student2@emap.local",
                "student3@emap.local",
                "student22@emap.local",
                "student33@emap.local",
            };

            foreach (var email in studentEmails)
            {
                var student = await userManager.FindByEmailAsync(email);
                if (student == null)
                {
                    student = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = email.Replace("@emap.local", "").ToUpper(), // e.g. STUDENT1
                        RegistrationNumber = "F20-" + Guid.NewGuid().ToString("N").Substring(0, 4),
                        Department = "Computer Science"
                    };

                    await userManager.CreateAsync(student, "Student@123");
                    await userManager.AddToRoleAsync(student, "Student");
                }
            }
        }
    }
}

