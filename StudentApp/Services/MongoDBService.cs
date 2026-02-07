using MongoDB.Driver;
using StudentApp.Models;
using System.Security.Authentication;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;

namespace StudentApp.Services;

public interface IMongoDBService
{
    Task Init();
    Task<EvaluatorUser> Login(string username, string password);
}

public class MongoDBService : IMongoDBService
{
    private string ConnectionString;
    private const string DatabaseName = "kioskoAppDB";
    private const string CollectionName = "student_users";

    private IMongoCollection<EvaluatorUser> _usersCollection;
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

        _usersCollection = database.GetCollection<EvaluatorUser>(CollectionName);

        _isInitialized = true;
        await SeedData();
    }

    private async Task SeedData()
    {
        var userCount = await _usersCollection.CountDocumentsAsync(new BsonDocument());
        if (userCount == 0)
        {
            var admin = new EvaluatorUser 
            { 
                Username = "alumno", 
                Password = "123", 
                FullName = "Alumno Demo", 
                Role = "Student" 
            };
            await _usersCollection.InsertOneAsync(admin);
        }
    }

    public async Task<EvaluatorUser> Login(string username, string password)
    {
        await Init();
        return await _usersCollection.Find(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();
    }
}
