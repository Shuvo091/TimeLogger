using System;
using System.ComponentModel.DataAnnotations;

namespace TimeLogger.Models.ViewModels
{
    public class CreateEditTopicModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(1, 100)]
        public int Percentage { get; set; }

        public IEnumerable<TimeLogViewModel>? TimeLogs { get; set; }

        [DataType(DataType.MultilineText)]
        public string Readiness { get; set; } = string.Empty;

        [Display(Name = "Total target hours (all time)")]
        [Range(0, double.MaxValue)]
        public double TotalTargetHoursAllTime { get; set; } = 0.0;
    }
}