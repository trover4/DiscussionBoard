using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    /// <summary>
    /// Holds validated title and content fields submitted when editing a thread or post.
    /// </summary>
    public class DiscussionEditViewModel
    {
        [Required]
        [StringLength(160, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
