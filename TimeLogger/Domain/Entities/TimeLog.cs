using System;

namespace SkillAllocationTracker.Domain.Entities
{
    public class TimeLog
    {
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
        public int DurationMinutes { get; set; } // store in minutes
        public string? Note { get; set; }
        public DateTime LogDate { get; set; } // Stored as UTC (ensure on create: DateTime.UtcNow or user-specified converted)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation (optional)
        public Topic? Topic { get; set; }
    }
}