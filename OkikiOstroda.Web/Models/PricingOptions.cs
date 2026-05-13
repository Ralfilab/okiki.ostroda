namespace OkikiOstroda.Web.Models;

public class PricingOptions
{
    public decimal HighSeasonDailyRate { get; set; } = 500;

    public decimal LowSeasonDailyRate { get; set; } = 400;

    public int MinimumNights { get; set; } = 2;

    public decimal DiscountedDailyRate { get; set; } = 400;

    public int DiscountStartsAfterNights { get; set; } = 2;
}
