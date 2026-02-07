using System.ComponentModel;
using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

[QueryProperty(nameof(Project), "Project")]
public partial class ProjectDetailsPage : ContentPage, INotifyPropertyChanged
{
    private readonly IMongoDBService _mongoDBService;
    
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
                 InnovationScore = _project.InnovationScore > 0 ? _project.InnovationScore : 8; // Default 8 if 0
                 TechScore = _project.TechScore > 0 ? _project.TechScore : 6; // Default 6
                 
                 // Map presentation score to index
                 int presentationScore = _project.PresentationScore > 0 ? _project.PresentationScore : 8; // Default 8 (Good)
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
        if (Project == null) return; // Should not happen

        // 1. Update Model
        Project.InnovationScore = InnovationScore;
        Project.TechScore = TechScore;
        Project.PresentationScore = _presentationScore;
        Project.Score = TotalScore.ToString();
        Project.IsEvaluated = true;
        Project.IsPending = false;
        
        // Update visual properties (though we reconstruct them on load, setting them here makes UI updating smoother if we navigate back)
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
                { "InnovationScore", InnovationScore.ToString() },
                { "TechScore", TechScore.ToString() },
                { "PresentationScore", _presentationScore.ToString() },
                { "TotalScore", TotalScore.ToString() }
            };

            await Shell.Current.GoToAsync(nameof(EvaluationResultPage), navigationParameter);
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error", $"No se pudo guardar la evaluación: {ex.Message}", "OK");
        }
    }
}
