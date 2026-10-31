using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.ViewModels
{
    public class UserProfile
    {
        [Required(ErrorMessage = "Please enter a display name.")]
        [MaxLength(60, ErrorMessage = "Display name can't be longer than 60 characters.")]
        [MinLength(2, ErrorMessage = "Display name must be longer than 1 character.")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an avatar.")]
        public string AvatarId { get; set; } = "default";
    }
}
