using SQLite;
using EvaluatorApp.Models;
using System.Text.Json;

namespace EvaluatorApp.Services;

public class LocalDbService
{
    private const string DB_NAME = "kiosko_offline.db3";
    private readonly SQLiteAsyncConnection _connection;

    public LocalDbService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
        _connection = new SQLiteAsyncConnection(dbPath);
        
        // Initialize Tables
        _connection.CreateTableAsync<Project>().Wait();
        _connection.CreateTableAsync<PendingEvaluation>().Wait();
    }

    // --- Projects ---
    public async Task<List<Project>> GetProjects()
    {
        var projects = await _connection.Table<Project>().ToListAsync();
        
        // Deserialize complex fields since SQLite doesn't store lists
        foreach (var p in projects)
        {
            if (!string.IsNullOrEmpty(p.EvaluationsJson))
            {
                try { p.Evaluations = JsonSerializer.Deserialize<List<Evaluation>>(p.EvaluationsJson) ?? new List<Evaluation>(); }
                catch { p.Evaluations = new List<Evaluation>(); }
            }
            if (!string.IsNullOrEmpty(p.MembersJson))
            {
                try { p.Members = JsonSerializer.Deserialize<List<string>>(p.MembersJson) ?? new List<string>(); }
                catch { p.Members = new List<string>(); }
            }
            
            p.RestoreVisuals();
        }
        
        return projects;
    }

    public async Task<Project> GetProject(string id)
    {
        return await _connection.Table<Project>().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task SaveProjects(List<Project> projects)
    {
        // Serialize complex fields before saving
        foreach (var p in projects)
        {
            p.EvaluationsJson = JsonSerializer.Serialize(p.Evaluations);
            p.MembersJson = JsonSerializer.Serialize(p.Members);
        }
        
        await _connection.InsertAllAsync(projects, "OR REPLACE");
    }

    public async Task UpdateProject(Project p)
    {
         p.EvaluationsJson = JsonSerializer.Serialize(p.Evaluations);
         p.MembersJson = JsonSerializer.Serialize(p.Members);
         await _connection.UpdateAsync(p);
    }

    // --- Pending Evaluations ---
    public async Task<List<PendingEvaluation>> GetPendingEvaluations()
    {
        return await _connection.Table<PendingEvaluation>().ToListAsync();
    }

    public async Task AddPendingEvaluation(PendingEvaluation evaluation)
    {
        await _connection.InsertAsync(evaluation);
    }

    public async Task DeletePendingEvaluation(int id)
    {
        await _connection.DeleteAsync<PendingEvaluation>(id);
    }
}
