using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAutoWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueCarIdFromRentals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CarId",
                table: "Rentals",
                column: "CarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
