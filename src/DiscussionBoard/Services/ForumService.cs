using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public class ForumService(ApplicationDbContext context)
{
    public async Task<List<Forum>> GetAllAsync()
    {
        return await context.Forums
            .AsQueryable()
            .Where(f => !f.IsDeleted)
            .ToListAsync();
    }
}
