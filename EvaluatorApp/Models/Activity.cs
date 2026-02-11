using System.Text.Json.Serialization;

namespace EvaluatorApp.Models;

public class Activity
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("projectTitle")]
    public string? ProjectTitle { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("iconColor")]
    public string? IconColor { get; set; }

    // Computed property for display
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - Timestamp;
            if (diff.TotalMinutes < 1) return "Ahora";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes}m";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays}d";
            return Timestamp.ToString("dd MMM, h:mm tt");
        }
    }
}
