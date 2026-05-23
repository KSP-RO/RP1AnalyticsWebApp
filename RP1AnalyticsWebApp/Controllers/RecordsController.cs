using Microsoft.AspNetCore.Mvc;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
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
            [FromQuery] CareerComparisonFilter filter,
            ProgramRecordType programType = ProgramRecordType.Completed)
        {
            return await _careerComparisonService.GetRecordsSnapshotAsync(filter, programType);
        }
    }
}
