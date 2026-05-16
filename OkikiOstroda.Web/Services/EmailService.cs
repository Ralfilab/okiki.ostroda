using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OkikiOstroda.Web.Models;
using System.Net;
using System.Net.Mail;

namespace OkikiOstroda.Web.Services;

public class EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger, IStringLocalizer<SharedResource> localizer)
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendReservationConfirmationAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            logger.LogWarning("SMTP settings are not configured. Reservation confirmation email for {ReservationId} was not sent.", reservation.Id);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = localizer["EmailConfirmationSubject", reservation.Id],
            Body = localizer["EmailConfirmationBody", reservation.StartDate, reservation.EndDate, reservation.TotalPrice],
            IsBodyHtml = false
        };
        message.To.Add(reservation.GuestEmail);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_options.UserName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.UserName, _options.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
