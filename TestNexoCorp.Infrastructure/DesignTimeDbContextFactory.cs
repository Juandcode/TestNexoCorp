using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TestNexoCorp.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var appsettingsPath = FindAppsettings(new DirectoryInfo(Directory.GetCurrentDirectory()));

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appsettingsPath ?? "appsettings.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(builder.Options);
        }

        private static string? FindAppsettings(DirectoryInfo? directory)
        {
            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName, "TestNexoCorp.Presentation", "appsettings.json");

                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            return null;
        }
    }
}
