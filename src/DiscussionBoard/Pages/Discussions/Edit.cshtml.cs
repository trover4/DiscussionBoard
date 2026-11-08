using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    public class ThreadEditModel(
        DiscussionThreadService threadService,
        IApplicationAuthorizationService authorizationService) : PageModel
    {
        [BindProperty]
        public DiscussionEditViewModel Input { get; set; } = new();

        public int ThreadId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var thread = await threadService.GetAsync(id);

            if (thread is null)
            {
                return NotFound();
            }

            if (!await authorizationService.CanEditAsync(User, thread))
            {
                return Forbid();
            }

            ThreadId = thread.Id;

            Input = new DiscussionEditViewModel
            {
                Title = thread.Title,
                Content = thread.Content
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var existingThread = await threadService.GetAsync(id);

            if (existingThread is null)
            {
                return NotFound();
            }

            if (!await authorizationService.CanEditAsync(User, existingThread))
            {
                return Forbid();
            }

            ThreadId = id;

            if (!ModelState.IsValid)
            {
                return Page();
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
