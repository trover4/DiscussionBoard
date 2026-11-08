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

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

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

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!string.IsNullOrWhiteSpace(SelectedUserId))
            {
                await _authorizationService.AddRoleAsync(
                    User, SelectedUserId, ApplicationRoles.Admin);
            }

            return RedirectToPage();
        }

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
