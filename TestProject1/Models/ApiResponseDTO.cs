using System.Text.Json.Serialization;

namespace TestProject1.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
