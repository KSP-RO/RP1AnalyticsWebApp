using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = Constants.Roles.Member)]
    public partial class EditCareerModel : PageModel
    {
        public string Username { get; set; }
        [BindProperty] public IndexModel.CareerInputModel Input { get; set; }
        private readonly UserManager<WebAppUser> _userManager;
        private readonly SignInManager<WebAppUser> _signInManager;
        private readonly CareerLogService _careerLogService;

        public CareerLog CareerLog { get; private set; }

        public EditCareerModel(UserManager<WebAppUser> userManager, SignInManager<WebAppUser> signInManager,
            CareerLogService careerLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _careerLogService = careerLogService;
            Input = new IndexModel.CareerInputModel();
        }

        private async Task LoadAsync(WebAppUser user)
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

            if (CareerLog.CareerLogMeta != null) InitFieldValues();
            else Input.CareerName = CareerLog.Name;

            await LoadAsync(user);
            return Page();
        }

        public RedirectToPageResult OnPost()
        {
            _careerLogService.UpdateMetaByToken(RouteData.Values["token"].ToString(),
                Input.CareerName,
                CreateCareerLogMeta());

            return new RedirectToPageResult("Index");
        }

        public CareerLogMeta CreateCareerLogMeta()
        {
            return new CareerLogMeta
            {
                CareerPlaystyle = Input.CareerPlaystyle,
                DifficultyLevel = Input.DifficultyLevel,
                ConfigurableStart = Input.ConfigurableStart,
                FailureModel = Input.FailureModel,
                DescriptionText = Input.DescriptionText,
                ModRecency = Input.ModRecency,
                VersionTag = Input.ModVersion?.ToString(),
                VersionSort = CreateSortableVersion(Input.ModVersion),
                CreationDate = Input.CreationDate.HasValue ? DateTime.SpecifyKind(Input.CreationDate.Value, DateTimeKind.Utc) : (DateTime?)null
            };
        }

        private void InitFieldValues()
        {
            Input.CareerName = CareerLog.Name;
            Input.DifficultyLevel = CareerLog.CareerLogMeta.DifficultyLevel;
            Input.FailureModel = CareerLog.CareerLogMeta.FailureModel;
            Input.CareerPlaystyle = CareerLog.CareerLogMeta.CareerPlaystyle;
            Input.DescriptionText = CareerLog.CareerLogMeta.DescriptionText;
            Input.ModRecency = CareerLog.CareerLogMeta.ModRecency;
            Input.ModVersion = string.IsNullOrWhiteSpace(CareerLog.CareerLogMeta.VersionTag) ? null : new Version(CareerLog.CareerLogMeta.VersionTag);
            Input.CreationDate = CareerLog.CareerLogMeta.CreationDate;
        }

        private static int? CreateSortableVersion(Version version)
        {
            if (version == null) return null;

            // 3 digits per component. Thus version 1.22.333 becomes 001022333
            return version.Major * 1000000 + version.Minor * 1000 + version.Build;
        }
    }
}