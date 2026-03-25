using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EvaluatorApp.Components;
using EvaluatorApp.Models;
using EvaluatorApp.Services;
using CommunityToolkit.Maui.Alerts;


namespace EvaluatorApp;

public partial class ProjectsPage : ContentPage, INotifyPropertyChanged
{
    private readonly ProjectRepository _repository;
    private ObservableCollection<Project> _projects = new();
    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set
        {
            if (_projects != value)
            {
                _projects = value;
                OnPropertyChanged();
            }
        }
    }

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

        // Trigger initial auto-play: play the first video in the list
        _initialAutoPlayDone = false;
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(800), () => TriggerInitialAutoPlay());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        PauseAllVideos();
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
            case "Juego":
                filtered = _allProjects.Where(p => p.ProjectType == "Juego");
                EmptyStatusText = "No hay juegos registrados";
                break;
            case "Proyecto":
                filtered = _allProjects.Where(p => p.ProjectType == "Proyecto" || string.IsNullOrEmpty(p.ProjectType));
                EmptyStatusText = "No hay proyectos integradores";
                break;
            case "All":
            default:
                filtered = _allProjects;
                EmptyStatusText = "No se encontraron proyectos";
                break;
        }

        var newProjects = new List<Project>();
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
            newProjects.Add(p);
        }

        Projects = new ObservableCollection<Project>(newProjects);
    }

    private void UpdateFilterVisuals(Button selectedBtn)
    {
        // Reset all
        ResetButtonVisual(BtnAll);
        ResetButtonVisual(BtnPending);
        ResetButtonVisual(BtnEvaluated);
        ResetButtonVisual(BtnJuego);
        ResetButtonVisual(BtnProyecto);

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

    // --- Auto-Play on Scroll Logic ---
    private ProjectCard _currentlyPlayingCard;
    private bool _initialAutoPlayDone = false;

    private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
    {
        UpdateAutoPlay();
    }

    private void TriggerInitialAutoPlay()
    {
        // On page load: play the first video in the list regardless of position
        _initialAutoPlayDone = true;

        foreach (var card in ProjectCard.ActiveCards)
        {
            if (!card.HasVideo) continue;
            
            card.PlayVideo();
            _currentlyPlayingCard = card;
            return; // Only play the first one
        }
    }

    private void UpdateAutoPlay()
    {
        try
        {
            // Get the viewport's visible area in screen coordinates
            var viewportHeight = ProjectsScrollView.Height;
            if (viewportHeight <= 0) return;

            // Find the 35% vertical mark of the screen (slightly above center)
            var viewportCenter = viewportHeight * 0.35;
            
            // Only cards whose center is within this pixels distance of the 35% mark can play
            // Shrunk from 0.35 to 0.20 to prevent videos from playing when they are completely out of the center area,
            // even if they are the closest *video* available.
            var maxDistanceAllowed = viewportHeight * 0.20;

            ProjectCard bestCard = null;
            double bestDistance = double.MaxValue;

            foreach (var card in ProjectCard.ActiveCards)
            {
                if (!card.HasVideo) continue;

                // Get card's position relative to the ScrollView (screen-relative)
                var cardBounds = GetCardBoundsInScrollView(card);
                if (cardBounds == null) continue;

                // Calculate the exact center of this card
                double cardCenter = cardBounds.Value.top + (card.CardHeight / 2.0);

                // Calculate distance from card's center to screen's center
                double distanceToCenter = Math.Abs(viewportCenter - cardCenter);

                // The card MUST be near the middle, and it must be the closest one found so far
                if (distanceToCenter < maxDistanceAllowed && distanceToCenter < bestDistance)
                {
                    bestDistance = distanceToCenter;
                    bestCard = card;
                }
            }

            if (bestCard != null && bestCard != _currentlyPlayingCard)
            {
                _currentlyPlayingCard?.PauseVideo();
                bestCard.PlayVideo();
                _currentlyPlayingCard = bestCard;
            }
            else if (bestCard == null && _currentlyPlayingCard != null)
            {
                _currentlyPlayingCard.PauseVideo();
                _currentlyPlayingCard = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoPlay] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the card's top/bottom position relative to the ScrollView's visible area.
    /// </summary>
    private (double top, double bottom)? GetCardBoundsInScrollView(ProjectCard card)
    {
        try
        {
            // Walk up from card to ScrollView, accumulating Y offset
            double yInContent = 0;
            var current = card as VisualElement;

            while (current != null && current != ProjectsScrollView)
            {
                yInContent += current.Y;
                current = current.Parent as VisualElement;
            }

            if (current == null) return null;

            // Convert content-Y to viewport-Y by subtracting scroll offset
            double topInViewport = yInContent - ProjectsScrollView.ScrollY;
            double bottomInViewport = topInViewport + card.Height;

            return (topInViewport, bottomInViewport);
        }
        catch
        {
            return null;
        }
    }

    private void PauseAllVideos()
    {
        try
        {
            foreach (var card in ProjectCard.ActiveCards)
            {
                card.PauseVideo();
            }
            _currentlyPlayingCard = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoPlay] PauseAll error: {ex.Message}");
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
