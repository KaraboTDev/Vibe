using Microsoft.EntityFrameworkCore;
using VibeApi.Models;

namespace VibeApi.Data
{
    public class VibeDbContext : DbContext
    {
        public VibeDbContext(DbContextOptions<VibeDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<VenueTag> VenueTags => Set<VenueTag>();
        public DbSet<Favorite> Favorites => Set<Favorite>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User
            builder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.PreferredLanguage).HasDefaultValue("en").HasMaxLength(10);
                e.Property(u => u.NotificationsEnabled).HasDefaultValue(true);
            });

            // Venue
            builder.Entity<Venue>(e =>
            {
                e.HasIndex(v => v.ExternalRefId);
                e.Property(v => v.ExternalRefId).HasMaxLength(200).IsRequired();
                e.Property(v => v.Name).HasMaxLength(200).IsRequired();
                e.Property(v => v.Latitude).HasPrecision(9, 6);
                e.Property(v => v.Longitude).HasPrecision(9, 6);
            });

            // Tag
            builder.Entity<Tag>(e =>
            {
                e.Property(t => t.Name).HasMaxLength(100).IsRequired();
            });

            // VenueTag
            builder.Entity<VenueTag>(e =>
            {
                e.HasIndex(vt => new { vt.VenueId, vt.TagId }).IsUnique();
                e.Property(vt => vt.VoteCount).HasDefaultValue(1);

                e.HasOne(vt => vt.Venue).WithMany(v => v.VenueTags).HasForeignKey(vt => vt.VenueId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(vt => vt.Tag).WithMany(t => t.VenueTags).HasForeignKey(vt => vt.TagId).OnDelete(DeleteBehavior.Cascade);
            });

            // Favorite
            builder.Entity<Favorite>(e =>
            {
                e.HasIndex(f => new { f.UserId, f.VenueId }).IsUnique();
                e.Property(f => f.SyncStatus).HasMaxLength(50).HasDefaultValue("synced");
                e.HasOne(f => f.User).WithMany(u => u.Favorites).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(f => f.Venue).WithMany(v => v.Favorites).HasForeignKey(f => f.VenueId).OnDelete(DeleteBehavior.Cascade);
            });

            // Seed tags
            builder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "Quiet" },
                new Tag { Id = 2, Name = "Social" },
                new Tag { Id = 3, Name = "Focus" },
                new Tag { Id = 4, Name = "Creative" },
                new Tag { Id = 5, Name = "Cheap Wifi" }
            );
        }
    }
}
