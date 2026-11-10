using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Creates a profile service backed by the application database.
        /// </summary>
        /// <param name="context">Provides access to existing user profiles.</param>
        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks that a display name is not used by another account.
        /// </summary>
        /// <param name="displayName">The proposed name, which is trimmed before checking.</param>
        /// <param name="excludeUserId">An optional account ID to ignore when editing the current user.</param>
        /// <returns>True if the normalized name is valid and no other user has it.</returns>
        public async Task<bool> IsDisplayNameAvailableAsync(
            string displayName,
            string? excludeUserId = null)
        {
            if (displayName == null)
            {
                return false;
            }

            displayName = displayName.Trim();

            if (displayName.Length < 2 || displayName.Length > 60)
            {
                return false;
            }

            return !await _context.Users.AnyAsync(
                user => user.DisplayName.ToLower() == displayName.ToLower()
                && (excludeUserId == null || user.Id != excludeUserId));
        }
    }
}
