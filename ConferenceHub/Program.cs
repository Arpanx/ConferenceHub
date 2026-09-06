using ConferenceHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ConferenceHubDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConferenceHub")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();


var app = builder.Build();

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