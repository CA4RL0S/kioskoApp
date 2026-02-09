using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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

    [BsonIgnore] // Colors are UI concerns, not usually stored directly in DB unless mapped. Storing hex might be better but for now let's reconstruct or ignore.
                 // Actually, to persist visual state we might want to store the hex string.
                 // For simplicity, let's assume status text drives color in a real app, or store hex.
                 // I will KEEP them as is to avoid breaking binding, but ignore them for DB to assume they are derived or non-persistent for this step, 
                 // OR I will make them non-mapped. 
                 // Wait, if I ignore them, they won't come back on reload.
                 // Better strategy: Store 'StatusHex' and similar, or just map 'StatusText' to colors in the UI layer.
                 // To minimize refactor risk: I will ignore Color properties for DB and rely on 'StatusText' to set them after load if possible, 
                 // OR just don't persist them and accept they might be default.
                 // Let's see... the hardcoded data set colors. 
                 // I will add a method 'RestoreColors()' to set colors based on StatusText or Score after loading from DB.
	public Color? StatusColor { get; set; }

    [BsonIgnore]
	public Color? StatusTextColor { get; set; }

    [BsonIgnore]
	public Color? StatusBackgroundColor { get; set; }
	
    [BsonElement("isPending")]
	public bool IsPending { get; set; }

    [BsonElement("isEvaluated")]
	public bool IsEvaluated { get; set; }

    [BsonElement("score")]
	public string? Score { get; set; }

    // Evaluation break down (persisting these is good for the details page)
    [BsonElement("innovationScore")]
    public double InnovationScore { get; set; }

    [BsonElement("techScore")]
    public double TechScore { get; set; }

    [BsonElement("presentationScore")]
    public int PresentationScore { get; set; }

    // Helper for ranking to parse Score safely
    public double ScoreValue 
    {
        get 
        {
            if (double.TryParse(Score, out double result))
                return result;
            return 0;
        }
    }

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
}
