using Microsoft.Extensions.DependencyInjection;
using SkillAllocationTracker.Domain.Entities;
using SkillAllocationTracker.Infrastructure.DbContexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkillAllocationTracker.API
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!db.WeeklyConfigs.Any())
            {
                db.WeeklyConfigs.Add(new WeeklyConfig { Id = Guid.NewGuid(), TotalWeeklyHours = 24, UpdatedAt = DateTime.UtcNow });
            }

            if (!db.Topics.Any())
            {
                db.Topics.AddRange(
                    new Topic { Id = Guid.NewGuid(), Name = "Data Structures & Algorithms", Percentage = 25 },
                    new Topic { Id = Guid.NewGuid(), Name = "System Design", Percentage = 25 },
                    new Topic { Id = Guid.NewGuid(), Name = "Production Engineering", Percentage = 30 },
                    new Topic { Id = Guid.NewGuid(), Name = "Project & Portfolio", Percentage = 10 },
                    new Topic { Id = Guid.NewGuid(), Name = "Public Presence", Percentage = 5 },
                    new Topic { Id = Guid.NewGuid(), Name = "Communication", Percentage = 5 }
                );
            }

            await db.SaveChangesAsync();
        }
    }
}