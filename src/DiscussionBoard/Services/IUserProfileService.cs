namespace DiscussionBoard.Services
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Checks whether a profile name can be assigned to an account.
        /// </summary>
        /// <param name="displayName">The proposed display name.</param>
        /// <param name="excludeUserId">An existing user ID to omit from the uniqueness check.</param>
        /// <returns>True when the name is valid and available.</returns>
        Task<bool> IsDisplayNameAvailableAsync(
            string displayName,
            string? excludeUserId = null);
    }
}
