using StudentApp.Models;

namespace StudentApp.Views;

[QueryProperty(nameof(Project), "Project")]
public partial class ProjectDetailsPage : ContentPage
{
    private Project _project;

    public Project Project
    {
        get => _project;
        set
        {
            _project = value;
            LoadProjectDetails();
        }
    }

    public ProjectDetailsPage()
    {
        InitializeComponent();
    }

    private void LoadProjectDetails()
    {
        if (_project == null) return;

        // Project header
        ProjectTitle.Text = _project.Title ?? "Proyecto";
        ProjectImage.Source = _project.DisplayImage;

        // Status
        if (_project.IsEvaluated)
        {
            StatusLabel.Text = "Evaluado";
            StatusLabel.TextColor = Color.FromArgb("#15803d");
            StatusBadge.BackgroundColor = Color.FromArgb("#dcfce7");
            StatusBadge.Stroke = Color.FromArgb("#22c55e");
            ScoreOverview.Text = $"Promedio: {_project.Score ?? "0"}/70";
        }
        else if (_project.IsPending)
        {
            StatusLabel.Text = "En revisión";
            StatusLabel.TextColor = Color.FromArgb("#854d0e");
            StatusBadge.BackgroundColor = Color.FromArgb("#fefce8");
            StatusBadge.Stroke = Color.FromArgb("#eab308");
            ScoreOverview.Text = "";
        }
        else
        {
            StatusLabel.Text = "Activo";
            StatusLabel.TextColor = Color.FromArgb("#1e40af");
            StatusBadge.BackgroundColor = Color.FromArgb("#dbeafe");
            StatusBadge.Stroke = Color.FromArgb("#197fe6");
            ScoreOverview.Text = "";
        }

        // Evaluator cards
        EvaluatorsList.Children.Clear();

        if (_project.Evaluations != null && _project.Evaluations.Count > 0)
        {
            NoEvalSection.IsVisible = false;
            EvaluatorsTitle.Text = $"Evaluaciones Recibidas ({_project.Evaluations.Count})";

            foreach (var eval in _project.Evaluations)
            {
                EvaluatorsList.Add(CreateEvaluatorCard(eval));
            }
        }
        else
        {
            NoEvalSection.IsVisible = true;
            EvaluatorsTitle.IsVisible = false;
        }
    }

    private View CreateEvaluatorCard(Evaluation eval)
    {
        // Avatar
        var avatar = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 28 },
            StrokeThickness = 2,
            Stroke = Color.FromArgb("#e5e7eb"),
            HeightRequest = 56,
            WidthRequest = 56,
            Content = new Label
            {
                Text = GetInitials(eval.EvaluatorName),
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            },
            BackgroundColor = Color.FromArgb("#197fe6")
        };

        // Name + date
        var nameLabel = new Label
        {
            Text = eval.EvaluatorName ?? "Evaluador",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111827")
        };

        var dateLabel = new Label
        {
            Text = eval.Timestamp != default ? eval.Timestamp.ToString("dd MMM yyyy") : "",
            FontSize = 12,
            TextColor = Color.FromArgb("#9ca3af")
        };

        // Score
        var scoreLabel = new Label
        {
            Text = $"{eval.TotalScore:0.#}/70",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#197fe6"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End
        };

        // Header row: avatar + name/date + score + arrow
        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(56)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(new GridLength(24))
            },
            ColumnSpacing = 12
        };

        headerGrid.Add(avatar, 0);
        var nameStack = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 2,
            Children = { nameLabel, dateLabel }
        };
        headerGrid.Add(nameStack, 1);
        headerGrid.Add(scoreLabel, 2);
        headerGrid.Add(new Label 
        { 
            Text = ">", 
            FontSize = 20, 
            FontAttributes = FontAttributes.Bold, 
            TextColor = Color.FromArgb("#9ca3af"), 
            VerticalOptions = LayoutOptions.Center, 
            HorizontalOptions = LayoutOptions.Center 
        }, 3);

        // Comment
        var cardContent = new VerticalStackLayout { Spacing = 12 };
        cardContent.Add(headerGrid);

        if (!string.IsNullOrEmpty(eval.Comments))
        {
            cardContent.Add(new Label
            {
                Text = $"\"{eval.Comments}\"",
                FontSize = 14,
                TextColor = Color.FromArgb("#374151"),
                FontAttributes = FontAttributes.Italic,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
        }

        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#e5e7eb"),
            StrokeThickness = 1,
            Padding = new Thickness(16),
            Content = cardContent,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.05f, Offset = new Point(0, 4), Radius = 16 }
        };

        // Make entire card tappable
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await Shell.Current.GoToAsync(nameof(EvaluationDetailPage), new Dictionary<string, object>
            {
                { "Evaluation", eval },
                { "ProjectTitle", _project.Title ?? "Proyecto" }
            });
        };
        card.GestureRecognizers.Add(tapGesture);

        return card;
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
