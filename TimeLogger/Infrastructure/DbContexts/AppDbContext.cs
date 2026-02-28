using Microsoft.EntityFrameworkCore;
using SkillAllocationTracker.Domain.Entities;

namespace SkillAllocationTracker.Infrastructure.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Topic> Topics { get; set; } = null!;
        public DbSet<WeeklyConfig> WeeklyConfigs { get; set; } = null!;
        public DbSet<TimeLog> TimeLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Topic>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).HasMaxLength(200).IsRequired();
                b.HasIndex(t => t.Name).IsUnique();
                b.Property(t => t.Percentage).IsRequired();
            });

            modelBuilder.Entity<WeeklyConfig>(b =>
            {
                b.HasKey(w => w.Id);
            });

            modelBuilder.Entity<TimeLog>(b =>
            {
                b.HasKey(l => l.Id);
                b.Property(l => l.DurationMinutes).IsRequired();
                b.Property(l => l.LogDate).IsRequired();
                b.HasOne(l => l.Topic).WithMany().HasForeignKey(l => l.TopicId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}