using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Diversion.Models;

namespace Diversion
{
    public class DiversionDbContext(DbContextOptions<DiversionDbContext> options) : IdentityDbContext(options)
    {

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<SubInterest> SubInterests { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityMembership> CommunityMemberships { get; set; }
        public DbSet<CommunityMessage> CommunityMessages { get; set; }
        public DbSet<DirectMessage> DirectMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubInterest>()
                .HasOne(si => si.Interest)
                .WithMany(i => i.SubInterests)
                .HasForeignKey(si => si.InterestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserInterest>()
                .HasOne(ui => ui.SubInterest)
                .WithMany(si => si.UserInterests)
                .HasForeignKey(ui => ui.SubInterestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.InterestTag)
                .WithMany(si => si.Events)
                .HasForeignKey(e => e.InterestTagId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .Property(e => e.TicketPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.Attendees)
                .HasForeignKey(ea => ea.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany()
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventAttendee>()
                .HasIndex(ea => new { ea.EventId, ea.UserId })
                .IsUnique();

            modelBuilder.Entity<UserInterest>()
                .HasIndex(ui => new { ui.UserId, ui.SubInterestId })
                .IsUnique();

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friendship>()
                .HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique();

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => fr.SenderId);

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => fr.ReceiverId);

            modelBuilder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId, fr.Status });

