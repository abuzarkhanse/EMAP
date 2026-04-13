using EMAP.Domain.Fyp;
using Microsoft.AspNetCore.Identity;

namespace EMAP.Domain.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
