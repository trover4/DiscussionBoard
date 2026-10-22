using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    public class Edit(DiscussionThreadService threadService) : PageModel
    {
        [BindProperty]
        public ThreadInput Input { get; set; } = new();

        public int ThreadId { get; set; }

        public class ThreadInput
        {
            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Content { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var thread = await threadService.GetAsync(id);

            if (thread is null)
            {
                return NotFound();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (thread.ApplicationUserId != userId)
            {
                return Forbid();
            }

            ThreadId = thread.Id;

            Input = new ThreadInput
            {
                Title = thread.Title,
                Content = thread.Content
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ThreadId = id;
                return Page();
            }

            var existingThread = await threadService.GetAsync(id);

            if (existingThread is null)
            {
                return NotFound();
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (existingThread.ApplicationUserId != userId)
            {
                return Forbid();
            }

            var updatedThread = new DiscussionThread
            {
                Id = existingThread.Id,
                Title = Input.Title,
                Content = Input.Content
            };

            bool success = await threadService.UpdateAsync(updatedThread);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("/Forums/Details", new { id = existingThread.ForumId });
        }
    }
}
