using System;

namespace RP1AnalyticsWebApp.Models
{
    public class LeaderItem
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime DateAdd { get; set; }
        public DateTime? DateRemove { get; set; }
        public double FireCost { get; set; }
    }
}
