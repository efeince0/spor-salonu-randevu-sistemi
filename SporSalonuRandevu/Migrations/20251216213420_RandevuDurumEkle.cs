using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuRandevu.Migrations
{
    /// <inheritdoc />
    public partial class RandevuDurumEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "Randevular",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Randevular");
        }
    }
}
