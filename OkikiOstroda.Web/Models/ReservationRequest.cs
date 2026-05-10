using System.ComponentModel.DataAnnotations;

namespace OkikiOstroda.Web.Models;

public class ReservationRequest
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [StringLength(160)]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(180)]
    public string GuestEmail { get; set; } = string.Empty;

    [Phone]
    [StringLength(40)]
    public string? GuestPhone { get; set; }

    [Range(1, 4)]
    public int Guests { get; set; } = 2;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
