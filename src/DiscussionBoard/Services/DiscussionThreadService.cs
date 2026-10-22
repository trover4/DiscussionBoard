using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services
{
    public class DiscussionThreadService(
        ApplicationDbContext context,
        ILogger<DiscussionThreadService> logger)
    {
        //Retrieve a thread by ID
        public async Task<DiscussionThread?> GetAsync(int id)
        {
            return await context.DiscussionThreads
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        }

        //Retrieve all threads in a forum
        public async Task<List<DiscussionThread>> GetForForumAsync(int forumId)
        {
            return await context.DiscussionThreads
                .AsNoTracking()
                .Where(t => t.ForumId == forumId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        //Add a new thread
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

        //Update a thread
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
