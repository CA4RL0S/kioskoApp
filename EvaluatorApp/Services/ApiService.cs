using System.Net.Http.Json;
using System.Text.Json;
using EvaluatorApp.Models;
using Microsoft.Maui.Devices;

namespace EvaluatorApp.Services;

public class ApiService : IMongoDBService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60); // Increased timeout for cold starts/slow network
        
        // Production API URL
        _baseUrl = "https://kioskoapp.onrender.com";

        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public Task Init()
    {
        // No-op for API
        return Task.CompletedTask;
    }

    public async Task<User> Login(string username, string password)
    {
        var loginRequest = new { Username = username, Password = password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await response.Content.ReadFromJsonAsync<User>(options);
        }
        
        throw new Exception("Login failed: " + response.ReasonPhrase);
    }

    public async Task<User> CreateUser(User user)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", user);
        
        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await response.Content.ReadFromJsonAsync<User>(options);
        }

        throw new Exception("Registration failed");
    }

    public async Task<List<Project>> GetProjects()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var projects = await _httpClient.GetFromJsonAsync<List<Project>>("/api/projects", options);
        
        if (projects != null)
        {
            // Restore visual state if needed, though models might not have correct data if API didn't return it
            // Project.cs in App has RestoreVisuals logic.
            foreach (var p in projects)
            {
                p.RestoreVisuals();
            }
        }

        return projects ?? new List<Project>();
    }

    public async Task UpdateProject(Project project)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/projects/{project.Id}", project);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error {response.StatusCode}: {errorContent}");
        }
    }

    public async Task UpdateUser(User user)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/users/{user.Id}", user);
        if (!response.IsSuccessStatusCode)
        {
             var errorContent = await response.Content.ReadAsStringAsync();
             throw new Exception($"Error {response.StatusCode}: {errorContent}");
        }
    }

    public async Task UpdateUserProfileImage(string userId, string imageUrl)
    {
        // Sending just string as body
        var response = await _httpClient.PatchAsJsonAsync($"/api/users/{userId}/image", imageUrl);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Activity>> GetActivities(string userId)
    {
        try 
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var activities = await _httpClient.GetFromJsonAsync<List<Activity>>($"/api/activities/{userId}", options);
            return activities ?? new List<Activity>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] Error getting activities: {ex.Message}");
            return new List<Activity>();
        }
    }

    public async Task CreateActivity(Activity activity)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/activities", activity);
        response.EnsureSuccessStatusCode();
    }
}
