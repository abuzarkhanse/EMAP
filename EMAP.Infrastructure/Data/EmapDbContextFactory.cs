using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EMAP.Infrastructure.Data
{
    public class EmapDbContextFactory : IDesignTimeDbContextFactory<EmapDbContext>
    {
        public EmapDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var webPath = Path.Combine(basePath, "..", "EMAP.Web");
            if (!Directory.Exists(webPath))
            {
                webPath = Path.Combine(basePath, "EMAP.Web");
            }

            var appSettingsPath = Path.Combine(webPath, "appsettings.json");

            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("Could not find appsettings.json", appSettingsPath);
            }

            var json = File.ReadAllText(appSettingsPath);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
            {
                throw new InvalidOperationException("ConnectionStrings section not found in appsettings.json");
            }

            if (!connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
            {
                throw new InvalidOperationException("DefaultConnection not found in appsettings.json");
            }

            var connectionString = defaultConnection.GetString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection is empty in appsettings.json");
            }

            var optionsBuilder = new DbContextOptionsBuilder<EmapDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new EmapDbContext(optionsBuilder.Options);
        }
    }
}
