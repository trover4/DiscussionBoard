using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscussionBoard.Models
{
    /// <summary>
    /// Represents the timestamp, level, message, and exception data in a Serilog JSON event.
    /// </summary>
    public class LogEntry
    {
        [JsonPropertyName("@t")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("@l")] // Info messages do not have their level logged to save space
        public string? Level { get; set; } = "Information";

        [JsonPropertyName("@m")]
        public string? Message { get; set; } = string.Empty;

        [JsonPropertyName("@x")]
        public string? Exception { get; set; } = string.Empty;
    }
}