            modelBuilder.Entity<Community>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Community>()
                .HasOne(c => c.Interest)
                .WithMany()
                .HasForeignKey(c => c.InterestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommunityMembership>()
                .HasOne(cm => cm.Community)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunityMembership>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunityMembership>()
                .HasIndex(cm => new { cm.CommunityId, cm.UserId })
                .IsUnique();

            modelBuilder.Entity<CommunityMessage>()
                .HasOne(cm => cm.Community)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.CommunityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommunityMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommunityMessage>()
                .HasOne(cm => cm.ReplyToMessage)
                .WithMany()
                .HasForeignKey(cm => cm.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommunityMessage>()
                .HasIndex(cm => new { cm.CommunityId, cm.SentAt });

            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Sender)
                .WithMany()
                .HasForeignKey(dm => dm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DirectMessage>()
                .HasOne(dm => dm.Receiver)
                .WithMany()
                .HasForeignKey(dm => dm.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DirectMessage>()
                .HasIndex(dm => new { dm.ReceiverId, dm.IsRead });

            modelBuilder.Entity<DirectMessage>()
                .HasIndex(dm => dm.SentAt);

            // Seed Data - Interests
            var outdoorsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var gamingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var modelsId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var fitnessId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var artsId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var musicId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var foodId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var techId = Guid.Parse("88888888-8888-8888-8888-888888888888");
            var scienceId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            var scifiId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var woodworkingId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var automotiveId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var literatureId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var martialArtsId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
            var popCultureId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var historyId = Guid.Parse("10101010-1010-1010-1010-101010101010");
            var animalsId = Guid.Parse("11111112-1111-1111-1111-111111111111");
            var philosophyId = Guid.Parse("22222223-2222-2222-2222-222222222222");
            var trueCrimeId = Guid.Parse("33333334-3333-3333-3333-333333333333");
            var financeId = Guid.Parse("44444445-4444-4444-4444-444444444444");
            var linguisticsId = Guid.Parse("55555556-5555-5555-5555-555555555555");
            var mythologyId = Guid.Parse("88888889-8888-8888-8888-888888888888");

            modelBuilder.Entity<Interest>().HasData(
                new Interest { Id = outdoorsId, Name = "Outdoors & Adventure", Description = "Outdoor activities and wilderness adventures" },
                new Interest { Id = gamingId, Name = "Gaming", Description = "Video games, board games, and tabletop gaming" },
                new Interest { Id = modelsId, Name = "Models & Collecting", Description = "Model building, collecting, and hobbies" },
                new Interest { Id = fitnessId, Name = "Fitness & Sports", Description = "Physical activities and sports" },
                new Interest { Id = artsId, Name = "Arts & Crafts", Description = "Creative and artistic pursuits" },
                new Interest { Id = musicId, Name = "Music", Description = "Music performance and appreciation" },
                new Interest { Id = foodId, Name = "Food & Drink", Description = "Culinary arts and beverages" },
                new Interest { Id = techId, Name = "Technology", Description = "Tech projects and programming" },
                new Interest { Id = scienceId, Name = "Science & Medicine", Description = "Scientific research and medical topics" },
                new Interest { Id = scifiId, Name = "Science Fiction & Fantasy", Description = "Sci-fi, fantasy, and speculative fiction" },
                new Interest { Id = woodworkingId, Name = "Woodworking & Craftsmanship", Description = "Woodworking and traditional crafts" },
                new Interest { Id = automotiveId, Name = "Automotive & Machinery", Description = "Cars, trucks, and heavy machinery" },
                new Interest { Id = literatureId, Name = "Literature & Writing", Description = "Books, writing, and storytelling" },
                new Interest { Id = martialArtsId, Name = "Martial Arts", Description = "Combat sports and martial disciplines" },
                new Interest { Id = popCultureId, Name = "Pop Culture & Nostalgia", Description = "Pop culture, retro trends, and media" },
                new Interest { Id = historyId, Name = "History & Archaeology", Description = "Historical events and ancient civilizations" },
                new Interest { Id = animalsId, Name = "Animals & Wildlife", Description = "Animal care, wildlife, and nature" },
                new Interest { Id = philosophyId, Name = "Philosophy & Spirituality", Description = "Philosophy, spirituality, and existential topics" },
                new Interest { Id = trueCrimeId, Name = "True Crime & Investigation", Description = "True crime, forensics, and criminal justice" },
                new Interest { Id = financeId, Name = "Finance & Investing", Description = "Stock market, trading, and personal finance" },
                new Interest { Id = linguisticsId, Name = "Linguistics & Languages", Description = "Language study, linguistics, and communication" },
                new Interest { Id = mythologyId, Name = "Mythology & Folklore", Description = "World mythology, folklore, and legends" }
            );

            // Seed Data - SubInterests
            modelBuilder.Entity<SubInterest>().HasData(
                // Outdoors & Adventure
                new SubInterest { Id = Guid.Parse("a0000001-0000-0000-0000-000000000001"), Name = "Hiking", InterestId = outdoorsId, Description = "Trail hiking and backpacking" },
                new SubInterest { Id = Guid.Parse("a0000002-0000-0000-0000-000000000002"), Name = "Camping", InterestId = outdoorsId, Description = "Outdoor camping and survival" },
                new SubInterest { Id = Guid.Parse("a0000003-0000-0000-0000-000000000003"), Name = "Rock Climbing", InterestId = outdoorsId, Description = "Indoor and outdoor climbing" },
                new SubInterest { Id = Guid.Parse("a0000004-0000-0000-0000-000000000004"), Name = "Kayaking", InterestId = outdoorsId, Description = "Kayaking and canoeing" },
                new SubInterest { Id = Guid.Parse("a0000005-0000-0000-0000-000000000005"), Name = "Mountain Biking", InterestId = outdoorsId, Description = "Off-road cycling" },
                new SubInterest { Id = Guid.Parse("a0000006-0000-0000-0000-000000000006"), Name = "White Water Rafting", InterestId = outdoorsId, Description = "River rafting and rapids navigation" },
                new SubInterest { Id = Guid.Parse("a0000007-0000-0000-0000-000000000007"), Name = "Primitive Survival", InterestId = outdoorsId, Description = "Bushcraft and wilderness survival skills" },
                new SubInterest { Id = Guid.Parse("a0000008-0000-0000-0000-000000000008"), Name = "Foraging", InterestId = outdoorsId, Description = "Wild edibles and mushroom hunting" },
                new SubInterest { Id = Guid.Parse("a0000009-0000-0000-0000-000000000009"), Name = "First Aid & Lifesaving", InterestId = outdoorsId, Description = "Emergency medical skills and survival" },

                // Gaming
                new SubInterest { Id = Guid.Parse("b0000001-0000-0000-0000-000000000001"), Name = "PC Gaming", InterestId = gamingId, Description = "Computer gaming" },
                new SubInterest { Id = Guid.Parse("b0000002-0000-0000-0000-000000000002"), Name = "Console Gaming", InterestId = gamingId, Description = "PlayStation, Xbox, Nintendo" },
                new SubInterest { Id = Guid.Parse("b0000003-0000-0000-0000-000000000003"), Name = "Board Games", InterestId = gamingId, Description = "Strategy and party board games" },
                new SubInterest { Id = Guid.Parse("b0000004-0000-0000-0000-000000000004"), Name = "TTRPGs", InterestId = gamingId, Description = "D&D, Pathfinder, and tabletop RPGs" },
                new SubInterest { Id = Guid.Parse("b0000005-0000-0000-0000-000000000005"), Name = "Card Games", InterestId = gamingId, Description = "TCGs, poker, and card games" },
                new SubInterest { Id = Guid.Parse("b0000006-0000-0000-0000-000000000006"), Name = "Retro Gaming", InterestId = gamingId, Description = "Classic consoles and arcade games" },
                new SubInterest { Id = Guid.Parse("b0000007-0000-0000-0000-000000000007"), Name = "Speedrunning", InterestId = gamingId, Description = "Video game speedrunning" },

                // Models & Collecting
                new SubInterest { Id = Guid.Parse("c0000001-0000-0000-0000-000000000001"), Name = "Gundam", InterestId = modelsId, Description = "Gundam model kits and gunpla" },
                new SubInterest { Id = Guid.Parse("c0000002-0000-0000-0000-000000000002"), Name = "Model Trains", InterestId = modelsId, Description = "Model trains and railroading layouts" },
                new SubInterest { Id = Guid.Parse("c0000003-0000-0000-0000-000000000003"), Name = "Model Cars", InterestId = modelsId, Description = "Scale model vehicles" },
                new SubInterest { Id = Guid.Parse("c0000004-0000-0000-0000-000000000004"), Name = "Miniature Painting", InterestId = modelsId, Description = "Warhammer and tabletop minis" },
                new SubInterest { Id = Guid.Parse("c0000005-0000-0000-0000-000000000005"), Name = "RC Vehicles", InterestId = modelsId, Description = "Remote control cars and drones" },
                new SubInterest { Id = Guid.Parse("c0000006-0000-0000-0000-000000000006"), Name = "Model Aircraft", InterestId = modelsId, Description = "Scale model planes and helicopters" },
                new SubInterest { Id = Guid.Parse("c0000007-0000-0000-0000-000000000007"), Name = "Die-Cast Collecting", InterestId = modelsId, Description = "Die-cast vehicle collections" },

                // Fitness & Sports
                new SubInterest { Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"), Name = "Yoga", InterestId = fitnessId, Description = "Yoga and meditation" },
                new SubInterest { Id = Guid.Parse("d0000002-0000-0000-0000-000000000002"), Name = "Running", InterestId = fitnessId, Description = "Running and marathons" },
                new SubInterest { Id = Guid.Parse("d0000003-0000-0000-0000-000000000003"), Name = "Weightlifting", InterestId = fitnessId, Description = "Strength training and bodybuilding" },
                new SubInterest { Id = Guid.Parse("d0000004-0000-0000-0000-000000000004"), Name = "CrossFit", InterestId = fitnessId, Description = "High-intensity functional fitness" },
                new SubInterest { Id = Guid.Parse("d0000005-0000-0000-0000-000000000005"), Name = "Swimming", InterestId = fitnessId, Description = "Swimming and aquatic sports" },
                new SubInterest { Id = Guid.Parse("d0000006-0000-0000-0000-000000000006"), Name = "Cycling", InterestId = fitnessId, Description = "Road and track cycling" },

                // Arts & Crafts
                new SubInterest { Id = Guid.Parse("e0000001-0000-0000-0000-000000000001"), Name = "Painting", InterestId = artsId, Description = "Canvas and fine art painting" },
                new SubInterest { Id = Guid.Parse("e0000002-0000-0000-0000-000000000002"), Name = "Drawing", InterestId = artsId, Description = "Sketching and illustration" },
                new SubInterest { Id = Guid.Parse("e0000003-0000-0000-0000-000000000003"), Name = "Photography", InterestId = artsId, Description = "Digital and film photography" },
                new SubInterest { Id = Guid.Parse("e0000004-0000-0000-0000-000000000004"), Name = "Pottery", InterestId = artsId, Description = "Ceramics and pottery" },
                new SubInterest { Id = Guid.Parse("e0000005-0000-0000-0000-000000000005"), Name = "Knitting & Crochet", InterestId = artsId, Description = "Fiber arts and textile crafts" },
                new SubInterest { Id = Guid.Parse("e0000006-0000-0000-0000-000000000006"), Name = "Digital Art", InterestId = artsId, Description = "Digital illustration and design" },
                new SubInterest { Id = Guid.Parse("e0000007-0000-0000-0000-000000000007"), Name = "Calligraphy", InterestId = artsId, Description = "Hand lettering and calligraphy" },

                // Music
                new SubInterest { Id = Guid.Parse("f0000001-0000-0000-0000-000000000001"), Name = "Guitar", InterestId = musicId, Description = "Acoustic and electric guitar" },
                new SubInterest { Id = Guid.Parse("f0000002-0000-0000-0000-000000000002"), Name = "Piano", InterestId = musicId, Description = "Piano and keyboard" },
                new SubInterest { Id = Guid.Parse("f0000003-0000-0000-0000-000000000003"), Name = "DJing", InterestId = musicId, Description = "DJ and electronic music" },
                new SubInterest { Id = Guid.Parse("f0000004-0000-0000-0000-000000000004"), Name = "Live Concerts", InterestId = musicId, Description = "Concert going and music events" },
                new SubInterest { Id = Guid.Parse("f0000005-0000-0000-0000-000000000005"), Name = "Music Production", InterestId = musicId, Description = "Audio production and recording" },
                new SubInterest { Id = Guid.Parse("f0000006-0000-0000-0000-000000000006"), Name = "Vinyl Collecting", InterestId = musicId, Description = "Record collecting and turntables" },
                new SubInterest { Id = Guid.Parse("f0000007-0000-0000-0000-000000000007"), Name = "Drums & Percussion", InterestId = musicId, Description = "Drumming and percussion instruments" },

                // Food & Drink
                new SubInterest { Id = Guid.Parse("10000001-0000-0000-0000-000000000001"), Name = "Cooking", InterestId = foodId, Description = "Home cooking and culinary arts" },
                new SubInterest { Id = Guid.Parse("10000002-0000-0000-0000-000000000002"), Name = "Baking", InterestId = foodId, Description = "Baking and pastry" },
                new SubInterest { Id = Guid.Parse("10000003-0000-0000-0000-000000000003"), Name = "Coffee", InterestId = foodId, Description = "Coffee brewing and appreciation" },
                new SubInterest { Id = Guid.Parse("10000004-0000-0000-0000-000000000004"), Name = "Craft Beer", InterestId = foodId, Description = "Beer tasting and brewing" },
                new SubInterest { Id = Guid.Parse("10000005-0000-0000-0000-000000000005"), Name = "Wine Tasting", InterestId = foodId, Description = "Wine appreciation and oenology" },
                new SubInterest { Id = Guid.Parse("10000006-0000-0000-0000-000000000006"), Name = "Sourdough Bread", InterestId = foodId, Description = "Sourdough baking and fermentation" },
                new SubInterest { Id = Guid.Parse("10000007-0000-0000-0000-000000000007"), Name = "BBQ & Smoking", InterestId = foodId, Description = "BBQ techniques and meat smoking" },

                // Technology
                new SubInterest { Id = Guid.Parse("20000001-0000-0000-0000-000000000001"), Name = "Programming", InterestId = techId, Description = "Software development and coding" },
                new SubInterest { Id = Guid.Parse("20000002-0000-0000-0000-000000000002"), Name = "3D Printing", InterestId = techId, Description = "3D printing and modeling" },
                new SubInterest { Id = Guid.Parse("20000003-0000-0000-0000-000000000003"), Name = "Electronics", InterestId = techId, Description = "Electronics and circuit building" },
                new SubInterest { Id = Guid.Parse("20000004-0000-0000-0000-000000000004"), Name = "Robotics", InterestId = techId, Description = "Robot building and automation" },
                new SubInterest { Id = Guid.Parse("20000005-0000-0000-0000-000000000005"), Name = "AI & Machine Learning", InterestId = techId, Description = "AI, ML, and data science" },
                new SubInterest { Id = Guid.Parse("20000006-0000-0000-0000-000000000006"), Name = "Cybersecurity", InterestId = techId, Description = "InfoSec and ethical hacking" },
                new SubInterest { Id = Guid.Parse("20000007-0000-0000-0000-000000000007"), Name = "Homelab", InterestId = techId, Description = "Home servers and networking" },

                // Science & Medicine
                new SubInterest { Id = Guid.Parse("30000001-0000-0000-0000-000000000001"), Name = "Medical Research", InterestId = scienceId, Description = "Medical news and breakthrough research" },
                new SubInterest { Id = Guid.Parse("30000002-0000-0000-0000-000000000002"), Name = "Astronomy", InterestId = scienceId, Description = "Space, stars, and celestial phenomena" },
                new SubInterest { Id = Guid.Parse("30000003-0000-0000-0000-000000000003"), Name = "Physics", InterestId = scienceId, Description = "Theoretical and applied physics" },
                new SubInterest { Id = Guid.Parse("30000004-0000-0000-0000-000000000004"), Name = "Biology & Ecology", InterestId = scienceId, Description = "Life sciences and ecosystems" },
                new SubInterest { Id = Guid.Parse("30000005-0000-0000-0000-000000000005"), Name = "Chemistry", InterestId = scienceId, Description = "Chemical reactions and compounds" },
                new SubInterest { Id = Guid.Parse("30000006-0000-0000-0000-000000000006"), Name = "Neuroscience", InterestId = scienceId, Description = "Brain research and cognitive science" },
                new SubInterest { Id = Guid.Parse("30000007-0000-0000-0000-000000000007"), Name = "Genetics", InterestId = scienceId, Description = "DNA, heredity, and genetic engineering" },

                // Science Fiction & Fantasy
                new SubInterest { Id = Guid.Parse("40000001-0000-0000-0000-000000000001"), Name = "Time Travel", InterestId = scifiId, Description = "Time travel theory and paradoxes" },
                new SubInterest { Id = Guid.Parse("40000002-0000-0000-0000-000000000002"), Name = "Zombies", InterestId = scifiId, Description = "Zombie apocalypse and survival" },
                new SubInterest { Id = Guid.Parse("40000003-0000-0000-0000-000000000003"), Name = "Space Opera", InterestId = scifiId, Description = "Star Wars, Star Trek, and space epics" },
                new SubInterest { Id = Guid.Parse("40000004-0000-0000-0000-000000000004"), Name = "Cyberpunk", InterestId = scifiId, Description = "Dystopian tech futures" },
                new SubInterest { Id = Guid.Parse("40000005-0000-0000-0000-000000000005"), Name = "Fantasy Worlds", InterestId = scifiId, Description = "High fantasy and world-building" },
                new SubInterest { Id = Guid.Parse("40000006-0000-0000-0000-000000000006"), Name = "Aliens & UFOs", InterestId = scifiId, Description = "Extraterrestrial life and phenomena" },
                new SubInterest { Id = Guid.Parse("40000007-0000-0000-0000-000000000007"), Name = "Alternate History", InterestId = scifiId, Description = "What-if scenarios and alternate timelines" },
                new SubInterest { Id = Guid.Parse("40000008-0000-0000-0000-000000000008"), Name = "Post-Apocalyptic", InterestId = scifiId, Description = "End-of-world scenarios and survival" },

                // Woodworking & Craftsmanship
                new SubInterest { Id = Guid.Parse("50000001-0000-0000-0000-000000000001"), Name = "Furniture Making", InterestId = woodworkingId, Description = "Custom furniture and cabinetry" },
                new SubInterest { Id = Guid.Parse("50000002-0000-0000-0000-000000000002"), Name = "Wood Turning", InterestId = woodworkingId, Description = "Lathe work and bowl turning" },
                new SubInterest { Id = Guid.Parse("50000003-0000-0000-0000-000000000003"), Name = "Wood Species", InterestId = woodworkingId, Description = "Exotic woods and lumber identification" },
                new SubInterest { Id = Guid.Parse("50000004-0000-0000-0000-000000000004"), Name = "Carving", InterestId = woodworkingId, Description = "Wood carving and whittling" },
                new SubInterest { Id = Guid.Parse("50000005-0000-0000-0000-000000000005"), Name = "Joinery", InterestId = woodworkingId, Description = "Traditional joinery techniques" },
                new SubInterest { Id = Guid.Parse("50000006-0000-0000-0000-000000000006"), Name = "Wood Finishing", InterestId = woodworkingId, Description = "Stains, oils, and finishing techniques" },
                new SubInterest { Id = Guid.Parse("50000007-0000-0000-0000-000000000007"), Name = "Hand Tools", InterestId = woodworkingId, Description = "Traditional hand tool woodworking" },

                // Automotive & Machinery
                new SubInterest { Id = Guid.Parse("60000001-0000-0000-0000-000000000001"), Name = "Classic Cars", InterestId = automotiveId, Description = "Vintage and classic automobiles" },
                new SubInterest { Id = Guid.Parse("60000002-0000-0000-0000-000000000002"), Name = "Trucks & Off-Road", InterestId = automotiveId, Description = "Pickup trucks and 4x4 vehicles" },
                new SubInterest { Id = Guid.Parse("60000003-0000-0000-0000-000000000003"), Name = "Auto Restoration", InterestId = automotiveId, Description = "Vehicle restoration and rebuilding" },
                new SubInterest { Id = Guid.Parse("60000004-0000-0000-0000-000000000004"), Name = "Muscle Cars", InterestId = automotiveId, Description = "American muscle and performance cars" },
                new SubInterest { Id = Guid.Parse("60000005-0000-0000-0000-000000000005"), Name = "Motorcycles", InterestId = automotiveId, Description = "Motorcycles and bike culture" },
                new SubInterest { Id = Guid.Parse("60000006-0000-0000-0000-000000000006"), Name = "Construction Equipment", InterestId = automotiveId, Description = "Heavy machinery and construction vehicles" },
                new SubInterest { Id = Guid.Parse("60000007-0000-0000-0000-000000000007"), Name = "Car Tuning", InterestId = automotiveId, Description = "Performance tuning and modification" },
                new SubInterest { Id = Guid.Parse("60000008-0000-0000-0000-000000000008"), Name = "JDM Culture", InterestId = automotiveId, Description = "Japanese domestic market vehicles" },

                // Literature & Writing
                new SubInterest { Id = Guid.Parse("70000001-0000-0000-0000-000000000001"), Name = "Science Fiction", InterestId = literatureId, Description = "Sci-fi literature and authors" },
                new SubInterest { Id = Guid.Parse("70000002-0000-0000-0000-000000000002"), Name = "Fantasy Novels", InterestId = literatureId, Description = "Epic fantasy and world-building" },
                new SubInterest { Id = Guid.Parse("70000003-0000-0000-0000-000000000003"), Name = "Mystery & Thriller", InterestId = literatureId, Description = "Detective fiction and suspense" },
                new SubInterest { Id = Guid.Parse("70000004-0000-0000-0000-000000000004"), Name = "Horror", InterestId = literatureId, Description = "Horror fiction and gothic literature" },
                new SubInterest { Id = Guid.Parse("70000005-0000-0000-0000-000000000005"), Name = "Poetry", InterestId = literatureId, Description = "Poetry reading and writing" },
                new SubInterest { Id = Guid.Parse("70000006-0000-0000-0000-000000000006"), Name = "Classic Literature", InterestId = literatureId, Description = "Canonical and classic works" },
                new SubInterest { Id = Guid.Parse("70000007-0000-0000-0000-000000000007"), Name = "Manga & Comics", InterestId = literatureId, Description = "Sequential art and graphic novels" },
                new SubInterest { Id = Guid.Parse("70000008-0000-0000-0000-000000000008"), Name = "Creative Writing", InterestId = literatureId, Description = "Fiction writing and storytelling" },

                // Martial Arts
                new SubInterest { Id = Guid.Parse("80000001-0000-0000-0000-000000000001"), Name = "Brazilian Jiu-Jitsu", InterestId = martialArtsId, Description = "BJJ grappling and ground fighting" },
                new SubInterest { Id = Guid.Parse("80000002-0000-0000-0000-000000000002"), Name = "Muay Thai", InterestId = martialArtsId, Description = "Thai boxing and kickboxing" },
                new SubInterest { Id = Guid.Parse("80000003-0000-0000-0000-000000000003"), Name = "Karate", InterestId = martialArtsId, Description = "Traditional karate styles" },
                new SubInterest { Id = Guid.Parse("80000004-0000-0000-0000-000000000004"), Name = "Taekwondo", InterestId = martialArtsId, Description = "Korean martial art and Olympic sport" },
                new SubInterest { Id = Guid.Parse("80000005-0000-0000-0000-000000000005"), Name = "Krav Maga", InterestId = martialArtsId, Description = "Israeli self-defense system" },
                new SubInterest { Id = Guid.Parse("80000006-0000-0000-0000-000000000006"), Name = "Boxing", InterestId = martialArtsId, Description = "Western boxing and pugilism" },
                new SubInterest { Id = Guid.Parse("80000007-0000-0000-0000-000000000007"), Name = "Kung Fu", InterestId = martialArtsId, Description = "Chinese martial arts styles" },
                new SubInterest { Id = Guid.Parse("80000008-0000-0000-0000-000000000008"), Name = "MMA", InterestId = martialArtsId, Description = "Mixed martial arts and UFC" },

                // Pop Culture & Nostalgia
                new SubInterest { Id = Guid.Parse("90000001-0000-0000-0000-000000000001"), Name = "80s Pop Culture", InterestId = popCultureId, Description = "1980s movies, music, and trends" },
                new SubInterest { Id = Guid.Parse("90000002-0000-0000-0000-000000000002"), Name = "90s Nostalgia", InterestId = popCultureId, Description = "1990s pop culture and nostalgia" },
                new SubInterest { Id = Guid.Parse("90000003-0000-0000-0000-000000000003"), Name = "Anime", InterestId = popCultureId, Description = "Japanese animation and culture" },
                new SubInterest { Id = Guid.Parse("90000004-0000-0000-0000-000000000004"), Name = "Comic Books", InterestId = popCultureId, Description = "Marvel, DC, and indie comics" },
                new SubInterest { Id = Guid.Parse("90000005-0000-0000-0000-000000000005"), Name = "Action Figures", InterestId = popCultureId, Description = "Toy collecting and vintage figures" },
                new SubInterest { Id = Guid.Parse("90000006-0000-0000-0000-000000000006"), Name = "Retro Tech", InterestId = popCultureId, Description = "Vintage computers and electronics" },
                new SubInterest { Id = Guid.Parse("90000007-0000-0000-0000-000000000007"), Name = "VHS & Physical Media", InterestId = popCultureId, Description = "VHS tapes and physical media collecting" },
                new SubInterest { Id = Guid.Parse("90000008-0000-0000-0000-000000000008"), Name = "Arcade Culture", InterestId = popCultureId, Description = "Arcade machines and pinball" },

                // History & Archaeology
                new SubInterest { Id = Guid.Parse("a1000001-0000-0000-0000-000000000001"), Name = "Ancient Rome", InterestId = historyId, Description = "Roman Empire and civilization" },
                new SubInterest { Id = Guid.Parse("a1000002-0000-0000-0000-000000000002"), Name = "Medieval History", InterestId = historyId, Description = "Middle Ages and feudalism" },
                new SubInterest { Id = Guid.Parse("a1000003-0000-0000-0000-000000000003"), Name = "World War II", InterestId = historyId, Description = "WWII history and battles" },
                new SubInterest { Id = Guid.Parse("a1000004-0000-0000-0000-000000000004"), Name = "Ancient Egypt", InterestId = historyId, Description = "Egyptian civilization and archaeology" },
                new SubInterest { Id = Guid.Parse("a1000005-0000-0000-0000-000000000005"), Name = "Vikings", InterestId = historyId, Description = "Norse culture and Viking Age" },
                new SubInterest { Id = Guid.Parse("a1000006-0000-0000-0000-000000000006"), Name = "American Civil War", InterestId = historyId, Description = "Civil War history and reenactment" },
                new SubInterest { Id = Guid.Parse("a1000007-0000-0000-0000-000000000007"), Name = "Archaeology", InterestId = historyId, Description = "Archaeological discoveries and digs" },
                new SubInterest { Id = Guid.Parse("a1000008-0000-0000-0000-000000000008"), Name = "Cold War", InterestId = historyId, Description = "Cold War era and espionage" },
                new SubInterest { Id = Guid.Parse("a1000009-0000-0000-0000-000000000009"), Name = "Old West", InterestId = historyId, Description = "American frontier and Wild West history" },
                new SubInterest { Id = Guid.Parse("a1000010-0000-0000-0000-000000000010"), Name = "Samurai History", InterestId = historyId, Description = "Japanese samurai and feudal era" },

                // Animals & Wildlife
                new SubInterest { Id = Guid.Parse("b1000001-0000-0000-0000-000000000001"), Name = "Guinea Pigs", InterestId = animalsId, Description = "Guinea pig care and breeding" },
                new SubInterest { Id = Guid.Parse("b1000002-0000-0000-0000-000000000002"), Name = "Kiwi Birds", InterestId = animalsId, Description = "Kiwi biology and conservation" },
                new SubInterest { Id = Guid.Parse("b1000003-0000-0000-0000-000000000003"), Name = "Rabbits", InterestId = animalsId, Description = "Rabbit care and husbandry" },
                new SubInterest { Id = Guid.Parse("b1000004-0000-0000-0000-000000000004"), Name = "Myrmecology", InterestId = animalsId, Description = "Study of ants and ant colonies" },
                new SubInterest { Id = Guid.Parse("b1000005-0000-0000-0000-000000000005"), Name = "Houseplants", InterestId = animalsId, Description = "Indoor plant care and propagation" },
                new SubInterest { Id = Guid.Parse("b1000006-0000-0000-0000-000000000006"), Name = "Birdwatching", InterestId = animalsId, Description = "Bird identification and observation" },

                // Philosophy & Spirituality
                new SubInterest { Id = Guid.Parse("c1000001-0000-0000-0000-000000000001"), Name = "Existentialism", InterestId = philosophyId, Description = "Existential philosophy and meaning" },
                new SubInterest { Id = Guid.Parse("c1000002-0000-0000-0000-000000000002"), Name = "Political Philosophy", InterestId = philosophyId, Description = "Philosophy of the state and governance" },
                new SubInterest { Id = Guid.Parse("c1000003-0000-0000-0000-000000000003"), Name = "UFOs & Extraterrestrials", InterestId = philosophyId, Description = "UFO phenomena and alien contact" },
                new SubInterest { Id = Guid.Parse("c1000004-0000-0000-0000-000000000004"), Name = "Meditation & Mindfulness", InterestId = philosophyId, Description = "Meditation practices and mindfulness" },
                new SubInterest { Id = Guid.Parse("c1000005-0000-0000-0000-000000000005"), Name = "Qabalah & Mysticism", InterestId = philosophyId, Description = "Jewish mysticism and Hermetic texts" },
                new SubInterest { Id = Guid.Parse("c1000006-0000-0000-0000-000000000006"), Name = "Eastern Philosophy", InterestId = philosophyId, Description = "Tao, Bhagavad Gita, and Eastern thought" },
                new SubInterest { Id = Guid.Parse("c1000007-0000-0000-0000-000000000007"), Name = "Simulation Theory", InterestId = philosophyId, Description = "Reality simulation and consciousness" },

                // True Crime & Investigation
                new SubInterest { Id = Guid.Parse("d1000001-0000-0000-0000-000000000001"), Name = "Serial Killers", InterestId = trueCrimeId, Description = "Serial killer investigations and psychology" },
                new SubInterest { Id = Guid.Parse("d1000002-0000-0000-0000-000000000002"), Name = "Forensics", InterestId = trueCrimeId, Description = "Forensic science and crime scene analysis" },
                new SubInterest { Id = Guid.Parse("d1000003-0000-0000-0000-000000000003"), Name = "Cold Cases", InterestId = trueCrimeId, Description = "Unsolved mysteries and cold cases" },
                new SubInterest { Id = Guid.Parse("d1000004-0000-0000-0000-000000000004"), Name = "Local Crime History", InterestId = trueCrimeId, Description = "Regional crime history and stories" },

                // Finance & Investing
                new SubInterest { Id = Guid.Parse("e1000001-0000-0000-0000-000000000001"), Name = "Stock Market", InterestId = financeId, Description = "Stock trading and market analysis" },
                new SubInterest { Id = Guid.Parse("e1000002-0000-0000-0000-000000000002"), Name = "SPX Options", InterestId = financeId, Description = "S&P 500 index options trading" },
                new SubInterest { Id = Guid.Parse("e1000003-0000-0000-0000-000000000003"), Name = "Cryptocurrency", InterestId = financeId, Description = "Digital currencies and blockchain" },
                new SubInterest { Id = Guid.Parse("e1000004-0000-0000-0000-000000000004"), Name = "Day Trading", InterestId = financeId, Description = "Active day trading strategies" },
                new SubInterest { Id = Guid.Parse("e1000005-0000-0000-0000-000000000005"), Name = "Personal Finance", InterestId = financeId, Description = "Budgeting and financial planning" },

                // Linguistics & Languages
                new SubInterest { Id = Guid.Parse("f1000001-0000-0000-0000-000000000001"), Name = "Linguistics", InterestId = linguisticsId, Description = "Language structure and evolution" },
                new SubInterest { Id = Guid.Parse("f1000002-0000-0000-0000-000000000002"), Name = "Phonetics", InterestId = linguisticsId, Description = "Speech sounds and pronunciation" },
                new SubInterest { Id = Guid.Parse("f1000003-0000-0000-0000-000000000003"), Name = "Etymology", InterestId = linguisticsId, Description = "Word origins and language history" },
                new SubInterest { Id = Guid.Parse("f1000004-0000-0000-0000-000000000004"), Name = "Constructed Languages", InterestId = linguisticsId, Description = "Conlangs like Esperanto and Klingon" },
                new SubInterest { Id = Guid.Parse("f1000005-0000-0000-0000-000000000005"), Name = "Sign Language", InterestId = linguisticsId, Description = "ASL and sign language systems" },

                // Mythology & Folklore
                new SubInterest { Id = Guid.Parse("13000001-0000-0000-0000-000000000001"), Name = "Greek Mythology", InterestId = mythologyId, Description = "Ancient Greek gods and heroes" },
                new SubInterest { Id = Guid.Parse("13000002-0000-0000-0000-000000000002"), Name = "Norse Mythology", InterestId = mythologyId, Description = "Viking gods and Norse legends" },
                new SubInterest { Id = Guid.Parse("13000003-0000-0000-0000-000000000003"), Name = "Irish Mythology", InterestId = mythologyId, Description = "Celtic and Irish folklore" },
                new SubInterest { Id = Guid.Parse("13000005-0000-0000-0000-000000000005"), Name = "Cryptozoology", InterestId = mythologyId, Description = "Cryptids and mysterious creatures" },
                new SubInterest { Id = Guid.Parse("13000006-0000-0000-0000-000000000006"), Name = "Urban Legends", InterestId = mythologyId, Description = "Modern folklore and urban myths" },

                // Additional Music SubInterests
                new SubInterest { Id = Guid.Parse("f0000008-0000-0000-0000-000000000008"), Name = "Jazz", InterestId = musicId, Description = "Jazz music and improvisation" },
                new SubInterest { Id = Guid.Parse("f0000009-0000-0000-0000-000000000009"), Name = "Classical Music", InterestId = musicId, Description = "Classical composers and orchestral music" },
                new SubInterest { Id = Guid.Parse("f0000010-0000-0000-0000-000000000010"), Name = "Synthesizers", InterestId = musicId, Description = "Analog and digital synthesizers" },
                new SubInterest { Id = Guid.Parse("f0000011-0000-0000-0000-000000000011"), Name = "World Music", InterestId = musicId, Description = "Dangdut, folk, and regional music" },

                // Additional Fitness & Sports SubInterests
                new SubInterest { Id = Guid.Parse("d0000007-0000-0000-0000-000000000007"), Name = "Sumo Wrestling", InterestId = fitnessId, Description = "Japanese sumo traditions and tournaments" },
                new SubInterest { Id = Guid.Parse("d0000008-0000-0000-0000-000000000008"), Name = "WWE Wrestling", InterestId = fitnessId, Description = "Professional wrestling entertainment" },
                new SubInterest { Id = Guid.Parse("d0000009-0000-0000-0000-000000000009"), Name = "Speedcubing", InterestId = fitnessId, Description = "Rubik's cube speedsolving" },
                new SubInterest { Id = Guid.Parse("d0000010-0000-0000-0000-000000000010"), Name = "Kendama", InterestId = fitnessId, Description = "Japanese kendama toy tricks" },
                new SubInterest { Id = Guid.Parse("d0000011-0000-0000-0000-000000000011"), Name = "Juggling", InterestId = fitnessId, Description = "Ball and object juggling" },

                // Additional Literature SubInterests
                new SubInterest { Id = Guid.Parse("70000009-0000-0000-0000-000000000009"), Name = "Warrior Cats", InterestId = literatureId, Description = "Warrior Cats book series fandom" },
                new SubInterest { Id = Guid.Parse("70000010-0000-0000-0000-000000000010"), Name = "Light Novels", InterestId = literatureId, Description = "Japanese light novels and web novels" },

                // Additional Pop Culture SubInterests
                new SubInterest { Id = Guid.Parse("90000009-0000-0000-0000-000000000009"), Name = "Kaiju & Monsters", InterestId = popCultureId, Description = "Godzilla, Mothra, and giant monsters" },
                new SubInterest { Id = Guid.Parse("90000010-0000-0000-0000-000000000010"), Name = "Michael Jackson", InterestId = popCultureId, Description = "King of Pop music and legacy" },
                new SubInterest { Id = Guid.Parse("90000011-0000-0000-0000-000000000011"), Name = "Digimon", InterestId = popCultureId, Description = "Digimon franchise and virtual pets" },
                new SubInterest { Id = Guid.Parse("90000012-0000-0000-0000-000000000012"), Name = "Obscure Internet History", InterestId = popCultureId, Description = "Early 2000s internet and lost media" },
                new SubInterest { Id = Guid.Parse("90000013-0000-0000-0000-000000000013"), Name = "South Park", InterestId = popCultureId, Description = "South Park animated series" },

                // Additional Arts & Crafts SubInterests
                new SubInterest { Id = Guid.Parse("e0000008-0000-0000-0000-000000000008"), Name = "Plush Making", InterestId = artsId, Description = "Plushie and soft toy creation" },
                new SubInterest { Id = Guid.Parse("e0000009-0000-0000-0000-000000000009"), Name = "Vintage Toy Collecting", InterestId = artsId, Description = "Vintage squeak toys and collectibles" },
                new SubInterest { Id = Guid.Parse("e0000010-0000-0000-0000-000000000010"), Name = "Antique Cameras", InterestId = artsId, Description = "Vintage camera collecting and restoration" },

                // Additional Science SubInterests
                new SubInterest { Id = Guid.Parse("30000008-0000-0000-0000-000000000008"), Name = "Astrophysics", InterestId = scienceId, Description = "Cosmology and theoretical astrophysics" },
                new SubInterest { Id = Guid.Parse("30000009-0000-0000-0000-000000000009"), Name = "Optics & Light", InterestId = scienceId, Description = "Light reflection and optical phenomena" },
                new SubInterest { Id = Guid.Parse("30000010-0000-0000-0000-000000000010"), Name = "Educational Theory", InterestId = scienceId, Description = "Learning psychology and pedagogy" },

                // Additional Technology SubInterests
                new SubInterest { Id = Guid.Parse("20000008-0000-0000-0000-000000000008"), Name = "Linux", InterestId = techId, Description = "Linux operating systems and open source" },
                new SubInterest { Id = Guid.Parse("20000009-0000-0000-0000-000000000009"), Name = "Intellectual Property", InterestId = techId, Description = "Copyright, patents, and IP law" },
                new SubInterest { Id = Guid.Parse("20000010-0000-0000-0000-000000000010"), Name = "Animatronics", InterestId = techId, Description = "Animatronic engineering and FNAF" },

                // Additional Models & Collecting SubInterests
                new SubInterest { Id = Guid.Parse("c0000008-0000-0000-0000-000000000008"), Name = "Genealogy", InterestId = modelsId, Description = "Family history and ancestry research" },
                new SubInterest { Id = Guid.Parse("c0000009-0000-0000-0000-000000000009"), Name = "Wooden Wands", InterestId = modelsId, Description = "Wand collecting and wood characteristics" },
                new SubInterest { Id = Guid.Parse("c0000010-0000-0000-0000-000000000010"), Name = "Sword Collecting", InterestId = modelsId, Description = "Historical and decorative swords" },
                new SubInterest { Id = Guid.Parse("c0000011-0000-0000-0000-000000000011"), Name = "Firearms", InterestId = modelsId, Description = "Firearm history and mechanics" },
                new SubInterest { Id = Guid.Parse("c0000012-0000-0000-0000-000000000012"), Name = "Civil War Firearms", InterestId = modelsId, Description = "American Civil War era weapons" },
                new SubInterest { Id = Guid.Parse("c0000013-0000-0000-0000-000000000013"), Name = "Knife Collecting", InterestId = modelsId, Description = "Folding knives and tactical blades" },
                new SubInterest { Id = Guid.Parse("c0000015-0000-0000-0000-000000000015"), Name = "Knife Throwing", InterestId = modelsId, Description = "Throwing knives and axes" },

                // Additional Gaming SubInterests
                new SubInterest { Id = Guid.Parse("b0000008-0000-0000-0000-000000000008"), Name = "Pokemon", InterestId = gamingId, Description = "Pokemon games and trading cards" }
            );
        }
    }
}
