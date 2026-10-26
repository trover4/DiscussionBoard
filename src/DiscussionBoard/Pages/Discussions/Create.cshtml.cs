using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    public class Create(DiscussionThreadService threadService,
        ApplicationDbContext context) : PageModel
    {
        [BindProperty]
        public ThreadInput Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ForumId { get; set; }

        public class ThreadInput
        {
            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Content { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var forum = await context.Forums.FindAsync(ForumId);

            if (forum is null || forum.IsDeleted)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var forum = await context.Forums.FindAsync(ForumId);

            if (forum is null || forum.IsDeleted)
            {
                return NotFound();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Challenge();
            }

            var thread = new DiscussionThread
            {
                Title = Input.Title,
                Content = Input.Content,
                ForumId = ForumId,
                ApplicationUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await threadService.AddAsync(thread);

            return RedirectToPage("/Forums/Details", new {id = ForumId});
        }
    }
}
