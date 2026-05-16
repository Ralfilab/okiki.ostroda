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
        ViewBag.Reservations = await db.Reservations
            .Where(x => x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.Blocked)
            .OrderBy(x => x.StartDate)
            .ToListAsync();
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
            ModelState.AddModelError(string.Empty, localizer["ValidationMinimumStay"]);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            var overlaps = await db.Reservations.AnyAsync(x =>
                (x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.Blocked) &&
                request.StartDate.Value < x.EndDate &&
                x.StartDate < request.EndDate.Value);

            if (overlaps)
            {
                ModelState.AddModelError(string.Empty, localizer["ValidationDateUnavailable"]);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Reservations = await db.Reservations.Where(x => x.Status == ReservationStatus.Confirmed || x.Status == ReservationStatus.Blocked).ToListAsync();
            return View(request);
        }

        var reservation = new Reservation
        {
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            Guests = request.Guests.GetValueOrDefault(),
            GuestAddress = request.GuestAddress,
            StartDate = request.StartDate.GetValueOrDefault(),
            EndDate = request.EndDate.GetValueOrDefault(),
            Notes = request.Notes,
            TotalPrice = pricingService.CalculateTotal(request.StartDate.GetValueOrDefault(), request.EndDate.GetValueOrDefault()),
            Status = ReservationStatus.Pending
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();
        await emailService.SendReservationConfirmationAsync(reservation);

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
}
