using System.Diagnostics;
using System.Net.Http.Headers;

namespace StudentApp.Services;

public interface IMicrosoftGraphService
{
    Task<Stream?> GetProfilePhotoAsync(string accessToken);
}

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<Stream?> GetProfilePhotoAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photo/$value");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }

            // 404 = user has no profile photo
            Debug.WriteLine($"Graph Photo API returned: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error fetching Microsoft profile photo: {ex.Message}");
            return null;
        }
    }
}
