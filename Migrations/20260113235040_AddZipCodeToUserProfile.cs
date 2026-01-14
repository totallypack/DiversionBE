using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diversion.Migrations
{
    /// <inheritdoc />
    public partial class AddZipCodeToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "UserProfiles");
        }
    }
}
