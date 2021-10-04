using System;

namespace RP1AnalyticsWebApp
{
    public static class Constants
    {
        public static DateTime CareerEpoch = new DateTime(1951, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Member = "Member";
        }
    }
}
