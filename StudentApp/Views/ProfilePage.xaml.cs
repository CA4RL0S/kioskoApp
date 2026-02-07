using Microsoft.Identity.Client;

namespace StudentApp.Views;

public partial class ProfilePage : ContentPage
{
    private string _userName;
    private string _userEmail;

    public ProfilePage(string userName, string userEmail)
    {
        InitializeComponent();
        _userName = userName;
        _userEmail = userEmail;

        NameLabel.Text = _userName;
        EmailLabel.Text = _userEmail;
    }
    
    // Default constructor for preview/navigation without data
    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sign Out", "Are you sure you want to sign out?", "Yes", "Cancel");
        if (!confirm) return;

        try
        {
            // Here we should ideally use the IPublicClientApplication from the service provider or pass it along.
            // For checking purposes, we might need to reconstruct it or expose it from App/MauiProgram if singleton.
            // A pattern common in simple MAUI apps is to clear the cache or just navigate back 
            // if we assume we re-build the PCA on Login page anyway.
            
            // However, to do a proper MSAL signout (and clear cookie flow if desirable), 
            // we would call RemoveAsync on the account.
            
            // For this iteration, we will just navigate back to the Login Page 
            // effectively "logging out" of the UI session.
            
            // Resolve Services from the service provider
            var mongoService = this.Handler?.MauiContext?.Services.GetService<Services.IMongoDBService>();
            var authService = this.Handler?.MauiContext?.Services.GetService<Services.IMsalAuthService>();
            
            if (authService != null)
            {
                await authService.SignOutAsync();
            }

            if (mongoService != null && authService != null)
            {
                 Application.Current.MainPage = new NavigationPage(new LoginPage(mongoService, authService)); 
            }
            else
            {
                 // Fallback or error handling
                 await DisplayAlert("Error", "Could not resolve services.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not sign out: {ex.Message}", "OK");
        }
    }
}
