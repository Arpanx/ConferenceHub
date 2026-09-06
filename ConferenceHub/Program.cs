using ConferenceHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

// Add services to the container.
if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<ConferenceHubDbContext>(options =>
        options.UseInMemoryDatabase("ConferenceHub"));
}
else
{
    builder.Services.AddDbContext<ConferenceHubDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("ConferenceHub")));
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ConferenceHubDbContext>();

    await DatabaseSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Angular static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Angular routing fallback
app.MapFallbackToFile("index.html");

app.Run();