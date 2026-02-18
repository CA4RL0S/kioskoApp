using System.ComponentModel;
using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

[QueryProperty(nameof(Project), "Project")]
public partial class ProjectDetailsPage : ContentPage, INotifyPropertyChanged
{
    private readonly IMongoDBService _mongoDBService;
    
    private string _comments;
    public string Comments
    {
        get => _comments;
        set
        {
            if (_comments != value)
            {
                _comments = value;
                OnPropertyChanged();
            }
        }
    }

    private Project _project;
    public Project Project
    {
        get => _project;
        set
        {
            _project = value;
            OnPropertyChanged();
            if (_project != null)
            {
                 // Check if current user has already evaluated
                 string userId = Preferences.Get("UserId", string.Empty);
                 var myEval = _project.Evaluations?.FirstOrDefault(e => e.EvaluatorId == userId);

                 if (myEval != null)
                 {
                     // Load existing evaluation
                     ProblemScore = myEval.ProblemScore;
                     InnovationScore = myEval.InnovationScore;
                     TechScore = myEval.TechScore;
                     ImpactScore = myEval.ImpactScore;
                     PresentationScore = myEval.PresentationScore;
                     KnowledgeScore = myEval.KnowledgeScore;
                     ResultsScore = myEval.ResultsScore;
                     
                     Comments = myEval.Comments;
                 }
                 else
                 {
                     // Default values for new evaluation (middle ground)
                     ProblemScore = 5;
                     InnovationScore = 5;
                     TechScore = 5;
                     ImpactScore = 5;
                     PresentationScore = 5;
                     KnowledgeScore = 5;
                     ResultsScore = 5;
                     
                     Comments = string.Empty;
                 }
            }
        }
    }

    // 1. Planteamiento del Problema
    private double _problemScore;
    public double ProblemScore
    {
        get => _problemScore;
        set { if (_problemScore != value) { _problemScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 2. Innovacion
    private double _innovationScore;
    public double InnovationScore
    {
        get => _innovationScore;
        set { if (_innovationScore != value) { _innovationScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 3. Viabilidad Tecnica
    private double _techScore;
    public double TechScore
    {
        get => _techScore;
        set { if (_techScore != value) { _techScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 4. Impacto
    private double _impactScore;
    public double ImpactScore
    {
        get => _impactScore;
        set { if (_impactScore != value) { _impactScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 5. Presentacion
    private double _presentationScore;
    public double PresentationScore
    {
        get => _presentationScore;
        set { if (_presentationScore != value) { _presentationScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 6. Conocimiento
    private double _knowledgeScore;
    public double KnowledgeScore
    {
        get => _knowledgeScore;
        set { if (_knowledgeScore != value) { _knowledgeScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }

    // 7. Resultados
    private double _resultsScore;
    public double ResultsScore
    {
        get => _resultsScore;
        set { if (_resultsScore != value) { _resultsScore = Math.Round(value); OnPropertyChanged(); OnPropertyChanged(nameof(TotalScore)); } }
    }


    public double TotalScore => ProblemScore + InnovationScore + TechScore + ImpactScore + PresentationScore + KnowledgeScore + ResultsScore;

    // public System.Windows.Input.ICommand SelectPresentationCommand { get; private set; } // No longer needed

    public ProjectDetailsPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        
        // Remove command init
        // SelectPresentationCommand = ...

        BindingContext = this;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // UpdatePresentationSelection removed - no longer needed
    
    // UpdateOptionVisualState removed - no longer needed
    
    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (Project == null) return;

        // Get Current User Info
        string userId = Preferences.Get("UserId", string.Empty);
        string userName = Preferences.Get("UserFullName", "Evaluador");

        if (string.IsNullOrEmpty(userId))
        {
            await DisplayAlert("Error", "No se pudo identificar al evaluador. Por favor inicia sesión nuevamente.", "OK");
            return;
        }

        // Create or Update Evaluation
        var evaluation = new Evaluation
        {
            EvaluatorId = userId,
            EvaluatorName = userName,
            ProblemScore = ProblemScore,
            InnovationScore = InnovationScore,
            TechScore = TechScore,
            ImpactScore = ImpactScore,
            PresentationScore = PresentationScore,
            KnowledgeScore = KnowledgeScore,
            ResultsScore = ResultsScore,
            TotalScore = TotalScore,
            Comments = Comments,
            Timestamp = DateTime.UtcNow
        };

        // Remove existing evaluation by this user if any
        var existingEvaluation = Project.Evaluations.FirstOrDefault(ev => ev.EvaluatorId == userId);
        if (existingEvaluation != null)
        {
            Project.Evaluations.Remove(existingEvaluation);
        }

        // Add new evaluation
        Project.Evaluations.Add(evaluation);

        // Recalculate Project Averages
        if (Project.Evaluations.Any())
        {
            Project.ProblemScore = Math.Round(Project.Evaluations.Average(ev => ev.ProblemScore), 1);
            Project.InnovationScore = Math.Round(Project.Evaluations.Average(ev => ev.InnovationScore), 1);
            Project.TechScore = Math.Round(Project.Evaluations.Average(ev => ev.TechScore), 1);
            Project.ImpactScore = Math.Round(Project.Evaluations.Average(ev => ev.ImpactScore), 1);
            Project.PresentationScore = Math.Round(Project.Evaluations.Average(ev => ev.PresentationScore), 1);
            Project.KnowledgeScore = Math.Round(Project.Evaluations.Average(ev => ev.KnowledgeScore), 1);
            Project.ResultsScore = Math.Round(Project.Evaluations.Average(ev => ev.ResultsScore), 1);
            
            Project.Score = Math.Round(Project.Evaluations.Average(ev => ev.TotalScore), 1).ToString();
        }
        else
        {
             // Fallback (shouldn't happen directly after add)
            Project.ProblemScore = ProblemScore;
            Project.InnovationScore = InnovationScore;
            Project.TechScore = TechScore;
            Project.ImpactScore = ImpactScore;
            Project.PresentationScore = PresentationScore;
            Project.KnowledgeScore = KnowledgeScore;
            Project.ResultsScore = ResultsScore;
            
            Project.Score = TotalScore.ToString();
        }

        Project.IsEvaluated = true;
        Project.IsPending = false;
        
        // Update visual properties
        Project.RestoreVisuals();

        // 2. Persist
        try
        {
            await _mongoDBService.UpdateProject(Project);

            // Record activity for this user
            try
            {
                var activity = new Activity
                {
                    UserId = userId,
                    Type = existingEvaluation != null ? "evaluation_updated" : "evaluation_completed",
                    ProjectTitle = Project.Title,
                    Description = existingEvaluation != null
                        ? $"Actualizaste la evaluación de {Project.Title}"
                        : $"Evaluaste {Project.Title} ({TotalScore}/70)",
                    Timestamp = DateTime.UtcNow,
                    Icon = "check_circle",
                    IconColor = "#10B981"
                };
                await _mongoDBService.CreateActivity(activity);
            }
            catch { /* Activity recording is non-critical */ }
        
            // 3. Navigate to Confirmation
            var navigationParameter = new Dictionary<string, object>
            {
                { "ProjectTitle", Project.Title },
                { "ProjectImageUrl", Project.ImageUrl },
                { "TotalScore", Project.Score } 
                // We could pass breakdown if the result page supports it, but for now Total is key
            };
            
            // Note: EvaluationResultPage might need updates to show new score breakdown if it was detailed
            await Shell.Current.GoToAsync(nameof(EvaluationResultPage), navigationParameter);
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error", $"No se pudo guardar la evaluación: {ex.Message}", "OK");
        }
    }
}
