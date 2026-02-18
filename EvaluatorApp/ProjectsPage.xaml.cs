using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EvaluatorApp.Models;
using EvaluatorApp.Services;
using CommunityToolkit.Maui.Alerts;


namespace EvaluatorApp;

public partial class ProjectsPage : ContentPage, INotifyPropertyChanged
{
    private readonly ProjectRepository _repository;
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

    private bool _isOffline;
    public bool IsOffline
    {
        get => _isOffline;
        set
        {
            if (_isOffline != value)
            {
                _isOffline = value;
                OnPropertyChanged();
            }
        }
    }

	public ProjectsPage(ProjectRepository repository)
	{
		InitializeComponent();
        _repository = repository;
		Projects = new ObservableCollection<Project>();
        
        RefreshCommand = new Command(async () =>
        {
            IsRefreshing = true;
            await LoadProjects();
            // Try to sync if online
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await _repository.SyncPendingEvaluations();
            }
            IsRefreshing = false;
        });

		BindingContext = this;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);
        
        // Listen to connectivity changes
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        CheckConnectivity();
        
        await LoadProjects();
        
        // Auto-sync on appear if online
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
             _ = _repository.SyncPendingEvaluations(); // Fire and forget
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        CheckConnectivity();
    }

    private void CheckConnectivity()
    {
        IsOffline = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
    }

    private List<Project> _allProjects = new();
    private string _currentFilter = "All";
    private string _searchText = string.Empty;

    private async Task LoadProjects()
    {
        try 
        {
            var projects = await _repository.GetProjects();
            
            // Personalize status for current user
            string userId = Preferences.Get("UserId", string.Empty);
            foreach (var p in projects)
            {
                p.UpdatePersonalizedStatus(userId);
            }

            _allProjects = projects; // Store full list
            ApplyFilter(_currentFilter); // Apply current filter to update view
            
            _allProjects = projects; // Store full list
            ApplyFilter(_currentFilter); // Apply current filter to update view
            
            // Toast removed in favor of persistent banner
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
