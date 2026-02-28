using System;

namespace SkillAllocationTracker.Domain.Entities
{
    public class WeeklyConfig
    {
        public Guid Id { get; set; }
        public int TotalWeeklyHours { get; set; } // e.g., 24
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}