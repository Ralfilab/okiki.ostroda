# Okiki Ostroda Website

Okiki Ostroda is a bilingual rental property website for `okikiostroda.pl`, built with ASP.NET Core MVC/Razor for SEO-friendly server-rendered pages.

## What this project is for

This website presents the Okiki Ostroda rental property in Boguszewo near Ostróda and provides a basic reservation workflow:

- Public rental property website
- Polish default language with English route support
- Mobile-friendly responsive layout
- Gallery using property photos
- Availability/reservation request form
- Bank transfer payment instructions after booking
- Email confirmation support via configurable SMTP settings
- Admin panel for viewing reservations and blocking dates
- SEO files: `robots.txt`, `sitemap.xml`, JSON-LD structured data

## Technology stack

- ASP.NET Core MVC / Razor Views
- Target framework: `.NET 10` (`net10.0`)
- MySQL database
- Entity Framework Core with Pomelo MySQL provider
- Bootstrap 5
- Custom CSS
- Razor view localization/resource structure

## Project structure

```text
okiki.ostroda/
├── OkikiOstroda.sln
├── README.md
├── pictures/                         # Original source photos
└── OkikiOstroda.Web/
    ├── Controllers/                  # MVC controllers
    ├── Data/                         # EF Core DbContext
    ├── Models/                       # Reservation/config models
    ├── Resources/                    # Localization resource files
    ├── Services/                     # Pricing and email services
    ├── Views/                        # Razor views
    ├── wwwroot/
    │   ├── css/site.css
    │   ├── images/                   # Website photos and static map
    │   ├── robots.txt
    │   └── sitemap.xml
    ├── appsettings.json
    └── OkikiOstroda.Web.csproj
```

## Prerequisites

Install the following before running the project:

1. **.NET 10 SDK**
   - Required because the project targets `net10.0`.
   - Check installed SDK:

   ```powershell
   dotnet --version
   ```

2. **MySQL Server 8+**
   - The app is configured for MySQL.

3. Optional but recommended:
   - Visual Studio 2026 / Rider / VS Code
   - MySQL Workbench or another database GUI

## Configuration

Main configuration is in:

```text
OkikiOstroda.Web/appsettings.json
```

### Database connection

Update this value for your MySQL server:

```json
"ConnectionStrings": {
  "OkikiConnection": "server=localhost;port=3306;database=okiki_ostroda;user=okiki_user;password=change-me"
}
```

Example local development connection:

```json
"OkikiConnection": "server=localhost;port=3306;database=okiki_ostroda;user=root;password=your-password"
```

### Admin login

Admin credentials come from `appsettings.json`:

```json
"Admin": {
  "UserName": "admin",
  "Password": "ChangeMe123!"
}
```

Change these before deployment.

The app validates these settings on startup. If either value is empty, startup fails with a clear configuration error.

### SMTP/email settings

SMTP is currently a placeholder. Fill this section when email credentials are available:

```json
"Smtp": {
  "Host": "",
  "Port": 587,
  "UserName": "",
  "Password": "",
  "FromEmail": "",
  "FromName": "Okiki Ostroda",
  "EnableSsl": true
}
```

If SMTP is not configured, reservations still work, but confirmation email sending is skipped and logged as a warning.

### Pricing

Pricing is configured in `appsettings.json`:

```json
"Pricing": {
  "HighSeasonDailyRate": 500,
  "LowSeasonDailyRate": 400,
  "MinimumNights": 2
}
```

Current rules:

- High season: June, July, August
- High season price: 500 PLN/day
- Low season price: 400 PLN/day
- Minimum stay: 2 nights

## Database setup

Create the database in MySQL:

```sql
CREATE DATABASE okiki_ostroda CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Then create and apply EF Core migrations.

From the repository root:

```powershell
dotnet ef migrations add InitialCreate --project OkikiOstroda.Web
dotnet ef database update --project OkikiOstroda.Web
```

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef
```

## Running locally

From the repository root:

