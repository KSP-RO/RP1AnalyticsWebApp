using Microsoft.AspNetCore.Mvc.RazorPages;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System.Collections.Generic;

namespace RP1AnalyticsWebApp
{
    public class RecordsModel : PageModel
    {
        private readonly CareerLogService _careerLogService;

        public List<ContractRecord> Records => _careerLogService.GetRecords();

        public RecordsModel(CareerLogService careerLogService)
        {
            _careerLogService = careerLogService;
        }

        public void OnGet()
        {

        }
    }
}