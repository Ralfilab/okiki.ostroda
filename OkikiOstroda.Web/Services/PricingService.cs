using Microsoft.Extensions.Options;
using OkikiOstroda.Web.Models;

namespace OkikiOstroda.Web.Services;

public class PricingService(IOptions<PricingOptions> options)
{
    private readonly PricingOptions _options = options.Value;

    public int CalculateNights(DateOnly startDate, DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber;
    }

    public bool IsValidStay(DateOnly startDate, DateOnly endDate)
    {
        return CalculateNights(startDate, endDate) >= _options.MinimumNights;
    }

    public decimal CalculateTotal(DateOnly startDate, DateOnly endDate)
    {
        var total = 0m;
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            total += IsHighSeason(date) ? _options.HighSeasonDailyRate : _options.LowSeasonDailyRate;
        }

        return total;
    }

    public bool IsHighSeason(DateOnly date)
    {
        return date.Month is >= 6 and <= 8;
    }
}
