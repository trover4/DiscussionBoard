using DiscussionBoard.Models;
using DiscussionBoard.Services;
using DiscussionBoard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public UserProfile Profile { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            Profile = new UserProfile
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
            Profile.DisplayName = Profile.DisplayName?.Trim()
                                  ?? string.Empty;

            // Validate length after trimming.
            if (Profile.DisplayName.Length < 2 ||
                Profile.DisplayName.Length > 60)
            {
                ModelState.AddModelError(
                    "Profile.DisplayName",
                    "Display name must be between 2 and 60 characters.");
            }

            // Validate the selected avatar against the catalog.
            if (!AvatarCatalog.IsValid(Profile.AvatarId))
            {
                ModelState.AddModelError(
                    "Profile.AvatarId",
                    "Please select a valid avatar.");
            }

            // Check that no other user has this display name.
            if (Profile.DisplayName.Length >= 2 &&
                Profile.DisplayName.Length <= 60)
            {
                bool available =
                    await _profileService.IsDisplayNameAvailableAsync(
                        Profile.DisplayName, user.Id);

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
            user.DisplayName = Profile.DisplayName;
            user.AvatarId = Profile.AvatarId;

            IdentityResult result =
                await _userManager.UpdateAsync(user);

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
