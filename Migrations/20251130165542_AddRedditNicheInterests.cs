using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Diversion.Migrations
{
    /// <inheritdoc />
    public partial class AddRedditNicheInterests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Interests",
                columns: new[] { "Id", "Description", "IconUrl", "Name" },
                values: new object[,]
                {
                    { new Guid("11111112-1111-1111-1111-111111111111"), "Animal care, wildlife, and nature", null, "Animals & Wildlife" },
                    { new Guid("22222223-2222-2222-2222-222222222222"), "Philosophy, spirituality, and existential topics", null, "Philosophy & Spirituality" },
                    { new Guid("33333334-3333-3333-3333-333333333333"), "True crime, forensics, and criminal justice", null, "True Crime & Investigation" },
                    { new Guid("44444445-4444-4444-4444-444444444444"), "Stock market, trading, and personal finance", null, "Finance & Investing" },
                    { new Guid("55555556-5555-5555-5555-555555555555"), "Language study, linguistics, and communication", null, "Linguistics & Languages" },
                    { new Guid("88888889-8888-8888-8888-888888888888"), "World mythology, folklore, and legends", null, "Mythology & Folklore" }
                });

            migrationBuilder.InsertData(
                table: "SubInterests",
                columns: new[] { "Id", "Description", "IconUrl", "InterestId", "Name" },
                values: new object[,]
                {
                    { new Guid("20000008-0000-0000-0000-000000000008"), "Linux operating systems and open source", null, new Guid("88888888-8888-8888-8888-888888888888"), "Linux" },
                    { new Guid("20000009-0000-0000-0000-000000000009"), "Copyright, patents, and IP law", null, new Guid("88888888-8888-8888-8888-888888888888"), "Intellectual Property" },
                    { new Guid("20000010-0000-0000-0000-000000000010"), "Animatronic engineering and FNAF", null, new Guid("88888888-8888-8888-8888-888888888888"), "Animatronics" },
                    { new Guid("30000008-0000-0000-0000-000000000008"), "Cosmology and theoretical astrophysics", null, new Guid("99999999-9999-9999-9999-999999999999"), "Astrophysics" },
                    { new Guid("30000009-0000-0000-0000-000000000009"), "Light reflection and optical phenomena", null, new Guid("99999999-9999-9999-9999-999999999999"), "Optics & Light" },
                    { new Guid("30000010-0000-0000-0000-000000000010"), "Learning psychology and pedagogy", null, new Guid("99999999-9999-9999-9999-999999999999"), "Educational Theory" },
                    { new Guid("70000009-0000-0000-0000-000000000009"), "Warrior Cats book series fandom", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Warrior Cats" },
                    { new Guid("70000010-0000-0000-0000-000000000010"), "Japanese light novels and web novels", null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Light Novels" },
                    { new Guid("90000009-0000-0000-0000-000000000009"), "Godzilla, Mothra, and giant monsters", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Kaiju & Monsters" },
                    { new Guid("90000010-0000-0000-0000-000000000010"), "King of Pop music and legacy", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Michael Jackson" },
                    { new Guid("90000011-0000-0000-0000-000000000011"), "Digimon franchise and virtual pets", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Digimon" },
                    { new Guid("90000012-0000-0000-0000-000000000012"), "Early 2000s internet and lost media", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Obscure Internet History" },
                    { new Guid("90000013-0000-0000-0000-000000000013"), "South Park animated series", null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "South Park" },
                    { new Guid("a0000009-0000-0000-0000-000000000009"), "Emergency medical skills and survival", null, new Guid("11111111-1111-1111-1111-111111111111"), "First Aid & Lifesaving" },
                    { new Guid("a1000009-0000-0000-0000-000000000009"), "American frontier and Wild West history", null, new Guid("10101010-1010-1010-1010-101010101010"), "Old West" },
                    { new Guid("a1000010-0000-0000-0000-000000000010"), "Japanese samurai and feudal era", null, new Guid("10101010-1010-1010-1010-101010101010"), "Samurai History" },
                    { new Guid("b0000008-0000-0000-0000-000000000008"), "Pokemon games and trading cards", null, new Guid("22222222-2222-2222-2222-222222222222"), "Pokemon" },
                    { new Guid("c0000008-0000-0000-0000-000000000008"), "Family history and ancestry research", null, new Guid("33333333-3333-3333-3333-333333333333"), "Genealogy" },
                    { new Guid("c0000009-0000-0000-0000-000000000009"), "Wand collecting and wood characteristics", null, new Guid("33333333-3333-3333-3333-333333333333"), "Wooden Wands" },
                    { new Guid("c0000010-0000-0000-0000-000000000010"), "Historical and decorative swords", null, new Guid("33333333-3333-3333-3333-333333333333"), "Sword Collecting" },
                    { new Guid("c0000011-0000-0000-0000-000000000011"), "Firearm history and mechanics", null, new Guid("33333333-3333-3333-3333-333333333333"), "Firearms" },
                    { new Guid("c0000012-0000-0000-0000-000000000012"), "American Civil War era weapons", null, new Guid("33333333-3333-3333-3333-333333333333"), "Civil War Firearms" },
                    { new Guid("c0000013-0000-0000-0000-000000000013"), "Folding knives and tactical blades", null, new Guid("33333333-3333-3333-3333-333333333333"), "Knife Collecting" },
                    { new Guid("c0000015-0000-0000-0000-000000000015"), "Throwing knives and axes", null, new Guid("33333333-3333-3333-3333-333333333333"), "Knife Throwing" },
                    { new Guid("d0000007-0000-0000-0000-000000000007"), "Japanese sumo traditions and tournaments", null, new Guid("44444444-4444-4444-4444-444444444444"), "Sumo Wrestling" },
                    { new Guid("d0000008-0000-0000-0000-000000000008"), "Professional wrestling entertainment", null, new Guid("44444444-4444-4444-4444-444444444444"), "WWE Wrestling" },
                    { new Guid("d0000009-0000-0000-0000-000000000009"), "Rubik's cube speedsolving", null, new Guid("44444444-4444-4444-4444-444444444444"), "Speedcubing" },
                    { new Guid("d0000010-0000-0000-0000-000000000010"), "Japanese kendama toy tricks", null, new Guid("44444444-4444-4444-4444-444444444444"), "Kendama" },
                    { new Guid("d0000011-0000-0000-0000-000000000011"), "Ball and object juggling", null, new Guid("44444444-4444-4444-4444-444444444444"), "Juggling" },
                    { new Guid("e0000008-0000-0000-0000-000000000008"), "Plushie and soft toy creation", null, new Guid("55555555-5555-5555-5555-555555555555"), "Plush Making" },
                    { new Guid("e0000009-0000-0000-0000-000000000009"), "Vintage squeak toys and collectibles", null, new Guid("55555555-5555-5555-5555-555555555555"), "Vintage Toy Collecting" },
                    { new Guid("e0000010-0000-0000-0000-000000000010"), "Vintage camera collecting and restoration", null, new Guid("55555555-5555-5555-5555-555555555555"), "Antique Cameras" },
                    { new Guid("f0000008-0000-0000-0000-000000000008"), "Jazz music and improvisation", null, new Guid("66666666-6666-6666-6666-666666666666"), "Jazz" },
                    { new Guid("f0000009-0000-0000-0000-000000000009"), "Classical composers and orchestral music", null, new Guid("66666666-6666-6666-6666-666666666666"), "Classical Music" },
                    { new Guid("f0000010-0000-0000-0000-000000000010"), "Analog and digital synthesizers", null, new Guid("66666666-6666-6666-6666-666666666666"), "Synthesizers" },
                    { new Guid("f0000011-0000-0000-0000-000000000011"), "Dangdut, folk, and regional music", null, new Guid("66666666-6666-6666-6666-666666666666"), "World Music" },
                    { new Guid("13000001-0000-0000-0000-000000000001"), "Ancient Greek gods and heroes", null, new Guid("88888889-8888-8888-8888-888888888888"), "Greek Mythology" },
                    { new Guid("13000002-0000-0000-0000-000000000002"), "Viking gods and Norse legends", null, new Guid("88888889-8888-8888-8888-888888888888"), "Norse Mythology" },
                    { new Guid("13000003-0000-0000-0000-000000000003"), "Celtic and Irish folklore", null, new Guid("88888889-8888-8888-8888-888888888888"), "Irish Mythology" },
                    { new Guid("13000005-0000-0000-0000-000000000005"), "Cryptids and mysterious creatures", null, new Guid("88888889-8888-8888-8888-888888888888"), "Cryptozoology" },
                    { new Guid("13000006-0000-0000-0000-000000000006"), "Modern folklore and urban myths", null, new Guid("88888889-8888-8888-8888-888888888888"), "Urban Legends" },
                    { new Guid("b1000001-0000-0000-0000-000000000001"), "Guinea pig care and breeding", null, new Guid("11111112-1111-1111-1111-111111111111"), "Guinea Pigs" },
                    { new Guid("b1000002-0000-0000-0000-000000000002"), "Kiwi biology and conservation", null, new Guid("11111112-1111-1111-1111-111111111111"), "Kiwi Birds" },
                    { new Guid("b1000003-0000-0000-0000-000000000003"), "Rabbit care and husbandry", null, new Guid("11111112-1111-1111-1111-111111111111"), "Rabbits" },
                    { new Guid("b1000004-0000-0000-0000-000000000004"), "Study of ants and ant colonies", null, new Guid("11111112-1111-1111-1111-111111111111"), "Myrmecology" },
                    { new Guid("b1000005-0000-0000-0000-000000000005"), "Indoor plant care and propagation", null, new Guid("11111112-1111-1111-1111-111111111111"), "Houseplants" },
                    { new Guid("b1000006-0000-0000-0000-000000000006"), "Bird identification and observation", null, new Guid("11111112-1111-1111-1111-111111111111"), "Birdwatching" },
                    { new Guid("c1000001-0000-0000-0000-000000000001"), "Existential philosophy and meaning", null, new Guid("22222223-2222-2222-2222-222222222222"), "Existentialism" },
                    { new Guid("c1000002-0000-0000-0000-000000000002"), "Philosophy of the state and governance", null, new Guid("22222223-2222-2222-2222-222222222222"), "Political Philosophy" },
                    { new Guid("c1000003-0000-0000-0000-000000000003"), "UFO phenomena and alien contact", null, new Guid("22222223-2222-2222-2222-222222222222"), "UFOs & Extraterrestrials" },
                    { new Guid("c1000004-0000-0000-0000-000000000004"), "Meditation practices and mindfulness", null, new Guid("22222223-2222-2222-2222-222222222222"), "Meditation & Mindfulness" },
                    { new Guid("c1000005-0000-0000-0000-000000000005"), "Jewish mysticism and Hermetic texts", null, new Guid("22222223-2222-2222-2222-222222222222"), "Qabalah & Mysticism" },
                    { new Guid("c1000006-0000-0000-0000-000000000006"), "Tao, Bhagavad Gita, and Eastern thought", null, new Guid("22222223-2222-2222-2222-222222222222"), "Eastern Philosophy" },
                    { new Guid("c1000007-0000-0000-0000-000000000007"), "Reality simulation and consciousness", null, new Guid("22222223-2222-2222-2222-222222222222"), "Simulation Theory" },
                    { new Guid("d1000001-0000-0000-0000-000000000001"), "Serial killer investigations and psychology", null, new Guid("33333334-3333-3333-3333-333333333333"), "Serial Killers" },
                    { new Guid("d1000002-0000-0000-0000-000000000002"), "Forensic science and crime scene analysis", null, new Guid("33333334-3333-3333-3333-333333333333"), "Forensics" },
                    { new Guid("d1000003-0000-0000-0000-000000000003"), "Unsolved mysteries and cold cases", null, new Guid("33333334-3333-3333-3333-333333333333"), "Cold Cases" },
                    { new Guid("d1000004-0000-0000-0000-000000000004"), "Regional crime history and stories", null, new Guid("33333334-3333-3333-3333-333333333333"), "Local Crime History" },
                    { new Guid("e1000001-0000-0000-0000-000000000001"), "Stock trading and market analysis", null, new Guid("44444445-4444-4444-4444-444444444444"), "Stock Market" },
                    { new Guid("e1000002-0000-0000-0000-000000000002"), "S&P 500 index options trading", null, new Guid("44444445-4444-4444-4444-444444444444"), "SPX Options" },
                    { new Guid("e1000003-0000-0000-0000-000000000003"), "Digital currencies and blockchain", null, new Guid("44444445-4444-4444-4444-444444444444"), "Cryptocurrency" },
                    { new Guid("e1000004-0000-0000-0000-000000000004"), "Active day trading strategies", null, new Guid("44444445-4444-4444-4444-444444444444"), "Day Trading" },
                    { new Guid("e1000005-0000-0000-0000-000000000005"), "Budgeting and financial planning", null, new Guid("44444445-4444-4444-4444-444444444444"), "Personal Finance" },
                    { new Guid("f1000001-0000-0000-0000-000000000001"), "Language structure and evolution", null, new Guid("55555556-5555-5555-5555-555555555555"), "Linguistics" },
                    { new Guid("f1000002-0000-0000-0000-000000000002"), "Speech sounds and pronunciation", null, new Guid("55555556-5555-5555-5555-555555555555"), "Phonetics" },
                    { new Guid("f1000003-0000-0000-0000-000000000003"), "Word origins and language history", null, new Guid("55555556-5555-5555-5555-555555555555"), "Etymology" },
                    { new Guid("f1000004-0000-0000-0000-000000000004"), "Conlangs like Esperanto and Klingon", null, new Guid("55555556-5555-5555-5555-555555555555"), "Constructed Languages" },
                    { new Guid("f1000005-0000-0000-0000-000000000005"), "ASL and sign language systems", null, new Guid("55555556-5555-5555-5555-555555555555"), "Sign Language" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("13000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("13000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("13000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("13000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("13000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("20000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("20000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("20000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("30000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("30000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("30000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("70000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("70000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("90000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("90000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("90000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("90000012-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("90000013-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("a0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("a1000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("a1000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("b1000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000012-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000013-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c0000015-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000006-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("c1000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d0000007-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d0000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("d1000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e1000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("e1000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f0000008-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f0000009-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f0000010-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f0000011-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f1000001-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f1000002-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f1000003-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f1000004-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "SubInterests",
                keyColumn: "Id",
                keyValue: new Guid("f1000005-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("11111112-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("22222223-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("33333334-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("44444445-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("55555556-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Interests",
                keyColumn: "Id",
                keyValue: new Guid("88888889-8888-8888-8888-888888888888"));
        }
    }
}
