using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using SQLite;

namespace EvaluatorApp.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [PrimaryKey]
    public string Id { get; set; }

    [BsonElement("nombre")] // Renamed from "title" as per request
	public string? Title { get; set; }

    [BsonElement("cycle")]
	public string? Cycle { get; set; }

    [BsonElement("informacion")] // Renamed from "description" as per request
	public string? Description { get; set; }

    [BsonElement("imageUrl")]
	public string? ImageUrl { get; set; }

    [BsonElement("integrantes")] // New field for matriculas
    [Ignore]
    public List<string> Members { get; set; } = new List<string>();
	
    [BsonElement("statusText")]
	public string? StatusText { get; set; }

    [BsonIgnore]
    [JsonIgnore]
    [Ignore]
	public Color? StatusColor { get; set; }

    [BsonIgnore]
    [JsonIgnore]
    [Ignore]
	public Color? StatusTextColor { get; set; }

    [BsonIgnore]
    [JsonIgnore]
    [Ignore]
	public Color? StatusBackgroundColor { get; set; }
	
    [BsonElement("isPending")]
    // ... rest of the file
	public bool IsPending { get; set; }

    [BsonElement("isEvaluated")]
	public bool IsEvaluated { get; set; }

    [BsonElement("score")]
	public string? Score { get; set; }

    // Evaluation break down (persisting these is good for the details page)
    // Evaluation break down (persisting these is good for the details page)
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

    // Helper for ranking to parse Score safely
    [BsonIgnore]
    [JsonIgnore]
    public double ScoreValue 
    {
        get 
        {
            if (double.TryParse(Score, out double result))
                return result;
            return 0;
        }
    }

    [BsonElement("evaluations")]
    [Ignore]
    public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    // Quick Hack for persistence: JSON strings
    // In a real app we'd use a One-to-Many generic solution or TextBlob
    public string MembersJson { get; set; }
    
    public string EvaluationsJson { get; set; }

    [BsonElement("videos")]
    [Ignore]
    public List<Video> Videos { get; set; } = new List<Video>();

    [BsonElement("documentos")]
    [Ignore]
    public List<Document> Documents { get; set; } = new List<Document>();

    public void RestoreVisuals()
    {
        if (IsEvaluated)
        {
            StatusText = "Evaluado";
            StatusColor = Colors.Green;
            StatusTextColor = Colors.Green;
            StatusBackgroundColor = Color.FromRgba("#E6F4EA"); // Light Green
        }
        else
        {
            StatusText = "Pendiente de Evaluación";
            StatusColor = Color.FromRgba("#CA8A04"); // Dark Yellow
            StatusTextColor = Color.FromRgba("#CA8A04");
            StatusBackgroundColor = Color.FromRgba("#FEFce8"); // Light Yellow
        }
    }

    public void UpdatePersonalizedStatus(string currentUserId)
    {
        var myEval = Evaluations?.FirstOrDefault(e => e.EvaluatorId == currentUserId);
        
        if (myEval != null)
        {
            IsEvaluated = true;
            IsPending = false;
            Score = myEval.TotalScore.ToString(); // Show MY score in the list
            RestoreVisuals();
        }
        else
        {
            IsEvaluated = false;
            IsPending = true;
            Score = string.Empty; // No score for me yet
            
            // Re-apply "Pending" visuals
            StatusText = "Pendiente de Evaluación";
            StatusColor = Color.FromArgb("#F59E0B");
            StatusTextColor = Color.FromRgb(180, 83, 9);
            StatusBackgroundColor = Color.FromRgba(255, 251, 235, 255);
        }
    }
    [BsonElement("videos")]
    public List<Video> Videos { get; set; } = new List<Video>();

    public bool HasVideo => Videos != null && Videos.Count > 0;
}

public class Video
{
    [BsonElement("url")]
    public string Url { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }
}

public class Evaluation
{
    [BsonElement("evaluatorId")]
    public string EvaluatorId { get; set; }

    [BsonElement("evaluatorName")]
    public string EvaluatorName { get; set; }

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
