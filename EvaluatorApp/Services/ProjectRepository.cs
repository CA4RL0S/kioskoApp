using EvaluatorApp.Models;
using System.Text.Json;

namespace EvaluatorApp.Services;

public class ProjectRepository
{
    private readonly IMongoDBService _apiService;
    private readonly LocalDbService _localService;

    public ProjectRepository(IMongoDBService apiService, LocalDbService localService)
    {
        _apiService = apiService;
        _localService = localService;
    }

    public async Task<List<Project>> GetProjects()
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                var projects = await _apiService.GetProjects();
                if (projects != null && projects.Count > 0)
                {
                    await _localService.SaveProjects(projects);
                }
                return projects;
            }
            catch
            {
                // Fallback to local if API fails even with internet
                return await _localService.GetProjects();
            }
        }
        else
        {
            return await _localService.GetProjects();
        }
    }

    public async Task<Project> GetProject(string id)
    {
        // Always try to get from local first if we want speed, or API if we want fresh. 
        // Given the requirement, let's try API if online to ensure freshness, else local.
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                // We don't have GetProject(id) in ApiService interface explicitly? 
                // Let's check IMongoDBService. If not, we might need to rely on the list or add it.
                // Assuming we fetch list and cache, accessing local might be enough for details 
                // if we don't need real-time updates from others.
                // However, detailed view usually wants fresh data.
                // Let's use local for now as it's populated by GetProjects.
                
                // If we need fresh details:
                // var project = await _apiService.GetProject(id);
                // await _localService.UpdateProject(project);
                // return project;
                
                // For simplified offline-first, let's rely on the Cache from GetProjects most of the time,
                // or just fetch from local.
                return await _localService.GetProject(id);
            }
            catch
            {
                 return await _localService.GetProject(id);
            }
        }
        return await _localService.GetProject(id);
    }

    public async Task SubmitEvaluation(Project project, Evaluation evaluation)
    {
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {
            await _apiService.UpdateProject(project);
            await _localService.UpdateProject(project); // Update local cache
        }
        else
        {
            // Offline: Queue it
            var pending = new PendingEvaluation
            {
                ProjectId = project.Id,
                ProjectTitle = project.Title,
                JsonPayload = JsonSerializer.Serialize(evaluation), // We might need to store the whole Project or just the Eval?
                // The API expects the whole Project object for UpdateProject.
                // So we should serialize the Project.
                Timestamp = DateTime.UtcNow
            };
            
            // Re-serialize with the whole project updated locally
            // First update local DB so the UI shows it as "Evaluated" (locally)
            await _localService.UpdateProject(project);
            
            pending.JsonPayload = JsonSerializer.Serialize(project); 
            await _localService.AddPendingEvaluation(pending);
            
            throw new Exception("OfflinePersistence"); // Signal to UI that it was saved offline
        }
    }

    public async Task SyncPendingEvaluations()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

        var pendingList = await _localService.GetPendingEvaluations();
        foreach (var item in pendingList)
        {
            try
            {
                var project = JsonSerializer.Deserialize<Project>(item.JsonPayload);
                if (project != null)
                {
                    await _apiService.UpdateProject(project);
                    await _localService.DeletePendingEvaluation(item.Id);
                }
            }
            catch
            {
                // Keep in queue or mark as failed? For now keep trying.
            }
        }
    }
}
