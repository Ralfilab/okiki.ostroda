using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OkikiOstroda.Web.Data;
using OkikiOstroda.Web.Models;
using OkikiOstroda.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("OkikiConnection");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
    });
builder.Services.Configure<PricingOptions>(builder.Configuration.GetSection("Pricing"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddOptions<OkikiAdminOptions>()
    .Bind(builder.Configuration.GetSection("OkikiAdmin"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.UserName), "OkikiAdmin:UserName is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "OkikiAdmin:Password is required.")
    .ValidateOnStart();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pl"),
    SupportedCultures = [new CultureInfo("pl"), new CultureInfo("en")],
    SupportedUICultures = [new CultureInfo("pl"), new CultureInfo("en")]
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "localized",
    pattern: "{culture:regex(^en$)}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
