using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuRandevu.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSlotFromAntrenor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotDakika",
                table: "Antrenorler");

            migrationBuilder.AlterColumn<string>(
                name: "Uzmanlik",
                table: "Antrenorler",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Uzmanlik",
                table: "Antrenorler",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "SlotDakika",
                table: "Antrenorler",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
