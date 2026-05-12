using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkikiOstroda.Web.Data;
using OkikiOstroda.Web.Models;
using OkikiOstroda.Web.Services;

namespace OkikiOstroda.Web.Controllers;

public class ReservationController(ApplicationDbContext db, PricingService pricingService, EmailService emailService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Reservations = await db.Reservations
            .Where(x => x.Status != ReservationStatus.Cancelled)
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
        if (!pricingService.IsValidStay(request.StartDate, request.EndDate))
        {
            ModelState.AddModelError(string.Empty, "Minimalny pobyt to 2 noce.");
        }

        var overlaps = await db.Reservations.AnyAsync(x =>
            x.Status != ReservationStatus.Cancelled &&
            request.StartDate < x.EndDate &&
            x.StartDate < request.EndDate);

        if (overlaps)
        {
            ModelState.AddModelError(string.Empty, "Wybrany termin jest niedostępny.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Reservations = await db.Reservations.Where(x => x.Status != ReservationStatus.Cancelled).ToListAsync();
            return View(request);
        }

        var reservation = new Reservation
        {
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            Guests = request.Guests,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            TotalPrice = pricingService.CalculateTotal(request.StartDate, request.EndDate),
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
