using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaizenEstate.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApartmentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Area",
                table: "Apartments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Rooms",
                table: "Apartments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "Apartments");
        }
    }
}
