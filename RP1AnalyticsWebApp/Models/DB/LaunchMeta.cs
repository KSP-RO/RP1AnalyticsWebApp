using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RP1AnalyticsWebApp.Models
{
    /// <summary>
    /// Additional metadata about launches that can't be sent through KSP.
    /// </summary>
    public class LaunchMeta
    {
        public bool? Success { get; set; }
    }
}
