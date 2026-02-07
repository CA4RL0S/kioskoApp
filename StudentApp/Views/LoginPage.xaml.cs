using StudentApp.Services;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace StudentApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly IMongoDBService _mongoDBService;
    private IPublicClientApplication _pca;

    // TODO: Replace with your actual Azure AD configuration
    private const string ClientId = "5272e248-fbdb-4761-888a-77c8b1f91ae6";
    private const string TenantId = "62099667-5562-4feb-a657-cf6bed8a4b72";
    private const string Authority = $"https://login.microsoftonline.com/{TenantId}";
    private readonly string[] Scopes = new string[] { "User.Read" };

    public LoginPage(IMongoDBService mongoDBService)
    {
        InitializeComponent();
        _mongoDBService = mongoDBService;
        InitializeMsal();
    }

    private void InitializeMsal()
    {
        _pca = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority(Authority)
            .WithRedirectUri($"msauth.com.companyname.studentapp://auth")
            .WithIosKeychainSecurityGroup("com.companyname.studentapp")
            .Build();
    }

    private async void OnMicrosoftLoginClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsRunning = true;
        
        try
        {
            // Attempt silent login first
            AuthenticationResult result;
            var accounts = await _pca.GetAccountsAsync();
            try
            {
                result = await _pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                                   .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                // Interactive login required
                var builder = _pca.AcquireTokenInteractive(Scopes);

#if ANDROID
                builder = builder.WithParentActivityOrWindow(Platform.CurrentActivity);
#endif
                
                result = await builder.ExecuteAsync();
            }

            if (result != null)
            {
                // Login Success
                string email = result.Account.Username;
                string name = result.ClaimsPrincipal?.FindFirst("name")?.Value ?? email;

                // Here you would typically verify the user in your MongoDB or auto-register them
                // var user = await _mongoDBService.LoginWithEmail(email); 

                await DisplayAlert("Bienvenido", $"Has iniciado sesión como: {name}\nEmail: {email}", "OK");
                
                // Navigate to main page
                // Application.Current.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            // Note: If ClientId is not configured, this will likely fail
            await DisplayAlert("Error de Autenticación", $"No se pudo iniciar sesión. Asegúrate de configurar el Client ID.\nDetalles: {ex.Message}", "OK");
            Debug.WriteLine($"MSAL Error: {ex}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }
}
