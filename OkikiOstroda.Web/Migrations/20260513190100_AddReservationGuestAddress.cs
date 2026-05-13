using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkikiOstroda.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationGuestAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE `Reservations` SET `GuestPhone` = '' WHERE `GuestPhone` IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "GuestPhone",
                table: "Reservations",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestAddress",
                table: "Reservations",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestAddress",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "GuestPhone",
                table: "Reservations",
                type: "varchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
