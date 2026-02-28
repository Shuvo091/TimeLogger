using System;

namespace SkillAllocationTracker.Application.DTOs
{
    public class TopicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Percentage { get; set; }
    }
}