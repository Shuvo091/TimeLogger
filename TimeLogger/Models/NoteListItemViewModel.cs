using SkillAllocationTracker.Domain.Entities;
using System;

namespace TimeLogger.Models
{
    public class NoteListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public NoteScope Scope { get; set; }
        public DateTime OccurrenceDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}