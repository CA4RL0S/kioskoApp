using System.Collections.ObjectModel;
using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class RankingPage : ContentPage, System.ComponentModel.INotifyPropertyChanged
{
    private readonly IMongoDBService _mongoDBService;
    
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

            // Clear previous
            Rank1 = null;
            Rank2 = null;
            Rank3 = null;
            RestProjects.Clear();

            int rank = 1;
            foreach (var proj in sortedProjects)
            {
                var rankedProj = new RankedProject(proj, rank);

                if (rank == 1) Rank1 = rankedProj;
                else if (rank == 2) Rank2 = rankedProj;
                else if (rank == 3) Rank3 = rankedProj;
                else RestProjects.Add(rankedProj);

                rank++;
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error loading ranking: {ex.Message}");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage"); // Changed to Main or back to Dashboard
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
         // Handle selection if needed
         if (e.CurrentSelection.FirstOrDefault() is RankedProject selected)
         {
             // Navigation logic here if details page is desired
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

    public bool IsRank1 => Rank == 1;
    public bool IsRank2 => Rank == 2;
    public bool IsRank3 => Rank == 3;
    public bool IsRankOther => Rank > 3;

    public RankedProject(Project project, int rank)
    {
        Project = project;
        Rank = rank;
    }
}
