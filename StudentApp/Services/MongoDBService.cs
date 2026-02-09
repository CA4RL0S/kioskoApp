using MongoDB.Driver;
using StudentApp.Models;
using System.Security.Authentication;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

namespace StudentApp.Services;

public interface IMongoDBService
{
    Task Init();
    Task<Student> GetOrCreateStudent(string email, string name);
    Task<Project> GetProject(string projectId);
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

        var settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
        settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
        
        var client = new MongoClient(settings);
        var database = client.GetDatabase(DatabaseName);

        _studentCollection = database.GetCollection<Student>(CollectionName);

        _isInitialized = true;
        // SeedData no longer needed for auth as we auto-create on login
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
}
