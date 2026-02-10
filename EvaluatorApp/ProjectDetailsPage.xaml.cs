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
                     InnovationScore = myEval.InnovationScore;
                     TechScore = myEval.TechScore;
                     Comments = myEval.Comments;

                     int presentationScore = myEval.PresentationScore;
                     int index = presentationScore switch
                     {
                         2 => 1,
                         5 => 2,
                         8 => 3,
                         10 => 4,
                         _ => 3
                     };
                     UpdatePresentationSelection(index);
                 }
                 else
                 {
                     // Default values for new evaluation
                     InnovationScore = 8;
                     TechScore = 6;
                     Comments = string.Empty;
                     UpdatePresentationSelection(3); // Good
                 }
            }
        }
    }

    private double _innovationScore = 8;
    private double _techScore = 6;
    private int _presentationScore = 7; // Default "Good" mapped value
    private int _selectedPresentationIndex = 3; 

    public double InnovationScore
    {
        get => _innovationScore;
        set
        {
            if (_innovationScore != value)
            {
                _innovationScore = Math.Round(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalScore));
            }
        }
    }

    public double TechScore
    {
        get => _techScore;
        set
        {
            if (_techScore != value)
            {
                _techScore = Math.Round(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalScore));
            }
        }
    }

    public double TotalScore => InnovationScore + TechScore + _presentationScore;

    public System.Windows.Input.ICommand SelectPresentationCommand { get; private set; }

    public ProjectDetailsPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        
        SelectPresentationCommand = new Command<string>((param) =>
        {
             if (int.TryParse(param, out int index))
             {
                 UpdatePresentationSelection(index);
             }
        });

        BindingContext = this;
    }




    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void UpdatePresentationSelection(int index)
    {
        _selectedPresentationIndex = index;

        _presentationScore = index switch
        {
            1 => 2,   // Poor
            2 => 5,   // Fair
            3 => 8,   // Good
            4 => 10,  // Great
            _ => 0
        };

        OnPropertyChanged(nameof(TotalScore));

        UpdateOptionVisualState(OptionPoor, LabelPoor, index == 1);
        UpdateOptionVisualState(OptionFair, LabelFair, index == 2);
        UpdateOptionVisualState(OptionGood, LabelGood, index == 3, ShadowGood);
        UpdateOptionVisualState(OptionGreat, LabelGreat, index == 4);
    }

    private void UpdateOptionVisualState(Border border, Label label, bool isSelected, Shadow? shadow = null)
    {
        if (isSelected)
        {
            border.BackgroundColor = Colors.White;
            border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 };
            label.TextColor = (Color)Application.Current.Resources["Primary"];
            if (shadow != null) shadow.Opacity = 0.05f;
        }
        else
        {
            border.BackgroundColor = Colors.Transparent;
            label.TextColor = (Color)Application.Current.Resources["Gray500"];
            if (shadow != null) shadow.Opacity = 0;
        }
    }
    
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
            InnovationScore = InnovationScore,
            TechScore = TechScore,
            PresentationScore = _presentationScore,
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
            Project.InnovationScore = Math.Round(Project.Evaluations.Average(ev => ev.InnovationScore), 1);
            Project.TechScore = Math.Round(Project.Evaluations.Average(ev => ev.TechScore), 1);
            
            // For presentation score (int), we can round to nearest int
            Project.PresentationScore = (int)Math.Round(Project.Evaluations.Average(ev => ev.PresentationScore));
            
            Project.Score = Math.Round(Project.Evaluations.Average(ev => ev.TotalScore), 1).ToString();
        }
        else
        {
             // Fallback (shouldn't happen directly after add)
            Project.InnovationScore = InnovationScore;
            Project.TechScore = TechScore;
            Project.PresentationScore = _presentationScore;
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
        
            // 3. Navigate to Confirmation
            var navigationParameter = new Dictionary<string, object>
            {
                { "ProjectTitle", Project.Title },
                { "ProjectImageUrl", Project.ImageUrl },
                { "InnovationScore", Project.InnovationScore.ToString() }, // Show AVERAGE
                { "TechScore", Project.TechScore.ToString() }, // Show AVERAGE
                { "PresentationScore", Project.PresentationScore.ToString() }, // Show AVERAGE
                { "TotalScore", Project.Score } // Show AVERAGE
            };

            await Shell.Current.GoToAsync(nameof(EvaluationResultPage), navigationParameter);
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error", $"No se pudo guardar la evaluación: {ex.Message}", "OK");
        }
    }
}
