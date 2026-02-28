using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimeLogger.Models.ViewModels
{
    public class TimeLogViewModel
    {
        [Required]
        public Guid TopicId { get; set; }

        [Required]
        [Range(1, 24*60)]
        public int DurationMinutes { get; set; }

        public string? Note { get; set; }

        [Required]
        public DateTime LogDate { get; set; } = DateTime.UtcNow;

        // simple topic list for select
        public IEnumerable<object>? Topics { get; set; }
    }
}