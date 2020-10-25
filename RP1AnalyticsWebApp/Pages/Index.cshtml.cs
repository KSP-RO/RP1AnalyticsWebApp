using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RP1AnalyticsWebApp.Services;

namespace rp1_analytics_server
{
    public class IndexModel : PageModel
    {
        private readonly CareerLogService _careerLogService;

        public string Career { get; set; }

        public List<SelectListItem> Careers =>
            _careerLogService.GetCareerList().Select(c => new SelectListItem { Value = c.Id, Text = c.name }).ToList();

        public IndexModel(CareerLogService careerLogService)
        {
            _careerLogService = careerLogService;
        }

        public void OnGet()
        {

        }
    }
}