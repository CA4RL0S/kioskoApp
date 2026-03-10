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
        
        if (!string.IsNullOrEmpty(CurrentStudentEmail)) 
        {
            UserNameLabel.Text = CurrentStudentName;

            // Load profile image
            string profileImage = Preferences.Get("StudentProfileImage", string.Empty);
            if (!string.IsNullOrEmpty(profileImage))
                ProfileImage.Source = profileImage;
            
            // Fetch student to get ProjectId
            var student = await _mongoDBService.GetOrCreateStudent(CurrentStudentEmail, CurrentStudentName);
            
            if (student != null && !string.IsNullOrEmpty(student.ProjectId))
            {
                await LoadProjectData(student.ProjectId);
            }
            else
            {
                // No project assigned state
                ProjectTitleLabel.Text = "Sin Proyecto Activo";
                ProjectDescLabel.Text = "Aún no se te ha asignado un proyecto.";
                StatusLabel.Text = "Sin asignar";
                GradeSection.IsVisible = false;
                ProjectDetailsRow.IsVisible = false;
            }
        }
    }

    private async Task LoadProjectData(string projectId)
    {
        var project = await _mongoDBService.GetProject(projectId);
        if (project != null)
        {
            ProjectTitleLabel.Text = project.Title ?? "Proyecto";
            ProjectDescLabel.Text = project.Description ?? "";
            
            // Show cycle info if available
            if (!string.IsNullOrEmpty(project.Cycle))
            {
                ProjectDetailsRow.IsVisible = true;
                ProjectCycleLabel.Text = $"Ciclo: {project.Cycle}";
            }

            // Grade section is always visible when there's a project
            GradeSection.IsVisible = true;

            if (project.IsEvaluated)
            {
                // Project has been evaluated — show the grade
                StatusLabel.Text = "Evaluado";
                StatusBadge.BackgroundColor = Color.FromArgb("#dcfce7");
                StatusBadge.Stroke = Color.FromArgb("#22c55e");
                StatusLabel.TextColor = Color.FromArgb("#15803d");

                GradeStatusLabel.Text = "Calificación Final";
                ScoreLabel.Text = project.Score ?? "-";
                ScoreLabel.TextColor = Color.FromArgb("#197fe6");
                ScoreLabel.FontSize = 28;

                // Show breakdown
                ScoreBreakdown.IsVisible = true;
                InnovationScoreLabel.Text = project.InnovationScore.ToString("0.#");
                TechScoreLabel.Text = project.TechScore.ToString("0.#");
                PresentationScoreLabel.Text = project.PresentationScore.ToString();
            }
            else if (project.IsPending)
            {
                StatusLabel.Text = "En revisión";
                GradeStatusLabel.Text = "Esperando calificación";
                ScoreLabel.Text = "⏳";
            }
            else
            {
                StatusLabel.Text = "Activo";
                StatusBadge.BackgroundColor = Color.FromArgb("#dbeafe");
                StatusBadge.Stroke = Color.FromArgb("#197fe6");
                StatusLabel.TextColor = Color.FromArgb("#1e40af");
                GradeStatusLabel.Text = "Pendiente de evaluación";
                ScoreLabel.Text = "—";
            }
        }
    }
}
