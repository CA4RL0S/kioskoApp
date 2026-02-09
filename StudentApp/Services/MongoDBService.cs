using MongoDB.Driver;
using StudentApp.Models;
using System.Security.Authentication;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

using System.Diagnostics;

namespace StudentApp.Services;

public interface IMongoDBService
{
    Task Init();
    Task<Student> GetOrCreateStudent(string email, string name);
    Task<Project> GetProject(string projectId);
    Task<List<StudentTask>> GetTasksByProject(string projectId);
}

public class MongoDBService : IMongoDBService
{
    private string ConnectionString;
    private const string DatabaseName = "kioskoAppDB";
    private const string CollectionName = "alumnos";

    private IMongoCollection<Student> _studentCollection;
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

        try
        {
            // HARDCODED FIX: Iterate through all 3 shard nodes to find the PRIMARY (Writable)
            // This is necessary because DirectConnection to a Secondary prevents writes (Login = Write)
            var shards = new[] 
            {
                "ac-ik3qpmp-shard-00-00.jmgzydq.mongodb.net:27017",
                "ac-ik3qpmp-shard-00-01.jmgzydq.mongodb.net:27017",
                "ac-ik3qpmp-shard-00-02.jmgzydq.mongodb.net:27017"
            };

            var userName = "carlosriv082_db_user";
            var pass = "CZMcLReUnyAKFwXj";
            
            IMongoDatabase foundDatabase = null;

            foreach (var shard in shards)
            {
                try
                {
                    var directConnString = $"mongodb://{userName}:{pass}@{shard}/?ssl=true&authSource=admin&directConnection=true";
                    
                    var settings = MongoClientSettings.FromUrl(new MongoUrl(directConnString));
                    settings.SslSettings = new SslSettings { 
                        EnabledSslProtocols = SslProtocols.Tls12,
                        ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true 
                    };
                    settings.ConnectTimeout = TimeSpan.FromSeconds(5); // Fast fail
                    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

                    var client = new MongoClient(settings);
                    var database = client.GetDatabase(DatabaseName);

                    // Check if this node is PRIMARY (writable)
                    var isMasterCommand = new BsonDocument("isMaster", 1);
                    var result = await database.RunCommandAsync((MongoDB.Driver.Command<BsonDocument>)isMasterCommand);

                    if (result.Contains("ismaster") && result["ismaster"].AsBoolean)
                    {
                        // Found the Primary!
                        System.Diagnostics.Debug.WriteLine($" Connected to PRIMARY: {shard}");
                        foundDatabase = database;
                        break;
                    }
                }
                catch (Exception attemptEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to connect to {shard}: {attemptEx.Message}");
                    continue;
                }
            }

            if (foundDatabase == null)
            {
                throw new Exception("Could not find a writable Primary node in the cluster.");
            }

            _studentCollection = foundDatabase.GetCollection<Student>(CollectionName);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MongoDB Init Error: {ex.Message}");
            // We can't display alert here as it's a service, but the caller (LoginPage) will catch it.
            throw new Exception("Error connectivity to database. Check internet.", ex);
        }
    }

    public async Task<Student> GetOrCreateStudent(string email, string name)
    {
        await Init();
        
        var existingStudent = await _studentCollection.Find(s => s.Email == email).FirstOrDefaultAsync();
        if (existingStudent != null)
        {
            return existingStudent;
        }

        // Create new student
        // Extract matricula from email (assuming standard format like 21310243@...)
        string matricula = email.Split('@')[0];

        var newStudent = new Student
        {
            Email = email,
            Name = name,
            Matricula = matricula,
            Role = "Student"
        };

        await _studentCollection.InsertOneAsync(newStudent);
        return newStudent;
    }

    public async Task<Project> GetProject(string projectId)
    {
        await Init();
        // Since we don't have a direct reference to the project collection in this file yet,
        // we need to add it.
        var database = new MongoClient(ConnectionString).GetDatabase(DatabaseName);
        var projectCollection = database.GetCollection<Project>("proyectos");
        
        return await projectCollection.Find(p => p.Id == projectId).FirstOrDefaultAsync();
    }

    public async Task<List<StudentTask>> GetTasksByProject(string projectId)
    {
        await Init();
        var database = new MongoClient(ConnectionString).GetDatabase(DatabaseName);
        var tasksCollection = database.GetCollection<StudentTask>("tasks");

        return await tasksCollection.Find(t => t.ProjectId == projectId).ToListAsync();
    }
}
