using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services

{
    public class PostService(ApplicationDbContext context)
    {
        public async Task<List<Post>> GetForThreadAsync(int threadId)
        {
            return await context.Posts
                .AsNoTracking()
                .Include(p => p.ApplicationUser)
                .Where(p => p.DiscussionThreadId == threadId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Post> AddAsync(Post post)
        {
            context.Posts.Add(post);
            await context.SaveChangesAsync();
            return post;
        }

        public async Task<Post?> GetAsync(int id)
        {
            return await context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<bool> UpdateAsync(Post post)
        {
            var existingPost = await context.Posts
                .FirstOrDefaultAsync(p => p.Id == post.Id && !p.IsDeleted);

            if (existingPost is null)
            {
                return false;
            }

            existingPost.Title = post.Title;
            existingPost.Content = post.Content;

            await context.SaveChangesAsync();

            return true;
        }
    }
}
