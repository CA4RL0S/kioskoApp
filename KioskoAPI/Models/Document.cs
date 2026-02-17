using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KioskoAPI.Models;

public class Document
{
    [BsonElement("url")]
    public string? Url { get; set; }

    [BsonElement("nombre")]
    public string? Name { get; set; }

    [BsonElement("tipo")]
    public string? Type { get; set; } // e.g., "PDF", "DOCX"
}
