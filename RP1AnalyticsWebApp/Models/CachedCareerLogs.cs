using System.Collections.Generic;
using System.ComponentModel;

namespace RP1AnalyticsWebApp.Models
{
    /// <summary>
    /// Wrapper for cache entry.
    /// Needs to be a 🦭ed class and have ImmutableObject attribute to prevent serialization and deserialization.
    /// </summary>
    [ImmutableObject(true)]
    public sealed class CachedCareerLogs
    {
        public List<CareerLog> Items { get; set; }
    }
}
