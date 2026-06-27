using System.Text.Json.Serialization;

namespace Web.Models
{
    public class WebContentDto
    {
        public int Id { get; set; }

        [JsonPropertyName("showId")]
        public string? ShowId { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("releaseYear")]
        public string? ReleaseYear { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        public string DisplayType
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Type))
                {
                    return Type;
                }

                if (!string.IsNullOrWhiteSpace(ContentType))
                {
                    return ContentType;
                }

                return "Movie";
            }
        }
    }
}