using System;

namespace SkillAllocationTracker.Domain.Entities
{
    public enum NoteScope
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }

    public class Note
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public NoteScope Scope { get; set; } = NoteScope.Daily;

        // The date the note applies to (e.g., for Daily = specific day, Weekly = week start date, Monthly = month start)
        public DateTime OccurrenceDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}