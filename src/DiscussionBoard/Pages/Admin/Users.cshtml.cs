using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Admin
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationAuthorizationService _authorizationService;

        /// <summary>
        /// Sets up the administrator page for looking up users and changing their roles.
        /// </summary>
        /// <param name="userManager">Provides account lookup and role membership information.</param>
        /// <param name="authorizationService">Performs validated administrator role changes.</param>
        public UsersModel(UserManager<ApplicationUser> userManager,
            IApplicationAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        [BindProperty]
        public string? SelectedUserId { get; set; }

        public List<SelectListItem> UserOptions { get; set; } = [];

        public ApplicationUser? SelectedUser { get; set; }

        public bool SelectedUserIsAdmin { get; set; }

        /// <summary>
        /// Populates the dropdown selection when the administration page opens.
        /// </summary>
        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

        /// <summary>
        /// Loads the selected account and whether it currently has administrator access.
        /// </summary>
        /// <returns>The page with the selected user, or the page with a selection error.</returns>
        public async Task<IActionResult> OnPostSelectAsync()
        {
            await LoadUsersAsync();

            if (string.IsNullOrWhiteSpace(SelectedUserId))
            {
                ModelState.AddModelError(nameof(SelectedUserId),
                    "Please select a user.");
                return Page();
            }

            SelectedUser = await _userManager.FindByIdAsync(SelectedUserId);

            if (SelectedUser is null)
            {
                ModelState.AddModelError(nameof(SelectedUserId),
                    "The selected user was not found.");
                return Page();
            }

            SelectedUserIsAdmin = await _userManager.IsInRoleAsync(
                SelectedUser, ApplicationRoles.Admin);

            return Page();
        }

        /// <summary>
        /// Builds the dropdown options from the accounts in the Identity database.
        /// </summary>
        private async Task LoadUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            UserOptions = new List<SelectListItem>();

            foreach (var user in users)
            {
                UserOptions.Add(new SelectListItem
                {
                    Value = user.Id,
                    Text = user.DisplayName + " (" + user.Email + ")"
                });
            }
        }

        /// <summary>
        /// Adds or removes the Admin role for the selected account through the authorization service.
        /// </summary>
        /// <returns>A redirect when successful, or an error page.</returns>
        public async Task<IActionResult> OnPostUpdateRoleAsync()
        {
            await LoadUsersAsync();

            if (string.IsNullOrWhiteSpace(SelectedUserId))
            {
                ModelState.AddModelError("", "Please select a user.");
                return Page();
            }

            SelectedUser = await _userManager.FindByIdAsync(SelectedUserId);

            if (SelectedUser == null)
            {
                ModelState.AddModelError("", "User not found.");
                return Page();
            }

            SelectedUserIsAdmin = await _userManager.IsInRoleAsync(
                SelectedUser, ApplicationRoles.Admin);

            IdentityResult result;

            // Toggles membership based on the selected user's current role.
            if (SelectedUserIsAdmin)
            {
                result = await _authorizationService.RemoveRoleAsync(
                    User, SelectedUserId, ApplicationRoles.Admin);
            }
            else
            {
                result = await _authorizationService.AddRoleAsync(
                    User, SelectedUserId, ApplicationRoles.Admin);
            }

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return Page();
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Handles adding roles for the selected account.
        /// </summary>
        /// <returns>A redirect back to the administration page.</returns>
        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedUserId))
            {
                await _authorizationService.AddRoleAsync(
                    User, SelectedUserId, ApplicationRoles.Admin);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Handles removing roles for the selected account.
        /// </summary>
        /// <returns>A redirect back to the administration page.</returns>
        public async Task<IActionResult> OnPostRemoveAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedUserId))
            {
                await _authorizationService.RemoveRoleAsync(
                    User, SelectedUserId, ApplicationRoles.Admin);
            }

            return RedirectToPage();
        }
    }
}
