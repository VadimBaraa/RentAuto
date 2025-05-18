using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentAutoWeb.Migrations
{
    /// <inheritdoc />
    public partial class Changes1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAdditionalInfos");

            migrationBuilder.AddColumn<string>(
                name: "DriverLicense",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicensePhotoPath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportData",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportPhotoPath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriverLicense",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DriverLicensePhotoPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportData",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PassportPhotoPath",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "UserAdditionalInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DriverLicense = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverLicensePhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassportData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassportPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAdditionalInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAdditionalInfos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAdditionalInfos_UserId",
                table: "UserAdditionalInfos",
                column: "UserId");
        }
    }
}
