using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KioskoAPI.Models;

public class Activity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("type")]
    public string? Type { get; set; } // "evaluation_completed", "comment_added"

    [BsonElement("projectTitle")]
    public string? ProjectTitle { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("icon")]
    public string? Icon { get; set; } // Material icon name, e.g. "check_circle", "comment"

    [BsonElement("iconColor")]
    public string? IconColor { get; set; } // Hex color, e.g. "#10B981"
}
