using StudentApp.Services;

namespace StudentApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;

    public LoginPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Por favor ingresa usuario y contraseña", "OK");
            return;
        }

        LoadingIndicator.IsRunning = true;
        UsernameEntry.IsEnabled = false;
        PasswordEntry.IsEnabled = false;

        try
        {
            var user = await _mongoDBService.Login(UsernameEntry.Text, PasswordEntry.Text);
            
            if (user != null)
            {
                // Navigate to main page (To be implemented)
                await DisplayAlert("Éxito", $"Bienvenido {user.FullName}", "OK");
                // Application.Current.MainPage = new AppShell(); // Uncomment when ready
            }
            else
            {
                await DisplayAlert("Error", "Credenciales inválidas", "OK");
            }
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            UsernameEntry.IsEnabled = true;
            PasswordEntry.IsEnabled = true;
        }
    }
}
