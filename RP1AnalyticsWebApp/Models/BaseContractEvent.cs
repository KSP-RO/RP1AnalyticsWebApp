using System;

namespace RP1AnalyticsWebApp.Models
{
    public class BaseContractEvent
    {
        public string ContractInternalName { get; set; }
        public string ContractDisplayName { get; set; }
        public ContractEventType Type { get; set; }
        public DateTime? Date { get; set; }
    }
}
