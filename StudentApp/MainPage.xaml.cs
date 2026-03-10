using StudentApp.Services;
using StudentApp.Models;
using StudentApp.Views;

namespace StudentApp;

public partial class MainPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    
    public static string CurrentStudentEmail { get; set; }
    public static string CurrentStudentName { get; set; }

	public MainPage(IMongoDBService mongoDBService)
	{
		InitializeComponent();
        _mongoDBService = mongoDBService;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage LoadData error: {ex}");
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage refresh error: {ex}");
        }
        finally
        {
            MainRefreshView.IsRefreshing = false;
        }
    }

    private async Task LoadData()
    {
        if (string.IsNullOrEmpty(CurrentStudentEmail)) return;

        if (string.IsNullOrEmpty(UserNameLabel.Text))
        {
            UserNameLabel.Text = CurrentStudentName;
            string profileImage = Preferences.Get("StudentProfileImage", string.Empty);
            if (!string.IsNullOrEmpty(profileImage))
                ProfileImage.Source = profileImage;
        }
        
        var student = await _mongoDBService.GetOrCreateStudent(CurrentStudentEmail, CurrentStudentName);
        if (student == null) return;

        var projects = await _mongoDBService.GetProjectsByMatricula(student.Matricula);

        ProjectListView.Children.Clear();

        if (projects == null || projects.Count == 0)
        {
            EmptyState.IsVisible = true;
            return;
        }

        EmptyState.IsVisible = false;

        foreach (var project in projects)
        {
            var card = CreateProjectCard(project);
            ProjectListView.Children.Add(card);
        }
    }

    private View CreateProjectCard(Project project)
    {
        Color statusBg, statusStroke, statusText;
        string statusLabel;

        if (project.IsEvaluated)
        {
            statusBg = Color.FromArgb("#dcfce7");
            statusStroke = Color.FromArgb("#22c55e");
            statusText = Color.FromArgb("#15803d");
            statusLabel = "Evaluado";
        }
        else if (project.IsPending)
        {
            statusBg = Color.FromArgb("#fefce8");
            statusStroke = Color.FromArgb("#eab308");
            statusText = Color.FromArgb("#854d0e");
            statusLabel = "En revisión";
        }
        else
        {
            statusBg = Color.FromArgb("#dbeafe");
            statusStroke = Color.FromArgb("#197fe6");
            statusText = Color.FromArgb("#1e40af");
            statusLabel = "Activo";
        }

        // Project image
        var image = new Image 
        { 
            Source = project.DisplayImage,
            Aspect = Aspect.AspectFill,
            HeightRequest = 160
        };

        var imageBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12, 12, 0, 0) },
            StrokeThickness = 0,
            Padding = 0,
            Content = image
        };

        // Status badge
        var badge = new Border
        {
            BackgroundColor = statusBg,
            Stroke = statusStroke,
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 4),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label { Text = statusLabel, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = statusText }
        };

        var title = new Label { Text = project.Title ?? "Proyecto", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#111827") };

        var scoreRow = new HorizontalStackLayout { Spacing = 16 };
        
        if (project.IsEvaluated)
        {
            scoreRow.Add(new HorizontalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = "⭐", FontSize = 14 },
                    new Label { Text = project.DisplayScore, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#197fe6"), VerticalOptions = LayoutOptions.Center }
                }
            });
        }

        if (!string.IsNullOrEmpty(project.EvaluationDate))
        {
            scoreRow.Add(new HorizontalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = "📅", FontSize = 14 },
                    new Label { Text = project.EvaluationDate, FontSize = 13, TextColor = Color.FromArgb("#6b7280"), VerticalOptions = LayoutOptions.Center }
                }
            });
        }

        if (!string.IsNullOrEmpty(project.Cycle))
        {
            scoreRow.Add(new HorizontalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = "📋", FontSize = 14 },
                    new Label { Text = project.Cycle, FontSize = 13, TextColor = Color.FromArgb("#6b7280"), VerticalOptions = LayoutOptions.Center }
                }
            });
        }

        var content = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { badge, title }
        };

        if (scoreRow.Children.Count > 0)
        {
            content.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#f3f4f6"), Margin = new Thickness(0, 4) });
            content.Add(scoreRow);
        }

        var cardBody = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Children = { content }
        };

        var cardStack = new VerticalStackLayout
        {
            Children = { imageBorder, cardBody }
        };

        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#e5e7eb"),
            StrokeThickness = 1,
            Padding = 0,
            Content = cardStack,
            Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.05f, Offset = new Point(0, 4), Radius = 20 }
        };

        // Tap the whole card to navigate
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await Shell.Current.GoToAsync(nameof(ProjectDetailsPage), new Dictionary<string, object>
            {
                { "Project", project }
            });
        };
        card.GestureRecognizers.Add(tapGesture);

        return card;
    }
}
