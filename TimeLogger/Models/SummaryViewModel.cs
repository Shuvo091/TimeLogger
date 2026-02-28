namespace TimeLogger.Models.ViewModels
{
    public class SummaryViewModel
    {
        public int TotalPlannedHours { get; set; }
        public double TotalLoggedHours { get; set; }
        public double CompletionPercentage { get; set; }
        public string MostFocusedTopic { get; set; } = "—";
    }
}