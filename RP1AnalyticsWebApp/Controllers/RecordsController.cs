using Microsoft.AspNetCore.Mvc;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordsController : ControllerBase
    {
        private readonly CareerComparisonService _careerComparisonService;

        public RecordsController(CareerComparisonService careerComparisonService)
        {
            _careerComparisonService = careerComparisonService;
        }

        [HttpGet(Name = "GetRecordsSnapshot")]
        public async Task<ActionResult<RecordsSnapshot>> GetRecordsSnapshotAsync(
            [FromQuery] List<string> players = null, [FromQuery] List<string> races = null,
            ComparisonEndDateMode careerDateMode = ComparisonEndDateMode.All,
            DateTime? careerDateStart = null, DateTime? careerDateEnd = null,
            ComparisonEndDateMode lastUpdateMode = ComparisonEndDateMode.All,
            DateTime? lastUpdateStart = null, DateTime? lastUpdateEnd = null,
            [FromQuery] List<string> rp1Versions = null,
            [FromQuery] List<string> difficulties = null, [FromQuery] List<string> playstyles = null,
            string recordEligibility = "All",
            ProgramRecordType programType = ProgramRecordType.Completed)
        {
            return await _careerComparisonService.GetRecordsSnapshotAsync(players, races, careerDateMode, careerDateStart,
                careerDateEnd, lastUpdateMode, lastUpdateStart, lastUpdateEnd, rp1Versions, difficulties, playstyles,
                recordEligibility, programType);
        }
    }
}
