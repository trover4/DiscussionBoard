using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Authorization
{
    public class ApplicationAuthorizationService : IApplicationAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public ApplicationAuthorizationService(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        public async Task<IdentityResult> AddRoleAsync(
            ClaimsPrincipal actingUser, string targetUserId, string roleName)
        {
            if (!IsValidAdmin(actingUser, roleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Unauthorized role."
                });
            }

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "No user selected."
                });
            }

            var user = await _userManager.FindByIdAsync(targetUserId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "User was not found."
                });
            }

            var currentUser = await _userManager.GetUserAsync(actingUser);

            if (currentUser == null ||
                !await _userManager.IsInRoleAsync(currentUser, ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Only administrators can manage roles."
                });
            }

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveRoleAsync(
            ClaimsPrincipal actingUser, string targetUserId, string roleName)
        {
            if (!IsValidAdmin(actingUser, roleName))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Unauthorized role."
                });
            }

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "No user selected."
                });
            }

            var user = await _userManager.FindByIdAsync(targetUserId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "User was not found."
                });
            }

            //Prevents removing admin role from the current admin user.
            string? currentUserId = _userManager.GetUserId(actingUser);

            if (currentUserId == targetUserId)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "You can't remove your Admin role."
                });
            }

            //Checks the selected user for admin role.
            if (!await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "The selected user is not an administrator."
                });
            }

            var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);

            if (admins.Count <= 1)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Can't remove the last administrator."
                });
            }

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> CanEditAsync(
            ClaimsPrincipal principal, DiscussionBase resource)
        {
            var result = await _authorizationService.AuthorizeAsync(
            principal,
            resource,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

            return result.Succeeded;
        }

        private bool IsValidAdmin(ClaimsPrincipal actingUser, string roleName)
        {
            return actingUser.Identity?.IsAuthenticated == true
                && actingUser.IsInRole(ApplicationRoles.Admin)
                && roleName == ApplicationRoles.Admin;
        }
    }
}
