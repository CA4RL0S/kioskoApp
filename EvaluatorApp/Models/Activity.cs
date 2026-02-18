using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using SQLite;

namespace EvaluatorApp.Models;

public class Activity
{
    [PrimaryKey, AutoIncrement]
    [JsonIgnore]
    public int LocalId { get; set; }

    [JsonPropertyName("id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [JsonIgnore]
    public bool IsSynced { get; set; }

    [JsonPropertyName("userId")]
    [BsonElement("userId")]
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
