namespace StudentApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly Services.IMongoDBService _mongoDBService;

    public ProfilePage(Services.IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Load profile image
            string profileImage = Preferences.Get("StudentProfileImage", string.Empty);
            if (!string.IsNullOrEmpty(profileImage))
                ProfileImage.Source = profileImage;

            // Load name and email from session
            if (!string.IsNullOrEmpty(StudentApp.MainPage.CurrentStudentName))
                NameLabel.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(StudentApp.MainPage.CurrentStudentName.ToLower());
                
            if (!string.IsNullOrEmpty(StudentApp.MainPage.CurrentStudentEmail))
                EmailLabel.Text = StudentApp.MainPage.CurrentStudentEmail;

            // Fetch student for matrícula
            if (!string.IsNullOrEmpty(StudentApp.MainPage.CurrentStudentEmail))
            {
                var student = await _mongoDBService.GetOrCreateStudent(
                    StudentApp.MainPage.CurrentStudentEmail, 
                    StudentApp.MainPage.CurrentStudentName);
                
                if (student != null)
                {
                    MatriculaLabel.Text = $"Matrícula: {student.Matricula}";
                    MatriculaDetailLabel.Text = student.Matricula;
                }
            }

            // Set switch state without firing Toggled event
            DarkModeSwitch.Toggled -= OnDarkModeToggled;
            DarkModeSwitch.IsToggled = Services.ThemeService.IsDarkMode();
            DarkModeSwitch.Toggled += OnDarkModeToggled;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProfilePage error: {ex}");
        }
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        Services.ThemeService.ApplyTheme(e.Value);
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que deseas cerrar sesión?", "Sí", "Cancelar");
        if (!confirm) return;

        try
        {
            var authService = this.Handler?.MauiContext?.Services.GetService<Services.IMsalAuthService>();
            
            if (authService != null)
            {
                await authService.SignOutAsync();
            }

            // Clear preferences
            Preferences.Remove("StudentProfileImage");
            Preferences.Remove("StudentName");
            Preferences.Remove("StudentEmail");

            // Navigate back to login using DI
            var mongoService = this.Handler?.MauiContext?.Services.GetService<Services.IMongoDBService>();
            var cloudinaryService = this.Handler?.MauiContext?.Services.GetService<Services.ICloudinaryService>();
            var graphService = this.Handler?.MauiContext?.Services.GetService<Services.IMicrosoftGraphService>();

            if (mongoService != null && authService != null && cloudinaryService != null && graphService != null)
            {
                 Application.Current.MainPage = new NavigationPage(new LoginPage(mongoService, authService, cloudinaryService, graphService)); 
            }
            else
            {
                 await DisplayAlert("Error", "No se pudieron resolver los servicios.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cerrar sesión: {ex.Message}", "OK");
        }
    }
}
