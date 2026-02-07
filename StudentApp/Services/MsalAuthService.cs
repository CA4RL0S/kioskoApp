using Microsoft.Identity.Client;
using System.Diagnostics;

namespace StudentApp.Services;

public interface IMsalAuthService
{
    Task<AuthenticationResult> SignInAsync();
    Task SignOutAsync();
}

public class MsalAuthService : IMsalAuthService
{
    private IPublicClientApplication _pca;

    // TODO: Replace with your actual Azure AD configuration (moved from LoginPage)
    private const string ClientId = "5272e248-fbdb-4761-888a-77c8b1f91ae6";
    private const string TenantId = "62099667-5562-4feb-a657-cf6bed8a4b72";
    private const string Authority = $"https://login.microsoftonline.com/{TenantId}";
    private readonly string[] Scopes = new string[] { "User.Read" };

    public MsalAuthService()
    {
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

    public async Task<AuthenticationResult> SignInAsync()
    {
        AuthenticationResult result = null;
        try
        {
            // Attempt silent login first
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MSAL Error: {ex}");
            throw; // Re-throw to let UI handle it or log it
        }

        return result;
    }

    public async Task SignOutAsync()
    {
        var accounts = await _pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _pca.RemoveAsync(account);
        }
    }
}
