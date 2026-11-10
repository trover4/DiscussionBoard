using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionBoard.Authorization
{
    public class IsOwnerOrAdminHandler : AuthorizationHandler<IsOwnerOrAdminRequirement, DiscussionBase>
    {
        /// <summary>
        /// Grants edit permission when the user is an administrator or owns the thread/post.
        /// </summary>
        /// <param name="context">Contains the current user and authorization decision.</param>
        /// <param name="requirement">The policy requirement to satisfy.</param>
        /// <param name="resource">The discussion thread or post being checked.</param>
        /// <returns>A completed task after evaluating the requirement.</returns>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsOwnerOrAdminRequirement requirement,
            DiscussionBase resource)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole(ApplicationRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Ownership is checked against the stored author ID, not user-submitted form data.
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null && userId == resource.ApplicationUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
