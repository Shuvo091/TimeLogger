using System;

namespace SkillAllocationTracker.Application.DTOs
{
    public class TopicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Percentage { get; set; }
        public string Readiness { get; set; } = string.Empty;
        public double TotalTargetHoursAllTime { get; set; } = 0.0;
    }
}