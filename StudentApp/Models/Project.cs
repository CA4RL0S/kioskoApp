using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StudentApp.Models;

[BsonIgnoreExtraElements]
public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("nombre")]
    public string? Title { get; set; }

    [BsonElement("cycle")]
    public string? Cycle { get; set; }

    [BsonElement("informacion")]
    public string? Description { get; set; }

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("integrantes")]
    public List<string> Members { get; set; } = new List<string>();

    [BsonElement("statusText")]
    public string? StatusText { get; set; }

    [BsonElement("isPending")]
    public bool IsPending { get; set; }

    [BsonElement("isEvaluated")]
    public bool IsEvaluated { get; set; }

    [BsonElement("score")]
    public string? Score { get; set; }

    [BsonElement("problemScore")]
    public double ProblemScore { get; set; }

    [BsonElement("innovationScore")]
    public double InnovationScore { get; set; }

    [BsonElement("techScore")]
    public double TechScore { get; set; }

    [BsonElement("impactScore")]
    public double ImpactScore { get; set; }

    [BsonElement("presentationScore")]
    public double PresentationScore { get; set; }

    [BsonElement("knowledgeScore")]
    public double KnowledgeScore { get; set; }

    [BsonElement("resultsScore")]
    public double ResultsScore { get; set; }
}
