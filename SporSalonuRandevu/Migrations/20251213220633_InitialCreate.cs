using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SporSalonuRandevu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Antrenorler_Salonlar_SalonId",
                table: "Antrenorler");

            migrationBuilder.DropForeignKey(
                name: "FK_Hizmetler_Salonlar_SalonId",
                table: "Hizmetler");

            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular");

            migrationBuilder.DropTable(
                name: "Salonlar");

            migrationBuilder.DropIndex(
                name: "IX_Randevular_SalonId",
                table: "Randevular");

            migrationBuilder.DropIndex(
                name: "IX_Hizmetler_SalonId",
                table: "Hizmetler");

            migrationBuilder.DropIndex(
                name: "IX_Antrenorler_SalonId",
                table: "Antrenorler");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Hizmetler");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Antrenorler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Randevular",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Hizmetler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Antrenorler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Salonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Adres = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salonlar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_SalonId",
                table: "Randevular",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Hizmetler_SalonId",
                table: "Hizmetler",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_SalonId",
                table: "Antrenorler",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Antrenorler_Salonlar_SalonId",
                table: "Antrenorler",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hizmetler_Salonlar_SalonId",
                table: "Hizmetler",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Salonlar_SalonId",
                table: "Randevular",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
