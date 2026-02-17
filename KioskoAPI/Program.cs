using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using KioskoAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Upload Limits (100 MB for Cloudinary Free Tier)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 105 * 1024 * 1024; // 105 MB (buffer)
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 105 * 1024 * 1024; // 105 MB
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

// Register MongoDB Service
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddScoped<CloudinaryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Enable CORS for mobile app access (dev)
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.Run();
