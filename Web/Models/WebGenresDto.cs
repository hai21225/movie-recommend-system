using System.Text.Json.Serialization;

namespace Web.Models
{
    public class WebGenresDto
    {
        [JsonPropertyName("genreId")]
        public int Id { get; set; }

        [JsonPropertyName("genreName")]
        public string Name { get; set; } = string.Empty;
    }
}