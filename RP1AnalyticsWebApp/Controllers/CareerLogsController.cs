using System.Collections.Generic;
using System.Threading.Tasks;
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
        public async Task<ActionResult<List<CareerLog>>> GetCareerLogsAsync()
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLogs");
            var res = await _careerLogService.GetAsync();
            if (!IsLocalhost)
            {
                res.ForEach(c => c.RemoveNonPublicData());
            }
            return res;
        }

        [HttpGet("List", Name = "GetCareerList")]
        public async Task<ActionResult<List<CareerListItem>>> GetCareerListAsync(string userName = null)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerList");
            return await _careerLogService.GetCareerListAsync(userName);
        }

        [HttpGet("{id:length(24)}", Name = "GetCareerLog")]
        public async Task<ActionResult<CareerLog>> GetCareerLogAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerLog", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            var careerLog = await _careerLogService.GetAsync(id);
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
        public async Task<ActionResult<List<BaseContractEvent>>> GetCareerContractsAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerContracts", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<BaseContractEvent> contractEvents = await _careerLogService.GetContractsForCareerAsync(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("{id:length(24)}/CompletedMilestones", Name = "GetCareerCompletedMilestones")]
        public async Task<ActionResult<List<BaseContractEvent>>> GetCareerCompletedMilestonesAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerCompletedMilestones", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<BaseContractEvent> contractEvents = await _careerLogService.GetCompletedMilestonesForCareerAsync(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("{id:length(24)}/CompletedRepeatables", Name = "GetCareerCompletedRepeatables")]
        public async Task<ActionResult<List<ContractEventWithCount>>> GetCareerCompletedRepeatablesAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCareerCompletedRepeatables", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ContractEventWithCount> contractEvents = await _careerLogService.GetRepeatableContractCompletionCountsForCareerAsync(id);
            if (contractEvents == null)
            {
                return NotFound();
            }

            return contractEvents;
        }

        [HttpGet("Contracts", Name = "GetContractRecords")]
        public async Task<ActionResult<List<ContractRecord>>> GetContractRecordsAsync()
        {
            _telemetry.TrackEvent("CareerLogsController-GetContractRecords");

            List<ContractRecord> events = await _careerLogService.GetContractRecordsAsync();
            return events;
        }

        [HttpGet("Contracts/{contract}", Name = "GetCompletionsForContract")]
        public async Task<ActionResult<List<ContractEventWithCareerInfo>>> GetCompletionsForContractAsync(string contract)
        {
            _telemetry.TrackEvent("CareerLogsController-GetCompletionsForContract", new Dictionary<string, string>
            {
                { nameof(contract), contract }
            });

            List<ContractEventWithCareerInfo> events = await _careerLogService.GetEventsForContractAsync(contract, ContractEventType.Complete);
            return events;
        }

        [HttpGet("{id:length(24)}/Tech", Name = "GetTechUnlocksForCareer")]
        public async Task<ActionResult<List<TechEvent>>> GetTechUnlocksForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetTechUnlocksForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<TechEvent> events = await _careerLogService.GetTechUnlocksForCareerAsync(id);
            return events;
        }

        [HttpGet("{id:length(24)}/Launches", Name = "GetLaunchesForCareer")]
        public async Task<ActionResult<List<LaunchEvent>>> GetLaunchesForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLaunchesForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LaunchEvent> events = await _careerLogService.GetLaunchesForCareerAsync(id);
            return events;
        }

        [HttpGet("{id:length(24)}/Facilities", Name = "GetFacilityConstructionsForCareer")]
        public async Task<ActionResult<List<FacilityConstruction>>> GetFacilityConstructionsForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetFacilityConstructionsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<FacilityConstruction> fcs = await _careerLogService.GetFacilityConstructionsForCareerAsync(id);
            return fcs;
        }

        [HttpGet("{id:length(24)}/LCs", Name = "GetLCsForCareer")]
        public async Task<ActionResult<List<LC>>> GetLCsForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLCsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LC> lcs = await _careerLogService.GetLCsForCareerAsync(id);
            return lcs;
        }

        [HttpGet("{id:length(24)}/Programs", Name = "GetProgramsForCareer")]
        public async Task<ActionResult<List<ProgramItem>>> GetProgramsForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetProgramsForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<ProgramItem> ps = await _careerLogService.GetProgramsForCareerAsync(id);
            return ps;
        }

        [HttpGet("{id:length(24)}/Leaders", Name = "GetLeadersForCareer")]
        public async Task<ActionResult<List<LeaderItem>>> GetLeadersForCareerAsync(string id)
        {
            _telemetry.TrackEvent("CareerLogsController-GetLeadersForCareer", new Dictionary<string, string>
            {
                { nameof(id), id }
            });

            List<LeaderItem> ls = await _careerLogService.GetLeadersForCareerAsync(id);
            return ls;
        }

        [HttpGet("Races", Name = "GetRaces")]
        public async Task<ActionResult<List<string>>> GetRacesAsync()
        {
            _telemetry.TrackEvent("CareerLogsController-GetRaces");
            List<string> res = await _careerLogService.GetRacesAsync();
            return res;
        }

        [HttpPost(Name = "CreateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Member)]
        public async Task<ActionResult<CareerLog>> CreateCareerAsync(CareerLog log)
        {
            _telemetry.TrackEvent("CareerLogsController-CreateCareer", new Dictionary<string, string>
            {
                { nameof(CareerLog.Name), log.Name }
            });

            log.UserLogin = User.Identity.Name;

            CareerLog res = await _careerLogService.CreateAsync(log);
            return CreatedAtRoute("CreateCareer", res);
        }

        [HttpPatch("{token:length(32)}", Name = "UpdateCareer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CareerLogDto>> UpdateCareerAsync(string token, CareerLogDto careerLog)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateCareer", new Dictionary<string, string>
            {
                { nameof(token), token }
            });

            CareerLog res = await _careerLogService.UpdateAsync(token, careerLog);
            if (res == null) return NotFound();

            return CreatedAtRoute("UpdateCareer", res);
        }

        [HttpPatch("{careerId:length(24)}/Race", Name = "UpdateRace")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<ActionResult<CareerLog>> UpdateRaceAsync(string careerId, [FromBody] string race)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateRace", new Dictionary<string, string>
            {
                { nameof(careerId), careerId },
                { nameof(race), race }
            });

            CareerLog c = await _careerLogService.UpdateRaceAsync(careerId, race);
            if (c == null) return NotFound();

            return CreatedAtRoute("UpdateRace", c);
        }

        [HttpPatch("{careerId:length(24)}/Launches/{launchId:length(32)}", Name = "UpdateLaunch")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(Roles = Constants.Roles.Member)]
        public async Task<ActionResult<LaunchEvent>> UpdateLaunchAsync(string careerId, string launchId, LaunchMeta meta)
        {
            _telemetry.TrackEvent("CareerLogsController-UpdateLaunch", new Dictionary<string, string>
            {
                { nameof(careerId), careerId },
                { nameof(launchId), launchId }
            });

            // TODO: fetching the entire career log isn't particularly optimal
            CareerLog c = await _careerLogService.GetAsync(careerId);
            if (c == null) return NotFound();
            if (c.UserLogin != User.Identity.Name) return Unauthorized();

            LaunchEvent l = c.LaunchEventEntries.Find(l => l.LaunchID == launchId);
            if (l == null) return NotFound();
            l.Metadata = meta;

            await _careerLogService.UpdateLaunchAsync(careerId, l);

            return CreatedAtRoute("UpdateLaunch", l);
        }
    }
}
