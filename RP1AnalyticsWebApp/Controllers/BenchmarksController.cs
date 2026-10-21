using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(Constants.RateLimitPolicies.Api)]
    public class BenchmarksController : ControllerBase
    {
        private readonly HistoricalBenchmarkService _historicalBenchmarkService;

        public BenchmarksController(HistoricalBenchmarkService historicalBenchmarkService)
        {
            _historicalBenchmarkService = historicalBenchmarkService;
        }

        [HttpGet("Historical", Name = "GetHistoricalBenchmarks")]
        public ActionResult<List<HistoricalBenchmark>> GetHistoricalBenchmarks()
        {
            return _historicalBenchmarkService.GetHistoricalBenchmarks();
        }
    }
}
