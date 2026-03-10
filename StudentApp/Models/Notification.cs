using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentApp.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("studentMatricula")]
    public string StudentMatricula { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("projectId")]
    public string? ProjectId { get; set; }

    [BsonElement("projectTitle")]
    public string? ProjectTitle { get; set; }

    [BsonElement("score")]
    public string? Score { get; set; }

    [BsonElement("evaluatorName")]
    public string? EvaluatorName { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed properties for UI
    [BsonIgnore]
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            if (diff.TotalMinutes < 1) return "Ahora";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"Hace {(int)diff.TotalDays}d";
            return CreatedAt.ToString("dd/MM/yyyy");
        }
    }

    [BsonIgnore]
    public Color CardBackgroundColor => IsRead ? Colors.Transparent : Color.FromArgb("#1A197fe6");

    [BsonIgnore]
    public double ReadOpacity => IsRead ? 0.6 : 1.0;
}
