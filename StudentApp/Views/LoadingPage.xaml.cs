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
        await CheckLogin();
    }

    private async Task CheckLogin()
    {
        await Task.Delay(500); // Slight delay for UI to stabilize/show loading

        var result = await _authService.CheckCachedLoginAsync();

        if (result != null)
        {
            // Valid session found
            string email = result.Account.Username;
            string name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;

            // Fetch student data to ensure we have context (ProjectId etc.)
             var student = await _mongoDBService.GetOrCreateStudent(email, name); // Or a GetStudentByEmail if strictly checking

            // Set session
            StudentApp.MainPage.CurrentStudentEmail = email;
            StudentApp.MainPage.CurrentStudentName = name;

            // Go to AppShell
             Application.Current.MainPage = new AppShell();
        }
        else
        {
            // No valid session, go to Login
            Application.Current.MainPage = new NavigationPage(_loginPage);
        }
    }
}
