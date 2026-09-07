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

    public DbSet<AdditionalService> AdditionalServices => Set<AdditionalService>();

    public DbSet<HallService> HallServices => Set<HallService>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingAdditionaService> BookingServices => Set<BookingAdditionaService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hall>()
            .Property(x => x.BaseHourlyRate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AdditionalService>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(x => x.TotalCost)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BookingAdditionaService>()
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

        modelBuilder.Entity<BookingAdditionaService>()
            .HasKey(x => new
            {
                x.BookingId,
                x.ServiceId
            });

        modelBuilder.Entity<BookingAdditionaService>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.BookingId);

        modelBuilder.Entity<BookingAdditionaService>()
            .HasOne(x => x.Service)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.ServiceId);
    }
}