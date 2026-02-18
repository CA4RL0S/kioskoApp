using MongoDB.Driver;
using EvaluatorApp.Models;
using System.Security.Authentication;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

namespace EvaluatorApp.Services;

public interface IMongoDBService
{
    Task Init();
    Task<User> Login(string username, string password);
    Task<User> CreateUser(User user);
    Task<List<Project>> GetProjects();
    Task UpdateProject(Project project);
    Task UpdateUser(User user);
    Task UpdateUserProfileImage(string userId, string imageUrl);
    Task<List<Activity>> GetActivities(string userId);
    Task CreateActivity(Activity activity);
}

public class MongoDBService : IMongoDBService
{
    private string ConnectionString;
    private const string DatabaseName = "kioskoAppDB";
    private const string UserCollectionName = "maestros";
    private const string ProjectCollectionName = "proyectos";
    private const string ActivityCollectionName = "activities";

    private IMongoCollection<User> _usersCollection;
    private IMongoCollection<Project> _projectsCollection;
    private IMongoCollection<Activity> _activitiesCollection;
    private bool _isInitialized;
    private readonly IConfiguration _configuration;

    public MongoDBService(IConfiguration configuration)
    {
        _configuration = configuration;
        ConnectionString = _configuration.GetConnectionString("MongoDB");
    }

    public async Task Init()
    {
        if (_isInitialized) return;

        var settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
        settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
        
        var client = new MongoClient(settings);
        var database = client.GetDatabase(DatabaseName);

        _usersCollection = database.GetCollection<User>(UserCollectionName);
        _projectsCollection = database.GetCollection<Project>(ProjectCollectionName);
        _activitiesCollection = database.GetCollection<Activity>(ActivityCollectionName);

        _isInitialized = true;
        await SeedData();
    }

    // ... (SeedData and other methods remain unchanged)

    public async Task<List<Activity>> GetActivities(string userId)
    {
        await Init();
        return await _activitiesCollection.Find(a => a.UserId == userId)
                                          .SortByDescending(a => a.Timestamp)
                                          .ToListAsync();
    }

