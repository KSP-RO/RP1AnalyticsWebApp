using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RP1AnalyticsWebApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace RP1AnalyticsWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TelemetryClient _telemetry;
        private readonly UserManager<WebAppUser> _userManager;

        public UsersController(UserManager<WebAppUser> userManager, TelemetryClient telemetry)
        {
            _userManager = userManager;
            _telemetry = telemetry;
        }

        [HttpGet(Name = "GetUsers")]
        public ActionResult<List<UserData>> Get()
        {
            return _userManager.Users.Select(u => new UserData
            {
                UserName = u.UserName,
                PreferredName = u.PreferredName
            }).ToList();
        }
    }
}
