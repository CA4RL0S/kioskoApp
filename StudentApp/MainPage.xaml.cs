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
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage LoadData error: {ex}");
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        try
        {
            await LoadData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage refresh error: {ex}");
        }
        finally
        {
            MainRefreshView.IsRefreshing = false;
        }
    }

    private async Task LoadData()
    {
        if (string.IsNullOrEmpty(CurrentStudentEmail)) return;

        if (string.IsNullOrEmpty(UserNameLabel.Text))
        {
            UserNameLabel.Text = GetShortName(CurrentStudentName);
            string profileImage = Preferences.Get("StudentProfileImage", string.Empty);
            if (!string.IsNullOrEmpty(profileImage))
                ProfileImage.Source = profileImage;
        }
        
        var student = await _mongoDBService.GetOrCreateStudent(CurrentStudentEmail, CurrentStudentName);
        if (student == null) return;

        var projects = await _mongoDBService.GetProjectsByMatricula(student.Matricula);

        if (projects == null || projects.Count == 0)
        {
            BindableLayout.SetItemsSource(ProjectListView, null);
            EmptyState.IsVisible = true;
            return;
        }

        foreach(var p in projects)
        {
            p.RestoreVisuals();
        }

        EmptyState.IsVisible = false;
        BindableLayout.SetItemsSource(ProjectListView, projects);
    }

    private async void OnProjectTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Project selectedProject)
        {
            await Shell.Current.GoToAsync(nameof(ProjectDetailsPage), new Dictionary<string, object>
            {
                { "Project", selectedProject }
            });
        }
    }

    /// <summary>
    /// Takes a full name like "CARLOS MANUEL RIVAS ORDONEZ" and returns "Carlos Rivas"
    /// (first name + first surname, title-cased).
    /// </summary>
    private static string GetShortName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "";
        
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "";
        
        string ToTitleCase(string s) => 
            char.ToUpper(s[0]) + s[1..].ToLower();
        
        if (parts.Length <= 2)
            return string.Join(" ", parts.Select(ToTitleCase));
        
        // For "CARLOS MANUEL RIVAS ORDONEZ" → first name is parts[0], first surname is parts[^2]
        // Typical Mexican name: [Nombre] [Segundo nombre] [Apellido paterno] [Apellido materno]
        // We want: Nombre + Apellido paterno
        string firstName = ToTitleCase(parts[0]);
        string firstSurname = ToTitleCase(parts[^2]); // second to last = apellido paterno
        
        return $"{firstName} {firstSurname}";
    }
}
