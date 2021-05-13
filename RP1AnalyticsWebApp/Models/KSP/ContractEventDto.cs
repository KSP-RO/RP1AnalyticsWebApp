using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ContractEventDto
    {
        public string InternalName { get; set; }
        public DateTime Date { get; set; }
        public double FundsChange { get; set; }
        public double RepChange { get; set; }
        public ContractEventType Type { get; set; }
    }
}
