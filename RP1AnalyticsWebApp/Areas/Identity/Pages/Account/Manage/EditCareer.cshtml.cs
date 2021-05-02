using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class EditCareerModel : PageModel
    {
        public string Username { get; set; }
        [BindProperty] public IndexModel.InputModel Input { get; set; }
        private readonly UserManager<MongoUser> _userManager;
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly CareerLogService _careerLogService;

        public CareerLog CareerLog { get; private set; }

        public EditCareerModel(UserManager<MongoUser> userManager, SignInManager<MongoUser> signInManager,
            CareerLogService careerLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _careerLogService = careerLogService;
            Input = new IndexModel.InputModel();
        }


        private async Task LoadAsync(MongoUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            CareerLog = _careerLogService.GetByToken(RouteData.Values["token"].ToString());

            if (CareerLog.careerLogMeta != null) InitFieldValues();
            else Input.CareerName = CareerLog.name;

            await LoadAsync(user);
            return Page();
        }

        public RedirectToPageResult OnPost()
        {
            _careerLogService.UpdateMetaByToken(RouteData.Values["token"].ToString(),
                CreateCareerLogMeta());

            return new RedirectToPageResult("Index");
        }

        private CareerLogMeta CreateCareerLogMeta()
        {
            return new CareerLogMeta
            {
                CareerPlaystyle = Input.CareerPlaystyle,
                DifficultyLevel = Input.DifficultyLevel,
                ConfigurableStart = Input.ConfigurableStart,
                FailureModel = Input.FailureModel,
                DescriptionText = Input.DescriptionText
            };
        }

        private void InitFieldValues()
        {
            Input.CareerName = CareerLog.name;
            Input.DifficultyLevel = CareerLog.careerLogMeta.DifficultyLevel;
            Input.FailureModel = CareerLog.careerLogMeta.FailureModel;
            Input.CareerPlaystyle = CareerLog.careerLogMeta.CareerPlaystyle;
            Input.DescriptionText = CareerLog.careerLogMeta.DescriptionText;
        }
    }
}