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

    public async Task SendNewReservationNotificationAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            logger.LogWarning("SMTP settings are not configured. Reservation notification email for {ReservationId} was not sent.", reservation.Id);
            return;
        }

        var nights = (reservation.EndDate.DayNumber - reservation.StartDate.DayNumber);
        var body = $"""
            Nowa rezerwacja #{reservation.Id}

            Imię i nazwisko: {reservation.GuestName}
            Email: {reservation.GuestEmail}
            Telefon: {reservation.GuestPhone}
            Adres: {reservation.GuestAddressStreet}, {reservation.GuestAddressPostCode} {reservation.GuestAddressTown}
            Liczba gości: {reservation.Guests}
            Termin: {reservation.StartDate:yyyy-MM-dd} - {reservation.EndDate:yyyy-MM-dd} ({nights} {(nights == 1 ? "noc" : "nocy")})
            Kwota: {reservation.TotalPrice:0.00} PLN
            Wiadomość: {reservation.Notes ?? "-"}
            """;

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = $"Nowa rezerwacja #{reservation.Id} - {reservation.GuestName}",
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(_options.FromEmail);

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
