using StudentApp.Services;

namespace StudentApp.Views;

public partial class LoadingPage : ContentPage
{
    private readonly IMsalAuthService _authService;
    private readonly IMongoDBService _mongoDBService;
    private readonly LoginPage _loginPage; // Injected to navigate to if logic fails

    public LoadingPage(IMsalAuthService authService, IMongoDBService mongoDBService, LoginPage loginPage)
    {
        InitializeComponent();
        _authService = authService;
        _mongoDBService = mongoDBService;
        _loginPage = loginPage;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await CheckLogin();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadingPage error: {ex}");
            // Fall back to login on any error
            Application.Current.MainPage = new NavigationPage(_loginPage);
        }
    }

    private async Task CheckLogin()
    {
        await Task.Delay(500);

        var result = await _authService.CheckCachedLoginAsync();

        if (result != null)
        {
            string email = result.Account.Username;
            string name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;

            try
            {
                var student = await _mongoDBService.GetOrCreateStudent(email, name);
                Preferences.Set("StudentProfileImage", student?.ProfileImageUrl ?? string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MongoDB fetch failed (non-critical): {ex.Message}");
            }

            StudentApp.MainPage.CurrentStudentEmail = email;
            StudentApp.MainPage.CurrentStudentName = name;
            Preferences.Set("StudentName", name);
            Preferences.Set("StudentEmail", email);

            Application.Current.MainPage = new AppShell();
        }
        else
        {
            Application.Current.MainPage = new NavigationPage(_loginPage);
        }
    }
}
