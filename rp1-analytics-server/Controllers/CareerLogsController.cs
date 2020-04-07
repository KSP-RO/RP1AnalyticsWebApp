using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rp1_analytics_server.Models;
using rp1_analytics_server.Services;

namespace rp1_analytics_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CareerLogsController : ControllerBase
    {
        private readonly CareerLogService _careerLogService;

        public CareerLogsController(CareerLogService careerLogService)
        {
            _careerLogService = careerLogService;
        }

        [HttpGet(Name = "GetCareerLogs")]
        public ActionResult<List<CareerLog>> Get() =>
            _careerLogService.Get();

        [HttpGet("{id:length(24)}", Name = "GetCareerLog")]
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
        public ActionResult<CareerLog> CreateMany(List<CareerLog> careerLogs)
        {
            _careerLogService.CreateMany(careerLogs);
            return CreatedAtRoute("GetCareerLogs", careerLogs);
        }
    }
}