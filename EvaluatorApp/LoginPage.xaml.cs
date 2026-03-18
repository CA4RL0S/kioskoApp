using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private bool _isPasswordVisible = false;

	public LoginPage(IMongoDBService mongoDBService)
	{
		InitializeComponent();
        _mongoDBService = mongoDBService;

        // Remove native iOS border from Entry controls
        RemoveEntryBorders();
	}

    private void RemoveEntryBorders()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
        {
#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif ANDROID
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
        });
    }

    private void OnTogglePassword(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;

        // Update icon
        TogglePasswordBtn.Source = new FontImageSource
        {
            FontFamily = "MaterialIcons",
            Glyph = _isPasswordVisible ? "visibility" : "visibility_off",
            Color = Color.FromArgb("#94A3B8"),
            Size = 20
        };
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim().ToLower();
        string password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Por favor ingresa usuario y contraseña", "OK");
            return;
        }

        // Show loading state
        LoginBtn.IsVisible = false;
        LoadingOverlay.IsVisible = true;

        try
        {
             var user = await _mongoDBService.Login(email, password);
             if (user != null)
             {
                 Preferences.Set("UserFullName", user.FullName ?? user.Username);
                 Preferences.Set("UserEmail", user.Email);
                 Preferences.Set("UserRole", user.Role);
                 Preferences.Set("UserId", user.Id);
                 Preferences.Set("UserProfileImage", user.ProfileImageUrl ?? string.Empty);
                 Preferences.Set("UserDepartment", user.Department ?? string.Empty);
                 Preferences.Set("UserPronouns", user.Pronouns ?? string.Empty);

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
            LoadingOverlay.IsVisible = false;
            LoginBtn.IsVisible = true;
        }
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SignUpPage));
    }
}
