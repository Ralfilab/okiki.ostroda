using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OkikiOstroda.Web.Data;
using OkikiOstroda.Web.Models;
using OkikiOstroda.Web.Services;

namespace OkikiOstroda.Web.Controllers;

public class AdminController(ApplicationDbContext db, IOptions<OkikiAdminOptions> adminOptions, PricingService pricingService, IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userName, string password)
    {
        var options = adminOptions.Value;
        if (userName != options.UserName || password != options.Password)
        {
            ModelState.AddModelError(string.Empty, localizer["AdminInvalidLogin"]);
            return View();
        }

        var claims = new[] { new Claim(ClaimTypes.Name, userName) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var reservations = await db.Reservations.OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return View(reservations);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(DateOnly startDate, DateOnly endDate)
    {
        if (!pricingService.IsValidStay(startDate, endDate))
        {
            return RedirectToAction(nameof(Index));
        }

        db.Reservations.Add(new Reservation
        {
            GuestName = "Admin",
            GuestEmail = "ralfilab@hotmail.co.uk",
            GuestPhone = "Admin",
            GuestAddressStreet = "Admin",
            GuestAddressTown = "Admin",
            GuestAddressPostCode = "Admin",
            Guests = 1,
            StartDate = startDate,
            EndDate = endDate,
            Status = ReservationStatus.Blocked,
            TotalPrice = 0
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, ReservationStatus status)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is not null)
        {
            reservation.Status = status;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await db.Reservations.FindAsync(id);
        if (reservation is not null)
        {
            db.Reservations.Remove(reservation);
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
