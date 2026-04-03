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
        private readonly UserManager<WebAppUser> _userManager;

        public UsersController(UserManager<WebAppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet(Name = "GetUsers")]
        public ActionResult<List<UserData>> Get()
        {
            return _userManager.Users.Select(u => new UserData
            {
                UserName = u.UserName,
                PreferredName = u.PreferredName
            }).ToList()
              .OrderBy(u => string.IsNullOrWhiteSpace(u.PreferredName) ? u.UserName : u.PreferredName)
              .ToList();
        }
    }
}
