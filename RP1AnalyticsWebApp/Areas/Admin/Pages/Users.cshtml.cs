using AspNetCore.Identity.Mongo.Model;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RP1AnalyticsWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Areas.Admin.Pages
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<WebAppUser> _userManager;
        private readonly RoleManager<MongoRole> _roleManager;
        private readonly SignInManager<WebAppUser> _signInManager;
        private readonly TelemetryClient _telemetry;

        public List<MongoRole> Roles => _roleManager.Roles.ToList();
        public List<WebAppUser> Users => _userManager.Users.ToList();

        public UsersModel(UserManager<WebAppUser> userManager, RoleManager<MongoRole> roleManager,
                          SignInManager<WebAppUser> signInManager, TelemetryClient telemetry)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _telemetry = telemetry;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string user, string role)
        {
            _telemetry.TrackEvent("AddRoleToUser", new Dictionary<string, string>
            {
                { nameof(user), user },
                { nameof(role), role }
            });
            await ProcessRoleChange(user, role, _userManager.AddToRoleAsync);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string user, string role)
        {
            _telemetry.TrackEvent("RemoveRoleFromUser", new Dictionary<string, string>
            {
                { nameof(user), user },
                { nameof(role), role }
            });
            await ProcessRoleChange(user, role, _userManager.RemoveFromRoleAsync);
            return RedirectToPage();
        }

        private async Task ProcessRoleChange(string userName, string roleName, Func<WebAppUser, string, Task> roleOp)
        {
            var u = _userManager.Users.First(u => u.UserName == userName);
            await roleOp(u, roleName);
            if (userName == User.Identity.Name)
                await _signInManager.RefreshSignInAsync(u);
        }
    }
}
