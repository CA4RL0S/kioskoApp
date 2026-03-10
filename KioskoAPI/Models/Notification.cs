using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KioskoAPI.Models;

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
}
