using EvaluatorApp.Services;

namespace EvaluatorApp;

public partial class ProfilePage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private readonly IMsalAuthService _msalAuthService;

    public ProfilePage(IMongoDBService mongoDBService, IMsalAuthService msalAuthService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        _msalAuthService = msalAuthService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, false);
        LoadUserProfile();
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
