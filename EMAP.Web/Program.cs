using EMAP.Domain.Users;
using EMAP.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EMAP.Web.Data;
using EMAP.Web.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EmapDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<EmapDbContext>()
    .AddDefaultTokenProviders();

// Identity cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// MVC + Razor Pages (REQUIRED)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Email Service
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddTransient<IEmailService, MailKitEmailService>();

builder.WebHost.UseUrls("http://0.0.0.0:5281");

var app = builder.Build();

// Seed Roles & Admin
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

// =======================
// Error Handling
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing + Identity
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ROLE-BASED ROOT REDIRECT (CORRECT PLACE)
app.Use(async (context, next) =>
{
    // Only handle ROOT "/"
    if (context.Request.Path == "/")
    {
        // Not logged in → Login
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            context.Response.Redirect("/Identity/Account/Login");
            return;
        }

        // Logged in → Role dashboards
        if (context.User.IsInRole("Admin"))
        {
            context.Response.Redirect("/Admin/Dashboard");
            return;
        }

        if (context.User.IsInRole("FYPCoordinator"))
        {
            context.Response.Redirect("/FypCoordinator/Dashboard");
            return;
        }

        if (context.User.IsInRole("Supervisor"))
        {
            context.Response.Redirect("/Supervisor/Dashboard");
            return;
        }

        if (context.User.IsInRole("Student"))
        {
            context.Response.Redirect("/Home/StudentDashboard");
            return;
        }
    }

    await next();
});

// Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // REQUIRED for Identity UI

app.Run();