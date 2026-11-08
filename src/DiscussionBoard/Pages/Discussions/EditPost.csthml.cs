using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    [Authorize]
    public class EditPostModel(
        PostService postService,
        IApplicationAuthorizationService authorizationService) : PageModel
    {
        [BindProperty]
        public DiscussionEditViewModel Input { get; set; } = new();

        public int ThreadId { get; set; }
        public int PostId { get; set; }

        public async Task<IActionResult> OnGetAsync(int threadId, int postId)
        {
            var post = await postService.GetAsync(postId);

            if (post is null || post.DiscussionThreadId != threadId)
            {
                return NotFound();
            }

            if (!await authorizationService.CanEditAsync(User, post))
            {
                return Forbid();
            }

            ThreadId = threadId;
            PostId = postId;

            Input = new DiscussionEditViewModel
            {
                Title = post.Title,
                Content = post.Content
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int threadId, int postId)
        {
            var post = await postService.GetAsync(postId);

            if (post is null || post.DiscussionThreadId != threadId)
            {
                return NotFound();
            }

            if (!await authorizationService.CanEditAsync(User, post))
            {
                return Forbid();
            }

            ThreadId = threadId;
            PostId = postId;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var updatedPost = new Post
            {
                Id = post.Id,
                Title = Input.Title,
                Content = Input.Content
            };

            var success = await postService.UpdateAsync(updatedPost);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToPage("/Discussions/Details",
                new { threadId });
        }
    }
}
