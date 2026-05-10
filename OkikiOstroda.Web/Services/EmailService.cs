using Microsoft.Extensions.Options;
using OkikiOstroda.Web.Models;
using System.Net;
using System.Net.Mail;

namespace OkikiOstroda.Web.Services;

public class EmailService(IOptions<SmtpOptions> options, ILogger<EmailService> logger)
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
            Subject = $"Okiki Ostroda - rezerwacja #{reservation.Id}",
            Body = $"Dziękujemy za rezerwację. Termin: {reservation.StartDate:yyyy-MM-dd} - {reservation.EndDate:yyyy-MM-dd}. Kwota: {reservation.TotalPrice:0.00} PLN. Numer konta: 52 1020 1127 0000 1602 0391 3597.",
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
