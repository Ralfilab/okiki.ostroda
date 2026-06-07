using System.ComponentModel.DataAnnotations;

namespace OkikiOstroda.Web.Models;

public class ExternalCalendar
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Url]
    public string ICalUrl { get; set; } = string.Empty;

    public DateTime? LastSyncUtc { get; set; }
}
