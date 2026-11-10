using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DiscussionBoard.Services
{
    public class DiscussionThreadService(
        ApplicationDbContext context,
        ILogger<DiscussionThreadService> logger)
    {
        /// <summary>
        /// Creates the thread service with a no-op logger that provides a database context.
        /// </summary>
        /// <param name="context">The database context used for thread operations.</param>
        public DiscussionThreadService(ApplicationDbContext context)
            : this(context, NullLogger<DiscussionThreadService>.Instance)
        {
        }

        /// <summary>
        /// Finds a non-deleted thread and its author.
        /// </summary>
        /// <param name="id">The thread to retrieve.</param>
        /// <returns>The matching thread, or null if none is available.</returns>
        public async Task<DiscussionThread?> GetAsync(int id)
        {
            return await context.DiscussionThreads
                .AsNoTracking()
                .Include(t => t.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        /// <summary>
        /// Retrieves all active threads belonging to the requested forum.
        /// </summary>
        /// <param name="forumId">The forum whose threads should be returned.</param>
        /// <returns>A list of non-deleted threads; empty when none match.</returns>
        public async Task<List<DiscussionThread>> GetForForumAsync(int forumId)
        {
            return await context.DiscussionThreads
                .AsNoTracking()
                .Where(t => t.ForumId == forumId && !t.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new discussion thread and saves it to the database.
        /// </summary>
        /// <param name="thread">The populated thread to create.</param>
        /// <returns>The created thread after saving.</returns>
        public async Task<DiscussionThread> AddAsync(DiscussionThread thread)
        {
            try
            {
                context.DiscussionThreads.Add(thread);
                await context.SaveChangesAsync();

                return thread;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to create discussion thread.");
                throw;
            }
        }

        /// <summary>
        /// Updates the fields of an existing, non-deleted thread.
        /// </summary>
        /// <param name="thread">Contains the thread ID and replacement title and content.</param>
        /// <returns>True when the thread was updated; false if it was not found.</returns>
        public async Task<bool> UpdateAsync(DiscussionThread thread)
        {
            try
            {
                var existingThread = await context.DiscussionThreads
                    .FirstOrDefaultAsync(t => t.Id == thread.Id && !t.IsDeleted);

                if (existingThread is null)
                {
                    return false;
                }

                // Copy only editable fields; preserve the original author, forum, and creation timestamp.
                existingThread.Title = thread.Title;
                existingThread.Content = thread.Content;

                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to update discussion thread {ThreadId}.", thread.Id);
                throw;
            }
        }
    }
}
