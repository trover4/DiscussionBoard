using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;

        public UserProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

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
