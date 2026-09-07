using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ConferenceHub.Data;

public class ConferenceHubDbContextFactory
    : IDesignTimeDbContextFactory<ConferenceHubDbContext>
{
    public ConferenceHubDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                optional: true)
            .Build();

        var connectionString =
            configuration.GetConnectionString("ConferenceHub");

        var optionsBuilder =
            new DbContextOptionsBuilder<ConferenceHubDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ConferenceHubDbContext(optionsBuilder.Options);
    }
}