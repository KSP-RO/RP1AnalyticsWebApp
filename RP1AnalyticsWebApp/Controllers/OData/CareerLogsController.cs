using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Controllers.OData
{
    [ApiController]
    [ODataAttributeRouting]
    [Route("odata")]
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

        //[EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Filter)]
        [HttpGet("careers", Name = "ODataGetCareerLogs")]
        public async Task<ActionResult<List<CareerLog>>> ODataGetCareerLogsAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = await _careerLogService.GetAsync(queryOptions);
            if (!IsLocalhost)
            {
                res.ForEach(c => c.RemoveNonPublicData());
            }
            return res;
        }

        [HttpGet("careers({id:length(24)})", Name = "ODataGetCareerLogsById")]
        public async Task<ActionResult<CareerLog>> GetCareerLogsByIdAsync(string id)
        {
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

        [HttpGet("careers({id:length(24)})/careerLogEntries", Name = "ODataGetCareerPeriods")]
        public async Task<ActionResult<List<CareerLogPeriod>>> GetCareerPeriodsAsync(string id, ODataQueryOptions<CareerLogPeriod> queryOptions)
        {
            var periods = await _careerLogService.GetCareerPeriodsAsync(id, queryOptions);
            if (periods == null)
            {
                return NotFound();
            }

            return periods;
        }

        [HttpGet("careers({careerId:length(24)})/careerLogEntries({periodStart})", Name = "ODataGetCareerPeriod")]
        public async Task<ActionResult<CareerLogPeriod>> GetCareerPeriodAsync(string careerId, string periodStart)
        {
            var dt = DateTime.Parse(periodStart);
            var period = await _careerLogService.GetCareerPeriodAsync(careerId, dt);
            if (period == null)
            {
                return NotFound();
            }

            return period;
        }

        [HttpGet("contractRecords", Name = "ODataGetContractRecords")]
        public async Task<ActionResult<List<ContractRecord>>> GetContractRecordsAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractRecord> events = await _careerLogService.GetContractRecordsAsync(queryOptions);
            return events;
        }

        [HttpGet("programRecords", Name = "ODataGetProgramRecords")]
        public async Task<ActionResult<List<ProgramRecord>>> GetProgramRecordsAsync(ODataQueryOptions<CareerLog> queryOptions, ProgramRecordType type = ProgramRecordType.Accepted)
        {
            List<ProgramRecord> events = await _careerLogService.GetProgramRecordsAsync(type, queryOptions: queryOptions);
            return events;
        }

        [HttpGet("programs({program})", Name = "ODataGetRecordsForSpecificProgram")]
        public async Task<ActionResult<List<ProgramItemWithCareerInfo>>> GetRecordsForSpecificProgramAsync(string program, ODataQueryOptions<CareerLog> queryOptions, ProgramRecordType type = ProgramRecordType.Accepted)
        {
            List<ProgramItemWithCareerInfo> events = await _careerLogService.GetProgramRecordsAsync(type, program, queryOptions: queryOptions);
            return events;
        }

        [HttpGet("contracts", Name = "ODataGetContracts")]
        public async Task<ActionResult<List<ContractEventWithCareerInfo>>> GetContractsAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractEventWithCareerInfo> events = await _careerLogService.GetAllContractEventsAsync(queryOptions);
            return events;
        }

        [HttpGet("contracts({contract})", Name = "ODataGetContractCompletions")]
        public async Task<ActionResult<List<ContractEventWithCareerInfo>>> GetContractsAsync(string contract, ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractEventWithCareerInfo> events = await _careerLogService.GetEventsForContractAsync(contract, ContractEventType.Complete, queryOptions);
            return events;
        }

        [HttpGet("careerListItems", Name = "ODataGetCareerListItems")]
        public async Task<ActionResult<List<CareerListItem>>> GetCareerListItemsAsync(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = await _careerLogService.GetCareerListAsync(queryOptions);
            return res;
        }
    }
}
