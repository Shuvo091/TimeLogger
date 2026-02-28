using SkillAllocationTracker.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace TimeLogger.Models
{
    public class NoteEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Title { get; set; } = null!;

        public string? Body { get; set; }

        [Required]
        public NoteScope Scope { get; set; } = NoteScope.Daily;

        [Required]
        [DataType(DataType.Date)]
        public DateTime OccurrenceDate { get; set; }
    }
}