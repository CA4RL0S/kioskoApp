using System.Collections.ObjectModel;
using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class RankingPage : ContentPage, System.ComponentModel.INotifyPropertyChanged
{
    private readonly IMongoDBService _mongoDBService;
    private const string RankingPrefsKey = "PreviousRanking";
    private List<RankedProject> _allRanked = new();
    private string _currentRankingFilter = "All";
    
    private RankedProject _rank1;
    public RankedProject Rank1
    {
        get => _rank1;
        set
        {
            _rank1 = value;
            OnPropertyChanged(nameof(Rank1));
            OnPropertyChanged(nameof(HasRank1));
        }
    }

    private RankedProject _rank2;
    public RankedProject Rank2
    {
        get => _rank2;
        set
        {
            _rank2 = value;
            OnPropertyChanged(nameof(Rank2));
            OnPropertyChanged(nameof(HasRank2));
        }
    }

    private RankedProject _rank3;
    public RankedProject Rank3
    {
        get => _rank3;
        set
        {
            _rank3 = value;
            OnPropertyChanged(nameof(Rank3));
            OnPropertyChanged(nameof(HasRank3));
        }
    }

    public bool HasRank1 => Rank1 != null;
    public bool HasRank2 => Rank2 != null;
    public bool HasRank3 => Rank3 != null;

    public ObservableCollection<RankedProject> RestProjects { get; set; } = new ObservableCollection<RankedProject>();

    public RankingPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);
        await LoadRanking();
    }

    private async Task LoadRanking()
    {
        try
        {
            var allProjects = await _mongoDBService.GetProjects();

            var sortedProjects = allProjects
                .Where(p => p.IsEvaluated)
                .OrderByDescending(p => p.ScoreValue)
                .ToList();

            // Load previous ranking from Preferences
            var previousRanking = LoadPreviousRanking();

            _allRanked = new List<RankedProject>();
            int rank = 1;
            var currentRanking = new Dictionary<string, int>();

            foreach (var proj in sortedProjects)
            {
                int previousRank = previousRanking.ContainsKey(proj.Id) ? previousRanking[proj.Id] : -1;
                var rankedProj = new RankedProject(proj, rank, previousRank);
                currentRanking[proj.Id] = rank;
                _allRanked.Add(rankedProj);
                rank++;
            }

            SaveCurrentRanking(currentRanking);
            RebuildRankingDisplay(_currentRankingFilter);
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error loading ranking: {ex.Message}");
        }
    }

    private void RebuildRankingDisplay(string filter)
    {
        IEnumerable<RankedProject> source = filter switch
        {
            "Juego" => _allRanked.Where(r => r.Project.ProjectType == "Juego"),
            "Proyecto" => _allRanked.Where(r => r.Project.ProjectType == "Proyecto" || string.IsNullOrEmpty(r.Project.ProjectType)),
            _ => _allRanked
        };

        var filtered = source.ToList();

        // Re-rank within the filtered subset
        Rank1 = null;
        Rank2 = null;
        Rank3 = null;
        RestProjects.Clear();

        int displayRank = 1;
        foreach (var item in filtered)
        {
            var reranked = new RankedProject(item.Project, displayRank, item.PreviousRank);
            if (displayRank == 1) Rank1 = reranked;
            else if (displayRank == 2) Rank2 = reranked;
            else if (displayRank == 3) Rank3 = reranked;
            else RestProjects.Add(reranked);
            displayRank++;
        }
    }

    private void OnRankingFilterClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string filter)
        {
            _currentRankingFilter = filter;
            RebuildRankingDisplay(filter);
            UpdateRankingChipVisuals(btn);
        }
    }

    private void UpdateRankingChipVisuals(Button selected)
    {
        ResetRankingChip(BtnRankAll);
        ResetRankingChip(BtnRankJuego);
        ResetRankingChip(BtnRankProyecto);

        selected.BackgroundColor = (Color)Application.Current!.Resources["Primary"];
        selected.TextColor = Colors.White;
        selected.BorderWidth = 0;
        selected.FontAttributes = FontAttributes.Bold;
    }

    private void ResetRankingChip(Button btn)
    {
        bool isDark = Application.Current!.UserAppTheme == AppTheme.Dark;
        btn.BackgroundColor = isDark
            ? (Color)Application.Current.Resources["Gray900"]
            : Colors.White;
        btn.TextColor = isDark
            ? (Color)Application.Current.Resources["Gray300"]
            : (Color)Application.Current.Resources["Gray500"];
        btn.BorderColor = isDark
            ? (Color)Application.Current.Resources["Gray600"]
            : (Color)Application.Current.Resources["Gray200"];
        btn.BorderWidth = 1;
        btn.FontAttributes = FontAttributes.None;
    }

    private Dictionary<string, int> LoadPreviousRanking()
    {
        var result = new Dictionary<string, int>();
        try
        {
            var json = Preferences.Get(RankingPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                var pairs = json.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int r))
                    {
                        result[parts[0]] = r;
                    }
                }
            }
        }
        catch { /* ignore parsing errors */ }
        return result;
    }

    private void SaveCurrentRanking(Dictionary<string, int> ranking)
    {
        var pairs = ranking.Select(kvp => $"{kvp.Key}:{kvp.Value}");
        Preferences.Set(RankingPrefsKey, string.Join(";", pairs));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
         if (e.CurrentSelection.FirstOrDefault() is RankedProject selected)
         {
         }
         ((CollectionView)sender).SelectedItem = null;
    }

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
}

public class RankedProject
{
    public Project Project { get; private set; }
    public int Rank { get; private set; }
    public int PreviousRank { get; private set; }

    public bool IsRank1 => Rank == 1;
    public bool IsRank2 => Rank == 2;
    public bool IsRank3 => Rank == 3;
    public bool IsRankOther => Rank > 3;

    // Rank change: positive = went UP, negative = went DOWN, 0 = same
    public int RankChange { get; private set; }

    // Display properties
    public string RankChangeIcon => RankChange > 0 ? "arrow_drop_up" : RankChange < 0 ? "arrow_drop_down" : "remove";
    public Color RankChangeColor => RankChange > 0 ? Color.FromArgb("#10B981") : RankChange < 0 ? Color.FromArgb("#EF4444") : Color.FromArgb("#6B7280");
    public string RankChangeText => RankChange == 0 ? "—" : Math.Abs(RankChange).ToString();
    public bool IsNewEntry { get; private set; }
    public bool HasRankChange => !IsNewEntry;

    public RankedProject(Project project, int rank, int previousRank = -1)
    {
        Project = project;
        Rank = rank;
        PreviousRank = previousRank;

        if (previousRank < 0)
        {
            // New entry — no previous data
            IsNewEntry = true;
            RankChange = 0;
        }
        else
        {
            IsNewEntry = false;
            // If previousRank was 5 and now is 3, that's going UP by 2 (positive)
            RankChange = previousRank - rank;
        }
    }
}
