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

        [HttpGet("Contracts", Name = "GetContractRecords")]
        public ActionResult<List<ContractRecord>> GetContractRecords()
        {
            _telemetry.TrackEvent("CareerLogsController-GetContractRecords");

            List<ContractRecord> events = _careerLogService.GetContractRecords();
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

        [HttpGet("{id:length(24)}/LCs", Name = "GetLCsForCareer")]
        public ActionResult<List<LC>> GetLCsForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLCsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LC> lcs = _careerLogService.GetLCsForCareer(id);
            return lcs;
        }

        [HttpGet("{id:length(24)}/Programs", Name = "GetProgramsForCareer")]
        public ActionResult<List<ProgramItem>> GetProgramsForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetProgramsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ProgramItem> ps = _careerLogService.GetProgramsForCareer(id);
            return ps;
        }

        [HttpGet("{id:length(24)}/Leaders", Name = "GetLeadersForCareer")]
        public ActionResult<List<LeaderItem>> GetLeadersForCareer(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLeadersForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LeaderItem> ls = _careerLogService.GetLeadersForCareer(id);
            return ls;
        }

        [HttpGet("Races", Name = "GetRaces")]
        public ActionResult<List<string>> GetRaces()
        {
            _telemetry.TrackEvent("CareerLogsController-GetRaces");
            List<string> res = _careerLogService.GetRaces();
            return res;
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
            if (res == null) return NotFound();

            return CreatedAtRoute("UpdateCareer", res);
        }

        [HttpPatch("{careerId:length(24)}/Race", Name = "UpdateRace")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Admin)]
        public ActionResult<CareerLog> UpdateRace(string careerId, [FromBody] string race)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateRace", new Dictionary<string, string>
            {
                { nameof(careerId), careerId },
                { nameof(race), race }
            });

            CareerLog c = _careerLogService.UpdateRace(careerId, race);
            if (c == null) return NotFound();

            return CreatedAtRoute("UpdateRace", c);
        }

        [HttpPatch("{careerId:length(24)}/Launches/{launchId:length(32)}", Name = "UpdateLaunch")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Member)]
        public ActionResult<LaunchEvent> UpdateLaunch(string careerId, string launchId, LaunchMeta meta)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateLaunch", new Dictionary<string, string>
            {
                { nameof(careerId), careerId },
                { nameof(launchId), launchId }
            });

            // TODO: fetching the entire career log isn't particularly optimal
            CareerLog c = _careerLogService.Get(careerId);
            if (c == null) return NotFound();
            if (c.UserLogin != User.Identity.Name) return Unauthorized();

            LaunchEvent l = c.LaunchEventEntries.Find(l => l.LaunchID == launchId);
            if (l == null) return NotFound();
            l.Metadata = meta;

            _careerLogService.UpdateLaunch(careerId, l);

            return CreatedAtRoute("UpdateLaunch", l);
        }
    }
}
