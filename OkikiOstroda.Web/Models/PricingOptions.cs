namespace OkikiOstroda.Web.Models;

public class PricingOptions
{
    public decimal HighSeasonDailyRate { get; set; } = 500;

    public decimal LowSeasonDailyRate { get; set; } = 400;

    public int MinimumNights { get; set; } = 2;
}
