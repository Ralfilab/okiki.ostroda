using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OkikiOstroda.Web.Migrations
{
    /// <inheritdoc />
    public partial class SplitGuestAddressIntoThreeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GuestAddressStreet",
                table: "Reservations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestAddressTown",
                table: "Reservations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestAddressPostCode",
                table: "Reservations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `Reservations` SET `GuestAddressStreet` = LEFT(`GuestAddress`, 200);");

            migrationBuilder.DropColumn(
                name: "GuestAddress",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestAddressPostCode",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GuestAddressStreet",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GuestAddressTown",
                table: "Reservations");

            migrationBuilder.AddColumn<string>(
                name: "GuestAddress",
                table: "Reservations",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
