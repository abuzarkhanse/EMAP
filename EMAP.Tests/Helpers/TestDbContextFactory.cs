using EMAP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMAP.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static EmapDbContext Create()
        {
            var options = new DbContextOptionsBuilder<EmapDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new EmapDbContext(options);
        }
    }
}
