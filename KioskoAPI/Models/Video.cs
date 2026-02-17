using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KioskoAPI.Models;

public class Video
{
    [BsonElement("url")]
    public string? Url { get; set; }

    [BsonElement("titulo")]
    public string? Title { get; set; }

    [BsonElement("descripcion")]
    public string? Description { get; set; }
}
