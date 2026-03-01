using System;

namespace SkillAllocationTracker.Domain.Entities
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Percentage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Readiness { get; set; } = string.Empty;
        public double TotalTargetHoursAllTime { get; set; } = 0.0;
        public double CalculatedWeeklyHours(double totalWeeklyHours)
        {
            return Math.Round((Percentage / 100.0) * totalWeeklyHours, 2);
        }
    }
}