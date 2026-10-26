using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models
{
    public class DiscussionBase : EntityBase
    {
        [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;

        [Required] public string Content { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
