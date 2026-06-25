using Microsoft.EntityFrameworkCore;
using OkikiOstroda.Web.Data;
using OkikiOstroda.Web.Models;

namespace OkikiOstroda.Web.Services;

public class CalendarSyncService(IServiceScopeFactory scopeFactory, ILogger<CalendarSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAllCalendarsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during calendar sync.");
            }

            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SyncAllCalendarsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var calendars = await db.ExternalCalendars.ToListAsync(ct);

        if (calendars.Count == 0) return;

        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        foreach (var calendar in calendars)
        {
            try
            {
                await SyncCalendarAsync(db, httpClient, calendar, ct);
                calendar.LastSyncUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to sync calendar '{Name}' ({Url}).", calendar.Name, calendar.ICalUrl);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SyncCalendarAsync(ApplicationDbContext db, HttpClient httpClient, ExternalCalendar calendar, CancellationToken ct)
    {
        var icsContent = await httpClient.GetStringAsync(calendar.ICalUrl, ct);
        var events = ParseICalEvents(icsContent);

        var source = $"iCal:{calendar.Name}";
        var existingBlocks = await db.Reservations
            .Where(x => x.Source == source)
            .ToListAsync(ct);

        var allReservations = await db.Reservations.ToListAsync(ct);

        var parsedSet = events.Select(e => (e.Start, e.End)).ToHashSet();
        var existingSet = allReservations.Select(r => (r.StartDate, r.EndDate)).ToHashSet();

        // Remove blocks that no longer exist in the external calendar
        foreach (var block in existingBlocks)
        {
            if (!parsedSet.Contains((block.StartDate, block.EndDate)))
            {
                db.Reservations.Remove(block);
            }
        }

        // Add new blocks from the external calendar
        foreach (var evt in events)
        {
            if (!existingSet.Contains((evt.Start, evt.End)))
            {
                db.Reservations.Add(new Reservation
                {
                    GuestName = calendar.Name,
                    GuestEmail = "-",
                    GuestPhone = "-",
                    GuestAddressStreet = "-",
                    GuestAddressTown = "-",
                    GuestAddressPostCode = "-",
                    Guests = 2,
                    StartDate = evt.Start,
                    EndDate = evt.End,
                    TotalPrice = 0,
                    Source = source,
                    Notes = evt.Summary
                });
            }
        }

        logger.LogInformation("Synced calendar '{Name}': {Count} events.", calendar.Name, events.Count);
    }

    private static List<ICalEvent> ParseICalEvents(string icsContent)
    {
        var events = new List<ICalEvent>();
        var lines = icsContent.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

        string? dtStart = null;
        string? dtEnd = null;
        string? summary = null;
        bool inEvent = false;

        foreach (var line in lines)
        {
            if (line == "BEGIN:VEVENT")
            {
                inEvent = true;
                dtStart = null;
                dtEnd = null;
                summary = null;
            }
            else if (line == "END:VEVENT" && inEvent)
            {
                inEvent = false;
                if (dtStart != null)
                {
                    var start = ParseICalDate(dtStart);
                    var end = dtEnd != null ? ParseICalDate(dtEnd) : start.AddDays(1);
                    if (start != default)
                    {
                        events.Add(new ICalEvent(start, end, summary ?? "External"));
                    }
                }
            }
            else if (inEvent)
            {
                if (line.StartsWith("DTSTART"))
                {
                    dtStart = ExtractValue(line);
                }
                else if (line.StartsWith("DTEND"))
                {
                    dtEnd = ExtractValue(line);
                }
                else if (line.StartsWith("SUMMARY"))
                {
                    summary = ExtractValue(line);
                }
            }
        }

        return events;
    }

    private static string ExtractValue(string line)
    {
        var colonIndex = line.IndexOf(':');
        return colonIndex >= 0 ? line[(colonIndex + 1)..] : line;
    }

    private static DateOnly ParseICalDate(string value)
    {
        // Handles formats like 20260615 or 20260615T120000Z
        var dateStr = value.Length >= 8 ? value[..8] : value;
        if (DateOnly.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }
        return default;
    }

    private record ICalEvent(DateOnly Start, DateOnly End, string Summary);
}
