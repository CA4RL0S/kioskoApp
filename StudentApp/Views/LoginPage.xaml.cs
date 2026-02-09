using StudentApp.Services;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace StudentApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private readonly IMsalAuthService _authService;

    public LoginPage(IMongoDBService mongoDBService, IMsalAuthService authService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        _authService = authService;
    }

    // Constructor for when only MongoDBService is passed (backwards compatibility/navigation)
    // Though ideally we should use DI for everything.
    public LoginPage(IMongoDBService mongoDBService) : this(mongoDBService, new MsalAuthService()) 
    {
        // Fallback if instantiated manually without auth service
    }

    private async void OnMicrosoftLoginClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsRunning = true;
        
        try
        {
            var result = await _authService.SignInAsync();

            if (result != null)
            {
                // Login Success
                string email = result.Account.Username;
                string name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;

                // Auto-register or get existing student
                var student = await _mongoDBService.GetOrCreateStudent(email, name);

                await DisplayAlert("Bienvenido", $"Has iniciado sesión como: {student.Name}\nMatrícula: {student.Matricula}", "OK");
                
                // Store session data (using MainPage statics for now as established in previous step)
                StudentApp.MainPage.CurrentStudentEmail = email;
                StudentApp.MainPage.CurrentStudentName = name;

                // Navigate to AppShell (which contains the TabBar)
                Application.Current.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de Autenticación", $"No se pudo iniciar sesión.\nDetalles: {ex.Message}", "OK");
            Debug.WriteLine($"MSAL Error: {ex}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }
}