```powershell
dotnet restore
dotnet build OkikiOstroda.sln
dotnet run --project OkikiOstroda.Web
```

Then open the URL shown in the terminal, usually one of:

```text
https://localhost:xxxx
http://localhost:xxxx
```

## Main URLs

Public pages:

```text
/                         Home
/Home/Property            Property details
/Home/Gallery             Gallery
/Home/Location            Location and maps
/Reservation              Pricing and reservation
/Home/Contact             Contact
/en/                      English route prefix placeholder
```

Admin pages:

```text
/Admin/Login              Admin login
/Admin                    Admin dashboard
```

## Reservation flow

1. Guest opens `/Reservation`.
2. Guest selects arrival/departure dates and submits contact details.
3. App checks:
   - minimum 2 nights
   - selected dates do not overlap existing non-cancelled reservations
4. Reservation is saved as `Pending`.
5. Confirmation page shows bank transfer details.
6. Email confirmation is sent if SMTP is configured.
7. Admin can mark reservation as `Confirmed`, `Cancelled`, `Blocked`, or `Pending`.

Bank account displayed after reservation:

```text
52 1020 1127 0000 1602 0391 3597
```

Transfer title format:

```text
Rezerwacja {ReservationId} - {GuestName}
```

## Admin functionality

The admin panel currently supports:

- Login/logout
- Viewing reservations
- Changing reservation status
- Blocking date ranges manually

The admin panel does **not** upload a map image. The map image is static and stored in `wwwroot/images/static-map.svg`.

## Images

Original photos are in:

```text
pictures/
```

Website images are served from:

```text
OkikiOstroda.Web/wwwroot/images/
```

Current behavior:

- `main_1.JPEG` is used as the homepage hero image.
- `main_1.JPEG` to `main_4.JPEG` are copied to web assets and can be used for banners.
- `IMG_*.JPEG` files are used by the gallery.
- `static-map.svg` is a placeholder static map image.

To replace the static map, overwrite:

```text
OkikiOstroda.Web/wwwroot/images/static-map.svg
```

or update the path in:

```text
OkikiOstroda.Web/Views/Home/Location.cshtml
```

## Localization

Polish is the default language.

English route support exists via:

```text
/en/
```

Resource files are located in:

```text
OkikiOstroda.Web/Resources/
```

Current resource files:

```text
SharedResource.pl.resx
SharedResource.en.resx
```

Most page text is currently written directly in Razor views. To fully translate the site later, move view text into resource files and inject localizers into views/controllers.

## SEO

SEO-related files and markup include:

- `wwwroot/robots.txt`
- `wwwroot/sitemap.xml`
- per-page `ViewData["Title"]`
- per-page `ViewData["Description"]`
- Open Graph tags in `_Layout.cshtml`
- JSON-LD `LodgingBusiness` structured data in `_Layout.cshtml`

Before production, verify URLs in:

```text
OkikiOstroda.Web/wwwroot/sitemap.xml
```

They currently point to:

```text
https://okikiostroda.pl
```

## Deployment checklist

Before deploying to production:

1. Install/use .NET 10 SDK on the server/build machine.
2. Create MySQL database and user.
3. Update `ConnectionStrings:OkikiConnection`.
4. Change admin username/password.
5. Configure SMTP settings when email account is available.
6. Run EF migrations/database update.
7. Replace placeholder email/contact data if needed.
8. Replace `static-map.svg` with final custom map image.
9. Verify `sitemap.xml` URLs.
10. Run:

```powershell
dotnet restore
dotnet build OkikiOstroda.sln
dotnet publish OkikiOstroda.Web -c Release
```

## Notes and known limitations

- The project targets `.NET 10`, so local build requires the .NET 10 SDK.
- Availability is currently represented by reservations/date blocks in the database, not by a full visual month calendar widget.
- English route support and resource files exist, but full English page content still needs to be expanded.
- Payment is manual bank transfer only; there is no online card payment integration.
- SMTP credentials are not included and must be configured securely outside source control for production.
