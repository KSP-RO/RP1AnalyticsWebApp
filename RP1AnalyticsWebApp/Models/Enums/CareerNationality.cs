using System.ComponentModel.DataAnnotations;

namespace RP1AnalyticsWebApp.Models
{
    public enum CareerNationality
    {
        Mix,
        US,
        [Display(Name = "Soviet/Russia")]
        RU,
        [Display(Name = "ESA")]
        EU,
        UK,
        [Display(Name = "Japan")]
        JP,
        [Display(Name = "China")]
        CN
    }
}
