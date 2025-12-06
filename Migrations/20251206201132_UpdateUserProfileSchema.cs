using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diversion.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "UserProfiles",
                newName: "DisplayName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "UserProfiles",
                newName: "UserName");
        }
    }
}
