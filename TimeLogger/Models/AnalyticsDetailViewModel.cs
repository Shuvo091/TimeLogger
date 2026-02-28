using System.Collections.Generic;

namespace TimeLogger.Models.ViewModels
{
    public class UnderOverItem
    {
        public string Name { get; set; } = string.Empty;
        public double Planned { get; set; }
        public double Actual { get; set; }
        public double Efficiency { get; set; }
    }

    public class AnalyticsDetailedViewModel
    {
        public string[] TopicNames { get; set; } = new string[0];
        public double[] Planned { get; set; } = new double[0];
        public double[] ActualThisWeek { get; set; } = new double[0];

        public List<UnderOverItem> UnderPerforming { get; set; } = new();
        public List<UnderOverItem> OverPerforming { get; set; } = new();

        public string[] TrendLabels { get; set; } = new string[0];
        public List<double[]> TrendValuesPerTopic { get; set; } = new();
    }
}