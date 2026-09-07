using ConferenceHub.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Data;

public class ConferenceHubDbContext : DbContext
{
    public ConferenceHubDbContext(
        DbContextOptions<ConferenceHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();

    public DbSet<AdditionalService> AdditionalServices => Set<AdditionalService>();

    public DbSet<HallAdditionalService> HallServices => Set<HallAdditionalService>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingAdditionalService> BookingServices => Set<BookingAdditionalService>();

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

        modelBuilder.Entity<BookingAdditionalService>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<HallAdditionalService>()
            .HasKey(x => new
            {
                x.HallId,
                x.AdditionalServiceId
            });

        modelBuilder.Entity<HallAdditionalService>()
            .HasOne(x => x.Hall)
            .WithMany(x => x.HallServices)
            .HasForeignKey(x => x.HallId);

        modelBuilder.Entity<HallAdditionalService>()
            .HasOne(x => x.AdditionalService)
            .WithMany(x => x.HallServices)
            .HasForeignKey(x => x.AdditionalServiceId);

        modelBuilder.Entity<BookingAdditionalService>()
            .HasKey(x => new
            {
                x.BookingId,
                x.AdditionalServiceId
            });

        modelBuilder.Entity<BookingAdditionalService>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.BookingId);

        modelBuilder.Entity<BookingAdditionalService>()
            .HasOne(x => x.AdditionalService)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(x => x.AdditionalServiceId);
    }
}