using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CareerLogsController : ControllerBase
    {
        private readonly CareerLogService _careerLogService;
        private readonly TelemetryClient _telemetry;

        public CareerLogsController(CareerLogService careerLogService, TelemetryClient telemetry)
        {
            _careerLogService = careerLogService;
            _telemetry = telemetry;
        }

        [HttpGet(Name = "GetCareerLogs")]
        public ActionResult<List<CareerLog>> Get() =>
            _careerLogService.Get();

        [HttpGet("IDs", Name = "GetCareerIDs")]
        public ActionResult<List<string>> GetCareerIDs() =>
            _careerLogService.GetCareerIDs();

        [HttpGet("{id:length(40)}", Name = "GetCareerLog")]
        public ActionResult<CareerLog> Get(string id)
        {
            var careerLog = _careerLogService.Get(id);

            if (careerLog == null)
            {
                return NotFound();
            }

            return careerLog;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CareerLogDto> CreateMany(List<CareerLogDto> careerLogs)
        {
            _telemetry.TrackEvent("CareerLogsController-CreateMany");
            _careerLogService.Create(careerLogs);
            return CreatedAtRoute("GetCareerLogs", careerLogs);
        }
    }
}