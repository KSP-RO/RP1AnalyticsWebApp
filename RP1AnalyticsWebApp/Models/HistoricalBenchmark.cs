using System;

namespace RP1AnalyticsWebApp.Models
{
    public class HistoricalBenchmark
    {
        public string Key { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Agency { get; set; }
        public string SourceUrl { get; set; }
        public string Note { get; set; }
    }
}
