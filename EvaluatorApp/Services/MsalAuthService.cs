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
    private bool _initialized = false;

    private const string ClientId = "5272e248-fbdb-4761-888a-77c8b1f91ae6";
    private const string TenantId = "62099667-5562-4feb-a657-cf6bed8a4b72";
    private const string Authority = $"https://login.microsoftonline.com/{TenantId}";
    private readonly string[] Scopes = new string[] { "User.Read" };

    // DO NOT initialize MSAL in constructor — it crashes on iOS when TeamId is null
    public MsalAuthService() { }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            _pca = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("msauth.com.carlos.kiosko://auth")
                .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                .Build();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MSAL Keychain init failed, using fallback: {ex.Message}");
            _pca = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("msauth.com.carlos.kiosko://auth")
                .Build();
        }
    }

    public async Task<AuthenticationResult> SignInAsync()
    {
        EnsureInitialized();
        AuthenticationResult result = null;
        try
        {
            var accounts = await _pca.GetAccountsAsync();
            try
            {
                result = await _pca.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                                   .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
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
            return null;
        }

        return result;
    }

    public async Task<AuthenticationResult> CheckCachedLoginAsync()
    {
        EnsureInitialized();
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
        EnsureInitialized();
        var accounts = await _pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await _pca.RemoveAsync(account);
        }
    }
}
