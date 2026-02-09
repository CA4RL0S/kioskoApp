using StudentApp.Services;
using StudentApp.Models;
using StudentApp.Views;

namespace StudentApp;

public partial class MainPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    
    // In a real app we'd use a robust session manager
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

        // Check if we have a student logged in (this basic session handling assumes LoginPage set these or we passed them)
        // Since we are using DI, we might need a SessionService. For now, we will rely on a basic check 
        // or just fetch a hardcoded/last login if this was a fresh start, but typically auth flow happens first.
        
        if (!string.IsNullOrEmpty(CurrentStudentEmail)) 
        {
            UserNameLabel.Text = CurrentStudentName;
            
            // Fetch student to get ProjectId
            var student = await _mongoDBService.GetOrCreateStudent(CurrentStudentEmail, CurrentStudentName);
            
            if (student != null && !string.IsNullOrEmpty(student.ProjectId))
            {
                LoadProjectData(student.ProjectId);
            }
            else
            {
                // No project assigned state
                ProjectTitleLabel.Text = "No Active Project";
                ProjectDescLabel.Text = "You have not been assigned to a project yet.";
                StatusLabel.Text = "Inactive";
            }
        }
    }

    private async void LoadProjectData(string projectId)
    {
        var project = await _mongoDBService.GetProject(projectId);
        if (project != null)
        {
            ProjectTitleLabel.Text = project.Title;
            ProjectDescLabel.Text = project.Description; // Or Cycle/Info if you mapped differently

            // Status Logic
            if (project.IsEvaluated)
            {
                StatusLabel.Text = "Evaluated";
                StatusLabel.TextColor = Colors.Green;
                // Update badge colors if using dynamic resources or binding
                GradeStatusLabel.Text = "Grade Available";
                ScoreLabel.Text = project.Score ?? "-";
            }
            else
            {
                StatusLabel.Text = "Pending Review";
                GradeStatusLabel.Text = "Awaiting Grade";
                ScoreLabel.Text = "⏳";
            }
        }
    }
}
