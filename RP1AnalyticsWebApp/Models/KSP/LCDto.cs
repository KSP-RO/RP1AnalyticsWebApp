using System;

namespace RP1AnalyticsWebApp.Models
{
    public class LCDto
    {
        public Guid Id { get; set; }
        public Guid ModId { get; set; }
        public double ModCost { get; set; }
        public string Name { get; set; }
        public LaunchComplexType LcType { get; set; }
        public float MassMax { get; set; }
        public float MassOrig { get; set; }
        public Vector3 SizeMax { get; set; }
        public bool IsHumanRated { get; set; }
    }
}
