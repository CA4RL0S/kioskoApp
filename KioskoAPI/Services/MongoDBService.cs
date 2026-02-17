using MongoDB.Driver;
using KioskoAPI.Models;
using System.Security.Authentication;
using MongoDB.Bson;

namespace KioskoAPI.Services;

public class MongoDBService
{
    private string? ConnectionString;
    private string? DatabaseName;
    private string? UserCollectionName;
    private string? ProjectCollectionName;
    private const string ActivityCollectionName = "actividades";

    private IMongoCollection<User>? _usersCollection;
    private IMongoCollection<Project>? _projectsCollection;
    private IMongoCollection<Activity>? _activitiesCollection;
    private bool _isInitialized;
    private readonly IConfiguration _configuration;

    public MongoDBService(IConfiguration configuration)
    {
        _configuration = configuration;
        ConnectionString = _configuration.GetConnectionString("MongoDB");
        DatabaseName = _configuration["Settings:DatabaseName"];
        UserCollectionName = _configuration["Settings:UserCollectionName"];
        ProjectCollectionName = _configuration["Settings:ProjectCollectionName"];
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

    private async Task SeedData()
    {
        if (_projectsCollection != null)
        {
            var count = await _projectsCollection.CountDocumentsAsync(new BsonDocument());
            if (count == 0)
            {
                var projects = new List<Project>
                {
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
                await _projectsCollection.InsertManyAsync(projects);
            }
        }

        if (_usersCollection != null)
        {
            var userCount = await _usersCollection.CountDocumentsAsync(new BsonDocument());
            if (userCount == 0)
            {
                var users = new List<User>
                {
                    new User 
                    { 
                        Username = "maestro1", 
                        Email = "maestro1@test.com", 
                        FullName = "Juan Pérez", 
                        Department = "Ingeniería", 
                        Role = "Evaluador", 
                        IsVerified = true, 
                        ProfileImageUrl = "https://randomuser.me/api/portraits/men/32.jpg" 
                    },
                    new User 
                    { 
                        Username = "maestro2", 
                        Email = "maestro2@test.com", 
                        FullName = "Ana García", 
                        Department = "Ciencias", 
                        Role = "Evaluador", 
                        IsVerified = false, 
                        ProfileImageUrl = "https://randomuser.me/api/portraits/women/44.jpg" 
                    },
                    new User 
                    { 
                        Username = "maestro3", 
                        Email = "maestro3@test.com", 
                        FullName = "Carlos López", 
                        Department = "Artes", 
                        Role = "Evaluador", 
                        IsVerified = false 
                    }
                };
                await _usersCollection.InsertManyAsync(users);
            }
        }
    }

    public async Task<User?> Login(string username, string password)
    {
        await Init();
        if (_usersCollection == null) return null;
        var user = await _usersCollection.Find(u => (u.Username == username || u.Email == username) && u.Password == password).FirstOrDefaultAsync();
        
        if (user != null && !user.IsVerified)
        {
            throw new Exception("Tu cuenta aún no ha sido verificada por un administrador.");
        }

        return user;
    }

    public async Task<User?> CreateUser(User user)
    {
        await Init();
        if (_usersCollection == null) throw new Exception("Database connection failed");
        
        var existingUser = await _usersCollection.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            throw new Exception("El correo electrónico ya está registrado.");
        }

        user.IsVerified = false; 
        user.Role = "Evaluador";
        
        await _usersCollection.InsertOneAsync(user);
        return user;
    }

    public async Task<List<Project>> GetProjects()
    {
        await Init();
        if (_projectsCollection == null) return new List<Project>();
        var projects = await _projectsCollection.Find(_ => true).ToListAsync();
        return projects;
    }

    public async Task<Project?> GetProject(string id)
    {
        await Init();
        if (_projectsCollection == null) return null;
        return await _projectsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateProject(Project project)
    {
        await Init();
        if (_projectsCollection == null) return;
        await _projectsCollection.ReplaceOneAsync(p => p.Id == project.Id, project);
    }

    public async Task CreateProject(Project project)
    {
        await Init();
        if (_projectsCollection == null) return;
        await _projectsCollection.InsertOneAsync(project);
    }

    public async Task DeleteProject(string id)
    {
        await Init();
        if (_projectsCollection == null) return;
        await _projectsCollection.DeleteOneAsync(p => p.Id == id);
    }

    // --- User Management ---

    public async Task<List<User>> GetUsers()
    {
        await Init();
        if (_usersCollection == null) return new List<User>();
        return await _usersCollection.Find(_ => true).ToListAsync();
    }

    public async Task VerifyUser(string id)
    {
        await Init();
        if (_usersCollection == null) return;
        
        var user = await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return;

        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var update = Builders<User>.Update.Set(u => u.IsVerified, !user.IsVerified); // Toggle
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteUser(string id)
    {
        await Init();
        if (_usersCollection == null) return;
        await _usersCollection.DeleteOneAsync(u => u.Id == id);
    }

    public async Task UpdateUser(User user)
    {
        await Init();
        if (_usersCollection == null) return;
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
        if (_usersCollection == null) return;
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.ProfileImageUrl, imageUrl);
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task<List<Activity>> GetActivitiesByUser(string userId)
    {
        await Init();
        if (_activitiesCollection == null) return new List<Activity>();
        var sort = Builders<Activity>.Sort.Descending(a => a.Timestamp);
        return await _activitiesCollection.Find(a => a.UserId == userId).Sort(sort).Limit(20).ToListAsync();
    }

    public async Task CreateActivity(Activity activity)
    {
        await Init();
        if (_activitiesCollection == null) return;
        await _activitiesCollection.InsertOneAsync(activity);
    }
}
