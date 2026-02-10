using Microsoft.Identity.Client;
using System.Diagnostics;

namespace EvaluatorApp.Services;

public interface IMsalAuthService
{
    Task<AuthenticationResult> SignInAsync();
    Task<AuthenticationResult> CheckCachedLoginAsync();
    Task SignOutAsync();
}

public class MsalAuthService : IMsalAuthService
{
    private IPublicClientApplication _pca;

    // Based on StudentApp configuration
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
            .WithRedirectUri("msauth.com.carlos.kiosko://auth")
            .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
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
            // Return null instead of throwing to let caller decide
            return null;
        }

        return result;
    }

    public async Task<AuthenticationResult> CheckCachedLoginAsync()
    {
        try
        {
            var accounts = await _pca.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            if (account != null)
            {
                return await _pca.AcquireTokenSilent(Scopes, account)
                                   .ExecuteAsync();
            }
        }
        catch (MsalUiRequiredException)
        {
            // Token expired or needs interaction
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Silent Auth Error: {ex.Message}");
            return null;
        }
        return null;
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
