using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<WebAppUser> _userManager;
        private readonly CareerLogService _careerLogService;

        public IndexModel(UserManager<WebAppUser> userManager, SignInManager<WebAppUser> signInManager,
            CareerLogService careerLogService)
        {
            _userManager = userManager;
            _careerLogService = careerLogService;
        }

        public string Username { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty]
        public FormModel Form { get; set; }

        public class FormModel
        {
            public CareerInputModel CareerInput { get; set; }
            public AccountInfoInputModel AccountInput { get; set; }
        }

        public class CareerInputModel
        {
            [Required]
            [MaxLength(60)]
            [Display(Name = "Career name")]
            public string CareerName { get; set; }

            [Display(Name = "Playstyle")] public CareerPlaystyle CareerPlaystyle { get; set; }

            [Display(Name = "Difficulty")] public DifficultyLevel DifficultyLevel { get; set; }

            [Display(Name = "Failure Mod")] public FailureModel FailureModel { get; set; }

            [Display(Name = "Configurable Start (leave at 'NONE' if unused)")]
            public ConfigurableStart ConfigurableStart { get; set; }

            [Display(Name = "Description")] public string DescriptionText { get; set; }

            [Display(Name = "Mod Recency")] public ModRecency ModRecency { get; set; }

            [Display(Name = "Installation Date")]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
            public DateTime? CreationDate { get; set; }

            [Display(Name = "RP-1 Version (X.Y.Z)")]
            public Version ModVersion { get; set; }
        }

        public class AccountInfoInputModel
        {
            [Display(Name = "Preferred name")]
            [StringLength(20)]
            public string PreferredName { get; set; }
        }

        public async Task<List<CareerListItem>> GetAllCareersAsync()
        {
            return await _careerLogService.GetCareerListWithTokensAsync(User.Identity.Name);
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

            await LoadAsync(user);

            if (!string.IsNullOrWhiteSpace(user.PreferredName))
            {
                Form = new FormModel
                {
                    AccountInput = new AccountInfoInputModel
                    {
                        PreferredName = user.PreferredName
                    }
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAccountAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            string prefName = Form.AccountInput.PreferredName;
            user.PreferredName = string.IsNullOrWhiteSpace(prefName) ? null : prefName;
            await _userManager.UpdateAsync(user);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCareerAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.IsInRoleAsync(user, Constants.Roles.Member))
            {
                return StatusCode(403);
            }

            await LoadAsync(user);

            await _careerLogService.CreateAsync(new CareerLog
            {
                Name = Form.CareerInput.CareerName,
                UserLogin = Username,
                EligibleForRecords = true,
                CareerLogMeta = CreateCareerLogMeta()
            });

            StatusMessage = "A new career was created successfully";
            return RedirectToPage();
        }

        public async Task<RedirectToPageResult> OnPostDelete(string token)
        {
            await _careerLogService.DeleteByTokenAsync(token);

            return new RedirectToPageResult("Index");
        }

        protected CareerLogMeta CreateCareerLogMeta()
        {
            return new CareerLogMeta
            {
                CareerPlaystyle = Form.CareerInput.CareerPlaystyle,
                DifficultyLevel = Form.CareerInput.DifficultyLevel,
                ConfigurableStart = Form.CareerInput.ConfigurableStart,
                FailureModel = Form.CareerInput.FailureModel,
                DescriptionText = Form.CareerInput.DescriptionText,
                ModRecency = Form.CareerInput.ModRecency,
                VersionTag = Form.CareerInput.ModVersion?.ToString(),
                VersionSort = CareerLogMeta.CreateSortableVersion(Form.CareerInput.ModVersion),
                CreationDate = Form.CareerInput.CreationDate.HasValue ? DateTime.SpecifyKind(Form.CareerInput.CreationDate.Value, DateTimeKind.Utc) : (DateTime?)null
            };
        }
    }
}