using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Forums

{
    public class Details(ApplicationDbContext context) : PageModel
    {
        public Forum Forum { get; set; } = null!;

        public List<DiscussionThread> Threads { get; set; } = [];

        /// <summary>
        /// Loads the requested forum and its discussion threads.
        /// </summary>
        /// <param name="id">The forum ID from the request.</param>
        /// <returns>The forum page, or NotFound when the forum is missing or deleted.</returns>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var forum = await context.Forums
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (forum is null)
            {
                return NotFound();
            }

            Forum = forum;

            Threads = await context.DiscussionThreads
                .AsNoTracking()
                .Include(t => t.ApplicationUser)
                .Where(t => t.ForumId == id && !t.IsDeleted)
                .ToListAsync();

            return Page();
        }
    }
}
