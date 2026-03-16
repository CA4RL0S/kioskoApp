using Microsoft.Identity.Client;
using System.Diagnostics;

namespace StudentApp.Services;

public interface IMsalAuthService
{
    Task<AuthenticationResult> SignInAsync();
    Task<AuthenticationResult> CheckCachedLoginAsync();
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

    private bool _initialized = false;

    // DO NOT initialize MSAL in constructor — it crashes on iOS Simulator when TeamId is null
    public MsalAuthService()
    {
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        try
        {
            var builder = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(Authority)
                .WithRedirectUri("msauth.com.carlos.kiosko.studentapp://auth");

#if IOS
            // Only use the Keychain Security Group on a Physical iOS Device where signing is present.
            // Using it on the Simulator will cause a null TeamId crash in MSAL during acquire token.
            if (DeviceInfo.DeviceType == DeviceType.Physical)
            {
                builder = builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
            }
#endif

            _pca = builder.Build();
            _initialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing MSAL: {ex.Message}");
            throw;
        }
    }

    public async Task<AuthenticationResult> SignInAsync()
    {
        AuthenticationResult result = null;
        try
        {
            EnsureInitialized();
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
            if (ex is not MsalUiRequiredException)
            {
                throw; // Rethrow to let the UI show the error
            }
            return null;
        }

        return result;
    }

    public async Task<AuthenticationResult> CheckCachedLoginAsync()
    {
        try
        {
            EnsureInitialized();
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
        try
        {
            EnsureInitialized();
            var accounts = await _pca.GetAccountsAsync();
            foreach (var account in accounts)
            {
                await _pca.RemoveAsync(account);
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"SignOut error: {ex.Message}");
        }
    }
}
