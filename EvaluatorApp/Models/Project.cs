using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace EvaluatorApp.Models;

public class Project
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
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
    public List<string> Members { get; set; } = new List<string>();
	
    [BsonElement("statusText")]
	public string? StatusText { get; set; }

    [BsonIgnore]
    [JsonIgnore]
	public Color? StatusColor { get; set; }

    [BsonIgnore]
    [JsonIgnore]
	public Color? StatusTextColor { get; set; }

    [BsonIgnore]
    [JsonIgnore]
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
    public List<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    public void RestoreVisuals()
    {
        if (IsEvaluated)
        {
            StatusText = "Evaluado";
            StatusColor = Colors.Green;
            StatusTextColor = Color.FromRgb(21, 128, 61);
            StatusBackgroundColor = Color.FromRgba(240, 253, 244, 255);
        }
        else
        {
            StatusText = "Pendiente de Evaluación";
            StatusColor = Color.FromArgb("#F59E0B");
            StatusTextColor = Color.FromRgb(180, 83, 9);
            StatusBackgroundColor = Color.FromRgba(255, 251, 235, 255);
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
