using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkikiOstroda.Web.Data;
using System.Text;

namespace OkikiOstroda.Web.Controllers;

public class CalendarController(ApplicationDbContext db, IConfiguration configuration) : Controller
{
    [HttpGet("/calendar.ics")]
    public async Task<IActionResult> Export(string? token)
    {
        var expectedToken = configuration["CalendarExport:Token"];
        if (!string.IsNullOrWhiteSpace(expectedToken) && token != expectedToken)
        {
            return Unauthorized();
        }

        var reservations = await db.Reservations.ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//Okiki Ostroda//Reservations//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var r in reservations)
        {
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:reservation-{r.Id}@okiki.pl");
            sb.AppendLine($"DTSTART;VALUE=DATE:{r.StartDate:yyyyMMdd}");
            sb.AppendLine($"DTEND;VALUE=DATE:{r.EndDate:yyyyMMdd}");
            sb.AppendLine($"SUMMARY:Reserved");
            sb.AppendLine($"DTSTAMP:{r.CreatedAtUtc:yyyyMMdd}T{r.CreatedAtUtc:HHmmss}Z");
            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");

        return Content(sb.ToString(), "text/calendar", Encoding.UTF8);
    }
}