    public async Task CreateActivity(Activity activity)
    {
        await Init();
        await _activitiesCollection.InsertOneAsync(activity);
    }
}

    private async Task SeedData()
    {
        // Wrapper for seeding to ensure we have data to play with
        var userCount = await _usersCollection.CountDocumentsAsync(new BsonDocument());
        if (userCount == 0)
        {
            var admin = new User { Username = "carlos", Password = "admin", Role = "Evaluador", Email = "carlos@test.com" };
            await _usersCollection.InsertOneAsync(admin);
        }

        var projectCount = await _projectsCollection.CountDocumentsAsync(new BsonDocument());
        if (projectCount == 0)
        {
            var initialProjects = new List<Project>
            {
                new Project
                {
                    Title = "Sistema de Gestión de Residuos",
                    Cycle = "Ciclo 2023-B",
                    Description = "Aplicación móvil para optimizar la recolección de basura...",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCr-YCpixRfBIMF8fitAAvhprepyTj1MiK2S0svbMfdNQxzTec1DM--qt4XgB84RK7OfwsGt_q3lHlXx7nIEYg36VtZ3oNdfp-aa8eX_Ak6gJhcTDgAjYO9ldipOOroQzK2vLz_bpgt147ZqkYcSWJxVvP6rxPQ-QqCGNgjvLCcNDtQKIclSF3qNY307dFoOxZ-YbG5ff60ffbwEwhsdNWK_zNJ2-Orf404VXISVVouns1nmJ09xTGN56WLZY3Wj-mrAYYaftz8O9c",
                    StatusText = "Pendiente de Evaluación",
                    IsPending = true,
                    IsEvaluated = false,
                    Members = new List<string> { "21310243", "21310100" }
                },
                new Project
                {
                    Title = "Plataforma E-Learning",
                    Cycle = "Ciclo 2023-A",
                    Description = "Sistema web para cursos online con seguimiento de progreso...",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBbzjAaxHIpjJDOy0mLy7cE5s--BHDNDWARtThsX_5L9img7bXF-LRyWyI1g0kco9VBnHWdAQ5b6AVzXbkHh6I7vS7F2fLm03_OkizvrB0Tw0hAj6tD99M8cmtEtXG04vA2XB00vMVje8qUyrF4hd26RPDM995ZYAdcApFKe3BNpG2NaRnLmsIXVPdEwQcazMJmTnRnvDN9p_194LvXZjJg_1JIV4FtqRvXovj4t5tM8cwkjXN54KMSYj-TM4ZJjLEL8yMR_dnhtc8",
                    StatusText = "Evaluado",
                    IsPending = false,
                    IsEvaluated = true,
                    Score = "18",
                    InnovationScore = 9,
                    TechScore = 9,
                    PresentationScore = 0, // Legacy
                    Members = new List<string> { "21310300" }
                },
                new Project
                {
                    Title = "Dashboard Financiero IA",
                    Cycle = "Ciclo 2024-A",
                    Description = "Implementación de modelos predictivos para análisis de riesgo...",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDSnAEBtq5DwuNpyqDa3DzMWqjRgcUfGQchan26XUIntTfpltZ5CEz92shE7-YTuTnqwxtGBm88NVu1dV4GlBR5bydqS153t48ha6wfbQlHQHgGlVgBrj5q4JyMGLWgyeIYGAGiTA1OSw-g_n8rwEHokqaepnLUNEHnOB_3rGimBEzEE7e0bD9q43Nb2gq870R4EiPVQPl11H3ZjRL01cc0Gme1VXPMi8T0gaaHTVn-3dO501JuwnEDBquvWNdqpOvuQloe-na8YtQ",
                    StatusText = "Pendiente de Evaluación",
                    IsPending = true,
                    IsEvaluated = false,
                    Members = new List<string> { "21310400", "21310401", "21310402" }
                },
                new Project
                {
                    Title = "Automatización de Riego",
                    Cycle = "Ciclo 2023-B",
                    Description = "Prototipo funcional de sistema de riego automatizado...",
                    ImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuD5sVqsgLhfumuqShSD7zA-718bg6dgs20umGO3cIWtE8zVG-Chzw9Yh_Q0Wq2vTqApNXLn8CQSbwVT1cZpZWZOjhFMuCRkGVH2kITbDGGu4G8NKKZBXGKOAUUamkEf8E3DiwQ0E80tZkNjb2bk9td-mKH0QgOuFae-oEDg00fHbFNyn1puDUNaexEc_DeYm2VDYAfmdOH3OxBOzE__x6ItD4sAcjWjo_vUsR7rcezBUWBwO5PbgoHKZLKfuXZbHYQjdmJtIXSlmPs",
                    StatusText = "Evaluado",
                    IsPending = false,
                    IsEvaluated = true,
                    Score = "16",
                    InnovationScore = 8,
                    TechScore = 8,
                    PresentationScore = 0,
                    Members = new List<string> { "21310500" }
                },
                new Project
                {
                    Title = "Local Space",
                    Cycle = "Ciclo 2024-A",
                    Description = "Información del proyecto Local Space",
                    ImageUrl = "https://imgs.search.brave.com/jJ-rmu1_J2ro-xUlAqDOZ2_utbwc1GhE9qr7wXQJI8E/rs:fit:860:0:0:0/g:ce/aHR0cHM6Ly9zdGF0/aWMudG9ra29icm9r/ZXIuY29tL3BpY3R1/cmVzLzc1NjM2Nzlf/NDU1ODkzMjY2NDkz/MzAxMjYxMTA4MzEy/NjA0NzMzMTk4MDM2/NzIzNjI0MTU0ODY2/MzU3NTgzMTA4NjM2/NzY5MDU1NDUzOTkz/MjYzODAuanBn",
                    StatusText = "Pendiente de Evaluación",
                    IsPending = true,
                    IsEvaluated = false,
                    Score = "0",
                    InnovationScore = 0,
                    TechScore = 0,
                    PresentationScore = 0,
                    Members = new List<string> { "Equipo Local" }
                }
            };

            await _projectsCollection.InsertManyAsync(initialProjects);
        }
    }

    public async Task<User> Login(string username, string password)
    {
        await Init();
        var user = await _usersCollection.Find(u => (u.Username == username || u.Email == username) && u.Password == password).FirstOrDefaultAsync();
        
        if (user != null && !user.IsVerified)
        {
            throw new Exception("Tu cuenta aún no ha sido verificada por un administrador.");
        }

        return user;
    }

    public async Task<User> CreateUser(User user)
    {
        await Init();
        
        var existingUser = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            throw new Exception("El correo electrónico ya está registrado.");
        }

        user.IsVerified = false; // Default to unverified
        user.Role = "Evaluador";
        
        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<List<Project>> GetProjects()
    {
        await Init();
        var projects = await _projectsCollection.Find(_ => true).ToListAsync();
        foreach (var p in projects)
        {
            p.RestoreVisuals();
        }
        return projects;
    }

    public async Task UpdateProject(Project project)
    {
        await Init();
        await _projectsCollection.ReplaceOneAsync(p => p.Id == project.Id, project);
    }

    public async Task UpdateUser(User user)
    {
        await Init();
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        
        var update = Builders<User>.Update
            .Set(u => u.FullName, user.FullName)
            .Set(u => u.Department, user.Department)
            .Set(u => u.Pronouns, user.Pronouns)
            .Set(u => u.ProfileImageUrl, user.ProfileImageUrl);
            
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task UpdateUserProfileImage(string userId, string imageUrl)
    {
        await Init();
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.ProfileImageUrl, imageUrl);
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task<List<Activity>> GetActivities(string userId)
    {
        await Init();
        return await _activitiesCollection.Find(a => a.UserId == userId)
                                          .SortByDescending(a => a.Timestamp)
                                          .ToListAsync();
    }

    public async Task CreateActivity(Activity activity)
    {
        await Init();
        await _activitiesCollection.InsertOneAsync(activity);
    }
}
