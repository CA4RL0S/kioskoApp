using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KioskoAPI.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("role")]
    public string? Role { get; set; }

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; }

    [BsonElement("fullName")]
    public string? FullName { get; set; }

    [BsonElement("department")]
    public string? Department { get; set; }

    [BsonElement("profileImageUrl")]
    public string? ProfileImageUrl { get; set; }

    [BsonElement("pronouns")]
    public string? Pronouns { get; set; }
}
