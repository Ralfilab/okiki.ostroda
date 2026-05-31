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
        var nights = CalculateNights(startDate, endDate);
        var isHighPeak = IsHighPeakSeason(startDate, endDate);
        var minimumRequired = isHighPeak ? _options.MinimumNightsHighPeak : _options.MinimumNightsLowPeak;
        return nights >= minimumRequired;
    }

    public decimal CalculateTotal(DateOnly startDate, DateOnly endDate)
    {
        var total = 0m;
        var nightNumber = 1;
        for (var date = startDate; date < endDate; date = date.AddDays(1), nightNumber++)
        {
            total += nightNumber > _options.DiscountStartsAfterNights
                ? _options.DiscountedDailyRate
                : IsHighSeason(date) ? _options.HighSeasonDailyRate : _options.LowSeasonDailyRate;
        }

        return total;
    }

    public bool IsHighSeason(DateOnly date)
    {
        return date.Month is >= 6 and <= 8;
    }

    public bool IsHighPeakSeason(DateOnly startDate, DateOnly endDate)
    {
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            if (date.Month is 7 or 8)
            {
                return true;
            }
        }
        return false;
    }
}
