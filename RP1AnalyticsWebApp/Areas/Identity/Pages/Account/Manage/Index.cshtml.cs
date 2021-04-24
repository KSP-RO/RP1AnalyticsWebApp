using System;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<MongoUser> _userManager;
        private readonly SignInManager<MongoUser> _signInManager;
        private readonly CareerLogService _careerLogService;

        public List<CareerListItem> Careers => _careerLogService.GetCareerListWithTokens(User.Identity.Name);

        public IndexModel(UserManager<MongoUser> userManager, SignInManager<MongoUser> signInManager,
            CareerLogService careerLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _careerLogService = careerLogService;
        }

        public string Username { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Career name")]
            public string CareerName { get; set; }


            [Display(Name = "Playstyle")] public CareerPlaystyle CareerPlaystyle { get; set; }

            [Display(Name = "Difficulty")] public DifficultyLevel DifficultyLevel { get; set; }

            [Display(Name = "Failure Mod")] public FailureModel FailureModel { get; set; }

            [Display(Name = "Configurable Start (leave at 'NONE' if unused)")]
            public ConfigurableStart ConfigurableStart { get; set; }

            [Display(Name = "Description")] public string DescriptionText { get; set; }
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

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _careerLogService.Create(new CareerLog
            {
                name = Input.CareerName,
                userLogin = Username,
                careerLogMeta = CreateCareerLogMeta()
            });

            StatusMessage = "A new career was created successfully";
            return RedirectToPage();
        }

        private CareerLogMeta CreateCareerLogMeta()
        {
            return new CareerLogMeta
            {
                CareerPlaystyle = Input.CareerPlaystyle,
                DifficultyLevel = Input.DifficultyLevel,
                DescriptionText = Input.DescriptionText,
                ConfigurableStart = Input.ConfigurableStart
            };
        }
    }
}