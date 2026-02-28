using System;

namespace SkillAllocationTracker.Domain.Entities
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        // Percentage 1..100
        public int Percentage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed property - not stored in DB
        public double CalculatedWeeklyHours(double totalWeeklyHours)
        {
            return Math.Round((Percentage / 100.0) * totalWeeklyHours, 2);
        }
    }
}