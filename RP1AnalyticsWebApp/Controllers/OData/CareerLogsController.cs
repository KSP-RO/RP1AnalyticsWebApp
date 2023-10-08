using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp.Controllers.OData
{
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
        public ActionResult<List<CareerLog>> ODataGetCareerLogs(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = _careerLogService.Get(queryOptions);
            if (!IsLocalhost)
            {
                res.ForEach(c => c.RemoveNonPublicData());
            }
            return res;
        }

        [HttpGet("careers({id:length(24)})", Name = "ODataGetCareerLogsById")]
        public ActionResult<CareerLog> GetCareerLogsById(string id)
        {
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

        [HttpGet("careers({id:length(24)})/careerLogEntries", Name = "ODataGetCareerPeriods")]
        public ActionResult<List<CareerLogPeriod>> GetCareerPeriods(string id, ODataQueryOptions<CareerLogPeriod> queryOptions)
        {
            var periods = _careerLogService.GetCareerPeriods(id, queryOptions);
            if (periods == null)
            {
                return NotFound();
            }

            return periods;
        }

        [HttpGet("careers({careerId:length(24)})/careerLogEntries({periodStart})", Name = "ODataGetCareerPeriod")]
        public ActionResult<CareerLogPeriod> GetCareerPeriod(string careerId, string periodStart)
        {
            var dt = DateTime.Parse(periodStart);
            var period = _careerLogService.GetCareerPeriod(careerId, dt);
            if (period == null)
            {
                return NotFound();
            }

            return period;
        }

        [HttpGet("contractRecords", Name = "ODataGetContractRecords")]
        public ActionResult<List<ContractRecord>> GetContractRecords(ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractRecord> events = _careerLogService.GetContractRecords(queryOptions);
            return events;
        }

        [HttpGet("programRecords", Name = "ODataGetProgramRecords")]
        public ActionResult<List<ProgramRecord>> GetProgramRecords(ODataQueryOptions<CareerLog> queryOptions, ProgramRecordType type = ProgramRecordType.Accepted)
        {
            List<ProgramRecord> events = _careerLogService.GetProgramRecords(type, queryOptions: queryOptions);
            return events;
        }

        [HttpGet("programs({program})", Name = "ODataGetRecordsForSpecificProgram")]
        public ActionResult<List<ProgramItemWithCareerInfo>> GetRecordsForSpecificProgram(string program, ODataQueryOptions<CareerLog> queryOptions, ProgramRecordType type = ProgramRecordType.Accepted)
        {
            List<ProgramItemWithCareerInfo> events = _careerLogService.GetProgramRecords(type, program, queryOptions: queryOptions);
            return events;
        }

        [HttpGet("contracts", Name = "ODataGetContracts")]
        public ActionResult<List<ContractEventWithCareerInfo>> GetContracts(ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractEventWithCareerInfo> events = _careerLogService.GetAllContractEvents(queryOptions);
            return events;
        }

        [HttpGet("contracts({contract})", Name = "ODataGetContractCompletions")]
        public ActionResult<List<ContractEventWithCareerInfo>> GetContracts(string contract, ODataQueryOptions<CareerLog> queryOptions)
        {
            List<ContractEventWithCareerInfo> events = _careerLogService.GetEventsForContract(contract, ContractEventType.Complete, queryOptions);
            return events;
        }

        [HttpGet("careerListItems", Name = "ODataGetCareerListItems")]
        public ActionResult<List<CareerListItem>> GetCareerListItems(ODataQueryOptions<CareerLog> queryOptions)
        {
            var res = _careerLogService.GetCareerList(queryOptions);
            return res;
        }
    }
}
