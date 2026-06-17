using System.ComponentModel.DataAnnotations;

namespace OkikiOstroda.Web.Models;

public class ReservationRequest
{
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [StringLength(160)]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationEmail")]
    [StringLength(180)]
    public string GuestEmail { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [Phone(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationPhone")]
    [StringLength(40)]
    public string GuestPhone { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [Range(1, 2, ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationGuestsRange")]
    public int? Guests { get; set; } = 2;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [StringLength(200)]
    public string GuestAddressStreet { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [StringLength(200)]
    public string GuestAddressTown { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "ValidationRequired")]
    [StringLength(200)]
    public string GuestAddressPostCode { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
