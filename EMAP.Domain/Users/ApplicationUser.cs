using Microsoft.AspNetCore.Identity;

namespace EMAP.Domain.Users;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty; // for students
    public string Department { get; set; } = string.Empty;
}
