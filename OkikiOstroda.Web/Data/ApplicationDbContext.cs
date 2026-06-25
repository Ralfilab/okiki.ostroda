using Microsoft.EntityFrameworkCore;
using OkikiOstroda.Web.Models;

namespace OkikiOstroda.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ExternalCalendar> ExternalCalendars => Set<ExternalCalendar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(x => x.TotalPrice).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.StartDate, x.EndDate });
            entity.HasIndex(x => x.Source);
        });
    }
}
