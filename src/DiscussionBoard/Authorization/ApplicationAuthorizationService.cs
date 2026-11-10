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

        /// <summary>
        /// Creates the authorization service used for role changes and ownership checks.
        /// </summary>
        /// <param name="userManager">Access to Identity user and role membership data.</param>
        /// <param name="authorizationService">Evaluates resource-based authorization policies.</param>
        public ApplicationAuthorizationService(UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Assigns the administrator role to the selected user after validating the acting administrator.
        /// </summary>
        /// <param name="actingUser">The user requesting the role change.</param>
        /// <param name="targetUserId">The ID of the account to update.</param>
        /// <param name="roleName">The role to assign; only Admin is permitted.</param>
        /// <returns>The Identity operation result, including validation errors when the request is rejected.</returns>
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

        /// <summary>
        /// Removes administrator access while preventing self-removal and removal of the final administrator.
        /// </summary>
        /// <param name="actingUser">The user requesting the role change.</param>
        /// <param name="targetUserId">The ID of the account to update.</param>
        /// <param name="roleName">The role to remove; only Admin is permitted.</param>
        /// <returns>The Identity operation result, including an explanation if removal is not allowed.</returns>
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

            // Keep at least one administrator account so the application cannot be locked out of role management.
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

        /// <summary>
        /// Checks whether the current user can edit the supplied discussion thread or post.
        /// </summary>
        /// <param name="principal">The user attempting the edit.</param>
        /// <param name="resource">The discussion item being edited.</param>
        /// <returns>True when the owner-or-administrator policy succeeds; otherwise false.</returns>
        public async Task<bool> CanEditAsync(
            ClaimsPrincipal principal, DiscussionBase resource)
        {
            var result = await _authorizationService.AuthorizeAsync(
            principal,
            resource,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

            return result.Succeeded;
        }

        /// <summary>
        /// Helper method for the initial authentication, administrator-role, and allowed-role checks.
        /// </summary>
        /// <param name="actingUser">The user requesting the change.</param>
        /// <param name="roleName">The role being modified.</param>
        /// <returns>True only for an authenticated administrator changing the Admin role.</returns>
        private bool IsValidAdmin(ClaimsPrincipal actingUser, string roleName)
        {
            return actingUser.Identity?.IsAuthenticated == true
                && actingUser.IsInRole(ApplicationRoles.Admin)
                && roleName == ApplicationRoles.Admin;
        }
    }
}
