using SQLite;

namespace EvaluatorApp.Models;

public class PendingEvaluation
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string ProjectId { get; set; }
    
    public string ProjectTitle { get; set; }
    
    public string JsonPayload { get; set; } // Stores the serialized Evaluation object
    
    public DateTime Timestamp { get; set; }
}
