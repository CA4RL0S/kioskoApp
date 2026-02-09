using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentApp.Models;

public class StudentTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

    [BsonElement("dueDate")]
    public DateTime DueDate { get; set; }

    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; }
}
