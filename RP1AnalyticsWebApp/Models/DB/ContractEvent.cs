using System;

namespace RP1AnalyticsWebApp.Models
{
    public class ContractEvent
    {
        public string InternalName { get; set; }
        public DateTime Date { get; set; }
        public double RepChange { get; set; }
        public ContractEventType Type { get; set; }

        public ContractEvent()
        {
        }

        public ContractEvent(ContractEventDto c)
        {
            InternalName = c.InternalName;
            Date = c.Date;
            RepChange = c.RepChange;
            Type = c.Type;
        }
    }
}
