using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diversion.Migrations
{
    /// <inheritdoc />
    public partial class AddCaregiverFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareTypes",
                table: "UserProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaregiverCredentials",
                table: "UserProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certifications",
                table: "UserProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "UserProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackgroundChecked",
                table: "UserProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiry",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "UserProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specializations",
                table: "UserProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CaregiverRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestCanManageEvents = table.Column<bool>(type: "bit", nullable: false),
                    RequestCanManageProfile = table.Column<bool>(type: "bit", nullable: false),
                    RequestCanManageFriendships = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaregiverRequests_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaregiverRequests_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CareRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaregiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CanManageEvents = table.Column<bool>(type: "bit", nullable: false),
                    CanManageProfile = table.Column<bool>(type: "bit", nullable: false),
                    CanManageFriendships = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareRelationships_AspNetUsers_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CareRelationships_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverRequests_RecipientId",
                table: "CaregiverRequests",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverRequests_SenderId",
                table: "CaregiverRequests",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverRequests_SenderId_RecipientId_Status",
                table: "CaregiverRequests",
                columns: new[] { "SenderId", "RecipientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CareRelationships_CaregiverId_RecipientId",
                table: "CareRelationships",
                columns: new[] { "CaregiverId", "RecipientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CareRelationships_RecipientId",
                table: "CareRelationships",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_CareRelationships_RecipientId_IsActive",
                table: "CareRelationships",
                columns: new[] { "RecipientId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaregiverRequests");

            migrationBuilder.DropTable(
                name: "CareRelationships");

            migrationBuilder.DropColumn(
                name: "CareTypes",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CaregiverCredentials",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Certifications",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IsBackgroundChecked",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseExpiry",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Specializations",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "UserProfiles");
        }
    }
}
