using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;

	public LoginPage(IMongoDBService mongoDBService)
	{
		InitializeComponent();
        _mongoDBService = mongoDBService;
	}

    // Default constructor for cases where DI might fail or previewer (optional)
    // But normally Maui DI handles it if registered.
    // However, if App.xaml.cs creates MainPage directly before DI, we need to be careful.
    // AppShell usually sets pages. 
    // Let's modify App.xaml.cs if needed, but usually defining it in MauiProgram is enough if Shell uses routes?
    // Wait, Shell ContentTemplate creates instances. If checking AppShell.xaml, <ShellContent ContentTemplate="{DataTemplate local:LoginPage}" />
    // This supports DI in .NET MAUI.
    
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor ingresa usuario y contraseña", "OK");
            return;
        }

        IsBusy = true; // Should ideally bind a loading indicator
        try
        {
             var user = await _mongoDBService.Login(email, password);
             if (user != null)
             {
                 await Shell.Current.GoToAsync("//ProjectsPage");
             }
             else
             {
                 await DisplayAlert("Error", "Credenciales incorrectas", "Intentar de nuevo");
                 PasswordEntry.Text = string.Empty;
             }
        }
        catch (Exception ex)
        {
             await DisplayAlert("Error", $"Error de conexión: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
