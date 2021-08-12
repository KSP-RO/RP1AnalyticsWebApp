using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System.Collections.Generic;
using System.Linq;

namespace rp1_analytics_server
{
    public class IndexModel : PageModel
    {
        private readonly CareerLogService _careerLogService;
        private readonly UserManager<WebAppUser> _userManager;

        public string Career { get; set; }

        public List<SelectListItem> CareersGroupedByUser { get; private set; }

        public IndexModel(CareerLogService careerLogService, UserManager<WebAppUser> userManager)
        {
            _careerLogService = careerLogService;
            _userManager = userManager;
        }

        public void OnGet()
        {
            List<WebAppUser> allUsers = _userManager.Users.ToList();
            List<CareerListItem> allCareers = _careerLogService.GetCareerList();
            CareersGroupedByUser = allCareers.GroupBy(c => c.User)
                                             .SelectMany(g => {
                                                 var group = new SelectListGroup
                                                 {
                                                     Name = allUsers.FirstOrDefault(u => u.UserName == g.Key)?.PreferredName ?? g.Key
                                                 };
                                                 return g.Select(c => new SelectListItem
                                                 {
                                                     Value = c.Id,
                                                     Text = c.Name,
                                                     Group = group
                                                 });
                                             })
                                             .ToList();
        }
    }
}