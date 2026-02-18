using MongoDB.Bson.Serialization.Attributes;

namespace EvaluatorApp.Models;

public class Document
{
    [BsonElement("url")]
    public string Url { get; set; }
    
    [BsonElement("nombre")]
    public string Name { get; set; }
}
