using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace DiscussionBoard.Pages
{
    [Authorize]
    public class MyProfile : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserProfileService _profileService;

        public MyProfile(
            UserManager<ApplicationUser> userManager,
            IUserProfileService profileService)
        {
            _userManager = userManager;
            _profileService = profileService;
        }

        [BindProperty]
        public UserProfile Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            Input = new UserProfile
            {
                DisplayName = user.DisplayName,
                AvatarId = user.AvatarId
            };

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Normalize the submitted display name.
            Input.DisplayName = Input.DisplayName?.Trim() ?? string.Empty;

            // Validate length after trimming.
            if (Input.DisplayName.Length < 2 ||
                Input.DisplayName.Length > 60)
            {
                ModelState.AddModelError(
                    "Profile.DisplayName",
                    "Display name must be between 2 and 60 characters.");
            }

            // Validate the selected avatar against the catalog.
            if (!AvatarCatalog.IsValid(Input.AvatarId))
            {
                ModelState.AddModelError(
                    "Profile.AvatarId",
                    "Please select a valid avatar.");
            }

            // Check that no other user has this display name.
            if (Input.DisplayName.Length is >= 2 and <= 60)
            {
                bool available =
                    await _profileService.IsDisplayNameAvailableAsync(
                        Input.DisplayName, user.Id);

                if (!available)
                {
                    ModelState.AddModelError(
                        "Profile.DisplayName",
                        "This display name is already taken.");
                }
            }

            if (!ModelState.IsValid)
                return Page();

            // Update only the permitted fields.
            user.DisplayName = Input.DisplayName;
            user.AvatarId = Input.AvatarId;

            IdentityResult result;
            try
            {
                result = await _userManager.UpdateAsync(user);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 })
            {
                ModelState.AddModelError(
                    "Input.DisplayName",
                    "That display name is already taken.");

                return Page();
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }

                return Page();
            }

            SuccessMessage = "Your profile has been updated.";

            return RedirectToPage();
        }
    }
}
