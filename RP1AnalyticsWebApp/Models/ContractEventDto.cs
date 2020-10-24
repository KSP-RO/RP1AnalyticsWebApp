using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ContractEventDto
    {
        public string internalName { get; set; }
        public DateTime date { get; set; }
        public double fundsChange { get; set; }
        public double repChange { get; set; }
        public ContractEventType type { get; set; }
    }

    public enum ContractEventType
    {
        Accept, Complete, Fail, Cancel
    }
}
