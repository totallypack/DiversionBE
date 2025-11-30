using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Diversion.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "Interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubInterests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubInterests_Interests_InterestId",
                        column: x => x.InterestId,
                        principalTable: "Interests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InterestTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiresRsvp = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_AspNetUsers_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_SubInterests_InterestTagId",
                        column: x => x.InterestTagId,
                        principalTable: "SubInterests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubInterestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInterests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInterests_SubInterests_SubInterestId",
                        column: x => x.SubInterestId,
                        principalTable: "SubInterests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserInterests_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventAttendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventAttendees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventAttendees_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Interests",
                columns: new[] { "Id", "Description", "IconUrl", "Name" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), "Historical events and ancient civilizations", null, "History & Archaeology" },
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Outdoor activities and wilderness adventures", null, "Outdoors & Adventure" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Video games, board games, and tabletop gaming", null, "Gaming" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Model building, collecting, and hobbies", null, "Models & Collecting" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Physical activities and sports", null, "Fitness & Sports" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Creative and artistic pursuits", null, "Arts & Crafts" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Music performance and appreciation", null, "Music" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Culinary arts and beverages", null, "Food & Drink" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "Tech projects and programming", null, "Technology" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "Scientific research and medical topics", null, "Science & Medicine" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Sci-fi, fantasy, and speculative fiction", null, "Science Fiction & Fantasy" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Woodworking and traditional crafts", null, "Woodworking & Craftsmanship" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Cars, trucks, and heavy machinery", null, "Automotive & Machinery" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Books, writing, and storytelling", null, "Literature & Writing" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Combat sports and martial disciplines", null, "Martial Arts" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Pop culture, retro trends, and media", null, "Pop Culture & Nostalgia" }
                });

            migrationBuilder.InsertData(
                table: "SubInterests",
                columns: new[] { "Id", "Description", "IconUrl", "InterestId", "Name" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), "Home cooking and culinary arts", null, new Guid("77777777-7777-7777-7777-777777777777"), "Cooking" },
                    { new Guid("10000002-0000-0000-0000-000000000002"), "Baking and pastry", null, new Guid("77777777-7777-7777-7777-777777777777"), "Baking" },
                    { new Guid("10000003-0000-0000-0000-000000000003"), "Coffee brewing and appreciation", null, new Guid("77777777-7777-7777-7777-777777777777"), "Coffee" },
                    { new Guid("10000004-0000-0000-0000-000000000004"), "Beer tasting and brewing", null, new Guid("77777777-7777-7777-7777-777777777777"), "Craft Beer" },
                    { new Guid("10000005-0000-0000-0000-000000000005"), "Wine appreciation and oenology", null, new Guid("77777777-7777-7777-7777-777777777777"), "Wine Tasting" },
                    { new Guid("10000006-0000-0000-0000-000000000006"), "Sourdough baking and fermentation", null, new Guid("77777777-7777-7777-7777-777777777777"), "Sourdough Bread" },
                    { new Guid("10000007-0000-0000-0000-000000000007"), "BBQ techniques and meat smoking", null, new Guid("77777777-7777-7777-7777-777777777777"), "BBQ & Smoking" },
                    { new Guid("20000001-0000-0000-0000-000000000001"), "Software development and coding", null, new Guid("88888888-8888-8888-8888-888888888888"), "Programming" },
                    { new Guid("20000002-0000-0000-0000-000000000002"), "3D printing and modeling", null, new Guid("88888888-8888-8888-8888-888888888888"), "3D Printing" },
                    { new Guid("20000003-0000-0000-0000-000000000003"), "Electronics and circuit building", null, new Guid("88888888-8888-8888-8888-888888888888"), "Electronics" },
                    { new Guid("20000004-0000-0000-0000-000000000004"), "Robot building and automation", null, new Guid("88888888-8888-8888-8888-888888888888"), "Robotics" },
                    { new Guid("20000005-0000-0000-0000-000000000005"), "AI, ML, and data science", null, new Guid("88888888-8888-8888-8888-888888888888"), "AI & Machine Learning" },
                    { new Guid("20000006-0000-0000-0000-000000000006"), "InfoSec and ethical hacking", null, new Guid("88888888-8888-8888-8888-888888888888"), "Cybersecurity" },
                    { new Guid("20000007-0000-0000-0000-000000000007"), "Home servers and networking", null, new Guid("88888888-8888-8888-8888-888888888888"), "Homelab" },
                    { new Guid("30000001-0000-0000-0000-000000000001"), "Medical news and breakthrough research", null, new Guid("99999999-9999-9999-9999-999999999999"), "Medical Research" },
                    { new Guid("30000002-0000-0000-0000-000000000002"), "Space, stars, and celestial phenomena", null, new Guid("99999999-9999-9999-9999-999999999999"), "Astronomy" },
                    { new Guid("30000003-0000-0000-0000-000000000003"), "Theoretical and applied physics", null, new Guid("99999999-9999-9999-9999-999999999999"), "Physics" },
                    { new Guid("30000004-0000-0000-0000-000000000004"), "Life sciences and ecosystems", null, new Guid("99999999-9999-9999-9999-999999999999"), "Biology & Ecology" },
                    { new Guid("30000005-0000-0000-0000-000000000005"), "Chemical reactions and compounds", null, new Guid("99999999-9999-9999-9999-999999999999"), "Chemistry" },
                    { new Guid("30000006-0000-0000-0000-000000000006"), "Brain research and cognitive science", null, new Guid("99999999-9999-9999-9999-999999999999"), "Neuroscience" },
                    { new Guid("30000007-0000-0000-0000-000000000007"), "DNA, heredity, and genetic engineering", null, new Guid("99999999-9999-9999-9999-999999999999"), "Genetics" },
                    { new Guid("40000001-0000-0000-0000-000000000001"), "Time travel theory and paradoxes", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Time Travel" },
                    { new Guid("40000002-0000-0000-0000-000000000002"), "Zombie apocalypse and survival", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Zombies" },
                    { new Guid("40000003-0000-0000-0000-000000000003"), "Star Wars, Star Trek, and space epics", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Space Opera" },
                    { new Guid("40000004-0000-0000-0000-000000000004"), "Dystopian tech futures", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Cyberpunk" },
                    { new Guid("40000005-0000-0000-0000-000000000005"), "High fantasy and world-building", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Fantasy Worlds" },
                    { new Guid("40000006-0000-0000-0000-000000000006"), "Extraterrestrial life and phenomena", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Aliens & UFOs" },
                    { new Guid("40000007-0000-0000-0000-000000000007"), "What-if scenarios and alternate timelines", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Alternate History" },
                    { new Guid("40000008-0000-0000-0000-000000000008"), "End-of-world scenarios and survival", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Post-Apocalyptic" },
                    { new Guid("50000001-0000-0000-0000-000000000001"), "Custom furniture and cabinetry", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Furniture Making" },
                    { new Guid("50000002-0000-0000-0000-000000000002"), "Lathe work and bowl turning", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Wood Turning" },
                    { new Guid("50000003-0000-0000-0000-000000000003"), "Exotic woods and lumber identification", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Wood Species" },
                    { new Guid("50000004-0000-0000-0000-000000000004"), "Wood carving and whittling", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Carving" },
                    { new Guid("50000005-0000-0000-0000-000000000005"), "Traditional joinery techniques", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Joinery" },
                    { new Guid("50000006-0000-0000-0000-000000000006"), "Stains, oils, and finishing techniques", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Wood Finishing" },
                    { new Guid("50000007-0000-0000-0000-000000000007"), "Traditional hand tool woodworking", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Hand Tools" },
                    { new Guid("60000001-0000-0000-0000-000000000001"), "Vintage and classic automobiles", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Classic Cars" },
                    { new Guid("60000002-0000-0000-0000-000000000002"), "Pickup trucks and 4x4 vehicles", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Trucks & Off-Road" },
                    { new Guid("60000003-0000-0000-0000-000000000003"), "Vehicle restoration and rebuilding", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Auto Restoration" },
                    { new Guid("60000004-0000-0000-0000-000000000004"), "American muscle and performance cars", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Muscle Cars" },
                    { new Guid("60000005-0000-0000-0000-000000000005"), "Motorcycles and bike culture", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Motorcycles" },
                    { new Guid("60000006-0000-0000-0000-000000000006"), "Heavy machinery and construction vehicles", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Construction Equipment" },
                    { new Guid("60000007-0000-0000-0000-000000000007"), "Performance tuning and modification", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Car Tuning" },
                    { new Guid("60000008-0000-0000-0000-000000000008"), "Japanese domestic market vehicles", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "JDM Culture" },
                    { new Guid("70000001-0000-0000-0000-000000000001"), "Sci-fi literature and authors", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Science Fiction" },
                    { new Guid("70000002-0000-0000-0000-000000000002"), "Epic fantasy and world-building", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Fantasy Novels" },
                    { new Guid("70000003-0000-0000-0000-000000000003"), "Detective fiction and suspense", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Mystery & Thriller" },
                    { new Guid("70000004-0000-0000-0000-000000000004"), "Horror fiction and gothic literature", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Horror" },
                    { new Guid("70000005-0000-0000-0000-000000000005"), "Poetry reading and writing", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Poetry" },
                    { new Guid("70000006-0000-0000-0000-000000000006"), "Canonical and classic works", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Classic Literature" },
                    { new Guid("70000007-0000-0000-0000-000000000007"), "Sequential art and graphic novels", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Manga & Comics" },
                    { new Guid("70000008-0000-0000-0000-000000000008"), "Fiction writing and storytelling", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Creative Writing" },
                    { new Guid("80000001-0000-0000-0000-000000000001"), "BJJ grappling and ground fighting", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Brazilian Jiu-Jitsu" },
                    { new Guid("80000002-0000-0000-0000-000000000002"), "Thai boxing and kickboxing", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Muay Thai" },
                    { new Guid("80000003-0000-0000-0000-000000000003"), "Traditional karate styles", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Karate" },
                    { new Guid("80000004-0000-0000-0000-000000000004"), "Korean martial art and Olympic sport", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Taekwondo" },
                    { new Guid("80000005-0000-0000-0000-000000000005"), "Israeli self-defense system", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Krav Maga" },
                    { new Guid("80000006-0000-0000-0000-000000000006"), "Western boxing and pugilism", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Boxing" },
                    { new Guid("80000007-0000-0000-0000-000000000007"), "Chinese martial arts styles", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Kung Fu" },
                    { new Guid("80000008-0000-0000-0000-000000000008"), "Mixed martial arts and UFC", null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "MMA" },
                    { new Guid("90000001-0000-0000-0000-000000000001"), "1980s movies, music, and trends", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "80s Pop Culture" },
                    { new Guid("90000002-0000-0000-0000-000000000002"), "1990s pop culture and nostalgia", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "90s Nostalgia" },
                    { new Guid("90000003-0000-0000-0000-000000000003"), "Japanese animation and culture", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Anime" },
                    { new Guid("90000004-0000-0000-0000-000000000004"), "Marvel, DC, and indie comics", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Comic Books" },
                    { new Guid("90000005-0000-0000-0000-000000000005"), "Toy collecting and vintage figures", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Action Figures" },
                    { new Guid("90000006-0000-0000-0000-000000000006"), "Vintage computers and electronics", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Retro Tech" },
                    { new Guid("90000007-0000-0000-0000-000000000007"), "VHS tapes and physical media collecting", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "VHS & Physical Media" },
                    { new Guid("90000008-0000-0000-0000-000000000008"), "Arcade machines and pinball", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Arcade Culture" },
                    { new Guid("a0000001-0000-0000-0000-000000000001"), "Trail hiking and backpacking", null, new Guid("11111111-1111-1111-1111-111111111111"), "Hiking" },
                    { new Guid("a0000002-0000-0000-0000-000000000002"), "Outdoor camping and survival", null, new Guid("11111111-1111-1111-1111-111111111111"), "Camping" },
                    { new Guid("a0000003-0000-0000-0000-000000000003"), "Indoor and outdoor climbing", null, new Guid("11111111-1111-1111-1111-111111111111"), "Rock Climbing" },
                    { new Guid("a0000004-0000-0000-0000-000000000004"), "Kayaking and canoeing", null, new Guid("11111111-1111-1111-1111-111111111111"), "Kayaking" },
                    { new Guid("a0000005-0000-0000-0000-000000000005"), "Off-road cycling", null, new Guid("11111111-1111-1111-1111-111111111111"), "Mountain Biking" },
                    { new Guid("a0000006-0000-0000-0000-000000000006"), "River rafting and rapids navigation", null, new Guid("11111111-1111-1111-1111-111111111111"), "White Water Rafting" },
                    { new Guid("a0000007-0000-0000-0000-000000000007"), "Bushcraft and wilderness survival skills", null, new Guid("11111111-1111-1111-1111-111111111111"), "Primitive Survival" },
                    { new Guid("a0000008-0000-0000-0000-000000000008"), "Wild edibles and mushroom hunting", null, new Guid("11111111-1111-1111-1111-111111111111"), "Foraging" },
                    { new Guid("a1000001-0000-0000-0000-000000000001"), "Roman Empire and civilization", null, new Guid("10101010-1010-1010-1010-101010101010"), "Ancient Rome" },
                    { new Guid("a1000002-0000-0000-0000-000000000002"), "Middle Ages and feudalism", null, new Guid("10101010-1010-1010-1010-101010101010"), "Medieval History" },
                    { new Guid("a1000003-0000-0000-0000-000000000003"), "WWII history and battles", null, new Guid("10101010-1010-1010-1010-101010101010"), "World War II" },
                    { new Guid("a1000004-0000-0000-0000-000000000004"), "Egyptian civilization and archaeology", null, new Guid("10101010-1010-1010-1010-101010101010"), "Ancient Egypt" },
                    { new Guid("a1000005-0000-0000-0000-000000000005"), "Norse culture and Viking Age", null, new Guid("10101010-1010-1010-1010-101010101010"), "Vikings" },
                    { new Guid("a1000006-0000-0000-0000-000000000006"), "Civil War history and reenactment", null, new Guid("10101010-1010-1010-1010-101010101010"), "American Civil War" },
                    { new Guid("a1000007-0000-0000-0000-000000000007"), "Archaeological discoveries and digs", null, new Guid("10101010-1010-1010-1010-101010101010"), "Archaeology" },
                    { new Guid("a1000008-0000-0000-0000-000000000008"), "Cold War era and espionage", null, new Guid("10101010-1010-1010-1010-101010101010"), "Cold War" },
                    { new Guid("b0000001-0000-0000-0000-000000000001"), "Computer gaming", null, new Guid("22222222-2222-2222-2222-222222222222"), "PC Gaming" },
                    { new Guid("b0000002-0000-0000-0000-000000000002"), "PlayStation, Xbox, Nintendo", null, new Guid("22222222-2222-2222-2222-222222222222"), "Console Gaming" },
                    { new Guid("b0000003-0000-0000-0000-000000000003"), "Strategy and party board games", null, new Guid("22222222-2222-2222-2222-222222222222"), "Board Games" },
                    { new Guid("b0000004-0000-0000-0000-000000000004"), "D&D, Pathfinder, and tabletop RPGs", null, new Guid("22222222-2222-2222-2222-222222222222"), "TTRPGs" },
                    { new Guid("b0000005-0000-0000-0000-000000000005"), "TCGs, poker, and card games", null, new Guid("22222222-2222-2222-2222-222222222222"), "Card Games" },
                    { new Guid("b0000006-0000-0000-0000-000000000006"), "Classic consoles and arcade games", null, new Guid("22222222-2222-2222-2222-222222222222"), "Retro Gaming" },
                    { new Guid("b0000007-0000-0000-0000-000000000007"), "Video game speedrunning", null, new Guid("22222222-2222-2222-2222-222222222222"), "Speedrunning" },
                    { new Guid("c0000001-0000-0000-0000-000000000001"), "Gundam model kits and gunpla", null, new Guid("33333333-3333-3333-3333-333333333333"), "Gundam" },
                    { new Guid("c0000002-0000-0000-0000-000000000002"), "Model trains and railroading layouts", null, new Guid("33333333-3333-3333-3333-333333333333"), "Model Trains" },
                    { new Guid("c0000003-0000-0000-0000-000000000003"), "Scale model vehicles", null, new Guid("33333333-3333-3333-3333-333333333333"), "Model Cars" },
                    { new Guid("c0000004-0000-0000-0000-000000000004"), "Warhammer and tabletop minis", null, new Guid("33333333-3333-3333-3333-333333333333"), "Miniature Painting" },
                    { new Guid("c0000005-0000-0000-0000-000000000005"), "Remote control cars and drones", null, new Guid("33333333-3333-3333-3333-333333333333"), "RC Vehicles" },
                    { new Guid("c0000006-0000-0000-0000-000000000006"), "Scale model planes and helicopters", null, new Guid("33333333-3333-3333-3333-333333333333"), "Model Aircraft" },
                    { new Guid("c0000007-0000-0000-0000-000000000007"), "Die-cast vehicle collections", null, new Guid("33333333-3333-3333-3333-333333333333"), "Die-Cast Collecting" },
                    { new Guid("d0000001-0000-0000-0000-000000000001"), "Yoga and meditation", null, new Guid("44444444-4444-4444-4444-444444444444"), "Yoga" },
                    { new Guid("d0000002-0000-0000-0000-000000000002"), "Running and marathons", null, new Guid("44444444-4444-4444-4444-444444444444"), "Running" },
                    { new Guid("d0000003-0000-0000-0000-000000000003"), "Strength training and bodybuilding", null, new Guid("44444444-4444-4444-4444-444444444444"), "Weightlifting" },
                    { new Guid("d0000004-0000-0000-0000-000000000004"), "High-intensity functional fitness", null, new Guid("44444444-4444-4444-4444-444444444444"), "CrossFit" },
                    { new Guid("d0000005-0000-0000-0000-000000000005"), "Swimming and aquatic sports", null, new Guid("44444444-4444-4444-4444-444444444444"), "Swimming" },
                    { new Guid("d0000006-0000-0000-0000-000000000006"), "Road and track cycling", null, new Guid("44444444-4444-4444-4444-444444444444"), "Cycling" },
                    { new Guid("e0000001-0000-0000-0000-000000000001"), "Canvas and fine art painting", null, new Guid("55555555-5555-5555-5555-555555555555"), "Painting" },
                    { new Guid("e0000002-0000-0000-0000-000000000002"), "Sketching and illustration", null, new Guid("55555555-5555-5555-5555-555555555555"), "Drawing" },
                    { new Guid("e0000003-0000-0000-0000-000000000003"), "Digital and film photography", null, new Guid("55555555-5555-5555-5555-555555555555"), "Photography" },
                    { new Guid("e0000004-0000-0000-0000-000000000004"), "Ceramics and pottery", null, new Guid("55555555-5555-5555-5555-555555555555"), "Pottery" },
                    { new Guid("e0000005-0000-0000-0000-000000000005"), "Fiber arts and textile crafts", null, new Guid("55555555-5555-5555-5555-555555555555"), "Knitting & Crochet" },
                    { new Guid("e0000006-0000-0000-0000-000000000006"), "Digital illustration and design", null, new Guid("55555555-5555-5555-5555-555555555555"), "Digital Art" },
                    { new Guid("e0000007-0000-0000-0000-000000000007"), "Hand lettering and calligraphy", null, new Guid("55555555-5555-5555-5555-555555555555"), "Calligraphy" },
                    { new Guid("f0000001-0000-0000-0000-000000000001"), "Acoustic and electric guitar", null, new Guid("66666666-6666-6666-6666-666666666666"), "Guitar" },
                    { new Guid("f0000002-0000-0000-0000-000000000002"), "Piano and keyboard", null, new Guid("66666666-6666-6666-6666-666666666666"), "Piano" },
                    { new Guid("f0000003-0000-0000-0000-000000000003"), "DJ and electronic music", null, new Guid("66666666-6666-6666-6666-666666666666"), "DJing" },
                    { new Guid("f0000004-0000-0000-0000-000000000004"), "Concert going and music events", null, new Guid("66666666-6666-6666-6666-666666666666"), "Live Concerts" },
                    { new Guid("f0000005-0000-0000-0000-000000000005"), "Audio production and recording", null, new Guid("66666666-6666-6666-6666-666666666666"), "Music Production" },
                    { new Guid("f0000006-0000-0000-0000-000000000006"), "Record collecting and turntables", null, new Guid("66666666-6666-6666-6666-666666666666"), "Vinyl Collecting" },
                    { new Guid("f0000007-0000-0000-0000-000000000007"), "Drumming and percussion instruments", null, new Guid("66666666-6666-6666-6666-666666666666"), "Drums & Percussion" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_EventId_UserId",
                table: "EventAttendees",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_UserId",
                table: "EventAttendees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_InterestTagId",
                table: "Events",
                column: "InterestTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerId",
                table: "Events",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubInterests_InterestId",
                table: "SubInterests",
                column: "InterestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_SubInterestId",
                table: "UserInterests",
                column: "SubInterestId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserId_SubInterestId",
                table: "UserInterests",
                columns: new[] { "UserId", "SubInterestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_UserProfileId",
                table: "UserInterests",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAttendees");

            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "SubInterests");

            migrationBuilder.DropTable(
                name: "Interests");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
