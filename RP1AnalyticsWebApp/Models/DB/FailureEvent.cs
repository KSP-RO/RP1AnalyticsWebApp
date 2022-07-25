using System;

namespace RP1AnalyticsWebApp.Models
{
    public class FailureEvent
    {
        public DateTime Date { get; set; }
        public string Part { get; set; }
        public string Type { get; set; }

        public FailureEvent()
        {
        }

        public FailureEvent(FailureEventDto f)
        {
            Date = f.Date;
            Part = f.Part;
            Type = f.Type;
        }
    }
}
