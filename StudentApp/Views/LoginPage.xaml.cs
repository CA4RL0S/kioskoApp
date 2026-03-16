using StudentApp.Services;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace StudentApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private readonly IMsalAuthService _authService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMicrosoftGraphService _graphService;

    private const string DefaultProfileImage = "https://res.cloudinary.com/djwpi6z29/image/upload/v1770699551/avatar-default-user-profile-icon-social-media-vector-57234208_y8gtgs.jpg";

    public LoginPage(IMongoDBService mongoDBService, IMsalAuthService authService, 
                     ICloudinaryService cloudinaryService, IMicrosoftGraphService graphService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        _authService = authService;
        _cloudinaryService = cloudinaryService;
        _graphService = graphService;
    }

    private async void OnMicrosoftLoginClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsRunning = true;
        
        try
        {
            var result = await _authService.SignInAsync();

            if (result != null)
            {
                string email = result.Account.Username;
                string name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;

                // Auto-register or get existing student
                var student = await _mongoDBService.GetOrCreateStudent(email, name);

                // Fetch and upload Microsoft profile photo if student doesn't have one yet
                string profileImageUrl = student.ProfileImageUrl ?? DefaultProfileImage;
                if (string.IsNullOrEmpty(student.ProfileImageUrl))
                {
                    profileImageUrl = await FetchAndUploadProfilePhoto(result.AccessToken, student.Id);
                }

                // Store session data
                StudentApp.MainPage.CurrentStudentEmail = email;
                StudentApp.MainPage.CurrentStudentName = name;
                Preferences.Set("StudentProfileImage", profileImageUrl);
                Preferences.Set("StudentName", name);
                Preferences.Set("StudentEmail", email);

                await DisplayAlert("Bienvenido", $"Has iniciado sesión como: {student.Name}\nMatrícula: {student.Matricula}", "OK");

                // Navigate to AppShell (which contains the TabBar)
                Application.Current.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            string errorTitle = "Error de Inicio de Sesión";
            string errorMessage = $"No se pudo completar el inicio de sesión.\n\nDetalle Técnico: {detail}";

            if (ex.Message.Contains("database", StringComparison.OrdinalIgnoreCase) || detail.Contains("database", StringComparison.OrdinalIgnoreCase) || 
                ex.Message.Contains("MongoDB", StringComparison.OrdinalIgnoreCase) || detail.Contains("MongoDB", StringComparison.OrdinalIgnoreCase))
            {
                errorTitle = "Error de Conexión";
                errorMessage = $"No se pudo conectar a la base de datos.\n\nVerifique su conexión a Internet o Firewall.\n\nDetalle Técnico: {detail}";
            }

            await DisplayAlert(errorTitle, errorMessage, "OK");
            Debug.WriteLine($"Login Error: {ex}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task<string> FetchAndUploadProfilePhoto(string accessToken, string studentId)
    {
        try
        {
            var photoStream = await _graphService.GetProfilePhotoAsync(accessToken);
            
            if (photoStream != null)
            {
                // Upload to Cloudinary
                var imageUrl = await _cloudinaryService.UploadImage(photoStream, $"student_{studentId}.jpg");
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // Save URL to MongoDB
                    await _mongoDBService.UpdateStudentProfileImage(studentId, imageUrl);
                    return imageUrl;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Profile photo upload failed (non-critical): {ex.Message}");
        }

        return DefaultProfileImage;
    }
}
