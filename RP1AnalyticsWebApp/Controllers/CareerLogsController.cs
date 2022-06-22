using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
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

        private bool IsLocalhost => string.Equals(HttpContext.Request.Host.Host, "localhost", System.StringComparison.OrdinalIgnoreCase);

        public CareerLogsController(CareerLogService careerLogService, TelemetryClient telemetry)
        {
            _careerLogService = careerLogService;
            _telemetry = telemetry;
        }

        [HttpGet(Name = "GetCareerLogs")]
        public ActionResult<List<CareerLog>> GetCareerLogs()
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLogs");
            var res = _careerLogService.Get();
            if (!IsLocalhost)
            {
                res.ForEach(c => c.RemoveNonPublicData());
            }
            return res;
        }

        [HttpGet("List", Name = "GetCareerList")]
        public ActionResult<List<CareerListItem>> GetCareerList(string userName = null)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerList");
            return _careerLogService.GetCareerList(userName);
        }

        [HttpGet("{id:length(24)}", Name = "GetCareerLog")]
        public ActionResult<CareerLog> GetCareerLog(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLog", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            var careerLog = _careerLogService.Get(id);
            if (careerLog == null)
            {
                return NotFound();
            }

            if (!IsLocalhost)
            {
                careerLog.RemoveNonPublicData();
            }

            return careerLog;
        }

        [HttpGet("{id:length(24)}/Contracts", Name = "GetCareerContracts")]
        public ActionResult<List<BaseContractEvent>> GetCareerContracts(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerContracts", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<BaseContractEvent> contractEvents = _careerLogService.GetContractsForCareer(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("{id:length(24)}/CompletedMilestones", Name = "GetCareerCompletedMilestones")]
        public ActionResult<List<BaseContractEvent>> GetCareerCompletedMilestones(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerCompletedMilestones", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<BaseContractEvent> contractEvents = _careerLogService.GetCompletedMilestonesForCareer(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("{id:length(24)}/CompletedRepeatables", Name = "GetCareerCompletedRepeatables")]
        public ActionResult<List<ContractEventWithCount>> GetCareerCompletedRepeatables(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerCompletedRepeatables", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ContractEventWithCount> contractEvents = _careerLogService.GetRepeatableContractCompletionCountsForCareer(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("Contracts", Name = "GetRecords")]
        public ActionResult<List<ContractRecord>> GetRecords()
        {
            _telemetry.TrackEvent("CareerLogsController-GetRecords");

            List<ContractRecord> events = _careerLogService.GetRecords();
            return events;
        }

        [HttpGet("Contracts/{contract}", Name = "GetCompletionsForContract")]
        public ActionResult<List<ContractEventWithCareerInfo>> GetCompletionsForContract(string contract)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCompletionsForContract", new Dictionary<string, string>
            {
                { nameof(contract), contract }
            });

            List<ContractEventWithCareerInfo> events = _careerLogService.GetEventsForContract(contract, ContractEventType.Complete);
            return events;
        }

        [HttpGet("{id:length(24)}/Tech", Name = "GetTechUnlocksForCareer")]
        public ActionResult<List<TechEvent>> GetTechUnlocksForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetTechUnlocksForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<TechEvent> events = _careerLogService.GetTechUnlocksForCareer(id);
            return events;
        }

        [HttpGet("{id:length(24)}/Launches", Name = "GetLaunchesForCareer")]
        public ActionResult<List<LaunchEvent>> GetLaunchesForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLaunchesForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LaunchEvent> events = _careerLogService.GetLaunchesForCareer(id);
            return events;
        }

        [HttpGet("{id:length(24)}/Facilities", Name = "GetFacilityConstructionsForCareer")]
        public ActionResult<List<FacilityConstruction>> GetFacilityConstructionsForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetFacilityConstructionsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<FacilityConstruction> fcs = _careerLogService.GetFacilityConstructionsForCareer(id);
            return fcs;
        }

        [HttpGet("{id:length(24)}/Programs", Name = "GetProgramsForCareer")]
        public ActionResult<List<Models.Program>> GetProgramsForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetProgramsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<Models.Program> ps = _careerLogService.GetProgramsForCareer(id);
            return ps;
        }

        [HttpPost(Name = "CreateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Member)]
        public ActionResult<CareerLog> CreateCareer(CareerLog log)
        {
            _telemetry.TrackEvent("CareerLogsController-CreateCareer", new Dictionary<string, string>
            {
                { nameof(CareerLog.Name), log.Name }
            });

            log.UserLogin = User.Identity.Name;

            CareerLog res = _careerLogService.Create(log);
            return CreatedAtRoute("CreateCareer", res);
        }

        [HttpPatch("{token:length(32)}", Name = "UpdateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CareerLogDto> UpdateCareer(string token, CareerLogDto careerLog)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateCareer", new Dictionary<string, string>
            {
                { nameof(token), token }
            });

            CareerLog res = _careerLogService.Update(token, careerLog);
            return CreatedAtRoute("UpdateCareer", res);
        }
    }
}
