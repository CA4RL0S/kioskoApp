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

    [BsonElement("evaluations")]
    public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    // Computed properties for UI
    public string DisplayScore => IsEvaluated ? $"{Score ?? "0"}/70" : "Sin calificar";
    public string DisplayImage => !string.IsNullOrEmpty(ImageUrl) ? ImageUrl : "https://res.cloudinary.com/djwpi6z29/image/upload/v1770699551/avatar-default-user-profile-icon-social-media-vector-57234208_y8gtgs.jpg";
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "Proyecto" : Title;

    [BsonIgnore]
    public Color StatusBackgroundColor { get; set; }
    [BsonIgnore]
    public Color StatusTextColor { get; set; }
    [BsonIgnore]
    public Color StatusDotColor { get; set; }
    [BsonIgnore]
    public string DisplayStatus { get; set; }

    public void RestoreVisuals()
    {
        if (IsEvaluated)
        {
            DisplayStatus = "Evaluado";
            StatusBackgroundColor = Color.FromArgb("#E6F4EA");
            StatusTextColor = Colors.Green;
            StatusDotColor = Colors.Green;
        }
        else if (IsPending)
        {
            DisplayStatus = "En revisión";
            StatusBackgroundColor = Color.FromArgb("#FEFce8");
            StatusTextColor = Color.FromArgb("#B45309");
            StatusDotColor = Color.FromArgb("#F59E0B");
        }
        else
        {
            DisplayStatus = "Activo";
            StatusBackgroundColor = Color.FromArgb("#dbeafe");
            StatusTextColor = Color.FromArgb("#1e40af");
            StatusDotColor = Color.FromArgb("#197fe6");
        }
    }

    public string EvaluationDate
    {
        get
        {
            if (Evaluations != null && Evaluations.Count > 0)
            {
                var latest = Evaluations.OrderByDescending(e => e.Timestamp).First();
                return latest.Timestamp.ToString("dd MMM yyyy");
            }
            return "";
        }
    }
}

[BsonIgnoreExtraElements]
public class Evaluation
{
    [BsonElement("evaluatorId")]
    public string? EvaluatorId { get; set; }

    [BsonElement("evaluatorName")]
    public string? EvaluatorName { get; set; }

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

    [BsonElement("totalScore")]
    public double TotalScore { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("comments")]
    public string? Comments { get; set; }
}
