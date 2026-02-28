namespace TimeLogger.Models.ViewModels
{
    public class AnalyticsViewModel
    {
        public string[] Labels { get; set; } = new string[0];
        public double[] Planned { get; set; } = new double[0];
        public double[] Actual { get; set; } = new double[0];

        public string[] TrendLabels { get; set; } = new string[0];
        public double[] TrendValues { get; set; } = new double[0];
    }
}