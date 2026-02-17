using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EvaluatorApp.Models;
using EvaluatorApp.Services;


namespace EvaluatorApp;

public partial class ProjectsPage : ContentPage, INotifyPropertyChanged
{
    private readonly IMongoDBService _mongoDBService;
	public ObservableCollection<Project> Projects { get; set; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing != value)
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RefreshCommand { get; private set; }

	public ProjectsPage(IMongoDBService mongoDBService)
	{
		InitializeComponent();
        _mongoDBService = mongoDBService;
		Projects = new ObservableCollection<Project>();
        
        RefreshCommand = new Command(async () =>
        {
            IsRefreshing = true;
            await LoadProjects();
            IsRefreshing = false;
        });

		BindingContext = this;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);
        
        // Only load if empty to act as initial load, otherwise refresh command handles it
        // Or we can just load silently. Let's load silently to ensure up to date.
        // We do NOT set IsRefreshing here to avoid spinner on every tab switch
        await LoadProjects();
    }

    private List<Project> _allProjects = new();
    private string _currentFilter = "All";
    private string _searchText = string.Empty;

    private async Task LoadProjects()
    {
        try 
        {
            var projects = await _mongoDBService.GetProjects();
            
            // Personalize status for current user
            string userId = Preferences.Get("UserId", string.Empty);
            foreach (var p in projects)
            {
                p.UpdatePersonalizedStatus(userId);
            }

            _allProjects = projects; // Store full list
            ApplyFilter(_currentFilter); // Apply current filter to update view
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los proyectos: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
        }
        finally
        {
            // Ensure refreshing spinner stops if it was started by command
            if (IsRefreshing) IsRefreshing = false;
        }
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string mainFilter)
        {
             _currentFilter = mainFilter;
             ApplyFilter(mainFilter);
             UpdateFilterVisuals(btn);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        ApplyFilter(_currentFilter);
    }

    private string _emptyStatusText = "No se encontraron proyectos";
    public string EmptyStatusText
    {
        get => _emptyStatusText;
        set
        {
            if (_emptyStatusText != value)
            {
                _emptyStatusText = value;
                OnPropertyChanged();
            }
        }
    }

    private void ApplyFilter(string filter)
    {
        Projects.Clear();
        IEnumerable<Project> filtered = _allProjects;

        switch (filter)
        {
            case "Pending":
                filtered = _allProjects.Where(p => p.IsPending);
                EmptyStatusText = "No hay pendientes";
                break;
            case "Evaluated":
                filtered = _allProjects.Where(p => p.IsEvaluated);
                EmptyStatusText = "No hay proyectos evaluados";
                break;
            case "2024-A":
                filtered = _allProjects.Where(p => p.Cycle == "Ciclo 2024-A");
                EmptyStatusText = "No hay proyectos del ciclo 2024-A";
                break;
            case "2023-B":
                filtered = _allProjects.Where(p => p.Cycle == "Ciclo 2023-B");
                EmptyStatusText = "No hay proyectos del ciclo 2023-B";
                break;
            case "All":
            default:
                filtered = _allProjects;
                EmptyStatusText = "No se encontraron proyectos";
                break;
        }

        foreach (var p in filtered)
        {
            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var search = _searchText.ToLowerInvariant();
                bool matches = (p.Title?.ToLowerInvariant().Contains(search) == true) ||
                               (p.Description?.ToLowerInvariant().Contains(search) == true) ||
                               (p.Members?.Any(m => m.ToLowerInvariant().Contains(search)) == true);
                if (!matches) continue;
            }
            Projects.Add(p);
        }
    }

    private void UpdateFilterVisuals(Button selectedBtn)
    {
        // Reset all
        ResetButtonVisual(BtnAll);
        ResetButtonVisual(BtnPending);
        ResetButtonVisual(BtnEvaluated);
        ResetButtonVisual(Btn2024A);
        ResetButtonVisual(Btn2023B);

        // Highlight selected
        selectedBtn.BackgroundColor = (Color)Application.Current.Resources["Primary"];
        selectedBtn.TextColor = Colors.White;
        selectedBtn.BorderWidth = 0;
        selectedBtn.FontAttributes = FontAttributes.Bold;
    }

    private void ResetButtonVisual(Button btn)
    {
        bool isDark = Microsoft.Maui.Controls.Application.Current!.UserAppTheme == AppTheme.Dark;
        btn.BackgroundColor = isDark 
            ? (Color)Microsoft.Maui.Controls.Application.Current.Resources["SurfaceDark"] 
            : Colors.White;
        btn.TextColor = isDark 
            ? Colors.White 
            : (Color)Microsoft.Maui.Controls.Application.Current.Resources["TextPrimary"];
        btn.BorderColor = isDark 
            ? (Color)Microsoft.Maui.Controls.Application.Current.Resources["Gray600"] 
            : (Color)Microsoft.Maui.Controls.Application.Current.Resources["Gray200"];
        btn.BorderWidth = 1;
        btn.FontAttributes = FontAttributes.None;
    }

    private async void OnProjectTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Project selectedProject)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Project", selectedProject }
            };
            await Shell.Current.GoToAsync(nameof(ProjectDetailsPage), navigationParameter);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
