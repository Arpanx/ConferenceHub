using ConferenceHub.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ConferenceHubDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (await context.Halls.AnyAsync(cancellationToken))
        {
            return;
        }

        var projector = new Service
        {
            Name = "Проєктор",
            Price = 500m
        };

        var wifi = new Service
        {
            Name = "Wi-Fi",
            Price = 300m
        };

        var sound = new Service
        {
            Name = "Звук",
            Price = 700m
        };

        context.Services.AddRange(projector, wifi, sound);

        var hallA = new Hall
        {
            Name = "Зал А",
            Capacity = 50,
            BaseHourlyRate = 2000m
        };

        var hallB = new Hall
        {
            Name = "Зал B",
            Capacity = 100,
            BaseHourlyRate = 3500m
        };

        var hallC = new Hall
        {
            Name = "Зал C",
            Capacity = 30,
            BaseHourlyRate = 1500m
        };

        context.Halls.AddRange(hallA, hallB, hallC);

        await context.SaveChangesAsync(cancellationToken);

        context.HallServices.AddRange(
            new HallService
            {
                HallId = hallA.Id,
                ServiceId = projector.Id
            },
            new HallService
            {
                HallId = hallA.Id,
                ServiceId = wifi.Id
            },
            new HallService
            {
                HallId = hallA.Id,
                ServiceId = sound.Id
            },

            new HallService
            {
                HallId = hallB.Id,
                ServiceId = projector.Id
            },
            new HallService
            {
                HallId = hallB.Id,
                ServiceId = wifi.Id
            },
            new HallService
            {
                HallId = hallB.Id,
                ServiceId = sound.Id
            },

            new HallService
            {
                HallId = hallC.Id,
                ServiceId = projector.Id
            },
            new HallService
            {
                HallId = hallC.Id,
                ServiceId = wifi.Id
            }
        );

        await context.SaveChangesAsync(cancellationToken);
    }
}