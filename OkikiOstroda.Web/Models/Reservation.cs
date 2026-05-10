using System.ComponentModel.DataAnnotations;

namespace OkikiOstroda.Web.Models;

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Blocked = 3
}

public class Reservation
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(180)]
    public string GuestEmail { get; set; } = string.Empty;

    [StringLength(40)]
    public string? GuestPhone { get; set; }

    [Range(1, 4)]
    public int Guests { get; set; } = 2;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal TotalPrice { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
