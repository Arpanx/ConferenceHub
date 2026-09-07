using ConferenceHub.Data;
using ConferenceHub.Exceptions;
using ConferenceHub.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

builder.Services.AddScoped<IHallService, HallService>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPricingService, PricingService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(
        AppContext.BaseDirectory,
        xmlFile);

    options.IncludeXmlComments(xmlPath);
});


var app = builder.Build();

app.UseExceptionHandler();

// =========================
// Database seeding
// =========================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ConferenceHubDbContext>();

    await DatabaseSeeder.SeedAsync(dbContext);
}

// =========================
// HTTP pipeline
// =========================
if (app.Environment.IsDevelopment())
{
    // https://localhost:7154/swagger
    app.UseSwagger();
    app.UseSwaggerUI();
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