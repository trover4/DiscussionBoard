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
    }
}
