using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OkikiOstroda.Web.Data;
using OkikiOstroda.Web.Models;
using OkikiOstroda.Web.Services;

namespace OkikiOstroda.Web.Controllers;

public class ReservationController(ApplicationDbContext db, PricingService pricingService, EmailService emailService, IStringLocalizer<SharedResource> localizer) : Controller
{    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await GetReservationsForViewBag();
        var today = DateOnly.FromDateTime(DateTime.Today);
        return View(new ReservationRequest
        {
            StartDate = today.AddDays(2),
            EndDate = today.AddDays(9)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReservationRequest request)
    {
        if (!request.StartDate.HasValue || !request.EndDate.HasValue)
        {
            ModelState.AddModelError(string.Empty, localizer["ValidationDateRangeRequired"]);
        }
        else if (!pricingService.IsValidStay(request.StartDate.Value, request.EndDate.Value))
        {
            var isHighPeak = pricingService.IsHighPeakSeason(request.StartDate.Value, request.EndDate.Value);
            var validationKey = isHighPeak ? "ValidationMinimumStayHighPeak" : "ValidationMinimumStayLowPeak";
            ModelState.AddModelError(string.Empty, localizer[validationKey]);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var overlaps = await db.Reservations.AnyAsync(x =>
                request.StartDate.Value < x.EndDate &&
                x.StartDate < request.EndDate.Value);

            if (overlaps)
            {
                ModelState.AddModelError(string.Empty, localizer["ValidationDateUnavailable"]);
            }
        }

        if (!ModelState.IsValid)
        {
            await GetReservationsForViewBag();
            return View(request);
        }

        var reservation = new Reservation
        {
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            Guests = request.Guests.GetValueOrDefault(),
            GuestAddressStreet = request.GuestAddressStreet,
            GuestAddressTown = request.GuestAddressTown,
            GuestAddressPostCode = request.GuestAddressPostCode,
            StartDate = request.StartDate.GetValueOrDefault(),
            EndDate = request.EndDate.GetValueOrDefault(),
            Notes = request.Notes,
            TotalPrice = pricingService.CalculateTotal(request.StartDate.GetValueOrDefault(), request.EndDate.GetValueOrDefault())
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        await emailService.SendReservationConfirmationAsync(reservation);
        await emailService.SendNewReservationNotificationAsync(reservation);

        return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    private async Task GetReservationsForViewBag()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        ViewBag.Reservations = await db.Reservations
            .Where(x => x.StartDate >= today)
            .OrderBy(x => x.StartDate)
            .Select(x => new { x.StartDate, x.EndDate })
            .ToListAsync();
    }
}
