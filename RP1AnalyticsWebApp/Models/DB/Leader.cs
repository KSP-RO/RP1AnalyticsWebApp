using System;

namespace RP1AnalyticsWebApp.Models
{
    public class Leader
    {
        public string Name { get; set; }
        public DateTime DateAdd { get; set; }
        public DateTime? DateRemove { get; set; }
        public double FireCost { get; set; }

        public Leader()
        {
        }

        public Leader(LeaderEventDto l)
        {
            Name = l.LeaderName;
            DateAdd = l.Date;
        }
    }
}
