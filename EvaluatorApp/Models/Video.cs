using MongoDB.Bson.Serialization.Attributes;

namespace EvaluatorApp.Models;

public class Video
{
    [BsonElement("url")]
    public string Url { get; set; }
    
    [BsonElement("descripcion")]
    public string Description { get; set; }
    
    // For offline support
    public string LocalPath { get; set; }
}
