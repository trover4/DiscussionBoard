using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions
{
    public class Details(DiscussionThreadService threadService,
        PostService postService) : PageModel
    {
        public DiscussionThread Thread { get; set; } = null!;

        public List<Post> Posts { get; set; } = [];

        public List<PostTreeBuilder.Node> PostTree { get; set; } = [];

        [BindProperty]
        public ReplyInput Input { get; set; } = new();

        public class ReplyInput
        {
            [Required]
            public string Content { get; set; } = string.Empty;

            public int? ParentPostId { get; set; }
        }

        /// <summary>
        /// Loads the requested discussion and replies.
        /// </summary>
        /// <param name="threadId">The ID of the discussion to display.</param>
        /// <returns>The discussion page, or NotFound if the thread does not exist.</returns>
        public async Task<IActionResult> OnGetAsync(int threadId)
        {
            return await LoadThreadAsync(threadId);
        }

        /// <summary>
        /// Validates and saves a reply to a thread or post.
        /// </summary>
        /// <param name="threadId">The thread receiving the reply.</param>
        /// <returns>A redirect after saving, or a challenge, validation page, or error response.</returns>
        public async Task<IActionResult> OnPostReplyAsync(int threadId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return await LoadThreadAsync(threadId);
            }

            var thread = await threadService.GetAsync(threadId);

            if (thread is null)
            {
                return NotFound();
            }

            // Reject parent-post IDs from other threads before allowing a nested reply.
            if (Input.ParentPostId is int parentId)
            {
                var posts = await postService.GetForThreadAsync(threadId);

                if (posts.All(p => p.Id != parentId))
                {
                    return BadRequest();
                }
            }

            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Challenge();
            }

            var post = new Post
            {
                Title = "Reply",
                Content = Input.Content,
                DiscussionThreadId = threadId,
                ParentPostId = Input.ParentPostId,
                ApplicationUserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await postService.AddAsync(post);

            return RedirectToPage(new { threadId });
        }

        /// <summary>
        /// Loads thread details and builds the reply tree used by the Razor page.
        /// </summary>
        /// <param name="threadId">The thread to load.</param>
        /// <returns>The rendered page, or NotFound when the thread is missing.</returns>
        private async Task<IActionResult> LoadThreadAsync(int threadId)
        {
            var thread = await threadService.GetAsync(threadId);

            if (thread is null)
            {
                return NotFound();
            }

            Thread = thread;
            Posts = await postService.GetForThreadAsync(threadId);
            PostTree = PostTreeBuilder.Build(Posts);

            return Page();
        }
    }
}
