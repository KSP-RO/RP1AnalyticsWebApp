using Microsoft.AspNetCore.Http;
using OpenTelemetry;
using System;
using System.Diagnostics;

namespace RP1AnalyticsWebApp.Services
{
    public class CustomTelemetryInitializer : BaseProcessor<Activity>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public override void OnEnd(Activity activity)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User.Identity?.IsAuthenticated ?? false)
            {
                activity.SetTag("enduser.id", httpContext.User.Identity.Name);
            }
        }
    }
}
