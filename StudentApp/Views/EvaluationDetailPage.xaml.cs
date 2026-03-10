using StudentApp.Models;

namespace StudentApp.Views;

[QueryProperty(nameof(Evaluation), "Evaluation")]
[QueryProperty(nameof(ProjectTitle), "ProjectTitle")]
public partial class EvaluationDetailPage : ContentPage
{
    private Evaluation _evaluation;
    private string _projectTitle;

    public Evaluation Evaluation
    {
        get => _evaluation;
        set
        {
            _evaluation = value;
            LoadDetails();
        }
    }

    public string ProjectTitle
    {
        get => _projectTitle;
        set => _projectTitle = value;
    }

    public EvaluationDetailPage()
    {
        InitializeComponent();
    }

    private void LoadDetails()
    {
        if (_evaluation == null) return;

        // Header
        EvaluatorName.Text = _evaluation.EvaluatorName ?? "Evaluador";
        AvatarInitials.Text = GetInitials(_evaluation.EvaluatorName);
        EvaluationDate.Text = _evaluation.Timestamp != default 
            ? _evaluation.Timestamp.ToString("dd MMM yyyy, hh:mm tt") 
            : "";

        // Total score
        TotalScoreLabel.Text = $"{_evaluation.TotalScore:0.#}";

        // Score breakdown
        ScoreItemsList.Children.Clear();

        var scoreItems = new[]
        {
            ("Planteamiento del Problema", _evaluation.ProblemScore, 10.0),
            ("Innovación", _evaluation.InnovationScore, 10.0),
            ("Desarrollo Técnico", _evaluation.TechScore, 10.0),
            ("Impacto", _evaluation.ImpactScore, 10.0),
            ("Presentación", _evaluation.PresentationScore, 10.0),
            ("Conocimiento", _evaluation.KnowledgeScore, 10.0),
            ("Resultados", _evaluation.ResultsScore, 10.0),
        };

        foreach (var (label, score, max) in scoreItems)
        {
            ScoreItemsList.Add(CreateScoreItem(label, score, max));
        }

        // Comment
        if (!string.IsNullOrEmpty(_evaluation.Comments))
        {
            CommentSection.IsVisible = true;
            CommentText.Text = _evaluation.Comments;
        }
    }

    private View CreateScoreItem(string label, double score, double max)
    {
        double percentage = max > 0 ? score / max : 0;
        var barColor = percentage >= 0.7 ? Color.FromArgb("#22c55e") : 
                       percentage >= 0.4 ? Color.FromArgb("#eab308") : Color.FromArgb("#ef4444");

        // Background bar
        var bgBar = new Border
        {
            BackgroundColor = Color.FromArgb("#f3f4f6"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 3 },
            HeightRequest = 6,
            HorizontalOptions = LayoutOptions.Fill
        };

        // Fill bar
        var fillBar = new Border
        {
            BackgroundColor = barColor,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 3 },
            HeightRequest = 6,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = percentage * 200
        };

        var barGrid = new Grid { HeightRequest = 6 };
        barGrid.Add(bgBar);
        barGrid.Add(fillBar);

        var labelText = new Label { Text = label, FontSize = 13, TextColor = Color.FromArgb("#374151") };
        var scoreText = new Label 
        { 
            Text = $"{score:0.#}/{max:0}", 
            FontSize = 15, 
            FontAttributes = FontAttributes.Bold, 
            TextColor = Color.FromArgb("#197fe6"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var leftStack = new VerticalStackLayout { Spacing = 6 };
        leftStack.Add(labelText);
        leftStack.Add(barGrid);

        contentGrid.Add(leftStack, 0);
        contentGrid.Add(scoreText, 1);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#f3f4f6"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(16, 12),
            Content = contentGrid
        };
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return parts[0][0].ToString().ToUpper();
    }
}
