using EMAP.Domain.Fyp;
using EMAP.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Infrastructure.Data;

public class EmapDbContext
    : IdentityDbContext<ApplicationUser>
{
    public EmapDbContext(DbContextOptions<EmapDbContext> options)
        : base(options)
    {
    }

    public DbSet<FypCall> FypCalls => Set<FypCall>();
    public DbSet<FypProject> FypProjects => Set<FypProject>();

    // later: Thesis, Research, Notifications, etc.
}
