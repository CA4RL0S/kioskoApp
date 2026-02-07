using System.Collections.ObjectModel;
using EvaluatorApp.Models;
using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class RankingPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    public ObservableCollection<RankedProject> RankedProjects { get; set; }

	public RankingPage(IMongoDBService mongoDBService)
	{
		InitializeComponent();
        _mongoDBService = mongoDBService;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
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

            RankedProjects = new ObservableCollection<RankedProject>();
            int rank = 1;
            foreach (var proj in sortedProjects)
            {
                RankedProjects.Add(new RankedProject(proj, rank));
                rank++;
            }

            RankingCollectionView.ItemsSource = RankedProjects;
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Error loading ranking: {ex.Message}");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProjectsPage");
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
         RankingCollectionView.SelectedItem = null;
    }
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
