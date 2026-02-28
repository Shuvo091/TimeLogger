using System;

namespace TimeLogger.Models.ViewModels
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Percentage { get; set; }
        public double CalculatedWeeklyHours { get; set; }

        // New metrics
        public double TotalHoursThisWeek { get; set; }
        public double TotalHoursAllTime { get; set; }
        public double PlannedWeeklyHours { get; set; }
        public double DifferenceHours { get; set; } // actual this week - planned
        public double EfficiencyPercent { get; set; } // (actual/planned)*100
    }
}