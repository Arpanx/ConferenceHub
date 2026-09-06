using ConferenceHub.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Data;

public class ConferenceHubDbContext : DbContext
{
    public ConferenceHubDbContext(DbContextOptions<ConferenceHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<HallService> HallServices => Set<HallService>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hall>()
            .Property(x => x.BaseHourlyRate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Service>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(x => x.TotalCost)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingService>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<HallService>()
            .HasKey(x => new
            {
                x.HallId,
                x.ServiceId
            });

        modelBuilder.Entity<HallService>()
            .HasOne(x => x.Hall)
            .WithMany(x => x.HallServices)
            .HasForeignKey(x => x.HallId);

        modelBuilder.Entity<HallService>()
            .HasOne(x => x.Service)
            .WithMany(x => x.HallServices)
            .HasForeignKey(x => x.ServiceId);

        modelBuilder.Entity<BookingService>()
            .HasKey(x => new
            {
                x.BookingId,
                x.ServiceId
            });

        modelBuilder.Entity<BookingService>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.BookingId);

        modelBuilder.Entity<BookingService>()
            .HasOne(x => x.Service)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.ServiceId);
    }
}