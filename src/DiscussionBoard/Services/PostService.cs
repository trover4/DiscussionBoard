using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services

{
    public class PostService(ApplicationDbContext context)
    {
        /// <summary>
        /// Gets active posts and their authors for the requested thread.
        /// </summary>
        /// <param name="threadId">The discussion whose posts should be loaded.</param>
        /// <returns>A list of non-deleted posts for that thread.</returns>
        public async Task<List<Post>> GetForThreadAsync(int threadId)
        {
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.ApplicationUser)
                .Where(p => p.DiscussionThreadId == threadId && !p.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new reply and saves it to the database.
        /// </summary>
        /// <param name="post">The reply to save.</param>
        /// <returns>The saved post.</returns>
        public async Task<Post> AddAsync(Post post)
        {
            context.Posts.Add(post);
            await context.SaveChangesAsync();
            return post;
        }

        /// <summary>
        /// Finds an active post by its ID without tracking the result.
        /// </summary>
        /// <param name="id">The post to retrieve.</param>
        /// <returns>The matching post, or null if missing or deleted.</returns>
        public async Task<Post?> GetAsync(int id)
        {
            return await context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        /// <summary>
        /// Updates the title and content of an existing post without changing its ownership.
        /// </summary>
        /// <param name="post">Contains the post ID and new editable values.</param>
        /// <returns>True when updated; false when no active post matches.</returns>
        public async Task<bool> UpdateAsync(Post post)
        {
            var existingPost = await context.Posts
                .FirstOrDefaultAsync(p => p.Id == post.Id && !p.IsDeleted);

            if (existingPost is null)
            {
                return false;
            }

            // Leave the thread, parent reply, author, and creation time unchanged.
            existingPost.Title = post.Title;
            existingPost.Content = post.Content;

            await context.SaveChangesAsync();

            return true;
        }
    }
}
