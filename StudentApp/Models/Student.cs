using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentApp.Models;

public class Student
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("matricula")]
    public string Matricula { get; set; }

    [BsonElement("nombre")] // Full Name
    public string Name { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("projectId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ProjectId { get; set; } // Nullable if not yet assigned

    [BsonElement("role")]
    public string Role { get; set; } = "Student";
}
