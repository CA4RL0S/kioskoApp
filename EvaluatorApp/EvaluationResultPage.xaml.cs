namespace EvaluatorApp;

[QueryProperty(nameof(ProjectTitle), "ProjectTitle")]
[QueryProperty(nameof(ProjectImageUrl), "ProjectImageUrl")]
[QueryProperty(nameof(InnovationScore), "InnovationScore")]
[QueryProperty(nameof(TechScore), "TechScore")]
[QueryProperty(nameof(PresentationScore), "PresentationScore")]
[QueryProperty(nameof(TotalScore), "TotalScore")]
public partial class EvaluationResultPage : ContentPage
{
    private string? projectTitle;
    public string? ProjectTitle
    {
        get => projectTitle;
        set
        {
            projectTitle = value;
            if (value != null) ProjectTitleLabel.Text = value;
        }
    }

    private string? projectImageUrl;
    public string? ProjectImageUrl
    {
        get => projectImageUrl;
        set
        {
            projectImageUrl = value;
            if (value != null) ProjectImage.Source = value;
        }
    }

    private string? innovationScore;
    public string? InnovationScore
    {
        get => innovationScore;
        set { innovationScore = value; if (value != null) InnovationScoreLabel.Text = value; }
    }

    private string? techScore;
    public string? TechScore
    {
        get => techScore;
        set { techScore = value; if (value != null) ExecutionScoreLabel.Text = value; }
    }

    private string? presentationScore;
    public string? PresentationScore
    {
        get => presentationScore;
        set { presentationScore = value; if (value != null) PresentationScoreLabel.Text = value; }
    }

    private string? totalScore;
    public string? TotalScore
    {
        get => totalScore;
        set 
        { 
            totalScore = value; 
            if (value != null) {
                TotalScoreLabel.Text = $"{value}/30";
                TotalScoreSubLabel.Text = value;
            }
        }
    }

	public EvaluationResultPage()
	{
		InitializeComponent();
	}

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProjectsPage");
    }

    private async void OnRankingClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RankingPage");
    }

    private async void OnProjectsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProjectsPage");
    }
}
