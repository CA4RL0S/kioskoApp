using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class MainPage : ContentPage
{
    private readonly IMongoDBService _dbService;
    private List<Project> _allProjects = new();
    private string _userId = string.Empty;

    public MainPage()
    {
        InitializeComponent();
        _dbService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
            .GetRequiredService<IMongoDBService>(
                IPlatformApplication.Current!.Services);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);

        LoadUserInfo();
        await LoadDataAsync();
    }

    private void LoadUserInfo()
    {
        _userId = Preferences.Get("UserId", string.Empty);
        string fullName = Preferences.Get("UserFullName", "Evaluador");
        string profileImage = Preferences.Get("UserProfileImage", string.Empty);

        UserNameLabel.Text = fullName;

        if (!string.IsNullOrEmpty(profileImage))
            ProfileImage.Source = profileImage;
        else
            ProfileImage.Source = "dotnet_bot.png"; // fallback
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load projects
            _allProjects = await _dbService.GetProjects();

            // Personalize statuses for this evaluator
            foreach (var p in _allProjects)
                p.UpdatePersonalizedStatus(_userId);

            // Compute stats
            int evaluatedCount = _allProjects.Count(p => p.IsEvaluated);
            int pendingCount = _allProjects.Count(p => p.IsPending);
            int totalCount = _allProjects.Count;

            EvaluatedCountLabel.Text = evaluatedCount.ToString("D2");
            PendingCountLabel.Text = pendingCount.ToString("D2");

            // Percentage
            if (totalCount > 0)
            {
                int percent = (int)((double)evaluatedCount / totalCount * 100);
                EvalPercentLabel.Text = $"{percent}%";
            }

            // Build featured projects (top scored)
            BuildFeaturedProjects();

            // Build pending evaluations list
            BuildPendingEvaluations();

            // Load activities
            await LoadActivitiesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading home data: {ex.Message}");
        }
    }

    private void BuildFeaturedProjects()
    {
        FeaturedProjectsLayout.Children.Clear();

        var featured = _allProjects
            .Where(p => p.IsEvaluated && p.ScoreValue > 0)
            .OrderByDescending(p => p.ScoreValue)
            .Take(5)
            .ToList();

        // If no featured, show all with images
        if (!featured.Any())
            featured = _allProjects.Where(p => !string.IsNullOrEmpty(p.ImageUrl)).Take(5).ToList();

        foreach (var project in featured)
        {
            var card = CreateFeaturedCard(project);
            FeaturedProjectsLayout.Children.Add(card);
        }
    }

    private View CreateFeaturedCard(Project project)
    {
        var border = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            WidthRequest = 220,
            BackgroundColor = Microsoft.Maui.Controls.Application.Current!.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1e293b") : Color.FromArgb("#f1f5f9"),
        };
        border.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#f1f5f9"), Color.FromArgb("#1e293b"));

        var grid = new Grid { RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition(new GridLength(120)),
            new RowDefinition(GridLength.Auto)
        }};

        // Image section
        var imageBorder = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                { CornerRadius = new CornerRadius(16, 16, 0, 0) },
            HeightRequest = 120
        };

        var img = new Image
        {
            Source = project.ImageUrl,
            Aspect = Aspect.AspectFill,
            HeightRequest = 120
        };
        imageBorder.Content = img;
        grid.SetRow(imageBorder, 0);
        grid.Children.Add(imageBorder);

        // Score badge
        if (project.IsEvaluated && project.ScoreValue > 0)
        {
            var badgeBorder = new Border
            {
                StrokeThickness = 0,
                Padding = new Thickness(8, 4),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 8, 8, 0),
                ZIndex = 10
            };
            badgeBorder.SetAppThemeColor(Border.BackgroundColorProperty,
                Color.FromArgb("#E6FFFFFF"), Color.FromArgb("#E60f172a"));

            var badgeLayout = new HorizontalStackLayout { Spacing = 4 };
            badgeLayout.Children.Add(new Label
            {
                Text = "star",
                FontFamily = "MaterialIcons",
                FontSize = 12,
                TextColor = Color.FromArgb("#FBBF24"),
                VerticalOptions = LayoutOptions.Center
            });
            var scoreLabel = new Label
            {
                Text = project.Score,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };
            scoreLabel.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);
            badgeLayout.Children.Add(scoreLabel);

            badgeBorder.Content = badgeLayout;
            grid.SetRow(badgeBorder, 0);
            grid.Children.Add(badgeBorder);
        }

        // Text section
        var textLayout = new VerticalStackLayout { Padding = new Thickness(12), Spacing = 4 };
        var titleLabel = new Label
        {
            Text = project.Title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        titleLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#111318"), Colors.White);
        textLayout.Children.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = project.Cycle,
            FontSize = 10
        };
        subtitleLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#616f89"), Color.FromArgb("#9ca3af"));
        textLayout.Children.Add(subtitleLabel);

        grid.SetRow(textLayout, 1);
        grid.Children.Add(textLayout);

        border.Content = grid;

        // Tap to navigate
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await Shell.Current.GoToAsync($"ProjectDetailsPage?projectId={project.Id}");
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private void BuildPendingEvaluations()
    {
        PendingEvaluationsLayout.Children.Clear();

        var pending = _allProjects
            .Where(p => p.IsPending)
            .Take(5)
            .ToList();

        foreach (var project in pending)
        {
            var card = CreatePendingCard(project);
            PendingEvaluationsLayout.Children.Add(card);
        }
    }

    private View CreatePendingCard(Project project)
    {
        var border = new Border
        {
            StrokeThickness = 1,
            Padding = new Thickness(16),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }
        };
        border.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#f1f5f9"), Color.FromArgb("#1e293b80"));
        border.SetAppThemeColor(Border.StrokeProperty,
            Color.FromArgb("#e2e8f0"), Color.FromArgb("#33475170"));

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(48)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 16
        };

        // Icon
        var iconBorder = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#1A135bec"),
            HeightRequest = 48,
            WidthRequest = 48,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }
        };
        iconBorder.Content = new Label
        {
            Text = "science",
            FontFamily = "MaterialIcons",
            FontSize = 24,
            TextColor = Color.FromArgb("#135bec"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        grid.SetColumn(iconBorder, 0);
        grid.Children.Add(iconBorder);

        // Text
        var textLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 2 };
        var titleLabel = new Label
        {
            Text = project.Title,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        titleLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#1e293b"), Colors.White);
        textLayout.Children.Add(titleLabel);

        string membersText = project.Members?.Count > 0
            ? $"Equipo: {project.Members.Count} miembros • {project.Cycle}"
            : project.Cycle ?? "";
        var subtitleLabel = new Label
        {
            Text = membersText,
            FontSize = 12
        };
        subtitleLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#616f89"), Color.FromArgb("#9ca3af"));
        textLayout.Children.Add(subtitleLabel);

        grid.SetColumn(textLayout, 1);
        grid.Children.Add(textLayout);

        // Evaluate button
        var evalButton = new Button
        {
            Text = "Evaluar",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Color.FromArgb("#135bec"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Padding = new Thickness(12, 8),
            HeightRequest = 36,
            VerticalOptions = LayoutOptions.Center
        };
        evalButton.Clicked += async (s, e) =>
        {
            await Shell.Current.GoToAsync($"ProjectDetailsPage?projectId={project.Id}");
        };
        grid.SetColumn(evalButton, 2);
        grid.Children.Add(evalButton);

        border.Content = grid;
        return border;
    }

    private async Task LoadActivitiesAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_userId))
            {
                EmptyActivityView.IsVisible = true;
                return;
            }

            var activities = await _dbService.GetActivities(_userId);

            ActivityTimelineLayout.Children.Clear();

            if (activities == null || !activities.Any())
            {
                EmptyActivityView.IsVisible = true;
                return;
            }

            EmptyActivityView.IsVisible = false;

            foreach (var activity in activities.Take(10))
            {
                var item = CreateActivityItem(activity);
                ActivityTimelineLayout.Children.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading activities: {ex.Message}");
            EmptyActivityView.IsVisible = true;
        }
    }

    private View CreateActivityItem(Activity activity)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(new GridLength(40)),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 16,
            Padding = new Thickness(0, 8)
        };

        // Timeline dot
        var dotBorder = new Border
        {
            HeightRequest = 40,
            WidthRequest = 40,
            StrokeThickness = 4,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 }
        };
        dotBorder.SetAppThemeColor(Border.BackgroundColorProperty,
            Color.FromArgb("#e2e8f0"), Color.FromArgb("#1e293b"));
        dotBorder.SetAppThemeColor(Border.StrokeProperty,
            (Color)Microsoft.Maui.Controls.Application.Current!.Resources["BackgroundLight"],
            (Color)Microsoft.Maui.Controls.Application.Current!.Resources["BackgroundDark"]);

        var iconLabel = new Label
        {
            Text = activity.Icon ?? "check_circle",
            FontFamily = "MaterialIcons",
            FontSize = 16,
            TextColor = !string.IsNullOrEmpty(activity.IconColor)
                ? Color.FromArgb(activity.IconColor)
                : Color.FromArgb("#135bec"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        dotBorder.Content = iconLabel;
        grid.SetColumn(dotBorder, 0);
        grid.Children.Add(dotBorder);

        // Text content
        var textLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 2 };

        var descLabel = new Label { FontSize = 13 };
        var formattedString = new FormattedString();

        if (!string.IsNullOrEmpty(activity.Description))
        {
            // Split description to bold the project name
            formattedString.Spans.Add(new Span { Text = activity.Description });
        }
        else
        {
            string verb = activity.Type switch
            {
                "evaluation_completed" => "Evaluaste ",
                "comment_added" => "Comentaste en ",
                _ => "Interactuaste con "
            };
            formattedString.Spans.Add(new Span { Text = verb });
            formattedString.Spans.Add(new Span
            {
                Text = activity.ProjectTitle ?? "un proyecto",
                FontAttributes = FontAttributes.Bold
            });
        }

        descLabel.FormattedText = formattedString;
        descLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#111318"), Colors.White);
        textLayout.Children.Add(descLabel);

        var timeLabel = new Label
        {
            Text = activity.TimeAgo,
            FontSize = 10
        };
        timeLabel.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#616f89"), Color.FromArgb("#9ca3af"));
        textLayout.Children.Add(timeLabel);

        grid.SetColumn(textLayout, 1);
        grid.Children.Add(textLayout);

        return grid;
    }

    private async void OnViewAllProjectsTapped(object sender, EventArgs e)
    {
        // Navigate to the Projects tab
        await Shell.Current.GoToAsync("//ProjectsPage");
    }
}
