using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class ProfilePage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private readonly IMsalAuthService _msalAuthService;
    private readonly ICloudinaryService _cloudinaryService;

    public ProfilePage(IMongoDBService mongoDBService, IMsalAuthService msalAuthService, ICloudinaryService cloudinaryService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        _msalAuthService = msalAuthService;
        _cloudinaryService = cloudinaryService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);
        LoadUserProfile();

        // Set switch state without firing Toggled event
        DarkModeSwitch.Toggled -= OnDarkModeToggled;
        DarkModeSwitch.IsToggled = ThemeService.IsDarkMode();
        DarkModeSwitch.Toggled += OnDarkModeToggled;
    }

    private void LoadUserProfile()
    {
        string fullName = Preferences.Get("UserFullName", "Usuario");
        string email = Preferences.Get("UserEmail", "usuario@test.com");
        string role = Preferences.Get("UserRole", "Evaluador");
        string profileImage = Preferences.Get("UserProfileImage", string.Empty);
        
        NameLabel.Text = fullName;
        EmailLabel.Text = email;
        RoleLabel.Text = role;
        
        if (!string.IsNullOrEmpty(profileImage))
        {
            ProfileImage.Source = profileImage;
        }
    }

    private async void OnProfileImageClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                // Show loading state (optional, could add an activity indicator)
                
                using var stream = await photo.OpenReadAsync();
                var imageUrl = await _cloudinaryService.UploadImage(stream, photo.FileName);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // Update UI
                    ProfileImage.Source = imageUrl;

                    // Update Local Preferences
                    Preferences.Set("UserProfileImage", imageUrl);

                    // Update Database
                    var userId = Preferences.Get("UserId", string.Empty);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _mongoDBService.UpdateUserProfileImage(userId, imageUrl);
                    }
                    
                    await DisplayAlert("Éxito", "Foto de perfil actualizada correctamente.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo actualizar la foto: {ex.Message}", "OK");
        }
    }

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        // Navigate to Edit Profile Page
        var editPage = Handler.MauiContext.Services.GetService<EditProfilePage>();
        await Navigation.PushAsync(editPage);
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Seguridad", "Navegar a Cambiar Contraseña", "OK");
    }

    private void OnDarkModeToggled(object sender, ToggledEventArgs e)
    {
        ThemeService.ApplyTheme(e.Value);
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Ayuda", "Navegar a Soporte", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");
        if (answer)
        {
            await _msalAuthService.SignOutAsync();
            Preferences.Clear(); // Clear session
            // Reset main page to logic to create a new session or go to login
             await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
