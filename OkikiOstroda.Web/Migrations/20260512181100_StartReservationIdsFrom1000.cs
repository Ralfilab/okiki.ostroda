using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkikiOstroda.Web.Migrations
{
    /// <inheritdoc />
    public partial class StartReservationIdsFrom1000 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Reservations` AUTO_INCREMENT = 1000;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `Reservations` AUTO_INCREMENT = 1;");
        }
    }
}
