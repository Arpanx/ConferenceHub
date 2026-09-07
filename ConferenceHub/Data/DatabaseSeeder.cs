using ConferenceHub.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ConferenceHubDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Не создаём дубликаты при повторном запуске приложения.
        if (await context.Halls.AnyAsync(cancellationToken))
        {
            return;
        }

        // =========================
        // Services
        // =========================

        var projector = new AdditionalService
        {
            Name = "Проєктор",
            Price = 500m,
            IsActive = true
        };

        var wifi = new AdditionalService
        {
            Name = "Wi-Fi",
            Price = 300m,
            IsActive = true
        };

        var sound = new AdditionalService
        {
            Name = "Звук",
            Price = 700m,
            IsActive = true
        };

        context.AdditionalServices.AddRange(
            projector,
            wifi,
            sound);

        // =========================
        // Halls
        // =========================

        var hallA = new Hall
        {
            Name = "Зал А",
            Capacity = 50,
            BaseHourlyRate = 2000m,
            IsActive = true
        };

        var hallB = new Hall
        {
            Name = "Зал B",
            Capacity = 100,
            BaseHourlyRate = 3500m,
            IsActive = true
        };

        var hallC = new Hall
        {
            Name = "Зал C",
            Capacity = 30,
            BaseHourlyRate = 1500m,
            IsActive = true
        };

        context.Halls.AddRange(
            hallA,
            hallB,
            hallC);

        // Сначала сохраняем основные сущности,
        // чтобы получить их ID.
        await context.SaveChangesAsync(cancellationToken);

        // =========================
        // Hall services
        // =========================

        context.HallServices.AddRange(
            // Зал А
            new HallAdditionalService
            {
                HallId = hallA.Id,
                AdditionalServiceId = projector.Id
            },
            new HallAdditionalService
            {
                HallId = hallA.Id,
                AdditionalServiceId = wifi.Id
            },
            new HallAdditionalService
            {
                HallId = hallA.Id,
                AdditionalServiceId = sound.Id
            },

            // Зал B
            new HallAdditionalService
            {
                HallId = hallB.Id,
                AdditionalServiceId = projector.Id
            },
            new HallAdditionalService
            {
                HallId = hallB.Id,
                AdditionalServiceId = sound.Id
            },

            // Зал C
            new HallAdditionalService
            {
                HallId = hallC.Id,
                AdditionalServiceId = projector.Id
            },
            new HallAdditionalService
            {
                HallId = hallC.Id,
                AdditionalServiceId = wifi.Id
            },
            new HallAdditionalService
            {
                HallId = hallC.Id,
                AdditionalServiceId = sound.Id
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}