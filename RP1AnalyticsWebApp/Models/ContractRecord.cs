using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ContractRecord
    {
        public string ContractInternalName { get; set; }
        public string ContractDisplayName { get; set; }
        public DateTime? Date { get; set; }
        public string CareerId { get; set; }
        public string CareerName { get; set; }
        public string UserLogin { get; set; }
        public string UserPreferredName { get; set; }
    }
}
