using System;

namespace TimeLogger.Models.ViewModels
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int Percentage { get; set; }
        public double CalculatedWeeklyHours { get; set; }
    }
}